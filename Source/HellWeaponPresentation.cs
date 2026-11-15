using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 [HarmonyPatch(typeof(PawnRenderUtility),"DrawEquipmentAiming")]
 public static class HellWeaponPresentation {
  public static void Prefix(Thing eq,ref Vector3 drawLoc,ref float aimAngle){
   if(eq==null||eq.def.defName!="EH_HellFlameIncarnateSword")return;var equipment=eq.TryGetComp<CompEquippable>();var pawn=equipment==null?null:equipment.PrimaryVerb.CasterPawn;var driver=pawn==null||pawn.jobs==null?null:pawn.jobs.curDriver as JobDriver_HellTechnique;if(driver==null)return;
   if(driver.Skill==2)aimAngle=(Find.TickManager.TicksGame*32)%360;
   else if(driver.Skill==5&&driver.phase==0){aimAngle=Mathf.LerpAngle(30,180,Mathf.Clamp01(driver.ticks/12f));var at=pawn.DrawPos;drawLoc.x=at.x;drawLoc.z=at.z-.35f;}
   else if(driver.Skill==3&&driver.phase==2)aimAngle=Mathf.LerpAngle(0,180,Mathf.Clamp01(driver.ticks/14f));
  }
 }
}
