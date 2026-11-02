using System;using System.IO;using System.Reflection;using UnityEngine;using ElementalHearts;
class CycloneVisualRegression {
 static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static int Main(string[] args){AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;string p=n=="ElementalHearts"?args[1]:n=="0Harmony"?args[2]:Path.Combine(args[0],n+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};try{typeof(CycloneVisualRegression).GetMethod("Run",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
 static void Run(){
  for(int seq=0;seq<9;seq++)foreach(int duration in new[]{0,1,18,24,180,300}){int last=-1;for(int age=-1;age<=330;age++){int f=CycloneVisualRules.Frame(seq,age,duration);Check(f>=0&&f<(seq>=7?6:8),"frame leaves atlas bounds");if(seq!=5&&seq<7){Check(f>=last,"one-shot effect runs backward");last=f;}}}
  Check(CycloneVisualRules.Frame(5,24,180)==0&&CycloneVisualRules.Frame(7,36,180)==0&&CycloneVisualRules.Frame(8,36,180)==0,"loop fails to wrap at its expected period");
  for(int mask=0;mask<32;mask++){bool a=(mask&1)!=0,b=(mask&2)!=0,c=(mask&4)!=0,d=(mask&8)!=0,e=(mask&16)!=0;Check(CycloneVisualRules.HaloVisible(a,b,c,d,e)==(a&&b&&c&&!d&&e),"halo gate differs from transformed/drafted policy");}
  Check(CycloneVisualRules.Finite(new Vector3(1,2,3))&&!CycloneVisualRules.Finite(new Vector3(float.NaN,0,0))&&!CycloneVisualRules.Finite(new Vector3(0,float.PositiveInfinity,0)),"invalid draw coordinates not rejected");
  Check(CycloneVisualRules.EffectLimit==128&&CycloneVisualRules.EchoLimit==32&&CycloneVisualRules.AirboundLimit==64&&CycloneVisualRules.HaloLimit==128&&CycloneVisualRules.MaxDuration==300,"presentation budgets changed");
  Console.WriteLine("PASS: one-shot/loop frame boundaries and wrap, halo eligibility, invalid coordinates and fixed presentation budgets.");
 }
}
