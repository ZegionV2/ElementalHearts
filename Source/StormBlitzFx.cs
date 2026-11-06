using System;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class StormBlitzFx {
  static readonly Material[] effects=new Material[6],halos=new Material[6];static readonly Material[,] bodies=new Material[7,3];static Material silhouette;
  static readonly string[] bodyNames={"Male","Female","Thin","Fat","Hulk","Child","Baby"};
  public static Material Material(int index){return effects[index]??(effects[index]=MaterialPool.MatFrom("Effects/ElementalHearts/Thunderstorm/Blitz/Blitz_"+index.ToString("00"),ShaderDatabase.Transparent));}
  public static Material Halo(int frame){return halos[frame]??(halos[frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Thunderstorm/HaloFrames/Halo_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  public static int HaloFrame(int tick,int seed){return ((tick+seed)/5%6+6)%6;}
  public static float HaloBlend(int tick,int seed){return EvolutionTimeline.Fraction((tick+seed)/5f);}
  public static Material Silhouette{get{return silhouette??(silhouette=MaterialPool.MatFrom("Effects/ElementalHearts/Shared/TransformSilhouette",ShaderDatabase.Transparent));}}
  public static Material Regalia(Pawn p){if(CourtAppearance.WearingKit(p))return CourtAppearance.Body(p);string name=p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName;int body=Array.IndexOf(bodyNames,name);if(body<0)body=0;int dir=p.Rotation==Rot4.North?2:p.Rotation==Rot4.South?0:1;return bodies[body,dir]??(bodies[body,dir]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/ThunderstormRegalia/ThunderstormRegalia_"+bodyNames[body]+"_"+new[]{"south","east","north"}[dir],ShaderDatabase.Transparent));}
  public static bool Visible(Map map,Vector3 a,Vector3 b){if(map==null||map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return false;var rect=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);return rect.Contains(a.ToIntVec3())||rect.Contains(b.ToIntVec3())||rect.Contains(((a+b)*.5f).ToIntVec3());}
  static MapComponent_StormBlitzFX Controller(Pawn p,Vector3 a,Vector3 b){return p!=null&&p.Spawned&&Visible(p.Map,a,b)?p.Map.GetComponent<MapComponent_StormBlitzFX>():null;}
  public static void Streak(Pawn p,Vector3 from,Vector3 to){var c=Controller(p,from,to);if(c!=null){c.Streak(0,from,to,12,.75f);c.Streak(5,from,to,16,.45f);}}
  public static void Echo(Pawn p,Vector3 at){var c=Controller(p,at,at);if(c!=null)c.Echo(p,at);}
  public static void Launch(Pawn p,Vector3 at){var c=Controller(p,at,at);if(c!=null){c.Burst(3,at,2,16);c.Echo(p,at);}}
  public static void Arrive(Pawn p,Vector3 at){var c=Controller(p,at,at);if(c!=null)c.Burst(4,at,1.8f,14);}
  public static void Impact(Pawn p,Vector3 at){var c=Controller(p,at,at);if(c!=null)c.Burst(2,at,1.8f,12);}
  public static void Arc(Pawn p,Vector3 from,Vector3 to){var c=Controller(p,from,to);if(c!=null){c.Streak(1,from,to,14,.55f);c.Burst(2,to,1.1f,10);}}
 }
 public sealed class MapComponent_StormBlitzFX:MapComponent {
  struct Effect{public bool active,line;public int sprite,tick,duration;public Vector3 from,to;public float size;}
  struct EchoData{public bool active,regal,west;public int tick;public Vector3 position;public float life,width,height;public Material material;}
  readonly Effect[] effects=new Effect[128];readonly EchoData[] echoes=new EchoData[64];int effectCursor,echoCursor;MaterialPropertyBlock block;
  public MapComponent_StormBlitzFX(Map map):base(map){}
  void Add(Effect e){effects[effectCursor]=e;effectCursor=(effectCursor+1)%effects.Length;}
  public void Streak(int sprite,Vector3 from,Vector3 to,int duration,float width){Add(new Effect{active=true,line=true,sprite=sprite,tick=Find.TickManager.TicksGame,duration=duration,from=from,to=to,size=width});}
  public void Burst(int sprite,Vector3 at,float size,int duration){Add(new Effect{active=true,sprite=sprite,tick=Find.TickManager.TicksGame,duration=duration,from=at,to=at,size=size});}
  public void Echo(Pawn p,Vector3 at){float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var scale=p.story==null||p.story.bodyType==null?Vector2.one:p.story.bodyType.bodyGraphicScale;bool regal=HediffComp_ElementalTransform.Active(p);float width=1.5f*life*scale.x,height=1.5f*life*scale.y;if(regal)RegalSideFit.Echo(p,"EH_ThunderstormUniform",ref at,ref width,ref height);echoes[echoCursor]=new EchoData{active=true,regal=regal,west=p.Rotation==Rot4.West,tick=Find.TickManager.TicksGame,position=at,life=life,width=width,height=height,material=regal?StormBlitzFx.Regalia(p):null};echoCursor=(echoCursor+1)%echoes.Length;}
  void Draw(Material material,Vector3 at,float width,float height,float angle,Color color){if(!at.ToIntVec3().InBounds(map)||at.ToIntVec3().Fogged(map))return;color.a*=ElementalHeartsMod.Settings.effectOpacity;at.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,color);Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,height)),material,0,null,0,block);}
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(5);
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!e.active)continue;float t=(now-e.tick)/(float)Math.Max(1,e.duration);if(t>=1){effects[i].active=false;continue;}var at=(e.from+e.to)*.5f;if(!visible.Contains(at.ToIntVec3()))continue;float fade=(1-t)*(1-t);if(e.line){var d=e.to-e.from;Draw(StormBlitzFx.Material(e.sprite),at,Math.Max(.8f,d.magnitude+1.0f),e.size*(1-.3f*t),d.AngleFlat()-90,new Color(1,1,1,.8f*fade));}else Draw(StormBlitzFx.Material(e.sprite),at,e.size*(.85f+.3f*t),e.size*(.85f+.3f*t),0,new Color(1,1,1,.7f*fade));}
   for(int i=0;i<echoes.Length;i++){var e=echoes[i];if(!e.active)continue;float t=(now-e.tick)/16f;if(t>=1){echoes[i]=new EchoData();continue;}if(!visible.Contains(e.position.ToIntVec3()))continue;float fade=(1-t)*(1-t);var shadow=e.position;shadow.z+=.18f*e.life;Draw(StormBlitzFx.Silhouette,shadow,1.8f*e.life,1.8f*e.life,0,new Color(1,.06f,.14f,.3f*fade));if(e.regal)Draw(e.material,e.position,e.west?-e.width:e.width,e.height,0,new Color(1,.2f,.28f,.55f*fade));}
  }
 }
}
