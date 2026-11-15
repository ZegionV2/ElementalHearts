using System;using System.Reflection;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class TempestMeleeClash {
  public static void Swing(Pawn source,IntVec3 cell){var c=HediffComp_TempestIncarnate.On(source);if(c==null||!source.Spawned||!HediffComp_ElementalTransform.Active(source)||TempestIncarnate.Busy(source)||!cell.InBounds(source.Map))return;int now=Find.TickManager.TicksGame;if(now-c.lastMeleeVisualTick<4)return;c.lastMeleeVisualTick=now;var at=cell.ToVector3Shifted();TempestVfx.Burst(source,15,at,3.4f,18);TempestVfx.Line(source,1,source.DrawPos,at,16,.65f);ElementalAudio.Ability(source,ElementalSound.Slash);}
 }
 [HarmonyPatch(typeof(Verb_MeleeAttack),"TryCastShot")]
 public static class TempestOrdinarySlash {
  public static void Prefix(Verb_MeleeAttack __instance,out IntVec3 __state){__state=TempestIncarnate.Active(__instance.CasterPawn)?__instance.CurrentTarget.Cell:IntVec3.Invalid;}
  public static void Postfix(Verb_MeleeAttack __instance,bool __result,IntVec3 __state){if(__result&&__state.IsValid)TempestMeleeClash.Swing(__instance.CasterPawn,__state);}
 }
 [HarmonyPatch]
 public static class TempestAnimatedSlash {
  static MethodBase method;
  public static bool Prepare(){var type=AccessTools.TypeByName("AM.Idle.IdleControllerComp");method=type==null?null:AccessTools.Method(type,"NotifyPawnDidMeleeAttack");return method!=null;}
  public static MethodBase TargetMethod(){return method;}
  public static void Postfix(object __instance,Thing __0,Verb_MeleeAttack __1){if(__1==null||__0==null)return;var comp=__instance as ThingComp;var p=comp==null?null:comp.parent as Pawn;if(p!=null&&__0.Spawned&&__0.Map==p.Map)TempestMeleeClash.Swing(p,__0.Position);}
 }
}
