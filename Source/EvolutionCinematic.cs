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
 public sealed class MapComponent_EvolutionCinematic:MapComponent {
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
  void Ritual(Pawn p,int elapsed){
   var f=EvolutionTimeline.At(elapsed);if(f.opacity<=0)return;Vector3 center=p.Position.ToVector3Shifted();center.z-=.15f;
   if(p.CurJobDef==DefDatabase<JobDef>.GetNamed("EH_EvolveGlacial")){Draw(MeshPool.plane10,GlacialVfx.Frame(4,Math.Min(7,(int)(f.progress*8))),center,2+f.charge,2+f.charge,f.spin*8,new Color(1,1,1,.6f*f.opacity));for(int i=0;i<6;i++){var at=center+EvolutionTimeline.Spiral(elapsed,i,p.thingIDNumber);Draw(MeshPool.plane10,GlacialVfx.Frame(0,(elapsed/5+i)%8),at,.35f,.65f,f.spin*25+i*60,new Color(1,1,1,.45f*f.opacity));}return;}
   if(p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveCyclone"){Draw(LightningVfx.FrameMesh(true),CycloneVfx.Frame(4,Math.Min(7,(int)(f.progress*8))),center,3+f.charge,3+f.charge,0,new Color(1,1,1,.65f*f.opacity));for(int i=0;i<6;i++){var at=center+EvolutionTimeline.Spiral(elapsed,i,p.thingIDNumber);Draw(MeshPool.plane10,CycloneVfx.Frame(1,(elapsed/5+i)%8),at,.45f,.8f,f.spin*25+i*60,new Color(1,1,1,.4f*f.opacity));}return;}
   if(p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveAdamant"){
    Ring(center,f.radius,f.spin*12,new Color(1,.7f,.22f,.35f*f.opacity));Draw(MeshPool.plane10,AdamantVfx.Frame(4,Math.Min(7,elapsed/20)),center,3.4f,3.4f,0,new Color(1,1,1,.55f*f.opacity));
    for(int i=0;i<8;i++){var at=center+EvolutionTimeline.Spiral(elapsed,i,p.thingIDNumber);Draw(MeshPool.plane10,solid,at,.10f,.16f,45+f.spin*12,new Color(1,.78f,.38f,.65f*f.opacity));}return;
   }
   bool storm=ThunderstormEvolution.HasLightning(p);Color tint=EvolutionTimeline.Accent(storm,f.progress);float alpha=f.opacity;
   Ring(center,f.radius,f.spin*25,new Color(tint.r,tint.g,tint.b,.27f*alpha));Ring(center,f.radius*.77f,-f.spin*35,new Color(1,.82f,.32f,.18f*alpha));
   Ring(center,.45f+.5f*f.charge,0,new Color(tint.r,tint.g,tint.b,.32f*alpha));
   // Rising spirals and shrinking orbits converge on the pawn before release.
   for(int i=0;i<12;i++){var offset=EvolutionTimeline.Spiral(elapsed,i,p.thingIDNumber);float phase=EvolutionTimeline.Fraction(elapsed/65f+i/12f+(p.thingIDNumber%19)*.01f);float fade=(float)Math.Sin(phase*Math.PI)*alpha;var at=center+offset;Color spark=Color.Lerp(new Color(1,.77f,.25f),tint,phase);spark.a=fade*.7f;float length=.12f+.2f*f.surge;Draw(MeshPool.plane10,solid,at,.045f,length,-f.spin*25+i*30,spark);
    if(!storm&&i%2==0&&f.progress>.3f){var wisps=center+offset*.65f;var c=new Color(.65f,.9f,1,.22f*fade*f.surge);Draw(MeshPool.plane10,ReversalVfx.Material(1),wisps,.45f+.25f*f.surge,.85f+1.2f*f.surge,f.spin*16+i*60,c);}
   }
   if(storm){var core=center;core.z+=.8f;for(int i=0;i<6;i++){float angle=f.spin+i*(float)Math.PI/3;var from=center+new Vector3((float)Math.Cos(angle)*f.radius,0,(float)Math.Sin(angle)*f.radius*.5f);Bolt(from,core,elapsed/4+i,tint,.5f*f.opacity*f.surge);}return;}
   if(f.surge>0){var column=center;column.z+=.7f;float taper=1-.7f*f.collapse;Draw(MeshPool.plane10,ReversalVfx.Material(1),column,1.1f*taper,1.8f+2*f.surge,0,new Color(.75f,.94f,1,.3f*f.surge*alpha));Draw(MeshPool.plane10,ReversalVfx.Material(0),center,2.2f+f.charge,2.2f+f.charge,f.spin*10,new Color(1,1,1,.35f*f.surge*alpha));}
   if(f.collapse>0){var core=center;core.z+=.65f;Draw(MeshPool.plane10,ReversalVfx.Material(5),core,.5f+.9f*f.collapse,.5f+.9f*f.collapse,-f.spin*15,new Color(1,1,1,.6f*f.collapse*alpha));}
  }
  void Burst(Ceremony c,int now){
   int elapsed=now-c.finishTick;float fade=EvolutionTimeline.BurstOpacity(elapsed),radius=EvolutionTimeline.BurstRadius(elapsed);Vector3 center=c.center;center.z-=.15f;
   if(AdamantEvolution.Active(c.pawn)){Ring(center,radius,elapsed,new Color(1,.73f,.25f,.6f*fade));Draw(MeshPool.plane10,AdamantVfx.Frame(2,Math.Min(7,elapsed/6)),center,8+radius,8+radius,0,new Color(1,1,1,.65f*fade));return;}
   if(CycloneEvolution.Active(c.pawn)){Ring(center,radius,elapsed*2,new Color(.25f,1,.62f,.65f*fade));Ring(center,radius*.7f,-elapsed*3,new Color(.85f,1,.92f,.45f*fade));Draw(MeshPool.plane10,CycloneVfx.Frame(6,Math.Min(7,elapsed/5)),center,2+radius,2+radius,0,new Color(1,1,1,.6f*fade));return;}
   if(ThunderstormEvolution.Active(c.pawn)){Ring(center,radius,elapsed*2,new Color(1,.06f,.14f,.65f*fade));var peak=center;peak.z+=1.2f*(1-elapsed/60f);for(int i=0;i<12;i++){float angle=i*(float)Math.PI/6;var end=center+new Vector3((float)Math.Cos(angle)*radius,0,(float)Math.Sin(angle)*radius*.65f);Bolt(peak,end,i+elapsed/4,new Color(1,.18f,.25f),.7f*fade);}return;}
   Ring(center,radius,elapsed*2,new Color(.15f,.75f,1,.65f*fade));Ring(center,radius*.7f,-elapsed*3,new Color(.9f,.97f,1,.45f*fade));
   Draw(MeshPool.plane10,ReversalVfx.Material(2),center,2.8f+radius*.45f,2.8f+radius*.45f,0,new Color(1,1,1,.55f*fade));
   var core=center;core.z+=.6f;Draw(MeshPool.plane10,ReversalVfx.Material(5),core,2.6f,2.6f,0,new Color(1,1,1,.6f*fade));
   for(int i=0;i<12;i++){float a=i*(float)Math.PI/6;var pos=center+new Vector3((float)Math.Cos(a)*radius,0,(float)Math.Sin(a)*radius*.7f);Draw(MeshPool.plane10,solid,pos,.06f,.24f,i*30,new Color(.5f,.85f,1,.7f*fade));}
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
