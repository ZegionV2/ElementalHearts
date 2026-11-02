using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts.Patches {
 [HarmonyPatch(typeof(PawnRenderNode),"ColorFor")]
 public static class LightningHairColor {
  public static void Postfix(PawnRenderNode __instance,Pawn pawn,ref Color __result){
   if(__instance is PawnRenderNode_Hair && pawn.story!=null && (HediffComp_LightningAbilities.IsLightningHair(pawn.story.hairDef)||pawn.story.hairDef.defName=="EH_FlameHair"||pawn.story.hairDef.defName=="EH_WindHair"))__result=Color.white;
  }
 }
}

