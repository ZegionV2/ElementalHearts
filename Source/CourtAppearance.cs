using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public sealed class CourtKitAppearance:DefModExtension {public ThingDef original;public bool heraldFaceguard;}
 public static class CourtAppearance {
  public const string TraitName="EH_SeveredOath",UniformPath="Things/Pawn/Humanlike/Apparel/CourtUniform/CourtUniform";
  static readonly string[] bodies={"Male","Female","Thin","Fat","Hulk","Child","Baby"},directions={"south","east","north"};
  static readonly Material[,,] materials=new Material[2,7,3];
  public static bool Bound(Pawn p){return p!=null&&p.story!=null&&p.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed(TraitName));}
  public static bool Faceguard(ThingDef def){var e=def==null?null:def.GetModExtension<CourtKitAppearance>();return e!=null&&e.heraldFaceguard;}
  public static ThingDef Original(ThingDef def){var e=def==null?null:def.GetModExtension<CourtKitAppearance>();return e==null||e.original==null?def:e.original;}
  public static ThingDef Resolve(Pawn p,string equipmentElement,string suffix){string name="EH_"+equipmentElement+suffix;if(suffix!="Sword"&&Bound(p)){var variant=DefDatabase<ThingDef>.GetNamedSilentFail("EH_Court_"+equipmentElement+suffix);if(variant!=null)return variant;}return DefDatabase<ThingDef>.GetNamedSilentFail(name);}
  public static bool WearingKit(Pawn p){if(p==null||p.apparel==null)return false;foreach(var a in p.apparel.WornApparel)if(a.def.HasModExtension<CourtKitAppearance>()&&a.def.defName.EndsWith("Uniform"))return true;return false;}
  public static Material Body(Pawn p){int b=Array.IndexOf(bodies,p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName);if(b<0)b=0;int d=p.Rotation==Rot4.South?0:p.Rotation==Rot4.North?2:1;int family=0;if(p.apparel!=null)foreach(var a in p.apparel.WornApparel)if(a.def.apparel.wornGraphicPath=="Things/Pawn/Humanlike/Apparel/CourtHeraldUniform/CourtHeraldUniform"){family=1;break;}string path=family==1?"Things/Pawn/Humanlike/Apparel/CourtHeraldUniform/CourtHeraldUniform":UniformPath;return materials[family,b,d]??(materials[family,b,d]=MaterialPool.MatFrom(path+"_"+bodies[b]+"_"+directions[d],ShaderDatabase.Transparent));}
  public static Vector2 Scale(Pawn p){return new Vector2(1,p.story!=null&&p.story.bodyType==BodyTypeDefOf.Hulk?1.1f:1);}
  public static void Echo(Pawn p,ref float width,ref float height){float native=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(p);var s=Scale(p);width=native*s.x;height=native*s.y;}
 }
 [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body),"ScaleFor")]
 public static class CourtBodyScale {
  public static void Postfix(PawnRenderNode __0,PawnDrawParms __1,ref Vector3 __result){if(__0.apparel==null||!(__0.apparel.def.HasModExtension<CourtKitAppearance>()||__0.apparel.def.defName=="EH_CourtTravelUniform"))return;__result.z*=CourtAppearance.Scale(__1.pawn).y;}
 }
}
