using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 [HarmonyPatch(typeof(PawnRenderUtility),"DrawEquipmentAiming")]
 public static class AuroraWeaponPresentation {
  public static void Prefix(Thing eq,ref Vector3 drawLoc,ref float aimAngle){if(eq==null||eq.def.defName!="EH_AuroraIncarnateSword")return;var equipment=eq.TryGetComp<CompEquippable>();var pawn=equipment==null?null:equipment.PrimaryVerb.CasterPawn;var driver=pawn==null||pawn.jobs==null?null:pawn.jobs.curDriver as JobDriver_AuroraTechnique;if(driver==null)return;if(driver.Skill==2)aimAngle=(Find.TickManager.TicksGame*32)%360;else if(driver.Skill==3||driver.Skill==6){aimAngle=180;drawLoc.x=pawn.DrawPos.x;drawLoc.z=pawn.DrawPos.z-.25f;}}
 }
}
