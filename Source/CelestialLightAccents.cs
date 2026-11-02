using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 // Pure, deterministic animation math. Never consumes gameplay Rand or spawns things.
 public static class CelestialLightMath {
  public const int AccentLimit=256,MaxSparks=12,RingSegments=48;
  public static float Unit(int seed,int index) {
   unchecked {uint h=(uint)seed+(uint)(index+1)*0x9e3779b9u;h^=h>>16;h*=0x85ebca6bu;h^=h>>13;h*=0xc2b2ae35u;h^=h>>16;return (h&0xffffff)/16777216f;}
  }
  public static float Progress(float age,float duration){return Mathf.Clamp01(age/Math.Max(1,duration));}
  public static float Expand(float progress){float p=1-Mathf.Clamp01(progress);return 1-p*p*p;}
  public static float Fade(float progress){float p=1-Mathf.Clamp01(progress);return p*p;}
  public static float Envelope(float progress){float p=Mathf.Clamp01(progress);return Mathf.Clamp01(p*7)*Fade(p);}
  public static int Seed(int sequence,int start,Vector3 from){unchecked{return sequence*73856093^start*19349663^((int)(from.x*16))*83492791^(int)(from.z*16);}}
  public static bool Rainbow(int sequence){return sequence>=7&&sequence<=11;}
  public static Vector3 Direction(float radians){return new Vector3(Mathf.Sin(radians),0,Mathf.Cos(radians));}
 }

 // Additional layers belong to the original Effect slot and disappear with it.
 // No new live particle objects, save state, timers, gameplay callbacks or material instances.
 public sealed partial class MapComponent_CelestialVfx {
  static Mesh lightRing,lightArc,lightNeedle,lightStar;
  static Material lightMaterial;
  static readonly Color SolarGold=new Color(1,.72f,.22f),SolarIvory=new Color(1,.96f,.76f);
  static readonly Color[] Prism={new Color(1,.38f,.38f),new Color(1,.68f,.25f),new Color(1,.94f,.45f),new Color(.42f,1,.72f),new Color(.48f,.8f,1),new Color(.84f,.55f,1)};
  int accentRemaining;CellRect accentView;
  public int LastAccentDraws {get{return CelestialLightMath.AccentLimit-accentRemaining;}}

  static Mesh Band(float degrees) {
   int n=CelestialLightMath.RingSegments;var v=new Vector3[(n+1)*2];var uv=new Vector2[v.Length];var triangles=new int[n*6];
   for(int i=0;i<=n;i++){float angle=(i/(float)n-.5f)*degrees*Mathf.Deg2Rad;Vector3 d=CelestialLightMath.Direction(angle);v[i*2]=d*.465f;v[i*2+1]=d*.5f;uv[i*2]=new Vector2(0,i/(float)n);uv[i*2+1]=new Vector2(1,i/(float)n);if(i<n){int k=i*6,a=i*2;triangles[k]=a;triangles[k+1]=a+1;triangles[k+2]=a+2;triangles[k+3]=a+1;triangles[k+4]=a+3;triangles[k+5]=a+2;}}
   var mesh=new Mesh{name="Celestial light wave "+degrees};mesh.vertices=v;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  static Mesh Needle() {
   var mesh=new Mesh{name="Celestial tapered photon"};mesh.vertices=new[]{new Vector3(0,0,.5f),new Vector3(.5f,0,-.1f),new Vector3(0,0,-.5f),new Vector3(-.5f,0,-.1f)};mesh.uv=new[]{new Vector2(.5f,1),new Vector2(1,.4f),new Vector2(.5f,0),new Vector2(0,.4f)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  static Mesh Star() {
   var v=new Vector3[9];var uv=new Vector2[9];var tris=new int[24];uv[0]=new Vector2(.5f,.5f);
   for(int i=0;i<8;i++){float a=i*Mathf.PI/4;v[i+1]=CelestialLightMath.Direction(a)*(i%2==0?.5f:.10f);uv[i+1]=new Vector2(v[i+1].x+.5f,v[i+1].z+.5f);tris[i*3]=0;tris[i*3+1]=i+1;tris[i*3+2]=(i+1)%8+1;}
   var mesh=new Mesh{name="Celestial prism glint"};mesh.vertices=v;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
  static void PrepareLights(){if(lightRing!=null)return;lightRing=Band(360);lightArc=Band(210);lightNeedle=Needle();lightStar=Star();lightMaterial=MaterialPool.MatFrom(BaseContent.WhiteTex,ShaderDatabase.MoteGlow,Color.white);}
  bool Accent(Mesh mesh,Vector3 at,float width,float length,float angle,Color color,float alpha,bool floor,Material material=null) {
   if(accentRemaining<=0||!CelestialVfx.Finite(at)||!accentView.Contains(at.ToIntVec3()))return false;
   var cell=at.ToIntVec3();if(!cell.InBounds(map)||cell.Fogged(map))return false;
   alpha=Mathf.Clamp01(alpha*ElementalHeartsMod.Settings.effectOpacity);if(alpha<.003f)return false;
   color.a=alpha;block.SetColor(ShaderPropertyIDs.Color,color);at.y=(floor?AltitudeLayer.MoteLow:AltitudeLayer.MoteOverhead).AltitudeFor();
   Graphics.DrawMesh(mesh,Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,length)),material??lightMaterial,0,null,0,block);accentRemaining--;return true;
  }
  void Wave(Vector3 at,float age,int lifetime,float diameter,float opacity,Color color,bool arc=false,float angle=0) {
   if(age<0||age>=lifetime||accentRemaining<=0)return;float p=CelestialLightMath.Progress(age,lifetime),size=diameter*(.18f+.82f*CelestialLightMath.Expand(p));float alpha=opacity*CelestialLightMath.Fade(p);Mesh mesh=arc?lightArc:lightRing;
   Accent(mesh,at,size*1.045f,size*1.045f,angle,color,alpha*.14f,true);
   Accent(mesh,at,size,size,angle,color,alpha*.55f,true);
   Accent(mesh,at,size*.977f,size*.977f,angle,SolarIvory,alpha*.23f,true);
  }
  void Flash(Vector3 at,int age,float scale,float opacity,bool rainbow=false) {
   if(age<0||age>=6)return;float p=1-age/6f;Accent(lightStar,at,scale*(.65f+.35f*p),scale*(.65f+.35f*p),age*7,rainbow?Prism[age%6]:SolarIvory,opacity*p*p,false);
  }
  void Photons(Effect e,Vector3 center,int age,int count,float radius,bool inward=false,bool rising=false,bool trail=false,bool spinning=false) {
   count=Math.Min(CelestialLightMath.MaxSparks,count);bool rainbow=CelestialLightMath.Rainbow(e.sequence);
   int seed=e.seed;Vector3 line=e.to-e.from;line.y=0;float heading=line.sqrMagnitude>.001f?line.AngleFlat():0;
   for(int i=0;i<count&&accentRemaining>0;i++) {
    float r=CelestialLightMath.Unit(seed,i),r2=CelestialLightMath.Unit(seed,i+19);float p=CelestialLightMath.Progress(age,e.duration);
    // Stagger each particle's short life. Gather/travel motes cycle only within the parent slot.
    bool loop=inward||rising;float local=loop?(p+r)%1:Mathf.Clamp01((p-r*.18f)/(.72f+r*.28f));
    float alpha=CelestialLightMath.Envelope(local)*e.opacity*(inward?.48f:.74f);if(alpha<.005f)continue;
    float radians=(i/(float)Math.Max(1,count)+r*.07f)*Mathf.PI*2;
    if(inward)radians+=local*1.25f;if(spinning)radians+=p*Mathf.PI*2;
    Vector3 d=CelestialLightMath.Direction(radians),at;
    if(trail){float along=Mathf.Clamp01(p-r*.42f);at=Vector3.Lerp(e.from,e.to,along);at+=CelestialLightMath.Direction((heading+90)*Mathf.Deg2Rad)*(r2-.5f)*.65f;}
    else if(rising){at=center+d*(radius*(.2f+r*.4f));at.z+=local*radius*1.6f;}
    else {float distance=inward?(1-CelestialLightMath.Expand(local))*radius:spinning?radius*(.72f+.15f*r):(.12f+CelestialLightMath.Expand(local))*(.45f+r*.55f)*radius;at=center+d*distance;}
    float angle=trail?heading:rising?0:radians*Mathf.Rad2Deg;float length=trail?.35f+r*.45f:rising?.12f+r*.18f:.15f+r*.35f;
    Color color=rainbow?Prism[i%Prism.Length]:i%3==0?SolarIvory:SolarGold;
    Accent(lightNeedle,at,.035f+r*.035f,length,angle,color,alpha,false);
    if(i%4==0)Accent(lightStar,at,.13f,.13f,angle+45,SolarIvory,alpha*.6f,false);
   }
  }
  void BeamLayers(Effect e,Vector3 at,int age) {
   float p=CelestialLightMath.Progress(age,e.duration);float envelope=Math.Min(1,age/4f)*Math.Min(1,(e.duration-age)/9f);int f=CelestialVisualAssets.FrameIndex(e.sequence,age,e.duration);var material=CelestialVisualAssets.Frame(e.sequence,f);
   Accent(Beam(),at,e.width*1.16f,e.height,0,SolarGold,e.opacity*.18f*envelope,false,material);
   Accent(Beam(),at,e.width*.43f,e.height*.98f,0,SolarIvory,e.opacity*.38f*envelope,false,material);
   if(e.sequence==13){Wave(at,age,Math.Min(42,e.duration),e.width*(1.4f-.5f*p),e.opacity*.45f,SolarGold);Photons(e,at,age,8,1.3f,false,true);}
   else {float diameter=e.sequence==4?6.2f:5.3f;Wave(at,age,30,diameter,e.opacity*.85f,SolarGold);Wave(at,age-6,28,diameter*1.12f,e.opacity*.5f,SolarIvory);Flash(at,age,1.4f,e.opacity*.65f);Photons(e,at,age,12,diameter*.5f);}
  }
  void DrawAccents(Effect e,Vector3 at,int age) {
   if(accentRemaining<=0||e.sequence>=18||!accentView.Contains(at.ToIntVec3())||!at.ToIntVec3().InBounds(map)||at.ToIntVec3().Fogged(map))return;
   PrepareLights();float heading=(e.to-e.from).AngleFlat();
   switch(e.sequence) {
    case 0:Wave(at,age,Math.Min(20,e.duration),2.7f,e.opacity*.55f,SolarGold,true,heading);Photons(e,at,age,5,1.2f);Flash(at,age,.7f,e.opacity*.45f);break;
    case 1:case 10:Wave(e.from,age,18,1.9f,e.opacity*.6f,e.sequence==10?Prism[4]:SolarGold);Photons(e,at,age,8,1,false,false,true);Flash(e.from,age,.8f,e.opacity*.45f,e.sequence==10);break;
    case 2:Wave(at,age,24,9.3f,e.opacity*.65f,SolarGold,true,age*28);Wave(at,age-4,20,8.7f,e.opacity*.32f,SolarIvory,true,180-age*24);Photons(e,at,age,8,3.8f,false,false,false,true);break;
    case 3:Photons(e,at,age,7,1.8f,true);break;
    case 4:case 13:case 14:BeamLayers(e,at,age);break;
    case 5:Photons(e,at,age,10,2.6f,true);Accent(lightStar,at,.35f+age/(float)e.duration*.35f,.35f+age/(float)e.duration*.35f,age*3,SolarIvory,e.opacity*.28f,false);break;
    case 6:Wave(at,age,24,9.6f,e.opacity*.9f,SolarGold);Wave(at,age-6,18,10.2f,e.opacity*.65f,SolarIvory);Flash(at,age,1.6f,e.opacity*.75f);Photons(e,at,age,12,4.5f);break;
    case 7:case 8:Photons(e,at,age,4,.65f,false,true);break;
    case 9:Flash(at,age,1.2f,e.opacity*.7f,true);Wave(at,age,12,2,e.opacity*.45f,Prism[4]);Photons(e,at,age,6,1.1f);break;
    case 11:Wave(at,age,18,2.8f,e.opacity*.5f,Prism[5],true,heading);Photons(e,at,age,6,1.3f);Flash(at,age,.85f,e.opacity*.55f,true);break;
    case 12:Photons(e,at,age,5,1.5f,true);break;
    case 15:Wave(at,age,Math.Min(30,e.duration),Math.Min(7,e.width),e.opacity*.6f,SolarGold);Wave(at,age-6,24,Math.Min(8,e.width*1.12f),e.opacity*.4f,SolarIvory);break;
    case 16:Photons(e,at,age,6,1.1f,false,true);break;
    case 17:Photons(e,at,age,3,.3f,false,true);break;
   }
  }
 }
}
