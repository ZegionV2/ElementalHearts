using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class FlameAutocast {
  public static void Evaluate(HediffComp_FlameAbilities c) {
   Pawn pawn=c.Pawn; int now=Find.TickManager.TicksGame; if(now<c.nextAutoCheck)return; c.nextAutoCheck=now+60;
   bool controlled=ElementalCombatPolicy.Controlled(pawn);
   if(!controlled)c.autoCombat=true;
   if(!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!c.CanAct||pawn.CurJob==null)return;
   if(pawn.stances.FullBodyBusy && !(pawn.stances.curStance is Stance_Cooldown))return;
   Job job=pawn.CurJob;
   if(job.def==FlameDefs.EH_PhoenixDescent){c.nextAutoCheck=now+6;return;}
   bool pursuing=ElementalCombatFlow.Pursuing(pawn,job);
   if(controlled && pawn.jobs.jobQueue.Count>0)return;
   bool attacking=job.def==JobDefOf.AttackMelee;
   bool approaching=SearchAndDestroyBridge.IsApproach(pawn,job);
   if(controlled && !attacking && !approaching && job.def!=JobDefOf.Wait_Combat)return;



   var reversal=HediffComp_AzureReversal.On(pawn);if(reversal!=null&&reversal.TryStart(true))return;
   Pawn target=(attacking||approaching||!controlled)?job.targetA.Pawn:null;
   if(!Hostile(pawn,target))target=null;
   if(target==null && (approaching||!controlled)){
    Pawn intended=pawn.mindState.enemyTarget as Pawn;
    if(Hostile(pawn,intended))target=intended;
   }
   if(target==null)target=ElementalCombatFlow.Nearest(pawn,controlled?(pursuing?6f:1.5f):Math.Max(c.Props.severRange,c.Props.flashRange));
   if(target==null)return; ElementalCombatFlow.Maintain(pawn,pursuing); c.nextAutoCheck=now+ElementalCombatFlow.ThinkDelay(true);
   float distance=pawn.Position.DistanceToSquared(target.Position);
   // At most one 25% escape roll per three seconds; consumes the normal dash cooldown.
   if(distance<=2 && now>=c.nextRetreatCheck && c.Remaining(FlamePower.Flash)==0){
    c.nextRetreatCheck=now+180;
    if(ElementalCombatFlow.CountHostiles(pawn,1.5f,2)>=2&&Rand.Chance(.25f)&&c.TryRetreat(target))return;
   }
   int enemies=c.Remaining(FlamePower.Dance)==0?ElementalCombatFlow.CountHostiles(pawn,c.Props.danceRadius,CourtTactics.CrowdThreshold(pawn,ElementalHeartsMod.Settings.danceThreshold)):0;
   if(enemies>=CourtTactics.CrowdThreshold(pawn,ElementalHeartsMod.Settings.danceThreshold)){if(TryAuto(c,FlamePower.Dance,pawn,true))return;}
   IntVec3 landing;
   if(c.Remaining(FlamePower.Flash)==0&&distance>9&&FlameCombat.TryLanding(pawn,target,c.Props.flashRange,out landing)){if(TryAuto(c,FlamePower.Flash,target,true))return;}
   if(c.Remaining(FlamePower.Sever)==0&&distance<=c.Props.severRange*c.Props.severRange&&FlameCombat.ClearLine(pawn.Position,target.Position,pawn.Map)){if(TryAuto(c,FlamePower.Sever,target,true))return;}
   if(enemies>0&&distance<=c.Props.danceRadius*c.Props.danceRadius){if(TryAuto(c,FlamePower.Dance,pawn,true))return;}
   if(c.Remaining(FlamePower.Heaven)==0&&FlameCombat.TryLanding(pawn,target,c.Props.heavenRange,out landing)){if(TryAuto(c,FlamePower.Heaven,target,true))return;}
   ElementalCombatFlow.ResumeMelee(pawn,target,pursuing);
  }
  static bool TryAuto(HediffComp_FlameAbilities c,FlamePower power,LocalTargetInfo target,bool automatic){int before=c.Remaining(power);var job=c.Pawn.CurJob;c.TryCast(power,target,automatic);return c.Remaining(power)>before||c.Pawn.CurJob!=job;}
  public static bool Hostile(Pawn source,Pawn target){return FlameCombat.Eligible(source,target)&&!target.IsPrisoner&&target.HostileTo(source);}
 }
}





