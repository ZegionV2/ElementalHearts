using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class WaterDefenseRules {
  public static bool Facing(float dot){return dot>=0.5f;} // 120 degree frontal cone.
  public static bool Crossed(float previous,float current,bool sampled){return Math.Abs(current)<=5f||(sampled&&Math.Sign(previous)!=Math.Sign(current)&&Math.Abs(previous)<120f&&Math.Abs(current)<120f);}
  public static bool Ready(bool active,bool awake,bool armed,bool busy,bool frontal,int now,int ready){return active&&awake&&armed&&!busy&&frontal&&now>=ready;}
 }
 public sealed class HediffCompProperties_WaterDefense:HediffCompProperties {
  public int recoveryTicks=45;
  public float parryChance=.8f,counterMultiplier=1.6f;
  public HediffCompProperties_WaterDefense(){compClass=typeof(HediffComp_WaterDefense);}
 }
 public sealed class HediffComp_WaterDefense:HediffComp {
  public bool autoCombat=true,nonLethal;
  public override void CompPostPostAdd(DamageInfo? info){autoCombat=true;}
  public int readyAt;
  private Pawn pending;
  private int pendingAt;
  private int pendingUntil;
  public void CopyStateTo(HediffComp_WaterDefense other){if(other==null)return;other.autoCombat=autoCombat;other.nonLethal=nonLethal;other.readyAt=readyAt;other.pending=pending;other.pendingAt=pendingAt;other.pendingUntil=pendingUntil;}
  public bool PendingCounter{get{return pending!=null;}}
  public HediffCompProperties_WaterDefense Props{get{return (HediffCompProperties_WaterDefense)props;}}
  public ElementalAutoMode Mode{get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  public static HediffComp_WaterDefense On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_WaterDefense>();if(c!=null)return c;}return null;}
  public bool Able{get{return !ElementalTeachingSession.Busy(Pawn)&&!AzureEvolution.Evolving(Pawn)&&!HediffComp_ElementalTransform.Transforming(Pawn)&&ElementalTeaching.Ready(Pawn)&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted);}}
  public bool Active{get{return ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat);}}
  public override void CompExposeData(){Scribe_Values.Look(ref autoCombat,"waterAuto",true);Scribe_Values.Look(ref nonLethal,"waterNonLethal",false);Scribe_Values.Look(ref readyAt,"waterReadyAt",0);Scribe_References.Look(ref pending,"waterCounterTarget");Scribe_Values.Look(ref pendingAt,"waterCounterAt",0);Scribe_Values.Look(ref pendingUntil,"EH_waterCounterUntil",0);}
  public bool Parry(Pawn attacker){
   if(!WaterDefense.Target(Pawn,attacker)||WaterDefense.counterDepth>0)return false;
   int now=Find.TickManager.TicksGame;
   Vector3 toward=(attacker.Position-Pawn.Position).ToVector3().normalized;
   bool facing=WaterDefenseRules.Facing(Vector3.Dot(Pawn.Rotation.FacingCell.ToVector3(),toward));
   if(!WaterDefenseRules.Ready(Active,Able,true,ElementalAnimationBridge.Controlled(Pawn)||Pawn.CurJobDef==DefDatabase<JobDef>.GetNamed("EH_WaterCounter"),facing,now,readyAt))return false;
   // A failed attempt also consumes recovery: multi-packet attacks cannot reroll.
   readyAt=now+Props.recoveryTicks;
   if(!Rand.Chance(Props.parryChance))return false;
   WaterEffects.Emit(Pawn,attacker,false);ElementalAudio.Ability(Pawn,ElementalSound.Parry);ElementalAnimationBridge.Deflect(Pawn,attacker);
   // Counter only hostiles. Neutral/manual friendly attacks can be parried without automatic retaliation.
   if(attacker.HostileTo(Pawn)&&Pawn.Position.DistanceToSquared(attacker.Position)<=2){pending=attacker;pendingAt=now+1;pendingUntil=now+120;}
   return true;
  }
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){
   if(pending==null||Find.TickManager.TicksGame<pendingAt)return;
   int now=Find.TickManager.TicksGame;if(!WaterAbilityRules.CounterLive(now,pendingAt,pendingUntil)){pending=null;return;}
   if(WaterTechnique.Busy(Pawn)||ElementalTeachingSession.Busy(Pawn)||HediffComp_ElementalTransform.Transforming(Pawn)||AzureEvolution.Evolving(Pawn)||ElementalAnimationBridge.ExternallyControlled(Pawn))return;
   Pawn target=pending;pending=null;
   if(!Able||!Active||!WaterDefense.Target(Pawn,target)||!target.HostileTo(Pawn)||Pawn.Position.DistanceToSquared(target.Position)>2||ElementalAnimationBridge.Controlled(Pawn)||ElementalAnimationBridge.Controlled(target)||Pawn.jobs.jobQueue.Count>0)return;
   Job current=Pawn.CurJob;
   if(ElementalCombatPolicy.Controlled(Pawn)&&current!=null&&current.def!=JobDefOf.AttackMelee&&current.def!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,current))return;
   Pawn.jobs.StartJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_WaterCounter"),target),JobCondition.InterruptForced,resumeCurJobAfterwards:true);
  }
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   yield return new Command_Action{defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),defaultDesc=(GlacialEvolution.Active(Pawn)?"Glacial Auto Combat combines Glacial Crescent, Frostwake Step, Crystal Vortex and Absolute Zero. While transformed, Glacial Explosion automatically answers two or more nearby visible enemies with safe knockback, a five-second icy-blue freeze and mild hypothermia. Still Water retains its 80% frontal parry and Returning Tide counters. Damaging hits build Pressure; Absolute Zero spends it. Off disables automatic skills and defense. Non-lethal retains the explosion push/freeze without wounds or hypothermia. Pending counters and controlled actions finish before another technique starts.":"Still Water: 80% chance to parry frontal melee and direct Heart strikes while wielding a melee weapon. 0.75-second recovery between attempts. Nearby hostile attackers trigger Returning Tide, a timed countercut. On automatically combines all four Water techniques, parries and counters. Off disables automation; manual techniques remain available. Non-lethal makes techniques, counters and normal melee use suppression instead of wounds. Successful damaging Water hits build Pressure, and Ocean's Judgement spends it. Pending counters wait briefly for owned techniques to finish."),icon=ElementalIcons.AutoCombat,groupable=false,action=()=>{var next=ElementalAutoModes.Next(Mode);autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;}};
  }
 }
 public static class WaterDefense {
  [ThreadStatic] public static int counterDepth;
  public static bool Target(Pawn p,Pawn other){return p!=null&&other!=null&&p!=other&&p.Spawned&&other.Spawned&&p.Map==other.Map&&!p.Dead&&!other.Dead&&!other.Downed&&GenSight.LineOfSight(p.Position,other.Position,p.Map);}
  public static bool TryParry(Pawn attacker,Pawn victim){var c=HediffComp_WaterDefense.On(victim);return c!=null&&c.Parry(attacker);}
  public static void Counter(Pawn p,Pawn target){
   var c=HediffComp_WaterDefense.On(p);if(c==null||!c.Able||!c.Active||!Target(p,target)||!target.HostileTo(p)||p.Position.DistanceToSquared(target.Position)>2)return;
   var profile=LightningCombat.Profile(p,1f);float amount=profile.damage*c.Props.counterMultiplier*ElementalTeaching.DamageFactor(p);
   counterDepth++;try{if(!ElementalNonLethal.TryApply(p,target,amount)){var result=target.TakeDamage(new DamageInfo(DamageDefOf.Cut,amount,profile.penetration,-1f,p,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,target));if(result!=null&&result.totalDamageDealt>0)WaterPressure.Apply(p,target);}}finally{counterDepth--;}
   WaterEffects.Emit(p,target,true);ElementalAudio.Ability(p,ElementalSound.Slash);
  }
 }
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class WaterMeleeParry {
  [HarmonyPriority(Priority.First+200)]
  public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){if(!WaterDefense.TryParry(__instance.CasterPawn,__0.Pawn))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 public sealed class JobDriver_WaterCounter:JobDriver {
  int elapsed;bool struck,animationStarted,sampled;float previous;
  object animation;
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref elapsed,"waterElapsed");Scribe_Values.Look(ref struck,"waterStruck");}
  protected override IEnumerable<Toil> MakeNewToils(){
   this.FailOn(()=>{var c=HediffComp_WaterDefense.On(pawn);return c==null||!c.Able||!c.Active||!WaterDefense.Target(pawn,TargetA.Pawn)||!TargetA.Pawn.HostileTo(pawn)||pawn.Position.DistanceToSquared(TargetA.Pawn.Position)>2||ElementalAnimationBridge.Controlled(pawn)||ElementalAnimationBridge.Controlled(TargetA.Pawn);});
   var t=new Toil();t.defaultCompleteMode=ToilCompleteMode.Never;
   t.tickAction=()=>{
    pawn.pather.StopDead();pawn.rotationTracker.FaceTarget(TargetA);
    elapsed++;
    if(!animationStarted){animationStarted=true;if(!struck){elapsed=1;animation=ElementalAnimationBridge.StartSwing(pawn,TargetA.Pawn);if(animation==null)pawn.Drawer.Notify_MeleeAttackOn(TargetA.Thing);}}
    bool advancing=animation==null||ElementalAnimationBridge.Advance(animation,elapsed);
    bool impact=animation==null?elapsed>=12:ElementalAnimationBridge.Crossed(pawn,TargetA.Pawn,animation,ref previous,ref sampled);
    if(!struck&&impact){struck=true;WaterDefense.Counter(pawn,TargetA.Pawn);}
    if(animation==null&&elapsed>=24||animation!=null&&(!advancing||!ElementalAnimationBridge.SwingAlive(pawn,animation))||elapsed>=180)ReadyForNextToil();
   };
   t.AddFinishAction(()=>ElementalAnimationBridge.StopSwing(pawn,animation));yield return t;
  }
 }
}
