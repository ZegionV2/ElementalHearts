using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class WaterEffects {
  public static void Emit(Pawn p,Pawn target,bool counter){if(p.Map!=Find.CurrentMap||ElementalHeartsMod.Settings.effectOpacity<=0)return;p.Map.GetComponent<MapComponent_WaterEffects>().Add(p.DrawPos,(target.DrawPos-p.DrawPos).AngleFlat(),counter);}
 }
 public sealed class MapComponent_WaterEffects:MapComponent {
  struct Arc{public Vector3 pos;public float angle;public int start;public bool counter;}
  readonly List<Arc> arcs=new List<Arc>();
  static Mesh mesh;static Material material;static MaterialPropertyBlock block;
  public MapComponent_WaterEffects(Map map):base(map){}
  public void Add(Vector3 pos,float angle,bool counter){if(arcs.Count>=32)arcs.RemoveAt(0);arcs.Add(new Arc{pos=pos,angle=angle,start=Find.TickManager.TicksGame,counter=counter});}
  public override void MapComponentTick(){int now=Find.TickManager.TicksGame;for(int i=arcs.Count-1;i>=0;i--)if(now-arcs[i].start>=18)arcs.RemoveAt(i);}
  static void Prepare(){if(mesh!=null)return;const int count=20;var v=new Vector3[(count+1)*2];var triangles=new int[count*6];
   for(int i=0;i<=count;i++){float a=(-65+i*130f/count)*Mathf.Deg2Rad;v[i*2]=new Vector3(Mathf.Sin(a)*.65f,0,Mathf.Cos(a)*.65f);v[i*2+1]=new Vector3(Mathf.Sin(a)*.8f,0,Mathf.Cos(a)*.8f);if(i<count){int j=i*6,k=i*2;triangles[j]=k;triangles[j+1]=k+1;triangles[j+2]=k+2;triangles[j+3]=k+1;triangles[j+4]=k+3;triangles[j+5]=k+2;}}
   mesh=new Mesh();mesh.vertices=v;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();material=SolidColorMaterials.SimpleSolidColorMaterial(Color.white,true);block=new MaterialPropertyBlock();
  }
  public override void MapComponentDraw(){if(map!=Find.CurrentMap||arcs.Count==0)return;Prepare();int now=Find.TickManager.TicksGame;
   foreach(var a in arcs){float t=Mathf.Clamp01((now-a.start)/18f);var pos=a.pos;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();float size=a.counter?1.4f:1f;block.SetColor("_Color",new Color(.25f,.8f,1f,(1-t)*ElementalHeartsMod.Settings.effectOpacity));Graphics.DrawMesh(mesh,Matrix4x4.TRS(pos,Quaternion.AngleAxis(a.angle+(a.counter?Mathf.Lerp(-45,45,t):0),Vector3.up),Vector3.one*size),material,0,null,0,block);}
  }
 }
}
