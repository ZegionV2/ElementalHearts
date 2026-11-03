using System;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class EarthSeismicRules {
  public const float QuakeRadius=4f,SpireRadius=3f;
  public static bool GroundStab(EarthPower power){return power==EarthPower.Bastion||power==EarthPower.Mountainfall;}
  public static int Windup(EarthPower power){return power==EarthPower.Bastion?36:power==EarthPower.Mountainfall?42:6;}
  public static float StabProgress(int now,int start,int impact){return EarthRules.Smooth((now-start)/(float)Math.Max(1,impact-start));}
  public static float StabLift(float phase){return .22f+.35f*EarthRules.Smooth(phase/.6f)-.7f*EarthRules.Smooth((phase-.6f)/.4f);}
  public static float FramePhase(int age,int duration){return 7f*Math.Max(0f,Math.Min(1f,age/(float)Math.Max(1,duration-1)));}
 }
 // Render only the native weapon while the saved Earth job owns a ground stab.
 // External animation ownership takes priority; normal equipment drawing is restored with the job.
 [HarmonyPatch(typeof(PawnRenderUtility),"DrawEquipmentAiming")]
 public static class EarthGroundStabDrawing {
  public static bool Prefix(Thing __0){
   var tracker=__0==null?null:__0.ParentHolder as Pawn_EquipmentTracker;
   var p=tracker==null?null:tracker.pawn;
   var cast=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_EarthTechnique;
   if(cast==null||!cast.GroundStab||!p.Spawned||p.stances.stunner.Stunned||ElementalAnimationBridge.ExternallyControlled(p))return true;
   var at=p.DrawPos;at.x+=.27f;at.z+=EarthSeismicRules.StabLift(cast.StabPhase);at.y=AltitudeLayer.PawnState.AltitudeFor()+.02f;
   var size=__0.Graphic.drawSize;
   Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.AngleAxis(180f,Vector3.up),new Vector3(size.x,1f,size.y)),__0.Graphic.MatSingle,0);
   return false;
  }
 }
}
