using System.Collections.Generic;
using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class ElementalTeaching {
  private static Texture2D icon;
  public static bool IsHeart(Hediff h){return h.def.tags!=null && h.def.tags.Contains(ElementalHeartUtility.HeartTag);}
  public static string Element(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs)if(IsHeart(h)){if(h.def.defName==CelestialAscendant.DefName)return "Celestial";if(h.def.defName==CycloneEvolution.DefName)return "Wind";if(h.def.defName==GlacialEvolution.DefName)return "Water";if(h.def.defName==AzureEvolution.DefName)return "Flame";if(h.def.defName==ThunderstormEvolution.DefName)return "Lightning";foreach(string e in new[]{"Lightning","Flame","Wind","Water"})if(h.def.defName=="EH_"+e+"Heart")return e;}return null;}
  public static bool Trained(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.TryGetComp<HediffComp_ElementalApprentice>()!=null)return true;return false;}
  public static float DamageFactor(Pawn p){return Trained(p)&&!ElementalHeartUtility.HasHeart(p)?.4f:1f;}
  public static bool Ready(Pawn p){return p!=null&&p.Spawned&&!p.Dead&&!p.Downed&&p.Awake()&&!p.InMentalState&&!p.stances.stunner.Stunned;}
  public static bool Valid(Pawn teacher,Pawn pupil){return !EvolvedLineage.Active(teacher)&&ElementalTeachingSession.Available(teacher,pupil)&&Ready(teacher)&&Ready(pupil)&&teacher!=pupil&&teacher.Map==pupil.Map&&Element(teacher)!=null&&pupil.RaceProps.Humanlike&&!pupil.IsPrisoner&&!pupil.HostileTo(teacher)&&!pupil.WorkTagIsDisabled(WorkTags.Violent)&&!ElementalHeartUtility.HasHeart(pupil)&&!Trained(pupil);}
  public static Command_Target Command(Pawn teacher){
   if(EvolvedLineage.Active(teacher))return ElementalBestow.Command(teacher);
   if(icon==null)icon=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Teach");
   var c=new Command_Target{defaultLabel="Teach "+Element(teacher),defaultDesc="Walk to a colonist or neutral pawn, then both stop and face each other for a three-second animated lesson. Elemental rings and a flowing spark show power passing from teacher to pupil. They learn this element's four techniques at 40% damage, with 50% longer base cooldowns. One element per pupil. Disciples may call a bound elemental sword and ascend after 50 credited kills and five in-game days of discipleship. No ascension bonuses or teaching power until promoted.",icon=icon,groupable=false,
    targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>Valid(teacher,t.Thing as Pawn)&&teacher.CanReach(t.Thing,PathEndMode.Touch,Danger.Some)},
    action=t=>teacher.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_TeachElemental"),t.Pawn),JobTag.Misc)};
   if(!Ready(teacher))c.Disable("The teacher must be awake and able to act.");return c;
  }
 }
 public sealed class JobDriver_ElementalTeach:JobDriver {
  int startedAt=-1,lastCue=-1;bool committed;
  public bool Committed{get{return committed;}}
  public int ElapsedAt(int tick){return CeremonyTimeline.Elapsed(tick,startedAt,ticksLeftThisToil,CeremonyTimeline.TeachingDuration);}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref startedAt,"EH_teachStarted",-1);Scribe_Values.Look(ref lastCue,"EH_teachCue",-1);Scribe_Values.Look(ref committed,"EH_teachCommitted");}
  void Hold(){ElementalTeachingSession.FaceAndStop(pawn,Pupil);int now=Find.TickManager.TicksGame;if(startedAt<0)startedAt=now-Math.Max(0,CeremonyTimeline.TeachingDuration-ticksLeftThisToil);pawn.Map.GetComponent<MapComponent_DiscipleCeremony>().Track(pawn);int cue=CeremonyTimeline.Cue(ElapsedAt(now),CeremonyTimeline.TeachingDuration);if(cue>lastCue){lastCue=cue;if(cue<2)ElementalAudio.Play(pawn,cue==0?"EH_TransformStart":"EH_TransformCharge");}}
  private Pawn Pupil {get{return job.targetA.Pawn;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return pawn.Reserve(job.targetA,job,1,-1,null,errorOnFailed);}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>ElementalTeachingSession.Release(pawn,Pupil));
   this.FailOn(()=>!ElementalTeaching.Valid(pawn,Pupil));
   yield return Toils_Goto.GotoThing(TargetIndex.A,PathEndMode.Touch);
   yield return Toils_General.Do(()=>ElementalTeachingSession.Begin(pawn,Pupil));
   var lesson=Toils_General.Wait(CeremonyTimeline.TeachingDuration,TargetIndex.A);
   lesson.handlingFacing=true;
   lesson.initAction=Hold;
   lesson.FailOn(()=>!ElementalTeachingSession.Paired(pawn,Pupil));
   lesson.tickAction=Hold;
   lesson.WithProgressBarToilDelay(TargetIndex.A);
   yield return lesson;
   var finish=new Toil();finish.initAction=()=>{if(committed||!ElementalTeaching.Valid(pawn,Pupil)||!ElementalTeachingSession.Paired(pawn,Pupil))return;string element=ElementalTeaching.Element(pawn);Pupil.health.AddHediff(DefDatabase<HediffDef>.GetNamed("EH_"+element+"Training"));committed=true;pawn.Map.GetComponent<MapComponent_DiscipleCeremony>().Complete(Pupil,element,false);ElementalAudio.Play(Pupil,"EH_TransformLock");Messages.Message(Pupil.LabelShort+" learned "+element.ToLower()+" techniques.",Pupil,MessageTypeDefOf.PositiveEvent);};finish.defaultCompleteMode=ToilCompleteMode.Instant;yield return finish;
  }
 }
 public sealed class HediffCompProperties_ElementalApprentice:HediffCompProperties {public HediffCompProperties_ElementalApprentice(){compClass=typeof(HediffComp_ElementalApprentice);}}
 public sealed class HediffComp_ElementalApprentice:HediffComp {
  public int discipleKills;public bool awakeningRolled;private int nextNpcKitCheck;
  public override void Notify_Spawned(){NpcDisciples.EquipDisciple(Pawn,DiscipleElement);}
  public string DiscipleElement{get{return parent.def.defName.Replace("EH_","").Replace("Training","");}}
  public override void CompExposeData(){Scribe_Values.Look(ref discipleKills,"EH_discipleKills");Scribe_Values.Look(ref awakeningRolled,"EH_awakeningRolled");}
  public override string CompTipStringExtra{get{return "Ascension: "+discipleKills+" / 50 kills; "+(parent.ageTicks/60000f).ToString("0.0")+" / 5 days as a disciple.";}}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? dinfo){if(victim!=null&&victim!=Pawn&&!ElementalHeartUtility.HasHeart(Pawn))discipleKills++;}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   foreach(var gizmo in ElementalBoundSwords.Commands(Pawn,DiscipleElement))yield return gizmo;
   yield return new Command_Action {defaultLabel="Disciple: "+discipleKills+"/50 kills",defaultDesc=CompTipStringExtra+" Press Ascend when both requirements are met. NPC disciples start their ceremony automatically. Kills count from discipleship.",icon=DiscipleProgressArt.Icon(discipleKills),groupable=false,action=()=>Messages.Message(CompTipStringExtra,Pawn,MessageTypeDefOf.NeutralEvent,false)};
   yield return DiscipleAscension.Command(Pawn);
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>DiscipleAscension.Apprentice(Pawn)==this&&!ElementalHeartUtility.HasHeart(Pawn),defaultLabel="DEV: Ascend-Ready",defaultDesc="God Mode only. Fulfill this disciple's 50 kills and five days requirements. Then press the normal Ascend button to perform the ceremony and become a normal Ascendant.",icon=DiscipleProgressArt.Icon(50),groupable=false,action=()=>MakeAscendReady()};

  }

  public bool MakeAscendReady(){if(!DebugSettings.godMode||DiscipleAscension.Apprentice(Pawn)!=this||ElementalHeartUtility.HasHeart(Pawn))return false;discipleKills=Math.Max(discipleKills,DiscipleProgressRules.RequiredKills);parent.ageTicks=Math.Max(parent.ageTicks,DiscipleProgressRules.RequiredTicks);return true;}
  public bool TryAscend(){return DiscipleAscension.Begin(Pawn);}
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){
   if(ElementalHeartUtility.HasHeart(Pawn)){Pawn.health.RemoveHediff(parent);return;}
   if(NpcDisciples.AutoNpc(Pawn)&&TryAscend())return;
   if(!Pawn.Spawned||Pawn.Dead)return;
   int now=Find.TickManager.TicksGame;if(now>=nextNpcKitCheck){nextNpcKitCheck=now+60;NpcDisciples.EquipDisciple(Pawn,DiscipleElement);}
   var lightning=parent.TryGetComp<HediffComp_LightningAbilities>();if(lightning!=null&&now>=lightning.nextAutoCheck){LightningAutocast.Evaluate(lightning);}
   var flame=parent.TryGetComp<HediffComp_FlameAbilities>();if(flame!=null&&now>=flame.nextAutoCheck){FlameAutocast.Evaluate(flame);}
  }
 }
}

namespace ElementalHearts {
 public static class DiscipleProgressRules {
  public const int RequiredKills=50,RequiredTicks=300000;
  public static bool Eligible(int kills,int discipleTicks){return kills>=RequiredKills&&discipleTicks>=RequiredTicks;}
 }
}
