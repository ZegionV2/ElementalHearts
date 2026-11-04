using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public enum EclipsePower {DuskCrescent,UmbralPursuit,CrimsonWaltz,RivenHorizon,NightfallReversal,SovereignEclipse}
 public static class EclipseRules {
  public static readonly string[] Art={"DuskCrescent","UmbralPursuit","CrimsonWaltz","RivenHorizon","NightfallReversal","SovereignEclipse","EvolveEclipse"};
  public static readonly string[] Names={"Dusk Crescent","Umbral Pursuit","Crimson Waltz","Riven Horizon","Nightfall Reversal","Sovereign Eclipse"};
  public static readonly int[] Cooldowns={240,300,480,840,1080,1800};
  public static readonly float[] Ranges={12,10,4.5f,12,4,14};
  public static readonly string[] Descriptions={
   "Release a twelve-cell black sword crescent edged in crimson. Stops at walls and strikes each visible hostile once, applying an Eclipse Brand.",
   "Flash through a checked route up to twelve cells long to the enemy's rear, then backstab once. Prefer the rear, with a clear flank as fallback. Cannot cross walls, occupied cells or blocked corners.",
   "Spin Eventide through a 4.5-cell circle in a rotating black-and-crimson wave. One strike per visible enemy across the full animation, applying Eclipse Brands.",
   "Mark fixed ground within twelve cells. After a 0.35-second warning, crossing waves rupture a 3.8-cell circle. One shared impact per enemy; consume your brands for +15% damage each, up to three.",
   "When two or more enemies pressure you within four cells, leap safely away, then rush back with shadow afterimages. Each enemy crossed receives one powerful cut. Automatic with Auto Combat.",
   "Gather darkness for 0.45 seconds, then release a heavy fourteen-cell finishing wave down a broad corridor. Walls stop the wave; up to sixteen enemies are each struck once. Consume your own brands for +15% damage per stack."};
  public const int VictimLimit=16;
 }
 public sealed class HediffCompProperties_EclipseAbilities:HediffCompProperties {public HediffCompProperties_EclipseAbilities(){compClass=typeof(HediffComp_EclipseAbilities);}}
 public sealed class HediffComp_EclipseAbilities:HediffComp {
  public bool autoCombat=true,nonLethal;
  List<int> readyAt=new List<int>{0,0,0,0,0,0};
  List<LunarMark> marks=new List<LunarMark>();
  int nextAuto,comboStep,comboUntil,lastAdvance=-1;bool refreshNeeded=true;
  public int combatUntil,nextSkyRoll,nextSummonAt;
  Pawn comboTarget;
  static int Now{get{return Find.TickManager.TicksGame;}}
  public static HediffComp_EclipseAbilities On(Pawn p){if(p!=null&&p.health!=null)foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName==EclipseAscendant.DefName)return h.TryGetComp<HediffComp_EclipseAbilities>();return null;}
  public ElementalAutoMode Mode{get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  internal bool CanAct{get{return ElementalTeaching.Ready(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&HediffComp_ElementalTransform.Active(Pawn)&&!HediffComp_ElementalTransform.Transforming(Pawn)&&!AzureEvolution.Evolving(Pawn)&&!ElementalTeachingSession.Busy(Pawn)&&!ElementalBestow.Busy(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon&&On(Pawn)==this;}}
  internal void Inherit(bool auto,bool nl,List<int> deadlines,List<LunarMark> savedMarks){autoCombat=auto;nonLethal=nl;readyAt=deadlines??new List<int>();while(readyAt.Count<6)readyAt.Add(0);marks=savedMarks??new List<LunarMark>();Prune();}
  // Health.Notify_Spawned enumerates the hediff list; defer additions until the next owned tick.
  void Register(){refreshNeeded=true;if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_EclipseCombat>().Register(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){Register();}
  public override void Notify_Spawned(){Register();}
  public override void CompPostPostRemoved(){if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_EclipseCombat>().Forget(Pawn);EclipseSky.RemoveBoost(Pawn);}
  public override void CompExposeData(){
   Scribe_Values.Look(ref autoCombat,"EH_eclipseAuto",true);Scribe_Values.Look(ref nonLethal,"EH_eclipseNonLethal");
   Scribe_Collections.Look(ref readyAt,"EH_eclipseCooldowns",LookMode.Value);Scribe_Collections.Look(ref marks,"EH_eclipseBrands",LookMode.Deep);
   Scribe_Values.Look(ref comboStep,"EH_eclipseComboStep");Scribe_Values.Look(ref comboUntil,"EH_eclipseComboUntil");Scribe_References.Look(ref comboTarget,"EH_eclipseComboTarget");
   Scribe_Values.Look(ref combatUntil,"EH_eclipseCombatUntil");Scribe_Values.Look(ref nextSkyRoll,"EH_eclipseNextSkyRoll");Scribe_Values.Look(ref nextSummonAt,"EH_eclipseNextSummonAt");
   if(Scribe.mode==LoadSaveMode.PostLoadInit){if(readyAt==null)readyAt=new List<int>();while(readyAt.Count<6)readyAt.Add(0);if(readyAt.Count>6)readyAt.RemoveRange(6,readyAt.Count-6);if(marks==null)marks=new List<LunarMark>();}
  }
  void Prune(){marks.RemoveAll(m=>m==null||m.target==null||m.target.Destroyed||m.target.Dead||m.expires<=Now);while(marks.Count>24)marks.RemoveAt(0);}
  public int Marks(Pawn target){foreach(var m in marks)if(m!=null&&m.target==target&&m.expires>Now)return Math.Max(0,Math.Min(3,m.stacks));return 0;}
  internal void Mark(Pawn target){if(!CelestialCombat.Hostile(Pawn,target))return;Prune();foreach(var m in marks)if(m.target==target){m.stacks=Math.Min(3,m.stacks+1);m.expires=Now+480;return;}if(marks.Count>=24)marks.RemoveAt(0);marks.Add(new LunarMark{target=target,stacks=1,expires=Now+480});}
  internal int Consume(Pawn target){int n=Marks(target);marks.RemoveAll(m=>m!=null&&m.target==target);return n;}
  internal IEnumerable<LunarMark> VisualMarks{get{return marks;}}
  public int Remaining(EclipsePower power){int i=(int)power;return i<0||i>=6?int.MaxValue:Math.Max(0,readyAt[i]-Now);}
  bool Valid(EclipsePower power,LocalTargetInfo target){
   if(power==EclipsePower.CrimsonWaltz)return true;
   if(power==EclipsePower.NightfallReversal){Pawn enemy;IntVec3 leap,end;return EclipseMovement.PlanReversal(Pawn,out enemy,out leap,out end);}
   var cell=target.Cell;float range=EclipseRules.Ranges[(int)power];
   if(!cell.InBounds(Pawn.Map)||cell.Fogged(Pawn.Map)||Pawn.Position.DistanceToSquared(cell)>range*range||!LightningCombat.ClearLine(Pawn.Position,cell,Pawn.Map))return false;
   if(power!=EclipsePower.UmbralPursuit)return LightningCombat.Open(cell,Pawn.Map);
   IntVec3 waypoint,landing;return CelestialCombat.Hostile(Pawn,target.Pawn)&&EclipseMovement.PlanPursuit(Pawn,target.Pawn,out waypoint,out landing);
  }
  public bool TryCast(EclipsePower power,LocalTargetInfo target,bool automatic){
   if((int)power<0||(int)power>=6||!CanAct||EclipseAscendant.Busy(Pawn)||Remaining(power)>0)return false;
   if(automatic&&(!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat)||Pawn.jobs.jobQueue.Count>0))return false;
   Job job;
   if(power==EclipsePower.NightfallReversal){Pawn enemy;IntVec3 leap,end;if(!EclipseMovement.PlanReversal(Pawn,out enemy,out leap,out end))return false;job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(EclipseAscendant.Technique),enemy,leap,end);}
   else if(power==EclipsePower.UmbralPursuit){IntVec3 waypoint,landing;if(!CelestialCombat.Hostile(Pawn,target.Pawn)||!EclipseMovement.PlanPursuit(Pawn,target.Pawn,out waypoint,out landing))return false;job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(EclipseAscendant.Technique),target,waypoint,landing);}
   else {if(!Valid(power,target))return false;if(power==EclipsePower.CrimsonWaltz)target=Pawn;if(power==EclipsePower.RivenHorizon)target=target.Cell;job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(EclipseAscendant.Technique),target);}
   job.count=(int)power;job.playerForced=!automatic;
   if(automatic){Pawn.stances.CancelBusyStanceHard();Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true,cancelBusyStances:true);}
   else if(!Pawn.jobs.TryTakeOrderedJob(job,JobTag.Misc))return false;
   Pawn.stances.CancelBusyStanceHard();if(Pawn.CurJob!=job)return false;
   readyAt[(int)power]=Now+EclipseRules.Cooldowns[(int)power];combatUntil=Now+600;return true;
  }
  internal void AdvanceCombo(Pawn target,int step){if(!CelestialCombat.Hostile(Pawn,target))return;comboTarget=target;comboStep=step%4;comboUntil=Now+480;}
  internal void Released(){nextAuto=Now+2;}
  public override void CompPostTickInterval(ref float adjustment,int delta){Advance(Now);}
  internal void Advance(int now){
   if(now==lastAdvance)return;lastAdvance=now;if(!Pawn.Spawned){EclipseSky.RemoveBoost(Pawn);return;}
   if(refreshNeeded||(now+Pawn.thingIDNumber)%15==0){refreshNeeded=false;EclipseSky.Refresh(Pawn);if(Pawn.Drafted&&HediffComp_ElementalTransform.Active(Pawn))EclipseVfx.Track(Pawn);}
   if(now>=nextSkyRoll){nextSkyRoll=now+600;if(now>=nextSummonAt&&combatUntil>now&&CanAct&&!EclipseSky.Empowered(Pawn)&&EclipseMovement.Pressured(Pawn,5))Pawn.Map.GetComponent<MapComponent_EclipseSky>().TrySummon(Pawn,false);}
   if(now<nextAuto)return;nextAuto=now+30;if(marks.Count>0)Prune();
   if(!CanAct||EclipseAscendant.Busy(Pawn)||Pawn.CurJob==null||Pawn.jobs.jobQueue.Count>0||!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat))return;
   var job=Pawn.CurJob;if(ElementalCombatPolicy.Controlled(Pawn)&&job.def!=JobDefOf.AttackMelee&&job.def!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,job))return;
   if(Pawn.stances.FullBodyBusy&&!(Pawn.stances.curStance is Stance_Cooldown))return;
   Pawn target=job.targetA.Pawn;if(!CelestialCombat.Hostile(Pawn,target)&&comboUntil>now&&CelestialCombat.Hostile(Pawn,comboTarget))target=comboTarget;
   if(!CelestialCombat.Hostile(Pawn,target))target=CelestialCombat.Nearest(Pawn,14);if(target==null)return;combatUntil=now+600;nextAuto=now+6;
   if(Remaining(EclipsePower.NightfallReversal)==0&&EclipseMovement.Pressured(Pawn,4)&&TryCast(EclipsePower.NightfallReversal,Pawn,true))return;
   if(Remaining(EclipsePower.SovereignEclipse)==0&&Marks(target)>=2&&TryCast(EclipsePower.SovereignEclipse,target,true))return;
   int start=target==comboTarget&&comboUntil>now?comboStep:0;
   for(int offset=0;offset<4;offset++){var power=(EclipsePower)((start+offset)%4);if(power==EclipsePower.CrimsonWaltz&&!LunarMovement.NearbyEnemy(Pawn,4.5f))continue;if(TryCast(power,power==EclipsePower.RivenHorizon?new LocalTargetInfo(target.Position):new LocalTargetInfo(target),true))return;}
  }
  public override string CompTipStringExtra{get{return (EclipseSky.Empowered(Pawn)?"Eclipse Dominion active: core combat stats match Celestial's tier. ":"Eclipse Dominion awakens during an actual eclipse. ")+"Passive: 15% eclipse chance per ten-second combat check with two visible enemies within five cells. Six in-game hours; shared one-day map cooldown. Each caster owns up to three Eclipse Brands per enemy.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   yield return ElementalBestow.Command(Pawn);
   if(!Pawn.Drafted)yield break;
   yield return new Command_Action{defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),defaultDesc="Cycle On / Off / Non-lethal. Flow through the first four forms, answer two close enemies with Nightfall Reversal, and use Sovereign Eclipse on branded targets. The eclipse summon is a separate combat passive.",icon=ElementalIcons.AutoCombat,groupable=false,action=()=>{var mode=ElementalAutoModes.Next(Mode);autoCombat=mode!=ElementalAutoMode.Off;nonLethal=mode==ElementalAutoMode.NonLethal;nextAuto=0;}};
   for(int i=0;i<6;i++){
    var power=(EclipsePower)i;Command command;
    if(power==EclipsePower.CrimsonWaltz||power==EclipsePower.NightfallReversal)command=new Command_Action{action=()=>TryCast(power,Pawn,false)};
    else command=new Command_Target{targetingParams=new TargetingParameters{canTargetPawns=power!=EclipsePower.RivenHorizon,canTargetLocations=power!=EclipsePower.UmbralPursuit,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,neverTargetIncapacitated=true,validator=t=>Valid(power,t.HasThing?new LocalTargetInfo(t.Thing):new LocalTargetInfo(t.Cell))},action=t=>TryCast(power,t,false),onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,EclipseRules.Ranges[(int)power]);}};
    command.defaultLabel=EclipseRules.Names[i];command.defaultDesc=EclipseRules.Descriptions[i]+" No resource cost. Cooldown: "+EclipseRules.Cooldowns[i]/60f+"s.";command.icon=EclipseVfx.Icon(i);command.groupable=false;
    if(!CanAct)command.Disable("Requires a conscious, drafted and transformed Eclipse Ascendant wielding a melee weapon.");else if(EclipseAscendant.Busy(Pawn))command.Disable("Finish the current Eclipse technique.");else if(Remaining(power)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(power)/60f)+"s");else if(power==EclipsePower.NightfallReversal&&!EclipseMovement.Pressured(Pawn,4))command.Disable("Requires two nearby visible hostiles.");
    yield return command;
   }
  }
 }
}
