using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public enum CycloneEffect { DrawTrail, RisingSpiral, CounterSweep, FinisherSlash, Formation, Loop, Release }
 public static class CycloneVisualRules {
  public const int EffectLimit=128, EchoLimit=32, AirboundLimit=64, HaloLimit=128, MaxDuration=300;
  public static int Frame(int sequence,int age,int duration) {
   int count=sequence>=7?6:8;age=Math.Max(0,age);
   if(sequence==5||sequence>=7)return age/(sequence==5?3:6)%count;
   return Math.Max(0,Math.Min(count-1,(int)((long)age*count/Math.Max(1,duration))));
  }
  public static bool HaloVisible(bool evolved,bool drafted,bool transformed,bool changing,bool standing){return evolved&&drafted&&transformed&&!changing&&standing;}
  public static bool Finite(Vector3 p){return !float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y)&&!float.IsInfinity(p.z);}
 }
 [StaticConstructorOnStartup]
 public static class CycloneVfx {
  public static readonly string[] Sequences={"DrawTrail","RisingSpiral","CounterSweep","FinisherSlash","CycloneForm","CycloneLoop","ReleaseBurst","Airbound","Halo"};
  static readonly string[] iconsNames={"VacuumDraw","SkywardGale","CycloneReversal","HeavenRendingTempest","SovereignCyclone","CycloneEvolution"};
  static readonly string[] directions={"south","east","north"};
  static readonly string[] bodyNames={"Male","Female","Thin","Fat","Hulk","Child","Baby"};
  static readonly Material[,] frames=new Material[9,8],bodies=new Material[7,3];
  static readonly Texture2D[] icons=new Texture2D[6];
  public static Texture2D Icon(int index){index=Math.Max(0,Math.Min(5,index));return icons[index]??(icons[index]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Cyclone/"+iconsNames[index]));}
  public static Material Frame(int sequence,int index){sequence=Math.Max(0,Math.Min(8,sequence));index=Math.Max(0,Math.Min(sequence>=7?5:7,index));return frames[sequence,index]??(frames[sequence,index]=MaterialPool.MatFrom("Effects/ElementalHearts/Cyclone/Frames/"+Sequences[sequence]+"_"+index.ToString("00"),ShaderDatabase.Transparent));}
  public static Material Body(Pawn p){int body=Array.IndexOf(bodyNames,p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName);if(body<0)body=0;int dir=p.Rotation==Rot4.South?0:p.Rotation==Rot4.North?2:1;return bodies[body,dir]??(bodies[body,dir]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/CycloneRegalia/CycloneRegalia_"+bodyNames[body]+"_"+directions[dir],ShaderDatabase.Transparent));}
  internal static bool HasLineage(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName=="EH_CycloneAscendant")return true;return false;}
  internal static bool Drawable(Pawn p){return p!=null&&p.Spawned&&!p.Dead&&!p.Downed;}
  // Presentation only: gameplay owns its job, saved stage timing, victims and impacts.
  public static int Emit(Pawn owner,CycloneEffect effect,Vector3 from,Vector3 to,int duration,float size,Pawn follow=null,int ageOffset=0){
   if(!Drawable(owner)||(int)effect<0||(int)effect>6||!CycloneVisualRules.Finite(from)||!CycloneVisualRules.Finite(to)||!StormBlitzFx.Visible(owner.Map,from,to))return 0;
   return owner.Map.GetComponent<MapComponent_CycloneVfx>().Add(effect,from,to,Find.TickManager.TicksGame,duration,size,follow,ageOffset);
  }
  public static void Ghost(Pawn p,Vector3 at){if(!Drawable(p)||!ElementalHeartsMod.Settings.showAfterimages||!CycloneVisualRules.Finite(at)||!StormBlitzFx.Visible(p.Map,at,at))return;p.Map.GetComponent<MapComponent_CycloneVfx>().Ghost(p,at,Find.TickManager.TicksGame);}
  public static void TrackHalo(Pawn p){if(Drawable(p))p.Map.GetComponent<MapComponent_CycloneVfx>().TrackHalo(p);}
  public static void TrackAirbound(Pawn p,int untilTick){if(Drawable(p))p.Map.GetComponent<MapComponent_CycloneVfx>().TrackAirbound(p,untilTick);}
  public static void Forget(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_CycloneVfx>().Forget(p);}
 }
 public sealed class MapComponent_CycloneVfx:MapComponent {
  struct Effect {public bool active;public int token,sequence,start,duration;public Vector3 from,to;public float size;public Pawn follow;}
  struct Echo {public bool active,west;public int start;public Vector3 at;public float width,height;public Material body;}
  struct Air {public Pawn pawn;public int until;}
  readonly Effect[] effects=new Effect[CycloneVisualRules.EffectLimit];
  readonly Echo[] echoes=new Echo[CycloneVisualRules.EchoLimit];
  readonly Air[] airbound=new Air[CycloneVisualRules.AirboundLimit];
  readonly Pawn[] halos=new Pawn[CycloneVisualRules.HaloLimit];
  int effectCursor,echoCursor,airCursor,haloCursor,nextToken;MaterialPropertyBlock block;
  public MapComponent_CycloneVfx(Map map):base(map){}
  public int Add(CycloneEffect effect,Vector3 from,Vector3 to,int now,int duration,float size,Pawn follow=null,int ageOffset=0){
   if((int)effect<0||(int)effect>6||!CycloneVisualRules.Finite(from)||!CycloneVisualRules.Finite(to)||float.IsNaN(size)||float.IsInfinity(size))return 0;
   duration=Math.Max(1,Math.Min(CycloneVisualRules.MaxDuration,duration));ageOffset=Math.Max(0,ageOffset);if(ageOffset>=duration)return 0;
   if(++nextToken<=0)nextToken=1;
   effects[effectCursor]=new Effect{active=true,token=nextToken,sequence=(int)effect,from=from,to=to,start=now-ageOffset,duration=duration,size=Mathf.Clamp(size,.1f,12f),follow=follow};effectCursor=(effectCursor+1)%effects.Length;return nextToken;
  }
  public void Stop(int token){if(token<=0)return;for(int i=0;i<effects.Length;i++)if(effects[i].active&&effects[i].token==token){effects[i]=new Effect();return;}}
  public void Ghost(Pawn p,Vector3 at,int now){if(!CycloneVfx.Drawable(p)||!CycloneVisualRules.Finite(at))return;float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var scale=p.story==null||p.story.bodyType==null?Vector2.one:p.story.bodyType.bodyGraphicScale;float width=1.5f*life*scale.x,height=1.5f*life*scale.y;RegalSideFit.Echo(p,"EH_CycloneUniform",ref at,ref width,ref height);echoes[echoCursor]=new Echo{active=true,start=now,at=at,west=p.Rotation==Rot4.West,width=width,height=height,body=CycloneVfx.Body(p)};echoCursor=(echoCursor+1)%echoes.Length;}
  public void TrackHalo(Pawn p){for(int i=0;i<halos.Length;i++)if(halos[i]==p)return;halos[haloCursor]=p;haloCursor=(haloCursor+1)%halos.Length;}
  public void TrackAirbound(Pawn p,int untilTick){int now=Find.TickManager.TicksGame;if(untilTick<=now)return;var item=new Air{pawn=p,until=Math.Min(untilTick,now+CycloneVisualRules.MaxDuration)};for(int i=0;i<airbound.Length;i++)if(airbound[i].pawn==p){airbound[i]=item;return;}airbound[airCursor]=item;airCursor=(airCursor+1)%airbound.Length;}
  public void UntrackAirbound(Pawn p){for(int i=0;i<airbound.Length;i++)if(airbound[i].pawn==p)airbound[i]=new Air();}
  public void Forget(Pawn p){for(int i=0;i<halos.Length;i++)if(halos[i]==p)halos[i]=null;for(int i=0;i<airbound.Length;i++)if(airbound[i].pawn==p)airbound[i]=new Air();for(int i=0;i<effects.Length;i++)if(effects[i].follow==p)effects[i]=new Effect();}
  bool Live(Pawn p){return CycloneVfx.Drawable(p)&&p.Map==map;}
  public override void FinalizeInit(){foreach(var p in map.mapPawns.AllPawnsSpawned)if(CycloneVfx.HasLineage(p))TrackHalo(p);}
  public override void MapComponentTick(){int now=Find.TickManager.TicksGame;if(now%15!=0)return;for(int i=0;i<effects.Length;i++)if(effects[i].active&&(now-effects[i].start>=effects[i].duration||effects[i].follow!=null&&!Live(effects[i].follow)))effects[i]=new Effect();for(int i=0;i<echoes.Length;i++)if(echoes[i].active&&now-echoes[i].start>=14)echoes[i]=new Echo();for(int i=0;i<airbound.Length;i++)if(airbound[i].pawn!=null&&(!Live(airbound[i].pawn)||now>=airbound[i].until))airbound[i]=new Air();for(int i=0;i<halos.Length;i++)if(halos[i]!=null&&(!Live(halos[i])||!CycloneVfx.HasLineage(halos[i])))halos[i]=null;}
  void Draw(Material material,Mesh mesh,Vector3 at,float width,float height,float angle,float opacity,bool floor=false){
   var cell=at.ToIntVec3();if(!cell.InBounds(map)||cell.Fogged(map))return;float alpha=opacity*ElementalHeartsMod.Settings.effectOpacity;if(alpha<=.001f)return;
   at.y=(floor?AltitudeLayer.MoteLow:AltitudeLayer.MoteOverhead).AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha));Graphics.DrawMesh(mesh,Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,height)),material,0,null,0,block);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(7);
   for(int i=0;i<effects.Length;i++){
    var e=effects[i];if(!e.active)continue;int age=now-e.start;if(age<0||age>=e.duration){effects[i]=new Effect();continue;}
    if(e.follow!=null){if(!Live(e.follow)){effects[i]=new Effect();continue;}e.from=e.to=e.follow.DrawPos;}
    bool line=e.sequence==0;var at=line?(e.from+e.to)*.5f:e.from;if(!visible.Contains(at.ToIntVec3()))continue;
    float angle=line?(e.to-e.from).AngleFlat()-90:0,width=line?Math.Max(.5f,(e.to-e.from).magnitude+.5f):e.size,height=e.size;
    bool upright=e.sequence==1||e.sequence==4||e.sequence==5;if(upright)height*=1.4f;
    float fade=e.sequence==5?Mathf.Min(1,(e.duration-age)/12f):Mathf.Min(1,(e.duration-age)/6f);
    Draw(CycloneVfx.Frame(e.sequence,CycloneVisualRules.Frame(e.sequence,age,e.duration)),upright?LightningVfx.FrameMesh(true):MeshPool.plane10,at,width,height,angle,.8f*fade,e.sequence==0||e.sequence==6);
   }
   for(int i=0;i<echoes.Length;i++){var e=echoes[i];if(!e.active)continue;float t=(now-e.start)/14f;if(t>=1){echoes[i]=new Echo();continue;}if(!visible.Contains(e.at.ToIntVec3()))continue;Draw(e.body,MeshPool.plane10,e.at,e.west?-e.width:e.width,e.height,0,.30f*(1-t)*(1-t));}
   for(int i=0;i<airbound.Length;i++){var a=airbound[i];if(a.pawn==null)continue;if(now>=a.until||!Live(a.pawn)){airbound[i]=new Air();continue;}if(!visible.Contains(a.pawn.Position))continue;float life=Mathf.Clamp(Mathf.Sqrt(a.pawn.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var at=a.pawn.DrawPos;at.z+=.18f*life;Draw(CycloneVfx.Frame(7,CycloneVisualRules.Frame(7,now+a.pawn.thingIDNumber,1)),MeshPool.plane10,at,1.5f*life,1.9f*life,0,.30f);}
   if(!ElementalHeartsMod.Settings.showAura)return;
   for(int i=0;i<halos.Length;i++){var p=halos[i];if(!Live(p)||!visible.Contains(p.Position)||!CycloneVisualRules.HaloVisible(CycloneVfx.HasLineage(p),p.Drafted,HediffComp_ElementalTransform.Active(p),HediffComp_ElementalTransform.Transforming(p),p.Awake()))continue;float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var at=p.DrawPos+p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);at.z+=(.43f+.02f*Mathf.Sin(now*.05f))*life;Draw(CycloneVfx.Frame(8,CycloneVisualRules.Frame(8,now+p.thingIDNumber,1)),MeshPool.plane10,at,1.1f*life,1.1f*life,0,.35f);}
  }
 }
}
