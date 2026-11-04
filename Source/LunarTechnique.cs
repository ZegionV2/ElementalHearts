using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public sealed partial class JobDriver_LunarTechnique {
  int movesetVersion=2;
  IntVec3 moveFrom=IntVec3.Invalid,moveTo=IntVec3.Invalid;
  int moveElapsed,moveDuration,movementKind,attackStart;
  bool moving;
  bool phaseVisualShown;
  public bool Moving {get{return movesetVersion==2&&moving;}}
  public Vector3 VisualPosition {
   get {
    float progress=LunarRules.Progress(moveElapsed,moveDuration);
    var at=Vector3.Lerp(moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),progress);
    if(movementKind==1)at.z+=(float)Math.Sin(progress*Math.PI)*.8f;
    return at;
   }
  }
  void ExposeMovesetData(){
   Scribe_Values.Look(ref movesetVersion,"EH_lunarMovesetVersion",0);
   Scribe_Values.Look(ref moveFrom,"EH_lunarMoveFrom",IntVec3.Invalid);
   Scribe_Values.Look(ref moveTo,"EH_lunarMoveTo",IntVec3.Invalid);
   Scribe_Values.Look(ref moveElapsed,"EH_lunarMoveElapsed");
   Scribe_Values.Look(ref moveDuration,"EH_lunarMoveDuration");
   Scribe_Values.Look(ref movementKind,"EH_lunarMovementKind");
   Scribe_Values.Look(ref moving,"EH_lunarMoving");
   Scribe_Values.Look(ref attackStart,"EH_lunarAttackStart");
  }

  protected override IEnumerable<Toil> MakeNewToils(){
   if(movesetVersion==0){foreach(var toil in LegacyToils())yield return toil;yield break;}
   AddFailCondition(()=>Comp==null||!Comp.CanAct||job.count<0||job.count>4);
   AddFinishAction(condition=>{moving=false;if(Comp!=null)Comp.Released();});
   if(Power==LunarPower.Step){
    yield return Movement(0);
    yield return OwnedImpact(()=>ImpactOnce(DashImpact));
   }else if(Power==LunarPower.Rush){
    yield return Movement(1);
    yield return Movement(2);
    yield return OwnedImpact(()=>ImpactOnce(RushImpact));
   }else{
    yield return OwnedImpact(PrepareAttack);
    yield return AttackAnimation();
   }
   var recovery=Toils_General.Wait(3);recovery.atomicWithPrevious=true;yield return recovery;
  }

  Toil OwnedImpact(Action action){
   var toil=Toils_General.Do(action);
   // A native melee cooldown may still be unwinding when Auto Combat takes ownership.
   toil.atomicWithPrevious=true;
   return toil;
  }

  void ImpactOnce(Action impact){
   if(committed)return;
   committed=true;impact();
  }
  void PrepareAttack(){
   pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();attackStart=Find.TickManager.TicksGame;
   if(Power==LunarPower.Crescent){
    Vector3 direction=(job.targetA.Cell-pawn.Position).ToVector3();
    if(direction.sqrMagnitude<.01f){EndJobWith(JobCondition.Incompletable);return;}
    direction.Normalize();job.targetB=(pawn.Position.ToVector3Shifted()+direction*11f).ToIntVec3();
   }else job.targetB=Power==LunarPower.Spin?pawn.Position:job.targetA.Cell;
   if(Power!=LunarPower.Spin)pawn.rotationTracker.FaceCell(job.targetB.Cell);
   if(Power==LunarPower.FallingMoon)ElementalAudio.Play(pawn,"EH_Sound_Water_Charge");
  }

  Toil AttackAnimation(){
   int duration=Power==LunarPower.Spin?16:Power==LunarPower.FallingMoon?24:8;
   int impactTick=Power==LunarPower.Spin?6:Power==LunarPower.FallingMoon?23:3;
   var animation=Toils_General.Wait(duration);animation.handlingFacing=true;
   animation.atomicWithPrevious=true;
   animation.tickAction=()=>{
    int elapsed=Find.TickManager.TicksGame-attackStart;
    if(!phaseVisualShown){
     phaseVisualShown=true;
     var at=job.targetB.Cell.ToVector3Shifted();
     if(Power==LunarPower.Spin)LunarVfx.Emit(pawn,2,pawn.DrawPos,pawn.DrawPos,Math.Max(1,duration-elapsed),7f,elapsed,duration);
     if(Power==LunarPower.FallingMoon)LunarVfx.Emit(pawn,7,at,at,Math.Max(1,duration-elapsed),6.4f,elapsed,duration);
    }
    if(Power==LunarPower.Spin)pawn.Rotation=Rot4.FromAngleFlat(elapsed*360f/16);
    if(elapsed>=impactTick){
     if(Power==LunarPower.Crescent)ImpactOnce(CrescentImpact);
     else if(Power==LunarPower.Spin)ImpactOnce(SpinImpact);
     else ImpactOnce(MoonImpact);
    }
   };
   return animation;
  }

  Toil Movement(int kind){
   var movement=new Toil();movement.defaultCompleteMode=ToilCompleteMode.Never;movement.handlingFacing=true;
   movement.atomicWithPrevious=true;
   movement.initAction=()=>{
    movementKind=kind;moveFrom=pawn.Position;moveElapsed=0;
    if(kind==0){
     if(!CelestialCombat.Hostile(pawn,Target)||!LunarMovement.DashLanding(pawn,Target,out moveTo)){EndJobWith(JobCondition.Incompletable);return;}
     moveDuration=LunarRules.DashTicks((moveTo-moveFrom).LengthHorizontal);
    }else if(kind==1){moveTo=job.targetB.Cell;moveDuration=LunarRules.LeapTicks;}
    else{
     if(!CelestialCombat.Hostile(pawn,Target)||!CelestialCombat.Landing(pawn,pawn.Position,Target,10f,out moveTo)){EndJobWith(JobCondition.Incompletable);return;}
     job.targetC=moveTo;moveDuration=LunarRules.RushTicks;
    }
    if(!CelestialCombat.Free(pawn,moveTo)||!CelestialCombat.Path(pawn,moveFrom,moveTo)){EndJobWith(JobCondition.Incompletable);return;}
    pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();moving=true;
    pawn.Rotation=Rot4.FromAngleFlat((moveTo-moveFrom).ToVector3().AngleFlat());
    LunarVfx.Emit(pawn,kind==1?6:kind==2?4:1,moveFrom.ToVector3Shifted(),kind==1?moveFrom.ToVector3Shifted():moveTo.ToVector3Shifted(),moveDuration,kind==1?2.8f:3.5f);
    ElementalAudio.Play(pawn,"EH_Sound_Wind_Dash");
   };
   movement.tickAction=()=>{
    pawn.pather.StopDead();
    if(!CelestialCombat.Free(pawn,moveTo)){moving=false;EndJobWith(JobCondition.Incompletable);return;}
    int next=Math.Min(moveDuration,moveElapsed+1);
    var at=Vector3.Lerp(moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),LunarRules.Progress(next,moveDuration));
    var cell=at.ToIntVec3();
    if(!CelestialCombat.Path(pawn,pawn.Position,cell)){moving=false;EndJobWith(JobCondition.Incompletable);return;}
    if(moveElapsed%2==0){LunarVfx.Ghost(pawn,VisualPosition);LunarVfx.Emit(pawn,kind==1?6:kind==2?4:1,VisualPosition,at,8,1.8f);}
    ThunderstormBlitz.Move(pawn,cell);moveElapsed=next;
    if(moveElapsed<moveDuration)return;
    moving=false;ThunderstormBlitz.Move(pawn,moveTo);
    if(kind==1)LunarVfx.Emit(pawn,6,pawn.DrawPos,pawn.DrawPos,12,2.8f);
    ReadyForNextToil();
   };
   return movement;
  }

  void CrescentImpact(){
   IntVec3 origin=pawn.Position,last=origin;
   int scanned=0;
   var struck=new HashSet<int>();
   Pawn first=null;
   foreach(var cell in LightningGeometry.Trace(origin,job.targetB.Cell)){
    if(!cell.InBounds(pawn.Map)||cell.Fogged(pawn.Map)||!LightningCombat.ClearLine(last,cell,pawn.Map))break;
    last=cell;
    var things=cell.GetThingList(pawn.Map);
    for(int i=0;i<things.Count&&scanned<512;i++){
     scanned++;var victim=things[i] as Pawn;
     if(!CelestialCombat.Hostile(pawn,victim)||!struck.Add(victim.thingIDNumber))continue;
     if(first==null)first=victim;
     if(LunarCombat.Strike(pawn,victim,2.15f))Comp.Mark(victim);
     if(struck.Count>=12)break;
    }
    if(scanned>=512||struck.Count>=12)break;
   }
   Comp.AdvanceCombo(Target??first,1);
   LunarVfx.Emit(pawn,0,pawn.DrawPos,last.ToVector3Shifted(),16,3.1f);
   ElementalAudio.Play(pawn,"EH_Sound_Wind_Slash");
  }
  void DashImpact(){
   if(!CelestialCombat.Hostile(pawn,Target)||pawn.Position.DistanceToSquared(Target.Position)>2||!LightningCombat.ClearLine(pawn.Position,Target.Position,pawn.Map))return;
   var center=Target.Position;
   pawn.rotationTracker.FaceCell(center);
   if(LunarCombat.Strike(pawn,Target,2.6f))Comp.Mark(Target);
   foreach(var enemy in CelestialCombat.Targets(pawn,center,3f,12))if(enemy!=Target)LunarCombat.Pull(pawn,enemy,center);
   Comp.AdvanceCombo(Target,2);
   LunarVfx.Emit(pawn,0,pawn.DrawPos,center.ToVector3Shifted(),12,2.4f);
   ElementalAudio.Play(pawn,"EH_Sound_Wind_Slash");
  }
  void SpinImpact(){
   Pawn first=null;
   foreach(var enemy in CelestialCombat.Targets(pawn,pawn.Position,3.6f,12)){
    if(!CelestialCombat.Hostile(pawn,enemy))continue;
    if(first==null)first=enemy;
    if(LunarCombat.Strike(pawn,enemy,2.3f))Comp.Mark(enemy);
   }
   Comp.AdvanceCombo(first,3);
   ElementalAudio.Play(pawn,"EH_Sound_Wind_Spin");
  }
  void MoonImpact(){
   var center=job.targetB.Cell;
   if(!center.InBounds(pawn.Map)||center.Fogged(pawn.Map)||pawn.Position.DistanceToSquared(center)>121||
    !LightningCombat.Open(center,pawn.Map)||!LightningCombat.ClearLine(pawn.Position,center,pawn.Map))return;
   foreach(var enemy in CelestialCombat.Targets(pawn,center,3.2f,12)){
    if(!CelestialCombat.Hostile(pawn,enemy))continue;
    LunarCombat.Strike(pawn,enemy,3.4f*(1+.2f*Comp.Consume(enemy)));
   }
   LunarVfx.Emit(pawn,3,center.ToVector3Shifted(),center.ToVector3Shifted(),24,7.3f);
   ElementalAudio.Play(pawn,"EH_Sound_Water_Impact");
  }
  void RushImpact(){
   var center=((moveFrom.ToVector3Shifted()+moveTo.ToVector3Shifted())*.5f).ToIntVec3();
   float radius=(moveTo-moveFrom).LengthHorizontal*.5f+1.5f;
   int struck=0;
   foreach(var enemy in CelestialCombat.Targets(pawn,center,radius,32)){
    if(!CelestialCombat.Hostile(pawn,enemy))continue;
    var at=enemy.Position.ToVector3Shifted();
    if(AzureReversalRules.LaneDistanceSquared(at.x,at.z,moveFrom.x+.5f,moveFrom.z+.5f,moveTo.x+.5f,moveTo.z+.5f)>1.5f*1.5f)continue;
    if(!LightningCombat.ClearLine(pawn.Position,enemy.Position,pawn.Map))continue;
    if(LunarCombat.Strike(pawn,enemy,3.4f))Comp.Mark(enemy);
    if(++struck>=12)break;
   }
   Comp.AdvanceCombo(Target,2);
   LunarVfx.Emit(pawn,4,moveFrom.ToVector3Shifted(),moveTo.ToVector3Shifted(),16,4.2f);
   LunarVfx.Emit(pawn,2,pawn.DrawPos,pawn.DrawPos,12,3.2f);
   ElementalAudio.Play(pawn,"EH_Sound_Water_Impact");
  }
 }
}
