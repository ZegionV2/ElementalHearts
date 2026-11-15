using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public static class TempestCombat {
  public const float BeamRadius=10;
  public const int BeamWarningTicks=60,BeamImpactTicks=18,BeamDischargeTicks=72;
  public static readonly string[] Descriptions={
   "Send a broad white-edged cloud crescent down a clear twenty-cell lane. Stops at walls; up to 24 enemies are struck once.",
   "Blitz up to eighteen cells into a checked, empty melee landing. A white lightning wake and fitted grey afterimages trace the rush.",
   "Turn Skyrend through an eight-cell pressure wheel. Up to twenty enemies receive one deliberate impact each.",
   "Watch an opponent for up to two seconds. Parry their next ordinary adjacent melee strike, or react to a successful dodge, dissolve into cloud, sidestep and rush back with a countercut. New orders cancel the guard. This never recursively parries an ability or another counter.",
   "Approach through a checked route, then deliver three distinct timed sword cuts with safe flanking footwork: a rising strike, an airborne follow-up and a heavy descending finisher. Three impacts maximum on the primary; no repeating damage field.",
   "Form a local maelstrom, briefly lift at most eight enemies and release them with one pressure strike and a safe outward push. Hold lasts 1.25 seconds, shortened to half a second against sovereign lineages. Uninterruptible targets take the release strike normally. Interruption releases all owned captures.",
   "Mark a fixed location within twenty-eight cells. A one-second storm seal warns enemies before a grand cloud funnel descends. At most 32 enemies in ten cells receive one committed impact. The remaining lightning and clouds are visual only. No weather, roof, terrain or building changes."
  };
  public static bool Enemy(Pawn p,Pawn t){return IncarnateCombat.Enemy(p,t);}
  public static List<Pawn> Targets(Pawn p,IntVec3 at,float radius,int cap){return IncarnateCombat.Targets(p,at,radius,cap);}
  public static bool Strike(Pawn source,Pawn target,LightningCombat.AttackProfile profile,float blade,float pressure){
   if(!Enemy(source,target))return false;
   float damage=profile.damage*ElementalTeaching.DamageFactor(source);
   if(ElementalNonLethal.TryApply(source,target,damage*(blade+pressure)))return true;
   if(blade>0)target.TakeDamage(new DamageInfo(profile.physicalDef,damage*blade,profile.penetration,-1,source,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,target));
   if(!target.Dead&&!target.Destroyed&&pressure>0)target.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("EH_TempestPressure"),damage*pressure,.5f,-1,source,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,target));
   return true;
  }
  public static bool Sovereign(Pawn p){return AuroraIncarnate.Active(p)||CelestialAscendant.Active(p)||ThunderIncarnate.Active(p)||HellFlameIncarnate.Active(p)||TempestIncarnate.Active(p)||EclipseSky.Empowered(p);}
  public static bool Threatening(Pawn p,Pawn t){return Enemy(p,t)&&p.Position.DistanceToSquared(t.Position)<=25&&t.CurJob!=null&&t.CurJobDef==JobDefOf.AttackMelee&&t.CurJob.targetA.Pawn==p;}
  public static bool Route(Pawn p,IntVec3 a,IntVec3 b,Pawn target){
   if(!IncarnateCombat.Route(p,a,b,target))return false;var prior=a;
   foreach(var cell in LightningGeometry.Trace(a,b)){
    var door=cell.GetEdifice(p.Map) as Building_Door;if(door!=null&&!door.Open)return false;
    int dx=cell.x-prior.x,dz=cell.z-prior.z;
    if(dx!=0&&dz!=0&&(!LightningCombat.Open(new IntVec3(prior.x+dx,0,prior.z),p.Map)||!LightningCombat.Open(new IntVec3(prior.x,0,prior.z+dz),p.Map)))return false;
    prior=cell;
   }return true;
  }
  public static bool Landing(Pawn p,IntVec3 from,Pawn target,float range,out IntVec3 end){end=IntVec3.Invalid;return Enemy(p,target)&&ThunderstormBlitz.ChooseLanding(from,target.Position,range,c=>ThunderstormBlitz.Landing(p,c),(a,b)=>Route(p,a,b,target),out end);}
  public static bool Reversal(Pawn p,Pawn target,out IntVec3 retreat,out IntVec3 end){
   retreat=end=IntVec3.Invalid;float best=float.MinValue;if(!Enemy(p,target))return false;
   foreach(var cell in GenRadial.RadialCellsAround(p.Position,4,false)){
    int distance=cell.DistanceToSquared(target.Position);IntVec3 landing;
    if(distance<5||distance>36||!ThunderstormBlitz.Landing(p,cell)||!Route(p,p.Position,cell,null)||!Landing(p,cell,target,8,out landing))continue;
    float score=distance-.1f*cell.DistanceToSquared(p.Position);if(score<=best)continue;best=score;retreat=cell;end=landing;
   }return retreat.IsValid;
  }
  public static Job Plan(Pawn p,int power,LocalTargetInfo target,bool automatic){
   if(p==null||!p.Spawned||!target.IsValid||!target.Cell.InBounds(p.Map)||target.Cell.Fogged(p.Map)||p.Position.DistanceToSquared(target.Cell)>TempestIncarnate.Ranges[power]*TempestIncarnate.Ranges[power])return null;
   IntVec3 end=IntVec3.Invalid;
   if(power==0){if(target.Cell==p.Position||!LightningCombat.ClearLine(p.Position,target.Cell,p.Map))return null;}
   else if(power==2||power==5){if(Targets(p,p.Position,8,1).Count==0)return null;target=p;}
   else if(power==3){if(!Enemy(p,target.Pawn))return null;}
   else if(power==6){if(!LightningCombat.Open(target.Cell,p.Map)||!LightningCombat.ClearLine(p.Position,target.Cell,p.Map))return null;end=target.Cell;}
   else if(!Landing(p,p.Position,target.Pawn,TempestIncarnate.Ranges[power],out end))return null;
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(TempestIncarnate.CombatJob),target,IntVec3.Invalid,end);job.count=power;job.playerForced=!automatic;return job;
  }
 }
 public sealed class JobDriver_TempestTechnique:JobDriver {
  public int phase,ticks,elapsed,duration,leg;
  IntVec3 start=IntVec3.Invalid,impactCell=IntVec3.Invalid;
  bool initialized,moving,committed,empowered,flowCommitted,captured,released,triggered;
  public bool impacting;
  HashSet<long> struck=new HashSet<long>();List<Pawn> held=new List<Pawn>();
  int effect;
  public int Skill{get{return job.count;}}
  Pawn Target{get{return job.targetA.Pawn;}}
  IntVec3 Destination{get{return Skill==3&&leg==0?job.targetB.Cell:job.targetC.Cell;}}
  public Vector3 VisualAt{get{return moving?Vector3.Lerp(start.ToVector3Shifted(),Destination.ToVector3Shifted(),Mathf.SmoothStep(0,1,elapsed/(float)Math.Max(1,duration))):pawn.Position.ToVector3Shifted();}}
  public float Lift{get{return Skill==4&&phase==2&&leg==2?.65f*Mathf.Sin(Mathf.Clamp01(ticks/12f)*Mathf.PI):0;}}
  public bool CaptureJob{get{return Skill==5||Skill==4;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){
   base.ExposeData();Scribe_Values.Look(ref phase,"EH_tempestPhase");Scribe_Values.Look(ref ticks,"EH_tempestTicks");Scribe_Values.Look(ref elapsed,"EH_tempestElapsed");Scribe_Values.Look(ref duration,"EH_tempestDuration");Scribe_Values.Look(ref leg,"EH_tempestLeg");
   Scribe_Values.Look(ref start,"EH_tempestStart",IntVec3.Invalid);Scribe_Values.Look(ref impactCell,"EH_tempestImpact",IntVec3.Invalid);
   Scribe_Values.Look(ref initialized,"EH_tempestInitialized");Scribe_Values.Look(ref moving,"EH_tempestMoving");Scribe_Values.Look(ref committed,"EH_tempestCommitted");Scribe_Values.Look(ref empowered,"EH_tempestEmpowered");Scribe_Values.Look(ref flowCommitted,"EH_tempestFlowCommitted");Scribe_Values.Look(ref captured,"EH_tempestCaptured");Scribe_Values.Look(ref released,"EH_tempestReleased");Scribe_Values.Look(ref triggered,"EH_tempestTriggered");
   Scribe_Collections.Look(ref struck,"EH_tempestStruck",LookMode.Value);Scribe_Collections.Look(ref held,"EH_tempestHeld",LookMode.Reference);
   if(struck==null)struck=new HashSet<long>();if(held==null)held=new List<Pawn>();
  }
  bool Valid(){return pawn.CurJob==job&&TempestIncarnate.Active(pawn)&&HediffComp_ElementalTransform.Active(pawn)&&!HediffComp_ElementalTransform.Transforming(pawn)&&ElementalTeaching.Ready(pawn)&&!pawn.WorkTagIsDisabled(WorkTags.Violent)&&(!ElementalCombatPolicy.Controlled(pawn)||pawn.Drafted)&&!ElementalAnimationBridge.ExternallyControlled(pawn);}
  void MoveStart(){start=pawn.Position;elapsed=0;duration=Math.Max(4,Math.Min(10,(int)Math.Ceiling(start.DistanceTo(Destination)*.55f)+3));moving=true;phase=1;ticks=0;pawn.Rotation=Rot4.FromAngleFlat((Destination-start).ToVector3().AngleFlat());ElementalAudio.Ability(pawn,ElementalSound.Dash);}
  void StopEffect(){if(effect!=0&&pawn.Spawned)pawn.Map.GetComponent<MapComponent_TempestVfx>().Stop(effect);effect=0;}
  void Visual(){
   StopEffect();if(!pawn.Spawned)return;
   if(Skill==6){if(phase==0)effect=TempestVfx.Owned(pawn,3,impactCell.ToVector3Shifted(),26,26,60,ticks);else if(phase==4)effect=TempestVfx.Owned(pawn,11,impactCell.ToVector3Shifted(),32,46,72,ticks);}
   if(Skill==5){if(ticks<24)effect=TempestVfx.Owned(pawn,23,pawn.DrawPos,22,22,24,ticks);else if(ticks<99)effect=TempestVfx.Owned(pawn,24,pawn.DrawPos,23,23,75,ticks-24);}
  }
  void Finish(){
   moving=false;impacting=false;StopEffect();
   foreach(var target in held){var c=CycloneAirbound.On(target);if(c!=null&&c.Owned(pawn,job.loadID)&&c.Controlling)c.Cancel();}
   var wind=HediffComp_WindAbilities.On(pawn);if(wind!=null)wind.nextAutoCheck=Find.TickManager.TicksGame+6;
   if(!pawn.Spawned||pawn.Dead||pawn.Downed||ThunderstormBlitz.Landing(pawn,pawn.Position))return;
   foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,1.5f,false))if(ThunderstormBlitz.Landing(pawn,cell)&&TempestCombat.Route(pawn,pawn.Position,cell,null)){ThunderstormBlitz.Move(pawn,cell);return;}
   if(start.IsValid&&ThunderstormBlitz.Landing(pawn,start)&&TempestCombat.Route(pawn,pawn.Position,start,null))ThunderstormBlitz.Move(pawn,start);
  }
  bool Hit(Pawn target,float blade,float pressure,int cut=0){
   if(!Valid()||!TempestCombat.Enemy(pawn,target)||!LightningCombat.ClearLine(pawn.Position,target.Position,pawn.Map))return false;
   long key=((long)target.thingIDNumber<<2)|(uint)cut;if(!struck.Add(key))return false;
   if(WaterDefense.TryParry(pawn,target))return false;
   if(!flowCommitted){flowCommitted=true;empowered=HediffComp_TempestIncarnate.On(pawn).Commit(Skill);}
   var profile=LightningCombat.Profile(pawn,10);profile.penetration+=.3f;if(empowered){profile.damage*=1.2f;profile.penetration+=.15f;}
   return TempestCombat.Strike(pawn,target,profile,blade,pressure);
  }
  void Impact(){
   if(committed)return;committed=true;impacting=true;
   try{
    ElementalAudio.Ability(pawn,Skill==0?ElementalSound.Slash:Skill==2?ElementalSound.Spin:ElementalSound.Impact);
    if(Skill==0){
     var direction=(job.targetA.Cell-pawn.Position).ToVector3().normalized;var end=(pawn.Position.ToVector3Shifted()+direction*20).ToIntVec3();var last=pawn.Position;
     foreach(var cell in LightningGeometry.Trace(pawn.Position,end)){if(!cell.InBounds(pawn.Map)||!LightningCombat.Open(cell,pawn.Map)||pawn.Position.DistanceToSquared(cell)>400)break;last=cell;}
     float length=pawn.Position.DistanceTo(last);int count=0;
     foreach(var t in TempestCombat.Targets(pawn,pawn.Position,20,32)){var off=(t.Position-pawn.Position).ToVector3();float along=Vector3.Dot(off,direction),across=Mathf.Abs(off.x*direction.z-off.z*direction.x);if(along<0||along>length+.5f||across>1.7f)continue;if(count++>=24)break;Hit(t,3.3f,.65f);}
     TempestVfx.Wave(pawn,0,pawn.DrawPos,last.ToVector3Shifted(),26,6);TempestVfx.Line(pawn,1,pawn.DrawPos,last.ToVector3Shifted(),22,1.2f);
    }else if(Skill==1||Skill==3){if(TempestCombat.Enemy(pawn,Target)&&pawn.Position.DistanceToSquared(Target.Position)<=2){Hit(Target,Skill==3?6.2f:3.8f,Skill==3?1.1f:.75f);TempestVfx.Burst(pawn,Skill==3?21:15,Target.DrawPos,5,24);}}
    else if(Skill==2){foreach(var t in TempestCombat.Targets(pawn,pawn.Position,8,20))Hit(t,4f,.9f);TempestVfx.Burst(pawn,2,pawn.DrawPos,26,38);}
    else if(Skill==6){foreach(var t in TempestCombat.Targets(pawn,impactCell,10,32))Hit(t,6.4f,1.6f);TempestVfx.Burst(pawn,7,impactCell.ToVector3Shifted(),30,44);}
   }finally{impacting=false;}
  }
  void Cut(){
   if(!TempestCombat.Enemy(pawn,Target)||pawn.Position.DistanceToSquared(Target.Position)>2){EndJobWith(JobCondition.Incompletable);return;}
   impacting=true;try{
    if(leg==0){Hit(Target,2.6f,.5f,0);TempestVfx.Burst(pawn,22,Target.DrawPos,6,18);}
    else if(leg==1){if(Hit(Target,2.8f,.55f,1)&&!Target.Dead&&!Target.Downed&&CycloneAirbound.Apply(pawn,Target,14,job.loadID)&&!held.Contains(Target))held.Add(Target);TempestVfx.Burst(pawn,22,Target.DrawPos,8,22);}
    else{committed=true;Hit(Target,5.8f,1.2f,2);TempestVfx.Burst(pawn,4,Target.DrawPos,12,28);var c=CycloneAirbound.On(Target);if(c!=null&&c.Owned(pawn,job.loadID))c.Release(true);}
    ElementalAudio.Ability(pawn,leg==2?ElementalSound.Impact:ElementalSound.Slash);
   }finally{impacting=false;}
   if(leg>=2){phase=3;ticks=0;return;}leg++;ticks=0;
   // Recheck each short flank; never teleport through the target, walls or occupied cells.
   foreach(var cell in GenAdj.CellsAdjacent8Way(Target)){
    if(cell==pawn.Position||!ThunderstormBlitz.Landing(pawn,cell)||!TempestCombat.Route(pawn,pawn.Position,cell,null)||!LightningCombat.ClearLine(cell,Target.Position,pawn.Map))continue;
    job.targetC=cell;MoveStart();return;
   }phase=2;
  }
  void Capture(){
   if(captured)return;captured=true;
   foreach(var t in TempestCombat.Targets(pawn,pawn.Position,8,8)){
    // Store the bounded candidate list too: anchored/immune targets still face the one release strike.
    held.Add(t);CycloneAirbound.Apply(pawn,t,TempestCombat.Sovereign(t)?30:75,job.loadID);
   }Visual();ElementalAudio.Ability(pawn,ElementalSound.Spin);
  }
  void Release(){
   if(released)return;released=committed=true;impacting=true;
   try{foreach(var t in held){var c=CycloneAirbound.On(t);try{if(TempestCombat.Enemy(pawn,t)&&pawn.Position.DistanceToSquared(t.Position)<=81)Hit(t,6.8f,1.2f);}finally{if(c!=null&&c.Owned(pawn,job.loadID))c.Release(true);}}}
   finally{impacting=false;}StopEffect();TempestVfx.Burst(pawn,12,pawn.DrawPos,27,32);ElementalAudio.Ability(pawn,ElementalSound.Burst);
  }
  public bool Parry(Pawn attacker){if(Skill!=3||phase!=0||triggered||ticks>=120||attacker!=Target||!Valid()||!TempestCombat.Enemy(pawn,attacker)||pawn.Position.DistanceToSquared(attacker.Position)>2)return false;triggered=true;return true;}
  void Tick(){
   pawn.pather.StopDead();ticks++;
   if(Skill==5){if(ticks>=24&&!captured)Capture();if(ticks>=99&&!released)Release();if(ticks>=119)ReadyForNextToil();return;}
   if(Skill==6){if(phase==0){if(!impactCell.InBounds(pawn.Map)||!LightningCombat.Open(impactCell,pawn.Map)){EndJobWith(JobCondition.Incompletable);return;}if(ticks<60)return;phase=4;ticks=0;Visual();return;}if(phase==4){if(ticks>=18&&!committed)Impact();if(ticks>=72){phase=3;ticks=0;}return;}if(phase==3&&ticks>=8)ReadyForNextToil();return;}
   if(Skill==3&&phase==0){if(!TempestCombat.Enemy(pawn,Target)){EndJobWith(JobCondition.Incompletable);return;}pawn.rotationTracker.FaceTarget(Target);if(!triggered){if(ticks>=120)ReadyForNextToil();return;}IntVec3 retreat,end;TempestVfx.Burst(pawn,5,pawn.DrawPos,5,20);TempestVfx.Echo(pawn,pawn.DrawPos);if(TempestCombat.Reversal(pawn,Target,out retreat,out end)){job.targetB=retreat;job.targetC=end;leg=0;MoveStart();}else{Impact();phase=3;ticks=0;}return;}
   if(phase==0){if(ticks<(Skill==2?8:5))return;Impact();phase=3;ticks=0;return;}
   if(phase==1){
    if(!Destination.IsValid||!ThunderstormBlitz.Landing(pawn,Destination)){EndJobWith(JobCondition.Incompletable);return;}
    var from=VisualAt;int next=Math.Min(duration,elapsed+1);var to=Vector3.Lerp(start.ToVector3Shifted(),Destination.ToVector3Shifted(),Mathf.SmoothStep(0,1,next/(float)duration));
    if(!TempestCombat.Route(pawn,pawn.Position,to.ToIntVec3(),Skill==3&&leg==0?null:Target)){EndJobWith(JobCondition.Incompletable);return;}
    TempestVfx.Line(pawn,1,from,to,18,1.5f);if(elapsed%2==0)TempestVfx.Echo(pawn,from);ThunderstormBlitz.Move(pawn,to.ToIntVec3());elapsed=next;if(elapsed<duration)return;moving=false;ThunderstormBlitz.Move(pawn,Destination);
    if(Skill==3&&leg==0){leg=1;IntVec3 end;if(!TempestCombat.Landing(pawn,pawn.Position,Target,8,out end)){EndJobWith(JobCondition.Incompletable);return;}job.targetC=end;MoveStart();return;}
    if(Skill==4){phase=2;ticks=0;return;}Impact();phase=3;ticks=0;return;
   }
   if(phase==2){if(ticks>=(leg==2?12:7))Cut();return;}if(phase==3&&ticks>=8)ReadyForNextToil();
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(c=>Finish());this.FailOn(()=>!Valid());var form=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};
   form.initAction=()=>{pawn.pather.StopDead();pawn.stances.CancelBusyStanceHard();TempestVfx.Track(pawn);if(initialized){Visual();return;}initialized=true;start=pawn.Position;impactCell=job.targetA.Cell;phase=0;ticks=0;ElementalAudio.Ability(pawn,Skill>=5?ElementalSound.Charge:ElementalSound.Slash);if(Skill==5||Skill==6)Visual();if(Skill==1||Skill==4)MoveStart();};
   form.tickAction=Tick;yield return form;
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class TempestDrawPosition {public static void Postfix(Pawn ___pawn,ref Vector3 __result){var d=___pawn.jobs==null?null:___pawn.jobs.curDriver as JobDriver_TempestTechnique;if(d==null)return;var pos=d.VisualAt;pos.y=__result.y;pos.z+=d.Lift;__result=pos;}}
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class StormveilGuard {
  [HarmonyPriority(Priority.First+150)]
  public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){var target=__0.Pawn;var d=target==null||target.jobs==null?null:target.jobs.curDriver as JobDriver_TempestTechnique;if(d==null||!d.Parry(__instance.CasterPawn))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 [HarmonyPatch(typeof(Verb_MeleeAttack),"SoundDodge")]
 public static class StormveilDodge {public static void Postfix(Verb_MeleeAttack __instance,Thing __0){var p=__0 as Pawn;var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_TempestTechnique;if(d!=null)d.Parry(__instance.CasterPawn);}}
}
