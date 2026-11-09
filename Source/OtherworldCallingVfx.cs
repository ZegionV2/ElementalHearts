using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class OtherworldCallingVfx {
  static readonly Material[][] frames=new Material[3][];
  static readonly Mesh portal;
  static readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
  static OtherworldCallingVfx(){
   string[] sequences={"PortalOpen","PortalLoop","PortalClose"};int[] count={8,6,8};
   for(int s=0;s<3;s++){frames[s]=new Material[count[s]];for(int i=0;i<count[s];i++)frames[s][i]=MaterialPool.MatFrom("Effects/OtherworldCalling/"+sequences[s]+"_"+i.ToString("00"),ShaderDatabase.Transparent);}
   float offset=224f/256f-.5f;portal=new Mesh{name="Otherworld doorway ground anchor"};portal.vertices=new[]{new Vector3(-.5f,0,-.5f+offset),new Vector3(-.5f,0,.5f+offset),new Vector3(.5f,0,.5f+offset),new Vector3(.5f,0,-.5f+offset)};portal.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};portal.triangles=new[]{0,1,2,0,2,3};portal.RecalculateNormals();portal.RecalculateBounds();
  }
  public static void Draw(Map map,IntVec3 cell,int age,bool closing=false){
   if(age<0||!CelestialVfx.Visible(map,cell.ToVector3Shifted(),cell.ToVector3Shifted())||cell.Fogged(map))return;
   int seq=closing?2:age<108?0:1;if(!closing&&age<60)return;int index=closing?Math.Min(7,age/4):seq==0?Math.Min(7,(age-60)/6):((age-108)/8)%6;
   float alpha=(closing?Math.Min(1,(32-age)/8f):Math.Min(1,(age-58)/8f))*ElementalHeartsMod.Settings.effectOpacity;if(alpha<=0)return;
   Vector3 at=cell.ToVector3Shifted();at.y=AltitudeLayer.MoteOverhead.AltitudeFor();var matrix=Matrix4x4.TRS(at,Quaternion.identity,new Vector3(2.35f,1,3.2f));
   if(!closing&&seq==1&&age<120){float blend=(age-108)/12f;block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha*(1-blend)));Graphics.DrawMesh(portal,matrix,frames[0][7],0,null,0,block);alpha*=blend;}
   block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha));Graphics.DrawMesh(portal,matrix,frames[seq][index],0,null,0,block);
  }
  public static void Arrival(Map map,IntVec3 cell){
   map.GetComponent<MapComponent_OtherworldCallingVfx>().Close(cell);
   var source=OtherworldCalling.State;Pawn caster=null;if(source!=null)foreach(var r in source.Records)if(r.map==map&&r.cell==cell){caster=r.caster;break;}
   if(caster!=null&&caster.Spawned&&caster.Map==map){var at=cell.ToVector3Shifted();CelestialVfx.Emit(caster,CelestialEffect.ArrivalBeam,at,at,24,2.3f,5);CelestialVfx.Emit(caster,CelestialEffect.ArrivalRing,at,at,30,3.4f,3.4f);CelestialVfx.Emit(caster,CelestialEffect.TravelMotes,at,at,40,2.5f,2.5f);ElementalAudio.Play(caster,"EH_TransformLock");}
  }
 }
 public sealed class MapComponent_OtherworldCallingVfx:MapComponent {
  struct Tail{public IntVec3 cell;public int start;public bool active;}
  readonly Tail[] tails=new Tail[16];int cursor;int nextSeal;
  public MapComponent_OtherworldCallingVfx(Map map):base(map){}
  public void Close(IntVec3 cell){tails[cursor]=new Tail{cell=cell,start=Find.TickManager.TicksGame,active=true};cursor=(cursor+1)%tails.Length;}
  public override void MapComponentTick(){
   var state=OtherworldCalling.State;if(state==null||state.Records.Count==0)return;int now=Find.TickManager.TicksGame;if(now<nextSeal)return;nextSeal=now+45;
   foreach(var r in state.Records)if(r.map==map&&!r.canceled&&!r.committed&&r.caster!=null&&r.caster.Spawned&&OtherworldCalling.Busy(r.caster)){var at=r.cell.ToVector3Shifted();CelestialVfx.Emit(r.caster,CelestialEffect.SolarSeal,at,at,48,2.6f,2.6f,null,0,.4f);}
  }
  public override void MapComponentDraw(){if(map!=Find.CurrentMap)return;var state=OtherworldCalling.State;if(state!=null)foreach(var r in state.Records)if(r.map==map&&!r.committed&&!r.canceled)OtherworldCallingVfx.Draw(map,r.cell,r.elapsed);int now=Find.TickManager.TicksGame;for(int i=0;i<tails.Length;i++)if(tails[i].active){int age=now-tails[i].start;if(age>=32)tails[i]=new Tail();else OtherworldCallingVfx.Draw(map,tails[i].cell,age,true);}}
 }
}
