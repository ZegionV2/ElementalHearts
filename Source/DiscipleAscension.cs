using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class DiscipleAscension {
  public const string JobName="EH_AscendDisciple";
  public const int Duration=210;
  public static HediffComp_ElementalApprentice Apprentice(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_ElementalApprentice>();if(c!=null)return c;}return null;}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==JobName;}
  public static string Problem(Pawn p,bool duringCeremony=false){
   var c=Apprentice(p);if(c==null||ElementalHeartUtility.HasHeart(p))return "Requires an unascended elemental disciple.";
   if(!DiscipleProgressRules.Eligible(c.discipleKills,c.parent.ageTicks))return "Requires 50 credited kills and five in-game days as a disciple.";
   if(!ElementalTeaching.Ready(p))return "Must be conscious, awake and able to act.";
   if(Busy(p)&&!duringCeremony)return "Ascension is already in progress.";
   if(ElementalTeachingSession.Busy(p)&&!Busy(p)||ElementalAnimationBridge.Controlled(p)||HediffComp_ElementalTransform.Transforming(p)||AzureEvolution.Evolving(p))return "Finish the current teaching, transformation or controlled action first.";
   if(p.CurJobDef!=null&&p.CurJobDef.defName.StartsWith("EH_")&&!Busy(p))return "Finish the current elemental action first.";
   return null;
  }
  public static bool Begin(Pawn p){if(Problem(p)!=null)return false;var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(JobName),p.Position);job.playerForced=ElementalCombatPolicy.Controlled(p);return p.jobs.TryTakeOrderedJob(job,JobTag.Misc);}
  public static Command_Action Command(Pawn p){var c=new Command_Action{defaultLabel="Ascend",defaultDesc="Become a normal "+(Apprentice(p)==null?"elemental":Apprentice(p).DiscipleElement)+" Ascendant after 50 credited kills and five disciple days. Stand still for a 3.5-second elemental ascension ceremony. Interrupted ceremonies grant nothing and can be retried. No resource cost.",icon=DiscipleProgressArt.Icon(50),groupable=false,action=()=>Begin(p)};string problem=Problem(p);if(problem!=null)c.Disable(problem);return c;}
  public static bool Complete(Pawn p){if(!Busy(p)||Problem(p,true)!=null)return false;var c=Apprentice(p);string element=c.DiscipleElement;var def=DefDatabase<HediffDef>.GetNamedSilentFail("EH_"+element+"Heart");if(def==null||!ElementalAscension.Apply(p,def))return false;p.Map.GetComponent<MapComponent_DiscipleCeremony>().Complete(p,element,true);ElementalAudio.Play(p,"EH_TransformLock");Messages.Message(p.LabelShort+" has ascended as a "+element+" Ascendant!",p,MessageTypeDefOf.PositiveEvent,false);return true;}
 }
 public sealed class JobDriver_AscendDisciple:JobDriver {
  int startedAt=-1,lastCue=-1;bool committed;
  public bool Committed{get{return committed;}}
  public int ElapsedAt(int tick){return CeremonyTimeline.Elapsed(tick,startedAt,ticksLeftThisToil,DiscipleAscension.Duration);}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref startedAt,"EH_discipleAscendStarted",-1);Scribe_Values.Look(ref lastCue,"EH_discipleAscendCue",-1);Scribe_Values.Look(ref committed,"EH_discipleAscendCommitted");}
  void Hold(){pawn.pather.StopDead();int now=Find.TickManager.TicksGame;if(startedAt<0)startedAt=now-Math.Max(0,DiscipleAscension.Duration-ticksLeftThisToil);pawn.Map.GetComponent<MapComponent_DiscipleCeremony>().Track(pawn);int cue=CeremonyTimeline.Cue(ElapsedAt(now),DiscipleAscension.Duration);if(cue>lastCue){lastCue=cue;ElementalAudio.Play(pawn,cue==0?"EH_TransformStart":cue==1?"EH_TransformCharge":"EH_TransformLock");}}
  protected override IEnumerable<Toil> MakeNewToils(){
   this.FailOn(()=>!committed&&(DiscipleAscension.Problem(pawn,true)!=null||pawn.Position!=job.targetA.Cell));
   var ritual=Toils_General.Wait(DiscipleAscension.Duration);ritual.handlingFacing=true;ritual.initAction=Hold;ritual.tickAction=Hold;ritual.WithProgressBarToilDelay(TargetIndex.A);yield return ritual;
   yield return Toils_General.Do(()=>{if(!committed&&DiscipleAscension.Complete(pawn))committed=true;});
  }
 }
}
