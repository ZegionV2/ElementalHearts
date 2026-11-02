using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts.Patches {
 // GraphicData.ignoreThingDrawColor locks world graphics, but worn apparel independently
 // reads DrawColor (including saved dyes). Restrict this postfix to our two gear Defs.
 [HarmonyPatch(typeof(ThingWithComps),"get_DrawColor")]
 public static class LightningGearColor {
  public static void Postfix(ThingWithComps __instance,ref Color __result){
   if(__instance.def.defName=="EH_LightningUniform"||__instance.def.defName=="EH_LightningSword"||__instance.def.defName=="EH_FlameUniform"||__instance.def.defName=="EH_FlameSword")__result=Color.white;
  }
 }
}
