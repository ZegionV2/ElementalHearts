using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public struct EvolutionFrame {
  public float progress,charge,surge,collapse,opacity,lift,radius,spin;
 }
 // Tick-derived, deterministic choreography: pausing and saving do not advance the animation.
 public static class EvolutionTimeline {
  public const int Duration=270,Afterglow=60;
  public static float Clamp(float x){return Math.Max(0,Math.Min(1,x));}
  public static float Smooth(float start,float end,float value){float t=Clamp((value-start)/(end-start));return t*t*(3-2*t);}
  public static EvolutionFrame At(int elapsed){
   float p=Clamp(elapsed/(float)Duration),charge=Smooth(0,.56f,p),surge=Smooth(.43f,.86f,p),collapse=Smooth(.80f,1,p);
   return new EvolutionFrame{progress=p,charge=charge,surge=surge,collapse=collapse,opacity=Smooth(0,.08f,p)*(1-.65f*Smooth(.94f,1,p)),lift=.32f*Smooth(.32f,.68f,p)*(1-Smooth(.86f,1,p)),radius=.85f+1.65f*charge,spin=p*15+surge*8};
  }
  public static Color Accent(bool storm,float progress){return Color.Lerp(storm?new Color(.12f,.55f,1):new Color(1,.35f,.055f),storm?new Color(1,.06f,.14f):new Color(.12f,.65f,1),Smooth(.24f,.73f,progress));}
  public static int Cue(int elapsed){return elapsed<90?0:elapsed<190?1:2;}
  public static float Fraction(float x){return x-(float)Math.Floor(x);}
  public static Vector3 Spiral(int elapsed,int index,int seed){
   var f=At(elapsed);float rise=Fraction(elapsed/65f+index/12f+(seed%19)*.01f),angle=f.spin+index*2.399963f;
   float radius=(2.5f-1.9f*f.collapse)*(1-.75f*rise);
   return new Vector3((float)Math.Cos(angle)*radius,0,(float)Math.Sin(angle)*radius*.45f+rise*(1.5f+1.3f*f.surge));
  }
  public static float BurstOpacity(int elapsed){float t=Clamp(elapsed/(float)Afterglow);return (1-t)*(1-t);}
  public static float BurstRadius(int elapsed){return .6f+4.4f*Smooth(0,1,elapsed/(float)Afterglow);}
 }
 public static class EvolutionCinematic {
  public static void Track(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_EvolutionCinematic>().Track(p);}
  public static void Complete(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_EvolutionCinematic>().Complete(p);}
 }
 public sealed partial class MapComponent_EvolutionCinematic:MapComponent {
  struct Ceremony {public Pawn pawn;public bool active,burst;public int finishTick;public Vector3 center;}
  readonly Ceremony[] ceremonies=new Ceremony[24];
  static Mesh ring;static Material solid;MaterialPropertyBlock properties;
  public MapComponent_EvolutionCinematic(Map map):base(map){}
  public void Track(Pawn p){
   for(int i=0;i<ceremonies.Length;i++)if(ceremonies[i].active&&ceremonies[i].pawn==p)return;
   for(int i=0;i<ceremonies.Length;i++)if(!ceremonies[i].active){ceremonies[i]=new Ceremony{active=true,pawn=p};return;}
  }
  public void Complete(Pawn p){
   Track(p);for(int i=0;i<ceremonies.Length;i++)if(ceremonies[i].active&&ceremonies[i].pawn==p){var c=ceremonies[i];c.burst=true;c.finishTick=Find.TickManager.TicksGame;c.center=p.Position.ToVector3Shifted();ceremonies[i]=c;return;}
  }
  public override void MapComponentTick(){
   int now=Find.TickManager.TicksGame;
   for(int i=0;i<ceremonies.Length;i++){var c=ceremonies[i];if(!c.active)continue;
    if(c.pawn==null||!c.pawn.Spawned||c.pawn.Map!=map||c.pawn.Dead||c.pawn.Downed||(!c.burst&&!AzureEvolution.Evolving(c.pawn))||(c.burst&&now-c.finishTick>=EvolutionTimeline.Afterglow))ceremonies[i]=new Ceremony();
   }
  }
  static void Prepare(){
   if(solid==null)solid=MaterialPool.MatFrom(BaseContent.WhiteTex,ShaderDatabase.Transparent,Color.white);
   if(ring!=null)return;const int sides=64;var vertices=new Vector3[sides*2];var uv=new Vector2[sides*2];var triangles=new int[sides*6];
   for(int i=0;i<sides;i++){float a=i*(float)Math.PI*2/sides;var dir=new Vector3((float)Math.Cos(a),0,(float)Math.Sin(a));vertices[i*2]=dir;vertices[i*2+1]=dir*.94f;uv[i*2]=uv[i*2+1]=new Vector2(.5f,.5f);int next=(i+1)%sides;int t=i*6;triangles[t]=i*2;triangles[t+1]=i*2+1;triangles[t+2]=next*2;triangles[t+3]=i*2+1;triangles[t+4]=next*2+1;triangles[t+5]=next*2;}
   ring=new Mesh{name="EH evolution ritual ring"};ring.vertices=vertices;ring.uv=uv;ring.triangles=triangles;ring.RecalculateNormals();ring.RecalculateBounds();
  }
  void Draw(Mesh mesh,Material material,Vector3 position,float width,float height,float angle,Color color){
   color.a*=ElementalHeartsMod.Settings.effectOpacity;if(color.a<=.001f)return;position.y=AltitudeLayer.MoteOverhead.AltitudeFor();properties.SetColor(ShaderPropertyIDs.Color,color);
   Graphics.DrawMesh(mesh,Matrix4x4.TRS(position,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,height)),material,0,null,0,properties);
  }
  void Ring(Vector3 center,float radius,float angle,Color color){Draw(ring,solid,center,radius,radius,angle,color);}
  void Bolt(Vector3 a,Vector3 b,int seed,Color tint,float alpha){
   var delta=b-a;var normal=new Vector3(-delta.z,0,delta.x).normalized;var prior=a;
   for(int i=1;i<=6;i++){float t=i/6f;var next=Vector3.Lerp(a,b,t);if(i<6)next+=normal*(float)Math.Sin(seed*.7f+i*2.3f)*.12f;var direction=next-prior;var color=tint;color.a=alpha;Draw(MeshPool.plane10,solid,(prior+next)*.5f,.055f,direction.magnitude,direction.AngleFlat(),color);prior=next;}
  }
  void Ritual(Pawn pawn,int elapsed){
   int motif=EvolutionMotif.ForJob(pawn.CurJobDef==null?null:pawn.CurJobDef.defName);
   DrawCoronation(motif,pawn.Position.ToVector3Shifted(),elapsed,pawn.thingIDNumber,false);
  }
  void Burst(Ceremony ceremony,int now){
   DrawCoronation(EvolutionMotif.ForPawn(ceremony.pawn),ceremony.center,now-ceremony.finishTick,ceremony.pawn.thingIDNumber,true);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   bool any=false;for(int i=0;i<ceremonies.Length;i++)if(ceremonies[i].active){any=true;break;}if(!any)return;
   if(properties==null)properties=new MaterialPropertyBlock();Prepare();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
   for(int i=0;i<ceremonies.Length;i++){var c=ceremonies[i];var p=c.pawn;if(!c.active||p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!visible.Contains(p.Position)||p.Position.Fogged(map))continue;
    if(c.burst){if(now-c.finishTick<EvolutionTimeline.Afterglow)Burst(c,now);continue;}
    if(!AzureEvolution.Evolving(p))continue;var driver=p.jobs.curDriver as JobDriver_EvolveAzure;if(driver!=null)Ritual(p,driver.ElapsedAt(now));
   }
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class EvolutionPawnLift {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(!AzureEvolution.Evolving(___pawn)||___pawn.Dead||___pawn.Downed||ElementalHeartsMod.Settings.effectOpacity<=0)return;var driver=___pawn.jobs.curDriver as JobDriver_EvolveAzure;if(driver!=null)__result.z+=EvolutionTimeline.At(driver.ElapsedAt(Find.TickManager.TicksGame)).lift;}
 }
}
