using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public static class AzureReversalRules {
  public const int RequiredEnemies=2,Cooldown=900,LeapTicks=24,RushTicks=18;
  public const float ThreatRadius=3f,LeapRange=5f,RushRange=12f,LaneWidth=1.15f;
  public static bool Surrounded(int enemies){return enemies>=RequiredEnemies;}
  public static float Progress(int tick,int start,int duration){return Math.Max(0f,Math.Min(1f,(tick-start)/(float)Math.Max(1,duration)));}
  public static float LaneDistanceSquared(float px,float pz,float ax,float az,float bx,float bz){float dx=bx-ax,dz=bz-az,len=dx*dx+dz*dz;float t=len<=0?0:Math.Max(0,Math.Min(1,((px-ax)*dx+(pz-az)*dz)/len));float x=px-(ax+t*dx),z=pz-(az+t*dz);return x*x+z*z;}
 }
 public static class AzureReversal {
  public const string JobName="EH_AzureReversal";
  public static bool Busy(Pawn p){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==JobName;}
  public static bool Conscious(Pawn p){return p!=null&&p.Spawned&&!p.Dead&&!p.Downed&&p.Awake()&&!p.InMentalState&&(!ElementalCombatPolicy.Controlled(p)||p.Drafted)&&!p.WorkTagIsDisabled(WorkTags.Violent)&&!p.stances.stunner.Stunned&&AzureEvolution.Active(p);}
  static bool Hostile(Pawn p,Pawn other){return other!=null&&other!=p&&other.Spawned&&other.Map==p.Map&&!other.Dead&&!other.Downed&&!other.IsPrisoner&&other.HostileTo(p);}
  public static bool Landing(Pawn p,IntVec3 cell){if(!cell.InBounds(p.Map)||!cell.Standable(p.Map)||cell.Fogged(p.Map))return false;foreach(var thing in cell.GetThingList(p.Map))if(thing is Pawn&&thing!=p)return false;return true;}
  public static bool Path(Pawn p,IntVec3 from,IntVec3 to,bool rush){foreach(var cell in FlameGeometry.Trace(from,to))if(!FlameCombat.Open(cell,p.Map)||(rush&&!cell.Standable(p.Map)))return false;return true;}
  static List<Pawn> Threats(Pawn p){var list=new List<Pawn>();foreach(var cell in GenRadial.RadialCellsAround(p.Position,AzureReversalRules.ThreatRadius,true)){if(!cell.InBounds(p.Map))continue;foreach(var thing in cell.GetThingList(p.Map)){var other=thing as Pawn;if(Hostile(p,other)&&FlameCombat.ClearLine(p.Position,other.Position,p.Map))list.Add(other);}}return list;}
  public static bool Crowded(Pawn p){return ElementalCombatFlow.CountHostiles(p,AzureReversalRules.ThreatRadius,AzureReversalRules.RequiredEnemies)>=AzureReversalRules.RequiredEnemies;}
  public static bool TryPlan(Pawn p,out IntVec3 leap,out IntVec3 end){
   var cells=new List<IntVec3>();foreach(var enemy in Threats(p))cells.Add(enemy.Position);
   return Plan(p.Position,cells,GenRadial.RadialCellsAround(p.Position,AzureReversalRules.LeapRange,false),c=>Landing(p,c),(a,b,rush)=>Path(p,a,b,rush),out leap,out end);
  }
  public static bool Plan(IntVec3 origin,IList<IntVec3> threats,IEnumerable<IntVec3> candidates,Func<IntVec3,bool> landing,Func<IntVec3,IntVec3,bool,bool> path,out IntVec3 leap,out IntVec3 end){
   leap=end=IntVec3.Invalid;if(!AzureReversalRules.Surrounded(threats.Count))return false;
   float best=float.MinValue;
   foreach(var candidate in candidates){
    int distance=candidate.DistanceToSquared(origin);if(distance<9||distance>AzureReversalRules.LeapRange*AzureReversalRules.LeapRange||!landing(candidate)||!path(origin,candidate,false))continue;
    float clearance=float.MaxValue;foreach(var enemy in threats)clearance=Math.Min(clearance,candidate.DistanceToSquared(enemy));
    if(clearance<6.25f)continue;
    foreach(var enemy in threats){
     var direction=(enemy-candidate).ToVector3();direction.Normalize();var finish=(enemy.ToVector3Shifted()+direction*2).ToIntVec3();
     if(candidate.DistanceToSquared(finish)>AzureReversalRules.RushRange*AzureReversalRules.RushRange||!landing(finish)||!path(candidate,finish,true))continue;
     int crossed=0;foreach(var target in threats)if(InLane(target,candidate,finish))crossed++;
     float score=clearance*2+crossed*6-distance*.1f;if(score>best){best=score;leap=candidate;end=finish;}
    }
   }
   return leap.IsValid&&end.IsValid;
  }
  public static bool InLane(IntVec3 cell,IntVec3 start,IntVec3 end){return AzureReversalRules.LaneDistanceSquared(cell.x,cell.z,start.x,start.z,end.x,end.z)<=AzureReversalRules.LaneWidth*AzureReversalRules.LaneWidth;}
  public static void Move(Pawn p,IntVec3 cell){p.pather.StopDead();p.Position=cell;p.Notify_Teleported(false,true);}
  public static void StrikeLane(Pawn p,IntVec3 start,IntVec3 end){
   var hit=new HashSet<Pawn>();var victims=new List<Pawn>();
   foreach(var step in FlameGeometry.Trace(start,end))foreach(var cell in GenRadial.RadialCellsAround(step,1.5f,true)){
    if(!cell.InBounds(p.Map)||!InLane(cell,start,end)||!FlameCombat.ClearLine(start,cell,p.Map))continue;
    foreach(var thing in cell.GetThingList(p.Map)){var other=thing as Pawn;if(Hostile(p,other)&&hit.Add(other))victims.Add(other);}
   }
   var profile=FlameCombat.Profile(p,10);
   foreach(var enemy in victims){if(!Conscious(p))break;if(!Hostile(p,enemy))continue;FlameCombat.Strike(p,enemy,profile,1.1f,1.3f,true);if(!enemy.Dead&&!enemy.Downed)FlameCharge.Apply(p,enemy);}
  }
 }
 public sealed class HediffCompProperties_AzureReversal:HediffCompProperties {public HediffCompProperties_AzureReversal(){compClass=typeof(HediffComp_AzureReversal);}}
 public sealed class HediffComp_AzureReversal:HediffComp {
  public int readyAt;
  public static HediffComp_AzureReversal On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_AzureReversal>();if(c!=null)return c;}return null;}
  public override void CompExposeData(){Scribe_Values.Look(ref readyAt,"EH_azureReversalReadyAt");}
  public bool TryStart(bool automatic){
   var abilities=HediffComp_FlameAbilities.On(Pawn);int now=Find.TickManager.TicksGame;
   if(abilities==null||!abilities.CanAct||AzureReversal.Busy(Pawn)||now<readyAt||Pawn.CurJob!=null&&Pawn.CurJob.def==FlameDefs.EH_PhoenixDescent||!AzureReversal.Crowded(Pawn))return false;
   IntVec3 leap,end;if(!AzureReversal.TryPlan(Pawn,out leap,out end)){if(!automatic)Messages.Message("No clear escape and return path for Azure Reversal.",MessageTypeDefOf.RejectInput,false);return false;}
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(AzureReversal.JobName),leap,end);job.playerForced=!automatic;
   Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:automatic);
   if(Pawn.CurJob!=job)return false;readyAt=now+AzureReversalRules.Cooldown;return true;
  }
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!AzureEvolution.Active(Pawn)||!ElementalCombatPolicy.Controlled(Pawn)||!Pawn.Drafted)yield break;
   var c=new Command_Action{defaultLabel="Azure Reversal",defaultDesc="When at least two hostile pawns crowd within three cells, leap up to five cells to safety, then rush back up to twelve cells through their line. Strikes each hostile once with a weapon-derived Azure attack. Does not cross walls or hit allies/prisoners. Respects non-lethal mode. Auto Combat can use this move. No resource cost; 15-second cooldown. Movement orders, undrafting, stun or downing cancel the move; a spent cooldown remains.",icon=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Azure/AzureReversal"),groupable=false,action=()=>TryStart(false)};
   var abilities=HediffComp_FlameAbilities.On(Pawn);int remaining=Math.Max(0,readyAt-Find.TickManager.TicksGame);
   if(abilities==null||!abilities.CanAct||Pawn.CurJob!=null&&Pawn.CurJob.def==FlameDefs.EH_PhoenixDescent)c.Disable("Must be conscious, drafted and free of another ability, lesson or transformation.");
   else if(remaining>0)c.Disable("Recharging: "+Math.Ceiling(remaining/60f)+"s");
   else if(!AzureReversal.Crowded(Pawn))c.Disable("Requires two hostile pawns within three cells.");yield return c;
  }
 }
 public sealed class JobDriver_AzureReversal:JobDriver {
  IntVec3 start;int phase,phaseStart;bool hit;
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref start,"EH_reversalStart");Scribe_Values.Look(ref phase,"EH_reversalPhase");Scribe_Values.Look(ref phaseStart,"EH_reversalPhaseStart");Scribe_Values.Look(ref hit,"EH_reversalHit");}
  public Vector3 VisualPosition(int tick){IntVec3 from=phase==1?start:job.targetA.Cell;IntVec3 to=phase==1?job.targetA.Cell:job.targetB.Cell;int duration=phase==1?AzureReversalRules.LeapTicks:AzureReversalRules.RushTicks;float t=AzureReversalRules.Progress(tick,phaseStart,duration);var pos=Vector3.Lerp(from.ToVector3Shifted(),to.ToVector3Shifted(),t);if(phase==1)pos.z+=(float)Math.Sin(t*Math.PI)*.65f;return pos;}
  public bool Moving {get{return phase==1||phase==2;}}
  protected override IEnumerable<Toil> MakeNewToils(){
   this.FailOn(()=>!AzureReversal.Conscious(pawn)||ElementalAnimationBridge.ExternallyControlled(pawn));
   var leap=Toils_General.Wait(AzureReversalRules.LeapTicks);leap.handlingFacing=true;
   leap.initAction=()=>{start=pawn.Position;phase=1;phaseStart=Find.TickManager.TicksGame;pawn.pather.StopDead();pawn.Rotation=Rot4.FromAngleFlat((job.targetA.Cell-start).ToVector3().AngleFlat());ReversalVfx.Emit(pawn,0,start.ToVector3Shifted(),2f,0);ElementalAudio.Ability(pawn,ElementalSound.Dash);};
   leap.tickAction=()=>{pawn.pather.StopDead();if(!AzureReversal.Landing(pawn,job.targetA.Cell)||!AzureReversal.Path(pawn,start,job.targetA.Cell,false))EndJobWith(JobCondition.Incompletable);else if(Find.TickManager.TicksGame%6==0)ReversalVfx.Emit(pawn,1,VisualPosition(Find.TickManager.TicksGame),1.7f,(job.targetA.Cell-start).ToVector3().AngleFlat());};yield return leap;
   yield return Toils_General.Do(()=>{phase=0;if(!AzureReversal.Landing(pawn,job.targetA.Cell)||!AzureReversal.Path(pawn,start,job.targetA.Cell,false)){EndJobWith(JobCondition.Incompletable);return;}AzureReversal.Move(pawn,job.targetA.Cell);ReversalVfx.Emit(pawn,2,pawn.Position.ToVector3Shifted(),2f,0);});
   var rush=Toils_General.Wait(AzureReversalRules.RushTicks);rush.handlingFacing=true;
   rush.initAction=()=>{ElementalAudio.Ability(pawn,ElementalSound.Dash);phase=2;phaseStart=Find.TickManager.TicksGame;pawn.pather.StopDead();pawn.Rotation=Rot4.FromAngleFlat((job.targetB.Cell-job.targetA.Cell).ToVector3().AngleFlat());};
   rush.tickAction=()=>{pawn.pather.StopDead();if(!AzureReversal.Landing(pawn,job.targetB.Cell)||!AzureReversal.Path(pawn,job.targetA.Cell,job.targetB.Cell,true))EndJobWith(JobCondition.Incompletable);else if(Find.TickManager.TicksGame%3==0)ReversalVfx.Emit(pawn,3,VisualPosition(Find.TickManager.TicksGame),2f,(job.targetB.Cell-job.targetA.Cell).ToVector3().AngleFlat());};yield return rush;
   yield return Toils_General.Do(()=>{phase=0;if(hit||!AzureReversal.Landing(pawn,job.targetB.Cell)||!AzureReversal.Path(pawn,job.targetA.Cell,job.targetB.Cell,true)){EndJobWith(JobCondition.Incompletable);return;}hit=true;ElementalAudio.Ability(pawn,ElementalSound.Impact);AzureReversal.Move(pawn,job.targetB.Cell);AzureReversal.StrikeLane(pawn,job.targetA.Cell,job.targetB.Cell);ReversalVfx.Emit(pawn,4,pawn.Position.ToVector3Shifted(),3f,(job.targetB.Cell-job.targetA.Cell).ToVector3().AngleFlat());ReversalVfx.Emit(pawn,5,pawn.Position.ToVector3Shifted(),2f,0);});
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class AzureReversalDrawPosition {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(!AzureReversal.Busy(___pawn))return;var driver=___pawn.jobs.curDriver as JobDriver_AzureReversal;if(driver==null||!driver.Moving)return;var visual=driver.VisualPosition(Find.TickManager.TicksGame);visual.y=__result.y;__result=visual;}
 }
}
