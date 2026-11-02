using System;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class ReversalVfx {
  static readonly Material[] mats=new Material[6];
  public static Material Material(int index){return mats[index]??(mats[index]=MaterialPool.MatFrom("Effects/ElementalHearts/Azure/Reversal/Reversal_"+index.ToString("00"),ShaderDatabase.Transparent));}
  public static void Emit(Pawn p,int index,Vector3 position,float size,float angle){if(p==null||!p.Spawned||p.Map!=Find.CurrentMap||ElementalHeartsMod.Settings.effectOpacity<=0)return;p.Map.GetComponent<MapComponent_ReversalVfx>().Add(index,position,size,angle);}
 }
 public sealed class MapComponent_ReversalVfx:MapComponent {
  struct Effect {public bool active;public int index,tick;public Vector3 position;public float size,angle;}
  readonly Effect[] effects=new Effect[48];MaterialPropertyBlock block;
  public MapComponent_ReversalVfx(Map map):base(map){}
  public void Add(int index,Vector3 position,float size,float angle){int now=Find.TickManager.TicksGame;for(int i=0;i<effects.Length;i++)if(!effects[i].active||now-effects[i].tick>=18){effects[i]=new Effect{active=true,index=index,tick=now,position=position,size=size,angle=angle};return;}}
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!e.active)continue;float t=(now-e.tick)/18f;if(t>=1){effects[i].active=false;continue;}if(!e.position.ToIntVec3().InBounds(map)||!visible.Contains(e.position.ToIntVec3())||e.position.ToIntVec3().Fogged(map))continue;
    var pos=e.position;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();float size=e.size*(.8f+.3f*t);block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,ElementalHeartsMod.Settings.effectOpacity*(1-t)));
    Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.AngleAxis(e.index==1||e.index==3||e.index==4?e.angle-90:0,Vector3.up),new Vector3(size,1,size)),ReversalVfx.Material(e.index),0,null,0,block);
   }
  }
 }
}
