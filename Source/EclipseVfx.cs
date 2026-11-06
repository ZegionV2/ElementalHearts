using System;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class EclipseVfx {
  static readonly string[] sequences={"DuskCrescent","UmbralFlash","UmbralDash","UmbralBackstab","CrimsonWaltz","RivenHorizon","NightfallLeap","NightfallRush","SovereignEclipse","EclipseTelegraph","Halo","EclipseMark"};
  static readonly string[] bodyNames={"Male","Female","Thin","Fat","Hulk","Child","Baby"},directions={"south","east","north"};
  static readonly Material[,] frames=new Material[12,8],bodies=new Material[7,3];
  static readonly Texture2D[] icons=new Texture2D[7];
  public static Texture2D Icon(int i){return icons[i]??(icons[i]=ContentFinder<Texture2D>.Get("UI/Abilities/ElementalHearts/Eclipse/"+EclipseRules.Art[i]));}
  public static Material Frame(int sequence,int frame){return frames[sequence,frame]??(frames[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Eclipse/Frames/"+sequences[sequence]+"_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  static Material Body(Pawn pawn){if(CourtAppearance.WearingKit(pawn))return CourtAppearance.Body(pawn);int body=Array.IndexOf(bodyNames,pawn.story==null||pawn.story.bodyType==null?"Male":pawn.story.bodyType.defName);if(body<0)body=0;int facing=pawn.Rotation==Rot4.South?0:pawn.Rotation==Rot4.North?2:1;return bodies[body,facing]??(bodies[body,facing]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/EclipseRegalia/EclipseRegalia_"+bodyNames[body]+"_"+directions[facing],ShaderDatabase.Transparent));}
  internal static void Emit(Pawn p,int sequence,Vector3 from,Vector3 to,int duration,float size,int ageOffset=0){if(p!=null&&p.Spawned&&StormBlitzFx.Visible(p.Map,from,to))p.Map.GetComponent<MapComponent_EclipseVfx>().Add(sequence,from,to,duration,size,ageOffset);}
  internal static void Ghost(Pawn p,Vector3 at){if(p.Spawned&&ElementalHeartsMod.Settings.showAfterimages&&StormBlitzFx.Visible(p.Map,at,at))p.Map.GetComponent<MapComponent_EclipseVfx>().Ghost(p,at,Body(p));}
  internal static void Track(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_EclipseVfx>().Track(p);}
  internal static void Manifest(Pawn p){Emit(p,9,p.DrawPos,p.DrawPos,90,3.8f);Emit(p,1,p.DrawPos+new Vector3(0,0,.6f),p.DrawPos+new Vector3(0,0,.6f),70,2.8f);}
 }
 public static class EclipseEffectRules {
  // The two wave atlases have their cutting edge on local -X; dash art faces +X.
  public static float Angle(int sequence,Vector3 delta){bool directional=sequence==0||sequence==2||sequence==3||sequence==7||sequence==8;return directional&&delta.sqrMagnitude>.001f?delta.AngleFlat()-90+(sequence==0||sequence==8?180:0):0;}
 }
 public sealed class MapComponent_EclipseVfx:MapComponent {
  struct Effect{public int sequence,start,duration;public float size;public Vector3 from,to;}
  struct Echo{public bool active,west;public int start;public Vector3 at;public float width,height;public Material body;}
  readonly Effect[] effects=new Effect[96];readonly Echo[] echoes=new Echo[32];readonly Pawn[] adorned=new Pawn[128];
  int effectCursor,echoCursor,pawnCursor;MaterialPropertyBlock properties;
  public MapComponent_EclipseVfx(Map map):base(map){}
  internal void Add(int sequence,Vector3 from,Vector3 to,int duration,float size,int ageOffset){effects[effectCursor]=new Effect{sequence=sequence,from=from,to=to,duration=Mathf.Clamp(duration,1,240),start=Find.TickManager.TicksGame-Math.Max(0,ageOffset),size=size};effectCursor=(effectCursor+1)%effects.Length;}
  internal void Ghost(Pawn p,Vector3 at,Material body){float native=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(p);var scale=MoonlineageApparelFit.Scale(p);echoes[echoCursor]=new Echo{active=true,west=p.Rotation==Rot4.West,start=Find.TickManager.TicksGame,at=at,width=native*scale.x,height=native*scale.y,body=body};echoCursor=(echoCursor+1)%echoes.Length;}
  internal void Track(Pawn p){int empty=-1;for(int i=0;i<adorned.Length;i++){if(adorned[i]==p)return;if(adorned[i]==null&&empty<0)empty=i;}if(empty>=0)adorned[empty]=p;else{adorned[pawnCursor]=p;pawnCursor=(pawnCursor+1)%adorned.Length;}}
  public override void FinalizeInit(){foreach(var p in map.mapPawns.AllPawnsSpawned)if(EclipseAscendant.Active(p))Track(p);}
  bool Visible(CellRect view,Vector3 at){var cell=at.ToIntVec3();return cell.InBounds(map)&&view.Contains(cell)&&!cell.Fogged(map);}
  void Draw(int sequence,int frame,Vector3 at,float size,float angle,float alpha,bool floor=false){at.y=(floor?AltitudeLayer.MoteLow:AltitudeLayer.MoteOverhead).AltitudeFor();properties.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha));Graphics.DrawMesh(WindVfx.FrameMesh(false),Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(size,1,size)),EclipseVfx.Frame(sequence,frame),0,null,0,properties);}
  public override void MapComponentDraw(){
   float opacity=ElementalHeartsMod.Settings.effectOpacity;if(opacity<=0||map!=Find.CurrentMap||Find.CameraDriver==null)return;if(properties==null)properties=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var view=Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
   for(int i=0;i<effects.Length;i++){var e=effects[i];int age=now-e.start;if(e.duration==0||age<0||age>=e.duration)continue;float p=age/(float)e.duration;var at=Vector3.Lerp(e.from,e.to,p);if(!Visible(view,at))continue;float angle=EclipseEffectRules.Angle(e.sequence,e.to-e.from);Draw(e.sequence,Math.Min(7,(int)(p*8)),at,e.size,angle,opacity,e.sequence==9);}
   for(int i=0;i<echoes.Length;i++){var e=echoes[i];if(!e.active)continue;float p=(now-e.start)/12f;if(p>=1){echoes[i]=new Echo();continue;}if(!Visible(view,e.at))continue;properties.SetColor(ShaderPropertyIDs.Color,new Color(1,.12f,.18f,.34f*(1-p)*(1-p)*opacity));var at=e.at;at.y=AltitudeLayer.MoteOverhead.AltitudeFor();Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.identity,new Vector3(e.west?-e.width:e.width,1,e.height)),e.body,0,null,0,properties);}
   if(!ElementalHeartsMod.Settings.showAura)return;int marksDrawn=0;
   for(int i=0;i<adorned.Length;i++){
    var pawn=adorned[i];if(pawn==null)continue;
    if(!pawn.Spawned||pawn.Map!=map||pawn.Dead||!EclipseAscendant.Active(pawn)){adorned[i]=null;continue;}
    if(pawn.Downed||!pawn.Drafted||!pawn.Awake()||!HediffComp_ElementalTransform.Active(pawn)||HediffComp_ElementalTransform.Transforming(pawn)||!Visible(view,pawn.DrawPos))continue;
    float life=Mathf.Clamp(Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);int frame=(now/7+pawn.thingIDNumber)%6;
    var head=pawn.DrawPos+pawn.Drawer.renderer.BaseHeadOffsetAt(pawn.Rotation);head.z+=(.42f+.018f*Mathf.Sin(now*.045f))*life;Draw(10,frame,head,1.25f*life,0,opacity*(EclipseSky.Empowered(pawn)?.7f:.43f));
    var comp=HediffComp_EclipseAbilities.On(pawn);
    foreach(var mark in comp.VisualMarks){if(marksDrawn>=64)break;var target=mark.target;if(mark.expires<=now||target==null||!target.Spawned||target.Map!=map||target.Dead||target.Downed||!Visible(view,target.DrawPos))continue;var at=target.DrawPos;at.z+=.65f;Draw(11,frame,at,.65f,0,opacity*.5f);marksDrawn++;}
   }
  }
 }
 public sealed partial class MapComponent_EvolutionCinematic {
  static Mesh eclipseDisc;
  void EclipseCoronation(Vector3 center,EvolutionFrame frame,int elapsed,float fade,bool burst){
   if(eclipseDisc==null){var outline=new Vector2[48];for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;outline[i]=new Vector2(Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f);}eclipseDisc=MotifMesh("EH eclipse coronation disc",outline);}
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):1.7f-.65f*frame.collapse;var core=center+new Vector3(0,0,burst?.8f:1.15f+.6f*frame.surge);
   var red=new Color(1,.035f,.09f,.7f*fade);Ring(center,radius,0,red);
   if(!burst){
    float size=.6f+1.1f*frame.surge;Draw(eclipseDisc,solid,core,size,size,0,new Color(.8f,.88f,1,.8f*fade));
    var shadow=core+new Vector3((1-frame.surge)*size*.9f,0,0);Draw(eclipseDisc,solid,shadow,size*.95f,size*.95f,0,new Color(.015f,.008f,.02f,.98f*fade));
    Ring(core,size*.5f,0,new Color(1,.04f,.12f,.8f*frame.surge*fade));
    for(int i=0;i<8;i++){float angle=i*Mathf.PI/4+elapsed*.025f;var at=Orbit(center,angle,radius);Stroke(at,Vector3.Lerp(at,core,.15f+.55f*frame.collapse),.025f,red);Diamond(at,.08f,elapsed*2,new Color(1,.8f,.82f,.8f*fade));}
   }else{Ring(center,radius*.82f,0,new Color(.95f,.85f,.88f,.5f*fade));for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var at=Orbit(center,a,radius);Stroke(at,Orbit(center,a+.18f,radius*.85f),.065f,red);}}
  }
 }
}
