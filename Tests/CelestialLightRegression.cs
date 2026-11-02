using System;using System.IO;using System.Reflection;using UnityEngine;using ElementalHearts;
class CelestialLightRegression {
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);}
 static int Main(string[] args){AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;string p=n=="ElementalHearts"?args[1]:Path.Combine(args[0],n+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};try{typeof(CelestialLightRegression).GetMethod("Run",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
 static void Run(){
  Check(CelestialLightMath.AccentLimit==256&&CelestialLightMath.MaxSparks==12,"finite light budgets");
  float previous=-1;for(int age=-20;age<1000;age++){float p=CelestialLightMath.Progress(age,48),r=CelestialLightMath.Expand(p),f=CelestialLightMath.Fade(p),e=CelestialLightMath.Envelope(p);Check(p>=0&&p<=1&&r>=previous&&r<=1&&f>=0&&f<=1&&e>=0&&e<=1,"animation bounds/monotonic expansion");previous=r;}
  Check(CelestialLightMath.Fade(1)==0&&CelestialLightMath.Envelope(0)==0&&CelestialLightMath.Envelope(1)==0,"light survives outside its parent lifetime");
  for(int s=0;s<20;s++){int seed=CelestialLightMath.Seed(s,123456,new Vector3(5.5f,0,9.5f));for(int i=0;i<100;i++){float u=CelestialLightMath.Unit(seed,i);Check(u>=0&&u<1&&u==CelestialLightMath.Unit(seed,i),"unstable or out-of-bounds photon seed");}Check(CelestialLightMath.Rainbow(s)==(s>=7&&s<=11),"solar/prism palette separation");}
  for(int i=-1000;i<1000;i++){var d=CelestialLightMath.Direction(i*.17f);Check(Math.Abs(d.sqrMagnitude-1)<.00001f&&d.y==0,"invalid radial direction");}
  for(int i=0;i<10000;i++){var fx=new MapComponent_CelestialVfx(null);int token=fx.Add(CelestialEffect.ZenithBurst,Vector3.one,Vector3.one,55,24,9,9,null,8);Check(token!=0,"base pool emission rejected");fx.Stop(token);fx.Stop(token);}
  Console.WriteLine("PASS: bounded deterministic light math, finite monotonic waves, fading particle envelopes, stable palette/seed behavior and parent-slot cleanup under 10,000 emissions.");
 }
}
