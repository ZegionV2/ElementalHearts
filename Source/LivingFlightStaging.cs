using System;using System.Collections.Generic;using System.Linq;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;using Verse.AI.Group;
namespace ElementalHearts {
 public static partial class LivingGroupTransport {
  public static string SiteProblem(Pawn p,Map map,IntVec3 cell){
   if(p==null||!p.Spawned||p.Map!=map)return "The carrier is no longer on this map.";
   if(!cell.InBounds(map)||cell.Fogged(map)||!cell.Standable(map)||cell.Roofed(map))return "Choose a visible, walkable cell under open sky.";
   foreach(var thing in cell.GetThingList(map))if(thing is Fire||thing is Building_Trap||thing is Building_LivingTransport||thing is Pawn&&thing!=p)return "Choose a clear launch cell.";
   if(!p.CanReach(cell,PathEndMode.OnCell,Danger.Deadly))return "The carrier cannot reach that launch site.";
   return null;
  }
  public static void PickSite(Pawn p){
   var existing=Existing(p);if(existing!=null){existing.OpenLoader();return;}string problem=CelestialLightTravel.PreparationProblem(p);if(problem!=null){Messages.Message(problem,MessageTypeDefOf.RejectInput,false);return;}
   var map=p.Map;Current.Game.CurrentMap=map;CameraJumper.TryJump(p);var parameters=TargetingParameters.ForCell();parameters.validator=t=>p.Spawned&&p.Map==map&&Find.CurrentMap==map&&t.Cell.InBounds(map);
   Func<LocalTargetInfo,bool> valid=t=>p.Spawned&&p.Map==map&&Find.CurrentMap==map&&SiteProblem(p,map,t.Cell)==null;
   var marks=new List<IntVec3>{IntVec3.Invalid};Messages.Message("Choose a clear outdoor launch site. Then choose the people and supplies to bring.",MessageTypeDefOf.NeutralEvent,false);
   Find.Targeter.BeginTargeting(parameters,t=>{if(valid(t))CreateAt(p,t.Cell,true);},t=>{if(t.Cell.InBounds(map)){marks[0]=t.Cell;GenDraw.DrawFieldEdges(marks,valid(t)?Color.green:Color.red);}},valid,p,null,LivingFlightAssets.FlyingPawn,true);
  }
 }
 public sealed partial class Building_LivingTransport {
  public bool carrierHauling,assistFinished;int nextHaulCheck;
  public bool CarrierHaulJob(Pawn p){return p==captain&&p!=null&&p.CurJob!=null&&p.CurJobDef.defName=="EH_HaulForLivingFlight"&&p.CurJob.targetB.Thing==this;}
  public void ExposeHauling(){Scribe_Values.Look(ref carrierHauling,"EH_flightCarrierHauling");Scribe_Values.Look(ref assistFinished,"EH_flightAssistFinished");}
  public void DetachCarrierLord(){var lord=captain==null?null:captain.GetLord();if(lord!=null&&lord.LordJob is LordJob_LoadAndEnterTransporters)lord.Notify_PawnLost(captain,PawnLostCondition.LeftVoluntarily);}
  public void HaulFinished(JobCondition condition){if(cancelling||committing||changingJobs||!Spawned)return;if(condition==JobCondition.Succeeded||condition==JobCondition.Incompletable)assistFinished=true;else cancelRequested=true;}
  public bool ResumeAfterHaul(){
   if(!carrierHauling)return false;if(CarrierHaulJob(captain))return true;
   if(assistFinished){carrierHauling=false;assistFinished=false;DropCarriedCargo();HoldCarrier();return true;}
   CancelPreparation("Travel preparation cancelled after the carrier received another order.",captain);return true;
  }
  public void DropCarriedCargo(){if(captain!=null&&captain.Spawned&&captain.carryTracker!=null&&captain.carryTracker.CarriedThing!=null){Thing placed;captain.carryTracker.TryDropCarriedThing(captain.Position,ThingPlaceMode.Near,out placed);}}
  bool FreeHelper(Pawn p){
   if(p==null||p==captain||Contains(p)||LivingGroupTransport.Preparing(p)||!p.Spawned||p.Map!=Map||p.Drafted||!ElementalTeaching.Ready(p)||p.WorkTagIsDisabled(WorkTags.Hauling)||p.workSettings==null||!p.workSettings.WorkIsActive(WorkTypeDefOf.Hauling))return false;
   var lord=p.GetLord();if(lord!=null&&(!(lord.LordJob is LordJob_LoadAndEnterTransporters)||((LordJob_LoadAndEnterTransporters)lord.LordJob).transportersGroup!=Transporter.groupID))return false;
   if(p.CurJob==null)return true;if(p.CurJobDef==JobDefOf.HaulToTransporter&&p.CurJob.targetB.Thing==this)return true;
   if(p.CurJob.playerForced)return false;string name=p.CurJobDef.defName;return name=="Wait"||name=="Wait_Wander"||name=="GotoWander";
  }
  public void TryAssistLoading(){
   int now=Find.TickManager.TicksGame;if(now<nextHaulCheck||!confirmed||launching||carrierHauling||!OwnsJob(captain)||!Transporter.AnythingLeftToLoad)return;nextHaulCheck=now+90;
   // Check only local colonists, at most once per 1.5 seconds. Respect work settings,
   // drafted pawns and existing orders; reservations remain the native loader's job.
   int checkedPawns=0;foreach(var helper in Map.mapPawns.FreeColonistsSpawned){if(++checkedPawns>128)break;if(!FreeHelper(helper))continue;if(helper.CurJobDef==JobDefOf.HaulToTransporter&&helper.CurJob.targetB.Thing==this)return;
    if(!LoadTransportersJobUtility.HasJobOnTransporter(helper,Transporter))continue;var work=LoadTransportersJobUtility.JobOnTransporter(helper,Transporter);if(work==null||work.def!=JobDefOf.HaulToTransporter||work.targetB.Thing!=this)continue;helper.jobs.StartJob(work,JobCondition.InterruptForced,cancelBusyStances:true);return;
   }
   if(!LoadTransportersJobUtility.HasJobOnTransporter(captain,Transporter))return;var job=LoadTransportersJobUtility.JobOnTransporter(captain,Transporter);if(job==null||job.def!=JobDefOf.HaulToTransporter||job.targetB.Thing!=this)return;
   job.def=DefDatabase<JobDef>.GetNamed("EH_HaulForLivingFlight");job.playerForced=true;carrierHauling=true;assistFinished=false;State(captain).ready=false;bool previous=changingJobs;changingJobs=true;try{captain.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);}finally{changingJobs=previous;}
  }
 }
 public sealed class JobDriver_HaulForLivingFlight:JobDriver_HaulToTransporter {
  protected override IEnumerable<Toil> MakeNewToils(){var anchor=job.targetB.Thing as Building_LivingTransport;AddFinishAction(condition=>{if(anchor!=null)anchor.HaulFinished(condition);});foreach(var toil in base.MakeNewToils())yield return toil;}
 }
 // The native loading lord also asks this utility for jobs. Selected companions
 // must gather, even while cargo remains; outside colonists and the captain keep
 // the ordinary hauling path. Do not replace an already-running carry operation.
 [HarmonyPatch(typeof(LoadTransportersJobUtility),"JobOnTransporter")]
 public static class LivingFlightPassengerGatherJob {
  static bool Prefix(Pawn __0,CompTransporter __1,ref Job __result){var b=__1==null?null:__1.parent as Building_LivingTransport;if(b==null||!b.confirmed||!b.Contains(__0)||__0==b.captain)return true;if(!b.Spawned||__0==null||!__0.Spawned||__0.Map!=b.Map||__0.Dead||__0.Downed||b.committing||b.cancelling){__result=null;return false;}__result=JobMaker.MakeJob(JobDefOf.EnterTransporter,b);return false;}
 }
 [HarmonyPatch(typeof(TransporterUtility),"AllSendableItems")]
 public static class LivingFlightReachableCargo {
  static bool Prefix(List<CompTransporter> transporters,Map map,ref IEnumerable<Thing> __result){if(transporters.Count!=1||!(transporters[0] is CompLivingTransport))return true;__result=CaravanFormingUtility.AllReachableColonyItems(map,true,transporters[0].LoadingInProgressOrReadyToLaunch,false).ToList();return false;}
 }
}
