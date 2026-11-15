using System;using System.Linq;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class LivingFlightAssets {
  public const string MarkerPath="UI/Commands/ElementalHearts/LivingFlight/FlyingPawn",LaunchPath="UI/Commands/ElementalHearts/LivingFlight/LaunchShared";
  static Texture2D marker,launch;static readonly Material[] frames=new Material[16],world=new Material[5];
  public static Texture2D LaunchIcon{get{return launch??(launch=ContentFinder<Texture2D>.Get(LaunchPath));}}
  public static Texture2D FlyingPawn{get{return marker??(marker=ContentFinder<Texture2D>.Get(MarkerPath));}}
  public static int Lineage(Pawn p){return AuroraIncarnate.Active(p)?4:TempestIncarnate.Active(p)?3:HellFlameIncarnate.Active(p)?2:ThunderIncarnate.Active(p)?1:0;}
  public static Color Tint(Pawn p){switch(Lineage(p)){case 4:return new Color(.5f,1,.88f);case 1:return new Color(1,.86f,.30f);case 2:return new Color(1,.34f,.19f);case 3:return new Color(.78f,.90f,1);default:return new Color(1,.91f,.65f);}}
  public static Material WorldMaterial(Pawn p){int n=Lineage(p);return world[n]??(world[n]=MaterialPool.MatFrom(new MaterialRequest(FlyingPawn,ShaderDatabase.WorldOverlayTransparentLit,Tint(p))));}
  public static Material Frame(int n){n=Mathf.Clamp(n,0,15);return frames[n]??(frames[n]=MaterialPool.MatFrom("Effects/ElementalHearts/LivingFlight/Frames/Launch_"+n.ToString("00"),ShaderDatabase.MoteGlow));}
  public static float Lift(int elapsed,bool departing){float t=Mathf.Clamp01(elapsed/(float)(departing?CelestialLightTravel.DepartTicks:CelestialLightTravel.ArriveTicks));if(!departing)return 4.5f*(1-Mathf.SmoothStep(0,1,t));float rise=Mathf.InverseLerp(.22f,1,t);return 4.5f*rise*rise;}
  public static void Begin(Pawn p,bool departing,int elapsed){if(p==null||!p.Spawned)return;var b=departing?LivingGroupTransport.Existing(p):null;float width=b==null?4:Mathf.Clamp(3.5f+Mathf.Sqrt(b.crew.Count)*.8f,4,8);p.Map.GetComponent<MapComponent_LivingFlightVfx>().Add(p,departing,elapsed,width);}
 }
 public sealed class MapComponent_LivingFlightVfx:MapComponent {
  struct Effect{public bool active,departing;public Pawn pawn;public Verse.AI.Job job;public Vector3 at;public Color tint;public int start,duration;public float size;}
  readonly Effect[] effects=new Effect[64];int cursor;MaterialPropertyBlock properties;
  public MapComponent_LivingFlightVfx(Map map):base(map){}
  public void Add(Pawn p,bool departing,int elapsed,float size){effects[cursor]=new Effect{active=true,departing=departing,pawn=p,job=p.CurJob,at=p.Position.ToVector3Shifted(),tint=LivingFlightAssets.Tint(p),start=Find.TickManager.TicksGame-Math.Max(0,elapsed),duration=departing?CelestialLightTravel.DepartTicks:CelestialLightTravel.ArriveTicks,size=size};cursor=(cursor+1)%effects.Length;}
  bool Alive(Effect e,int now){return e.active&&now-e.start<e.duration&&e.pawn!=null&&e.pawn.Spawned&&!e.pawn.Dead&&e.pawn.CurJob==e.job;}
  public override void MapComponentTick(){if(Find.TickManager.TicksGame%15!=0)return;for(int i=0;i<effects.Length;i++)if(effects[i].active&&!Alive(effects[i],Find.TickManager.TicksGame))effects[i].active=false;}
  void Draw(int frame,Vector3 at,float size,float alpha,Color tint){if(alpha<.001f)return;tint.a=alpha*ElementalHeartsMod.Settings.effectOpacity;properties.SetColor(ShaderPropertyIDs.Color,tint);at.y=AltitudeLayer.MoteLow.AltitudeFor();Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.identity,new Vector3(size,1,size)),LivingFlightAssets.Frame(frame),0,null,0,properties);}
  public override void MapComponentDraw(){
   if(Find.CurrentMap!=map||WorldRendererUtility.WorldSelected||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;if(properties==null)properties=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var view=Find.CameraDriver.CurrentViewRect.ExpandedBy(9);
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!Alive(e,now)||!view.Contains(e.at.ToIntVec3())||e.at.ToIntVec3().Fogged(map))continue;float t=Mathf.Clamp01((now-e.start)/(float)e.duration);float phase=(e.departing?t:Mathf.Lerp(.5f,1,t))*15;int frame=(int)phase;float blend=phase-frame;float alpha=.75f*Mathf.Min(1,t*8)*Mathf.Min(1,(1-t)*6);float size=e.size*Mathf.Lerp(.85f,1.12f,Mathf.SmoothStep(0,1,t));Draw(frame,e.at,size,alpha*(1-blend),e.tint);if(frame<15)Draw(frame+1,e.at,size,alpha*blend,e.tint);
   }
  }
 }
}
