using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 public static class EvolutionMotif {
  public static int ForJob(string job){
   switch(job){
    case "EH_EvolveThunderstorm":return 1;
    case "EH_EvolveGlacial":return 2;
    case "EH_EvolveCyclone":return 3;
    case "EH_EvolveAdamant":return 4;
    case "EH_EvolveEclipse":return 5;
    default:return 0;
   }
  }
  public static int ForPawn(Pawn pawn){
   if(ThunderstormEvolution.Active(pawn))return 1;
   if(GlacialEvolution.Active(pawn))return 2;
   if(CycloneEvolution.Active(pawn))return 3;
   if(AdamantEvolution.Active(pawn))return 4;
   if(EclipseAscendant.Active(pawn))return 5;
   return 0;
  }
 }

 public sealed partial class MapComponent_EvolutionCinematic {
  static Mesh flamePetal,crystalFacet,windRibbon;

  static Mesh MotifMesh(string name,Vector2[] outline){
   var vertices=new Vector3[outline.Length+1];
   var uv=new Vector2[vertices.Length];
   var triangles=new int[outline.Length*3];
   for(int i=0;i<outline.Length;i++){
    vertices[i+1]=new Vector3(outline[i].x,0,outline[i].y);
    uv[i+1]=new Vector2(.5f,.5f);
    triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%outline.Length+1;
   }
   var mesh=new Mesh{name=name};
   mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
   mesh.RecalculateNormals();mesh.RecalculateBounds();
   return mesh;
  }
  static void PrepareMotifs(){
   if(flamePetal!=null)return;
   flamePetal=MotifMesh("EH azure coronation flame",new[]{
    new Vector2(0,.8f),new Vector2(.17f,.24f),new Vector2(.24f,-.10f),new Vector2(.12f,-.35f),
    new Vector2(0,-.45f),new Vector2(-.19f,-.24f),new Vector2(-.13f,.12f),new Vector2(-.04f,.04f)});
   crystalFacet=MotifMesh("EH coronation crystal facet",new[]{
    new Vector2(0,.65f),new Vector2(.32f,0),new Vector2(0,-.65f),new Vector2(-.32f,0)});
   windRibbon=MotifMesh("EH cyclone coronation ribbon",new[]{
    new Vector2(-.65f,-.08f),new Vector2(-.3f,.12f),new Vector2(.12f,.18f),new Vector2(.65f,.05f),
    new Vector2(.18f,.08f),new Vector2(-.15f,.02f)});
  }
  static Vector3 Orbit(Vector3 center,float angle,float radius,float flatten=.58f){
   return center+new Vector3((float)Math.Cos(angle)*radius,0,(float)Math.Sin(angle)*radius*flatten);
  }
  void Stroke(Vector3 from,Vector3 to,float width,Color color){
   Vector3 delta=to-from;
   if(delta.sqrMagnitude<.0001f)return;
   Draw(MeshPool.plane10,solid,(from+to)*.5f,width,delta.magnitude,delta.AngleFlat(),color);
  }
  void Diamond(Vector3 at,float size,float angle,Color color){
   Draw(crystalFacet,solid,at,size,size,angle,color);
  }
  void DrawCoronation(int motif,Vector3 center,int elapsed,int seed,bool burst){
   PrepareMotifs();
   center.z-=.15f;
   var frame=EvolutionTimeline.At(burst?EvolutionTimeline.Duration:elapsed);
   float fade=burst?EvolutionTimeline.BurstOpacity(elapsed):frame.opacity;
   if(fade<=.001f)return;
   switch(motif){
    case 1: StormCoronation(center,frame,elapsed,fade,burst);break;
    case 2: FrostCoronation(center,frame,elapsed,fade,burst);break;
    case 3: WindCoronation(center,frame,elapsed,fade,burst);break;
    case 4: AdamantCoronation(center,frame,elapsed,fade,burst);break;
    case 5: EclipseCoronation(center,frame,elapsed,fade,burst);break;
    default: AzureCoronation(center,frame,elapsed,seed,fade,burst);break;
   }
  }

  void AzureCoronation(Vector3 center,EvolutionFrame frame,int elapsed,int seed,float fade,bool burst){
   float progress=burst?elapsed/(float)EvolutionTimeline.Afterglow:frame.progress;
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):1.8f-.95f*frame.collapse;
   Color blue=Color.Lerp(new Color(1f,.3f,.04f),new Color(.06f,.48f,1f),EvolutionTimeline.Smooth(.08f,.45f,frame.progress));
   for(int i=0;i<10;i++){
    float angle=i*(float)Math.PI/5+elapsed*.025f;
    var at=Orbit(center,angle,radius);
    if(!burst)at.z+=.25f+.45f*(float)Math.Sin(elapsed*.08f+i);
    Draw(flamePetal,solid,at,.32f+.28f*frame.surge,burst?.85f:1.1f+frame.surge,burst?-i*36:12*(float)Math.Sin(elapsed*.04f+i),new Color(blue.r,blue.g,blue.b,.63f*fade));
    Draw(flamePetal,solid,at,.13f,.72f,0,new Color(.76f,.96f,1f,.65f*fade));
   }
   if(!burst){
    var core=center+new Vector3(0,0,.6f);
    Draw(flamePetal,solid,core,.8f*frame.surge,2.1f*frame.surge,0,new Color(.16f,.75f,1f,.36f*fade));
    for(int i=0;i<8;i++){
     var ember=center+EvolutionTimeline.Spiral(elapsed,i,seed);
     Diamond(ember,.065f,45,new Color(.75f,.92f,1f,.7f*fade));
    }
   }else Ring(center,radius*.75f,0,new Color(.12f,.64f,1f,.4f*fade));
  }

  void StormCoronation(Vector3 center,EvolutionFrame frame,int elapsed,float fade,bool burst){
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):.7f+1.6f*frame.charge;
   var core=center+new Vector3(0,0,burst?.9f:1.4f*frame.surge);
   Color red=Color.Lerp(new Color(.18f,.6f,1f),new Color(1f,.025f,.12f),frame.surge);
   for(int i=0;i<8;i++){
    float angle=i*(float)Math.PI/4;
    var end=Orbit(center,angle,radius);
    Bolt(end,core,elapsed/3+i,red,fade*(.20f+.65f*frame.surge));
    Diamond(end,.14f,45,new Color(1f,.7f,.75f,.7f*fade));
   }
   if(!burst){
    float flash=(elapsed/3)%4==0?1f:.35f;
    Stroke(center,core+new Vector3(0,0,1f),.065f,new Color(1f,.88f,.91f,flash*frame.surge*fade));
    for(int i=0;i<8;i++){
     float angle=i*(float)Math.PI/4;
     Stroke(Orbit(center,angle,radius),Orbit(center,angle+(float)Math.PI/4,radius),.035f,new Color(1f,.06f,.13f,.4f*fade));
    }
   }
  }

  void Snowflake(Vector3 center,float radius,float angle,float fade){
   for(int i=0;i<6;i++){
    float a=angle+i*(float)Math.PI/3;
    var end=Orbit(center,a,radius,1f);
    Stroke(center,end,.035f,new Color(.65f,.9f,1f,.7f*fade));
    var fork=Vector3.Lerp(center,end,.62f);
    for(int side=-1;side<=1;side+=2)
     Stroke(fork,Orbit(fork,a+side*.75f,radius*.26f,1f),.025f,new Color(.8f,.97f,1f,.6f*fade));
   }
  }
  void FrostCoronation(Vector3 center,EvolutionFrame frame,int elapsed,float fade,bool burst){
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):.9f+1.35f*frame.charge;
   Snowflake(center,radius,elapsed*.003f,fade);
   for(int i=0;i<6;i++){
    float a=i*(float)Math.PI/3;
    var at=Orbit(center,a,burst?radius:radius*(1-.45f*frame.collapse));
    at.z+=burst?0:frame.surge*.55f;
    float size=burst?.40f:.22f+.65f*frame.surge;
    Diamond(at,size,i*60,new Color(.3f,.7f,1f,.70f*fade));
    Diamond(at,size*.52f,i*60,new Color(.9f,1f,1f,.75f*fade));
   }
   if(!burst)Diamond(center+new Vector3(0,0,.65f),.9f*frame.collapse,0,new Color(.85f,.98f,1f,.75f*fade));
  }

  void WindCoronation(Vector3 center,EvolutionFrame frame,int elapsed,float fade,bool burst){
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):1.1f+frame.charge;
   for(int band=0;band<3;band++){
    float altitude=burst?0:band*.55f*frame.surge;
    float bandRadius=radius*(1-band*.20f);
    for(int i=0;i<7;i++){
     float a=elapsed*.055f+i*.64f+band*2f;
     float sweep=EvolutionTimeline.Smooth(0,.6f,frame.progress);
     var at=Orbit(center+new Vector3(0,0,altitude),a,bandRadius*(burst?1f:1-.45f*frame.collapse));
     Draw(windRibbon,solid,at,.60f+.55f*sweep,.6f,-a*Mathf.Rad2Deg,new Color(.28f,1f,.63f,fade*(.5f-band*.1f)));
    }
   }
   if(burst){
    for(int i=0;i<9;i++){
     float a=i*(float)Math.PI*2/9;
     Stroke(Orbit(center,a,radius*.75f),Orbit(center,a,radius),.045f,new Color(.75f,1f,.88f,.6f*fade));
    }
   }
  }

  void AdamantCoronation(Vector3 center,EvolutionFrame frame,int elapsed,float fade,bool burst){
   float radius=burst?EvolutionTimeline.BurstRadius(elapsed):1.55f-.9f*frame.collapse;
   for(int i=0;i<8;i++){
    float a=i*(float)Math.PI/4;
    var at=Orbit(center,a,radius);
    at.z+=burst?0:.75f*frame.surge;
    float size=burst?.36f:.24f+.44f*frame.charge;
    Diamond(at,size,45,new Color(1f,.65f,.13f,.8f*fade));
    Diamond(at,size*.55f,45,new Color(1f,.96f,.68f,.8f*fade));
    if(!burst){
     Stroke(Orbit(center,a,radius*.38f),Orbit(center,a,radius),.045f,new Color(.9f,.57f,.18f,.55f*fade));
     Stroke(Orbit(center,a,radius),Orbit(center,a+(float)Math.PI/4,radius),.026f,new Color(1f,.83f,.46f,.5f*fade));
    }else Stroke(Orbit(center,a,radius*.7f),at,.06f,new Color(1f,.85f,.5f,.6f*fade));
   }
   if(!burst){
    var core=center+new Vector3(0,0,.65f);
    Diamond(core,.70f*frame.collapse,45,new Color(1f,.8f,.3f,.7f*fade));
    Diamond(core,.35f*frame.collapse,45,new Color(1f,1f,.86f,.9f*fade));
   }
  }
 }
}
