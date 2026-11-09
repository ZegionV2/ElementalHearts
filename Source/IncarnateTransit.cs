using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;
namespace ElementalHearts {
 // Both light lineages share the tested container, map preparation and rollback path.
 public static class IncarnateTransit {
  public const string Depart="EH_ThunderTravelDepart",Arrive="EH_ThunderTravelArrive";
  public static bool Active(Pawn p){return CelestialAscendant.Active(p)||ThunderIncarnate.Active(p);}
  public static bool Thunder(Pawn p){return ThunderIncarnate.Active(p);}
  public static Texture2D Icon(Pawn p){return Thunder(p)?ThunderStep.Icon(true):CelestialVisualAssets.Icon(6);}
  public static Texture2D Icon(bool thunder){return thunder?ThunderStep.Icon(true):CelestialVisualAssets.Icon(6);}
  public static PlanetTile Destination(Pawn p){var thunder=HediffComp_ThunderIncarnate.On(p);if(thunder!=null)return thunder.travelDestination;var solar=HediffComp_CelestialAbilities.On(p);return solar==null?PlanetTile.Invalid:solar.travelDestination;}
  public static void Destination(Pawn p,PlanetTile tile){var thunder=HediffComp_ThunderIncarnate.On(p);if(thunder!=null){thunder.travelDestination=tile;return;}var solar=HediffComp_CelestialAbilities.On(p);if(solar!=null)solar.travelDestination=tile;}
  public static string DepartJob(Pawn p){return Thunder(p)?Depart:CelestialAscendant.Depart;}
  public static string ArriveJob(Pawn p){return Thunder(p)?Arrive:CelestialAscendant.Arrive;}
  public static int Emit(Pawn p,CelestialEffect effect,int duration,float width,float height,int elapsed=0){if(!Thunder(p))return CelestialVfx.Emit(p,effect,p.DrawPos,p.DrawPos,duration,width,height,null,elapsed);if(p==null||!p.Spawned)return 0;var controller=p.Map.GetComponent<MapComponent_IncarnateVfx>();bool beam=effect==CelestialEffect.DepartureBeam||effect==CelestialEffect.ArrivalBeam;return controller.Add(beam?11:14,p.DrawPos,p.DrawPos,beam?width*1.7f:width,beam?height*1.7f:height,System.Math.Max(1,duration-elapsed),0,false,Mathf.Clamp01(elapsed/(float)duration),effect==CelestialEffect.TravelMotes?null:p);}
  public static void Stop(Pawn p,int id){if(p==null||!p.Spawned)return;if(Thunder(p))p.Map.GetComponent<MapComponent_IncarnateVfx>().Stop(id);else p.Map.GetComponent<MapComponent_CelestialVfx>().Stop(id);}
 }
}
