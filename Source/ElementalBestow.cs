using System;using System.Collections.Generic;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class BestowRules {
  public const int Duration=240;
  public static bool Recipient(bool humanlike,bool prisoner,bool hostile,bool violent,bool hasHeart,bool trained,bool sameElement){return humanlike&&!prisoner&&!hostile&&violent&&!hasHeart&&(!trained||sameElement);}
  public static float Progress(int elapsed){return Mathf.Clamp01(elapsed/(float)Duration);}
  public static float Energy(int elapsed){float p=Progress(elapsed);return Mathf.Clamp01(p/.25f)*Mathf.Clamp01((1-p)/.12f);}
  public static int Cue(int elapsed){return elapsed<60?0:elapsed<190?1:2;}
 }
 public static class ElementalBestow {
  public const string Giving="EH_BestowElemental",Receiving="EH_ReceiveElementalBestow";
  static Texture2D icon;
  static bool JobIs(Pawn p,string name){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==name;}
  public static bool Busy(Pawn p){return JobIs(p,Giving)||JobIs(p,Receiving);}
  static bool SameTraining(Pawn pupil,string element){foreach(var h in pupil.health.hediffSet.hediffs)if(h.TryGetComp<HediffComp_ElementalApprentice>()!=null&&h.def.defName!="EH_"+element+"Training")return false;return true;}
  public static bool Available(Pawn giver,Pawn pupil){
   if(giver==null||pupil==null||EarthCombat.Busy(giver)||EarthCombat.Busy(pupil)||DiscipleAscension.Busy(giver)||DiscipleAscension.Busy(pupil)||giver.jobs==null||pupil.jobs==null||ElementalAnimationBridge.Controlled(giver)||ElementalAnimationBridge.Controlled(pupil)||AzureEvolution.Evolving(giver)||AzureEvolution.Evolving(pupil)||HediffComp_ElementalTransform.Transforming(giver)||HediffComp_ElementalTransform.Transforming(pupil))return false;
   if(giver.CurJob!=null&&(giver.CurJob.def.defName==ElementalTeachingSession.Teaching||giver.CurJob.def.defName==ElementalTeachingSession.Receiving)||pupil.CurJob!=null&&(pupil.CurJob.def.defName==ElementalTeachingSession.Teaching||pupil.CurJob.def.defName==ElementalTeachingSession.Receiving))return false;
   return !JobIs(giver,Receiving)&&!JobIs(pupil,Giving)&&(!JobIs(pupil,Receiving)||pupil.CurJob.targetA.Pawn==giver);
  }
  public static bool Valid(Pawn giver,Pawn pupil){return Available(giver,pupil)&&ElementalTeaching.Ready(giver)&&ElementalTeaching.Ready(pupil)&&giver!=pupil&&giver.Map==pupil.Map&&EvolvedLineage.Active(giver)&&ElementalTeaching.Element(giver)!=null&&BestowRules.Recipient(pupil.RaceProps.Humanlike,pupil.IsPrisoner,pupil.HostileTo(giver),!pupil.WorkTagIsDisabled(WorkTags.Violent),ElementalHeartUtility.HasHeart(pupil),ElementalTeaching.Trained(pupil),CelestialAscendant.Active(giver)||SameTraining(pupil,ElementalTeaching.Element(giver)));}
  public static bool Paired(Pawn giver,Pawn pupil){return ElementalTeaching.Ready(giver)&&ElementalTeaching.Ready(pupil)&&giver.Map==pupil.Map&&TeachingPairRules.Valid(JobIs(giver,Giving),JobIs(pupil,Receiving),giver.CurJob!=null&&giver.CurJob.targetA.Pawn==pupil,pupil.CurJob!=null&&pupil.CurJob.targetA.Pawn==giver,giver.Position.DistanceToSquared(pupil.Position));}
  public static void Begin(Pawn giver,Pawn pupil){if(!Valid(giver,pupil)||giver.Position.DistanceToSquared(pupil.Position)>2)return;if(!JobIs(pupil,Receiving)||pupil.CurJob.targetA.Pawn!=giver){var receive=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Receiving),giver);receive.playerForced=true;pupil.jobs.StartJob(receive,JobCondition.InterruptForced,resumeCurJobAfterwards:true,cancelBusyStances:true,tag:JobTag.Misc,keepCarryingThingOverride:true);}ElementalTeachingSession.FaceAndStop(giver,pupil);}
  public static void Release(Pawn giver,Pawn pupil){if(JobIs(pupil,Receiving)&&pupil.CurJob.targetA.Pawn==giver)pupil.jobs.EndCurrentJob(JobCondition.InterruptForced,true);}
  public static bool Grant(Pawn giver,Pawn pupil,string outcome=null){
   if(!Valid(giver,pupil)||!Paired(giver,pupil))return false;
   bool celestial=CelestialAscendant.Active(giver);string element=celestial?outcome:ElementalTeaching.Element(giver);
   if(celestial&&Array.IndexOf(NpcDisciples.Elements,element)<0)return false;
   var def=DefDatabase<HediffDef>.GetNamedSilentFail("EH_"+element+"Heart");
   if(def==null||!ElementalAscension.Apply(pupil,def))return false;
   if(celestial)MakeEvolutionReady(pupil);
   return true;
  }
  static void MakeEvolutionReady(Pawn pupil){
   // Use the existing saved milestones. Evolution remains the recipient's choice.
   foreach(var h in pupil.health.hediffSet.hediffs){
    switch(h.def.defName){
     case "EH_FlameHeart":var flame=h.TryGetComp<HediffComp_FlameMastery>();if(flame==null)continue;flame.MakeEvolutionReady();break;
     case "EH_LightningHeart":var lightning=h.TryGetComp<HediffComp_ThunderstormMastery>();if(lightning==null)continue;lightning.MakeEvolutionReady();break;
     case "EH_WaterHeart":var water=h.TryGetComp<HediffComp_GlacialMastery>();if(water==null)continue;water.kills=Math.Max(water.kills,150);break;
     case "EH_WindHeart":var wind=h.TryGetComp<HediffComp_CycloneMastery>();if(wind==null)continue;wind.kills=Math.Max(wind.kills,150);break;
     case "EH_EarthHeart":var earth=h.TryGetComp<HediffComp_AdamantMastery>();if(earth==null)continue;earth.kills=Math.Max(earth.kills,150);break;
     default:continue;
    }
    h.ageTicks=Math.Max(h.ageTicks,600000);pupil.health.Notify_HediffChanged(h);return;
   }
  }
  public static Command_Target Command(Pawn giver){
   if(icon==null)icon=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Bestow");
   var c=new Command_Target{defaultLabel=CelestialAscendant.Active(giver)?"Bestow (Random)":"Bestow "+ElementalTeaching.Element(giver),defaultDesc=(CelestialAscendant.Active(giver)?"Give the recipient one randomly chosen NORMAL lineage: Lightning, Flame, Wind, Water or Earth, each with equal chance. Also fulfill its 150 mastery kills and ten Ascendant days immediately: after the ceremony, press the ordinary Evolve button whenever desired. Evolution is optional and readiness survives saving/loading. The result is chosen once for this ceremony and saved through loading. Disciples of any element can receive the gift. Celestial is never granted by Bestow. ":"")+"Share a spark of your sovereign power. Walk to an unascended humanlike pawn, then both stop and face each other for a four-second elemental investiture. Azure Flame grants normal Flame ascension; Thunderstorm grants normal Lightning ascension; Glacial grants Water, Cyclone grants Wind and Adamant grants Earth. The recipient skips discipleship and gains the full normal Ascendant condition. Same-element disciples can also receive the gift. One lineage per pawn; no resource cost. Interrupted ceremonies grant nothing.",icon=icon,groupable=false,targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>Valid(giver,t.Thing as Pawn)&&giver.CanReach(t.Thing,PathEndMode.Touch,Danger.Some)},action=t=>giver.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Giving),t.Pawn),JobTag.Misc)};
   if(!ElementalTeaching.Ready(giver)||ElementalTeachingSession.Busy(giver)||ElementalAnimationBridge.Controlled(giver)||HediffComp_ElementalTransform.Transforming(giver)||AzureEvolution.Evolving(giver))c.Disable("Finish the current action before bestowing ascension.");return c;
  }
 }
 public sealed class JobDriver_ElementalBestow:JobDriver {
  int startedAt=-1,lastCue=-1;bool committed;string grantedElement;
  public string GrantedElement{get{return grantedElement;}}
  Pawn Pupil{get{return job.targetA.Pawn;}}
  public int ElapsedAt(int tick){return startedAt>=0?Math.Max(0,tick-startedAt):Math.Max(0,BestowRules.Duration-ticksLeftThisToil);}
  public bool Committed{get{return committed;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return pawn.Reserve(job.targetA,job,1,-1,null,errorOnFailed);}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref startedAt,"EH_bestowStarted",-1);Scribe_Values.Look(ref lastCue,"EH_bestowCue",-1);Scribe_Values.Look(ref committed,"EH_bestowCommitted");Scribe_Values.Look(ref grantedElement,"EH_bestowGrantedElement");}
  void Hold(){if(grantedElement==null)grantedElement=CelestialAscendant.Active(pawn)?NpcDisciples.Elements[Rand.Range(0,NpcDisciples.Elements.Length)]:ElementalTeaching.Element(pawn);ElementalTeachingSession.FaceAndStop(pawn,Pupil);int now=Find.TickManager.TicksGame;if(startedAt<0)startedAt=now-Math.Max(0,BestowRules.Duration-ticksLeftThisToil);pawn.Map.GetComponent<MapComponent_BestowVfx>().Track(pawn);int cue=BestowRules.Cue(ElapsedAt(now));if(cue>lastCue){lastCue=cue;ElementalAudio.Play(pawn,cue==0?"EH_TransformStart":cue==1?"EH_TransformCharge":ElementalAudio.SoundName(pawn,ElementalSound.Impact));}}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>ElementalBestow.Release(pawn,Pupil));this.FailOn(()=>!ElementalBestow.Valid(pawn,Pupil));yield return Toils_Goto.GotoThing(TargetIndex.A,PathEndMode.Touch);yield return Toils_General.Do(()=>ElementalBestow.Begin(pawn,Pupil));
   var ritual=Toils_General.Wait(BestowRules.Duration,TargetIndex.A);ritual.handlingFacing=true;ritual.initAction=Hold;ritual.tickAction=Hold;ritual.FailOn(()=>!ElementalBestow.Paired(pawn,Pupil));ritual.WithProgressBarToilDelay(TargetIndex.A);yield return ritual;
   yield return Toils_General.Do(()=>{if(committed)return;if(ElementalBestow.Grant(pawn,Pupil,grantedElement)){committed=true;if(CelestialAscendant.Active(pawn)){CelestialVfx.Emit(Pupil,CelestialEffect.ArrivalRing,Pupil.DrawPos,Pupil.DrawPos,36,3,3);CelestialVfx.Emit(Pupil,CelestialEffect.RainbowGlint,Pupil.DrawPos,Pupil.DrawPos,30,2,2);}else pawn.Map.GetComponent<MapComponent_BestowVfx>().Complete(Pupil.Position.ToVector3Shifted(),ThunderstormEvolution.Active(pawn),CycloneEvolution.Active(pawn),AdamantEvolution.Active(pawn));Messages.Message(Pupil.LabelShort+" received "+grantedElement.ToLower()+" ascension.",Pupil,MessageTypeDefOf.PositiveEvent,false);}});
  }
 }
 public sealed class JobDriver_ReceiveElementalBestow:JobDriver {
  Pawn Giver{get{return job.targetA.Pawn;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  protected override IEnumerable<Toil> MakeNewToils(){this.FailOn(()=>!ElementalBestow.Paired(Giver,pawn));var wait=new Toil();wait.defaultCompleteMode=ToilCompleteMode.Never;wait.handlingFacing=true;wait.initAction=()=>ElementalTeachingSession.FaceAndStop(Giver,pawn);wait.tickAction=()=>ElementalTeachingSession.FaceAndStop(Giver,pawn);yield return wait;}
 }
}
