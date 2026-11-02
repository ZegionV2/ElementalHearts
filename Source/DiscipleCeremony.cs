using System;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class CeremonyTimeline {
  public const int TeachingDuration=180,Afterglow=42;
  public static int Elapsed(int tick,int started,int left,int duration){return Math.Min(duration,Math.Max(0,started>=0?tick-started:duration-left));}
  public static float Progress(int elapsed,int duration){return Mathf.Clamp01(elapsed/(float)Math.Max(1,duration));}
  public static float Energy(int elapsed,int duration){float p=Progress(elapsed,duration);return EvolutionTimeline.Smooth(0,.12f,p)*(1-EvolutionTimeline.Smooth(.92f,1,p));}
  public static int Cue(int elapsed,int duration){float p=Progress(elapsed,duration);return p<.3f?0:p<.78f?1:2;}
  public static float Lift(int elapsed){float p=Progress(elapsed,DiscipleAscension.Duration);return .12f*EvolutionTimeline.Smooth(.25f,.6f,p)*(1-EvolutionTimeline.Smooth(.82f,1,p));}
  public static Color Accent(string element,float alpha){return element=="Flame"?new Color(1,.36f,.08f,alpha):element=="Wind"?new Color(.38f,1,.68f,alpha):element=="Water"?new Color(.16f,.68f,1,alpha):new Color(.58f,.76f,1,alpha);}
 }
 // Fixed local registries; job drivers own saved timing and re-register after loading.
 public sealed class MapComponent_DiscipleCeremony:MapComponent {
  struct Burst {public bool active,ascension;public string element;public int tick;public Vector3 at;}
  readonly Pawn[] actors=new Pawn[32];readonly Burst[] bursts=new Burst[32];int cursor;
  static Mesh ring;static Material solid;MaterialPropertyBlock block;
  public MapComponent_DiscipleCeremony(Map map):base(map){}
  public void Track(Pawn p){if(p==null)return;for(int i=0;i<actors.Length;i++)if(actors[i]==p)return;for(int i=0;i<actors.Length;i++)if(actors[i]==null){actors[i]=p;return;}}
  public void Complete(Pawn p,string element,bool ascension){bursts[cursor]=new Burst{active=true,ascension=ascension,element=element,tick=Find.TickManager.TicksGame,at=p.Position.ToVector3Shifted()};cursor=(cursor+1)%bursts.Length;}
  static bool Active(Pawn p){return p!=null&&p.Spawned&&!p.Dead&&!p.Downed&&p.jobs!=null&&(p.jobs.curDriver is JobDriver_ElementalTeach||p.jobs.curDriver is JobDriver_AscendDisciple);}
  public override void MapComponentTick(){int now=Find.TickManager.TicksGame;for(int i=0;i<actors.Length;i++)if(actors[i]!=null&&(!Active(actors[i])||actors[i].Map!=map))actors[i]=null;for(int i=0;i<bursts.Length;i++)if(bursts[i].active&&now-bursts[i].tick>=CeremonyTimeline.Afterglow)bursts[i]=new Burst();}
  static void Prepare(){if(solid==null)solid=MaterialPool.MatFrom(BaseContent.WhiteTex,ShaderDatabase.Transparent,Color.white);if(ring!=null)return;const int n=48;var v=new Vector3[n*2];var uv=new Vector2[n*2];var tri=new int[n*6];for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n;var d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));v[2*i]=d;v[2*i+1]=d*.955f;uv[2*i]=uv[2*i+1]=new Vector2(.5f,.5f);int j=(i+1)%n,k=i*6;tri[k]=2*i;tri[k+1]=2*i+1;tri[k+2]=2*j;tri[k+3]=2*i+1;tri[k+4]=2*j+1;tri[k+5]=2*j;}ring=new Mesh{name="EH disciple ceremony ring"};ring.vertices=v;ring.uv=uv;ring.triangles=tri;ring.RecalculateNormals();ring.RecalculateBounds();}
  void Draw(Mesh mesh,Vector3 at,float width,float height,float angle,Color color){var cell=at.ToIntVec3();if(!cell.InBounds(map)||cell.Fogged(map))return;color.a*=ElementalHeartsMod.Settings.effectOpacity;if(color.a<=.001f)return;at.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,color);Graphics.DrawMesh(mesh,Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,height)),solid,0,null,0,block);}
  void Line(Vector3 a,Vector3 b,float width,Color color){var d=b-a;Draw(MeshPool.plane10,(a+b)*.5f,d.magnitude,width,d.AngleFlat()-90,color);}
  void Lesson(Pawn teacher,Pawn pupil,int elapsed){
   float p=CeremonyTimeline.Progress(elapsed,CeremonyTimeline.TeachingDuration),e=CeremonyTimeline.Energy(elapsed,CeremonyTimeline.TeachingDuration);string element=ElementalTeaching.Element(teacher);var tint=CeremonyTimeline.Accent(element,.6f*e);var a=teacher.Position.ToVector3Shifted();var b=pupil.Position.ToVector3Shifted();Draw(ring,a,.55f,.33f,0,CeremonyTimeline.Accent(element,.28f*e));Draw(ring,b,.32f+.28f*p,.2f+.17f*p,0,tint);a.z+=.65f;b.z+=.65f;
   if(p>.12f){var prior=a;for(int i=1;i<=8;i++){float f=i/8f;var at=Vector3.Lerp(a,b,f);at.z+=Mathf.Sin(f*Mathf.PI*2-elapsed*.08f)*.08f*e;Line(prior,at,.025f,tint);prior=at;}for(int i=0;i<5;i++){float f=EvolutionTimeline.Fraction(elapsed/55f+i/5f);var at=Vector3.Lerp(a,b,f);at.z+=Mathf.Sin(f*Mathf.PI)*.22f;Draw(MeshPool.plane10,at,.045f,.07f,45,new Color(.92f,.98f,1,.7f*e));}}
   for(int i=0;i<5;i++){float a1=elapsed*.055f+i*Mathf.PI*2/5;var at=b+new Vector3(Mathf.Cos(a1)*(.45f-.2f*p),0,Mathf.Sin(a1)*.18f);Draw(MeshPool.plane10,at,.035f,.09f,a1*Mathf.Rad2Deg,tint);}
  }
  void Ascension(Pawn pawn,int elapsed){
   var c=DiscipleAscension.Apprentice(pawn);if(c==null)return;float p=CeremonyTimeline.Progress(elapsed,DiscipleAscension.Duration),e=CeremonyTimeline.Energy(elapsed,DiscipleAscension.Duration),gather=EvolutionTimeline.Smooth(.12f,.85f,p);var center=pawn.Position.ToVector3Shifted();var tint=CeremonyTimeline.Accent(c.DiscipleElement,.55f*e);float radius=.8f+.35f*Mathf.Sin(p*Mathf.PI);Draw(ring,center,radius,radius*.65f,elapsed*.4f,tint);Draw(ring,center,radius*.72f,radius*.45f,-elapsed*.6f,new Color(.9f,.96f,1,.27f*e));
   for(int i=0;i<10;i++){float phase=EvolutionTimeline.Fraction(elapsed/60f+i/10f),angle=elapsed*.045f+i*2.399963f,r=(.9f-.6f*gather)*(1-.5f*phase);var at=center+new Vector3(Mathf.Cos(angle)*r,0,Mathf.Sin(angle)*r*.4f+.1f+phase*1.6f);var color=CeremonyTimeline.Accent(c.DiscipleElement,.65f*e*Mathf.Sin(phase*Mathf.PI));Draw(MeshPool.plane10,at,.04f,.12f+gather*.12f,-angle*Mathf.Rad2Deg,color);}
   if(p>.65f){float surge=EvolutionTimeline.Smooth(.65f,.9f,p);var core=center;core.z+=.65f;Draw(ring,core,.1f+.3f*surge,.1f+.3f*surge,0,new Color(.9f,.98f,1,.5f*surge*e));for(int i=0;i<6;i++){float a=elapsed*.04f+i*Mathf.PI/3;var at=core+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)*.65f)*(.85f-.55f*surge);Line(at,Vector3.Lerp(at,core,.25f),.025f,tint);}}
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;bool any=false;for(int i=0;i<actors.Length;i++)if(actors[i]!=null){any=true;break;}if(!any)for(int i=0;i<bursts.Length;i++)if(bursts[i].active){any=true;break;}if(!any)return;Prepare();if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);
   for(int i=0;i<actors.Length;i++){var p=actors[i];if(!Active(p)||p.Map!=map||p.Position.Fogged(map))continue;var teach=p.jobs.curDriver as JobDriver_ElementalTeach;if(teach!=null){var pupil=p.CurJob.targetA.Pawn;if(teach.Committed||!ElementalTeachingSession.Paired(p,pupil)||!visible.Contains(p.Position)&&!visible.Contains(pupil.Position))continue;Lesson(p,pupil,teach.ElapsedAt(now));}else{var ascend=p.jobs.curDriver as JobDriver_AscendDisciple;if(ascend!=null&&!ascend.Committed&&visible.Contains(p.Position))Ascension(p,ascend.ElapsedAt(now));}}
   for(int i=0;i<bursts.Length;i++){var b=bursts[i];if(!b.active||!visible.Contains(b.at.ToIntVec3()))continue;float p=CeremonyTimeline.Progress(now-b.tick,CeremonyTimeline.Afterglow),fade=(1-p)*(1-p);float size=(b.ascension?.4f:.2f)+(b.ascension?1.6f:.7f)*p;Draw(ring,b.at,size,size*.7f,0,CeremonyTimeline.Accent(b.element,.65f*fade));if(b.ascension)for(int j=0;j<8;j++){float a=j*Mathf.PI/4;var at=b.at+new Vector3(Mathf.Cos(a)*size,0,Mathf.Sin(a)*size*.6f+.35f);Draw(MeshPool.plane10,at,.04f,.15f,j*45,new Color(.92f,.98f,1,.7f*fade));}}
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class DiscipleAscensionLift {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(___pawn==null||___pawn.Dead||___pawn.Downed||!DiscipleAscension.Busy(___pawn)||ElementalHeartsMod.Settings.effectOpacity<=0)return;var driver=___pawn.jobs.curDriver as JobDriver_AscendDisciple;if(driver!=null&&!driver.Committed)__result.z+=CeremonyTimeline.Lift(driver.ElapsedAt(Find.TickManager.TicksGame));}
 }
}
