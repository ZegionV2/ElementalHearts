using System;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class TempestDragonVfx {
  static readonly Material[] materials=new Material[8];
  public static Material Material(int frame){return materials[frame]??(materials[frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Thunderstorm/TempestDragon/Dragon_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  public static void Trace(Pawn pawn,ref int handle,Vector3 from,Vector3 to){if(!StormBlitzFx.Visible(pawn.Map,from,to))return;var fx=pawn.Map.GetComponent<MapComponent_TempestDragonVfx>();if(!fx.Extend(handle,to))handle=fx.Begin(from,to,Find.TickManager.TicksGame);}
 }
 public sealed class MapComponent_TempestDragonVfx:MapComponent {
  struct Trail{public bool active;public int token,tick;public Vector3 from,to;}
  readonly Trail[] trails=new Trail[128];int serial;MaterialPropertyBlock block;
  public MapComponent_TempestDragonVfx(Map map):base(map){}
  public int Begin(Vector3 from,Vector3 to,int now){int token=serial++;if(serial==int.MaxValue)serial=0;trails[token%trails.Length]=new Trail{active=true,token=token,tick=now,from=from,to=to};return token;}
  public bool Extend(int token,Vector3 to){if(token<0)return false;int slot=token%trails.Length;if(!trails[slot].active||trails[slot].token!=token)return false;trails[slot].to=to;return true;}
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
   for(int i=0;i<trails.Length;i++){var trail=trails[i];if(!trail.active)continue;int age=now-trail.tick;if(age>=TempestDragonRules.TrailLifetime){trails[i].active=false;continue;}var at=(trail.from+trail.to)*.5f;if(!visible.Contains(at.ToIntVec3())||!at.ToIntVec3().InBounds(map)||at.ToIntVec3().Fogged(map))continue;var delta=trail.to-trail.from;if(delta.sqrMagnitude<.01f)continue;at.y=AltitudeLayer.MoteOverhead.AltitudeFor();float alpha=Mathf.Sqrt(Mathf.Clamp01(1-age/(float)TempestDragonRules.TrailLifetime))*ElementalHeartsMod.Settings.effectOpacity;block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.AngleAxis(delta.AngleFlat()-90,Vector3.up),new Vector3(delta.magnitude,1,1.5f)),TempestDragonVfx.Material(TempestDragonRules.Frame(age)),0,null,0,block);}
  }
 }
}
