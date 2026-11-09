using System;using System.Reflection;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class IncarnateMeleeClash {
  public static void Swing(Pawn source,IntVec3 cell){var c=HediffComp_ThunderIncarnate.On(source);if(c==null||!source.Spawned||!HediffComp_ElementalTransform.Active(source)||ThunderIncarnate.Busy(source)||!cell.InBounds(source.Map))return;int now=Find.TickManager.TicksGame;if(now-c.lastMeleeVisualTick<4)return;c.lastMeleeVisualTick=now;var at=cell.ToVector3Shifted();IncarnateVfx.Burst(source,15,at,3.4f,18);IncarnateVfx.StormArcs(source,at,1.7f,6);ElementalAudio.Ability(source,ElementalSound.Slash);}
 }
 [HarmonyPatch(typeof(Verb_MeleeAttack),"TryCastShot")]
 public static class IncarnateOrdinarySlash {
  public static void Prefix(Verb_MeleeAttack __instance,out IntVec3 __state){__state=ThunderIncarnate.Active(__instance.CasterPawn)?__instance.CurrentTarget.Cell:IntVec3.Invalid;}
  public static void Postfix(Verb_MeleeAttack __instance,bool __result,IntVec3 __state){if(__result&&__state.IsValid)IncarnateMeleeClash.Swing(__instance.CasterPawn,__state);}
 }
 [HarmonyPatch]
 public static class IncarnateAnimatedSlash {
  static MethodBase method;
  public static bool Prepare(){var type=AccessTools.TypeByName("AM.Idle.IdleControllerComp");method=type==null?null:AccessTools.Method(type,"NotifyPawnDidMeleeAttack");return method!=null;}
  public static MethodBase TargetMethod(){return method;}
  public static void Postfix(object __instance,Thing __0,Verb_MeleeAttack __1){if(__1==null||__0==null)return;var comp=__instance as ThingComp;var p=comp==null?null:comp.parent as Pawn;if(p!=null&&__0.Spawned&&__0.Map==p.Map)IncarnateMeleeClash.Swing(p,__0.Position);}
 }
}
