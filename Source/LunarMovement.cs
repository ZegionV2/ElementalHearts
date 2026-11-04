using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public static class LunarRules {
  public const int VictimLimit=12,LeapTicks=12,RushTicks=10;
  public static float Progress(int elapsed,int duration){
   float t=Math.Max(0,Math.Min(1,elapsed/(float)Math.Max(1,duration)));
   return t*t*(3f-2f*t);
  }
  public static int DashTicks(float distance){return Math.Max(5,Math.Min(9,(int)Math.Ceiling(distance*1.1f)));}
 }

 public static class LunarMovement {
  public static bool NearbyEnemy(Pawn pawn,float radius){
   foreach(var target in CelestialCombat.Targets(pawn,pawn.Position,radius,32))
    if(CelestialCombat.Hostile(pawn,target))return true;
   return false;
  }
  public static bool Pressured(Pawn pawn){
   int count=0;
   foreach(var target in CelestialCombat.Targets(pawn,pawn.Position,3f,32))
    if(CelestialCombat.Hostile(pawn,target)&&++count>=2)return true;
   return false;
  }
  public static bool DashLanding(Pawn pawn,Pawn target,out IntVec3 landing){
   if(CelestialCombat.Landing(pawn,pawn.Position,target,8f,out landing))return true;
   if(pawn.Position.DistanceToSquared(target.Position)>2)return false;
   landing=pawn.Position;return true;
  }
  public static bool PlanRush(Pawn pawn,out Pawn opponent,out IntVec3 leap,out IntVec3 end){
   opponent=null;leap=end=IntVec3.Invalid;
   if(pawn==null||!pawn.Spawned)return false;
   var enemies=new List<Pawn>();
   foreach(var target in CelestialCombat.Targets(pawn,pawn.Position,3f,32))
    if(CelestialCombat.Hostile(pawn,target))enemies.Add(target);
   if(enemies.Count<2)return false;
   enemies.Sort((a,b)=>pawn.Position.DistanceToSquared(a.Position).CompareTo(pawn.Position.DistanceToSquared(b.Position)));
   foreach(var target in enemies){
    IntVec3 retreat,landing;
    if(!CelestialCombat.Retreat(pawn,target,out retreat)||
     !CelestialCombat.Landing(pawn,retreat,target,10f,out landing))continue;
    opponent=target;leap=retreat;end=landing;return true;
   }
   return false;
  }
 }

 public sealed class MapComponent_LunarCombat : MapComponent {
  readonly List<Pawn> pawns=new List<Pawn>();
  public MapComponent_LunarCombat(Map map):base(map){}
  public void Register(Pawn pawn){if(pawn!=null&&pawns.Count<256&&!pawns.Contains(pawn))pawns.Add(pawn);}
  public void Forget(Pawn pawn){pawns.Remove(pawn);}
  public override void FinalizeInit(){foreach(var pawn in map.mapPawns.AllPawnsSpawned)if(HediffComp_LunarAbilities.On(pawn)!=null)Register(pawn);}
  public override void MapComponentTick(){
   if(LongEventHandler.ShouldWaitForEvent)return;
   int now=Find.TickManager.TicksGame;
   for(int i=pawns.Count-1;i>=0;i--){
    var pawn=pawns[i];var comp=HediffComp_LunarAbilities.On(pawn);
    if(comp==null||!pawn.Spawned||pawn.Map!=map||pawn.Dead){pawns.RemoveAt(i);continue;}
    comp.Advance(now);
   }
  }
 }

 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class LunarMovementDraw {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){
   if(___pawn.Dead||___pawn.Downed||!LunarAscendant.Busy(___pawn))return;
   var driver=___pawn.jobs.curDriver as JobDriver_LunarTechnique;
   if(driver==null||!driver.Moving||ElementalAnimationBridge.ExternallyControlled(___pawn))return;
   var position=driver.VisualPosition;position.y=__result.y;__result=position;
  }
 }
}
