using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public sealed class JobDriver_CelestialTechnique:JobDriver {
  public int elapsed;int duration,impactAt,finishAt;bool initialized,committed,completed,snapshotReady;IntVec3 origin=IntVec3.Invalid,start=IntVec3.Invalid,goal=IntVec3.Invalid,impactCell=IntVec3.Invalid;float damage,penetration;ThingDef weapon;HashSet<int> struck=new HashSet<int>();int effect;
  public CelestialPower Power{get{return (CelestialPower)Math.Max(0,Math.Min(5,job.count));}}
  Pawn Target{get{return job.targetA.Pawn;}}
  int DashStart{get{return Power==CelestialPower.FakeRainbow?6:0;}}
  public bool Committed{get{return committed;}}
  public bool Moving{get{return initialized&&(Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow)&&elapsed>=DashStart&&elapsed<DashStart+duration;}}
  public Vector3 VisualAt(int age){return Vector3.Lerp(start.ToVector3Shifted(),goal.ToVector3Shifted(),CelestialRules.Progress(age-DashStart,duration));}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref elapsed,"EH_celestialElapsed");Scribe_Values.Look(ref duration,"EH_celestialDuration");Scribe_Values.Look(ref impactAt,"EH_celestialImpactAt");Scribe_Values.Look(ref finishAt,"EH_celestialFinishAt");Scribe_Values.Look(ref initialized,"EH_celestialInitialized");Scribe_Values.Look(ref committed,"EH_celestialCommitted");Scribe_Values.Look(ref completed,"EH_celestialCompleted");Scribe_Values.Look(ref snapshotReady,"EH_celestialSnapshotReady");Scribe_Values.Look(ref origin,"EH_celestialOrigin",IntVec3.Invalid);Scribe_Values.Look(ref start,"EH_celestialStart",IntVec3.Invalid);Scribe_Values.Look(ref goal,"EH_celestialGoal",IntVec3.Invalid);Scribe_Values.Look(ref impactCell,"EH_celestialImpactCell",IntVec3.Invalid);Scribe_Values.Look(ref damage,"EH_celestialDamage");Scribe_Values.Look(ref penetration,"EH_celestialPenetration");Scribe_Defs.Look(ref weapon,"EH_celestialWeapon");Scribe_Collections.Look(ref struck,"EH_celestialVictims",LookMode.Value);if(struck==null)struck=new HashSet<int>();}
  bool Valid(){var c=HediffComp_CelestialAbilities.On(pawn);return pawn.CurJob==job&&job.count>=0&&job.count<=5&&c!=null&&c.Able&&!ElementalAnimationBridge.ExternallyControlled(pawn)&&(!initialized||Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow||pawn.Position==origin);}
  bool TargetValid(float range){return CelestialCombat.Eligible(pawn,Target)&&pawn.Position.DistanceToSquared(Target.Position)<=range*range&&LightningCombat.ClearLine(pawn.Position,Target.Position,pawn.Map);}
  void Abort(){if(pawn.CurJob==job)EndJobWith(JobCondition.Incompletable);}
  void Snapshot(){if(snapshotReady)return;var profile=LightningCombat.Profile(pawn,12);damage=profile.damage;penetration=profile.penetration;weapon=profile.weapon;snapshotReady=true;}
  bool Hit(Pawn target,float multiplier,bool mark){if(!CelestialCombat.Eligible(pawn,target)||struck.Count>=CelestialRules.MaxVictims||!struck.Add(target.thingIDNumber))return false;Snapshot();bool hit=CelestialCombat.Strike(pawn,target,damage,penetration,weapon,multiplier);if(hit&&mark)CelestialRadiance.Apply(pawn,target);return hit;}
  void Complete(){if(completed)return;completed=true;var c=HediffComp_CelestialAbilities.On(pawn);if(c!=null)c.Completed(Power);}
  void StopVisual(){if(effect!=0&&pawn.Spawned)pawn.Map.GetComponent<MapComponent_CelestialVfx>().Stop(effect);effect=0;}
  void Visual(){
   StopVisual();if(!pawn.Spawned)return;
   if(Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow){int age=elapsed-DashStart;if(age>=0&&age<duration)effect=CelestialVfx.Emit(pawn,Power==CelestialPower.FakeRainbow?CelestialEffect.CounterDash:CelestialEffect.SunstrideTrail,start.ToVector3Shifted(),goal.ToVector3Shifted(),duration,1,1,null,age);}
   else if(Power==CelestialPower.Dawncleaver&&elapsed>=6&&elapsed<26)effect=CelestialVfx.Emit(pawn,CelestialEffect.DawnCrescent,start.ToVector3Shifted(),goal.ToVector3Shifted(),20,2.5f,2.5f,null,elapsed-6);
   else if(Power==CelestialPower.SolarJudgment){if(!committed&&Target!=null&&Target.Spawned)effect=CelestialVfx.Emit(pawn,CelestialEffect.JudgmentTelegraph,Target.DrawPos,Target.DrawPos,impactAt,3.5f,3.5f,Target,elapsed);else if(impactCell.IsValid)effect=CelestialVfx.Emit(pawn,CelestialEffect.JudgmentColumn,impactCell.ToVector3Shifted(),impactCell.ToVector3Shifted(),32,3.5f,6,null,Math.Max(0,elapsed-impactAt));}
   else if(Power==CelestialPower.HeavenlyZenith)effect=CelestialVfx.Emit(pawn,committed?CelestialEffect.ZenithBurst:CelestialEffect.ZenithFormation,origin.ToVector3Shifted()+new Vector3(0,0,committed?0:1),origin.ToVector3Shifted(),committed?24:impactAt,committed?9:2.8f,committed?9:2.8f,null,committed?Math.Max(0,elapsed-impactAt):elapsed);
   else if(Power==CelestialPower.CoronaDance&&committed)effect=CelestialVfx.Emit(pawn,CelestialEffect.CoronaSpin,origin.ToVector3Shifted(),origin.ToVector3Shifted(),24,9,9,null,Math.Max(0,elapsed-impactAt));
  }
  void Init(){
   pawn.pather.StopDead();pawn.stances.CancelBusyStanceHard();
   if(!initialized){
    origin=start=pawn.Position;Snapshot();
    if(Power==CelestialPower.FakeRainbow){if(!CelestialCombat.Hostile(pawn,Target)||!CelestialCombat.Free(pawn,job.targetB.Cell)||!CelestialCombat.Path(pawn,pawn.Position,job.targetB.Cell)){Abort();return;}start=job.targetB.Cell;goal=job.targetC.Cell;ThunderstormBlitz.Move(pawn,start);CelestialVfx.Emit(pawn,CelestialEffect.RainbowGlint,start.ToVector3Shifted(),start.ToVector3Shifted(),18,1.8f,1.8f);}
    else if(Power==CelestialPower.Sunstride)goal=job.targetB.Cell;
    else if(Power==CelestialPower.Dawncleaver){if(Target==null||!Target.Spawned){Abort();return;}var vector=(Target.Position-origin).ToVector3().normalized*12;var end=(origin.ToVector3Shifted()+vector).ToIntVec3();goal=origin;foreach(var cell in LightningGeometry.Trace(origin,end)){if(!cell.InBounds(pawn.Map)||!LightningCombat.Open(cell,pawn.Map))break;goal=cell;}}
    if(Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow){duration=CelestialRules.DashDuration((goal-start).LengthHorizontal);impactAt=DashStart+duration+2;finishAt=impactAt+10;}
    else {impactAt=Power==CelestialPower.SolarJudgment?24:Power==CelestialPower.HeavenlyZenith?28:8;finishAt=Power==CelestialPower.Dawncleaver?32:impactAt+(Power==CelestialPower.SolarJudgment?24:24);}
    initialized=true;ElementalAudio.Ability(pawn,Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow?ElementalSound.Dash:Power==CelestialPower.SolarJudgment||Power==CelestialPower.HeavenlyZenith?ElementalSound.Charge:ElementalSound.Slash);
   }
   Visual();
  }
  void Sweep(){int age=elapsed-6;if(age==0)Visual();var from=Vector3.Lerp(start.ToVector3Shifted(),goal.ToVector3Shifted(),Mathf.Clamp01((age-1)/20f));var to=Vector3.Lerp(start.ToVector3Shifted(),goal.ToVector3Shifted(),Mathf.Clamp01(age/20f));if(!LightningCombat.ClearLine(start,to.ToIntVec3(),pawn.Map)){Abort();return;}foreach(var target in CelestialCombat.Targets(pawn,to.ToIntVec3(),2,CelestialRules.MaxVictims)){var at=target.Position.ToVector3Shifted();if(StormBlitzRules.LaneDistanceSquared(at.x,at.z,from.x,from.z,to.x,to.z)>1.25f*1.25f||!LightningCombat.ClearLine(start,target.Position,pawn.Map))continue;Hit(target,2.8f,true);if(!Valid())return;}if(age>=20){committed=true;Complete();}}
  void Impact(){
   if(committed)return;committed=true;
   if(Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow){if(TargetValid(1.5f)&&pawn.Position.DistanceToSquared(Target.Position)<=2){if(Power==CelestialPower.FakeRainbow){CelestialCombat.counterDepth++;WaterDefense.counterDepth++;try{Hit(Target,4.2f,true);}finally{CelestialCombat.counterDepth--;WaterDefense.counterDepth--;}}else Hit(Target,3.6f,true);pawn.Drawer.Notify_MeleeAttackOn(Target);CelestialVfx.Emit(pawn,Power==CelestialPower.FakeRainbow?CelestialEffect.RainbowSlash:CelestialEffect.DawnCrescent,pawn.DrawPos,Target.DrawPos,18,2.5f,2.5f);}}
   else if(Power==CelestialPower.CoronaDance){foreach(var target in CelestialCombat.Targets(pawn,pawn.Position,4.5f,CelestialRules.MaxVictims)){Hit(target,2.8f,true);if(!Valid())break;}Visual();}
   else if(Power==CelestialPower.SolarJudgment){if(!TargetValid(14))return;impactCell=Target.Position;int marks=CelestialRadiance.Stacks(pawn,Target);bool hit=Hit(Target,5f+.65f*marks,false);if(hit)CelestialRadiance.Consume(pawn,Target);int secondary=0;foreach(var target in CelestialCombat.Targets(pawn,impactCell,2.5f,9)){if(target==Target||secondary>=8)continue;Hit(target,2.4f,false);secondary++;if(!Valid())break;}Visual();}
   else if(Power==CelestialPower.HeavenlyZenith){var c=HediffComp_CelestialAbilities.On(pawn);if(c!=null)c.coronaUntil=Find.TickManager.TicksGame+180;foreach(var target in CelestialCombat.Targets(pawn,pawn.Position,4.5f,12)){Hit(target,4.6f,true);if(target!=null&&!target.Dead&&!target.Downed)CelestialCombat.Push(pawn,target,3);if(!Valid())break;}Visual();}
   Complete();ElementalAudio.Ability(pawn,Power==CelestialPower.CoronaDance?ElementalSound.Spin:Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow?ElementalSound.Slash:ElementalSound.Impact);
  }
  void Cleanup(){StopVisual();var c=HediffComp_CelestialAbilities.On(pawn);if(c!=null)c.nextAutoCheck=Find.TickManager.TicksGame+8;if(!initialized||!pawn.Spawned||pawn.Dead||pawn.Downed||ElementalAnimationBridge.ExternallyControlled(pawn)||CelestialCombat.Free(pawn,pawn.Position))return;foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,1.5f,false))if(CelestialCombat.Free(pawn,cell)&&LightningCombat.ClearLine(pawn.Position,cell,pawn.Map)){ThunderstormBlitz.Move(pawn,cell);return;}}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>Cleanup());this.FailOn(()=>!Valid());var cast=new Toil();cast.defaultCompleteMode=ToilCompleteMode.Never;cast.handlingFacing=true;cast.initAction=Init;
   cast.tickAction=()=>{if(!initialized)return;pawn.pather.StopDead();using(new ElementalManualTarget(pawn,job.playerForced?Target:null)){
    elapsed++;
    if(Power==CelestialPower.Sunstride||Power==CelestialPower.FakeRainbow){
     if(!committed&&!TargetValid(18)){Abort();return;}
     if(elapsed>=DashStart&&elapsed<=DashStart+duration){if(!CelestialCombat.Free(pawn,goal)||!CelestialCombat.Path(pawn,pawn.Position,goal)){Abort();return;}if(elapsed==DashStart)Visual();var before=VisualAt(elapsed-1);var next=VisualAt(elapsed);if(!CelestialCombat.Path(pawn,pawn.Position,next.ToIntVec3())){Abort();return;}pawn.Rotation=Rot4.FromAngleFlat((goal-start).ToVector3().AngleFlat());if(elapsed%2==0)CelestialVfx.Ghost(pawn,before);ThunderstormBlitz.Move(pawn,next.ToIntVec3());}
     if(elapsed>=impactAt&&!committed)Impact();
    }else if(Power==CelestialPower.Dawncleaver){pawn.Rotation=Rot4.FromAngleFlat((goal-start).ToVector3().AngleFlat());if(elapsed>=6&&elapsed<=26&&!committed)Sweep();}
    else {if(Power==CelestialPower.SolarJudgment&&!committed){if(!TargetValid(14)){Abort();return;}pawn.rotationTracker.FaceTarget(Target);}if(elapsed>=impactAt&&!committed)Impact();if(Power==CelestialPower.CoronaDance&&committed)pawn.Rotation=new Rot4((elapsed/4)%4);}
    if(elapsed>=finishAt)ReadyForNextToil();
   }};yield return cast;
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class CelestialDashDrawPosition {public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(___pawn==null||___pawn.jobs==null)return;var driver=___pawn.jobs.curDriver as JobDriver_CelestialTechnique;if(driver==null||!driver.Moving)return;var at=driver.VisualAt(driver.elapsed);at.y=__result.y;__result=at;}}
}
