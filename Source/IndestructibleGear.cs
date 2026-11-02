using System;
using HarmonyLib;
using Verse;

namespace ElementalHearts {
 public sealed class IndestructibleElementalGear : DefModExtension {}
 public static class ElementalGear {
  public static bool Protected(Thing thing){return thing!=null&&thing.def!=null&&thing.def.HasModExtension<IndestructibleElementalGear>();}
  public static int PreserveHitPoints(int current,int maximum,int proposed){return Math.Max(proposed,Math.Min(current,maximum));}
 }
 public sealed class CompProperties_IndestructibleGear : CompProperties {
  public CompProperties_IndestructibleGear(){compClass=typeof(CompIndestructibleGear);}
 }
 public sealed class CompIndestructibleGear : ThingComp {
  public override void PostPostMake(){base.PostPostMake();parent.HitPoints=parent.MaxHitPoints;}
  public override void PostExposeData(){base.PostExposeData();if(Scribe.mode==LoadSaveMode.PostLoadInit&&!parent.Destroyed)parent.HitPoints=parent.MaxHitPoints;}
 }
 // Durability protection does not absorb damage meant for the pawn wearing the armor.
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class ElementalGearNoDamage {
  [HarmonyPriority(Priority.First)]
  public static bool Prefix(Thing __instance,ref DamageWorker.DamageResult __result){if(!ElementalGear.Protected(__instance))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 [HarmonyPatch(typeof(Thing),"set_HitPoints")]
 public static class ElementalGearNoWear {
  public static void Prefix(Thing __instance,ref int value){if(ElementalGear.Protected(__instance))value=ElementalGear.PreserveHitPoints(__instance.HitPoints,__instance.MaxHitPoints,value);}
 }
}
