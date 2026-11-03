using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public enum LunarPower { Crescent, Step, Parry, FallingMoon, Veil }

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
  internal static readonly string[] Art={"CrescentDraw","MoonfallStep","TidalParry","FallingMoon","Moonveil"};
  static readonly string[] Names={"Crescent Draw","Moonfall Step","Tidal Parry","Falling Moon","Moonveil"};
  static readonly string[] Descriptions={
   "Send a silver blade wave down a clear lane. Each enemy struck once gains one Moonbrand for eight seconds.",
   "Dash to an open tile beside a Moonbranded enemy and strike. Pull nearby enemies one safe tile toward the impact. Your marks remain for Falling Moon.",
   "Watch the selected opponent for two seconds. Deflect their first melee hit, sidestep if possible and counter with a cut that briefly pins the attacker.",
   "Mark a fixed ground position, then release a silver shockwave after a short warning. Consume your Moonbrands for 35% more damage per stack. Each enemy is hit once.",
   "Ward yourself and nearby allies for four seconds. Ranged weapon damage falls by 30%. Multiple wards refresh the duration."};
  static readonly int[] Cooldowns={300,420,780,1080,1320};
  static readonly float[] Ranges={9f,7f,1.5f,10f,3f};
  public bool autoCombat=true, nonLethal, eclipseReady;
  List<int> readyAt=new List<int>{0,0,0,0,0};
  List<LunarMark> marks=new List<LunarMark>();
  int nextAuto, comboUntil;
  Pawn comboTarget;
  int comboStep;
  static int Now {get{return Find.TickManager.TicksGame;}}

  public static HediffComp_LunarAbilities On(Pawn pawn){
   if(pawn==null||pawn.health==null)return null;
   foreach(var h in pawn.health.hediffSet.hediffs)
    if(h.def.defName==LunarAscendant.DefName)return h.TryGetComp<HediffComp_LunarAbilities>();
   return null;
  }
  public override string CompTipStringExtra {get{return eclipseReady?"Celestial Bestow fulfilled this pawn's future Eclipse evolution milestones.":"Crescent Draw builds up to three Moonbrands per target. Each Lunar Ascendant owns their own marks.";}}
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
    if(marks==null)marks=new List<LunarMark>();
   }
  }

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

  int Remaining(LunarPower power){return Math.Max(0,readyAt[(int)power]-Now);}
  bool Valid(LunarPower power, LocalTargetInfo target){
   if(power==LunarPower.Veil)return true;
   var cell=target.Cell;
   float range=Ranges[(int)power];
   if(!cell.InBounds(Pawn.Map)||cell.Fogged(Pawn.Map)||Pawn.Position.DistanceToSquared(cell)>range*range||
    !LightningCombat.ClearLine(Pawn.Position,cell,Pawn.Map))return false;
   if(power==LunarPower.FallingMoon)return LightningCombat.Open(cell,Pawn.Map);
   if(!CelestialCombat.Hostile(Pawn,target.Pawn))return false;
   IntVec3 landing;
   return power!=LunarPower.Step||(Marks(target.Pawn)>0&&LunarCombat.Landing(Pawn,target.Pawn,out landing));
  }

  public bool TryCast(LunarPower power,LocalTargetInfo target,bool automatic){
   if((int)power<0||(int)power>=Names.Length||!CanAct||LunarAscendant.Busy(Pawn)||Remaining(power)>0||!Valid(power,target))return false;
   if(automatic&&(!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat)||Pawn.jobs.jobQueue.Count>0))return false;
   if(power==LunarPower.Veil)target=Pawn;
   if(power==LunarPower.FallingMoon)target=target.Cell;
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(LunarAscendant.Technique),target);
   job.count=(int)power;
   job.playerForced=!automatic;
   if(automatic){
    Pawn.stances.CancelBusyStanceHard();
    Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);
   }else{
    if(!Pawn.jobs.TryTakeOrderedJob(job,JobTag.Misc))return false;
    Pawn.stances.CancelBusyStanceHard();
   }
   if(Pawn.CurJob!=job)return false;
   readyAt[(int)power]=Now+Cooldowns[(int)power];
   return true;
  }

  internal void AdvanceCombo(Pawn target,int step){
   if(!CelestialCombat.Hostile(Pawn,target)||Marks(target)==0)return;
   comboTarget=target;comboStep=step;comboUntil=Now+300;
  }
  public override void CompPostTickInterval(ref float adjustment,int delta){
   if(!Pawn.Spawned)return;
   if(Pawn.Drafted&&HediffComp_ElementalTransform.Active(Pawn))LunarVfx.TrackAura(Pawn);
   if(Now<nextAuto)return;
   nextAuto=Now+15;
   if(marks.Count>0)PruneMarks();
   if(!CanAct||LunarAscendant.Busy(Pawn)||Pawn.CurJob==null||Pawn.jobs.jobQueue.Count>0||
    !ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat))return;
   var job=Pawn.CurJob;
   if(ElementalCombatPolicy.Controlled(Pawn)&&job.def!=JobDefOf.AttackMelee&&job.def!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,job))return;
   if(Pawn.stances.FullBodyBusy&&!(Pawn.stances.curStance is Stance_Cooldown))return;
   Pawn target=job.targetA.Pawn;
   if(!CelestialCombat.Hostile(Pawn,target)&&comboUntil>Now&&CelestialCombat.Hostile(Pawn,comboTarget)&&Marks(comboTarget)>0)
    target=comboTarget;
   if(target!=comboTarget||comboUntil<=Now)comboStep=0;
   if(!CelestialCombat.Hostile(Pawn,target))target=CelestialCombat.Nearest(Pawn,9f);
   if(target==null)return;
   int distance=Pawn.Position.DistanceToSquared(target.Position);
   if(Marks(target)>0){
    if(comboStep<2&&TryCast(LunarPower.Step,target,true))return;
    if(TryCast(LunarPower.FallingMoon,target.Position,true))return;
   }
   if(TryCast(LunarPower.Crescent,target,true))return;
   if(target.CurJobDef==JobDefOf.AttackMelee&&target.CurJob.targetA.Pawn==Pawn&&TryCast(LunarPower.Parry,target,true))return;
   if(distance<=81&&target.equipment!=null&&target.equipment.Primary!=null&&target.equipment.Primary.def.IsRangedWeapon)
    TryCast(LunarPower.Veil,Pawn,true);
  }

  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn)||!Pawn.Drafted)yield break;
   yield return new Command_Action{
    defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),
    defaultDesc="Cycle On / Off / Non-lethal. Lunar builds Moonbrands, closes in with Moonfall Step, then spends the marks on Falling Moon.",
    icon=ElementalIcons.AutoCombat, groupable=false,
    action=()=>{var next=ElementalAutoModes.Next(Mode);autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;nextAuto=0;}
   };
   for(int i=0;i<Names.Length;i++){
    var power=(LunarPower)i;
    Command command;
    if(power==LunarPower.Veil)command=new Command_Action{action=()=>TryCast(power,Pawn,false)};
    else command=new Command_Target{
     targetingParams=new TargetingParameters{canTargetPawns=power!=LunarPower.FallingMoon,canTargetLocations=power==LunarPower.FallingMoon,
      canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,neverTargetIncapacitated=true,
      validator=t=>Valid(power,t.HasThing?new LocalTargetInfo(t.Thing):new LocalTargetInfo(t.Cell))},
     action=t=>TryCast(power,t,false),onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,Ranges[(int)power]);}
    };
    command.defaultLabel=Names[i];command.defaultDesc=Descriptions[i]+" Cooldown: "+Cooldowns[i]/60f+"s.";
    command.icon=LunarVfx.Icon(i);command.groupable=false;
    if(!CanAct)command.Disable("Requires a conscious, drafted and transformed Lunar Ascendant with a melee weapon.");
    else if(LunarAscendant.Busy(Pawn))command.Disable("Finish the current Lunar technique.");
    else if(Remaining(power)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(power)/60f)+"s");
    yield return command;
   }
  }
 }
}
