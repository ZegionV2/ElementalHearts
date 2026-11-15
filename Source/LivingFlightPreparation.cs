using System;using System.Collections.Generic;using System.Linq;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;using Verse.AI.Group;
namespace ElementalHearts {
 // Native arrival menus inspect a holder's contents. This ephemeral reference-only
 // view exposes the still-spawned crew without boarding, despawning, or changing any
 // pawn/item's real holdingOwner. It is never registered, ticked or serialized.
 public sealed class LivingFlightPlanningContents:IThingHolder {
  readonly ThingOwner<Thing> references;
  public LivingFlightPlanningContents(Building_LivingTransport anchor){references=new ThingOwner<Thing>(this,false,LookMode.Reference);references.InnerListForReading.AddRange(anchor.crew.Where(c=>c.pawn!=null).Select(c=>(Thing)c.pawn));references.InnerListForReading.AddRange(anchor.Transporter.innerContainer.Where(t=>!(t is Pawn)));}
  public IThingHolder ParentHolder{get{return null;}}
  public ThingOwner GetDirectlyHeldThings(){return references;}
  public void GetChildHolders(List<IThingHolder> list){ThingOwnerUtility.AppendThingHoldersFromThings(list,references);}
 }
 public sealed class LivingFlightPawnState:IExposable {
  public Pawn pawn;public bool drafted,ready;public IntVec3 slot=IntVec3.Invalid;
  public static LivingFlightPawnState Capture(Pawn p){return new LivingFlightPawnState{pawn=p,drafted=p!=null&&p.Drafted};}
  public void ExposeData(){Scribe_References.Look(ref pawn,"pawn");Scribe_Values.Look(ref drafted,"drafted");Scribe_Values.Look(ref ready,"ready");Scribe_Values.Look(ref slot,"slot",IntVec3.Invalid);}
 }
 public sealed class LivingFlightManifest:IExposable {
  public Pawn captain;public bool pause,attack;public List<LivingFlightPawnState> crew=new List<LivingFlightPawnState>();
  public static LivingFlightManifest Capture(Pawn captain,IEnumerable<Pawn> pawns,TransportersArrivalAction action){return new LivingFlightManifest{captain=captain,crew=pawns.Distinct().Select(LivingFlightPawnState.Capture).ToList(),pause=Find.TickManager.CurTimeSpeed==TimeSpeed.Paused,attack=action is TransportersArrivalAction_AttackSettlement};}
  public void ExposeData(){Scribe_References.Look(ref captain,"captain");Scribe_Values.Look(ref pause,"pause");Scribe_Values.Look(ref attack,"attack");Scribe_Collections.Look(ref crew,"crew",LookMode.Deep);if(Scribe.mode==LoadSaveMode.PostLoadInit&&crew==null)crew=new List<LivingFlightPawnState>();}
 }
 public sealed partial class Building_LivingTransport {
  public int version;public bool confirmed,launching,committing,cancelling;public List<LivingFlightPawnState> crew=new List<LivingFlightPawnState>();
  public PlanetTile destination=PlanetTile.Invalid;public TransportersArrivalAction arrival;bool migrationQueued,cancelRequested;public bool changingJobs;
  public void ExposePreparation(){ExposeHauling();Scribe_Values.Look(ref version,"EH_visibleFlightVersion");Scribe_Values.Look(ref confirmed,"EH_visibleFlightConfirmed");Scribe_Values.Look(ref launching,"EH_visibleFlightLaunching");Scribe_Collections.Look(ref crew,"EH_visibleFlightCrew",LookMode.Deep);Scribe_Values.Look(ref destination,"EH_visibleFlightDestination",PlanetTile.Invalid);Scribe_Deep.Look(ref arrival,"EH_visibleFlightArrival");if(Scribe.mode==LoadSaveMode.PostLoadInit&&crew==null)crew=new List<LivingFlightPawnState>();}
  public override void SpawnSetup(Map map,bool respawningAfterLoad){base.SpawnSetup(map,respawningAfterLoad);if(respawningAfterLoad){migrationQueued=true;LongEventHandler.ExecuteWhenFinished(()=>{migrationQueued=false;if(Spawned&&version<2)MigrateLoadingPod();});}}
  public LivingFlightPawnState State(Pawn p){return crew.FirstOrDefault(c=>c.pawn==p);}
  public bool Contains(Pawn p){return State(p)!=null;}
  public bool OwnsJob(Pawn p){if(CarrierHaulJob(p))return true;return p!=null&&p.CurJob!=null&&p.CurJob.targetA.Thing==this&&(p.CurJobDef.defName=="EH_WaitForLivingFlight"||p.CurJobDef==JobDefOf.EnterTransporter);}
  public void HoldCarrier(){if(!Spawned||captain==null||!captain.Spawned)return;var state=State(captain);if(state==null){state=LivingFlightPawnState.Capture(captain);crew.Insert(0,state);}bool previous=changingJobs;changingJobs=true;try{if(captain.drafter!=null&&captain.Drafted!=state.drafted)captain.drafter.Drafted=state.drafted;
   if(!OwnsJob(captain)||captain.CurJobDef.defName!="EH_WaitForLivingFlight"){var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_WaitForLivingFlight"),this);job.playerForced=true;captain.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);}state.slot=Position;state.ready=captain.Position==Position;if(Transporter.LeftToLoadContains(captain))Transporter.SubtractFromToLoadList(captain,1,true);}finally{changingJobs=previous;}
  }
  public void SetManifest(List<LivingFlightPawnState> next){
   if(!Spawned||launching)return;if(carrierHauling){carrierHauling=false;assistFinished=false;DropCarriedCargo();}var removed=crew.Where(c=>!next.Any(n=>n.pawn==c.pawn)).ToList();confirmed=true;crew=next;
   foreach(var state in removed){var p=state.pawn;if(p==null)continue;var lord=p.GetLord();if(lord!=null&&lord.LordJob is LordJob_LoadAndEnterTransporters)lord.Notify_PawnLost(p,PawnLostCondition.LeftVoluntarily);if(OwnsJob(p))p.jobs.StopAll();if(p.drafter!=null)p.drafter.Drafted=state.drafted;}
   DetachCarrierLord();HoldCarrier();foreach(var state in crew){var p=state.pawn;if(p==captain||p==null||!p.Spawned||p.Map!=Map)continue;if(p.Downed){if(p.Position.DistanceToSquared(Position)<=16)MarkGathered(p);continue;}if(p.Faction!=Faction.OfPlayer||p.carryTracker!=null&&p.carryTracker.CarriedThing!=null)continue;
    // TryAccept can assign a native haul synchronously before the accepted crew
    // is known. Redirect an empty-handed passenger now; an actual carry already
    // in progress may finish safely before the gathering-job patch takes over.
    var gather=JobMaker.MakeJob(JobDefOf.EnterTransporter,this);gather.playerForced=true;p.jobs.StartJob(gather,JobCondition.InterruptForced,cancelBusyStances:true);
   }
  }
  public bool FindSlot(Pawn p,out IntVec3 result){
   result=IntVec3.Invalid;var state=State(p);if(p==captain){result=Position;return true;}
   if(state!=null&&state.slot.IsValid&&SlotSafe(state.slot,p)){result=state.slot;return true;}
   int visited=0;foreach(var cell in GenRadial.RadialCellsAround(Position,12,true)){if(++visited>512)break;if(!SlotSafe(cell,p))continue;if(p.Spawned&&!p.CanReach(cell,PathEndMode.OnCell,Danger.Deadly))continue;result=cell;if(state!=null)state.slot=cell;return true;}return false;
  }
  bool SlotSafe(IntVec3 cell,Pawn p){if(!cell.InBounds(Map)||!cell.Standable(Map)||cell.Roofed(Map)||cell==Position||cell.GetFirstPawn(Map)!=null&&cell.GetFirstPawn(Map)!=p)return false;return !crew.Any(c=>c.pawn!=p&&c.slot==cell);}
  public void MarkGathered(Pawn p){var state=State(p);if(state==null)return;state.ready=true;state.slot=p.Position;if(Transporter.LeftToLoadContains(p))Transporter.SubtractFromToLoadList(p,1,true);}
  public float TotalMass{get{return Transporter.MassUsage+crew.Where(c=>c.pawn!=null&&!Transporter.innerContainer.Contains(c.pawn)).Sum(c=>c.pawn.GetStatValue(StatDefOf.Mass)+MassUtility.GearAndInventoryMass(c.pawn));}}
  public string ReadyProblem(bool committingLaunch){
   if(carrierHauling)return "The carrier is collecting the selected supplies.";
   if(!confirmed)return "Confirm the people and cargo in the loading menu first.";
   if(launching&&!committingLaunch)return "The party is already taking flight.";
   if(!LivingGroupTransport.Able(captain)||!ElementalTeaching.Ready(captain)||captain.Map!=Map)return "The carrier must remain conscious at the gathering point.";
   if(captain.Position!=Position)return "The carrier is moving to the chosen launch site.";
   if(!launching&&!OwnsJob(captain))return "The carrier is no longer waiting at the gathering point.";
   if(Position.Roofed(Map))return "The carrier needs clear sky before launching.";
   foreach(var state in crew){var p=state.pawn;if(p==null||p.Dead||!p.Spawned||p.Map!=Map||!state.ready||p.Position.DistanceToSquared(Position)>169)return "Waiting for the selected companions to gather beside the carrier.";if(p!=captain&&!p.Downed&&!OwnsJob(p))return "A selected companion is no longer waiting with the party.";}
   if(Transporter.AnythingLeftToLoad)return "Waiting for colonists to bring the selected supplies or passengers.";
   if(TotalMass>Transporter.MassCapacity+.01f)return "The people and supplies exceed the carrier's current capacity.";
   return null;
  }
  public void PreparationTick(){
   if(!Spawned||migrationQueued||committing||cancelling)return;if(version<2){MigrateLoadingPod();return;}
   // Native hauling can carry prisoners, animals and downed pawns. Unpack them visibly
   // after the container operation finishes; no pawn remains boarded during preparation.
   foreach(var p in Transporter.innerContainer.OfType<Pawn>().ToList())UnpackPassenger(p);
   if(cancelRequested){CancelPreparation("Travel preparation cancelled after the carrier received another order.",captain);return;}
   if(Find.TickManager.TicksGame%15!=0)return;
   if(!LivingGroupTransport.Able(captain)||!ElementalTeaching.Ready(captain)||captain.Map!=Map){CancelPreparation("Travel preparation ended because the carrier is unavailable.");return;}
   if(crew.Any(c=>c.pawn==null||c.pawn.Dead)){CancelPreparation("Travel preparation ended because a selected companion is unavailable.");return;}
   if(ResumeAfterHaul())return;
   if(launching){if(captain.CurJobDef!=DefDatabase<JobDef>.GetNamed(IncarnateTransit.DepartJob(captain)))CancelPreparation("Departure interrupted; the party remains on the map.",captain);return;}
   if(!OwnsJob(captain)){CancelPreparation("Travel preparation cancelled after the carrier received another order.",captain);return;}TryAssistLoading();
  }
  void UnpackPassenger(Pawn p){if(!Transporter.innerContainer.Contains(p))return;var state=State(p);if(state==null){state=LivingFlightPawnState.Capture(p);crew.Add(state);}IntVec3 cell=p==captain?Position:IntVec3.Invalid;if(p!=captain&&!FindSlot(p,out cell))return;
   Thing placed;if(!Transporter.innerContainer.TryDrop(p,cell,Map,ThingPlaceMode.Direct,out placed))return;if(p==captain){HoldCarrier();return;}if(p.Downed){MarkGathered(p);return;}var job=JobMaker.MakeJob(JobDefOf.EnterTransporter,this);p.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);
  }
  public void MigrateLoadingPod(){
   if(!Spawned||version>=2)return;bool previous=changingJobs;changingJobs=true;try{version=2;
   var people=new List<Pawn>();if(captain!=null)people.Add(captain);people.AddRange(Transporter.innerContainer.OfType<Pawn>());if(Transporter.leftToLoad!=null)people.AddRange(Transporter.leftToLoad.SelectMany(t=>t.things).OfType<Pawn>());
   crew=people.Distinct().Select(LivingFlightPawnState.Capture).ToList();confirmed=Transporter.LoadingInProgressOrReadyToLaunch;Transporter.TryRemoveLord(Map);
   foreach(var p in Transporter.innerContainer.OfType<Pawn>().OrderByDescending(p=>p==captain).ToList())UnpackPassenger(p);
   if(!LivingGroupTransport.Able(captain)||!captain.Spawned){CancelPreparation(null);return;}
   if(confirmed)TransporterUtility.MakeLordsAsAppropriate(crew.Select(c=>c.pawn).Where(p=>p!=null&&p.Spawned).ToList(),new List<CompTransporter>{Transporter},Map);
   DetachCarrierLord();HoldCarrier();
   if(confirmed)foreach(var c in crew)if(c.pawn!=captain&&c.pawn!=null&&c.pawn.Spawned&&!c.pawn.Downed&&c.pawn.Faction==Faction.OfPlayer){var gather=JobMaker.MakeJob(JobDefOf.EnterTransporter,this);gather.playerForced=true;c.pawn.jobs.StartJob(gather,JobCondition.InterruptForced,cancelBusyStances:true);}
   }finally{changingJobs=previous;}
  }
  public bool CancelPreparation(string message,Pawn preserveDraftOverride=null){
   if(!Spawned||committing||cancelling)return false;cancelling=true;launching=false;confirmed=false;Transporter.TryRemoveLord(Map);
   foreach(var state in crew){var p=state.pawn;if(p==null)continue;if(p!=preserveDraftOverride&&(OwnsJob(p)||p==captain&&p.CurJobDef==DefDatabase<JobDef>.GetNamed(IncarnateTransit.DepartJob(p))))p.jobs.StopAll();if(p!=preserveDraftOverride&&p.drafter!=null&&p.Spawned&&!p.Downed&&!p.InMentalState)p.drafter.Drafted=state.drafted;}
   if(preserveDraftOverride==null&&carrierHauling)DropCarriedCargo();Transporter.CleanUpLoadingVars(Map);if(Transporter.innerContainer.Count==0)Destroy(DestroyMode.Vanish);cancelling=false;
   if(!message.NullOrEmpty())Messages.Message(message,MessageTypeDefOf.NeutralEvent,false);return Destroyed;
  }
  // Cleanup can run inside StartJob. Defer unloading until the next anchor tick so
  // stopping a waiting job cannot recursively replace the player's new order.
  public void Interrupted(Pawn p){if(Spawned&&!launching&&!committing&&!cancelling&&!changingJobs&&p==captain)cancelRequested=true;}
  public void DepartureInterrupted(){if(Spawned&&launching&&!committing&&!cancelling)cancelRequested=true;}
  public void StartDeparture(PlanetTile tile,TransportersArrivalAction action){
   if(!Spawned||ReadyProblem(false)!=null)return;destination=tile;var manifest=LivingFlightManifest.Capture(captain,crew.Select(c=>c.pawn),action);manifest.crew=crew.Select(c=>new LivingFlightPawnState{pawn=c.pawn,drafted=c.drafted}).ToList();arrival=LivingFlightArrival.Wrap(action,manifest);launching=true;IncarnateTransit.Destination(captain,tile);
   // The native loading lord expects boarding. Once ready it must release the
   // still-visible crew before the elemental departure job takes ownership.
   Transporter.TryRemoveLord(Map);
   foreach(var state in crew){var p=state.pawn;if(p==null||!p.Spawned)continue;p.pather.StopDead();p.Drawer.tweener.ResetTweenedPosToRoot();}
   foreach(var state in crew){var p=state.pawn;if(p==null||p==captain||p.Downed)continue;var gather=JobMaker.MakeJob(JobDefOf.EnterTransporter,this);gather.playerForced=true;p.jobs.StartJob(gather,JobCondition.InterruptForced,cancelBusyStances:true);}
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(IncarnateTransit.DepartJob(captain)),Position,IntVec3.Invalid);job.playerForced=true;captain.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);if(captain.CurJob!=job){launching=false;CancelPreparation(null);return;}
   Current.Game.CurrentMap=Map;CameraJumper.TryJump(captain);Find.Selector.ClearSelection();Find.Selector.Select(captain);if(Find.TickManager.CurTimeSpeed==TimeSpeed.Paused)Find.TickManager.CurTimeSpeed=TimeSpeed.Normal;
  }
  public bool CommitDeparture(){
   if(!Spawned||!launching||ReadyProblem(true)!=null)return false;var map=Map;committing=true;var info=new ActiveTransporterInfo{sentTransporterDef=def};var origins=crew.ToDictionary(c=>c.pawn,c=>c.pawn.Position);WorldObject_LivingGroupFlight flight=null;
   try{
    flight=(WorldObject_LivingGroupFlight)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed(LivingGroupTransport.FlightDef));flight.Tile=map.Tile;flight.destinationTile=destination;flight.arrivalAction=arrival;flight.SetFaction(Faction.OfPlayer);Find.WorldObjects.Add(flight);Transporter.TryRemoveLord(map);
    foreach(var state in crew){var p=state.pawn;p.jobs.StopAll();p.DeSpawn();if(!info.innerContainer.TryAdd(p))throw new InvalidOperationException("Could not board "+p);}
    info.innerContainer.TryAddRangeOrTransfer(Transporter.innerContainer,true,true);flight.AddTransporter(info,false);Transporter.CleanUpLoadingVars(map);Destroy(DestroyMode.Vanish);CameraJumper.TryJump(new GlobalTargetInfo(map.Tile));Find.WorldSelector.ClearSelection();Find.WorldSelector.Select(flight);if(Find.TickManager.CurTimeSpeed==TimeSpeed.Paused)Find.TickManager.CurTimeSpeed=TimeSpeed.Normal;return true;
   }catch(Exception e){
    foreach(var state in crew){var p=state.pawn;if(info.innerContainer.Contains(p))info.innerContainer.Remove(p);if(!p.Spawned)GenSpawn.Spawn(p,origins[p],map);}
    foreach(var thing in info.innerContainer.ToList())if(!(thing is Pawn))Transporter.innerContainer.TryAddOrTransfer(thing);
    if(flight!=null&&flight.Spawned)Find.WorldObjects.Remove(flight);committing=false;launching=false;CancelPreparation("Departure interrupted. Your party and supplies remain at the origin.");Log.Error("[Elemental Hearts] Living-flight launch rolled back: "+e);return false;
   }
  }
 }
 public sealed class JobDriver_WaitForLivingFlight:JobDriver {
  Building_LivingTransport Anchor{get{return job.targetA.Thing as Building_LivingTransport;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){var b=Anchor;return b!=null&&pawn.Reserve(b.Position,job,1,-1,null,errorOnFailed);}
  protected override IEnumerable<Toil> MakeNewToils(){var b=Anchor;AddFinishAction(c=>{if(b!=null)b.Interrupted(pawn);});this.FailOn(()=>b==null||!b.Spawned||!LivingGroupTransport.Able(pawn));yield return Toils_Goto.GotoThing(TargetIndex.A,PathEndMode.OnCell);var wait=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};wait.initAction=()=>b.MarkGathered(pawn);wait.tickAction=()=>{pawn.pather.StopDead();b.MarkGathered(pawn);};yield return wait;}
 }
 [HarmonyPatch(typeof(Dialog_LoadTransporters),"TryAccept")]
 public static class LivingFlightChooseManifest {
  static void Prefix(Dialog_LoadTransporters __instance,out List<LivingFlightPawnState> __state){__state=null;var d=__instance as Dialog_LivingTransport;if(d==null)return;var list=d.Transfers;var required=list.FirstOrDefault(t=>t.things.Contains(d.anchor.captain));if(required!=null)required.AdjustTo(1);__state=list.Where(t=>t.CountToTransfer>0).SelectMany(t=>t.things.OfType<Pawn>().Take(t.CountToTransfer)).Distinct().Select(p=>d.anchor.State(p)??LivingFlightPawnState.Capture(p)).ToList();d.anchor.changingJobs=true;}
  static void Postfix(Dialog_LoadTransporters __instance,bool __result,List<LivingFlightPawnState> __state){var d=__instance as Dialog_LivingTransport;if(d==null)return;try{if(__result&&__state!=null){d.anchor.SetManifest(__state);Find.Selector.ClearSelection();Find.Selector.Select(d.anchor.captain);}}finally{d.anchor.changingJobs=false;}}
  static Exception Finalizer(Dialog_LoadTransporters __instance,Exception __exception){var d=__instance as Dialog_LivingTransport;if(d!=null)d.anchor.changingJobs=false;return __exception;}
 }
 [HarmonyPatch(typeof(JobDriver_EnterTransporter),"MakeNewToils")]
 public static class LivingFlightGatherInsteadOfBoard {
  static bool Prefix(JobDriver_EnterTransporter __instance,ref IEnumerable<Toil> __result){var b=__instance.job.targetA.Thing as Building_LivingTransport;if(b==null)return true;__result=Gather(__instance,b);return false;}
  static IEnumerable<Toil> Gather(JobDriver_EnterTransporter driver,Building_LivingTransport b){var p=driver.pawn;driver.FailOn(()=>!b.Spawned||!b.Contains(p)||p.Dead||p.InMentalState);yield return Toils_General.Do(()=>{IntVec3 cell;if(!b.FindSlot(p,out cell)){driver.EndJobWith(JobCondition.Incompletable);return;}driver.job.SetTarget(TargetIndex.B,cell);});yield return Toils_Goto.GotoCell(TargetIndex.B,PathEndMode.OnCell);var wait=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};wait.initAction=()=>b.MarkGathered(p);wait.tickAction=()=>{b.MarkGathered(p);p.pather.StopDead();p.rotationTracker.FaceTarget(b.captain);};yield return wait;}
 }
 [HarmonyPatch(typeof(CelestialLightTravel),"Launch",new Type[]{typeof(Pawn),typeof(PlanetTile),typeof(IntVec3),typeof(bool),typeof(bool)})]
 public static class LivingFlightLaunchParty {
  static bool Prefix(Pawn p,ref bool __result){var b=LivingGroupTransport.Existing(p);if(b==null||!b.launching)return true;__result=b.CommitDeparture();return false;}
 }
 [HarmonyPatch(typeof(JobDriver_CelestialDepart),"Cleanup")]
 public static class LivingFlightInterruptedDeparture {static void Postfix(JobDriver_CelestialDepart __instance){var b=LivingGroupTransport.Existing(__instance.pawn);if(b!=null)b.DepartureInterrupted();}}
 [HarmonyPatch(typeof(Pawn),"GetGizmos")]
 public static class LivingFlightCarrierCommands {
  static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result,Pawn __instance){foreach(var g in __result)yield return g;var b=LivingGroupTransport.Existing(__instance);if(b==null||!b.Spawned)yield break;foreach(var g in b.GetComp<CompLivingLaunch>().CompGetGizmosExtra())yield return g;yield return new Command_Action{defaultLabel="Cancel travel preparation",defaultDesc="Release the gathered party, safely unload the supplies and resume normal control.",icon=ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),groupable=false,action=()=>b.CancelPreparation(null)};}
 }
 [HarmonyPatch(typeof(Pawn),"GetInspectString")]
 public static class LivingFlightCarrierInspect {static void Postfix(Pawn __instance,ref string __result){var b=LivingGroupTransport.Existing(__instance);if(b!=null&&b.Spawned)__result+="\nLiving flight: "+b.crew.Count(c=>c.ready)+" / "+b.crew.Count+" gathered; "+b.TotalMass.ToString("0")+" / "+b.Transporter.MassCapacity.ToString("0")+" kg. "+(b.ReadyProblem(false)??"Ready to launch.");}}
}
