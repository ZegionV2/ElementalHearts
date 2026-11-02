using HarmonyLib;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 // Keep the covered shoulders in place; fit the visible head above the low collar.
 // Using the native shared head origin keeps skin, hair, beard and solar halo aligned.
 public static class CelestialRegaliaHeadFit {
  public static float Lift(Pawn pawn) {
   if(pawn==null||pawn.story==null||pawn.ageTracker==null)return 0;
   float coverage=0;
   var transform=HediffComp_ElementalTransform.On(pawn);
   if(transform!=null&&transform.PulseElement=="Celestial"&&HediffComp_ElementalTransform.Transforming(pawn))
    coverage=Mathf.SmoothStep(0,1,ManifestMeshRules.PartCoverage(transform.Coverage,false));
   else if(pawn.apparel!=null) {
    var apparel=pawn.apparel.WornApparel;
    for(int i=0;i<apparel.Count;i++)if(apparel[i].def.defName=="EH_CelestialUniform"){coverage=1;break;}
   }
   if(coverage<=0)return 0;
   string body=pawn.story.bodyType==null?"Male":pawn.story.bodyType.defName;
   float lift=body=="Hulk"?.15f:body=="Fat"?.12f:.10f;
   return lift*HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn)/1.5f*coverage;
  }
 }
 [HarmonyPatch(typeof(PawnRenderer),"BaseHeadOffsetAt")]
 public static class CelestialRegaliaHeadOrigin {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){__result.z+=CelestialRegaliaHeadFit.Lift(___pawn);}
 }
}
