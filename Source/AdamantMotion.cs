using HarmonyLib;using UnityEngine;using Verse;
namespace ElementalHearts {
 // Cosmetic interpolation only: the checked destination is already committed by gameplay.
 // No victim control/job ownership, delayed damage or save state is created by this draw layer.
 public sealed class MapComponent_EarthPushVisual:MapComponent {
  struct Push {public Pawn pawn;public IntVec3 end;public Vector3 start;public int tick;}
  readonly Push[] pushes=new Push[32];int cursor,until;public bool Active{get{return Find.TickManager.TicksGame<until;}}
  public MapComponent_EarthPushVisual(Map map):base(map){}
  public void Track(Pawn p,IntVec3 from,IntVec3 to){until=Find.TickManager.TicksGame+10;pushes[cursor]=new Push{pawn=p,start=from.ToVector3Shifted(),end=to,tick=Find.TickManager.TicksGame};cursor=(cursor+1)%pushes.Length;}
  public void Draw(Pawn p,ref Vector3 at){int now=Find.TickManager.TicksGame;for(int i=0;i<pushes.Length;i++){var v=pushes[i];if(v.pawn==null)continue;if(now-v.tick>=10||!v.pawn.Spawned||v.pawn.Map!=map||v.pawn.Dead||v.pawn.Downed||v.pawn.Position!=v.end){pushes[i]=new Push();continue;}if(v.pawn!=p)continue;float t=EarthRules.Smooth((now-v.tick)/10f);var pos=Vector3.Lerp(v.start,v.end.ToVector3Shifted(),t);pos.y=at.y;at=pos;return;}}
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")] public static class AdamantPushDrawing {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){var p=___pawn;if(p==null||!p.Spawned||!p.Map.GetComponent<MapComponent_EarthPushVisual>().Active||ElementalAnimationBridge.ExternallyControlled(p))return;p.Map.GetComponent<MapComponent_EarthPushVisual>().Draw(p,ref __result);}
 }
}
