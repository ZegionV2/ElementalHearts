using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class MoonSwordPresentation {
  public const float Enlargement=1.30f;
  public static bool Supported(Thing eq){return eq!=null&&eq.def!=null&&(eq.def.defName=="EH_LunarSword"||eq.def.defName=="EH_EclipseSword");}
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
  // Reuse the game's mesh, material, angle, flip and recoil logic; change only the
  // held transform. Inventory/ground icons are untouched. AM uses its own profile.
  public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions){
   var native=AccessTools.Method(typeof(Matrix4x4),"TRS",new[]{typeof(Vector3),typeof(Quaternion),typeof(Vector3)});
   var adjusted=AccessTools.Method(typeof(MoonSwordPresentation),"HeldMatrix");
   foreach(var instruction in instructions){
    if(instruction.Calls(native)){var load=new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0);load.labels.AddRange(instruction.labels);load.blocks.AddRange(instruction.blocks);yield return load;yield return new CodeInstruction(System.Reflection.Emit.OpCodes.Call,adjusted);}
    else yield return instruction;
   }
  }
 }
}
