using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public enum LunarPower { Crescent=0, Step=1, Spin=2, FallingMoon=3, Rush=4, Parry=2, Veil=4 }

 public sealed class LunarMark : IExposable {
  public Pawn target;
  public int stacks, expires;
  public void ExposeData(){
   Scribe_References.Look(ref target,"target");
   Scribe_Values.Look(ref stacks,"stacks");
   Scribe_Values.Look(ref expires,"expires");
  }
 }

 public static class LunarAscendant {
  public const string DefName="EH_LunarHeart", Technique="EH_LunarTechnique";
  public static bool Busy(Pawn pawn){return pawn!=null&&pawn.CurJobDef!=null&&pawn.CurJobDef.defName==Technique;}

  [DebugAction("Elemental Hearts","Grant Lunar ascension",actionType=DebugActionType.ToolMapForPawns,allowedGameStates=AllowedGameStates.PlayingOnMap)]
  public static void Grant(Pawn pawn){
   if(ElementalAscension.Apply(pawn,DefDatabase<HediffDef>.GetNamed(DefName)))
    Messages.Message(pawn.LabelShort+" received Lunar ascension.",pawn,MessageTypeDefOf.PositiveEvent,false);
  }
 }

 public sealed class HediffCompProperties_LunarAbilities : HediffCompProperties {
  public HediffCompProperties_LunarAbilities(){compClass=typeof(HediffComp_LunarAbilities);}
 }

 public sealed class HediffComp_LunarAbilities : HediffComp {
  internal static readonly string[] Art={"CrescentDraw","MoonfallStep","SeleneDance","FallingMoon","MoonbreakRush"};
  static readonly string[] Names={"Crescent Draw","Moonfall Step","Selene Dance","Falling Moon","Moonbreak Rush"};
  static readonly string[] Descriptions={
   "Release a fast silver sword wave along an eleven-cell lane. Each hostile is struck once and gains a Moonbrand.",
   "Dash up to eight clear cells to an enemy's flank and strike once. Draw nearby enemies one safe cell toward the impact. Moonbrands are a bonus, so you can open with the dash.",
   "Spin Moonwake through a 3.6-cell circle. A rotating silver double crescent shows one strike per visible hostile. Successful hits build Moonbrands.",
   "Mark fixed ground within eleven cells. After a 0.4-second warning, a 3.2-cell lunar explosion hits each enemy once. Spend your marks for 20% extra damage per stack, up to three.",
   "When two hostiles pressure you within three cells, leap up to four cells away and rush back along a safe lane. Each enemy crossed receives one powerful cut. Auto Combat can use this move."};
  static readonly int[] Cooldowns={240,300,480,900,1080};
  static readonly float[] Ranges={11f,8f,3.6f,11f,3f};
  public bool autoCombat=true, nonLethal, eclipseReady;
  List<int> readyAt=new List<int>{0,0,0,0,0};
  List<LunarMark> marks=new List<LunarMark>();
  int nextAuto, comboUntil, lastAdvance=-1;
  Pawn comboTarget;
  int comboStep;
  static int Now {get{return Find.TickManager.TicksGame;}}

  public static HediffComp_LunarAbilities On(Pawn pawn){
   if(pawn==null||pawn.health==null)return null;
   foreach(var h in pawn.health.hediffSet.hediffs)
    if(h.def.defName==LunarAscendant.DefName)return h.TryGetComp<HediffComp_LunarAbilities>();
   return null;
  }
  public override string CompTipStringExtra {get{return eclipseReady?"Celestial Bestow fulfilled this pawn's Eclipse evolution milestones. Press Evolve: Eclipse when ready.":"Crescent Draw builds up to three Moonbrands per target. Each Lunar Ascendant owns their own marks.";}}
  public ElementalAutoMode Mode {get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  internal bool CanAct {
   get {
    return ElementalTeaching.Ready(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&
     !Pawn.WorkTagIsDisabled(WorkTags.Violent)&&HediffComp_ElementalTransform.Active(Pawn)&&
     Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon&&
     !HediffComp_ElementalTransform.Transforming(Pawn)&&!ElementalTeachingSession.Busy(Pawn)&&
     !ElementalAnimationBridge.Controlled(Pawn)&&On(Pawn)==this;
   }
  }

  void Register(){if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_LunarCombat>().Register(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){Register();}
  public override void Notify_Spawned(){Register();}
  public override void CompPostPostRemoved(){if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_LunarCombat>().Forget(Pawn);}

  public override void CompExposeData(){
   Scribe_Collections.Look(ref readyAt,"EH_lunarCooldowns",LookMode.Value);
   Scribe_Collections.Look(ref marks,"EH_moonbrands",LookMode.Deep);
   Scribe_Values.Look(ref autoCombat,"EH_lunarAuto",true);
   Scribe_Values.Look(ref nonLethal,"EH_lunarNonLethal");
   Scribe_Values.Look(ref eclipseReady,"EH_eclipseReady");
   Scribe_Values.Look(ref comboStep,"EH_lunarComboStep");
   Scribe_Values.Look(ref comboUntil,"EH_lunarComboUntil");
   Scribe_References.Look(ref comboTarget,"EH_lunarComboTarget");
   if(Scribe.mode==LoadSaveMode.PostLoadInit){
    if(readyAt==null)readyAt=new List<int>();
    while(readyAt.Count<5)readyAt.Add(0);
    if(readyAt.Count>5)readyAt.RemoveRange(5,readyAt.Count-5);
    if(marks==null)marks=new List<LunarMark>();
   }
  }

  internal List<int> ExportDeadlines(){return new List<int>(readyAt);}
  internal List<LunarMark> ExportMarks(){return new List<LunarMark>(marks);}
  void PruneMarks(){marks.RemoveAll(mark=>mark==null||mark.target==null||mark.target.Dead||mark.target.Destroyed||mark.expires<=Now);}
  public int Marks(Pawn target){
   foreach(var mark in marks)if(mark!=null&&mark.target==target&&mark.expires>Now)return mark.stacks;
   return 0;
  }
  internal void Mark(Pawn target){
   if(!CelestialCombat.Hostile(Pawn,target))return;
   PruneMarks();
   foreach(var mark in marks){
    if(mark.target!=target)continue;
    mark.stacks=Math.Min(3,mark.stacks+1);mark.expires=Now+480;return;
   }
   if(marks.Count>=24)marks.RemoveAt(0);
   marks.Add(new LunarMark{target=target,stacks=1,expires=Now+480});
  }
  internal int Consume(Pawn target){
   int count=Marks(target);
   marks.RemoveAll(mark=>mark!=null&&mark.target==target);
   return count;
  }

  public int Remaining(LunarPower power){return Math.Max(0,readyAt[(int)power]-Now);}
  bool Valid(LunarPower power, LocalTargetInfo target){
   if(power==LunarPower.Spin)return true;
   if(power==LunarPower.Rush){Pawn opponent;IntVec3 leap,end;return LunarMovement.PlanRush(Pawn,out opponent,out leap,out end);}
   var cell=target.Cell;
   float range=Ranges[(int)power];
   if(!cell.InBounds(Pawn.Map)||cell.Fogged(Pawn.Map)||Pawn.Position.DistanceToSquared(cell)>range*range||
    !LightningCombat.ClearLine(Pawn.Position,cell,Pawn.Map))return false;
   if(power==LunarPower.FallingMoon||power==LunarPower.Crescent)return LightningCombat.Open(cell,Pawn.Map);
   if(!CelestialCombat.Hostile(Pawn,target.Pawn))return false;
   IntVec3 landing;
   return power!=LunarPower.Step||LunarMovement.DashLanding(Pawn,target.Pawn,out landing);
  }

  public bool TryCast(LunarPower power,LocalTargetInfo target,bool automatic){
   if((int)power<0||(int)power>=Names.Length||!CanAct||LunarAscendant.Busy(Pawn)||Remaining(power)>0)return false;
   if(automatic&&(!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat)||Pawn.jobs.jobQueue.Count>0))return false;
   Job job;
   if(power==LunarPower.Rush){
    Pawn opponent;IntVec3 leap,end;
    if(!LunarMovement.PlanRush(Pawn,out opponent,out leap,out end))return false;
    job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(LunarAscendant.Technique),opponent,leap);
    job.targetC=end;
   }else{
    if(!Valid(power,target))return false;
    if(power==LunarPower.Spin)target=Pawn;
    if(power==LunarPower.FallingMoon)target=target.Cell;
    job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(LunarAscendant.Technique),target);
   }
   job.count=(int)power;job.playerForced=!automatic;
   if(automatic){
    Pawn.stances.CancelBusyStanceHard();
    Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true,cancelBusyStances:true);
    Pawn.stances.CancelBusyStanceHard();
   }else{
    if(!Pawn.jobs.TryTakeOrderedJob(job,JobTag.Misc))return false;
    Pawn.stances.CancelBusyStanceHard();
   }
   if(Pawn.CurJob!=job)return false;
   readyAt[(int)power]=Now+Cooldowns[(int)power];
   return true;
  }

  internal void AdvanceCombo(Pawn target,int step){
   if(!CelestialCombat.Hostile(Pawn,target))return;
   comboTarget=target;comboStep=step%4;comboUntil=Now+420;
  }
  internal void Released(){nextAuto=Now+2;}
  public override void CompPostTickInterval(ref float adjustment,int delta){Advance(Now);}
  internal void Advance(int now){
   if(now==lastAdvance)return;lastAdvance=now;
   if(!Pawn.Spawned)return;
   if(now%15==0&&Pawn.Drafted&&HediffComp_ElementalTransform.Active(Pawn))LunarVfx.TrackAura(Pawn);
   if(now<nextAuto)return;
   nextAuto=now+30;
   if(marks.Count>0)PruneMarks();
   if(!CanAct||LunarAscendant.Busy(Pawn)||Pawn.CurJob==null||Pawn.jobs.jobQueue.Count>0||
    !ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat))return;
   var job=Pawn.CurJob;
   if(ElementalCombatPolicy.Controlled(Pawn)&&job.def!=JobDefOf.AttackMelee&&job.def!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,job))return;
   if(Pawn.stances.FullBodyBusy&&!(Pawn.stances.curStance is Stance_Cooldown))return;
   Pawn target=job.targetA.Pawn;
   if(!CelestialCombat.Hostile(Pawn,target)&&comboUntil>now&&CelestialCombat.Hostile(Pawn,comboTarget))target=comboTarget;
   if(!CelestialCombat.Hostile(Pawn,target))target=CelestialCombat.Nearest(Pawn,11f);
   if(target==null)return;
   nextAuto=now+6;
   if(Remaining(LunarPower.Rush)==0&&LunarMovement.Pressured(Pawn)&&TryCast(LunarPower.Rush,Pawn,true))return;
   int start=target==comboTarget&&comboUntil>now?comboStep:0;
   for(int offset=0;offset<4;offset++){
    var power=(LunarPower)((start+offset)%4);
    if(power==LunarPower.Spin&&!LunarMovement.NearbyEnemy(Pawn,3.6f))continue;
    if(power==LunarPower.FallingMoon&&Marks(target)==0&&start!=3)continue;
    if(TryCast(power,power==LunarPower.FallingMoon?new LocalTargetInfo(target.Position):new LocalTargetInfo(target),true))return;
   }
  }

  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn)||!Pawn.Drafted)yield break;
   yield return new Command_Action{
    defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),
    defaultDesc="Cycle On / Off / Non-lethal. Chain Crescent Draw, Moonfall Step, Selene Dance and Falling Moon. Moonbreak Rush answers two nearby enemies.",
    icon=ElementalIcons.AutoCombat, groupable=false,
    action=()=>{var next=ElementalAutoModes.Next(Mode);autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;nextAuto=0;}
   };
   for(int i=0;i<Names.Length;i++){
    var power=(LunarPower)i;
    Command command;
    if(power==LunarPower.Spin||power==LunarPower.Rush)command=new Command_Action{action=()=>TryCast(power,Pawn,false)};
    else command=new Command_Target{
     targetingParams=new TargetingParameters{canTargetPawns=power!=LunarPower.FallingMoon,canTargetLocations=power==LunarPower.FallingMoon||power==LunarPower.Crescent,
      canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,neverTargetIncapacitated=true,
      validator=t=>Valid(power,t.HasThing?new LocalTargetInfo(t.Thing):new LocalTargetInfo(t.Cell))},
     action=t=>TryCast(power,t,false),onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,Ranges[(int)power]);}
    };
    command.defaultLabel=Names[i];command.defaultDesc=Descriptions[i]+" Cooldown: "+Cooldowns[i]/60f+"s.";
    command.icon=LunarVfx.Icon(i);command.groupable=false;
    if(!CanAct)command.Disable("Requires a conscious, drafted and transformed Lunar Ascendant with a melee weapon.");
    else if(LunarAscendant.Busy(Pawn))command.Disable("Finish the current Lunar technique.");
    else if(Remaining(power)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(power)/60f)+"s");
    else if(power==LunarPower.Rush&&!LunarMovement.Pressured(Pawn))command.Disable("Requires two nearby hostiles.");
    yield return command;
   }
  }
 }
}
