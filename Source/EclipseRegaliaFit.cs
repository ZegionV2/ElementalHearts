using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class EclipseRegaliaFit {
  public const string Uniform="EH_EclipseUniform";
  // Fade the side fit with manifestation so Revert restores the normal body width.
  public static float Coverage(Pawn pawn){if(pawn==null||pawn.apparel==null)return 0;var transform=HediffComp_ElementalTransform.On(pawn);if(transform!=null&&transform.PulseElement=="Eclipse"&&HediffComp_ElementalTransform.Transforming(pawn)&&!CourtAppearance.Bound(pawn))return Mathf.SmoothStep(0,1,ManifestMeshRules.PartCoverage(transform.Coverage,false));foreach(var item in pawn.apparel.WornApparel)if(item.def.defName==Uniform)return 1;return 0;}
  public static float Side(Pawn pawn,Rot4 facing){if(pawn==null||pawn.story==null||(facing!=Rot4.East&&facing!=Rot4.West))return 1;float coverage=Coverage(pawn);if(coverage<=0)return 1;var body=CourtBodyCompatibility.Native(pawn.story.bodyType);float width=body==BodyTypeDefOf.Fat?.74f:body==BodyTypeDefOf.Hulk?.78f:body==BodyTypeDefOf.Thin?.84f:body.defName=="Child"||body.defName=="Baby"?.84f:.80f;return Mathf.Lerp(1,width,coverage);}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker),"ScaleFor")]
 public static class EclipseBodySideFit {
  // The body and garment share the fit; shrinking only the garment exposes skin.
  public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){if(__0 is PawnRenderNode_Body)__result.x*=EclipseRegaliaFit.Side(__1.pawn,__1.facing);}
 }
 [HarmonyPatch(typeof(ApparelGraphicRecordGetter),"TryGetGraphicApparel")]
 public static class EclipseCustomBodyGraphic {
  // Use the nearest complete native outfit variant for replacement body types.
  [HarmonyPriority(Priority.First+200)]public static void Prefix(Apparel __0,ref BodyTypeDef __1){if(__0!=null&&__0.def.defName==EclipseRegaliaFit.Uniform)__1=CourtBodyCompatibility.Native(__1);}
 }
}
