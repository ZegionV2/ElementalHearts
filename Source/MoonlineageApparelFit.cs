using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class MoonlineageApparelFit {
  public static bool Garment(string def){return def=="EH_LunarUniform"||def=="EH_EclipseUniform";}
  // Hulk's native texture reaches the bottom of its UV square. A taller apparel quad
  // gives the fitted cloak a real hem without cropping it or moving the naked body.
  public static Vector2 Scale(Pawn pawn){return pawn!=null&&pawn.story!=null&&pawn.story.bodyType==BodyTypeDefOf.Hulk?new Vector2(1,1.10f):Vector2.one;}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"ScaleFor")]
 public static class MoonlineageApparelScale {
  public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){
   if(__0.apparel==null||!MoonlineageApparelFit.Garment(__0.apparel.def.defName))return;
   var scale=MoonlineageApparelFit.Scale(__1.pawn);__result.x*=scale.x;__result.z*=scale.y;
  }
 }
 public static class LunarRegaliaHeadFit {
  public static float Lift(Pawn pawn){
   if(pawn==null||pawn.story==null||pawn.ageTracker==null)return 0;
   float coverage=0;var transform=HediffComp_ElementalTransform.On(pawn);
   if(transform!=null&&transform.PulseElement=="Lunar"&&HediffComp_ElementalTransform.Transforming(pawn))coverage=Mathf.SmoothStep(0,1,ManifestMeshRules.PartCoverage(transform.Coverage,false));
   else if(pawn.apparel!=null)foreach(var apparel in pawn.apparel.WornApparel)if(apparel.def.defName=="EH_LunarUniform"){coverage=1;break;}
   if(coverage<=0)return 0;
   string body=pawn.story.bodyType==null?"Male":pawn.story.bodyType.defName;
   float lift=body=="Hulk"?.15f:body=="Fat"?.12f:body=="Child"?.08f:body=="Baby"?.07f:.10f;
   return lift*HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn)/1.5f*coverage;
  }
 }
 [HarmonyPatch(typeof(PawnRenderer),"BaseHeadOffsetAt")]
 public static class LunarRegaliaHeadOrigin {
  // Shared native head origin raises face, helmet, hair and beard together.
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){__result.z+=LunarRegaliaHeadFit.Lift(___pawn);}
 }
}
