using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class StormBlitzRules {
  public const int MaxSecondary=3;
  public static int Duration(float distance){return Math.Max(6,Math.Min(14,(int)Math.Ceiling(distance*1.2f)+4));}
  public static float Progress(int elapsed,int duration){return Math.Max(0,Math.Min(1,elapsed/(float)Math.Max(1,duration)));}
  public static float LaneDistanceSquared(float px,float pz,float ax,float az,float bx,float bz){return AzureReversalRules.LaneDistanceSquared(px,pz,ax,az,bx,bz);}
  public static bool CanGraze(int already,bool primary,bool eligible,bool duplicate){return already<MaxSecondary&&!primary&&eligible&&!duplicate;}
 }
 public static class ThunderstormBlitz {
  public const string JobName="EH_ThunderstormBlitz";
  public static bool Busy(Pawn p){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==JobName;}
  public static bool Impacting(Pawn p){var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_ThunderstormBlitz;return d!=null&&d.impacting;}
  public static bool Landing(Pawn p,IntVec3 cell){if(!cell.InBounds(p.Map)||!cell.Standable(p.Map)||cell.Fogged(p.Map))return false;foreach(var thing in cell.GetThingList(p.Map))if(thing is Pawn&&thing!=p)return false;return true;}
  public static bool Path(Pawn p,IntVec3 start,IntVec3 end){foreach(var cell in LightningGeometry.Trace(start,end))if(!cell.InBounds(p.Map)||!cell.Standable(p.Map)||!LightningCombat.Open(cell,p.Map))return false;return true;}
  public static bool ChooseLanding(IntVec3 start,IntVec3 target,float range,Func<IntVec3,bool> free,Func<IntVec3,IntVec3,bool> path,out IntVec3 end){
   end=IntVec3.Invalid;float best=float.MinValue;var forward=(target-start).ToVector3();forward.Normalize();
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){if(x==0&&z==0)continue;var cell=target+new IntVec3(x,0,z);if(cell.DistanceToSquared(start)>range*range||!free(cell)||!path(start,cell)||!path(cell,target))continue;float score=x*forward.x+z*forward.z-cell.DistanceToSquared(start)*.005f;if(score>best){best=score;end=cell;}}
   return end.IsValid;
  }
  public static bool TryStart(Pawn p,LocalTargetInfo target,bool finisher,bool automatic){
   var c=HediffComp_LightningAbilities.On(p);if(c==null||!c.CanAct||!ThunderstormEvolution.Active(p)||Busy(p)||!target.HasThing||!LightningCombat.Eligible(p,target.Pawn))return false;
   IntVec3 end;if(!ChooseLanding(p.Position,target.Cell,finisher?c.Props.heavenRange:c.Props.flashRange,cell=>Landing(p,cell),(a,b)=>Path(p,a,b),out end))return false;
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(JobName),target,end);job.count=finisher?1:0;job.playerForced=!automatic;
   p.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);return p.CurJob==job;
  }
  public static void Move(Pawn p,IntVec3 cell){if(p.Position==cell)return;p.pather.StopDead();p.Position=cell;p.Notify_Teleported(false,true);}
 }
 public sealed class JobDriver_ThunderstormBlitz:JobDriver {
  IntVec3 start;int elapsed,duration;bool initialized,committed,moving;HashSet<int> struck=new HashSet<int>();public bool impacting;
  bool Finisher{get{return job.count==1;}}
  Pawn Target{get{return job.targetA.Pawn;}}
  public bool Moving{get{return moving;}}
  public int Elapsed{get{return elapsed;}}
  public Vector3 VisualAt(int ticks){return Vector3.Lerp(start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),StormBlitzRules.Progress(ticks,duration));}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref start,"EH_blitzStart");Scribe_Values.Look(ref elapsed,"EH_blitzElapsed");Scribe_Values.Look(ref duration,"EH_blitzDuration");Scribe_Values.Look(ref initialized,"EH_blitzInitialized");Scribe_Values.Look(ref committed,"EH_blitzCommitted");Scribe_Values.Look(ref moving,"EH_blitzMoving");Scribe_Collections.Look(ref struck,"EH_blitzStruck",LookMode.Value);if(struck==null)struck=new HashSet<int>();}
  bool Valid(){return pawn.Spawned&&!pawn.Dead&&!pawn.Downed&&pawn.Awake()&&!pawn.WorkTagIsDisabled(WorkTags.Violent)&&!pawn.InMentalState&&!pawn.stances.stunner.Stunned&&(!ElementalCombatPolicy.Controlled(pawn)||pawn.Drafted)&&ThunderstormEvolution.Active(pawn)&&!ElementalAnimationBridge.ExternallyControlled(pawn)&&Target!=null&&Target.Spawned&&Target.Map==pawn.Map&&!Target.Dead&&!Target.Downed;}
  void FaceTarget(){if(Target!=null)pawn.Rotation=Rot4.FromAngleFlat((Target.Position-pawn.Position).ToVector3().AngleFlat());}
  bool Adjacent(){return Target!=null&&pawn.Position.DistanceToSquared(Target.Position)<=2&&LightningCombat.ClearLine(pawn.Position,Target.Position,pawn.Map);}
  void Settle(){moving=false;if(!pawn.Spawned||pawn.Dead||pawn.Downed||ElementalAnimationBridge.ExternallyControlled(pawn))return;if(ThunderstormBlitz.Landing(pawn,pawn.Position))return;foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,1.5f,false))if(ThunderstormBlitz.Landing(pawn,cell)&&ThunderstormBlitz.Path(pawn,pawn.Position,cell)){ThunderstormBlitz.Move(pawn,cell);return;}if(initialized&&ThunderstormBlitz.Landing(pawn,start)&&ThunderstormBlitz.Path(pawn,pawn.Position,start))ThunderstormBlitz.Move(pawn,start);}
  void Graze(Vector3 from,Vector3 to){
   if(Finisher||struck.Count>=StormBlitzRules.MaxSecondary)return;var candidates=LightningCombat.Targets(pawn,pawn.Position,1.8f);
   foreach(var other in candidates){bool eligible=other.HostileTo(pawn)&&!other.IsPrisoner&&!other.Dead&&!other.Downed;
    if(!StormBlitzRules.CanGraze(struck.Count,other==Target,eligible,struck.Contains(other.thingIDNumber)))continue;var at=other.Position.ToVector3Shifted();
    if(StormBlitzRules.LaneDistanceSquared(at.x,at.z,from.x,from.z,to.x,to.z)>1.15f*1.15f)continue;struck.Add(other.thingIDNumber);
    LightningCombat.Strike(pawn,other,LightningCombat.Profile(pawn,10),.4f,.4f,true);StormBlitzFx.Impact(pawn,other.Position.ToVector3Shifted());if(!Valid())return;
   }
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>{impacting=false;Settle();if(Finisher&&!committed&&condition==JobCondition.Incompletable&&Target!=null&&(Target.Dead||Target.Downed)){var c=HediffComp_LightningAbilities.On(pawn);if(c!=null)c.RefundHeaven();}});
   this.FailOn(()=>!Valid());
   var rush=new Toil();rush.defaultCompleteMode=ToilCompleteMode.Never;rush.handlingFacing=true;
   rush.initAction=()=>{if(!initialized){start=pawn.Position;duration=StormBlitzRules.Duration((job.targetB.Cell-start).LengthHorizontal);elapsed=0;initialized=true;ElementalAudio.Ability(pawn,ElementalSound.Dash);}moving=true;pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();pawn.Rotation=Rot4.FromAngleFlat((job.targetB.Cell-start).ToVector3().AngleFlat());StormBlitzFx.Launch(pawn,pawn.Position.ToVector3Shifted());};
   rush.tickAction=()=>{
    pawn.pather.StopDead();if(!ThunderstormBlitz.Landing(pawn,job.targetB.Cell)){EndJobWith(JobCondition.Incompletable);return;}
    var from=VisualAt(elapsed);int next=Math.Min(duration,elapsed+1);var to=VisualAt(next);var cell=to.ToIntVec3();
    if(!ThunderstormBlitz.Path(pawn,pawn.Position,cell)){EndJobWith(JobCondition.Incompletable);return;}
    if(elapsed%2==0)StormBlitzFx.Echo(pawn,from);StormBlitzFx.Streak(pawn,from,to);ThunderstormBlitz.Move(pawn,cell);elapsed=next;Graze(from,to);
    if(!Valid()){EndJobWith(JobCondition.Incompletable);return;}if(elapsed>=duration){moving=false;ThunderstormBlitz.Move(pawn,job.targetB.Cell);StormBlitzFx.Arrive(pawn,pawn.Position.ToVector3Shifted());ReadyForNextToil();}
   };yield return rush;
   if(Finisher){var warning=Toils_General.Wait(18);warning.handlingFacing=true;warning.initAction=()=>{if(!Adjacent()){EndJobWith(JobCondition.Incompletable);return;}ElementalAudio.Ability(pawn,ElementalSound.Charge);ThunderstormVfx.Emit(pawn,pawn.Map,4,Target.Position.ToVector3Shifted(),Target.Position.ToVector3Shifted(),18,3.6f);};warning.tickAction=()=>{pawn.pather.StopDead();FaceTarget();if(!Adjacent())EndJobWith(JobCondition.Incompletable);};yield return warning;}
   yield return Toils_General.Do(()=>{
    if(committed||!Adjacent()){EndJobWith(JobCondition.Incompletable);return;}committed=true;FaceTarget();impacting=true;if(!Finisher)ElementalAudio.Ability(pawn,ElementalSound.Slash);
    try{using(new ElementalManualTarget(pawn,job.playerForced?Target:null)){var c=HediffComp_LightningAbilities.On(pawn);if(c==null)return;if(Finisher)c.HeavenImpact(Target);else if(LightningCombat.Strike(pawn,Target,LightningCombat.Profile(pawn,c.Props.fistFloor),c.Props.flashPhysical,c.Props.flashElectrical,true)){LightningCharge.Apply(pawn,Target);ThunderstormEvolution.Slow(Target);}StormBlitzFx.Impact(pawn,Target.Position.ToVector3Shifted());}}
    finally{impacting=false;}
   });
   var recovery=Toils_General.Wait(6);recovery.handlingFacing=true;yield return recovery;
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class ThunderstormBlitzDrawPosition {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(!ThunderstormBlitz.Busy(___pawn))return;var d=___pawn.jobs.curDriver as JobDriver_ThunderstormBlitz;if(d==null||!d.Moving)return;var pos=d.VisualAt(d.Elapsed);pos.y=__result.y;__result=pos;}
 }
}
