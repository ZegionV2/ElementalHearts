using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class WaterTechnique {
  public const string JobName="EH_WaterTechnique";
  public static bool Busy(Pawn p){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==JobName;}
 }
 public sealed class JobDriver_WaterTechnique:JobDriver {
  int elapsed,duration;bool initialized,committed,completed,snapshotReady;IntVec3 start;IntVec3 impactCell=IntVec3.Invalid;float damage,penetration;ThingDef weapon;HashSet<int> struck=new HashSet<int>();
  public WaterPower Power{get{return (WaterPower)Math.Max(0,Math.Min(3,job.count));}}
  public bool Committed{get{return committed;}}
  public bool Moving{get{return initialized&&Power==WaterPower.Step&&elapsed<duration;}}
  public int Elapsed{get{return elapsed;}}
  public Vector3 VisualAt(int at){return Vector3.Lerp(start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),WaterAbilityRules.StepProgress(at,duration));}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref elapsed,"EH_waterTechniqueElapsed");Scribe_Values.Look(ref duration,"EH_waterStepDuration");Scribe_Values.Look(ref start,"EH_waterTechniqueStart");Scribe_Values.Look(ref impactCell,"EH_waterImpactCell",IntVec3.Invalid);Scribe_Values.Look(ref initialized,"EH_waterTechniqueInitialized");Scribe_Values.Look(ref committed,"EH_waterTechniqueCommitted");Scribe_Values.Look(ref completed,"EH_waterTechniqueCompleted");Scribe_Values.Look(ref snapshotReady,"EH_waterSnapshotReady");Scribe_Values.Look(ref damage,"EH_waterDamageSnapshot");Scribe_Values.Look(ref penetration,"EH_waterPenSnapshot");Scribe_Defs.Look(ref weapon,"EH_waterWeaponSnapshot");Scribe_Collections.Look(ref struck,"EH_waterTechniqueVictims",LookMode.Value);if(struck==null)struck=new HashSet<int>();}
  bool Valid(){var c=HediffComp_WaterDefense.On(pawn);return pawn.CurJob==job&&job.count>=0&&job.count<=3&&c!=null&&c.Able&&HediffComp_WaterAbilities.On(pawn)!=null&&!ElementalAnimationBridge.ExternallyControlled(pawn);}
  void Abort(){if(pawn.CurJob==job)EndJobWith(JobCondition.Incompletable);}
  bool TargetValid(float range){var target=job.targetA.Pawn;return WaterCombat.Eligible(pawn,target)&&pawn.Position.DistanceToSquared(target.Position)<=range*range&&LightningCombat.ClearLine(pawn.Position,target.Position,pawn.Map);}
  LightningCombat.AttackProfile Profile(){if(!snapshotReady){var profile=LightningCombat.Profile(pawn,10);damage=profile.damage;penetration=profile.penetration;weapon=profile.weapon;snapshotReady=true;}return new LightningCombat.AttackProfile{damage=damage,penetration=penetration,weapon=weapon};}
  bool Hit(Pawn target,float multiplier,bool mark){if(!WaterCombat.Eligible(pawn,target)||!WaterAbilityRules.Commit(struck,target.thingIDNumber,WaterAbilityRules.MaxVictims))return false;bool hit=WaterCombat.Strike(pawn,target,Profile(),multiplier);if(hit&&mark)WaterPressure.Apply(pawn,target);return hit;}
  void MarkComplete(){if(completed)return;completed=true;var c=HediffComp_WaterAbilities.On(pawn);if(c!=null)c.Completed(Power);}
  void Settle(){if(Power!=WaterPower.Step||!pawn.Spawned||pawn.Dead||pawn.Downed||ElementalAnimationBridge.ExternallyControlled(pawn)||ThunderstormBlitz.Landing(pawn,pawn.Position))return;foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,1.5f,false))if(ThunderstormBlitz.Landing(pawn,cell)&&ThunderstormBlitz.Path(pawn,pawn.Position,cell)){ThunderstormBlitz.Move(pawn,cell);return;}if(initialized&&ThunderstormBlitz.Landing(pawn,start)&&ThunderstormBlitz.Path(pawn,pawn.Position,start))ThunderstormBlitz.Move(pawn,start);}
  void Sweep(){
   int age=elapsed-6;if(age==0){WaterAbilityVfx.Emit(pawn,0,start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),16,2.1f);ElementalAudio.Ability(pawn,ElementalSound.Slash);}float before=Mathf.Clamp01((age-1)/16f),after=Mathf.Clamp01(age/16f);var from=Vector3.Lerp(start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),before);var to=Vector3.Lerp(start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),after);
   if(!LightningCombat.ClearLine(start,to.ToIntVec3(),pawn.Map)){Abort();return;}if(struck.Count<WaterAbilityRules.MaxVictims)foreach(var target in WaterCombat.Targets(pawn,to.ToIntVec3(),2,WaterAbilityRules.MaxVictims)){var at=target.Position.ToVector3Shifted();if(StormBlitzRules.LaneDistanceSquared(at.x,at.z,from.x,from.z,to.x,to.z)>.8f*.8f||!LightningCombat.ClearLine(start,target.Position,pawn.Map))continue;Hit(target,WaterAbilityRules.Damage(Power,0),true);if(!Valid()){Abort();return;}}if(age==16){committed=true;MarkComplete();}
  }
  void Impact(){
   if(committed)return;committed=true;var profile=Profile();
   if(Power==WaterPower.Step){if(TargetValid(1.5f)&&pawn.Position.DistanceToSquared(job.targetA.Pawn.Position)<=2)Hit(job.targetA.Pawn,WaterAbilityRules.Damage(Power,0),true);}
   else if(Power==WaterPower.Whirlpool){WaterAbilityVfx.Emit(pawn,2,pawn.DrawPos,pawn.DrawPos,24,GlacialEvolution.Active(pawn)?10.8f:8.8f);foreach(var target in WaterCombat.Targets(pawn,pawn.Position,GlacialEvolution.Range(pawn,WaterPower.Whirlpool),WaterAbilityRules.MaxVictims)){Hit(target,WaterAbilityRules.Damage(Power,0),true);if(!Valid())break;}}
   else {var primary=job.targetA.Pawn;if(!WaterCombat.Eligible(pawn,primary))return;var center=primary.Position;impactCell=center;int stacks=WaterPressure.Stacks(pawn,primary);WaterAbilityVfx.Emit(pawn,3,center.ToVector3Shifted(),center.ToVector3Shifted(),32,4);bool hit=Hit(primary,WaterAbilityRules.Damage(Power,stacks),false);if(hit)WaterPressure.Consume(pawn,primary);int secondary=0;foreach(var target in WaterCombat.Targets(pawn,center,GlacialEvolution.Active(pawn)?3f:2.5f,WaterAbilityRules.BurstVictims+1)){if(target==primary||secondary>=WaterAbilityRules.BurstVictims)continue;if(!Valid())break;Hit(target,1.5f,false);secondary++;}}
   if(!Valid()){Abort();return;}MarkComplete();ElementalAudio.Ability(pawn,Power==WaterPower.Whirlpool?ElementalSound.Spin:Power==WaterPower.Step?ElementalSound.Slash:ElementalSound.Impact);
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>Settle());this.FailOn(()=>!Valid());var cast=new Toil();cast.defaultCompleteMode=ToilCompleteMode.Never;cast.handlingFacing=true;
   cast.initAction=()=>{pawn.pather.StopDead();pawn.stances.CancelBusyStanceHard();if(!initialized){initialized=true;if(Power==WaterPower.Step)ElementalAudio.Ability(pawn,ElementalSound.Dash);else if(Power==WaterPower.Judgement)ElementalAudio.Ability(pawn,ElementalSound.Charge);start=pawn.Position;duration=Power==WaterPower.Step?WaterAbilityRules.StepDuration((job.targetB.Cell-start).LengthHorizontal):0;}if(Power==WaterPower.Step){pawn.Rotation=Rot4.FromAngleFlat((job.targetB.Cell-start).ToVector3().AngleFlat());WaterAbilityVfx.Emit(pawn,1,start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),28,1.8f,pawn,elapsed);}else if(Power==WaterPower.Judgement&&!committed&&job.targetA.Pawn!=null)WaterAbilityVfx.Emit(pawn,4,job.targetA.Pawn.DrawPos,job.targetA.Pawn.DrawPos,GlacialEvolution.Warmup(pawn,Power),2.6f,job.targetA.Pawn,elapsed);
    else if(Power==WaterPower.Judgement&&committed&&impactCell.IsValid)WaterAbilityVfx.Emit(pawn,3,impactCell.ToVector3Shifted(),impactCell.ToVector3Shifted(),32,4,null,Math.Max(0,elapsed-GlacialEvolution.Warmup(pawn,Power)));
    else if(Power==WaterPower.Torrent&&elapsed>=6&&elapsed<22)WaterAbilityVfx.Emit(pawn,0,start.ToVector3Shifted(),job.targetB.Cell.ToVector3Shifted(),16,2.1f,null,elapsed-6);
    else if(Power==WaterPower.Whirlpool&&committed)WaterAbilityVfx.Emit(pawn,2,start.ToVector3Shifted(),start.ToVector3Shifted(),24,GlacialEvolution.Active(pawn)?10.8f:8.8f,null,Math.Max(0,elapsed-6));};
   cast.tickAction=()=>{
    pawn.pather.StopDead();using(new ElementalManualTarget(pawn,job.playerForced?job.targetA.Pawn:null)){
     if(!committed&&(Power==WaterPower.Step||Power==WaterPower.Judgement)&&!TargetValid(Power==WaterPower.Step?GlacialEvolution.Range(pawn,WaterPower.Step)+2:GlacialEvolution.Range(pawn,WaterPower.Judgement))){Abort();return;}elapsed++;
     if(Power==WaterPower.Step){if(elapsed<=duration){if(!ThunderstormBlitz.Landing(pawn,job.targetB.Cell)){Abort();return;}var from=VisualAt(elapsed-1);var to=VisualAt(elapsed);if(!ThunderstormBlitz.Path(pawn,pawn.Position,to.ToIntVec3())){Abort();return;}if(elapsed%2==0)WaterAbilityVfx.Ghost(pawn,from);ThunderstormBlitz.Move(pawn,to.ToIntVec3());if(elapsed==duration)Impact();}if(elapsed>=duration+6)ReadyForNextToil();}
     else {if(Power==WaterPower.Torrent)pawn.Rotation=Rot4.FromAngleFlat((job.targetB.Cell-pawn.Position).ToVector3().AngleFlat());else if(Power==WaterPower.Judgement&&job.targetA.Pawn!=null&&job.targetA.Pawn.Spawned)pawn.rotationTracker.FaceTarget(job.targetA);if(Power==WaterPower.Torrent){if(elapsed>=6&&elapsed<=22)Sweep();if(elapsed>=28)ReadyForNextToil();}else {if(elapsed>=GlacialEvolution.Warmup(pawn,Power)&&!committed)Impact();if(elapsed>=GlacialEvolution.Warmup(pawn,Power)+WaterAbilityRules.Recovery(Power))ReadyForNextToil();}}
    }
   };yield return cast;
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class WaterStepDrawPosition {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(!WaterTechnique.Busy(___pawn))return;var driver=___pawn.jobs.curDriver as JobDriver_WaterTechnique;if(driver==null||!driver.Moving)return;var pos=driver.VisualAt(driver.Elapsed);pos.y=__result.y;__result=pos;}
 }
}
