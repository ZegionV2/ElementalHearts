using HarmonyLib;
using RimWorld;
using Verse;

namespace ElementalHearts {
 public static class ElementalResistances {
  public static float DamageFactor(Pawn pawn,DamageDef damage){
   if(pawn==null||damage==null)return 1f;
   string element=ElementalTeaching.Element(pawn);
   if(element=="Earth"&&damage==DamageDefOf.Blunt)return AdamantEvolution.Active(pawn)?.65f:.80f;
   if(element=="Wind"&&damage==DamageDefOf.Bomb)return CycloneEvolution.Active(pawn)?.65f:.80f;
   if((element=="Celestial"||EclipseSky.Empowered(pawn))&&(damage==DamageDefOf.Burn||damage==DamageDefOf.Flame||
    damage.GetModExtension<ThermalDamageExtension>()!=null))return .10f;
   if((element=="Celestial"||EclipseSky.Empowered(pawn))&&damage.defName=="EH_SolarDamage")return .25f;
   return 1f;
  }
 }

 [HarmonyPatch(typeof(Pawn),"PreApplyDamage")]
 public static class ElementalResistancePatch {
  public static void Postfix(Pawn __instance,ref DamageInfo __0,bool __1){
   if(!__1)__0.SetAmount(__0.Amount*ElementalResistances.DamageFactor(__instance,__0.Def));
  }
 }
}
