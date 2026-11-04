using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public static class LunarVfx {
  static readonly string[] sequences={"CrescentDraw","MoonfallStep","SeleneDance","FallingMoon","MoonbreakRush","Idle","LunarLeap","Moonveil","TidalParry"};
  static readonly string[] directions={"south","east","north"};
  static readonly string[] bodyNames={"Male","Female","Thin","Fat","Hulk","Child","Baby"};
  static readonly Material[,] frames=new Material[9,8],bodies=new Material[7,3];
  static readonly Texture2D[] icons=new Texture2D[5];
  public static Texture2D Icon(int power){
   return icons[power]??(icons[power]=ContentFinder<Texture2D>.Get("UI/Abilities/ElementalHearts/Lunar/"+HediffComp_LunarAbilities.Art[power]));
  }
  public static Material Frame(int sequence,int frame){
   return frames[sequence,frame]??(frames[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Lunar/Frames/"+sequences[sequence]+"_"+frame.ToString("00"),ShaderDatabase.Transparent));
  }
  internal static Material Body(Pawn pawn){
   int body=Array.IndexOf(bodyNames,pawn.story==null||pawn.story.bodyType==null?"Male":pawn.story.bodyType.defName);
   if(body<0)body=0;
   int facing=pawn.Rotation==Rot4.South?0:pawn.Rotation==Rot4.North?2:1;
   return bodies[body,facing]??(bodies[body,facing]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/LunarUniform/LunarUniform_"+bodyNames[body]+"_"+directions[facing],ShaderDatabase.Transparent));
  }
  internal static void Emit(Pawn pawn,int sequence,Vector3 from,Vector3 to,int duration,float size,int ageOffset=0,int fullDuration=0){
   if(StormBlitzFx.Visible(pawn.Map,from,to))pawn.Map.GetComponent<MapComponent_LunarVfx>().Add(sequence,from,to,fullDuration>0?fullDuration:duration,size,ageOffset);
  }
  internal static void Ghost(Pawn pawn,Vector3 at){
   if(ElementalHeartsMod.Settings.showAfterimages&&StormBlitzFx.Visible(pawn.Map,at,at))pawn.Map.GetComponent<MapComponent_LunarVfx>().Ghost(pawn,at);
  }
  internal static void TrackAura(Pawn pawn){pawn.Map.GetComponent<MapComponent_LunarVfx>().Track(pawn);}
 }

 public sealed class MapComponent_LunarVfx : MapComponent {
  struct Effect { public int sequence, start, duration; public float size; public Vector3 from,to; }
  struct Echo {public bool active,west;public int start;public Vector3 at;public float width,height;public Material body;}
  readonly Effect[] effects=new Effect[96];
  readonly Echo[] echoes=new Echo[32];
  readonly Pawn[] auras=new Pawn[128];
  int effectCursor,auraCursor,echoCursor;
  MaterialPropertyBlock tint;
  public MapComponent_LunarVfx(Map map):base(map){}
  internal void Add(int sequence,Vector3 from,Vector3 to,int duration,float size,int ageOffset=0){
   effects[effectCursor]=new Effect{sequence=sequence,from=from,to=to,duration=Mathf.Clamp(duration,1,240),start=Find.TickManager.TicksGame-ageOffset,size=size};
   effectCursor=(effectCursor+1)%effects.Length;
  }
  internal void Ghost(Pawn pawn,Vector3 at){
   float native=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(pawn);
   var scale=MoonlineageApparelFit.Scale(pawn);
   echoes[echoCursor]=new Echo{active=true,at=at,start=Find.TickManager.TicksGame,west=pawn.Rotation==Rot4.West,
    width=native*scale.x,height=native*scale.y,body=LunarVfx.Body(pawn)};
   echoCursor=(echoCursor+1)%echoes.Length;
  }
  internal void Track(Pawn pawn){
   int empty=-1;
   for(int i=0;i<auras.Length;i++){
    if(auras[i]==pawn)return;
    if(empty<0&&auras[i]==null)empty=i;
   }
   if(empty>=0)auras[empty]=pawn;
   else {auras[auraCursor]=pawn;auraCursor=(auraCursor+1)%auras.Length;}
  }
  public override void MapComponentDraw(){
   float opacity=ElementalHeartsMod.Settings.effectOpacity;
   if(opacity<=0f||map!=Find.CurrentMap||Find.CameraDriver==null)return;
   var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);
   int now=Find.TickManager.TicksGame;
   for(int i=0;i<effects.Length;i++){
    var effect=effects[i];
    int age=now-effect.start;
    if(age<0||age>=effect.duration)continue;
    float progress=age/(float)effect.duration;
    Vector3 at=Vector3.Lerp(effect.from,effect.to,progress);
    if(!Visible(visible,at))continue;
    float angle=(effect.sequence<=1||effect.sequence==4)?(effect.to-effect.from).AngleFlat()-90f:0f;
    Draw(effect.sequence,Mathf.Min(7,(int)(progress*8)),at,effect.size,angle,opacity,AltitudeLayer.MoteOverhead);
   }
   for(int i=0;i<echoes.Length;i++){
    var echo=echoes[i];if(!echo.active)continue;
    float progress=(now-echo.start)/12f;
    if(progress>=1f){echoes[i]=new Echo();continue;}
    if(!Visible(visible,echo.at))continue;
    if(tint==null)tint=new MaterialPropertyBlock();
    tint.SetColor("_Color",new Color(.7f,.72f,1f,.32f*(1-progress)*(1-progress)*opacity));
    var at=echo.at;at.y=AltitudeLayer.MoteOverhead.AltitudeFor();
    Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.identity,new Vector3(echo.west?-echo.width:echo.width,1,echo.height)),echo.body,0,null,0,tint);
   }
   if(!ElementalHeartsMod.Settings.showAura)return;
   for(int i=0;i<auras.Length;i++){
    var pawn=auras[i];
    if(pawn==null)continue;
    if(!pawn.Spawned||pawn.Map!=map||pawn.Dead||pawn.Downed||!pawn.Drafted||!HediffComp_ElementalTransform.Active(pawn)||HediffComp_LunarAbilities.On(pawn)==null){auras[i]=null;continue;}
    var at=pawn.DrawPos;
    if(Visible(visible,at))Draw(5,(now/6+pawn.thingIDNumber)%6,at,2.6f,0f,opacity*.7f,AltitudeLayer.MoteLow);
   }
  }
  bool Visible(CellRect view,Vector3 at){var cell=at.ToIntVec3();return view.Contains(cell)&&cell.InBounds(map)&&!cell.Fogged(map);}
  void Draw(int sequence,int frame,Vector3 at,float size,float angle,float opacity,AltitudeLayer layer){
   at.y=layer.AltitudeFor();
   if(tint==null)tint=new MaterialPropertyBlock();
   tint.SetColor("_Color",new Color(1f,1f,1f,opacity));
   Graphics.DrawMesh(WindVfx.FrameMesh(false),Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(size,1f,size)),
    LunarVfx.Frame(sequence,frame),0,null,0,tint);
  }
 }
}
