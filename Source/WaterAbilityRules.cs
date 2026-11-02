using System;using UnityEngine;
namespace ElementalHearts {
 public enum WaterPower {Torrent,Step,Whirlpool,Judgement}
 public static class WaterAbilityRules {
  public const int MaxVictims=16,BurstVictims=8,ComboLifetime=480;
  static readonly int[] cooldowns={300,450,750,1800};static readonly float[] ranges={10f,6f,4f,12f};
  public static int Cooldown(WaterPower p){return cooldowns[(int)p];}
  public static float Range(WaterPower p){return ranges[(int)p];}
  public static int Warmup(WaterPower p){return p==WaterPower.Judgement?24:p==WaterPower.Whirlpool?6:p==WaterPower.Torrent?6:0;}
  public static int Recovery(WaterPower p){return p==WaterPower.Judgement?12:6;}
  public static float Damage(WaterPower p,int pressure){return p==WaterPower.Torrent?2.3f:p==WaterPower.Step?3f:p==WaterPower.Whirlpool?2.5f:10f+.75f*Math.Max(0,Math.Min(3,pressure));}
  public static int StepDuration(float distance){return Math.Max(6,Math.Min(12,(int)Math.Ceiling(distance)+4));}
  public static float StepProgress(int elapsed,int duration){float t=Mathf.Clamp01(elapsed/(float)Math.Max(1,duration));return t*t*(3-2*t);}
  public static int Frame(int age,int duration){return Math.Max(0,Math.Min(7,(int)((long)Math.Max(0,age)*8/Math.Max(1,duration))));}
  public static int Choose(int comboStep,int pressure,int enemies,bool[] valid){if(pressure>=3&&valid[3])return 3;if(enemies>=2&&valid[2])return 2;for(int i=0;i<4;i++){int p=(Math.Max(0,comboStep)+i)%4;if(valid[p])return p;}return -1;}
  public static bool Commit(System.Collections.Generic.HashSet<int> ids,int victim,int limit){return ids.Count<limit&&ids.Add(victim);}
  public static bool CounterLive(int now,int pendingAt,int until){return now<=(until>0?until:pendingAt+120);}
 }
}
