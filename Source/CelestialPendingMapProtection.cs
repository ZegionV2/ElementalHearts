using System.Collections.Generic;using System.Reflection;using HarmonyLib;using RimWorld.Planet;using Verse;
namespace ElementalHearts {
 // Empty temporary maps may otherwise be removed between choosing a landing and departure.
 [HarmonyPatch]
 public static class CelestialPendingMapProtection {
  static IEnumerable<MethodBase> TargetMethods(){foreach(var type in new[]{typeof(MapParent),typeof(Settlement),typeof(Site)}){var method=AccessTools.DeclaredMethod(type,"ShouldRemoveMapNow");if(method!=null)yield return method;}}
  public static bool Reserved(PlanetTile tile){if(CelestialLightTravel.PlanningAt(tile)||CelestialLightTravel.DepartureReserved(tile))return true;foreach(var obj in Find.WorldObjects.AllWorldObjects){var flight=obj as WorldObject_CelestialLightTravel;if(flight!=null&&flight.Container.Count>0&&(flight.destination==tile||flight.Tile==tile))return true;}return false;}
  static void Postfix(MapParent __instance,ref bool __result,ref bool __0){if(!__result||!__instance.HasMap||!Reserved(__instance.Tile))return;__result=false;__0=false;}
 }
}
