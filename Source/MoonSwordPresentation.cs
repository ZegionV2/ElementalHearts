using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class MoonSwordPresentation {
  public const float Enlargement=1.30f;
  public static bool Supported(Thing eq){return eq!=null&&eq.def!=null&&(eq.def.defName=="EH_LunarSword"||eq.def.defName=="EH_EclipseSword");}
  public static Mesh HeldMesh(Mesh mesh,Thing eq){if(eq!=null&&eq.def!=null&&eq.def.defName=="EH_ThunderIncarnateSword"){if(mesh==MeshPool.plane10)return MeshPool.plane10Flip;if(mesh==MeshPool.plane10Flip)return MeshPool.plane10;}return mesh;}
  public static Matrix4x4 HeldMatrix(Vector3 position,Quaternion rotation,Vector3 scale,Thing eq){
   if(!Supported(eq))return Matrix4x4.TRS(position,rotation,scale);
   // Native PNG grip landmarks: keep the same point in the hand as the blade grows.
   float gripY=eq.def.defName=="EH_LunarSword"?209f:201f;
   var grip=new Vector3(0,0,(.5f-gripY/256f)*scale.z);
   position+=rotation*(grip*(1-Enlargement));
   scale.x*=Enlargement;scale.z*=Enlargement;
   return Matrix4x4.TRS(position,rotation,scale);
  }
 }
 [HarmonyPatch(typeof(PawnRenderUtility),"DrawEquipmentAiming")]
 public static class HeldMoonSwordScale {
  // Preserve native angle, recoil and material. Only the Incarnate katana swaps
  // native UV mirrors, keeping winding and grip intact. Moon sword scale stays fitted.
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions){
   var native=AccessTools.Method(typeof(Matrix4x4),"TRS",new[]{typeof(Vector3),typeof(Quaternion),typeof(Vector3)});
   var adjusted=AccessTools.Method(typeof(MoonSwordPresentation),"HeldMatrix");
   var mesh=AccessTools.Method(typeof(MoonSwordPresentation),"HeldMesh");
   var normal=AccessTools.Field(typeof(MeshPool),"plane10");var flipped=AccessTools.Field(typeof(MeshPool),"plane10Flip");
   foreach(var instruction in instructions){
    if(instruction.Calls(native)){var load=new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);load.blocks.AddRange(instruction.blocks);yield return load;yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Call,adjusted);}
    else{yield return instruction;if(instruction.LoadsField(normal)||instruction.LoadsField(flipped)){yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Call,mesh);}}
   }
  }
 }
}
