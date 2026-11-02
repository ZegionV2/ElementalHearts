using System;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 // Asset access only. Gameplay must emit through its existing bounded local effect/echo pools.
 public enum CelestialEffect {DawnCrescent,SunstrideTrail,CoronaSpin,JudgmentTelegraph,JudgmentColumn,ZenithFormation,ZenithBurst,RainbowMirage,RainbowGlint,RetreatFlash,CounterDash,RainbowSlash,SolarSeal,DepartureBeam,ArrivalBeam,ArrivalRing,TravelMotes,Radiance,Halo,SwordAura}
 [StaticConstructorOnStartup]
 public static class CelestialVisualAssets {
  public static readonly string[] Sequences={"DawnCrescent","SunstrideTrail","CoronaSpin","JudgmentTelegraph","JudgmentColumn","ZenithFormation","ZenithBurst","RainbowMirage","RainbowGlint","RetreatFlash","CounterDash","RainbowSlash","SolarSeal","DepartureBeam","ArrivalBeam","ArrivalRing","TravelMotes","Radiance","Halo","SwordAura"};
  public static readonly string[] Commands={"Dawncleaver","Sunstride","CoronaDance","SolarJudgment","HeavenlyZenith","FakeRainbow","LightTravel"};
  static readonly int[] counts={8,8,8,8,8,8,8,6,6,4,8,8,8,8,8,8,6,6,6,6};
  static readonly string[] bodies={"Male","Female","Thin","Fat","Hulk","Child","Baby"},directions={"south","east","north"};
  static readonly Material[,] frames=new Material[20,8],apparel=new Material[7,3];
  static readonly Texture2D[] icons=new Texture2D[7];
  public static int Count(int sequence){return counts[Math.Max(0,Math.Min(19,sequence))];}
  public static bool Loop(int sequence){return sequence==2||sequence==7||sequence==8||sequence>=16;}
  public static int FrameIndex(int sequence,int age,int duration){int n=Count(sequence);age=Math.Max(0,age);return Loop(sequence)?age/6%n:Math.Min(n-1,(int)((long)age*n/Math.Max(1,duration)));}
  public static bool AdornmentsVisible(bool celestial,bool transformed,bool drafted,bool changing,bool standing){return celestial&&transformed&&drafted&&!changing&&standing;}
  public static Material Frame(int sequence,int frame){sequence=Math.Max(0,Math.Min(19,sequence));frame=Math.Max(0,Math.Min(Count(sequence)-1,frame));return frames[sequence,frame]??(frames[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Celestial/Frames/"+Sequences[sequence]+"_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  public static Texture2D Icon(int index){index=Math.Max(0,Math.Min(6,index));return icons[index]??(icons[index]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Celestial/"+Commands[index]));}
  public static Material Body(Pawn p){int body=Array.IndexOf(bodies,p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName);if(body<0)body=0;int dir=p.Rotation==Rot4.South?0:p.Rotation==Rot4.North?2:1;return apparel[body,dir]??(apparel[body,dir]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/CelestialRegalia/CelestialRegalia_"+bodies[body]+"_"+directions[dir],ShaderDatabase.Transparent));}
 }
}
