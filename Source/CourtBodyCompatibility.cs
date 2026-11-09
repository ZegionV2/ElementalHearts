using System;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class CourtBodyCompatibility {
  public static bool Uniform(Apparel apparel){if(apparel==null||apparel.def.apparel==null)return false;string path=apparel.def.apparel.wornGraphicPath;return path==CourtAppearance.UniformPath||path=="Things/Pawn/Humanlike/Apparel/CourtHeraldUniform/CourtHeraldUniform";}
  public static BodyTypeDef Native(BodyTypeDef body){if(body==null)return BodyTypeDefOf.Male;switch(body.defName){case "Male":case "Female":case "Thin":case "Fat":case "Hulk":case "Child":case "Baby":return body;}string name=body.defName.ToLowerInvariant();if(name.Contains("baby"))return DefDatabase<BodyTypeDef>.GetNamed("Baby");if(name.Contains("child"))return DefDatabase<BodyTypeDef>.GetNamed("Child");if(name.Contains("hulk"))return BodyTypeDefOf.Hulk;if(name.Contains("fat"))return BodyTypeDefOf.Fat;if(name.Contains("thin"))return BodyTypeDefOf.Thin;return name.Contains("female")?BodyTypeDefOf.Female:BodyTypeDefOf.Male;}
 }
 [HarmonyPatch(typeof(ApparelGraphicRecordGetter),"TryGetGraphicApparel")]
 public static class CourtNativeBodyGraphic {
  [HarmonyPriority(Priority.First+200)]public static void Prefix(Apparel __0,ref BodyTypeDef __1,out bool __state){__state=CourtBodyCompatibility.Uniform(__0)&&CourtBodyCompatibility.Native(__1)!=__1;if(__state)__1=CourtBodyCompatibility.Native(__1);}
  [HarmonyPriority(Priority.Last-100)]public static void Postfix(Apparel __0,BodyTypeDef __1,bool __state,ref ApparelGraphicRecord __3,ref bool __result){if(!__state||!CourtBodyCompatibility.Uniform(__0))return;var graphic=GraphicDatabase.Get<Graphic_Multi>(__0.WornGraphicPath+"_"+CourtBodyCompatibility.Native(__1).defName,ShaderDatabase.Cutout,__0.def.graphicData.drawSize,Color.white);__3=new ApparelGraphicRecord(graphic,__0);__result=true;}
 }
}
