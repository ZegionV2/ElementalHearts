using System;using System.IO;using System.Reflection;using System.Runtime.Serialization;using System.Collections.Generic;using System.Linq;using ElementalHearts;using Verse;using Verse.AI;
class EarthRegression {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static int checks;
 static T Empty<T>(){return (T)FormatterServices.GetUninitializedObject(typeof(T));}
 static void Check(bool value,string label){checks++;if(!value)throw new Exception(label);}
 static void Set(object o,string key,object value){o.GetType().GetField(key,F).SetValue(o,value);}
 static int Main(string[] args){
  AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;string path=n=="ElementalHearts"?args[1]:n=="0Harmony"?args[2]:Path.Combine(args[0],n+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
  try{typeof(EarthRegression).GetMethod("Run",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}
 }
 static void Run(){
  Check((int)EarthPower.Bastion==2&&(int)EarthPower.Mountainfall==3&&HediffComp_EarthAbilities.Names.Length==4,"seismic replacements preserve four saved skill slots");
  Check(EarthSeismicRules.GroundStab(EarthPower.Bastion)&&EarthSeismicRules.GroundStab(EarthPower.Mountainfall)&&!EarthSeismicRules.GroundStab(EarthPower.Cleave)&&!EarthSeismicRules.GroundStab(EarthPower.Advance),"only seismic casts own ground-stab drawing");
  Check(EarthSeismicRules.Windup(EarthPower.Bastion)==36&&EarthSeismicRules.Windup(EarthPower.Mountainfall)==42&&EarthSeismicRules.Windup(EarthPower.Cleave)==6,"seismic telegraphs preserve intentional windup");
  Check(EarthSeismicRules.StabProgress(100,100,136)==0&&EarthSeismicRules.StabProgress(118,100,136)==.5f&&EarthSeismicRules.StabProgress(136,100,136)==1,"saved tick deadlines drive exact stab progression");
  Check(EarthSeismicRules.StabLift(.6f)>EarthSeismicRules.StabLift(0)&&EarthSeismicRules.StabLift(1)<EarthSeismicRules.StabLift(0),"sword raises before plunging below its starting point");
  Check(EarthSeismicRules.FramePhase(-1,32)==0&&EarthSeismicRules.FramePhase(31,32)==7&&EarthSeismicRules.FramePhase(99,32)==7,"all eight effect frames have bounded endpoints");
  Check(EarthSeismicRules.QuakeRadius==4&&EarthSeismicRules.SpireRadius==3&&EarthRules.VictimLimit==16,"local seismic searches retain fixed radii and victim bound");
  Check(EarthRules.Stacks(3,101,100)==3&&EarthRules.Stacks(3,100,100)==0,"Resolve exact expiry");
  Check(EarthRules.Stacks(999,101,100)==3&&EarthRules.Stacks(-1,101,100)==0,"Resolve stack bounds");
  Check(Math.Abs(EarthRules.Defense(3,true,1)-.68f)<.00001f,"Resolve and Bastion multiply once");
  Check(Math.Abs(EarthRules.Defense(3,false,1)-.85f)<.00001f,"Resolve defense");
  Check(Math.Abs(EarthRules.Defense(0,true,.4f)-.92f)<.00001f,"disciple defensive scaling");
  Check(EarthRules.Impact(-20)==1&&Math.Abs(EarthRules.Impact(999)-1.3f)<.00001f,"Resolve finisher clamp");
  Check(EarthRules.RepathAllowed(0,4,3,36)&&!EarthRules.RepathAllowed(1,4,3,36)&&!EarthRules.RepathAllowed(0,10,3,36)&&!EarthRules.RepathAllowed(0,4,3,37)&&!EarthRules.RepathAllowed(0,4,0,4),"one charge adjustment, twelve-step and six-cell limits");
  foreach(float t in new[]{-10f,0f,.25f,.5f,.75f,1f,10f})Check(EarthRules.Smooth(t)>=0&&EarthRules.Smooth(t)<=1,"transition clamp");
  Check(EarthRules.CombatWeight(100,100,280)==0&&EarthRules.CombatWeight(112,100,280)==1&&EarthRules.CombatWeight(280,100,280)==0,"aura fades at actual engagement boundaries");
  Check(EarthRules.EffectPhase(3,0,30)==3&&EarthRules.EffectPhase(3,29,30)==5&&EarthRules.EffectPhase(0,0,18)==2,"impact frame is present on the damage tick");
  var full=new HediffComp_EarthAbilities{props=new HediffCompProperties_EarthAbilities()};
  var pupil=new HediffComp_EarthAbilities{props=new HediffCompProperties_EarthAbilities{cooldownFactor=1.5f}};
  foreach(EarthPower p in Enum.GetValues(typeof(EarthPower)))Check(pupil.Cooldown(p)==(int)Math.Ceiling(full.Cooldown(p)*1.5f),"disciple cooldown");
  for(int duration=1;duration<100;duration++){int prior=0;for(int elapsed=-1;elapsed<duration+5;elapsed++){int frame=EarthRules.Frame(elapsed,duration);Check(frame>=0&&frame<6&&frame>=prior,"frame monotonicity/bounds");prior=frame;}}
  Check(Enumerable.Range(0,30).Select(x=>EarthRules.Frame(x,30)).Distinct().Count()==6,"all six aura frames");
  full.autoCombat=false;full.nonLethal=true;full.readyAt=new List<int>{60,120,180,240};full.guardUntil=1234;full.resolve=3;full.combatStarted=70;
  full.CopyStateTo(pupil);Check(!pupil.autoCombat&&pupil.nonLethal&&pupil.readyAt.SequenceEqual(full.readyAt)&&pupil.combatStarted==70,"promotion controls/cooldowns");
  pupil.readyAt[0]=999;Check(full.readyAt[0]==60&&pupil.guardUntil==0&&pupil.resolve==0,"promotion does not alias cooldowns or grant old guard");
  var pawn=Empty<Pawn>();pawn.health=Empty<Pawn_HealthTracker>();pawn.health.hediffSet=Empty<HediffSet>();pawn.health.hediffSet.hediffs=new List<Hediff>();
  var heart=new HediffWithComps{pawn=pawn,def=new HediffDef{defName="EH_EarthHeart",tags=new List<string>{ElementalHeartUtility.HeartTag}},comps=new List<HediffComp>()};
  var component=new HediffComp_EarthAbilities{parent=heart,props=new HediffCompProperties_EarthAbilities()};heart.comps.Add(component);pawn.health.hediffSet.hediffs.Add(heart);
  Check(ElementalTeaching.Element(pawn)=="Earth"&&HediffComp_EarthAbilities.On(pawn)==component,"Earth identity and lookup");
  heart.def=new HediffDef{defName="EH_EarthTraining"};heart.comps.Add(new HediffComp_ElementalApprentice{parent=heart});
  Check(ElementalTeaching.Trained(pawn)&&ElementalTeaching.Element(pawn)==null&&ElementalTeaching.DamageFactor(pawn)==.4f,"Earth disciple tier isolation");
  Check(NpcDisciples.Elements.Length==5&&NpcDisciples.Elements.Distinct().Count()==5&&Array.IndexOf(NpcDisciples.Elements,"Earth")>=0,"NPC/Bestow five distinct choices");
  pawn.jobs=Empty<Pawn_JobTracker>();pawn.jobs.curJob=new Job{def=new JobDef{defName="EH_EarthTechnique"}};
  Check(EarthCombat.Busy(pawn)&&!EarthCombat.OtherAction(pawn),"technique busy gate");
  pawn.jobs.curJob.def.defName="Goto";Check(!EarthCombat.Busy(pawn)&&!EarthCombat.OtherAction(pawn),"new movement order releases ownership");
  pawn.jobs.curJob.def.defName="EH_TeachElemental";Check(EarthCombat.OtherAction(pawn),"teaching gate");
  var selected=Empty<Pawn>();var incidental=Empty<Pawn>();
  using(new ElementalManualTarget(pawn,selected)){
   Check(ElementalManualTarget.Explicit(pawn,selected)&&!ElementalManualTarget.Explicit(pawn,incidental),"manual target does not leak to area victims");
  }
  Check(!ElementalManualTarget.Explicit(pawn,selected),"manual target context cleanup");
  var start=new IntVec3(10,0,10);var tree=WindNavigation.Tree(start,(a,b)=>b.x<12&&b.z!=12);Check(tree.Count<=113&&!tree.ContainsKey(new IntVec3(12,0,10))&&!tree.ContainsKey(new IntVec3(10,0,12)),"bounded charge tree does not cross blockers");
  var route=WindNavigation.Path(tree,start,new IntVec3(11,0,11));Check(route!=null&&route.Count<=12,"charge route available");
  var fx=new MapComponent_Earth(null);var pool=(Array)typeof(MapComponent_Earth).GetField("effects",F).GetValue(fx);Check(pool.Length==96,"bounded cosmetic pool");
  var driver=new JobDriver_EarthTechnique{pawn=Empty<Pawn>(),job=new Job{count=(int)EarthPower.Mountainfall}};
  Set(driver,"startedAt",100);Set(driver,"impactAt",142);Set(driver,"finishAt",150);Set(driver,"impactCommitted",true);Set(driver,"routeIndex",1);Set(driver,"route",new List<IntVec3>{new IntVec3(1,0,1),new IntVec3(2,0,1)});
  Set(driver,"stepFrom",new IntVec3(1,0,1));Set(driver,"expected",new IntVec3(2,0,1));Set(driver,"stepAt",100);
  Check(driver.VisualAt(100)==new IntVec3(1,0,1).ToVector3Shifted()&&driver.VisualAt(102)==new IntVec3(2,0,1).ToVector3Shifted(),"charge interpolation exact endpoints");
  Check(Math.Abs(driver.VisualAt(101).x-2f)<.0001f,"charge interpolation midpoint");
  Set(driver,"chargeOrigin",new IntVec3(0,0,1));Set(driver,"repaths",1);Set(driver,"stepsTaken",2);
  var saved=new EarthSaveCase{cast=driver,comp=new HediffComp_EarthAbilities{autoCombat=false,nonLethal=true,readyAt=new List<int>{100,200,300,400},resolve=2,resolveUntil=900,nextResolve=700,guardUntil=800}};
  string file=Path.Combine(Path.GetTempPath(),"Earth_"+Guid.NewGuid().ToString("N")+".xml");
  Scribe.saver.InitSaving(file,"EarthTest");Scribe_Deep.Look(ref saved,"state");Scribe.saver.FinalizeSaving();
  var xml=new System.Xml.XmlDocument();xml.Load(file);
  Check(xml.SelectSingleNode("EarthTest/state/cast/EH_earthCommitted").InnerText=="True","saved impact cannot replay");
  Check(xml.SelectSingleNode("EarthTest/state/cast/EH_earthImpactAt").InnerText=="142"&&xml.SelectNodes("EarthTest/state/cast/EH_earthRoute/li").Count==2&&xml.SelectSingleNode("EarthTest/state/cast/EH_earthRouteIndex").InnerText=="1","saved timing and movement");
  Check(xml.SelectSingleNode("EarthTest/state/EH_earthAuto").InnerText=="False"&&xml.SelectSingleNode("EarthTest/state/EH_earthNonLethal").InnerText=="True"&&xml.SelectNodes("EarthTest/state/EH_earthReadyAt/li").Count==4,"saved controls/cooldowns");
  Check(xml.SelectSingleNode("EarthTest/state/EH_earthResolveUntil").InnerText=="900"&&xml.SelectSingleNode("EarthTest/state/EH_earthGuardUntil").InnerText=="800","saved expiry deadlines");
  Check(xml.SelectSingleNode("EarthTest/state/cast/EH_earthRepaths").InnerText=="1"&&xml.SelectSingleNode("EarthTest/state/cast/EH_earthStepsTaken").InnerText=="2"&&xml.SelectSingleNode("EarthTest/state/cast/EH_earthStepAt").InnerText=="100","saved interpolation and replanning budget");
  File.Delete(file);
  Console.WriteLine("PASS: "+checks+" Earth checks: Resolve/defense/cooldown bounds, frames, native lineage and pupil identity, promotion state, five NPC/Bestow choices, action replacement, manual target isolation, bounded movement, fixed VFX pool and native Scribe timing/commitment/route/control fields.");
 }
}
public class EarthSaveCase:IExposable {public JobDriver_EarthTechnique cast;public HediffComp_EarthAbilities comp;public void ExposeData(){Scribe_Deep.Look(ref cast,"cast");comp.CompExposeData();}}

