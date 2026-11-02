using HarmonyLib;
using RimWorld;
using Verse;

namespace ElementalHearts.Patches
{
    // Ordinary equipped/unarmed melee has no hediff callback that reports this damage result.
    // Only damaging hits trigger; extra flame uses TakeDamage, never this verb, so no recursion.
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class FlameElectrifiedMeleeHit
    {
        public static void Postfix(Verb_MeleeAttackDamage __instance, LocalTargetInfo __0, DamageWorker.DamageResult __result)
        {
            if (__result == null || __result.totalDamageDealt <= 0f) return;
            HediffComp_FlamePassives comp = HediffComp_FlamePassives.On(__instance.CasterPawn);
            if (comp != null) comp.MeleeHit(__0.Pawn);
        }
    }

    // In the verified 1.6 TryCastShot IL, SoundDodge is called only inside the successful-dodge
    // branch. A probability-method postfix would NOT establish a dodge. Observe this existing
    // branch without changing its return value, then defer the counter out of the attack stack.
    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundDodge")]
    public static class FlameSuccessfulMeleeDodge
    {
        public static void Postfix(Verb_MeleeAttack __instance, Thing __0)
        {
            HediffComp_FlamePassives comp = HediffComp_FlamePassives.On(__0 as Pawn);
            if (comp != null) comp.SuccessfulDodge(__instance.CasterPawn);
        }
    }

    // The same thermal Def must distinguish self-attributed flame from other sources;
    // a static hediff damage factor cannot express that distinction. Respect existing absorption.
    // Explicit Defs/extensions avoid incorrectly treating every burn/fire packet as electricity.
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class ThermalResistance
    {
        public static void Postfix(Pawn __instance, ref DamageInfo __0, ref bool __1)
        {
            if (__1) return;
            HediffComp_FlamePassives comp = HediffComp_FlamePassives.On(__instance);
            if (comp == null) return;
            bool ownFlame = __0.Def == FlameDefs.EH_Flame && __0.Instigator == __instance;
            if (ownFlame) { __0.SetAmount(0f); __1 = true; return; }
            if ((comp.Props.thermalDamageDefs != null && comp.Props.thermalDamageDefs.Contains(__0.Def)) ||
                __0.Def.GetModExtension<ThermalDamageExtension>() != null)
                __0.SetAmount(__0.Amount * comp.Props.resistanceFactor);
        }
    }
}
