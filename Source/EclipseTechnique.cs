using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public sealed class JobDriver_EclipseTechnique:JobDriver {
  bool committed,moving,visualsRestored;
  IntVec3 moveFrom=IntVec3.Invalid,moveTo=IntVec3.Invalid,attackOrigin=IntVec3.Invalid;
  int moveElapsed,moveDuration,movementKind,attackStart;
  EclipsePower Power{get{return (EclipsePower)job.count;}}
  Pawn Target{get{return job.targetA.Pawn;}}
  HediffComp_EclipseAbilities Comp{get{return HediffComp_EclipseAbilities.On(pawn);}}
  public bool Moving{get{return moving;}}
  public Vector3 VisualPosition{get{float p=LunarRules.Progress(moveElapsed,moveDuration);var at=Vector3.Lerp(moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),p);if(movementKind==2)at.z+=(float)Math.Sin(p*Math.PI)*.9f;return at;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref committed,"EH_eclipseImpactCommitted");Scribe_Values.Look(ref moving,"EH_eclipseMoving");Scribe_Values.Look(ref moveFrom,"EH_eclipseMoveFrom",IntVec3.Invalid);Scribe_Values.Look(ref moveTo,"EH_eclipseMoveTo",IntVec3.Invalid);Scribe_Values.Look(ref moveElapsed,"EH_eclipseMoveElapsed");Scribe_Values.Look(ref moveDuration,"EH_eclipseMoveDuration");Scribe_Values.Look(ref movementKind,"EH_eclipseMovementKind");Scribe_Values.Look(ref attackStart,"EH_eclipseAttackStart");Scribe_Values.Look(ref attackOrigin,"EH_eclipseAttackOrigin",IntVec3.Invalid);}
  Toil Owned(Action action){var toil=Toils_General.Do(action);toil.atomicWithPrevious=true;return toil;}
  void Impact(Action action){if(committed)return;committed=true;action();}
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFailCondition(()=>Comp==null||!Comp.CanAct||job.count<0||job.count>=6);
   AddFinishAction(condition=>{moving=false;if(Comp!=null)Comp.Released();});
   if(Power==EclipsePower.UmbralPursuit){yield return Movement(0);yield return Movement(1);yield return Owned(()=>Impact(Backstab));}
   else if(Power==EclipsePower.NightfallReversal){yield return Movement(2);yield return Movement(3);yield return Owned(()=>Impact(Rush));}
   else{yield return Owned(PrepareAttack);yield return Animation();}
   var recovery=Toils_General.Wait(3);recovery.atomicWithPrevious=true;yield return recovery;
  }
  void PrepareAttack(){
   pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();attackOrigin=pawn.Position;attackStart=Find.TickManager.TicksGame;visualsRestored=false;
   if(Power==EclipsePower.DuskCrescent||Power==EclipsePower.SovereignEclipse){var direction=(job.targetA.Cell-pawn.Position).ToVector3();if(direction.sqrMagnitude<.01f){EndJobWith(JobCondition.Incompletable);return;}direction.Normalize();job.targetB=(pawn.Position.ToVector3Shifted()+direction*EclipseRules.Ranges[(int)Power]).ToIntVec3();}
   else job.targetB=Power==EclipsePower.CrimsonWaltz?pawn.Position:job.targetA.Cell;
   if(Power!=EclipsePower.CrimsonWaltz)pawn.rotationTracker.FaceCell(job.targetB.Cell);
   if(Power==EclipsePower.RivenHorizon||Power==EclipsePower.SovereignEclipse)ElementalAudio.Ability(pawn,ElementalSound.Charge);
  }
  Toil Animation(){
   int duration=Power==EclipsePower.CrimsonWaltz?16:Power==EclipsePower.RivenHorizon?22:Power==EclipsePower.SovereignEclipse?29:8;
   int impactTick=Power==EclipsePower.CrimsonWaltz?6:Power==EclipsePower.RivenHorizon?21:Power==EclipsePower.SovereignEclipse?28:3;
   var toil=Toils_General.Wait(duration);toil.atomicWithPrevious=true;toil.handlingFacing=true;
   toil.tickAction=()=>{
    if(pawn.Position!=attackOrigin){EndJobWith(JobCondition.Incompletable);return;}pawn.pather.StopDead();
    int elapsed=Find.TickManager.TicksGame-attackStart;
    if(!visualsRestored){visualsRestored=true;var at=job.targetB.Cell.ToVector3Shifted();if(Power==EclipsePower.CrimsonWaltz)EclipseVfx.Emit(pawn,4,pawn.DrawPos,pawn.DrawPos,duration,8.6f,elapsed);else if(Power==EclipsePower.RivenHorizon)EclipseVfx.Emit(pawn,9,at,at,duration,7.6f,elapsed);else if(Power==EclipsePower.SovereignEclipse)EclipseVfx.Emit(pawn,9,pawn.DrawPos,pawn.DrawPos,duration,4.2f,elapsed);}
    if(Power==EclipsePower.CrimsonWaltz)pawn.Rotation=Rot4.FromAngleFlat(elapsed*360f/16);
    if(elapsed>=impactTick){if(Power==EclipsePower.DuskCrescent)Impact(()=>Wave(false));else if(Power==EclipsePower.SovereignEclipse)Impact(()=>Wave(true));else if(Power==EclipsePower.CrimsonWaltz)Impact(Spin);else Impact(Horizon);}
   };return toil;
  }
  Toil Movement(int kind){
   var toil=new Toil();toil.defaultCompleteMode=ToilCompleteMode.Never;toil.atomicWithPrevious=true;toil.handlingFacing=true;
   toil.initAction=()=>{
    movementKind=kind;moveFrom=pawn.Position;moveElapsed=0;visualsRestored=false;
    if(kind<2){moveTo=kind==0?job.targetB.Cell:job.targetC.Cell;moveDuration=EclipseMovement.TravelTicks(moveFrom,moveTo);}
    else if(kind==2){moveTo=job.targetB.Cell;moveDuration=12;}
    else{if(!CelestialCombat.Hostile(pawn,Target)||!EclipseMovement.Landing(pawn,pawn.Position,Target,12,9,out moveTo)){EndJobWith(JobCondition.Incompletable);return;}job.targetC=moveTo;moveDuration=9;}
    if(!CelestialCombat.Free(pawn,moveTo)||!EclipseMovement.MotionClear(pawn,moveFrom,moveTo,moveDuration)){EndJobWith(JobCondition.Incompletable);return;}
    pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();moving=true;
    if(moveTo!=moveFrom)pawn.rotationTracker.FaceCell(moveTo);
    EclipseVfx.Emit(pawn,1,pawn.DrawPos,pawn.DrawPos,10,2.4f);ElementalAudio.Ability(pawn,ElementalSound.Dash);
   };
   toil.tickAction=()=>{
    pawn.pather.StopDead();if(!CelestialCombat.Free(pawn,moveTo)){moving=false;EndJobWith(JobCondition.Incompletable);return;}
    if(!visualsRestored){visualsRestored=true;EclipseVfx.Emit(pawn,kind==2?6:kind==3?7:2,moveFrom.ToVector3Shifted(),kind==2?moveFrom.ToVector3Shifted():moveTo.ToVector3Shifted(),moveDuration,kind==2?3.2f:4.2f,moveElapsed);}
    int next=Math.Min(moveDuration,moveElapsed+1);var at=Vector3.Lerp(moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),LunarRules.Progress(next,moveDuration));var cell=at.ToIntVec3();
    if(!CelestialCombat.Path(pawn,pawn.Position,cell)){moving=false;EndJobWith(JobCondition.Incompletable);return;}
    if(moveElapsed%2==0){EclipseVfx.Ghost(pawn,VisualPosition);EclipseVfx.Emit(pawn,kind==2?6:kind==3?7:2,VisualPosition,at,6,2.2f);}
    ThunderstormBlitz.Move(pawn,cell);moveElapsed=next;
    if(moveElapsed>=moveDuration){moving=false;ThunderstormBlitz.Move(pawn,moveTo);if(kind==1||kind==2)EclipseVfx.Emit(pawn,1,pawn.DrawPos,pawn.DrawPos,10,2.5f);ReadyForNextToil();}
   };return toil;
  }
  void Wave(bool sovereign){
   IntVec3 origin=pawn.Position,last=origin;foreach(var cell in LightningGeometry.Trace(origin,job.targetB.Cell)){if(!cell.InBounds(pawn.Map)||cell.Fogged(pawn.Map)||!LightningCombat.Open(cell,pawn.Map)||!LightningCombat.ClearLine(last,cell,pawn.Map))break;last=cell;}
   float width=sovereign?2f:.75f;var center=((origin.ToVector3Shifted()+last.ToVector3Shifted())*.5f).ToIntVec3();float radius=(last-origin).LengthHorizontal*.5f+width;
   Pawn first=null;int count=0;
   foreach(var enemy in CelestialCombat.Targets(pawn,center,radius,48)){
    if(!CelestialCombat.Hostile(pawn,enemy)||!LightningCombat.ClearLine(origin,enemy.Position,pawn.Map))continue;
    var at=enemy.Position.ToVector3Shifted();if(AzureReversalRules.LaneDistanceSquared(at.x,at.z,origin.x+.5f,origin.z+.5f,last.x+.5f,last.z+.5f)>width*width)continue;
    if(first==null)first=enemy;float multiplier=sovereign?4.6f*(1+.15f*Comp.Consume(enemy)):2.4f;
    if(EclipseCombat.Strike(pawn,enemy,multiplier)&&!sovereign)Comp.Mark(enemy);if(++count>=EclipseRules.VictimLimit)break;
   }
   Comp.AdvanceCombo(Target??first,sovereign?0:1);EclipseVfx.Emit(pawn,sovereign?8:0,pawn.DrawPos,last.ToVector3Shifted(),sovereign?22:14,sovereign?5.6f:3.7f);ElementalAudio.Ability(pawn,sovereign?ElementalSound.Impact:ElementalSound.Slash);
  }
  void Backstab(){
   if(!CelestialCombat.Hostile(pawn,Target)||pawn.Position.DistanceToSquared(Target.Position)>2||!LightningCombat.ClearLine(pawn.Position,Target.Position,pawn.Map))return;
   pawn.rotationTracker.FaceCell(Target.Position);if(EclipseCombat.Strike(pawn,Target,2.8f))Comp.Mark(Target);Comp.AdvanceCombo(Target,2);
   EclipseVfx.Emit(pawn,3,pawn.DrawPos,Target.DrawPos,12,3f);ElementalAudio.Ability(pawn,ElementalSound.Slash);
  }
  void Spin(){Pawn first=null;foreach(var enemy in CelestialCombat.Targets(pawn,pawn.Position,4.5f,EclipseRules.VictimLimit)){if(!CelestialCombat.Hostile(pawn,enemy))continue;if(first==null)first=enemy;if(EclipseCombat.Strike(pawn,enemy,2.5f))Comp.Mark(enemy);}Comp.AdvanceCombo(first,3);ElementalAudio.Ability(pawn,ElementalSound.Spin);}
  void Horizon(){
   var center=job.targetB.Cell;if(!center.InBounds(pawn.Map)||center.Fogged(pawn.Map)||pawn.Position.DistanceToSquared(center)>144||!LightningCombat.Open(center,pawn.Map)||!LightningCombat.ClearLine(pawn.Position,center,pawn.Map))return;
   foreach(var enemy in CelestialCombat.Targets(pawn,center,3.8f,EclipseRules.VictimLimit))if(CelestialCombat.Hostile(pawn,enemy))EclipseCombat.Strike(pawn,enemy,3.8f*(1+.15f*Comp.Consume(enemy)));
   EclipseVfx.Emit(pawn,5,center.ToVector3Shifted(),center.ToVector3Shifted(),22,8.4f);ElementalAudio.Ability(pawn,ElementalSound.Impact);
  }
  void Rush(){
   var center=((moveFrom.ToVector3Shifted()+moveTo.ToVector3Shifted())*.5f).ToIntVec3();float radius=(moveTo-moveFrom).LengthHorizontal*.5f+1.5f;int count=0;
   foreach(var enemy in CelestialCombat.Targets(pawn,center,radius,48)){
    if(!CelestialCombat.Hostile(pawn,enemy)||!LightningCombat.ClearLine(pawn.Position,enemy.Position,pawn.Map))continue;var at=enemy.Position.ToVector3Shifted();
    if(AzureReversalRules.LaneDistanceSquared(at.x,at.z,moveFrom.x+.5f,moveFrom.z+.5f,moveTo.x+.5f,moveTo.z+.5f)>2.25f)continue;
    if(EclipseCombat.Strike(pawn,enemy,3.8f))Comp.Mark(enemy);if(++count>=EclipseRules.VictimLimit)break;
   }
   Comp.AdvanceCombo(Target,2);EclipseVfx.Emit(pawn,7,moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),14,4.8f);EclipseVfx.Emit(pawn,3,pawn.DrawPos,pawn.DrawPos,12,3.2f);ElementalAudio.Ability(pawn,ElementalSound.Impact);
  }
 }
}
