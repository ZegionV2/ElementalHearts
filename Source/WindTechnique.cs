using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public sealed class JobDriver_WindTechnique:JobDriver {
  private IntVec3 origin=IntVec3.Invalid,destination=IntVec3.Invalid;
  private int nextStepTick;
  private List<int> struck=new List<int>();
  private List<IntVec3> route=new List<IntVec3>();
  private int routeIndex;
  private HediffComp_WindAbilities cachedComp;
  private HediffComp_WindAbilities Comp {get{return cachedComp??(cachedComp=HediffComp_WindAbilities.On(pawn));}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref origin,"windOrigin");Scribe_Values.Look(ref destination,"windDestination");Scribe_Values.Look(ref nextStepTick,"windNextStep");Scribe_Collections.Look(ref struck,"windStruck",LookMode.Value);if(struck==null)struck=new List<int>();Scribe_Collections.Look(ref route,"windRoute",LookMode.Value);if(route==null)route=new List<IntVec3>();Scribe_Values.Look(ref routeIndex,"windRouteIndex",0);}
  private bool TargetValid(){return WindCombat.Eligible(pawn,TargetA.Pawn);}
  private bool EnsureTarget(float range){
   if(TargetValid()&&pawn.Position.DistanceToSquared(TargetA.Cell)<=range*range&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map))return true;
   // Manual casts retain exactly the selected pawn. Automatic combos spend only their remaining hits.
   if(!ElementalCombatFlow.CarryCombo(job.playerForced,
      ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(pawn),ElementalHeartsMod.Settings.enableAutomation,Comp.autoCombat),
      job.count==(int)WindPower.Rise||job.count==(int)WindPower.Tempest,pawn.jobs.jobQueue.Count))return false;
   Pawn next=ElementalCombatFlow.Nearest(pawn,range,TargetA.Pawn);if(next==null)return false;
   job.targetA=next;return true;
  }
  private void Slash(float factor,int sequence,bool stagger,float windEffectSize=2.1f){
   if(!EnsureTarget(1.5f)||pawn.Position.DistanceToSquared(TargetA.Cell)>2){EndJobWith(JobCondition.Incompletable);return;}
   if(CycloneEvolution.Active(pawn)){
    CycloneVfx.Emit(pawn,(CycloneEffect)sequence,pawn.DrawPos,TargetA.Pawn.DrawPos,24,sequence==3?3.3f:2.6f);
   }else if(sequence==(int)WindPower.Counter){
    WindVfx.Add(pawn.Map,sequence,pawn.DrawPos,pawn.DrawPos,18,3.3f);
   }else{
    WindVfx.Add(pawn.Map,sequence,pawn.DrawPos,TargetA.Pawn.DrawPos,18,windEffectSize);
   }
   bool landed=WindCombat.Strike(pawn,TargetA.Pawn,factor);
   if(landed&&CycloneEvolution.Active(pawn)&&job.count==(int)WindPower.Rise)CycloneAirbound.Apply(pawn,TargetA.Pawn,36,0);
   if(landed&&stagger&&!TargetA.Pawn.Dead&&!TargetA.Pawn.Downed)TargetA.Pawn.stances.stunner.StunFor(Comp.Props.staggerTicks,pawn);
   ElementalAudio.Ability(pawn,job.count==(int)WindPower.Counter?ElementalSound.Spin:job.count==(int)WindPower.Tempest?ElementalSound.Impact:ElementalSound.Slash);
  }
  private void DrawCut(IntVec3 center){
   if(CycloneEvolution.Active(pawn)&&struck.Count>=12)return;
   foreach(var cell in GenRadial.RadialCellsAround(center,1f,true)){
    if(!cell.InBounds(pawn.Map)||!LightningCombat.ClearLine(center,cell,pawn.Map))continue;
    foreach(Pawn victim in cell.GetThingList(pawn.Map).OfType<Pawn>().ToList()){
     if(CycloneEvolution.Active(pawn)&&struck.Count>=12)return;
     if(!WindCombat.Eligible(pawn,victim)||struck.Contains(victim.thingIDNumber))continue;
     struck.Add(victim.thingIDNumber);WindCombat.Strike(pawn,victim,Comp.Props.drawDamage);
    }
   }
  }
  private Toil Move(int kind){
   var toil=new Toil();toil.defaultCompleteMode=ToilCompleteMode.Never;
   toil.initAction=delegate{
    origin=pawn.Position;destination=IntVec3.Invalid;nextStepTick=Find.TickManager.TicksGame;route=null;
    if(kind!=0&&kind!=4&&!EnsureTarget(Comp.Props.tempestRange)){EndJobWith(JobCondition.Incompletable);return;}
    bool ok;
    if(kind==4){destination=job.targetB.Cell;ok=WindCombat.Landing(pawn,destination)&&WindCombat.ClearPath(origin,destination,pawn.Map);}
    else if(kind==0)ok=WindCombat.DrawLanding(pawn,TargetA.Pawn,Comp.Props.drawRange,out destination);
    else ok=WindCombat.Flank(pawn,TargetA.Pawn,Comp.Props.tempestRange,kind!=1,out destination,out route);
    // Crowded melee must not discard an otherwise valid follow-up just because a flank is occupied.
    if(!ok&&kind!=0&&kind!=4&&pawn.Position.DistanceToSquared(TargetA.Cell)<=2&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map)){destination=pawn.Position;ok=true;}
    if(!ok){EndJobWith(JobCondition.Incompletable);return;}
    if(route==null)route=WindCombat.Route(pawn,destination,kind==0);routeIndex=0;
    if(route==null){route=new List<IntVec3>();EndJobWith(JobCondition.Incompletable);return;}
    ElementalAudio.Ability(pawn,ElementalSound.Dash);
    pawn.pather.StopDead();
    if(kind==0||kind==4){if(CycloneEvolution.Active(pawn))CycloneVfx.Emit(pawn,CycloneEffect.DrawTrail,pawn.DrawPos,destination.ToVector3Shifted(),24,2.4f);else WindVfx.Add(pawn.Map,0,pawn.DrawPos,destination.ToVector3Shifted(),18,2f);}
   };
   toil.tickAction=delegate{
    int now=Find.TickManager.TicksGame;if(now<nextStepTick)return;nextStepTick=now+2;
    if(pawn.Position==destination){ReadyForNextToil();return;}
    if(!destination.IsValid||!WindCombat.Landing(pawn,destination)){
     if(kind!=0&&kind!=4&&TargetValid()&&pawn.Position.DistanceToSquared(TargetA.Cell)<=2&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map)){ReadyForNextToil();return;}
     EndJobWith(JobCondition.Incompletable);return;
    }
    if(routeIndex>=route.Count){EndJobWith(JobCondition.Incompletable);return;}
    IntVec3 next=route[routeIndex++];
    if(pawn.Position.DistanceToSquared(next)>2){EndJobWith(JobCondition.Incompletable);return;}
    if(!WindCombat.ClearPath(pawn.Position,next,pawn.Map)){EndJobWith(JobCondition.Incompletable);return;}
    // The draw may pass through its opponent, but never stops on an occupied cell.
    if(kind!=0&&next!=destination&&!WindCombat.Landing(pawn,next)){EndJobWith(JobCondition.Incompletable);return;}
    WindCombat.Step(pawn,next);
    if(pawn.Position==destination){if(kind==0){DrawCut(origin);foreach(var cell in route)DrawCut(cell);if(CycloneEvolution.Active(pawn))ElementalAudio.Ability(pawn,ElementalSound.Slash);}ReadyForNextToil();}
   };
   return toil;
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>{var c=Comp;if(c!=null){c.EndGuard();if(condition==JobCondition.Succeeded||condition==JobCondition.Incompletable)c.nextAutoCheck=Find.TickManager.TicksGame+6;}});
   AddFailCondition(()=>Comp==null||!Comp.CanAct||(job.count==(int)WindPower.Counter&&!TargetValid()));
   if(job.count==4){yield return Move(4);yield return Toils_General.Wait(6);yield break;}
   if(job.count==(int)WindPower.Draw){yield return Move(0);yield return Toils_General.Wait(3);yield break;}
   if(job.count==(int)WindPower.Rise){
    yield return Toils_General.Do(()=>Slash(Comp.Props.riseFirstDamage,1,true));yield return Toils_General.Wait(5);
    yield return Toils_General.Do(()=>{Slash(Comp.Props.riseSecondDamage,1,true);if(Comp!=null)Comp.Status(WindDefs.EH_WindGuard,120,1);});yield break;
   }
   if(job.count==(int)WindPower.Counter){
    var guard=new Toil();guard.defaultCompleteMode=ToilCompleteMode.Never;
    guard.initAction=delegate{if(!CycloneEvolution.Active(pawn)||Comp.guardTarget!=TargetA.Pawn||Comp.guardUntil==0&&!Comp.counterTriggered){Comp.guardTarget=TargetA.Pawn;Comp.counterTriggered=false;Comp.guardUntil=Find.TickManager.TicksGame+Comp.Props.guardDuration;}if(!Comp.counterTriggered)Comp.Status(WindDefs.EH_WindGuard,Math.Max(1,Comp.guardUntil-Find.TickManager.TicksGame),1);pawn.pather.StopDead();};
    guard.tickAction=delegate{
     if(Comp.counterTriggered){ReadyForNextToil();return;}
     if(Find.TickManager.TicksGame>=Comp.guardUntil||pawn.Position.DistanceToSquared(TargetA.Cell)>Comp.Props.guardRange*Comp.Props.guardRange)EndJobWith(JobCondition.Succeeded);
    };
    yield return guard;yield return Move(2);
    yield return Toils_General.Do(delegate{
     Slash(Comp.Props.counterDamage,2,false);
     var swept=new HashSet<int>();int secondary=0;
     foreach(var c in GenRadial.RadialCellsAround(pawn.Position,1.5f,false)){
      if(!c.InBounds(pawn.Map)||!LightningCombat.ClearLine(pawn.Position,c,pawn.Map))continue;
      foreach(var p in c.GetThingList(pawn.Map).OfType<Pawn>().ToList()){if(p==TargetA.Pawn||!WindCombat.Eligible(pawn,p)||!swept.Add(p.thingIDNumber))continue;if(CycloneEvolution.Active(pawn)&&secondary>=6)return;secondary++;WindCombat.Strike(pawn,p,Comp.Props.counterSweepDamage);}
     }
    });yield break;
   }
   yield return Move(1);yield return Toils_General.Do(()=>Slash(Comp.Props.tempestOpeningDamage,3,true));yield return Toils_General.Wait(5);
   yield return Move(2);yield return Toils_General.Do(()=>Slash(Comp.Props.tempestSecondDamage,3,true));yield return Toils_General.Wait(5);
   yield return Move(2);yield return Toils_General.Do(()=>Slash(Comp.Props.tempestFinishDamage,3,true,3.3f));
  }
 }
 public static class WindAutocast {
  private static readonly WindPower[] close={WindPower.Rise,WindPower.Tempest,WindPower.Draw,WindPower.Counter};
  private static readonly WindPower[] distant={WindPower.Draw,WindPower.Tempest};
  public static void Evaluate(HediffComp_WindAbilities c){
   Pawn pawn=c.Pawn;bool controlled=ElementalCombatPolicy.Controlled(pawn);if(!controlled)c.autoCombat=true;
   if(!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!c.CanAct||pawn.CurJob==null)return;
   if(c.Busy){c.nextAutoCheck=Find.TickManager.TicksGame+6;return;}
   if(pawn.stances.FullBodyBusy&&!(pawn.stances.curStance is Stance_Cooldown))return;
   Job job=pawn.CurJob;bool pursuing=ElementalCombatFlow.Pursuing(pawn,job);
   if(controlled&&pawn.jobs.jobQueue.Count>0)return;
   bool approach=SearchAndDestroyBridge.IsApproach(pawn,job),attack=job.def==JobDefOf.AttackMelee;
   if(controlled&&!approach&&!attack&&job.def!=JobDefOf.Wait_Combat)return;
   var sovereign=HediffComp_SovereignCyclone.On(pawn);if(sovereign!=null&&sovereign.TryCast())return;
   Pawn target=job.targetA.Pawn;if(!WindCombat.Hostile(pawn,target))target=null;
   if(target==null&&(!controlled||approach)){target=pawn.mindState.enemyTarget as Pawn;if(!WindCombat.Hostile(pawn,target))target=null;}
   if(target==null)target=ElementalCombatFlow.Nearest(pawn,controlled?(pursuing?6f:1.5f):c.Props.drawRange);
   if(target==null)return; ElementalCombatFlow.Maintain(pawn,pursuing);
   int distance=pawn.Position.DistanceToSquared(target.Position),now=Find.TickManager.TicksGame;
   c.nextAutoCheck=now+ElementalCombatFlow.ThinkDelay(true);
   if(distance<=2&&now>=c.nextRetreatCheck&&c.Remaining(WindPower.Draw)==0){
    c.nextRetreatCheck=now+180;
    if(ElementalCombatFlow.CountHostiles(pawn,1.5f,2)>=2&&Rand.Chance(.25f)&&c.TryRetreat(target))return;
   }
   var choices=distance<=2?close:distant;
   // Keep pressure with attacks; use the waiting counter only when the attack options are exhausted.
   for(int pass=0;pass<2;pass++)for(int i=0;i<choices.Length;i++){
    var power=choices[i];bool repeat=(int)power==c.lastPower;if((pass==0&&repeat)||(pass==1&&!repeat))continue;
    if(power==WindPower.Counter&&(target.CurJob==null||target.CurJob.def!=JobDefOf.AttackMelee||target.CurJob.targetA.Pawn!=pawn))continue;
    if(c.TryCast(power,target,true))return;
   }
   ElementalCombatFlow.ResumeMelee(pawn,target,pursuing);
  }
 }
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class WindCounterGuard {
  [HarmonyPriority(Priority.First+100)]
  public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){
   var comp=HediffComp_WindAbilities.On(__0.Pawn);
   if(comp==null||!comp.TriggerCounter(__instance.CasterPawn))return true;
   __result=new DamageWorker.DamageResult();return false;
  }
  public static void Postfix(Verb_MeleeAttackDamage __instance,DamageWorker.DamageResult __result){if(__result!=null&&__result.totalDamageDealt>0){var c=HediffComp_WindAbilities.On(__instance.CasterPawn);if(c!=null)c.MarkCombat();}}
 }
 [HarmonyPatch(typeof(Verb_MeleeAttack),"SoundDodge")]
 public static class WindCounterDodge {
  public static void Postfix(Verb_MeleeAttack __instance,Thing __0){var c=HediffComp_WindAbilities.On(__0 as Pawn);if(c!=null)c.TriggerCounter(__instance.CasterPawn);}
 }
}


