using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public static class LunarCombat {
  public const int VictimLimit=12;
  internal const string WardDef="EH_LunarMoonveil";

  internal static bool Landing(Pawn source,Pawn target,out IntVec3 landing){
   if(CelestialCombat.Landing(source,source.Position,target,7f,out landing))return true;
   if(source.Position.DistanceToSquared(target.Position)>2)return false;
   landing=source.Position;
   return true;
  }

  internal static bool Strike(Pawn source,Pawn target,float multiplier){
   if(!CelestialCombat.Hostile(source,target)||WaterDefense.TryParry(source,target))return false;
   var attack=LightningCombat.Profile(source,10);
   float damage=attack.damage*multiplier;
   if(ElementalNonLethal.TryApply(source,target,damage))return CelestialCombat.Hostile(source,target);
   var result=target.TakeDamage(new DamageInfo(DamageDefOf.Cut,damage,attack.penetration,-1f,source,null,attack.weapon));
   return result!=null&&result.totalDamageDealt>0;
  }

  internal static void Pull(Pawn source,Pawn target,IntVec3 center){
   if(!CelestialCombat.Hostile(source,target)||ElementalAnimationBridge.Controlled(target)||LunarAscendant.Busy(target))return;
   var start=target.Position;
   var cell=start+new IntVec3(Math.Sign(center.x-start.x),0,Math.Sign(center.z-start.z));
   if(cell!=start&&cell!=center&&CelestialCombat.Path(target,start,cell))ThunderstormBlitz.Move(target,cell);
  }

  internal static void Ward(Pawn pawn){
   var def=DefDatabase<HediffDef>.GetNamed(WardDef);
   var ward=pawn.health.hediffSet.GetFirstHediffOfDef(def)??pawn.health.AddHediff(def);
   ward.TryGetComp<HediffComp_Disappears>().ticksToDisappear=240;
  }
 }

 public sealed partial class JobDriver_LunarTechnique : JobDriver {
  bool committed, countered;
  bool telegraphShown;
  LunarPower Power {get{return (LunarPower)job.count;}}
  Pawn Target {get{return job.targetA.Pawn;}}
  HediffComp_LunarAbilities Comp {get{return HediffComp_LunarAbilities.On(pawn);}}

  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){
   base.ExposeData();
   Scribe_Values.Look(ref committed,"EH_lunarImpactCommitted");
   Scribe_Values.Look(ref countered,"EH_lunarCountered");
   ExposeMovesetData();
  }

  internal bool TryParry(Pawn attacker){
   if(movesetVersion!=0||Power!=LunarPower.Parry||countered||committed||attacker!=Target||!CelestialCombat.Hostile(pawn,attacker)||
    pawn.Position.DistanceToSquared(attacker.Position)>2||Comp==null||!Comp.CanAct)return false;
   countered=true;
   return true;
  }

  IEnumerable<Toil> LegacyToils(){
   AddFailCondition(()=>Comp==null||!Comp.CanAct||job.count<0||job.count>4);
   yield return Toils_General.Do(LegacyPrepare);
   if(Power==LunarPower.Parry){
    var watch=Toils_General.Wait(120,TargetIndex.A);
    watch.handlingFacing=true;
    watch.tickAction=()=>{
     if(!CelestialCombat.Hostile(pawn,Target)||pawn.Position.DistanceToSquared(Target.Position)>2){EndJobWith(JobCondition.Incompletable);return;}
     pawn.rotationTracker.FaceCell(Target.Position);
     if(countered)ReadyForNextToil();
    };
    yield return watch;
   }else if(Power!=LunarPower.Veil){
    var windup=Toils_General.Wait(Power==LunarPower.FallingMoon?36:Power==LunarPower.Step?6:10);
    windup.tickAction=()=>{
     pawn.rotationTracker.FaceCell(job.targetB.Cell);
     if(Power==LunarPower.FallingMoon&&!telegraphShown){
      telegraphShown=true;
      var at=job.targetB.Cell.ToVector3Shifted();
      LunarVfx.Emit(pawn,7,at,at,Math.Max(1,ticksLeftThisToil),5.5f);
     }
    };
    yield return windup;
   }
   yield return Toils_General.Do(LegacyImpact);
   yield return Toils_General.Wait(8);
  }

  void LegacyPrepare(){
   pawn.pather.StopDead();
   job.targetB=job.targetA.Cell;
   if(Power==LunarPower.Step){
    IntVec3 landing;
    if(!CelestialCombat.Hostile(pawn,Target)||!LunarCombat.Landing(pawn,Target,out landing)){EndJobWith(JobCondition.Incompletable);return;}
    job.targetB=landing;
   }
   if(Power==LunarPower.FallingMoon)ElementalAudio.Play(pawn,"EH_Sound_Water_Charge");
  }

  void LegacyImpact(){
   if(committed)return;
   committed=true;
   switch(Power){
    case LunarPower.Crescent: LegacyCrescent();break;
    case LunarPower.Step: LegacyStep();break;
    case LunarPower.Parry: if(countered)LegacyCounter();break;
    case LunarPower.FallingMoon: LegacyFallingMoon();break;
    case LunarPower.Veil: LegacyVeil();break;
   }
  }

  void LegacyCrescent(){
   var origin=pawn.Position;
   var end=job.targetB.Cell;
   if(!end.InBounds(pawn.Map)||origin.DistanceToSquared(end)>81)return;
   int inspected=0;
   var hit=new HashSet<int>();
   IntVec3 last=origin;
   foreach(var cell in LightningGeometry.Trace(origin,end)){
    if(cell==origin)continue;
    if(!cell.InBounds(pawn.Map)||cell.Fogged(pawn.Map)||!LightningCombat.ClearLine(last,cell,pawn.Map))break;
    last=cell;
    var things=cell.GetThingList(pawn.Map);
    for(int i=0;i<things.Count&&inspected<512;i++){
     inspected++;
     var victim=things[i] as Pawn;
     if(!CelestialCombat.Hostile(pawn,victim)||!hit.Add(victim.thingIDNumber))continue;
     if(LunarCombat.Strike(pawn,victim,1.4f))Comp.Mark(victim);
     if(hit.Count>=LunarCombat.VictimLimit)break;
    }
    if(hit.Count>=LunarCombat.VictimLimit||inspected>=512)break;
   }
   Comp.AdvanceCombo(Target,1);
   LunarVfx.Emit(pawn,0,pawn.DrawPos,last.ToVector3Shifted(),20,2.8f);
   ElementalAudio.Play(pawn,"EH_Sound_Wind_Slash");
  }

  void LegacyStep(){
   if(!CelestialCombat.Hostile(pawn,Target)||Comp.Marks(Target)==0)return;
   var landing=job.targetB.Cell;
   if(!landing.InBounds(pawn.Map)||pawn.Position.DistanceToSquared(landing)>49||landing.DistanceToSquared(Target.Position)>2||
    !CelestialCombat.Free(pawn,landing)||!CelestialCombat.Path(pawn,pawn.Position,landing)||!LightningCombat.ClearLine(landing,Target.Position,pawn.Map))return;
   var start=pawn.DrawPos;
   var impact=Target.Position;
   ThunderstormBlitz.Move(pawn,landing);
   pawn.rotationTracker.FaceCell(impact);
   LunarVfx.Emit(pawn,1,start,pawn.DrawPos,16,3.3f);
   LunarCombat.Strike(pawn,Target,1.65f);
   foreach(var victim in CelestialCombat.Targets(pawn,impact,3f,LunarCombat.VictimLimit))
    if(victim!=Target)LunarCombat.Pull(pawn,victim,impact);
   Comp.AdvanceCombo(Target,2);
   ElementalAudio.Play(pawn,"EH_Sound_Wind_Dash");
  }

  void LegacyCounter(){
   if(!CelestialCombat.Hostile(pawn,Target)||pawn.Position.DistanceToSquared(Target.Position)>2)return;
   IntVec3 flank;
   if(CelestialCombat.Landing(pawn,pawn.Position,Target,1.5f,out flank))ThunderstormBlitz.Move(pawn,flank);
   pawn.rotationTracker.FaceCell(Target.Position);
   LunarVfx.Emit(pawn,8,pawn.DrawPos,pawn.DrawPos,18,3f);
   if(LunarCombat.Strike(pawn,Target,1.2f)&&CelestialCombat.Hostile(pawn,Target)){
    Target.pather.StopDead();
    Target.stances.stunner.StunFor(60,pawn);
   }
   ElementalAudio.Play(pawn,"EH_Sound_Water_Parry");
  }

  void LegacyFallingMoon(){
   var center=job.targetB.Cell;
   if(!center.InBounds(pawn.Map)||center.Fogged(pawn.Map)||pawn.Position.DistanceToSquared(center)>100||
    !LightningCombat.Open(center,pawn.Map)||!LightningCombat.ClearLine(pawn.Position,center,pawn.Map))return;
   foreach(var victim in CelestialCombat.Targets(pawn,center,2.8f,LunarCombat.VictimLimit)){
    if(!CelestialCombat.Hostile(pawn,victim))continue;
    int stacks=Comp.Consume(victim);
    LunarCombat.Strike(pawn,victim,2f*(1f+.35f*stacks));
   }
   var at=center.ToVector3Shifted();
   LunarVfx.Emit(pawn,3,at,at,24,6.6f);
   ElementalAudio.Play(pawn,"EH_Sound_Water_Impact");
  }

  void LegacyVeil(){
   LunarCombat.Ward(pawn);
   int protectedCount=1, inspected=0;
   foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,3f,true)){
    if(!cell.InBounds(pawn.Map)||cell.Fogged(pawn.Map)||!LightningCombat.ClearLine(pawn.Position,cell,pawn.Map))continue;
    foreach(var thing in cell.GetThingList(pawn.Map)){
     if(++inspected>256)break;
     var ally=thing as Pawn;
     if(ally==null||ally==pawn||ally.Dead||ally.Downed||pawn.Faction==null||ally.Faction==null||
      (pawn.Faction!=ally.Faction&&pawn.Faction.RelationKindWith(ally.Faction)!=FactionRelationKind.Ally))continue;
     LunarCombat.Ward(ally);
     if(++protectedCount>=LunarCombat.VictimLimit)break;
    }
    if(inspected>256||protectedCount>=LunarCombat.VictimLimit)break;
   }
   LunarVfx.Emit(pawn,7,pawn.DrawPos,pawn.DrawPos,240,6.5f);
   ElementalAudio.Play(pawn,"EH_Sound_Water_Charge");
  }
 }

 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class LunarParryPatch {
  [HarmonyPriority(Priority.First+110)]
  public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){
   var target=__0.Pawn;
   var driver=target==null||target.jobs==null?null:target.jobs.curDriver as JobDriver_LunarTechnique;
   if(driver==null||!driver.TryParry(__instance.CasterPawn))return true;
   __result=new DamageWorker.DamageResult();
   return false;
  }
 }

 [HarmonyPatch(typeof(Pawn),"PreApplyDamage")]
 public static class LunarWardPatch {
  static HediffDef ward;
  public static void Postfix(Pawn __instance,ref DamageInfo __0,bool __1){
   if(__1||__0.Weapon==null||!__0.Weapon.IsRangedWeapon||__0.Instigator==__instance)return;
   if(ward==null)ward=DefDatabase<HediffDef>.GetNamed(LunarCombat.WardDef);
   if(__instance.health.hediffSet.HasHediff(ward))__0.SetAmount(__0.Amount*.70f);
  }
 }
}
