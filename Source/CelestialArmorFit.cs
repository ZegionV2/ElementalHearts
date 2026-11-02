using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class CelestialArmorFit {
  public struct Fit {public float x,z,pixelX,pixelDown;public Fit(float x,float z,float px,float down){this.x=x;this.z=z;pixelX=px;pixelDown=down;}}
  static readonly Fit[][] data={
   new[]{new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.08f,1f,13f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f)},
   new[]{new Fit(1.0f,1.0f,-10f,-1f),new Fit(1.0f,1.0f,-6f,0f),new Fit(1.0f,1.0f,-3f,0f),new Fit(1.04f,1.0f,-17f,0f),new Fit(1.0f,1.16f,-6f,13f),new Fit(1.0f,1.0f,-2f,0f),new Fit(1.0f,1.0f,0f,0f)},
   new[]{new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,6f),new Fit(1.0f,1.1f,0f,14f),new Fit(1.0f,1.0f,0f,0f),new Fit(1.0f,1.0f,0f,0f)}
  };
  static int Body(Pawn p){switch(p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName){case "Female":return 1;case "Thin":return 2;case "Fat":return 3;case "Hulk":return 4;case "Child":return 5;case "Baby":return 6;default:return 0;}}
  public static bool TryGet(string def,Pawn p,Rot4 facing,out Fit fit){fit=default(Fit);if(p==null||def!="EH_CelestialUniform")return false;int dir=facing==Rot4.South?0:facing==Rot4.North?2:1;fit=data[dir][Body(p)];return true;}
  public static Vector3 Offset(Fit fit,Rot4 facing,float width){return new Vector3((facing==Rot4.West?-1:1)*fit.pixelX*width/256f,0,-fit.pixelDown*width/256f);}
  public static void Echo(Pawn p,ref Vector3 at,ref float width,ref float height){Fit fit;if(!TryGet("EH_CelestialUniform",p,p.Rotation,out fit))return;float native=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(p);at+=Offset(fit,p.Rotation,native);width=native*fit.x;height=native*fit.z;}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"OffsetFor")]
 public static class CelestialApparelOffset {public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){CelestialArmorFit.Fit fit;if(__0.apparel!=null&&CelestialArmorFit.TryGet(__0.apparel.def.defName,__1.pawn,__1.facing,out fit))__result+=CelestialArmorFit.Offset(fit,__1.facing,HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(__1.pawn));}}
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"ScaleFor")]
 public static class CelestialApparelScale {public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){CelestialArmorFit.Fit fit;if(__0.apparel!=null&&CelestialArmorFit.TryGet(__0.apparel.def.defName,__1.pawn,__1.facing,out fit)){__result.x*=fit.x;__result.z*=fit.z;}}}
}
