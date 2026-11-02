using System;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class ScarletTempestRules {
  public const int Frames=8,Sprite=6;
  public static int Frame(int elapsed,int duration){return Math.Max(0,Math.Min(Frames-1,(int)((long)Math.Max(0,elapsed)*Frames/Math.Max(1,duration))));}
  public static float Opacity(int elapsed,int duration){return Math.Max(0,Math.Min(1,(duration-elapsed)/3f));}
  public static bool KeepZone(bool circuit){return circuit;}
 }
 public static class ScarletTempestVfx {
  static readonly Material[] materials=new Material[ScarletTempestRules.Frames];
  public static Material Material(int frame){frame=Math.Max(0,Math.Min(ScarletTempestRules.Frames-1,frame));return materials[frame]??(materials[frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Thunderstorm/Spin/Spin_"+frame.ToString("00"),ShaderDatabase.Transparent));}
 }
}
