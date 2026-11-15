using RimWorld;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 // A suspended attack is the return path from an automatic technique, not a new
 // player order. Preserve it once, while still respecting genuine queued orders.
 public static class IncarnateCombatFlow {
  public static bool BlockingOrders(Pawn pawn) {
   if(pawn.jobs.jobQueue.Count==0)return false;
   var current=pawn.CurJob;
   if(current==null||(current.def!=JobDefOf.AttackMelee&&current.def!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(pawn,current)))return true;
   Pawn focus=current.def==JobDefOf.AttackMelee?current.targetA.Pawn:null;int inspected=0;
   foreach(var queued in pawn.jobs.jobQueue){
    if(++inspected>32)return true;
    var job=queued.job;if(job==null)return true;
    if(job.def==JobDefOf.Wait_Combat)continue;
    if(job.def==JobDefOf.AttackMelee){
     var target=job.targetA.Pawn;if(target==null)return true;
     if(!IncarnateCombat.Enemy(pawn,target))continue;
     if(focus==null)focus=target;else if(focus!=target)return true;
     continue;
    }
    return true;
   }
   return false;
  }
  public static bool SuspendOnce(Pawn pawn,bool automatic){return automatic&&pawn.jobs.jobQueue.Count==0;}
  public static Pawn Target(Pawn pawn,Pawn fallback){
   var current=pawn.CurJob;var target=current==null?null:current.targetA.Pawn;
   if(IncarnateCombat.Enemy(pawn,target))return target;
   int inspected=0;foreach(var queued in pawn.jobs.jobQueue){if(++inspected>32)break;if(queued.job.def!=JobDefOf.AttackMelee)continue;target=queued.job.targetA.Pawn;if(IncarnateCombat.Enemy(pawn,target))return target;}
   return fallback;
  }
 }
}
