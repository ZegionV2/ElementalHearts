using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 // The preview has no passengers, world registration, ticking or save data. The Tower owns
 // discovery, god-mode visibility, its floor picker and the native saved arrival action.
 public sealed class LivingTravelPreview:Caravan {
  public Action<PlanetTile,CaravanArrivalAction> chosen;
  public World sourceWorld;
 }
 public static class TowerTravelBridge {
  public static List<FloatMenuOption> Options(WorldObject tower,Action<PlanetTile,TransportersArrivalAction> launch){
   var preview=new LivingTravelPreview{def=WorldObjectDefOf.Caravan,Tile=tower.Tile,sourceWorld=Find.World};preview.SetFaction(Faction.OfPlayer);
   preview.chosen=(tile,action)=>{if(preview.sourceWorld==Find.World&&tower.Spawned)launch(tile,new TransportersArrivalAction_LivingTower(action));};
   return tower.GetFloatMenuOptions(preview).ToList();
  }
 }
 [HarmonyPatch(typeof(Caravan_PathFollower),"StartPath")]
 public static class LivingTowerCapture {
  static bool Prefix(Caravan ___caravan,PlanetTile destTile,CaravanArrivalAction arrivalAction,ref bool __result){var preview=___caravan as LivingTravelPreview;if(preview==null)return true;__result=true;if(preview.sourceWorld==Find.World&&arrivalAction!=null)preview.chosen(destTile,arrivalAction);return false;}
 }
 public sealed class TransportersArrivalAction_LivingTower:TransportersArrivalAction_FormCaravan {
  public CaravanArrivalAction nativeAction;
  public TransportersArrivalAction_LivingTower(){}
  public TransportersArrivalAction_LivingTower(CaravanArrivalAction action){nativeAction=action;}
  public override void ExposeData(){base.ExposeData();Scribe_Deep.Look(ref nativeAction,"EH_towerNativeArrival");}
  public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods,PlanetTile tile){return true;}
  public override void Arrived(List<ActiveTransporterInfo> pods,PlanetTile tile){
   var caravan=LivingArrivalParty.Form(pods,tile);if(caravan==null)return;
   // Revalidate after the flight: a different party may now occupy the Tower floor.
   if(nativeAction==null||!nativeAction.StillValid(caravan,tile)){LongEventHandler.ExecuteWhenFinished(()=>{CameraJumper.TryJump(caravan);Messages.Message("Tower entry is no longer available. Your party and cargo are safe in a caravan at the tower.",MessageTypeDefOf.RejectInput,false);});return;}
   nativeAction.Arrived(caravan);
  }
 }
 public static class LivingArrivalParty {
  public static Caravan Form(List<ActiveTransporterInfo> pods,PlanetTile tile){
   var pawn=pods.SelectMany(p=>p.innerContainer).OfType<Pawn>().FirstOrDefault();
   new TransportersArrivalAction_FormCaravan().Arrived(pods,tile);
   return pawn==null?null:Find.WorldObjects.Caravans.FirstOrDefault(c=>c.PawnsListForReading.Contains(pawn));
  }
 }
 public sealed class PawnsArrivalModeWorker_LivingCell:PawnsArrivalModeWorker_CenterDrop {
  public override void TravellingTransportersArrived(List<ActiveTransporterInfo> pods,Map map){
   var manifest=LivingFlightArrival.From(pods);
   var caravan=LivingArrivalParty.Form(pods,map.Tile);if(caravan==null)return;
   var record=Current.Game.GetComponent<GameComponent_LivingLanding>().Register(caravan,map.Parent,manifest);
   LongEventHandler.ExecuteWhenFinished(()=>Current.Game.GetComponent<GameComponent_LivingLanding>().Begin(record));
  }
 }
 public sealed class LivingLanding:IExposable {
  public Caravan caravan;public MapParent destination;public LivingFlightManifest manifest;
  public bool Valid{get{return caravan!=null&&caravan.Spawned&&destination!=null&&destination.Spawned&&destination.HasMap&&caravan.Tile==destination.Tile&&caravan.Faction==Faction.OfPlayer;}}
  public void ExposeData(){Scribe_References.Look(ref caravan,"caravan");Scribe_References.Look(ref destination,"destination");Scribe_Deep.Look(ref manifest,"EH_landingManifest");}
 }
 public sealed class GameComponent_LivingLanding:GameComponent {
  public List<LivingLanding> pending=new List<LivingLanding>();
  public GameComponent_LivingLanding(Game game){}
  public override void ExposeData(){Scribe_Collections.Look(ref pending,"EH_groupLandingChoices",LookMode.Deep);if(Scribe.mode==LoadSaveMode.PostLoadInit&&pending==null)pending=new List<LivingLanding>();}
  public LivingLanding For(Caravan caravan){return pending.FirstOrDefault(r=>r.caravan==caravan&&r.Valid);}
  public LivingLanding Register(Caravan caravan,MapParent parent){return Register(caravan,parent,LivingFlightManifest.Capture(LivingGroupTransport.Carrier(caravan),caravan.PawnsListForReading,null));}
  public LivingLanding Register(Caravan caravan,MapParent parent,LivingFlightManifest manifest){pending.RemoveAll(r=>!r.Valid||r.caravan==caravan);var record=new LivingLanding{caravan=caravan,destination=parent,manifest=manifest};pending.Add(record);return record;}
  public void Begin(LivingLanding record){
   if(!record.Valid)return;var map=record.destination.Map;Current.Game.CurrentMap=map;CameraJumper.TryJump(map.Center,map);Find.TickManager.Pause();
   Messages.Message("Choose a clear, unroofed landing cell. Cancel to keep the party in its caravan; select the caravan's Choose landing cell command to resume.",MessageTypeDefOf.NeutralEvent,false);
   var parameters=TargetingParameters.ForCell();parameters.validator=t=>record.Valid&&Find.CurrentMap==map&&t.Cell.InBounds(map);
   Func<LocalTargetInfo,bool> valid=t=>record.Valid&&Find.CurrentMap==map&&!WorldRendererUtility.WorldSelected&&CelestialLightTravel.LandingSafe(t.Cell,map,true);
   var marks=new List<IntVec3>{IntVec3.Invalid};
   Find.Targeter.BeginTargeting(parameters,t=>{if(valid(t))Land(record,t.Cell);},t=>{if(Find.CurrentMap==map&&t.Cell.InBounds(map)){marks[0]=t.Cell;GenDraw.DrawFieldEdges(marks,valid(t)?Color.yellow:Color.red);}},valid,null,()=>{if(record.Valid){CameraJumper.TryJump(record.caravan);Find.WorldSelector.ClearSelection();Find.WorldSelector.Select(record.caravan);}},IncarnateTransit.Icon(LivingGroupTransport.Carrier(record.caravan)),true,null,t=>Find.TickManager.Pause());
  }
  public bool Land(LivingLanding record,IntVec3 cell){
   if(!record.Valid)return false;var caravan=record.caravan;var map=record.destination.Map;
   // Reserve every destination before transferring a single passenger. No forced overlap,
   // indoor landings or partial unloading when a chosen cell becomes obstructed.
   var cells=new Dictionary<Pawn,IntVec3>();var used=new HashSet<IntVec3>();
   foreach(var pawn in caravan.PawnsListForReading){IntVec3 found=IntVec3.Invalid;foreach(var c in GenRadial.RadialCellsAround(cell,12,true)){if(!used.Contains(c)&&CelestialLightTravel.LandingSafe(c,map,true)){found=c;break;}}if(!found.IsValid){Messages.Message("Not enough clear outdoor space for this party. Choose another landing cell.",MessageTypeDefOf.RejectInput,false);return false;}cells.Add(pawn,found);used.Add(found);}
   if(!CelestialLightTravel.LandingSafe(cell,map,true))return false;
   var party=caravan.PawnsListForReading.ToList();CaravanEnterMapUtility.Enter(caravan,map,p=>cells[p],CaravanDropInventoryMode.DoNotDrop,true);pending.Remove(record);
   foreach(var pawn in party)map.fogGrid.Unfog(pawn.Position);LivingFlightVisuals.Arrived(record.manifest??LivingFlightManifest.Capture(party.FirstOrDefault(IncarnateTransit.Active),party,null));
   if(party.Count>0)CameraJumper.TryJump(party[0]);return true;
  }
  public override void GameComponentTick(){if(Find.TickManager.TicksGame%250==0)pending.RemoveAll(r=>!r.Valid);}
 }
}
