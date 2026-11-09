using System;using System.Collections.Generic;using HarmonyLib;using LudeonTK;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public enum CelestialPower {Dawncleaver,Sunstride,CoronaDance,SolarJudgment,HeavenlyZenith,FakeRainbow}
 public static class CelestialRules {
  public const int MaxVictims=16,MaxSearchThings=512,MaxCandidates=32;
  static readonly int[] cooldowns={210,270,480,1080,1500,480};static readonly float[] ranges={12,10,4.5f,14,4.5f,12};
  public static int Cooldown(CelestialPower power){return cooldowns[(int)power];}
  public static float Range(CelestialPower power){return ranges[(int)power];}
  public static int DashDuration(float distance){return Math.Max(5,Math.Min(12,(int)Math.Ceiling(distance)+2));}
  public static float Progress(int age,int duration){float p=Mathf.Clamp01(age/(float)Math.Max(1,duration));return p*p*(3-2*p);}
  public static int Choose(int step,int marks,int crowd,bool[] valid){if(crowd>=2&&valid[4])return 4;if(marks>=2&&valid[3])return 3;if(crowd>=2&&valid[2])return 2;for(int i=0;i<4;i++){int n=(Math.Max(0,step)+i)%4;if(valid[n])return n;}return -1;}
 }
 public static class CelestialAscendant {
  public const string DefName="EH_CelestialAscendant",Technique="EH_CelestialTechnique",Depart="EH_CelestialDepart",Arrive="EH_CelestialArrive";
  public static bool Active(Pawn p){return HediffComp_CelestialAbilities.On(p)!=null;}
  public static bool Busy(Pawn p){if(p==null||p.jobs==null||p.CurJobDef==null)return false;string n=p.CurJobDef.defName;return n==Technique||n==Depart||n==Arrive||n==OtherworldCalling.JobName;}
  [DebugAction("Elemental Hearts","Grant Celestial ascension",actionType=DebugActionType.ToolMapForPawns,allowedGameStates=AllowedGameStates.PlayingOnMap)]
  public static void Grant(Pawn p){if(p!=null&&ElementalAscension.Apply(p,DefDatabase<HediffDef>.GetNamed(DefName)))Messages.Message(p.LabelShort+" received Celestial ascension.",p,MessageTypeDefOf.PositiveEvent,false);}
 }
 public sealed class HediffCompProperties_CelestialAbilities:HediffCompProperties {public HediffCompProperties_CelestialAbilities(){compClass=typeof(HediffComp_CelestialAbilities);}}
 public sealed class HediffComp_CelestialAbilities:HediffComp {
  public string bestowElement="Flame";public bool bestowEvolutionReady=true;public int callingReadyAt,callingCooldownVersion;
  public bool autoCombat=true,nonLethal;public List<int> readyAt=new List<int>{0,0,0,0,0,0};public int nextAutoCheck,comboStep,comboUntil,coronaUntil,mirageArmedUntil;
  public Pawn rainbowTarget;public IntVec3 rainbowRetreat=IntVec3.Invalid,rainbowLanding=IntVec3.Invalid;public int rainbowUntil;public PlanetTile travelDestination=PlanetTile.Invalid;
  Pawn blockSource;int blockUntil;
  public static HediffComp_CelestialAbilities On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_CelestialAbilities>();if(c!=null)return c;}return null;}
  public ElementalAutoMode Mode{get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  public bool Automatic{get{return ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat);}}
  public bool Able{get{return CelestialAscendant.Active(Pawn)&&ElementalTeaching.Ready(Pawn)&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&HediffComp_ElementalTransform.Active(Pawn)&&!HediffComp_ElementalTransform.Transforming(Pawn)&&!ElementalTeachingSession.Busy(Pawn)&&!AzureEvolution.Evolving(Pawn)&&!ElementalAnimationBridge.ExternallyControlled(Pawn)&&Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon;}}
  public bool CanAct{get{return Able&&!CelestialAscendant.Busy(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&rainbowTarget==null;}}
  public override void CompPostPostAdd(DamageInfo? info){autoCombat=true;if(Pawn.Spawned){CelestialVfx.Manifest(Pawn,90);Pawn.Map.GetComponent<MapComponent_CelestialVfx>().Track(Pawn);}}
  public override void Notify_Spawned(){if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_CelestialVfx>().Track(Pawn);}
  public override void CompExposeData(){Scribe_Values.Look(ref callingReadyAt,"EH_celestialCallingReadyAt");Scribe_Values.Look(ref callingCooldownVersion,"EH_callingCooldownVersion");Scribe_Values.Look(ref bestowElement,"EH_celestialBestowElement","Flame");Scribe_Values.Look(ref bestowEvolutionReady,"EH_celestialBestowEvolutionReady",true);if(Array.IndexOf(CelestialBestowOptions.Elements,bestowElement)<0)bestowElement="Flame";Scribe_Values.Look(ref autoCombat,"EH_celestialAuto",true);Scribe_Values.Look(ref nonLethal,"EH_celestialNonLethal");Scribe_Collections.Look(ref readyAt,"EH_celestialReadyAt",LookMode.Value);Scribe_Values.Look(ref nextAutoCheck,"EH_celestialAutoCheck");Scribe_Values.Look(ref comboStep,"EH_celestialComboStep");Scribe_Values.Look(ref comboUntil,"EH_celestialComboUntil");Scribe_Values.Look(ref coronaUntil,"EH_celestialCoronaUntil");Scribe_Values.Look(ref mirageArmedUntil,"EH_celestialMirageArmed");Scribe_References.Look(ref rainbowTarget,"EH_celestialRainbowTarget");Scribe_Values.Look(ref rainbowRetreat,"EH_celestialRainbowRetreat",IntVec3.Invalid);Scribe_Values.Look(ref rainbowLanding,"EH_celestialRainbowLanding",IntVec3.Invalid);Scribe_Values.Look(ref rainbowUntil,"EH_celestialRainbowUntil");Scribe_Values.Look(ref travelDestination,"EH_celestialTravelDestination",PlanetTile.Invalid);if(readyAt==null)readyAt=new List<int>();while(readyAt.Count<6)readyAt.Add(0);if(readyAt.Count>6)readyAt.RemoveRange(6,readyAt.Count-6);}
  public bool Ready(CelestialPower power){return Find.TickManager.TicksGame>=readyAt[(int)power];}
  public bool TryCast(CelestialPower power,Pawn target,bool automatic){
   if(power==CelestialPower.FakeRainbow)return ArmRainbow();if(!CanAct||!Ready(power)||power<0||power>CelestialPower.HeavenlyZenith)return false;
   IntVec3 landing=IntVec3.Invalid;
   using(new ElementalManualTarget(Pawn,automatic?null:target)){
    if(power==CelestialPower.HeavenlyZenith){if(CelestialCombat.Crowd(Pawn)<2)return false;target=Pawn;}
    else if(power==CelestialPower.CoronaDance)target=Pawn;
    else if(!CelestialCombat.Eligible(Pawn,target)||Pawn.Position.DistanceToSquared(target.Position)>Math.Pow(CelestialRules.Range(power),2)||!LightningCombat.ClearLine(Pawn.Position,target.Position,Pawn.Map))return false;
    if(power==CelestialPower.Sunstride&&!CelestialCombat.Landing(Pawn,Pawn.Position,target,CelestialRules.Range(power),out landing))return false;
   }
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(CelestialAscendant.Technique),target,landing);job.count=(int)power;job.playerForced=!automatic;
   Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:automatic,cancelBusyStances:true);if(Pawn.CurJob!=job)return false;readyAt[(int)power]=Find.TickManager.TicksGame+CelestialRules.Cooldown(power);return true;
  }
  public bool ArmRainbow(){if(!CanAct||!Ready(CelestialPower.FakeRainbow))return false;mirageArmedUntil=Find.TickManager.TicksGame+180;CelestialVfx.Emit(Pawn,CelestialEffect.RainbowGlint,Pawn.DrawPos,Pawn.DrawPos,24,1.5f,1.5f,Pawn);return true;}
  public bool Parry(Pawn attacker){
   int now=Find.TickManager.TicksGame;if(CelestialCombat.counterDepth>0||WaterDefense.counterDepth>0)return false;if(attacker!=null&&attacker==blockSource&&now<=blockUntil)return true;
   if(CelestialCombat.counterDepth>0||WaterDefense.counterDepth>0||(!Automatic&&now>=mirageArmedUntil)||!CanAct||!Ready(CelestialPower.FakeRainbow)||!CelestialCombat.Hostile(Pawn,attacker)||Pawn.Position.DistanceToSquared(attacker.Position)>144)return false;
   IntVec3 retreat,landing;if(!CelestialCombat.Retreat(Pawn,attacker,out retreat)||!CelestialCombat.Landing(Pawn,retreat,attacker,16,out landing))return false;
   readyAt[5]=now+CelestialRules.Cooldown(CelestialPower.FakeRainbow);mirageArmedUntil=0;rainbowTarget=attacker;rainbowRetreat=retreat;rainbowLanding=landing;rainbowUntil=now+120;blockSource=attacker;blockUntil=now+1;
   CelestialVfx.Emit(Pawn,CelestialEffect.RainbowMirage,Pawn.DrawPos,Pawn.DrawPos,32,1.8f,1.8f);CelestialVfx.Emit(Pawn,CelestialEffect.RetreatFlash,Pawn.DrawPos,Pawn.DrawPos,12,1.7f,1.7f);ElementalAudio.Ability(Pawn,ElementalSound.Parry);return true;
  }
  void PendingRainbow(){
   if(rainbowTarget==null)return;int now=Find.TickManager.TicksGame;
   if(now>rainbowUntil||!Able||!CelestialCombat.Hostile(Pawn,rainbowTarget)||Pawn.jobs.jobQueue.Count>0||!AutoJob()){rainbowTarget=null;return;}
   if(CelestialAscendant.Busy(Pawn)||ElementalAnimationBridge.Controlled(Pawn))return;
   var target=rainbowTarget;rainbowTarget=null;var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(CelestialAscendant.Technique),target,rainbowRetreat,rainbowLanding);job.count=5;Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true,cancelBusyStances:true);
  }
  bool AutoJob(){if(!ElementalCombatPolicy.Controlled(Pawn))return true;var j=Pawn.CurJob;return j!=null&&(j.def==JobDefOf.AttackMelee||j.def==JobDefOf.Wait_Combat||SearchAndDestroyBridge.IsApproach(Pawn,j));}
  void Auto(){
   int now=Find.TickManager.TicksGame;nextAutoCheck=now+12;if(!Automatic||!CanAct||Pawn.jobs.jobQueue.Count>0||!AutoJob()){nextAutoCheck=now+45;return;}
   var target=Pawn.CurJob==null?null:Pawn.CurJob.targetA.Pawn;if(!CelestialCombat.Hostile(Pawn,target)||Pawn.Position.DistanceToSquared(target.Position)>196)target=CelestialCombat.Nearest(Pawn,14);if(target==null){nextAutoCheck=now+45;return;}
   int crowd=CelestialCombat.Crowd(Pawn);bool[] valid=new bool[5];for(int i=0;i<5;i++){var power=(CelestialPower)i;valid[i]=Ready(power)&&(i==2?Pawn.Position.DistanceToSquared(target.Position)<=20.25:i==4?crowd>=2:Pawn.Position.DistanceToSquared(target.Position)<=Math.Pow(CelestialRules.Range(power),2));}
   if(now>=comboUntil)comboStep=0;int chosen=CelestialRules.Choose(comboStep,CelestialRadiance.Stacks(Pawn,target),crowd,valid);if(chosen>=0)TryCast((CelestialPower)chosen,target,true);
  }
  public void Completed(CelestialPower power){comboStep=((int)power+1)%4;comboUntil=Find.TickManager.TicksGame+480;nextAutoCheck=Find.TickManager.TicksGame+8;}
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){if(!Pawn.Spawned||Pawn.Dead)return;OtherworldCalling.UpgradeCooldown(Pawn);int now=Find.TickManager.TicksGame;if(now%30<delta)Pawn.Map.GetComponent<MapComponent_CelestialVfx>().Track(Pawn);if(Pawn.Downed){rainbowTarget=null;return;}PendingRainbow();if(now>=nextAutoCheck)Auto();}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   yield return new Command_Action{defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),defaultDesc="Automatically combine solar sword forms, Radiance finishers, Heavenly Zenith and Fake Rainbow. Off disables automatic attacks and parries; manual skills remain usable. Non-lethal replaces wounds with suppression, including normal melee. Bestow and Light Travel are always manual.",icon=ElementalIcons.AutoCombat,groupable=false,action=()=>{var mode=ElementalAutoModes.Next(Mode);autoCombat=mode!=ElementalAutoMode.Off;nonLethal=mode==ElementalAutoMode.NonLethal;}};
   yield return ElementalBestow.Command(Pawn);yield return CelestialLightTravel.Command(Pawn);yield return OtherworldCalling.Command(Pawn);if(DebugSettings.godMode)yield return OtherworldCalling.DevCommand(Pawn);
   if(!Pawn.Drafted)yield break;
   string[] labels={"Dawncleaver","Sunstride","Corona Dance","Solar Judgment","Heavenly Zenith","Fake Rainbow"};string[] desc={"A broad golden sword crescent strikes up to sixteen distinct enemies along a twelve-cell lane. Applies caster-owned Radiance. One impact per target.","Dash up to ten clear cells in a streak of light, leave golden afterimages and finish with a committed sword strike. Cannot cross walls or occupied cells.","Spin through flowing solar sword forms in a 4.5-cell circle. One shared impact per enemy across the entire animation, adding Radiance.","Telegraph a descending solar pillar up to fourteen cells away. Consume your own Radiance on the primary target for a stronger finishing strike, with a bounded nearby burst.","Automatically answers two or more visible hostiles within 4.5 cells. Gather a miniature sun, release one radiant strike per enemy, push survivors along clear cells and gain a three-second defensive corona. Can also be pressed under the same crowd condition.","Reactive parry: leave a rainbow mirage, retreat safely, then counter-dash with light trails and one rainbow slash. Automatic with Auto Combat. Press manually to prepare a three-second parry window. A clear retreat and return route are required. Counterstrikes cannot recursively parry each other."};
   for(int i=0;i<6;i++){var power=(CelestialPower)i;Command c;
    if(i==0||i==1||i==3)c=new Command_Target{defaultLabel=labels[i],defaultDesc=desc[i]+" No resource cost.",icon=CelestialVisualAssets.Icon(i),groupable=false,targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>CelestialCombat.Eligible(Pawn,t.Thing as Pawn)},action=t=>TryCast(power,t.Pawn,false)};
    else c=new Command_Action{defaultLabel=labels[i],defaultDesc=desc[i]+" No resource cost.",icon=CelestialVisualAssets.Icon(i),groupable=false,action=()=>TryCast(power,Pawn,false)};
    if(!CanAct)c.Disable("Transform, wield a melee weapon and finish the current action first.");else if(!Ready(power))c.Disable("Recharging: "+Math.Ceiling((readyAt[i]-Find.TickManager.TicksGame)/60f)+"s");else if(i==4&&CelestialCombat.Crowd(Pawn)<2)c.Disable("Requires at least two nearby visible hostiles.");yield return c;
   }
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class CelestialDamageParry {
  [HarmonyPriority(Priority.First+250)]public static bool Prefix(Thing __instance,DamageInfo dinfo,ref DamageWorker.DamageResult __result){var p=__instance as Pawn;var c=HediffComp_CelestialAbilities.On(p);var attacker=dinfo.Instigator as Pawn;if(c==null||attacker==null||dinfo.Amount<=0||dinfo.Def==null||!dinfo.Def.ExternalViolenceFor(p)||!c.Parry(attacker))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class CelestialMeleeParry {
  [HarmonyPriority(Priority.First+250)]public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){var c=HediffComp_CelestialAbilities.On(__0.Pawn);if(c==null||!c.Parry(__instance.CasterPawn))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class CelestialSovereignWard {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){var p=__instance as Pawn;var c=HediffComp_CelestialAbilities.On(p);if(c==null||dinfo.Amount<=0)return;var source=dinfo.Instigator as Pawn;float factor=source!=null&&ElementalHeartUtility.HasHeart(source)&&!CelestialAscendant.Active(source)&&!EclipseSky.Empowered(source)?.2f:1f;if(Find.TickManager.TicksGame<c.coronaUntil)factor*=.35f;dinfo.SetAmount(dinfo.Amount*factor);}
 }
}


