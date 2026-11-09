using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ElementalHearts {
 // Common, whole-body benefits: independent of the manifested outfit and safe in caravans.
 public static class AscendantBenefits {
  static HashSet<HediffDef> diseases;
  public static float CarryFactor(Pawn pawn){return !ElementalHeartUtility.HasHeart(pawn)?1f:(CelestialAscendant.Active(pawn)||ThunderIncarnate.Active(pawn)||EclipseSky.Empowered(pawn))?15f:EvolvedLineage.Active(pawn)?10f:5f;}
  public static float TravelFactor(List<Pawn> pawns){
   if(pawns==null)return 1f;float total=0;int count=0;
   foreach(var pawn in pawns){if(pawn==null||pawn.Dead||pawn.Downed||!pawn.RaceProps.Humanlike)continue;count++;total+=!ElementalHeartUtility.HasHeart(pawn)?1f:(CelestialAscendant.Active(pawn)||ThunderIncarnate.Active(pawn)||EclipseSky.Empowered(pawn))?4f:EvolvedLineage.Active(pawn)?3f:2f;}
   return count==0?1f:total/count;
  }
  public static bool Disease(HediffDef def){
   if(def==null||!def.isBad)return false;
   if(diseases==null){
    diseases=new HashSet<HediffDef>();
    foreach(var incident in DefDatabase<IncidentDef>.AllDefsListForReading)if(incident.diseaseIncident!=null)diseases.Add(incident.diseaseIncident);
    foreach(var h in DefDatabase<HediffDef>.AllDefsListForReading){
     if(h.makesSickThought||h.defName=="FoodPoisoning"||h.defName=="LungRotExposure"||h.defName=="Scaria"||h.defName=="ScariaInfection")diseases.Add(h);
     if(h.comps!=null)foreach(var comp in h.comps)if(comp is HediffCompProperties_Immunizable){diseases.Add(h);break;}
    }
   }
   return diseases.Contains(def);
  }
  public static bool Reject(Pawn pawn,HediffDef def){
   if(def==null||!ElementalHeartUtility.HasHeart(pawn))return false;
   // Melee Animation applies this specific condition for its non-lethal execution outcome.
   // No hard dependency, execution patch, generic stun immunity or death/downing override.
   return Disease(def)||(def.defName=="AM_KnockedOut"&&EvolvedLineage.Active(pawn));
  }
  public static void Cleanse(Pawn pawn){
   if(!ElementalHeartUtility.HasHeart(pawn))return;
   for(int i=pawn.health.hediffSet.hediffs.Count-1;i>=0;i--){var h=pawn.health.hediffSet.hediffs[i];if(Reject(pawn,h.def))pawn.health.RemoveHediff(h);}
  }
 }
 [HarmonyPatch(typeof(Pawn_HealthTracker),"AddHediff",new Type[]{typeof(Hediff),typeof(BodyPartRecord),typeof(DamageInfo?),typeof(DamageWorker.DamageResult)})]
 public static class AscendantHealthProtection {
  public static bool Prefix(Pawn ___pawn,Hediff hediff){return hediff==null||!AscendantBenefits.Reject(___pawn,hediff.def);}
 }
 [HarmonyPatch(typeof(IncidentWorker_Disease),"PotentialVictims")]
 public static class AscendantDiseaseVictims {
  public static void Postfix(ref IEnumerable<Pawn> __result){__result=WithoutAscendants(__result);}
  static IEnumerable<Pawn> WithoutAscendants(IEnumerable<Pawn> original){foreach(var pawn in original)if(!ElementalHeartUtility.HasHeart(pawn))yield return pawn;}
 }
 [HarmonyPatch(typeof(MassUtility),"Capacity")]
 public static class AscendantCaravanCapacity {
  public static void Postfix(Pawn p,StringBuilder explanation,ref float __result){
   float factor=AscendantBenefits.CarryFactor(p);if(factor<=1f||__result<=0)return;float extra=__result*(factor-1f);__result+=extra;
   if(explanation!=null)explanation.AppendLine().Append("    Ascendant carrying power (x").Append(factor.ToString("0")).Append("): ").Append(extra.ToStringMassOffset());
  }
 }
 [HarmonyPatch(typeof(CaravanTicksPerMoveUtility),"GetTicksPerMove",new Type[]{typeof(List<Pawn>),typeof(float),typeof(float),typeof(bool),typeof(StringBuilder)})]
 public static class AscendantCaravanTravel {
  public static void Postfix(List<Pawn> pawns,bool isShuttle,StringBuilder explanation,ref int __result){
   if(isShuttle||__result<=0)return;float factor=AscendantBenefits.TravelFactor(pawns);if(factor<=1f)return;
   __result=Math.Max(1,(int)Math.Round(__result/factor));
   if(explanation!=null)explanation.AppendLine().Append("Ascendant travel speed: x").Append(factor.ToString("0.##")).Append(" (average of walking humanlike members)");
  }
 }
}
