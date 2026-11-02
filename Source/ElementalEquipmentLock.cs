using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class ElementalEquipmentLock {
  public static bool Policy(bool transformed,bool internalChange,bool dead){return transformed&&!internalChange&&!dead;}
  public static bool Locked(Pawn p){
   if(p==null)return false;var c=HediffComp_ElementalTransform.On(p);
   return c!=null&&Policy(c.transformed,c.changingEquipment,p.Dead);
  }
  public static bool ChangeJob(string name){return name=="Wear"||name=="Equip"||name=="RemoveApparel"||name=="DropEquipment";}
  public static bool Blocks(Pawn p,Job job){return job!=null&&job.def!=null&&Locked(p)&&ChangeJob(job.def.defName);}
 }
 // Expose the native apparel lock to gear tabs, outfit logic and strip/drop checks.
 [HarmonyPatch(typeof(Pawn_ApparelTracker),"IsLocked")]
 public static class TransformApparelLocked {
  public static void Postfix(Pawn ___pawn,ref bool __result){if(ElementalEquipmentLock.Locked(___pawn))__result=true;}
 }
 [HarmonyPatch(typeof(Pawn_ApparelTracker),"WouldReplaceLockedApparel")]
 public static class TransformNoNewApparel {
  public static void Postfix(Pawn ___pawn,ref bool __result){if(ElementalEquipmentLock.Locked(___pawn))__result=true;}
 }
 [HarmonyPatch]
 public static class TransformNoApparelDrop {
  static MethodBase TargetMethod(){return AccessTools.Method(typeof(Pawn_ApparelTracker),"TryDrop",new[]{typeof(Apparel),typeof(Apparel).MakeByRefType(),typeof(IntVec3),typeof(bool)});}
  public static bool Prefix(Pawn ___pawn,ref Apparel resultingAp,ref bool __result){if(!ElementalEquipmentLock.Locked(___pawn))return true;resultingAp=null;__result=false;return false;}
 }
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"TryDropEquipment")]
 public static class TransformNoWeaponDrop {
  public static bool Prefix(Pawn ___pawn,ref ThingWithComps resultingEq,ref bool __result){if(!ElementalEquipmentLock.Locked(___pawn))return true;resultingEq=null;__result=false;return false;}
 }
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"TryTransferEquipmentToContainer")]
 public static class TransformNoWeaponTransfer {
  public static bool Prefix(Pawn ___pawn,ref bool __result){if(!ElementalEquipmentLock.Locked(___pawn))return true;__result=false;return false;}
 }
 [HarmonyPatch(typeof(JobGiver_OptimizeApparel),"TryGiveJob")]
 public static class TransformIgnoreOutfitChanges {
  public static bool Prefix(Pawn pawn,ref Job __result){if(!ElementalEquipmentLock.Locked(pawn))return true;__result=null;return false;}
 }
 [HarmonyPatch(typeof(Pawn_JobTracker),"TryTakeOrderedJob")]
 public static class TransformRejectGearOrder {
  public static bool Prefix(Pawn ___pawn,Job job,ref bool __result){
   if(!ElementalEquipmentLock.Blocks(___pawn,job))return true;
   Messages.Message("Revert the elemental transformation before changing apparel or weapons.",___pawn,MessageTypeDefOf.RejectInput,false);__result=false;return false;
  }
 }
 // Also catch queued jobs and ordinary outfit jobs started by incidents or AI.
 [HarmonyPatch(typeof(Pawn_JobTracker),"StartJob")]
 public static class TransformRejectGearJob {
  public static bool Prefix(Pawn ___pawn,Job newJob){return !ElementalEquipmentLock.Blocks(___pawn,newJob);}
 }
}
