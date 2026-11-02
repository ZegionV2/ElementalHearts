using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 // Pose correction measured against the native body alpha; textures and gameplay are unchanged.
 public static class RegalSideFit {
  public struct Fit {public float x,z,pixelX,pixelDown;public Fit(float x,float z,float px,float down){this.x=x;this.z=z;pixelX=px;pixelDown=down;}}
  static readonly Fit[][] fits={
   new[]{new Fit(1.14f,1.0f,-18f,6f),new Fit(1.06f,1.04f,-19f,0f),new Fit(1.0f,1.0f,-15f,0f),new Fit(1.58f,1.02f,-33f,2f),new Fit(1.32f,1.04f,-26f,10f),new Fit(1.06f,1.0f,-14f,0f),new Fit(1.1f,1.0f,-12f,-6f)},
   new[]{new Fit(1.18f,1.0f,-15f,3f),new Fit(1.14f,1.04f,-17f,0f),new Fit(1.0f,1.0f,-13f,0f),new Fit(1.62f,1.0f,-29f,3f),new Fit(1.36f,1.04f,-21f,9f),new Fit(1.08f,1.0f,-11f,0f),new Fit(1.12f,1.0f,-9f,-10f)},
   new[]{new Fit(1.12f,1.0f,-15f,5f),new Fit(1.04f,1.08f,-17f,0f),new Fit(1.0f,1.0f,-14f,0f),new Fit(1.56f,1.02f,-31f,3f),new Fit(1.28f,1.06f,-23f,10f),new Fit(1.02f,1.0f,-11f,-2f),new Fit(1.06f,1.0f,-8f,-10f)},
   new[]{new Fit(1.14f,1.0f,-16f,0f),new Fit(1.08f,1.08f,-17f,-2f),new Fit(1.0f,1.0f,-14f,0f),new Fit(1.58f,1.02f,-33f,2f),new Fit(1.34f,1.08f,-26f,5f),new Fit(1.04f,1.0f,-11f,-6f),new Fit(1.12f,1.0f,-9f,-9f)},
   new[]{new Fit(1.1f,1.0f,1f,0f),new Fit(1.25f,1.06f,-14f,2f),new Fit(1.13f,1.0f,-8f,0f),new Fit(1.51f,1.03f,-12f,3f),new Fit(1.36f,1.08f,-6f,12f),new Fit(1.2f,1.0f,-1f,-1f),new Fit(1.26f,1.0f,-1f,-13f)},
   new[]{new Fit(1.08f,1.0f,-1f,-9f),new Fit(1.18f,1.03f,-11f,2f),new Fit(1.1f,1.0f,-8f,0f),new Fit(1.41f,1.0f,-5f,3f),new Fit(1.36f,1.07f,-8f,11f),new Fit(1.06f,1.0f,2f,-3f),new Fit(1.16f,1.0f,0f,-15f)},
   new[]{new Fit(1.06f,1.0f,-1f,-4f),new Fit(1.26f,1.06f,-15f,0f),new Fit(1.18f,1.0f,-8f,0f),new Fit(1.51f,1.0f,-10f,4f),new Fit(1.28f,1.2f,-12f,3f),new Fit(1.18f,1.0f,1f,0f),new Fit(1.26f,1.0f,-2f,-14f)},
   new[]{new Fit(1.03f,1.0f,0f,-8f),new Fit(1.19f,1.01f,-11f,2f),new Fit(1.1f,1.0f,-6f,11f),new Fit(1.36f,1.0f,-6f,1f),new Fit(1.37f,1.06f,-4f,13f),new Fit(1.05f,1.0f,4f,-4f),new Fit(1.18f,1.0f,5f,-3f)}
  };
  static int Family(string def){switch(def){case "EH_AzureFlameUniform":return 0;case "EH_ThunderstormUniform":return 1;case "EH_GlacialUniform":return 2;case "EH_CycloneUniform":return 3;case "EH_LightningUniform":return 4;case "EH_FlameUniform":return 5;case "EH_WindUniform":return 6;case "EH_WaterUniform":return 7;default:return -1;}}
  static int Body(Pawn p){switch(p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName){case "Female":return 1;case "Thin":return 2;case "Fat":return 3;case "Hulk":return 4;case "Child":return 5;case "Baby":return 6;default:return 0;}}
  public static bool TryGet(string def,Pawn pawn,Rot4 facing,out Fit fit){fit=default(Fit);int family=Family(def);if(pawn==null||family<0||(facing!=Rot4.East&&facing!=Rot4.West))return false;fit=fits[family][Body(pawn)];return true;}
  public static Vector3 Offset(Fit fit,Rot4 facing,float nativeWidth){return new Vector3((facing==Rot4.West?-1:1)*fit.pixelX*nativeWidth/256f,0,-fit.pixelDown*nativeWidth/256f);}
  public static void Echo(Pawn pawn,string def,ref Vector3 at,ref float width,ref float height){Fit fit;if(!TryGet(def,pawn,pawn.Rotation,out fit))return;float native=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn);at+=Offset(fit,pawn.Rotation,native);width=native*fit.x;height=native*fit.z;}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"OffsetFor")]
 public static class RegalSideOffset {
  public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){RegalSideFit.Fit fit;if(__0.apparel==null||!RegalSideFit.TryGet(__0.apparel.def.defName,__1.pawn,__1.facing,out fit))return;__result+=RegalSideFit.Offset(fit,__1.facing,HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(__1.pawn));}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"ScaleFor")]
 public static class RegalSideScale {
  public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){RegalSideFit.Fit fit;if(__0.apparel==null||!RegalSideFit.TryGet(__0.apparel.def.defName,__1.pawn,__1.facing,out fit))return;__result.x*=fit.x;__result.z*=fit.z;}
 }
}
