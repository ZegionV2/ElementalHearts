using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class OtherworldApparelFit {
  public static bool Outfit(Apparel a){return a!=null&&(a.def.defName=="EH_OtherworldGakuran"||a.def.defName=="EH_OtherworldSailor");}
  public static float Lift(Pawn p){if(p==null||p.apparel==null)return 0;foreach(var a in p.apparel.WornApparel)if(Outfit(a))return .06f*HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(p)/1.5f;return 0;}
 }
 [HarmonyPatch(typeof(PawnRenderer),"BaseHeadOffsetAt")]
 public static class OtherworldUniformHeadOrigin {public static void Postfix(Pawn ___pawn,ref Vector3 __result){__result.z+=OtherworldApparelFit.Lift(___pawn);}}
 [HarmonyPatch(typeof(ApparelGraphicRecordGetter),"TryGetGraphicApparel")]
 public static class OtherworldCustomBodyFallback {
  public static void Prefix(Apparel __0,ref BodyTypeDef __1){if(!OtherworldApparelFit.Outfit(__0)||__1==null||__1.defName=="FemaleBB")return;__1=CourtBodyCompatibility.Native(__1);}
 }
 [HarmonyPatch(typeof(Pawn_ApparelTracker),"Wear")]
 public static class OtherworldGenderedOutfits {
  public static bool Prefix(Apparel newApparel,Pawn ___pawn){if(newApparel==null||___pawn==null)return true;return newApparel.def.defName!="EH_OtherworldGakuran"&&newApparel.def.defName!="EH_OtherworldSailor"||newApparel.def.defName=="EH_OtherworldGakuran"&&___pawn.gender==Gender.Male||newApparel.def.defName=="EH_OtherworldSailor"&&___pawn.gender==Gender.Female;}
 }
}
