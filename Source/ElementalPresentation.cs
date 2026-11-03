using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class ElementalIcons {
  public static readonly Texture2D Transform=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Transform");
  public static readonly Texture2D Hair=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/ElementalHair");
  public static readonly Texture2D AutoCombat=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/AutoCombat");
 }
 public static class ElementalCombatPolicy {
  public static bool TargetAllowed(bool explicitlySelected,bool friendlyFire,bool prisoner,bool hostile){
   return explicitlySelected || friendlyFire || (!prisoner && hostile);
  }
  public static bool AutomationEnabled(bool controlled,bool globalEnabled,bool pawnEnabled){
   return !controlled || (globalEnabled && pawnEnabled);
  }
  public static bool Controlled(Pawn pawn){return pawn!=null && pawn.IsPlayerControlled;}
  // Bounded search for a clear landing that increases separation from ALL nearby enemies.
  public static bool TryRetreat(Pawn pawn,Pawn threat,float range,Func<IntVec3,IntVec3,Map,bool> clear,out IntVec3 landing){
   landing=IntVec3.Invalid;
   if(pawn==null||threat==null||!pawn.Spawned||!threat.Spawned||pawn.Map!=threat.Map)return false;
   List<Pawn> enemies=new List<Pawn>();
   foreach(IntVec3 c in GenRadial.RadialCellsAround(pawn.Position,10f,true)){
    if(!c.InBounds(pawn.Map))continue;
    foreach(Thing t in c.GetThingList(pawn.Map)){var p=t as Pawn;if(p!=null&&!p.Dead&&!p.Downed&&p.HostileTo(pawn))enemies.Add(p);}
   }
   if(!enemies.Contains(threat))enemies.Add(threat);
   int before=enemies.Min(e=>pawn.Position.DistanceToSquared(e.Position));float best=-1f;
   foreach(IntVec3 c in GenRadial.RadialCellsAround(pawn.Position,Math.Min(5f,range),false)){
    if(pawn.Position.DistanceToSquared(c)<9||!c.InBounds(pawn.Map)||!c.Standable(pawn.Map)||!clear(pawn.Position,c,pawn.Map))continue;
    if(c.GetThingList(pawn.Map).Any(t=>t is Pawn || t is Fire))continue;
    int separation=enemies.Min(e=>c.DistanceToSquared(e.Position));
    if(separation<9||separation<=before)continue;
    float score=separation-.1f*pawn.Position.DistanceToSquared(c);
    if(score>best){best=score;landing=c;}
   }
   return landing.IsValid;
  }
 }
 // Only the explicitly clicked primary target bypasses faction filtering.
 // Delayed ultimate jobs carry this intent through their saved playerForced/targetA fields.
 public sealed class ElementalManualTarget : IDisposable {
  [ThreadStatic] private static Pawn caster;
  [ThreadStatic] private static Pawn target;
  private readonly Pawn previousCaster,previousTarget;
  public ElementalManualTarget(Pawn source,Pawn selected){previousCaster=caster;previousTarget=target;caster=source;target=selected;}
  public void Dispose(){caster=previousCaster;target=previousTarget;}
  public static bool Explicit(Pawn source,Pawn victim){
   if(source==null||victim==null)return false;
   if(caster==source&&target==victim)return true;
   Job job=source.jobs==null?null:source.CurJob;
   return job!=null&&job.playerForced&&job.targetA.Pawn==victim&&
    (job.def.defName=="EH_HeavenlyThunder"||job.def.defName=="EH_PhoenixDescent"||job.def.defName=="EH_WindTechnique"||job.def.defName=="EH_EarthTechnique");
  }
 }
 public static class ElementalStylePolicy {
  public static bool Reserved(StyleItemDef style){
   return style!=null && (style.defName=="EH_LightningHair"||style.defName=="EH_LightningHairShort"||style.defName=="EH_FlameHair"||style.defName=="EH_WindHair");
  }
 }
 [HarmonyPatch(typeof(PawnStyleItemChooser),"WantsToUseStyle")]
 public static class ElementalNoRandomHair {
  public static void Postfix(StyleItemDef __1,ref bool __result){if(ElementalStylePolicy.Reserved(__1))__result=false;}
 }
 [HarmonyPatch(typeof(PawnStyleItemChooser),"TotalStyleItemLikelihood")]
 public static class ElementalNoRandomHairWeight {
  public static void Postfix(StyleItemDef __0,ref float __result){if(ElementalStylePolicy.Reserved(__0))__result=0f;}
 }
}

