using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public enum EarthPower { Cleave, Advance, Bastion, Mountainfall }
 public static class EarthRules {
  public const int ResolveDuration=480,ResolveInterval=120,BastionDuration=240,VictimLimit=16;
  public static int Stacks(int stacks,int expires,int now){return expires>now?Math.Max(0,Math.Min(3,stacks)):0;}
  public static float Defense(int stacks,bool bastion,float strength){return (1f-.05f*Math.Max(0,Math.Min(3,stacks)))*(bastion?1f-.20f*strength:1f);}
  public static float Impact(int stacks){return 1f+.1f*Math.Max(0,Math.Min(3,stacks));}
  public static int Cooldown(int baseline,float factor){return Math.Max(1,(int)Math.Ceiling(baseline*factor));}
  public static int Frame(int elapsed,int duration){return Math.Max(0,Math.Min(5,(int)((long)Math.Max(0,elapsed)*6/Math.Max(1,duration))));}
 }
 public static class EarthAscendant {
  [DebugAction("Elemental Hearts","Grant Earth ascension",actionType=DebugActionType.ToolMapForPawns,allowedGameStates=AllowedGameStates.PlayingOnMap)]
  public static void Grant(Pawn p){if(p!=null&&ElementalAscension.Apply(p,DefDatabase<HediffDef>.GetNamed("EH_EarthHeart")))Messages.Message(p.LabelShort+" received Earth ascension.",p,MessageTypeDefOf.PositiveEvent,false);}
 }
 public sealed class HediffCompProperties_EarthAbilities:HediffCompProperties {
  public float cooldownFactor=1f;
  public HediffCompProperties_EarthAbilities(){compClass=typeof(HediffComp_EarthAbilities);}
 }
 public sealed class HediffComp_EarthAbilities:HediffComp {
  public bool autoCombat=true,nonLethal;
  public List<int> readyAt=new List<int>{0,0,0,0};
  public int resolve,resolveUntil,nextResolve,guardUntil,combatUntil,activationUntil,nextAutoCheck;
  int lastAdvance=-1;
  public static readonly string[] Names={"Faultline Cleave","Stonebreaker Advance","Bastion Stance","Mountainfall"};
  public static readonly int[] Cooldowns={360,540,1080,1800};
  public static readonly float[] Ranges={8f,6f,0f,8f};
  public static HediffComp_EarthAbilities On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_EarthAbilities>();if(c!=null)return c;}return null;}
  public bool Full{get{return ElementalTeaching.IsHeart(parent);}}
  public bool HasWeapon{get{return Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon;}}
  public bool Busy{get{return EarthCombat.Busy(Pawn);}}
  public bool CanAct{get{return Pawn.Spawned&&!Pawn.Dead&&!Pawn.Downed&&Pawn.Awake()&&!Pawn.InMentalState&&HasWeapon&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&!Pawn.stances.stunner.Stunned&&!ElementalAnimationBridge.Controlled(Pawn)&&!ElementalTeachingSession.Busy(Pawn)&&!HediffComp_ElementalTransform.Transforming(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&Pawn.health.hediffSet.hediffs.Contains(parent);}}
  public int Resolve{get{return Full&&HediffComp_ElementalTransform.Active(Pawn)?EarthRules.Stacks(resolve,resolveUntil,Find.TickManager.TicksGame):0;}}
  public bool GuardActive{get{return Pawn.Spawned&&!Pawn.Dead&&!Pawn.Downed&&Pawn.Awake()&&!Pawn.InMentalState&&!Pawn.stances.stunner.Stunned&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&HasWeapon&&guardUntil>Find.TickManager.TicksGame&&(!Full||HediffComp_ElementalTransform.Active(Pawn));}}
  public ElementalAutoMode Mode{get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  public int Cooldown(EarthPower p){return EarthRules.Cooldown(Cooldowns[(int)p],((HediffCompProperties_EarthAbilities)props).cooldownFactor);}
  public int Remaining(EarthPower p){return Math.Max(0,readyAt[(int)p]-Find.TickManager.TicksGame);}
  public void MarkCombat(){combatUntil=Find.TickManager.TicksGame+180;}
  public void GainResolve(){if(!Full||!HediffComp_ElementalTransform.Active(Pawn))return;int now=Find.TickManager.TicksGame;resolve=Math.Min(3,Resolve+1);resolveUntil=now+EarthRules.ResolveDuration;}
  public void CopyStateTo(HediffComp_EarthAbilities dest){if(dest==null)return;dest.autoCombat=autoCombat;dest.nonLethal=nonLethal;dest.readyAt=new List<int>(readyAt);dest.combatUntil=combatUntil;}
  public void Register(){if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_Earth>().Register(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){Register();}
  public override void Notify_Spawned(){Register();}
  public override void CompPostPostRemoved(){EndGuard();if(Pawn.Spawned)Pawn.Map.GetComponent<MapComponent_Earth>().Forget(Pawn);}
  public override void CompExposeData(){
   Scribe_Collections.Look(ref readyAt,"EH_earthReadyAt",LookMode.Value);if(readyAt==null||readyAt.Count!=4)readyAt=new List<int>{0,0,0,0};
   Scribe_Values.Look(ref autoCombat,"EH_earthAuto",true);Scribe_Values.Look(ref nonLethal,"EH_earthNonLethal");
   Scribe_Values.Look(ref resolve,"EH_earthResolve");Scribe_Values.Look(ref resolveUntil,"EH_earthResolveUntil");Scribe_Values.Look(ref nextResolve,"EH_earthNextResolve");
   Scribe_Values.Look(ref guardUntil,"EH_earthGuardUntil");Scribe_Values.Look(ref combatUntil,"EH_earthCombatUntil");Scribe_Values.Look(ref activationUntil,"EH_earthActivationUntil");Scribe_Values.Look(ref nextAutoCheck,"EH_earthNextAuto");
  }
  public override string CompTipStringExtra{get{return "Resolve: "+Resolve+"/3. Each stack reduces remaining damage by 5%; expires after eight seconds. Mountainfall consumes stacks for +10% impact damage each.";}}
  public override void Notify_PawnPostApplyDamage(DamageInfo info,float dealt){
   if(dealt<=0)return;MarkCombat();int now=Find.TickManager.TicksGame;
   if(now<nextResolve||info.Instigator==null||info.Instigator==Pawn||!info.Instigator.HostileTo(Pawn)||info.Def==null||!info.Def.ExternalViolenceFor(Pawn))return;
   // Repeated damage-over-time ticks do not farm Resolve.
   if(!(info.Instigator is Pawn)&&!(info.Instigator is Building))return;
   if(!Full||!HediffComp_ElementalTransform.Active(Pawn))return;nextResolve=now+EarthRules.ResolveInterval;GainResolve();
  }
  public override void CompPostTickInterval(ref float adjustment,int delta){AdvanceTick(Find.TickManager.TicksGame);}
  internal void AdvanceTick(int now){
   if(now==lastAdvance)return;lastAdvance=now;
   if(Resolve==0){resolve=0;resolveUntil=0;}
   if(!GuardActive)EndGuard();
   if(!Pawn.Spawned||Pawn.Dead||now<nextAutoCheck)return;
   nextAutoCheck=now+60;EarthAutocast.Evaluate(this);
  }
  public void BeginGuard(){
   guardUntil=Find.TickManager.TicksGame+EarthRules.BastionDuration;
   var def=DefDatabase<HediffDef>.GetNamed("EH_EarthBastion");var h=Pawn.health.hediffSet.GetFirstHediffOfDef(def)??Pawn.health.AddHediff(def);
   h.TryGetComp<HediffComp_Disappears>().ticksToDisappear=EarthRules.BastionDuration;
   MarkCombat();Register();ElementalAudio.Ability(Pawn,ElementalSound.Parry);
  }
  public void EndGuard(){guardUntil=0;var def=DefDatabase<HediffDef>.GetNamedSilentFail("EH_EarthBastion");if(def==null||Pawn.health==null)return;var h=Pawn.health.hediffSet.GetFirstHediffOfDef(def);if(h!=null)Pawn.health.RemoveHediff(h);}
  public bool Valid(EarthPower power,Pawn target,bool manual){
   if(power==EarthPower.Bastion)return !Full||HediffComp_ElementalTransform.Active(Pawn);
   if(!WindCombat.AliveTarget(Pawn,target)||!manual&&!WindCombat.Hostile(Pawn,target))return false;
   float range=Ranges[(int)power];if(Pawn.Position.DistanceToSquared(target.Position)>range*range||!LightningCombat.ClearLine(Pawn.Position,target.Position,Pawn.Map))return false;
   IntVec3 landing;return power!=EarthPower.Advance||EarthCombat.FindLanding(Pawn,target,out landing);
  }
  public bool TryCast(EarthPower power,Pawn target,bool automatic){
   if(!CanAct||Busy||EarthCombat.OtherAction(Pawn)||Remaining(power)>0||!Valid(power,target,!automatic))return false;
   if(automatic&&(!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(Pawn),ElementalHeartsMod.Settings.enableAutomation,autoCombat)||Pawn.jobs.jobQueue.Count>0))return false;
   if(power==EarthPower.Bastion){readyAt[(int)power]=Find.TickManager.TicksGame+Cooldown(power);BeginGuard();return true;}
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_EarthTechnique"),target);job.count=(int)power;job.playerForced=!automatic;
   if(automatic){Pawn.stances.CancelBusyStanceHard();Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);}
   else{if(!Pawn.jobs.TryTakeOrderedJob(job,JobTag.Misc))return false;Pawn.stances.CancelBusyStanceHard();}
   if(Pawn.CurJob!=job)return false;
   readyAt[(int)power]=Find.TickManager.TicksGame+Cooldown(power);MarkCombat();return true;
  }
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   if(Full)yield return ElementalTeaching.Command(Pawn);
   if(!Pawn.Drafted)yield break;
   yield return new Command_Action{defaultLabel="Auto Combat: "+ElementalAutoModes.Label(Mode),defaultDesc="Cycle On / Off / Non-lethal. Earth closes gaps, cleaves and releases Mountainfall against crowds or with full Resolve. Bastion can protect you while ordinary melee continues. Non-lethal replaces wounds with the shared suppression effect.",icon=ElementalIcons.AutoCombat,groupable=false,action=()=>{var next=ElementalAutoModes.Next(Mode);autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;}};
   if(Full)yield return new Command_Action{defaultLabel="Resolve: "+Resolve+"/3",defaultDesc=CompTipStringExtra,icon=EarthVfx.Icon(2),groupable=false,action=()=>Messages.Message(CompTipStringExtra,Pawn,MessageTypeDefOf.NeutralEvent,false)};
   for(int i=0;i<4;i++){
    var power=(EarthPower)i;
    string[] details={
     "A narrow eight-cell faultline cleave. Hits up to 16 eligible enemies once, stops at walls, briefly staggers survivors, and grants one Resolve on a successful hostile impact.",
     "Charge up to six cells along a clear, unoccupied route to the target's flank, then strike once. No wall crossing. Rechecks every movement step.",
     "For four seconds, reduce remaining personal damage by 20% and same-faction allies' by 10% within three clear cells. Strongest allied Bastion applies once. Movement is 25% slower. Disciples receive 40% of the defensive effect. Full Ascendants must be transformed.",
     "Mark the selected opponent, then release a three-cell seismic impact after 0.7 seconds. Consume up to three Resolve for +10% impact damage each. Push survivors up to two clear cells. No building damage or permanent terrain."};
    Command cmd;
    if(power==EarthPower.Bastion)cmd=new Command_Action{action=()=>TryCast(power,null,false)};
    else cmd=new Command_Target{targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,neverTargetIncapacitated=true,validator=t=>Valid(power,t.Thing as Pawn,true)},action=t=>TryCast(power,t.Pawn,false),onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,Ranges[(int)power]);}};
    cmd.defaultLabel=Names[i];cmd.defaultDesc=details[i]+" Cooldown: "+Cooldown(power)/60f+"s. Manual casts may select any standing faction. Existing friendly-fire rules govern collateral targets.";cmd.icon=EarthVfx.Icon(i);cmd.groupable=false;
    if(!CanAct)cmd.Disable("Requires an awake, drafted pawn capable of violence and holding a melee weapon.");else if(Busy||EarthCombat.OtherAction(Pawn))cmd.Disable("Finish the current elemental action.");else if(Remaining(power)>0)cmd.Disable("Recharging: "+Math.Ceiling(Remaining(power)/60f)+"s");else if(power==EarthPower.Bastion&&!Valid(power,null,true))cmd.Disable("Transform to use Bastion.");
    yield return cmd;
   }
  }
 }
 public static class EarthAutocast {
  public static void Evaluate(HediffComp_EarthAbilities c){
   Pawn p=c.Pawn;bool controlled=ElementalCombatPolicy.Controlled(p);if(!controlled)c.autoCombat=true;
   if(!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!c.CanAct||p.CurJob==null)return;
   if(c.Busy){c.nextAutoCheck=Find.TickManager.TicksGame+6;return;}
   if(EarthCombat.OtherAction(p)||controlled&&p.jobs.jobQueue.Count>0||p.stances.FullBodyBusy&&!(p.stances.curStance is Stance_Cooldown))return;
   var job=p.CurJob;bool approach=SearchAndDestroyBridge.IsApproach(p,job),pursuing=ElementalCombatFlow.Pursuing(p,job);
   if(controlled&&!approach&&job.def!=JobDefOf.AttackMelee&&job.def!=JobDefOf.Wait_Combat)return;
   Pawn target=job.targetA.Pawn;if(!WindCombat.Hostile(p,target))target=ElementalCombatFlow.Nearest(p,controlled?(pursuing?6f:1.5f):8f);
   c.nextAutoCheck=Find.TickManager.TicksGame+ElementalCombatFlow.ThinkDelay(target!=null);
   if(target==null)return;ElementalCombatFlow.Maintain(p,pursuing);
   bool crowd=ElementalCombatFlow.CountHostiles(p,3f,2)>=2;
   if(!c.GuardActive&&c.Remaining(EarthPower.Bastion)==0&&(crowd||EarthCombat.ThreatenedAlly(p))){if(c.TryCast(EarthPower.Bastion,null,true))c.nextAutoCheck=Find.TickManager.TicksGame+6;}
   if(p.Position.DistanceToSquared(target.Position)>2&&c.TryCast(EarthPower.Advance,target,true))return;
   if((crowd||c.Resolve==3)&&c.TryCast(EarthPower.Mountainfall,target,true))return;
   if(c.TryCast(EarthPower.Cleave,target,true))return;
   ElementalCombatFlow.ResumeMelee(p,target,pursuing);
  }
 }
 public sealed class ThoughtWorker_EarthResolve:ThoughtWorker {
  protected override ThoughtState CurrentStateInternal(Pawn p){return ElementalTeaching.Element(p)=="Earth"?ThoughtState.ActiveAtStage(0):ThoughtState.Inactive;}
 }
}

