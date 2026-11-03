using System.Collections.Generic;using RimWorld;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class ElementalTeachingSession {
  public const string Teaching="EH_TeachElemental",Receiving="EH_ReceiveElementalTeaching";
  static bool JobIs(Pawn p,string name){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==name;}
  public static bool Busy(Pawn p){return JobIs(p,Teaching)||JobIs(p,Receiving)||ElementalBestow.Busy(p)||DiscipleAscension.Busy(p);}
  public static bool Available(Pawn teacher,Pawn pupil){
   if(teacher==null||pupil==null||EarthCombat.Busy(teacher)||EarthCombat.Busy(pupil)||LunarAscendant.Busy(teacher)||LunarAscendant.Busy(pupil)||DiscipleAscension.Busy(teacher)||DiscipleAscension.Busy(pupil)||ElementalBestow.Busy(teacher)||ElementalBestow.Busy(pupil)||teacher.jobs==null||pupil.jobs==null||ElementalAnimationBridge.Controlled(teacher)||ElementalAnimationBridge.Controlled(pupil)||AzureEvolution.Evolving(teacher)||AzureEvolution.Evolving(pupil)||HediffComp_ElementalTransform.Transforming(teacher)||HediffComp_ElementalTransform.Transforming(pupil))return false;
   return !JobIs(teacher,Receiving)&&!JobIs(pupil,Teaching)&&(!JobIs(pupil,Receiving)||pupil.CurJob.targetA.Pawn==teacher);
  }
  public static bool Paired(Pawn teacher,Pawn pupil){
   if(!ElementalTeaching.Ready(teacher)||!ElementalTeaching.Ready(pupil)||teacher.Map!=pupil.Map)return false;
   return TeachingPairRules.Valid(JobIs(teacher,Teaching),JobIs(pupil,Receiving),teacher.CurJob!=null&&teacher.CurJob.targetA.Pawn==pupil,pupil.CurJob!=null&&pupil.CurJob.targetA.Pawn==teacher,teacher.Position.DistanceToSquared(pupil.Position));
  }
  public static void Begin(Pawn teacher,Pawn pupil){
   if(!ElementalTeaching.Valid(teacher,pupil)||teacher.Position.DistanceToSquared(pupil.Position)>2)return;
   if(!JobIs(pupil,Receiving)||pupil.CurJob.targetA.Pawn!=teacher){var receive=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Receiving),teacher);receive.playerForced=true;pupil.jobs.StartJob(receive,JobCondition.InterruptForced,resumeCurJobAfterwards:true,cancelBusyStances:true,tag:JobTag.Misc,keepCarryingThingOverride:true);}
   FaceAndStop(teacher,pupil);
  }
  public static void FaceAndStop(Pawn teacher,Pawn pupil){if(teacher==null||pupil==null)return;teacher.pather.StopDead();pupil.pather.StopDead();teacher.rotationTracker.FaceTarget(pupil);pupil.rotationTracker.FaceTarget(teacher);}
  public static void Release(Pawn teacher,Pawn pupil){if(JobIs(pupil,Receiving)&&pupil.CurJob.targetA.Pawn==teacher)pupil.jobs.EndCurrentJob(JobCondition.InterruptForced,true);}
 }
 public static class TeachingPairRules {
  public static bool Valid(bool teaching,bool receiving,bool teacherTargetsPupil,bool pupilTargetsTeacher,int distanceSquared){return teaching&&receiving&&teacherTargetsPupil&&pupilTargetsTeacher&&distanceSquared<=2;}
 }
 public sealed class JobDriver_ReceiveElementalTeaching:JobDriver {
  Pawn Teacher{get{return job.targetA.Pawn;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFailCondition(()=>!ElementalTeachingSession.Paired(Teacher,pawn));
   var wait=new Toil();wait.defaultCompleteMode=ToilCompleteMode.Never;wait.handlingFacing=true;wait.initAction=()=>ElementalTeachingSession.FaceAndStop(Teacher,pawn);wait.tickAction=()=>ElementalTeachingSession.FaceAndStop(Teacher,pawn);yield return wait;
  }
 }
}
