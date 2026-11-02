using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ElementalHearts {
 public enum ElementalAutoMode { On, Off, NonLethal }
 public static class ElementalAutoModes {
  public static ElementalAutoMode FromFlags(bool automatic,bool nonLethal){return !automatic?ElementalAutoMode.Off:nonLethal?ElementalAutoMode.NonLethal:ElementalAutoMode.On;}
  public static ElementalAutoMode Next(ElementalAutoMode mode){return mode==ElementalAutoMode.On?ElementalAutoMode.Off:mode==ElementalAutoMode.Off?ElementalAutoMode.NonLethal:ElementalAutoMode.On;}
  public static string Label(ElementalAutoMode mode){return mode==ElementalAutoMode.NonLethal?"Non-lethal":mode.ToString();}
 }
 public static class ElementalNonLethal {
  public const float ForceFactor=.2f, KnockdownChance=.75f;
  public static bool Enabled(Pawn source){
   if(source==null)return false;var celestial=HediffComp_CelestialAbilities.On(source);if(celestial!=null)return celestial.Mode==ElementalAutoMode.NonLethal;
   var water=HediffComp_WaterDefense.On(source);if(water!=null)return water.Mode==ElementalAutoMode.NonLethal;
   var lightning=HediffComp_LightningAbilities.On(source);
   if(lightning!=null)return lightning.CurrentAutoMode==ElementalAutoMode.NonLethal;
   var flame=HediffComp_FlameAbilities.On(source);
   if(flame!=null)return flame.CurrentAutoMode==ElementalAutoMode.NonLethal; var wind=HediffComp_WindAbilities.On(source);return wind!=null&&wind.CurrentAutoMode==ElementalAutoMode.NonLethal;
  }
  public static float ReducedForce(float original){return Math.Max(0f,Math.Min(100f,original*ForceFactor));}
  public static int StunDuration(float original){return 45+(int)Math.Min(45f,ReducedForce(original));}
  // Returning true consumes the entire hit, including its physical/elemental/EMP packets.
  // Suppression replaces injury: damage reduction alone cannot protect a wounded vital part.
  public static bool TryApply(Pawn source,Pawn victim,float originalForce){
   if(!Enabled(source))return false;
   if(victim==null||victim==source||victim.Dead||victim.Downed||!victim.Spawned||victim.health==null)return true;
   var wind=HediffComp_WindAbilities.On(source);if(wind!=null)wind.MarkCombat(); var lightning=HediffComp_LightningPassives.On(source);if(lightning!=null)lightning.MarkCombat();
   var flame=HediffComp_FlamePassives.On(source);if(flame!=null)flame.MarkCombat();
   if(!Rand.Chance(KnockdownChance)){
    if(victim.stances!=null)victim.stances.stunner.StunFor(StunDuration(originalForce),source);
    return true;
   }
   HediffDef def=DefDatabase<HediffDef>.GetNamed("EH_ElementalSuppression");
   // Vanilla's random death-on-downing roll is disabled only during this harmless status change.
   // Real wounds, disease and all later attacks keep their normal death handling.
   float previous=victim.health.overrideDeathOnDownedChance;
   try {
    victim.health.overrideDeathOnDownedChance=0f;
    if(!victim.health.hediffSet.HasHediff(def))victim.health.AddHediff(def);
   } finally {victim.health.overrideDeathOnDownedChance=previous;}
   return true;
  }
 }
 // Auto combat accompanies normal melee jobs. Prevent the next sword/fist hit undoing restraint.
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class ElementalRestrainedMelee {
  [HarmonyPriority(Priority.First)]
  public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){
   Pawn source=__instance.CasterPawn,victim=__0.Pawn;
   if(victim==null||!ElementalNonLethal.Enabled(source))return true;
   ElementalNonLethal.TryApply(source,victim,10f);
   __result=new DamageWorker.DamageResult();
   return false;
  }
 }
}

