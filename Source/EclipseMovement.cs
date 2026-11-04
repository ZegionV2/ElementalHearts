using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class EclipseMovement {
  public static int TravelTicks(IntVec3 from,IntVec3 to){return Math.Max(2,Math.Min(7,(int)Math.Ceiling((to-from).LengthHorizontal)));}
  public static bool MotionClear(Pawn pawn,IntVec3 from,IntVec3 to,int ticks){
   if(!CelestialCombat.Path(pawn,from,to))return false;
   var prior=from;
   for(int i=1;i<=ticks;i++){var cell=Vector3.Lerp(from.ToVector3Shifted(),to.ToVector3Shifted(),LunarRules.Progress(i,ticks)).ToIntVec3();if(!CelestialCombat.Path(pawn,prior,cell))return false;prior=cell;}
   return true;
  }
  public static bool Landing(Pawn pawn,IntVec3 from,Pawn target,float range,int ticks,out IntVec3 landing){
   landing=IntVec3.Invalid;if(!CelestialCombat.Hostile(pawn,target))return false;int best=int.MaxValue;
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){if(x==0&&z==0)continue;var cell=target.Position+new IntVec3(x,0,z);int distance=from.DistanceToSquared(cell);if(distance==0||distance>range*range||distance>=best||!CelestialCombat.Free(pawn,cell)||!LightningCombat.ClearLine(cell,target.Position,pawn.Map)||!MotionClear(pawn,from,cell,ticks))continue;landing=cell;best=distance;}
   return landing.IsValid;
  }
  public static bool Pressured(Pawn pawn,float radius){int count=0;foreach(var enemy in CelestialCombat.Targets(pawn,pawn.Position,radius,32))if(CelestialCombat.Hostile(pawn,enemy)&&++count>=2)return true;return false;}
  public static bool PlanPursuit(Pawn pawn,Pawn target,out IntVec3 waypoint,out IntVec3 landing){
   waypoint=landing=IntVec3.Invalid;if(!CelestialCombat.Hostile(pawn,target)||pawn.Position.DistanceToSquared(target.Position)>100)return false;
   float best=float.MaxValue;var facing=target.Rotation.FacingCell;
   // Eight landing cells and eight bounded outer waypoints. Never path through the victim.
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){
    if(x==0&&z==0)continue;var end=target.Position+new IntVec3(x,0,z);
    if(!CelestialCombat.Free(pawn,end)||!LightningCombat.ClearLine(end,target.Position,pawn.Map))continue;
    float rear=x*facing.x+z*facing.z,preference=rear<0?0:rear==0?20:40;
    if(end==pawn.Position){float score=preference+1;if(score<best){best=score;waypoint=landing=end;}continue;}
    if(MotionClear(pawn,pawn.Position,end,TravelTicks(pawn.Position,end))){
     float length=(end-pawn.Position).LengthHorizontal;
     if(length<=12&&length+preference<best){best=length+preference;waypoint=pawn.Position;landing=end;}
    }
    for(int wx=-1;wx<=1;wx++)for(int wz=-1;wz<=1;wz++){
     if(wx==0&&wz==0)continue;var via=target.Position+new IntVec3(wx*2,0,wz*2);
     float length=(via-pawn.Position).LengthHorizontal+(end-via).LengthHorizontal;
     if(length>12||length+preference>=best||!CelestialCombat.Free(pawn,via)||!MotionClear(pawn,pawn.Position,via,TravelTicks(pawn.Position,via))||!MotionClear(pawn,via,end,TravelTicks(via,end)))continue;
     best=length+preference;waypoint=via;landing=end;
    }
   }
   return landing.IsValid;
  }
  public static bool PlanReversal(Pawn pawn,out Pawn opponent,out IntVec3 leap,out IntVec3 end){
   opponent=null;leap=end=IntVec3.Invalid;if(pawn==null||!pawn.Spawned||!Pressured(pawn,4))return false;
   var enemies=CelestialCombat.Targets(pawn,pawn.Position,4,32);
   enemies.Sort((a,b)=>pawn.Position.DistanceToSquared(a.Position).CompareTo(pawn.Position.DistanceToSquared(b.Position)));
   foreach(var target in enemies){if(!CelestialCombat.Hostile(pawn,target))continue;IntVec3 retreat,landing;if(!CelestialCombat.Retreat(pawn,target,out retreat)||!MotionClear(pawn,pawn.Position,retreat,12)||!Landing(pawn,retreat,target,12,9,out landing))continue;opponent=target;leap=retreat;end=landing;return true;}return false;
  }
 }
 public static class EclipseCombat {
  public static bool Strike(Pawn source,Pawn target,float multiplier){
   if(!CelestialCombat.Hostile(source,target)||WaterDefense.TryParry(source,target))return false;
   var profile=LightningCombat.Profile(source,10);float damage=profile.damage*multiplier*(EclipseSky.Empowered(source)?1.15f:1f);
   if(ElementalNonLethal.TryApply(source,target,damage))return CelestialCombat.Hostile(source,target);
   var result=target.TakeDamage(new DamageInfo(DamageDefOf.Cut,damage,Math.Max(.5f,profile.penetration+.25f),-1,source,null,profile.weapon));
   return result!=null&&result.totalDamageDealt>0;
  }
 }
 public sealed class MapComponent_EclipseCombat:MapComponent {
  readonly List<Pawn> pawns=new List<Pawn>();
  public MapComponent_EclipseCombat(Map map):base(map){}
  public void Register(Pawn p){if(p!=null&&pawns.Count<256&&!pawns.Contains(p))pawns.Add(p);}
  public void Forget(Pawn p){pawns.Remove(p);}
  public override void FinalizeInit(){foreach(var p in map.mapPawns.AllPawnsSpawned)if(EclipseAscendant.Active(p))Register(p);}
  public override void MapComponentTick(){if(LongEventHandler.ShouldWaitForEvent)return;int now=Find.TickManager.TicksGame;for(int i=pawns.Count-1;i>=0;i--){var p=pawns[i];var c=HediffComp_EclipseAbilities.On(p);if(c==null||!p.Spawned||p.Map!=map||p.Dead){pawns.RemoveAt(i);continue;}c.Advance(now);}}
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class EclipseMovementDraw {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(___pawn.Dead||___pawn.Downed||!EclipseAscendant.Busy(___pawn)||ElementalAnimationBridge.ExternallyControlled(___pawn))return;var driver=___pawn.jobs.curDriver as JobDriver_EclipseTechnique;if(driver!=null&&driver.Moving){var at=driver.VisualPosition;at.y=__result.y;__result=at;}}
 }
}
