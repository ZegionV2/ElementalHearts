using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts.Patches {
 // Restore only the efficiency lost to non-permanent injuries. Missing parts,
 // scars, disease stage penalties, prosthetic efficiencies and other modifiers remain.
 [HarmonyPatch(typeof(PawnCapacityUtility),"CalculatePartEfficiency")]
 public static class FlameInjuryEfficiency {
  public static void Postfix(HediffSet diffSet,BodyPartRecord part,ref float __result){
   if(diffSet.pawn.Dead||(HediffComp_FlameAbilities.On(diffSet.pawn)==null&&!CelestialAscendant.Active(diffSet.pawn))||diffSet.PartIsMissing(part)||diffSet.AncestorHasDirectlyAddedParts(part))return;
   float injuryDamage=0f,baseEfficiency=1f,offset=0f;
   foreach(Hediff h in diffSet.hediffs){
    if(h.Part!=part)continue;
    if(h.CurStage!=null){if(h.CurStage.partIgnoreMissingHP)return;offset+=h.CurStage.partEfficiencyOffset;}
    if(h is Hediff_AddedPart)baseEfficiency*=h.def.addedPartProps.partEfficiency;
    var injury=h as Hediff_Injury;if(injury!=null&&!injury.IsPermanent())injuryDamage+=injury.Severity;
   }
   if(injuryDamage<=0f)return;
   float max=part.def.GetMaxHealth(diffSet.pawn),health=diffSet.GetPartHealth(part);
   float before=Mathf.Clamp01(health/max),after=Mathf.Clamp01((health+injuryDamage)/max);
   if(DamageWorker_AddInjury.ShouldReduceDamageToPreservePart(part)){before=Mathf.InverseLerp(.1f,1f,before);after=Mathf.InverseLerp(.1f,1f,after);}
   __result+=Math.Max(0f,baseEfficiency*after+offset)-Math.Max(0f,baseEfficiency*before+offset);
  }
 }
 [HarmonyPatch(typeof(StaggerHandler),"StaggerFor")]
 public static class FlameNoHitSlowdown {
  public static bool Prefix(Pawn ___parent,ref bool __result){if(HediffComp_FlameAbilities.On(___parent)==null&&!CelestialAscendant.Active(___parent))return true;__result=false;return false;}
 }
}





