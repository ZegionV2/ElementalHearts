using HarmonyLib;
using RimWorld;
using Verse;

namespace ElementalHearts.Patches
{
    // Ordinary equipped/unarmed melee has no hediff callback that reports this damage result.
    // Only damaging hits trigger; extra lightning uses TakeDamage, never this verb, so no recursion.
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class ElectrifiedMeleeHit
    {
        public static void Postfix(Verb_MeleeAttackDamage __instance, LocalTargetInfo __0, DamageWorker.DamageResult __result)
        {
            if (__result == null || __result.totalDamageDealt <= 0f) return;
            HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(__instance.CasterPawn);
            if (comp != null) comp.MeleeHit(__0.Pawn);
        }
    }

    // In the verified 1.6 TryCastShot IL, SoundDodge is called only inside the successful-dodge
    // branch. A probability-method postfix would NOT establish a dodge. Observe this existing
    // branch without changing its return value, then defer the counter out of the attack stack.
    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundDodge")]
    public static class SuccessfulMeleeDodge
    {
        public static void Postfix(Verb_MeleeAttack __instance, Thing __0)
        {
            HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(__0 as Pawn);
            if (comp != null) comp.SuccessfulDodge(__instance.CasterPawn);
        }
    }

    // The same electrical Def must distinguish self-attributed lightning from other sources;
    // a static hediff damage factor cannot express that distinction. Respect existing absorption.
    // Explicit Defs/extensions avoid incorrectly treating every burn/fire packet as electricity.
    [HarmonyPatch(typeof(Pawn), "PreApplyDamage")]
    public static class ElectricalResistance
    {
        public static void Postfix(Pawn __instance, ref DamageInfo __0, ref bool __1)
        {
            if (__1) return;
            HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(__instance);
            if (comp == null) return;
            bool ownLightning = __0.Def == LightningDefs.EH_Lightning && __0.Instigator == __instance;
            if (ownLightning) { __0.SetAmount(0f); __1 = true; return; }
            if ((comp.Props.electricalDamageDefs != null && comp.Props.electricalDamageDefs.Contains(__0.Def)) ||
                __0.Def.GetModExtension<ElectricalDamageExtension>() != null)
                __0.SetAmount(__0.Amount * comp.Props.resistanceFactor);
        }
    }
}
