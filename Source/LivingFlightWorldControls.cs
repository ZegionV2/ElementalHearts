using HarmonyLib;using RimWorld.Planet;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class LivingFlightWorldControls {
  public static WorldObject_CelestialLightTravel Selected{get{if(Find.WorldSelector==null)return null;var selected=Find.WorldSelector.SelectedObjects;return selected.Count==1?selected[0] as WorldObject_CelestialLightTravel:null;}}
 }
 // The stock world selector builds these controls from Tile. A moving vessel's Tile
 // retains the original map for safe return, so it must not offer that map's caravan.
 [HarmonyPatch(typeof(WorldGizmoUtility),"TryGetCaravanGizmo")]
 public static class LivingFlightNoOriginCaravan {
  public static bool Prefix(ref Gizmo __0,ref bool __result){if(LivingFlightWorldControls.Selected==null)return true;__0=null;__result=false;return false;}
 }
 [HarmonyPatch(typeof(WorldGizmoUtility),"GetJumpToGizmo")]
 public static class LivingFlightJumpToTraveler {
  public static void Postfix(ref Gizmo __result){var flight=LivingFlightWorldControls.Selected;if(flight==null)return;var command=__result as Command_Action;if(command==null)command=new Command_Action();command.defaultLabel="View flight";command.defaultDesc="Center the world camera on the traveler's current position.";command.icon=ContentFinder<Texture2D>.Get(flight.def.texture);command.action=flight.FocusWorld;command.groupable=false;__result=command;}
 }
}
