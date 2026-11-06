using System;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class ReversalVfx {
  static readonly int[] sequences={4,5,6,5,0,6};
  public static Material Material(int index){return Frame(index,0);}
  public static Material Frame(int index,int frame){return AzureVfx.EffectMaterial(sequences[Mathf.Clamp(index,0,5)],frame);}
  public static void Emit(Pawn p,int index,Vector3 position,float size,float angle){if(p==null||!p.Spawned||!StormBlitzFx.Visible(p.Map,position,position))return;p.Map.GetComponent<MapComponent_ReversalVfx>().Add(index,position,size,angle);}
 }
 public sealed class MapComponent_ReversalVfx:MapComponent {
  struct Effect {public bool active;public int index,tick;public Vector3 position;public float size,angle;}
  readonly Effect[] effects=new Effect[48];MaterialPropertyBlock block;
  public MapComponent_ReversalVfx(Map map):base(map){}
  public void Add(int index,Vector3 position,float size,float angle){int now=Find.TickManager.TicksGame;for(int i=0;i<effects.Length;i++)if(!effects[i].active||now-effects[i].tick>=18){effects[i]=new Effect{active=true,index=index,tick=now,position=position,size=size,angle=angle};return;}}
  void Draw(Effect e,int frame,float t,float opacity){
   var pos=e.position;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();float size=e.size*(.8f+.3f*t);bool trail=e.index==1||e.index==3;
   block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,ElementalHeartsMod.Settings.effectOpacity*(1-t)*opacity));
   Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.AngleAxis(trail||e.index==4?e.angle-90:0,Vector3.up),new Vector3(size*(trail?1.6f:1),1,size*(trail?.65f:1))),ReversalVfx.Frame(e.index,frame),0,null,0,block);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!e.active)continue;int age=now-e.tick;if(age<0)continue;float t=age/18f;if(t>=1){effects[i].active=false;continue;}if(!e.position.ToIntVec3().InBounds(map)||!visible.Contains(e.position.ToIntVec3())||e.position.ToIntVec3().Fogged(map))continue;
    float phase=Mathf.Clamp01(age/17f)*7;int first=(int)phase,second=Math.Min(7,first+1);float blend=Mathf.SmoothStep(0,1,phase-first);
    if(blend<.999f)Draw(e,first,t,1-blend);if(blend>.001f&&first!=second)Draw(e,second,t,blend);
   }
  }
 }
}
