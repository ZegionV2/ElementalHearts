using HarmonyLib;using UnityEngine;using Verse;
namespace ElementalHearts {
 // The map portion rises into the departure column and descends during arrival;
 // the original pawn then continues in its saved world-map vessel.
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class CelestialFlightPresentation {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){
   if(___pawn==null||___pawn.jobs==null||!___pawn.Spawned||___pawn.Dead||___pawn.Downed)return;
   var depart=___pawn.jobs.curDriver as JobDriver_CelestialDepart;if(depart!=null){__result.z+=LivingFlightAssets.Lift(depart.elapsed,true);return;}
   var arrive=___pawn.jobs.curDriver as JobDriver_CelestialArrive;if(arrive!=null)__result.z+=LivingFlightAssets.Lift(arrive.elapsed,false);
   var passenger=___pawn.jobs.curDriver as JobDriver_LivingPassengerArrive;if(passenger!=null)__result.z+=LivingFlightAssets.Lift(passenger.elapsed,false);
   var anchor=___pawn.CurJob==null?null:___pawn.CurJob.targetA.Thing as Building_LivingTransport;if(anchor!=null&&anchor.launching&&anchor.captain!=___pawn&&anchor.captain!=null){var lead=anchor.captain.jobs.curDriver as JobDriver_CelestialDepart;if(lead!=null)__result.z+=LivingFlightAssets.Lift(lead.elapsed,true);}
  }
 }
}
