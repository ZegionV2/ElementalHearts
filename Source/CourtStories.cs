using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace ElementalHearts {
 public sealed class CourtOrder:IExposable {
  public Map map;public Faction faction;public CourtStronghold origin;public Settlement settlementOrigin;public Settlement Origin{get{return settlementOrigin??origin;}}public int due,seed,element,attempts,id;public string storyKind;public float points;public bool herald,committed;
  public void ExposeData(){Scribe_Values.Look(ref id,"id");Scribe_Values.Look(ref storyKind,"storyKind");Scribe_References.Look(ref map,"map");Scribe_References.Look(ref faction,"faction");Scribe_References.Look(ref origin,"stronghold");Scribe_References.Look(ref settlementOrigin,"settlementOrigin");Scribe_Values.Look(ref due,"due");Scribe_Values.Look(ref seed,"seed");Scribe_Values.Look(ref element,"element");Scribe_Values.Look(ref points,"points");Scribe_Values.Look(ref herald,"herald");Scribe_Values.Look(ref committed,"committed");Scribe_Values.Look(ref attempts,"attempts");}
 }
 public sealed class WorldComponent_CourtStories:WorldComponent {
  public const int OrderLimit=2;
  List<CourtOrder> orders=new List<CourtOrder>();int lastWrit=-900000,lastDeserter=-1800000,lastAssault=-720000,lastIntel=-720000,lastCrucible=-1200000,lastUltimatum=-1800000,nextOrderId;
  public WorldComponent_CourtStories(World world):base(world){}
  public int Pending{get{return orders.Count;}}
  public int LastCrucible{get{return lastCrucible;}}public int LastUltimatum{get{return lastUltimatum;}}
  public void MarkCrucible(){lastCrucible=Find.TickManager.TicksGame;}public void MarkUltimatum(){lastUltimatum=Find.TickManager.TicksGame;}
  public CourtOrder FindOrder(int id){return id<=0?null:orders.Find(o=>o!=null&&o.id==id&&!o.committed);}
  public bool CancelOrder(int id){var order=FindOrder(id);if(order==null)return false;order.committed=true;orders.Remove(order);return true;}
  public CourtOrder QueueOffer(Map map,Faction faction,float points,bool herald,int element,int delay,Settlement origin,string kind){if(!Queue(map,faction,points,herald,element,delay,origin))return null;var order=orders[orders.Count-1];order.storyKind=kind;return order;}
  public int LastAssault{get{return lastAssault;}}public int LastIntel{get{return lastIntel;}}
  public void MarkAssault(){lastAssault=Find.TickManager.TicksGame;}public void MarkIntel(){lastIntel=Find.TickManager.TicksGame;}
  public int LastWrit{get{return lastWrit;}}public int LastDeserter{get{return lastDeserter;}}
  public override void ExposeData(){Scribe_Values.Look(ref lastCrucible,"EH_courtLastCrucible",-1200000);Scribe_Values.Look(ref lastUltimatum,"EH_courtLastUltimatum",-1800000);Scribe_Values.Look(ref nextOrderId,"EH_courtNextOrderId");Scribe_Collections.Look(ref orders,"EH_courtStoryOrders",LookMode.Deep);Scribe_Values.Look(ref lastAssault,"EH_courtLastAssault",-720000);Scribe_Values.Look(ref lastIntel,"EH_courtLastIntel",-720000);Scribe_Values.Look(ref lastWrit,"EH_courtLastWrit",-900000);Scribe_Values.Look(ref lastDeserter,"EH_courtLastDeserter",-1800000);if(orders==null)orders=new List<CourtOrder>();if(Scribe.mode==LoadSaveMode.PostLoadInit)foreach(var o in orders)if(o!=null){if(o.id<=0)o.id=++nextOrderId;else nextOrderId=Math.Max(nextOrderId,o.id);}}
  public bool HasOrder(Map map){foreach(var o in orders)if(o!=null&&o.map==map&&!o.committed)return true;return false;}
  public bool CanWrit(Map map,float points){return map!=null&&map.IsPlayerHome&&CourtStories.Enabled&&CourtHeralds.Eligible(points,Find.TickManager.TicksGame)&&(!CourtStrongholds.CampaignSeeded||CourtStrongholds.Bases().Exists(h=>!h.HasMap))&&orders.Count<OrderLimit&&!HasOrder(map)&&Find.TickManager.TicksGame-lastWrit>=15*60000;}
  public bool Queue(Map map,Faction faction,float points,bool herald,int element,int delay){return Queue(map,faction,points,herald,element,delay,(Settlement)null);}
  public bool Queue(Map map,Faction faction,float points,bool herald,int element,int delay,CourtStronghold origin){return Queue(map,faction,points,herald,element,delay,(Settlement)origin);}
  public bool Queue(Map map,Faction faction,float points,bool herald,int element,int delay,Settlement origin){
   if(map==null||!Current.Game.Maps.Contains(map)||faction==null||faction.defeated||!faction.HostileTo(Faction.OfPlayer)||orders.Count>=OrderLimit||HasOrder(map)||!CourtStories.Enabled||!ElementalHeartsMod.Settings.courtRaids||float.IsNaN(points)||float.IsInfinity(points))return false;
   if(herald&&!CourtHeralds.Eligible(points,Find.TickManager.TicksGame)||origin!=null&&(origin.Destroyed||origin.Faction!=faction))return false;
   if(!herald&&points<CourtRaidRules.DiscipleCost)return false;
   orders.Add(new CourtOrder{id=++nextOrderId,map=map,origin=origin as CourtStronghold,settlementOrigin=origin,faction=faction,points=Math.Min(points,10000),herald=herald,element=Math.Max(0,Math.Min(4,element)),seed=Rand.Int,due=Find.TickManager.TicksGame+Math.Max(60,delay)});if(herald)lastWrit=Find.TickManager.TicksGame;return true;
  }
  public void CancelFrom(CourtStronghold origin){CancelFrom((Settlement)origin);}
  public void CancelFrom(Settlement origin){for(int i=orders.Count-1;i>=0;i--)if(orders[i]!=null&&orders[i].Origin==origin){var map=orders[i].map;orders.RemoveAt(i);if(map!=null&&Current.Game.Maps.Contains(map))Find.LetterStack.ReceiveLetter("Court expedition withdrawn","The expedition from "+origin.Label+" has been cancelled. Its stronghold has fallen or been removed.",LetterDefOf.NeutralEvent,map.Parent);}}
  public void MarkDeserter(){lastDeserter=Find.TickManager.TicksGame;}
  public override void WorldComponentTick(){if(Find.TickManager==null||Find.TickManager.TicksGame%60!=0||orders.Count==0)return;int now=Find.TickManager.TicksGame;
   for(int i=orders.Count-1;i>=0;i--){var order=orders[i];if(order==null||order.committed||order.map==null||!Current.Game.Maps.Contains(order.map)||order.faction==null||order.faction.defeated||!order.faction.HostileTo(Faction.OfPlayer)){orders.RemoveAt(i);continue;}
    if(!CourtStories.Enabled||!ElementalHeartsMod.Settings.courtRaids||order.herald&&!ElementalHeartsMod.Settings.courtHeralds){orders.RemoveAt(i);Find.LetterStack.ReceiveLetter("Court force withdraws","A previously announced Court force has withdrawn. Its event or raid setting was disabled.",LetterDefOf.NeutralEvent,order.map.Parent);continue;}
    if(order.Origin!=null&&(order.Origin.Destroyed||order.Origin.HasMap)){orders.RemoveAt(i);Find.LetterStack.ReceiveLetter("Court force recalled","The announced Court expedition was recalled to defend its stronghold.",LetterDefOf.NeutralEvent,order.map.Parent);continue;}if(now<order.due)continue;
    // Commit/remove before invoking the native raid worker: an arrival can never be replayed by a save.
    order.committed=true;orders.RemoveAt(i);bool success=false,faulted=false;
    try{using(new CourtRaidScope(order.herald,order.element)){var parms=new IncidentParms{target=order.map,faction=order.faction,points=order.points,raidStrategy=RaidStrategyDefOf.ImmediateAttack,raidArrivalMode=PawnsArrivalModeDefOf.EdgeWalkIn,pawnGroupMakerSeed=order.seed,customLetterLabel=order.storyKind=="ultimatum"?"Court ultimatum enforced":order.storyKind=="crucible"?"Crucible recovery force":order.herald?"Herald's writ fulfilled":"Oathbreaker pursuit",customLetterText=order.storyKind=="crucible"?"The Court recovery force announced when you secured the crucible has reached your perimeter. It seeks to reclaim the elemental materials.":order.herald?"The Severed Court's announced "+CourtHeralds.Titles[order.element]+" has reached your perimeter with an escort. The commander wears elite ivory-and-obsidian regalia and an open-brow faceguard, and commands five "+CourtHeralds.Lineages[order.element]+" techniques.":"The Severed Court has sent a recovery squad after the disciple you sheltered. The force has reached your perimeter."};success=IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);}}
    catch(Exception e){faulted=true;Log.Error("[Elemental Hearts] Court story raid failed: "+e);}
    // A native failure without an exception has not arrived. An exception may follow a partial arrival;
    // never replay that uncertain side effect.
    if(!success&&!faulted&&order.attempts<2){order.committed=false;order.attempts++;order.due=now+2500;orders.Add(order);}else if(!success)Find.LetterStack.ReceiveLetter("Court assault dispersed","The announced Court force could not complete its approach. No further arrival is scheduled.",LetterDefOf.NeutralEvent,order.map.Parent);break;
   }
  }
 }
 public static class CourtStories {
  public static bool Enabled{get{return ElementalHeartsMod.Settings!=null&&ElementalHeartsMod.Settings.courtStories;}}
  public static WorldComponent_CourtStories State{get{return Find.World==null?null:Find.World.GetComponent<WorldComponent_CourtStories>();}}
  public static Faction Enemy{get{if(Find.FactionManager==null)return null;foreach(var f in Find.FactionManager.AllFactionsListForReading)if(SeveredCourt.IsFaction(f)&&!f.defeated&&f.HostileTo(Faction.OfPlayer))return f;return null;}}
  public static bool Entry(Map map,out IntVec3 cell){cell=IntVec3.Invalid;return map!=null&&Current.Game.Maps.Contains(map)&&CellFinder.TryFindRandomEdgeCellWith(c=>c.Standable(map)&&c.GetFirstPawn(map)==null,map,0,out cell);}
 }
 public sealed class IncidentWorker_CourtHeraldWrit:IncidentWorker {
  protected override bool CanFireNowSub(IncidentParms parms){var map=parms.target as Map;return CourtStories.Enabled&&CourtStories.Enemy!=null&&CourtStories.State!=null&&CourtStories.State.CanWrit(map,parms.points);}
  protected override bool TryExecuteWorker(IncidentParms parms){var map=parms.target as Map;if(!CanFireNowSub(parms))return false;int element;Settlement origin;if(!CourtStrongholds.TryBaseCommander(out element,out origin)||!CourtStories.State.Queue(map,CourtStories.Enemy,parms.points,true,element,120000,origin))return false;
   Find.LetterStack.ReceiveLetter("Herald's writ: "+CourtHeralds.Titles[element],"The Severed Court has delivered a sealed writ. An evolved "+CourtHeralds.Titles[element]+" is marching against this settlement with an escort"+(origin==null?".":" from "+origin.Label+".")+"\n\nThe force is expected in two days. Prepare cover, recovery supplies and your own Ascendants.\n\nOne Herald leads the force, commanding five techniques of the "+CourtHeralds.Lineages[element]+" lineage. Its elite regalia and open-brow faceguard mark it among the escort.",DefDatabase<LetterDef>.GetNamed("EH_CourtHeraldWarning"),map.Parent,CourtStories.Enemy);return true;
  }
 }
 public sealed class IncidentWorker_CourtDeserter:IncidentWorker {
  protected override bool CanFireNowSub(IncidentParms parms){var map=parms.target as Map;var state=CourtStories.State;if(!CourtStories.Enabled||CourtStories.Enemy==null||state==null||!ElementalHeartsMod.Settings.courtRaids||map==null||!map.IsPlayerHome||Find.TickManager.TicksGame<30*60000||state.Pending>=WorldComponent_CourtStories.OrderLimit||state.HasOrder(map)||Find.TickManager.TicksGame-state.LastDeserter<30*60000)return false;foreach(var letter in Find.LetterStack.LettersListForReading)if(letter is ChoiceLetter_CourtDeserter&&!((ChoiceLetter_CourtDeserter)letter).Resolved)return false;IntVec3 entry;return CourtStories.Entry(map,out entry);}
  protected override bool TryExecuteWorker(IncidentParms parms){if(!CanFireNowSub(parms))return false;var map=(Map)parms.target;int element=Rand.Range(0,5);var kind=DefDatabase<PawnKindDef>.GetNamed("EH_CourtOathbreaker"+SeveredCourt.Elements[element]);var pawn=PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind,CourtStories.Enemy,forceGenerateNewPawn:true,mustBeCapableOfViolence:true,canGeneratePawnRelations:true));Find.WorldPawns.PassToWorld(pawn,PawnDiscardDecideMode.KeepForever);
   var letter=(ChoiceLetter_CourtDeserter)LetterMaker.MakeLetter("A broken oath",pawn.LabelShort+", a "+SeveredCourt.Elements[element]+" disciple of the Severed Court, asks for sanctuary. They abandoned a punitive expedition and escaped with minor injuries.\n\nAccepting brings one disciple into your colony. They renounce the Severed Oath, keeping their elemental power and bound sword; future transformations use the normal elemental apparel. The Court will learn of the defection: expect a warned recovery squad in about a day and a half. That pursuit contains ordinary Court ranks.\n\nYou may refuse without receiving a pursuit raid.",DefDatabase<LetterDef>.GetNamed("EH_CourtDeserterOffer"));letter.pawn=pawn;letter.map=map;letter.faction=CourtStories.Enemy;letter.element=element;letter.pursuitPoints=Math.Max(1200,Math.Min(3600,StorytellerUtility.DefaultThreatPointsNow(map)));letter.StartTimeout(60000);Find.LetterStack.ReceiveLetter(letter);CourtStories.State.MarkDeserter();return true;
  }
 }
 public sealed class ChoiceLetter_CourtDeserter:ChoiceLetter {
  public Pawn pawn;public Map map;public Faction faction;public CourtStronghold origin;public int element;public float pursuitPoints;bool resolved;
  public bool Resolved{get{return resolved;}}
  public override void ExposeData(){base.ExposeData();Scribe_References.Look(ref pawn,"EH_deserterPawn");Scribe_References.Look(ref map,"EH_deserterMap");Scribe_References.Look(ref faction,"EH_deserterFaction");Scribe_Values.Look(ref element,"EH_deserterElement");Scribe_Values.Look(ref pursuitPoints,"EH_deserterPursuitPoints");Scribe_Values.Look(ref resolved,"EH_deserterResolved");if(Scribe.mode==LoadSaveMode.PostLoadInit)Text=Text.ToString().Replace("retaining their oath and bound sword","renouncing the Severed Oath while keeping their elemental power and bound sword");}
  bool Available(out string reason){reason=null;if(resolved||pawn==null||pawn.Dead||map==null||!Current.Game.Maps.Contains(map))reason="This offer is no longer available.";else if(!CourtStories.Enabled||!ElementalHeartsMod.Settings.courtRaids)reason="Court stories and raids must be enabled to accept this encounter.";else if(faction==null||faction.defeated||!faction.HostileTo(Faction.OfPlayer))reason="The pursuing Court faction is no longer hostile or active.";else if(CourtStories.State==null||CourtStories.State.HasOrder(map)||CourtStories.State.Pending>=WorldComponent_CourtStories.OrderLimit)reason="Resolve the pending Court encounter before offering sanctuary.";return reason==null;}
  public bool Accept(){IntVec3 entry;string reason;if(!Available(out reason)||!CourtStories.Entry(map,out entry))return false;
   // Reserve the warned pursuit first. The choice never grants an unannounced follow-up attack.
   if(!CourtStories.State.Queue(map,faction,pursuitPoints,false,element,90000))return false;resolved=true;Find.WorldPawns.RemovePawn(pawn);pawn.SetFaction(Faction.OfPlayer);GenSpawn.Spawn(pawn,entry,map);var injury=HediffMaker.MakeHediff(HediffDefOf.Cut,pawn,pawn.RaceProps.body.corePart);injury.Severity=4;pawn.health.AddHediff(injury);
   Find.LetterStack.ReceiveLetter("Sanctuary granted",pawn.LabelShort+" has joined your colony. A Court recovery squad is expected at this settlement in about a day and a half. Treat the disciple's injuries and prepare your defenses.",LetterDefOf.PositiveEvent,pawn,faction);Find.LetterStack.RemoveLetter(this);return true;
  }
  public void Refuse(){if(resolved)return;resolved=true;Find.LetterStack.RemoveLetter(this);}
  public override IEnumerable<DiaOption> Choices{get{var yes=new DiaOption("Offer sanctuary"){action=()=>Accept(),resolveTree=true};string reason;if(!Available(out reason))yes.Disable(reason);yield return yes;yield return new DiaOption("Refuse sanctuary"){action=Refuse,resolveTree=true};yield return Option_Postpone;}}
  public override void Removed(){resolved=true;if(pawn!=null&&!pawn.Spawned&&!pawn.Destroyed&&Find.WorldPawns!=null){Find.WorldPawns.RemovePawn(pawn);Find.WorldPawns.PassToWorld(pawn,PawnDiscardDecideMode.Decide);}base.Removed();}
 }
}
