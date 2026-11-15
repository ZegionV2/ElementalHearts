using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class HellUtility {
  public const string Depart="EH_HellTravelDepart",Arrive="EH_HellTravelArrive";
  public static int Emit(Pawn p,CelestialEffect effect,int duration,float width,float height,int elapsed){if(p==null||!p.Spawned)return 0;int sprite=effect==CelestialEffect.DepartureBeam?16:effect==CelestialEffect.ArrivalBeam?17:effect==CelestialEffect.TravelMotes?19:18;return p.Map.GetComponent<MapComponent_HellVfx>().Add(sprite,p.DrawPos,p.DrawPos,width,height,System.Math.Max(1,duration-elapsed),0,false,Mathf.Clamp01(elapsed/(float)duration),p);}
 }
}
