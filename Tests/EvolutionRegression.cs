using System;using System.IO;using System.Reflection;using System.Runtime.Serialization;using System.Collections.Generic;
using ElementalHearts;using RimWorld;using Verse;using Verse.AI;using UnityEngine;
class EvolutionRegression {
 static string modRoot;
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static T Empty<T>(){return (T)FormatterServices.GetUninitializedObject(typeof(T));}
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Set(object o,string key,object value){o.GetType().GetField(key,F).SetValue(o,value);}
 static int Main(string[] args){modRoot=Path.GetDirectoryName(Path.GetDirectoryName(args[1]));AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;string p=n=="ElementalHearts"?args[1]:n=="0Harmony"?args[2]:Path.Combine(args[0],n+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};try{typeof(EvolutionRegression).GetMethod("Run",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
 static bool Unit(float v){return !float.IsNaN(v)&&!float.IsInfinity(v)&&v>=0&&v<=1;}
 static void Run(){
  Check(EvolutionTimeline.Duration==270&&EvolutionTimeline.Afterglow==60,"Unexpected ritual timing");
  for(int tick=-20;tick<=300;tick++){
   var f=EvolutionTimeline.At(tick);Check(Unit(f.progress)&&Unit(f.charge)&&Unit(f.surge)&&Unit(f.collapse)&&Unit(f.opacity)&&f.lift>=0&&f.lift<=.32f&&f.radius>=.85f&&f.radius<=2.5f,"Invalid choreography curve at "+tick);
   if(tick<270)Check(EvolutionTimeline.At(tick+1).charge>=f.charge&&EvolutionTimeline.At(tick+1).collapse>=f.collapse,"Ritual build/release curve reversed");
   for(int i=0;i<12;i++){var a=EvolutionTimeline.Spiral(tick,i,12345);var b=EvolutionTimeline.Spiral(tick,i,12345);Check(a==b&&!float.IsNaN(a.x)&&!float.IsNaN(a.z)&&Math.Abs(a.x)<=2.5f&&Math.Abs(a.z)<4,"Unbounded or non-deterministic particles");}
  }
  Check(EvolutionTimeline.At(0).opacity==0&&EvolutionTimeline.At(0).lift==0&&EvolutionTimeline.At(270).lift==0&&EvolutionTimeline.At(180).lift>.25f,"Pawn lift does not start/end grounded or fails to rise");
  Check(EvolutionTimeline.Cue(89)==0&&EvolutionTimeline.Cue(90)==1&&EvolutionTimeline.Cue(189)==1&&EvolutionTimeline.Cue(190)==2,"Staged audio boundaries");
  Check(EvolutionTimeline.BurstOpacity(0)==1&&EvolutionTimeline.BurstOpacity(60)==0&&EvolutionTimeline.BurstOpacity(100)==0&&EvolutionTimeline.BurstRadius(60)==5,"Finishing effects do not expire/bound correctly");
  Console.WriteLine("PASS: 321 choreography frames, deterministic bounded spirals, smooth grounded lift endpoints, charge/release progression, audio cue boundaries and finite shockwave lifetime/radius.");
  var driver=new JobDriver_EvolveAzure();driver.pawn=Empty<Pawn>();driver.job=new Job();driver.ticksLeftThisToil=180;Check(driver.ElapsedAt(1000)==90,"Older 180-tick ritual fallback loses progress");Set(driver,"ritualStartTick",1000);Check(driver.ElapsedAt(1045)==45&&driver.ElapsedAt(995)==0,"Saved ritual start progress/clamping");
  var toils=new List<Toil>((IEnumerable<Toil>)typeof(JobDriver_EvolveAzure).GetMethod("MakeNewToils",F).Invoke(driver,null));Check(toils.Count==2&&toils[0].defaultDuration==270&&toils[0].handlingFacing&&toils[0].initAction!=null&&toils[0].tickAction!=null&&toils[1].initAction!=null,"Evolution job missing staged wait or completion action");
  var p=Empty<Pawn>();p.jobs=Empty<Pawn_JobTracker>();p.jobs.curJob=new Job{def=new JobDef{defName="EH_EvolveAzure"}};Check(AzureEvolution.Evolving(p),"Ritual gate missing");p.jobs.curJob.def.defName="Goto";Check(!AzureEvolution.Evolving(p),"Ritual visual/lift gate sticks after replacement order");
  var component=new MapComponent_EvolutionCinematic(null);for(int i=0;i<30;i++)component.Track(Empty<Pawn>());component.Track(p);var states=(Array)typeof(MapComponent_EvolutionCinematic).GetField("ceremonies",F).GetValue(component);int count=0;foreach(var state in states)if((bool)state.GetType().GetField("active").GetValue(state))count++;Check(states.Length==24&&count==24,"Ritual tracker is not bounded");var duplicate=new MapComponent_EvolutionCinematic(null);for(int i=0;i<30;i++)duplicate.Track(p);var slots=(Array)typeof(MapComponent_EvolutionCinematic).GetField("ceremonies",F).GetValue(duplicate);int used=0;foreach(var state in slots)if((bool)state.GetType().GetField("active").GetValue(state))used++;Check(used==1,"Tracking duplicates a ceremony per tick");
  Console.WriteLine("PASS: native 270-tick held/facing toil, saved/legacy progress recovery, replacement-order gate release and bounded/deduplicated ritual slots.");
  Set(driver,"ritualStartTick",54321);Set(driver,"lastCue",2);var payload=new EvolutionSaveCase{driver=driver};string file=Path.Combine(Path.GetTempPath(),"EvolutionTest_"+Guid.NewGuid().ToString("N")+".xml");Scribe.saver.InitSaving(file,"EvolutionTest");Scribe_Deep.Look(ref payload,"state");Scribe.saver.FinalizeSaving();var xml=new System.Xml.XmlDocument();xml.Load(file);Check(xml.SelectSingleNode("EvolutionTest/state/driver/EH_evolutionRitualStart").InnerText=="54321"&&xml.SelectSingleNode("EvolutionTest/state/driver/EH_evolutionRitualCue").InnerText=="2","Native save output lost ritual timing or stage audio");File.Delete(file);
  var meta=Empty<ModMetaData>();Set(meta,"rootDirInt",new DirectoryInfo(modRoot));string preview=(string)typeof(ModMetaData).GetProperty("PreviewImagePath",F).GetValue(meta,null);Check(preview.EndsWith(Path.Combine("About","Preview.png"))&&File.Exists(preview),"Generated thumbnail is not in the native loader path");
  Console.WriteLine("PASS: native Scribe save output for evolution timing/audio and native ModMetaData resolves installed-package About/Preview.png. GPU rendering and native load are not exercised.");
 }
}
public class EvolutionSaveCase:IExposable {public JobDriver_EvolveAzure driver;public void ExposeData(){Scribe_Deep.Look(ref driver,"driver");}}
