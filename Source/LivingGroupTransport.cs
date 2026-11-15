using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 // New journeys reuse native cargo loading and destination actions with visible crew and elemental map animations.
 // The original solo world object/jobs remain available for old saves already in flight.
 public static partial class LivingGroupTransport {
  public const string PodDef="EH_LivingTransport",FlightDef="EH_LivingGroupFlight";
  public const string Description="Choose a clear, reachable outdoor launch cell, then use the familiar loading menu to select companions, animals and supplies. The carrier stays manifested and walks to the chosen site. Companions gather nearby; available colonists haul cargo. If nobody suitable is free, the carrier fetches supplies and returns to wait at the launch site. Select the carrier and press Launch when ready. Travel again edits the manifest; Cancel preparation safely releases the party and cargo. Capacity follows the carrier's enhanced carrying capacity. Settlement visit/attack, center/edge/chosen-cell arrivals and ISEKAI Tower floor choices are preserved. Destination menus keep the world visible. Shared launch effects and a flying-pawn world marker support each lineage's elemental departure and arrival. No fuel, cost or cooldown.";
  public static bool Able(Pawn p){return p!=null&&!p.Dead&&!p.Downed&&!p.InMentalState&&p.Faction==Faction.OfPlayer&&IncarnateTransit.Active(p)&&p.health.capacities.CapableOf(PawnCapacityDefOf.Moving);}
  public static bool Preparing(Pawn p){if(p==null||p.jobs==null)return false;if(p.jobs.curDriver is JobDriver_LivingPassengerArrive)return true;var b=p.CurJob==null?null:(p.CurJob.targetA.Thing as Building_LivingTransport??p.CurJob.targetB.Thing as Building_LivingTransport);return b!=null&&b.Spawned&&!b.cancelling&&!b.committing&&b.Contains(p);}
  public static Building_LivingTransport Existing(Pawn p){foreach(var map in Find.Maps)foreach(var t in map.listerThings.ThingsOfDef(DefDatabase<ThingDef>.GetNamed(PodDef))){var b=t as Building_LivingTransport;if(b!=null&&b.captain==p)return b;}return null;}
  public static Building_LivingTransport Create(Pawn p,bool open=true){return CreateAt(p,p==null?IntVec3.Invalid:p.Position,open);}
  public static Building_LivingTransport CreateAt(Pawn p,IntVec3 site,bool open=true){
   if(!Able(p)||!p.Spawned){Messages.Message("A conscious Celestial or Incarnate must create the transport on a map.",MessageTypeDefOf.RejectInput,false);return null;}
   var existing=Existing(p);if(existing!=null){Select(p);if(open)existing.OpenLoader();return existing;}
   string problem=CelestialLightTravel.PreparationProblem(p);if(problem!=null){Messages.Message(problem,MessageTypeDefOf.RejectInput,false);return null;}
   string siteProblem=SiteProblem(p,p.Map,site);if(siteProblem!=null){Messages.Message(siteProblem,MessageTypeDefOf.RejectInput,false);return null;}
   var pod=(Building_LivingTransport)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed(PodDef));pod.captain=p;pod.version=2;pod.crew.Add(LivingFlightPawnState.Capture(p));pod.SetFaction(Faction.OfPlayer);GenSpawn.Spawn(pod,site,p.Map);
   pod.HoldCarrier();Select(p);if(open)pod.OpenLoader();return pod;
  }
  static void Select(Thing t){CameraJumper.TryJump(t);Find.Selector.ClearSelection();Find.Selector.Select(t);}
  public static List<FloatMenuOption> Options(PlanetTile tile,IEnumerable<IThingHolder> holders,Action<PlanetTile,TransportersArrivalAction> launch){
   var pods=holders.Select(h=>h is CompLivingTransport?(IThingHolder)new LivingFlightPlanningContents((Building_LivingTransport)((CompLivingTransport)h).parent):h).ToList();var tower=Find.WorldObjects.AllWorldObjects.FirstOrDefault(w=>w.Tile==tile&&w.GetType().FullName=="IsekaiDungeons.WorldObject_Tower");
   if(tower!=null)return TowerTravelBridge.Options(tower,launch);
   // CompLaunchable.GetOptionsForTile dispatches shuttle menus in this game version.
   // Ask world objects for their transport-pod menus explicitly (including mod overrides).
   var result=new List<FloatMenuOption>();
   if(TransportersArrivalAction_FormCaravan.CanFormCaravanAt(pods,tile)&&!Find.WorldObjects.AnySettlementBaseAt(tile)&&!Find.WorldObjects.AnySiteAt(tile))
    result.Add(new FloatMenuOption("FormCaravanHere".Translate(),()=>launch(tile,new TransportersArrivalAction_FormCaravan())));
   foreach(var worldObject in Find.WorldObjects.AllWorldObjects.Where(w=>w.Tile==tile))result.AddRange(worldObject.GetTransportersFloatMenuOptions(pods,launch));
   var settlement=Find.WorldObjects.SettlementAt(tile);
   if(settlement!=null&&TransportersArrivalAction_AttackSettlement.CanAttack(pods,settlement))
    result.Add(new FloatMenuOption("Attack: choose landing cell on arrival",()=>launch(tile,new TransportersArrivalAction_AttackSettlement(settlement,DefDatabase<PawnsArrivalModeDef>.GetNamed("EH_LivingChooseCell")))));
   return result;
  }
  public static void Choose(PlanetTile origin,IEnumerable<IThingHolder> holders,Texture2D icon,Action<PlanetTile,TransportersArrivalAction> launch){
   CameraJumper.TryJump(new GlobalTargetInfo(origin));
   Find.WorldTargeter.BeginTargeting(t=>{if(!t.IsValid||!t.Tile.Valid||t.Tile==origin||t.Tile.Layer!=origin.Layer)return false;var options=Options(t.Tile,holders,launch);if(options.Count==0){Messages.Message("No available arrival action here.",MessageTypeDefOf.RejectInput,false);return false;}Find.WindowStack.Add(new FloatMenu(options));return true;},true,LivingFlightAssets.FlyingPawn,false,null,t=>new TaggedString("Choose a destination, then choose how the party arrives."),t=>t.IsValid&&t.Tile.Valid&&t.Tile!=origin&&t.Tile.Layer==origin.Layer,origin,true);
  }
  public static Pawn Carrier(Caravan caravan){return caravan==null?null:caravan.PawnsListForReading.FirstOrDefault(Able);}
  public static float PayloadMass(Caravan caravan){return caravan.PawnsListForReading.Sum(p=>p.GetStatValue(StatDefOf.Mass)+MassUtility.GearAndInventoryMass(p));}
  public static void LaunchCaravan(Caravan caravan,Pawn captain,PlanetTile tile,TransportersArrivalAction action){
   if(caravan==null||!caravan.Spawned||!Able(captain)||!caravan.PawnsListForReading.Contains(captain)||caravan.Tile==tile||caravan.Tile.Layer!=tile.Layer)return;
   if(PayloadMass(caravan)>MassUtility.Capacity(captain,null)){Messages.Message("The party exceeds this carrier's flight capacity.",MessageTypeDefOf.RejectInput,false);return;}
   var info=new ActiveTransporterInfo{sentTransporterDef=DefDatabase<ThingDef>.GetNamed(PodDef)};var party=caravan.PawnsListForReading.ToList();
   // Validate before changing ownership. Every pawn carries its original equipment and inventory.
   if(action!=null&&!action.StillValid(new[]{(IThingHolder)caravan},tile)){Messages.Message("That arrival action is no longer available.",MessageTypeDefOf.RejectInput,false);return;}
   var flight=(TravellingTransporters)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed(FlightDef));flight.Tile=caravan.Tile;flight.destinationTile=tile;flight.arrivalAction=LivingFlightArrival.Wrap(action,LivingFlightManifest.Capture(captain,party,action));flight.SetFaction(Faction.OfPlayer);
   Find.WorldObjects.Add(flight);caravan.pather.StopDead();
   foreach(var pawn in party){caravan.RemovePawn(pawn);if(info.innerContainer.TryAdd(pawn))continue;
    caravan.AddPawn(pawn,false);foreach(var boarded in info.innerContainer.OfType<Pawn>().ToList()){info.innerContainer.Remove(boarded);caravan.AddPawn(boarded,false);}Find.WorldObjects.Remove(flight);Messages.Message("Could not board the whole party. Everyone remains in the caravan.",MessageTypeDefOf.RejectInput,false);return;}
   flight.AddTransporter(info,false);if(caravan.Spawned)Find.WorldObjects.Remove(caravan);
   Find.WorldSelector.ClearSelection();Find.WorldSelector.Select(flight);CameraJumper.TryJump(flight);
  }
 }
 public sealed partial class Building_LivingTransport:Building {
  public Pawn captain;
  public CompTransporter Transporter{get{return GetComp<CompTransporter>();}}
  public override void ExposeData(){base.ExposeData();Scribe_References.Look(ref captain,"EH_transportCaptain");ExposePreparation();}
  public void OpenLoader(){if(Spawned&&!launching)Find.WindowStack.Add(new Dialog_LivingTransport(Map,Transporter,captain));}
  public bool Dismiss(){return CancelPreparation(null);}
  public override string GetInspectString(){return base.GetInspectString()+"\nCarrier: "+(captain==null?"missing":captain.LabelShortCap.ToString());}
 }
 public sealed class CompProperties_LivingTransport:CompProperties_Transporter {
  public CompProperties_LivingTransport(){compClass=typeof(CompLivingTransport);max1PerGroup=true;canChangeAssignedThingsAfterStarting=true;shouldTickContents=true;massCapacity=450;}
 }
 public sealed class CompLivingTransport:CompTransporter {
  public override bool RequiresFuelingPort{get{return false;}}
  public override void CompTick(){base.CompTick();((Building_LivingTransport)parent).PreparationTick();}
  public override IEnumerable<Gizmo> CompGetGizmosExtra(){var b=(Building_LivingTransport)parent;yield return new Command_Action{defaultLabel="Load party and cargo",defaultDesc=LivingGroupTransport.Description,icon=IncarnateTransit.Icon(b.captain),action=b.OpenLoader};yield return new Command_Action{defaultLabel="Unload and dismiss",defaultDesc="Cancel loading, safely unload everyone and all cargo, then remove this temporary transport.",icon=ContentFinder<Texture2D>.Get("UI/Designators/Cancel"),action=()=>b.Dismiss()};}
 }
 [HarmonyPatch(typeof(CompTransporter),"get_MassCapacity")]
 public static class LivingTransportCapacity {
  static void Postfix(CompTransporter __instance,ref float __result){var b=__instance.parent as Building_LivingTransport;if(b!=null&&b.captain!=null)__result=Mathf.Max(1,MassUtility.Capacity(b.captain,null));}
 }
 public sealed class CompProperties_LivingLaunch:CompProperties_Launchable {
  public CompProperties_LivingLaunch(){compClass=typeof(CompLivingLaunch);fixedLaunchDistanceMax=1000000;fuelPerTile=0;minFuelCost=0;cooldownTicks=0;}
 }
 public sealed class CompLivingLaunch:CompLaunchable {
  public override bool RequiresFuelingPort{get{return false;}}
  public override float FuelLevel{get{return 1000000;}}
  public override float MaxFuelLevel{get{return 1000000;}}
  public override AcceptanceReport CanLaunch(float? overrideFuelLevel=null){
   var b=(Building_LivingTransport)parent;if(!b.Spawned||b.Faction!=Faction.OfPlayer)return "Transport unavailable.";
   string problem=b.ReadyProblem(false);if(problem!=null)return problem;
   return base.CanLaunch(overrideFuelLevel);
  }
  public void LaunchChecked(PlanetTile tile,TransportersArrivalAction arrival){if(!parent.Spawned)return;var report=CanLaunch();if(!report){Messages.Message(report.Reason,MessageTypeDefOf.RejectInput,false);return;}((Building_LivingTransport)parent).StartDeparture(tile,arrival);}
  public override IEnumerable<Gizmo> CompGetGizmosExtra(){var command=new Command_Action{defaultLabel="Launch living flight",defaultDesc=LivingGroupTransport.Description,icon=LivingFlightAssets.LaunchIcon,groupable=false,action=()=>LivingGroupTransport.Choose(parent.Map.Tile,new[]{(IThingHolder)Transporter},LivingFlightAssets.LaunchIcon,LaunchChecked)};var report=CanLaunch();if(!report)command.Disable(report.Reason);yield return command;}
  public override string CompInspectStringExtra(){return "Free flight · no fuel or cooldown";}
 }
 public sealed class Dialog_LivingTransport:Dialog_LoadTransporters {
  readonly Pawn captain;public readonly Building_LivingTransport anchor;
  static readonly FieldInfo transfers=AccessTools.Field(typeof(Dialog_LoadTransporters),"transferables");
  static readonly MethodInfo changed=AccessTools.Method(typeof(Dialog_LoadTransporters),"CountToTransferChanged");
  public List<TransferableOneWay> Transfers{get{return transfers.GetValue(this) as List<TransferableOneWay>;}}
  public Dialog_LivingTransport(Map map,CompTransporter pod,Pawn captain):base(map,new List<CompTransporter>{pod}){this.captain=captain;anchor=pod.parent as Building_LivingTransport;}
  public override void PostOpen(){base.PostOpen();var list=Transfers;if(list!=null){foreach(var state in anchor.crew){var row=list.FirstOrDefault(t=>t.things.Contains(state.pawn));if(row!=null)row.AdjustTo(1);}changed.Invoke(this,null);}}
  public override void PostClose(){base.PostClose();if(anchor!=null&&anchor.Spawned&&!anchor.confirmed)anchor.CancelPreparation(null);}
 }
 public sealed class WorldObject_LivingGroupFlight:TravellingTransporters {
  public override string Label{get{var p=Pawns.FirstOrDefault(IncarnateTransit.Active);return p==null?"Living flight":p.LabelShort+" — living flight";}}
  public override Texture2D ExpandingIcon{get{return LivingFlightAssets.FlyingPawn;}}
  public override Color ExpandingIconColor{get{return LivingFlightAssets.Tint(Pawns.FirstOrDefault(IncarnateTransit.Active));}}
  public override Material Material{get{return LivingFlightAssets.WorldMaterial(Pawns.FirstOrDefault(IncarnateTransit.Active));}}
  public override Material ExpandingMaterial{get{return Material;}}
  public override void ExposeData(){base.ExposeData();if(Scribe.mode==LoadSaveMode.PostLoadInit&&arrivalAction!=null&&!(arrivalAction is LivingFlightArrival))arrivalAction=LivingFlightArrival.Wrap(arrivalAction,LivingFlightManifest.Capture(Pawns.FirstOrDefault(IncarnateTransit.Active),Pawns.ToList(),arrivalAction));}
 }
 [HarmonyPatch(typeof(Caravan),"GetGizmos")]
 public static class LivingTransportCaravanCommands {
  static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result,Caravan __instance){foreach(var g in __result)yield return g;var caravan=__instance;if(caravan.Faction!=Faction.OfPlayer)yield break;
   var pending=Current.Game.GetComponent<GameComponent_LivingLanding>().For(caravan);if(pending!=null)yield return new Command_Action{defaultLabel="Choose landing cell",defaultDesc="Resume the saved landing choice. Canceling keeps the entire party in this caravan.",icon=IncarnateTransit.Icon(LivingGroupTransport.Carrier(caravan)),action=()=>Current.Game.GetComponent<GameComponent_LivingLanding>().Begin(pending)};
   var captain=LivingGroupTransport.Carrier(caravan);if(captain!=null){var c=new Command_Action{defaultLabel="Living flight",defaultDesc="Fly onward with the whole caravan. "+LivingGroupTransport.Description,icon=LivingFlightAssets.LaunchIcon,action=()=>LivingGroupTransport.Choose(caravan.Tile,new[]{(IThingHolder)caravan},LivingFlightAssets.LaunchIcon,(t,a)=>LivingGroupTransport.LaunchCaravan(caravan,captain,t,a))};if(LivingGroupTransport.PayloadMass(caravan)>MassUtility.Capacity(captain,null))c.Disable("The party exceeds this carrier's flight capacity.");yield return c;}
  }
 }
}
