using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public static class OtherworldCalling {
  public const string JobName="EH_OtherworldCalling";
  public const int Price=1000,Duration=180,Cooldown=2500,PendingLimit=8;
  public static int CooldownTicks{get{return ElementalHeartsMod.Settings==null?Cooldown:(int)(Mathf.Clamp(ElementalHeartsMod.Settings.callingCooldownHours,0,24)*2500);}}
  sealed class SilverPreview {public SilverPreview(){}public Map map;public float refreshed=-1;public int amount;}
  static readonly ConditionalWeakTable<Pawn,SilverPreview> silverPreviews=new ConditionalWeakTable<Pawn,SilverPreview>();
  static int PreviewSilver(Pawn p){var cached=silverPreviews.GetOrCreateValue(p);float now=Time.realtimeSinceStartup;if(cached.map!=p.Map||cached.refreshed<0||now<cached.refreshed||now-cached.refreshed>=.3f){cached.map=p.Map;cached.amount=Silver(p,null);cached.refreshed=now;}return cached.amount;}
  public static void UpgradeCooldown(Pawn p){var c=HediffComp_CelestialAbilities.On(p);if(c==null||c.callingCooldownVersion>=1)return;int now=Find.TickManager.TicksGame;if(c.callingReadyAt>now){int legacy=ElementalHeartsMod.Settings==null?60000:(int)(Mathf.Clamp(ElementalHeartsMod.Settings.callingCooldownDays,0,5)*60000);c.callingReadyAt=legacy>Cooldown?Math.Max(0,c.callingReadyAt-legacy+Cooldown):Math.Min(c.callingReadyAt,now+Cooldown);}c.callingCooldownVersion=1;}
  static Texture2D icon;
  public static GameComponent_OtherworldCalling State {get{return Current.Game==null?null:Current.Game.GetComponent<GameComponent_OtherworldCalling>();}}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==JobName;}
  public static string Problem(Pawn p,bool money=true){return Problem(p,money,false);}
  public static string Problem(Pawn p,bool money,bool preview){
   if(p==null||!CelestialAscendant.Active(p)||!ElementalTeaching.Ready(p)||!ElementalCombatPolicy.Controlled(p))return "Requires a conscious player-controlled Celestial on a map.";
   UpgradeCooldown(p);
   if(CelestialAscendant.Busy(p)||ElementalAnimationBridge.Controlled(p)||ElementalBestow.Busy(p)||ElementalTeachingSession.Busy(p)||HediffComp_ElementalTransform.Transforming(p)||AzureEvolution.Evolving(p))return "Finish the current action before calling.";
   if(p.carryTracker!=null&&p.carryTracker.CarriedThing!=null)return "Put down the carried pawn or item first.";
   string compatibility=OtherworldIsekai.Problem;if(compatibility!=null)return compatibility;
   if(State==null||State.Records.Count>=PendingLimit)return "Resolve a pending otherworld arrival or silver refund first.";
   int remaining=HediffComp_CelestialAbilities.On(p).callingReadyAt-Find.TickManager.TicksGame;
   if(remaining>0)return "Calling recharges in "+remaining.ToStringTicksToPeriod()+".";
   if(money&&(preview?PreviewSilver(p):Silver(p,null))<Price)return "Requires 1,000 reachable, visible, allowed silver anywhere on this map. Storage and a home area are not required.";
   return null;
  }
  public static bool Safe(Map map,IntVec3 cell){return CelestialLightTravel.Safe(cell,map)&&!cell.GetThingList(map).Any(t=>t.def.passability==Traversability.Impassable);}
  public static bool Landing(Map map,IntVec3 wanted,out IntVec3 cell){
   cell=IntVec3.Invalid;if(map==null||!Current.Game.Maps.Contains(map)||!wanted.InBounds(map))return false;
   int checkedCells=0;foreach(var c in GenRadial.RadialCellsAround(wanted,8,true)){if(++checkedCells>128)break;if(Safe(map,c)){cell=c;return true;}}return false;
  }
  public static int Silver(Pawn p,List<Thing> selected){
   if(p==null||!p.Spawned)return 0;int sum=0;var map=p.Map;
   foreach(var t in map.listerThings.ThingsOfDef(ThingDefOf.Silver)){
    if(!t.Spawned||t.Destroyed||t.stackCount<=0||t.Position.Fogged(map)||t.IsForbidden(Faction.OfPlayer)||!p.CanReach(t,PathEndMode.ClosestTouch,Danger.Some))continue;
    if(selected!=null)selected.Add(t);sum+=Math.Min(Price-sum,t.stackCount);if(sum>=Price)return Price;
   }return sum;
  }
  public static bool Start(Pawn caster,IntVec3 target){
   string problem=Problem(caster);if(problem!=null||Find.CurrentMap!=caster.Map||!Safe(caster.Map,target)){Messages.Message(problem??"Choose a safe, visible cell on the Celestial's current map.",MessageTypeDefOf.RejectInput,false);return false;}
   var record=new OtherworldCallRecord{caster=caster,map=caster.Map,cell=target,origin=caster.Position,id=State.NextId++};State.Add(record);
   try{
    var stacks=new List<Thing>();if(Silver(caster,stacks)<Price){State.Cancel(record.id);return false;}
    int left=Price;foreach(var stack in stacks){int amount=Math.Min(left,stack.stackCount);var part=stack.SplitOff(amount);if(!record.Contents.TryAdd(part)){if(!part.Spawned)GenPlace.TryPlaceThing(part,caster.Position,caster.Map,ThingPlaceMode.Near);throw new InvalidOperationException("Could not reserve silver.");}left-=amount;if(left==0)break;}
    if(left!=0)throw new InvalidOperationException("Silver changed during reservation.");
    var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(JobName),target);job.count=record.id;job.playerForced=true;
    caster.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);
    if(caster.CurJob!=job){State.Cancel(record.id);return false;}return true;
   }catch(Exception e){State.Cancel(record.id);Log.Warning("[Elemental Hearts] Calling canceled safely: "+e.GetBaseException().Message);Messages.Message("Otherworld Calling could not start. Reserved silver is being returned.",MessageTypeDefOf.RejectInput,false);return false;}
  }
  public static Command Command(Pawn p){
   if(icon==null)icon=ContentFinder<Texture2D>.Get("UI/Abilities/OtherworldCalling");
   var c=new Command_OtherworldCalling{caster=p,defaultLabel="Otherworld Calling",defaultDesc="Spend 1,000 silver to call one permanent human colonist aged 18–24. Men arrive in a fitted boys' gakuran; women in a fitted schoolgirl sailor uniform. No Fat or Hulk recruits. Each brings one powerful ISEKAI identity and two compatible talents, but no elemental lineage until Bestow. Three-second ceremony; one in-game hour cooldown by default (configurable). Canceling before commitment returns the reserved silver. Right-click to recover a pending arrival.",icon=icon,groupable=false,targetingParams=new TargetingParameters{canTargetLocations=true,canTargetPawns=false,canTargetBuildings=false,canTargetItems=false,validator=t=>p.Spawned&&Safe(p.Map,t.Cell)},action=t=>Start(p,t.Cell)};
   string why=Problem(p,true,true);if(why!=null)c.Disable(why);return c;
  }
  public static Command DevCommand(Pawn p){return new Command_Action{defaultLabel="DEV: Calling-Ready",defaultDesc="Clear only Otherworld Calling's cooldown. The next call still requires 1,000 silver and ISEKAI compatibility.",groupable=false,action=()=>{if(!DebugSettings.godMode)return;var c=HediffComp_CelestialAbilities.On(p);if(c!=null)c.callingReadyAt=0;}};}
  public static void Recover(int id){
   var record=State==null?null:State.Find(id);Map map=Find.CurrentMap;
   if(record==null||!record.committed||record.person==null||map==null||!map.IsPlayerHome){Messages.Message("Open a player home map to choose a new arrival cell.",MessageTypeDefOf.RejectInput,false);return;}
   Find.Targeter.BeginTargeting(new TargetingParameters{canTargetLocations=true,canTargetPawns=false,canTargetBuildings=false,canTargetItems=false,validator=t=>Safe(map,t.Cell)},t=>{if(Find.CurrentMap!=map||!Safe(map,t.Cell))return;record.map=map;record.cell=t.Cell;record.awaitingRecovery=false;State.Deliver(record);});
  }
 }
 public sealed class Command_OtherworldCalling:Command_Target {
  public Pawn caster;
  public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions{get{var state=OtherworldCalling.State;if(state!=null)foreach(var r in state.Records)if(r.committed&&r.person!=null&&!r.person.Spawned){int id=r.id;yield return new FloatMenuOption("Place "+r.person.LabelShort+" (already paid)",()=>OtherworldCalling.Recover(id));}}}
 }
 public sealed class OtherworldCallRecord:IExposable,IThingHolder {
  ThingOwner inner;public GameComponent_OtherworldCalling parent;
  public int id,elapsed;public Pawn caster,person;public Map map;public IntVec3 cell=IntVec3.Invalid,origin=IntVec3.Invalid;
  public bool committed,canceled,paymentConsumed,notified,awaitingRecovery;public string identity;public List<string> talents=new List<string>();
  public OtherworldCallRecord(){inner=new ThingOwner<Thing>(this,false,LookMode.Deep);}
  public ThingOwner Contents{get{return inner;}}public IThingHolder ParentHolder{get{return parent;}}
  public ThingOwner GetDirectlyHeldThings(){return inner;}public void GetChildHolders(List<IThingHolder> children){ThingOwnerUtility.AppendThingHoldersFromThings(children,inner);}
  public void ExposeData(){Scribe_Values.Look(ref id,"EH_callId");Scribe_Values.Look(ref elapsed,"EH_callElapsed");Scribe_References.Look(ref caster,"EH_callCaster");Scribe_References.Look(ref person,"EH_callPerson");Scribe_References.Look(ref map,"EH_callMap");Scribe_Values.Look(ref cell,"EH_callCell",IntVec3.Invalid);Scribe_Values.Look(ref origin,"EH_callOrigin",IntVec3.Invalid);Scribe_Values.Look(ref committed,"EH_callCommitted");Scribe_Values.Look(ref canceled,"EH_callCanceled");Scribe_Values.Look(ref paymentConsumed,"EH_callPaid");Scribe_Values.Look(ref notified,"EH_callNotified");Scribe_Values.Look(ref awaitingRecovery,"EH_callAwaitingRecovery");Scribe_Values.Look(ref identity,"EH_callIdentity");Scribe_Collections.Look(ref talents,"EH_callTalents",LookMode.Value);Scribe_Deep.Look(ref inner,"EH_callContents",this);if(inner==null)inner=new ThingOwner<Thing>(this,false,LookMode.Deep);}
 }
 public sealed class GameComponent_OtherworldCalling:GameComponent,IThingHolder {
  public List<OtherworldCallRecord> Records=new List<OtherworldCallRecord>();public int NextId=1;readonly ThingOwner root;
  public GameComponent_OtherworldCalling(Game game){root=new ThingOwner<Thing>(this,false,LookMode.Deep);}
  public IThingHolder ParentHolder{get{return null;}}public ThingOwner GetDirectlyHeldThings(){return root;}
  public void GetChildHolders(List<IThingHolder> children){foreach(var r in Records)children.Add(r);}
  public override void ExposeData(){Scribe_Values.Look(ref NextId,"EH_otherworldNextId",1);Scribe_Collections.Look(ref Records,"EH_otherworldCalls",LookMode.Deep);if(Records==null)Records=new List<OtherworldCallRecord>();foreach(var r in Records)r.parent=this;}
  public void Add(OtherworldCallRecord r){r.parent=this;Records.Add(r);}public OtherworldCallRecord Find(int id){return Records.FirstOrDefault(r=>r.id==id);}
  public void Cancel(int id){var r=Find(id);if(r==null||r.committed||r.canceled)return;r.canceled=true;if(r.elapsed>=60&&r.map!=null&&Current.Game.Maps.Contains(r.map))r.map.GetComponent<MapComponent_OtherworldCallingVfx>().Close(r.cell);Refund(r);}
  public bool Commit(OtherworldCallRecord r){
   if(r==null||r.canceled)return false;if(r.committed)return true;
   IntVec3 landing;if(!OtherworldCalling.Landing(r.map,r.cell,out landing)){Cancel(r.id);return false;}
   Pawn person=null;
   try{
    if(r.Contents.Where(t=>t.def==ThingDefOf.Silver).Sum(t=>t.stackCount)!=OtherworldCalling.Price)throw new InvalidOperationException("Silver escrow is incomplete.");
    person=OtherworldIsekai.Generate(r);if(person==null||!r.Contents.TryAdd(person))throw new InvalidOperationException("Could not safely hold the generated person.");r.person=person;
    // No callbacks or yields between marking the fixed pawn, consuming escrow and setting cooldown.
    r.committed=true;r.cell=landing;
    foreach(var t in r.Contents.Where(t=>t.def==ThingDefOf.Silver).ToList()){r.Contents.Remove(t);t.Destroy();}r.paymentConsumed=true;
    var c=HediffComp_CelestialAbilities.On(r.caster);if(c!=null)c.callingReadyAt=Verse.Find.TickManager.TicksGame+OtherworldCalling.CooldownTicks;
    Deliver(r);return true;
   }catch(Exception e){
    if(!r.committed){if(person!=null&&!person.Destroyed){r.Contents.Remove(person);person.Destroy(DestroyMode.Vanish);}r.person=null;Cancel(r.id);}else NotifyPending(r);
    Log.Warning("[Elemental Hearts] Otherworld Calling recovered safely: "+e.GetBaseException().Message);return r.committed;
   }
  }
  public bool Deliver(OtherworldCallRecord r){
   if(r==null||!r.committed||r.person==null)return false;if(r.person.Spawned){r.Contents.Remove(r.person);return true;}
   IntVec3 at;if(!OtherworldCalling.Landing(r.map,r.cell,out at)){NotifyPending(r);return false;}
   try{
    var person=r.person;r.Contents.Remove(person);
    try{GenSpawn.Spawn(person,at,r.map);}catch{if(!person.Spawned)r.Contents.TryAdd(person);throw;}
    if(!person.Spawned){r.Contents.TryAdd(person);NotifyPending(r);return false;}
    r.cell=at;person.Rotation=Rot4.South;person.Drawer.renderer.SetAllGraphicsDirty();
    if(!r.notified){r.notified=true;Verse.Find.LetterStack.ReceiveLetter("An otherworlder answers",person.LabelShort+" has joined the colony. Age "+person.ageTracker.AgeBiologicalYears+"; "+person.gender.ToString().ToLowerInvariant()+" academy uniform. They bring a powerful ISEKAI identity and two talents, and can receive elemental power through Bestow. The completed calling consumed 1,000 silver.",LetterDefOf.PositiveEvent,person);}
    OtherworldCallingVfx.Arrival(r.map,at);return true;
   }catch(Exception e){NotifyPending(r);Log.Warning("[Elemental Hearts] Paid arrival retained for recovery: "+e.GetBaseException().Message);return r.person.Spawned;}
  }
  void NotifyPending(OtherworldCallRecord r){r.awaitingRecovery=true;if(r.notified)return;r.notified=true;var letter=(ChoiceLetter_OtherworldArrival)LetterMaker.MakeLetter("Otherworld arrival waiting","The traveler is safely held beyond the doorway. The 1,000 silver has already been paid. Open a player home map and choose a safe arrival cell; this delivers the same person without rerolling or charging again.",DefDatabase<LetterDef>.GetNamed("EH_OtherworldArrivalPending"));letter.callId=r.id;Verse.Find.LetterStack.ReceiveLetter(letter);}
  void Refund(OtherworldCallRecord r){
   if(r.committed)return;Map map=r.map!=null&&Current.Game.Maps.Contains(r.map)?r.map:Verse.Find.Maps.FirstOrDefault(m=>m.IsPlayerHome);if(map==null)return;
   IntVec3 at=r.origin.InBounds(map)?r.origin:map.Center;
   foreach(var t in r.Contents.ToList()){Thing dropped;if(t.def==ThingDefOf.Silver)r.Contents.TryDrop(t,at,map,ThingPlaceMode.Near,out dropped);}
  }
  public override void GameComponentTick(){
   if(Records.Count==0||Verse.Find.TickManager.TicksGame%30!=0)return;
   foreach(var r in Records.ToList()){
    if(!r.committed&&!r.canceled&&(r.caster==null||!OtherworldCalling.Busy(r.caster)||r.caster.CurJob.count!=r.id))r.canceled=true;
    if(r.canceled){Refund(r);if(r.Contents.Count==0)Records.Remove(r);}
    else if(r.committed&&!r.awaitingRecovery&&Deliver(r)){if(r.Contents.Count==0)Records.Remove(r);}
   }
  }
 }
 public sealed class ChoiceLetter_OtherworldArrival:ChoiceLetter {
  public int callId;
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref callId,"EH_pendingCallId");}
  public override IEnumerable<DiaOption> Choices{get{var state=OtherworldCalling.State;var r=state==null?null:state.Find(callId);if(r!=null&&r.person!=null&&!r.person.Spawned)yield return new DiaOption("Choose arrival cell"){action=()=>OtherworldCalling.Recover(callId),resolveTree=true};else yield return new DiaOption("Close"){action=()=>Verse.Find.LetterStack.RemoveLetter(this),resolveTree=true};yield return Option_Postpone;}}
 }
 public sealed class JobDriver_OtherworldCalling:JobDriver {
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  OtherworldCallRecord Record{get{return OtherworldCalling.State==null?null:OtherworldCalling.State.Find(job.count);}}
  bool Valid(){var r=Record;return r!=null&&!r.canceled&&pawn.Spawned&&pawn.Map==r.map&&pawn.Position==r.origin&&CelestialAscendant.Active(pawn)&&ElementalTeaching.Ready(pawn)&&!ElementalAnimationBridge.ExternallyControlled(pawn);}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>{if(OtherworldCalling.State!=null)OtherworldCalling.State.Cancel(job.count);});this.FailOn(()=>!Valid());
   var hold=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};
   hold.initAction=()=>{pawn.pather.StopDead();pawn.stances.CancelBusyStanceHard();var r=Record;if(r!=null&&r.elapsed==0)ElementalAudio.Play(pawn,"EH_TransformStart");};
   hold.tickAction=()=>{var r=Record;if(r==null){EndJobWith(JobCondition.Incompletable);return;}pawn.pather.StopDead();pawn.rotationTracker.FaceCell(r.cell);r.elapsed++;if(r.elapsed==60)ElementalAudio.Play(pawn,"EH_TransformCharge");if(r.elapsed>=OtherworldCalling.Duration){bool done=OtherworldCalling.State.Commit(r);EndJobWith(done?JobCondition.Succeeded:JobCondition.Incompletable);}};
   yield return hold;
  }
 }
}



