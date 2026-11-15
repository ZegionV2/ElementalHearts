using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 [HarmonyPatch(typeof(PawnRenderUtility),"DrawEquipmentAiming")]
 public static class TempestWeaponPresentation {
  public static void Prefix(Thing eq,ref Vector3 drawLoc,ref float aimAngle){
   if(eq==null||eq.def.defName!="EH_TempestIncarnateSword")return;var equipment=eq.TryGetComp<CompEquippable>();var pawn=equipment==null?null:equipment.PrimaryVerb.CasterPawn;var driver=pawn==null||pawn.jobs==null?null:pawn.jobs.curDriver as JobDriver_TempestTechnique;if(driver==null)return;
   if(driver.Skill==2)aimAngle=(Find.TickManager.TicksGame*32)%360;
   else if(driver.Skill==4&&driver.phase==2)aimAngle=Mathf.LerpAngle(20,180,Mathf.Clamp01(driver.ticks/12f));
   else if(driver.Skill==5){aimAngle=180;var at=pawn.DrawPos;drawLoc.x=at.x;drawLoc.z=at.z-.3f;}
  }
 }
}
