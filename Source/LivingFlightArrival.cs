using System;using System.Collections.Generic;using System.Linq;using System.Reflection;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 // Preserve native destination actions (including Tower floors and diplomatic
 // consequences), replacing only their pod-shaped map arrival presentation.
 public sealed class LivingFlightArrival:TransportersArrivalAction {
  public TransportersArrivalAction native;public LivingFlightManifest manifest;
  [ThreadStatic]public static LivingFlightManifest Current;
  public static TransportersArrivalAction Wrap(TransportersArrivalAction action,LivingFlightManifest manifest){return action is LivingFlightArrival?action:new LivingFlightArrival{native=action??new TransportersArrivalAction_FormCaravan(),manifest=manifest};}
  public override bool GeneratesMap{get{return native!=null&&native.GeneratesMap;}}
  public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods,PlanetTile tile){return native!=null&&native.ShouldUseLongEvent(pods,tile);}
  public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods,PlanetTile tile){if(native==null)return false;return native.StillValid(pods,tile);}
  public override void ExposeData(){Scribe_Deep.Look(ref native,"EH_livingNativeArrival");Scribe_Deep.Look(ref manifest,"EH_livingFlightManifest");}
  public override void Arrived(List<ActiveTransporterInfo> pods,PlanetTile tile){var previous=Current;Current=manifest??From(pods);try{if(native==null)new TransportersArrivalAction_FormCaravan().Arrived(pods,tile);else native.Arrived(pods,tile);var completed=Current;LongEventHandler.ExecuteWhenFinished(()=>LivingFlightVisuals.Arrived(completed));}finally{Current=previous;}}
  public static bool Matches(List<ActiveTransporterInfo> pods){return Current!=null||pods.Any(p=>p.sentTransporterDef!=null&&p.sentTransporterDef.defName==LivingGroupTransport.PodDef);}
  public static LivingFlightManifest From(List<ActiveTransporterInfo> pods){if(Current!=null)return Current;var pawns=pods.SelectMany(p=>p.innerContainer).OfType<Pawn>().ToList();return LivingFlightManifest.Capture(pawns.FirstOrDefault(IncarnateTransit.Active),pawns,null);}
  public static void Land(List<ActiveTransporterInfo> pods,Map map,IntVec3 preferred){var manifest=From(pods);var caravan=LivingArrivalParty.Form(pods,map.Tile);if(caravan==null)return;var controller=CurrentGameLanding;var record=controller.Register(caravan,map.Parent,manifest);LongEventHandler.ExecuteWhenFinished(()=>{IntVec3 cell;if(FindCell(map,preferred,out cell)&&controller.Land(record,cell))return;controller.Begin(record);});}
  static GameComponent_LivingLanding CurrentGameLanding{get{return Verse.Current.Game.GetComponent<GameComponent_LivingLanding>();}}
  static bool FindCell(Map map,IntVec3 preferred,out IntVec3 result){result=IntVec3.Invalid;if(!preferred.InBounds(map))preferred=map.Center;int n=0;foreach(var cell in GenRadial.RadialCellsAround(preferred,18,true)){if(++n>1024)break;if(CelestialLightTravel.LandingSafe(cell,map,true)){result=cell;return true;}}return false;}
 }
 [HarmonyPatch(typeof(TransportersArrivalAction_LandInSpecificCell),"Arrived")]
 public static class LivingFlightSpecificArrival {
  static readonly FieldInfo parent=AccessTools.Field(typeof(TransportersArrivalAction_LandInSpecificCell),"mapParent"),cell=AccessTools.Field(typeof(TransportersArrivalAction_LandInSpecificCell),"cell");
  static bool Prefix(TransportersArrivalAction_LandInSpecificCell __instance,List<ActiveTransporterInfo> __0,PlanetTile __1){if(!LivingFlightArrival.Matches(__0))return true;var destination=parent.GetValue(__instance) as MapParent;var map=destination==null?null:destination.Map;if(map==null){new TransportersArrivalAction_FormCaravan().Arrived(__0,__1);return false;}LivingFlightArrival.Land(__0,map,(IntVec3)cell.GetValue(__instance));return false;}
 }
 [HarmonyPatch]
 public static class LivingFlightAttackArrival {
  static IEnumerable<MethodBase> TargetMethods(){yield return AccessTools.Method(typeof(PawnsArrivalModeWorker_CenterDrop),"TravellingTransportersArrived");yield return AccessTools.Method(typeof(PawnsArrivalModeWorker_EdgeDrop),"TravellingTransportersArrived");}
  static bool Prefix(List<ActiveTransporterInfo> __0,Map __1,MethodBase __originalMethod){if(!LivingFlightArrival.Matches(__0))return true;IntVec3 at;if(__originalMethod.DeclaringType==typeof(PawnsArrivalModeWorker_EdgeDrop))at=DropCellFinder.FindRaidDropCenterDistant(__1,false,true);else if(!DropCellFinder.TryFindRaidDropCenterClose(out at,__1,true,true,true))at=__1.Center;LivingFlightArrival.Land(__0,__1,at);return false;}
 }
 public static class LivingFlightVisuals {
  static readonly Dictionary<Texture2D,Material> materials=new Dictionary<Texture2D,Material>();
  public static Material WorldMaterial(Texture2D texture){Material material;if(!materials.TryGetValue(texture,out material)){material=MaterialPool.MatFrom(new MaterialRequest(texture,ShaderDatabase.WorldOverlayTransparentLit,Color.white));materials.Add(texture,material);}return material;}
  public static void Arrived(LivingFlightManifest manifest){
   if(manifest==null)return;var spawned=manifest.crew.Where(c=>c.pawn!=null&&c.pawn.Spawned&&!c.pawn.Dead).ToList();if(spawned.Count==0){if(manifest.pause)Find.TickManager.Pause();return;}
   foreach(var state in spawned){var p=state.pawn;if(p.Downed)continue;bool draft=manifest.attack&&p.IsColonistPlayerControlled||state.drafted;if(p.drafter!=null)p.drafter.Drafted=draft;
    if(IncarnateTransit.Active(p)){if(p.jobs.curDriver is JobDriver_CelestialArrive)continue;CelestialLightTravel.CompleteArrival(p,p.Map,p.Position,manifest.pause&&p==manifest.captain,draft);}
    else if(!(p.jobs.curDriver is JobDriver_LivingPassengerArrive)){var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_LivingPassengerArrive"),p.Position);job.playerForced=true;p.jobs.StartJob(job,JobCondition.InterruptForced,cancelBusyStances:true);}
   }
  }
 }
 public sealed class JobDriver_LivingPassengerArrive:JobDriver {
  public int elapsed;public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref elapsed,"EH_livingPassengerArrivalElapsed");}
  protected override IEnumerable<Toil> MakeNewToils(){this.FailOn(()=>!pawn.Spawned||pawn.Dead||pawn.Downed||pawn.Position!=job.targetA.Cell);var arrival=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};arrival.tickAction=()=>{pawn.pather.StopDead();if(++elapsed>=CelestialLightTravel.ArriveTicks)ReadyForNextToil();};yield return arrival;}
 }
}
