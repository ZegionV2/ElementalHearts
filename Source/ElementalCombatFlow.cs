using System;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public static class ElementalCombatFlow {
  private sealed class Engagement {public Engagement(){} public int until;}
  private static readonly ConditionalWeakTable<Pawn,Engagement> engagements=new ConditionalWeakTable<Pawn,Engagement>();
  public static int ThinkDelay(bool hasTarget){return hasTarget?12:60;}
  public static void Maintain(Pawn pawn,bool pursuing){if(pursuing)engagements.GetOrCreateValue(pawn).until=Find.TickManager.TicksGame+120;}
  public static bool CarryCombo(bool manual,bool enabled,bool multiStrike,int queued){return !manual&&enabled&&multiStrike&&queued<=1;}
  public static bool ContinuePursuit(bool attackingHostile,bool approach,bool waiting,bool forced,bool queued,bool recent){
   return !queued&&(attackingHostile||approach||(waiting&&!forced&&recent));
  }
  // Only an existing combat order authorizes pursuit. Explicit hold/move/work orders cancel it.
  public static bool Pursuing(Pawn pawn,Job job){
   var state=engagements.GetOrCreateValue(pawn);int now=Find.TickManager.TicksGame;
   Pawn target=job.targetA.Pawn;
   bool attacking=job.def==JobDefOf.AttackMelee&&target!=null&&!target.IsPrisoner&&target.HostileTo(pawn);
   bool approach=SearchAndDestroyBridge.IsApproach(pawn,job);
   bool queued=pawn.jobs.jobQueue.Count>0;
   bool active=ContinuePursuit(attacking,approach,job.def==JobDefOf.Wait_Combat,job.playerForced,queued,now<state.until);
   if(attacking||approach){if(!queued)state.until=now+120;}
   else if(!active)state.until=0;
   return active;
  }
  public static bool Hostile(Pawn source,Pawn target){return target!=null&&target!=source&&target.Spawned&&!target.Dead&&!target.Downed&&target.Map==source.Map&&!target.IsPrisoner&&target.HostileTo(source);}
  public static Pawn Nearest(Pawn source,float radius,Pawn excluded=null){
   Pawn best=null;int distance=int.MaxValue;
   foreach(var cell in GenRadial.RadialCellsAround(source.Position,radius,true)){
    if(!cell.InBounds(source.Map))continue;
    int d=source.Position.DistanceToSquared(cell);if(d>distance)continue;
    var things=cell.GetThingList(source.Map);
    for(int i=0;i<things.Count;i++){
     var p=things[i] as Pawn;if(p==excluded||!Hostile(source,p)||(d==distance&&best!=null&&p.thingIDNumber>=best.thingIDNumber))continue;
     if(!LightningCombat.ClearLine(source.Position,cell,source.Map))continue;
     best=p;distance=d;
    }
   }
   return best;
  }
  public static int CountHostiles(Pawn source,float radius,int limit){
   int count=0;
   foreach(var cell in GenRadial.RadialCellsAround(source.Position,radius,true)){
    if(!cell.InBounds(source.Map))continue;var things=cell.GetThingList(source.Map);
    for(int i=0;i<things.Count;i++)if(Hostile(source,things[i] as Pawn)&&LightningCombat.ClearLine(source.Position,cell,source.Map)){if(++count>=limit)return count;}
   }
   return count;
  }
  public static void ResumeMelee(Pawn pawn,Pawn target,bool pursuing){
   if(!pursuing||!Hostile(pawn,target)||pawn.jobs.jobQueue.Count>0)return;
   if(pawn.CurJob!=null&&pawn.CurJob.def==JobDefOf.AttackMelee&&Hostile(pawn,pawn.CurJob.targetA.Pawn))return;
   Job next=JobMaker.MakeJob(JobDefOf.AttackMelee,target);next.maxNumMeleeAttacks=int.MaxValue;
   next.expiryInterval=120;next.checkOverrideOnExpire=true;
   pawn.jobs.StartJob(next,JobCondition.InterruptForced);
  }
 }
}
