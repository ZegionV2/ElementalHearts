using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class AuroraCombat {
  [ThreadStatic]public static int counterDepth;
  public static readonly string[] Descriptions={
   "Send Winterglass through a broad twenty-two-cell lane. A white/cyan crescent and snow wake strike up to 24 visible hostiles once, stopping at walls. Builds owned Pressure.",
   "Blitz between at most five distinct enemies along rapid checked straight cuts. First approach: 18 cells; later links: eight cells. Aurora ribbons and frosted garment afterimages follow the actual route. A lone opponent receives one stronger dueling cut, never five repeated hits.",
   "Sweep a seven-cell frost crown through up to twenty enemies. Layered crescent frames show the complete spin, but each victim receives one hit. Builds owned Pressure.",
   "Unfurl green, cyan and violet northern-light curtains over a fixed location within 24 cells. A half-second formation and rippling lights precede one six-cell shatter. Consumes only your Pressure, up to 20% bonus on that same impact; surviving enemies briefly freeze. The curtains deal no damage ticks.",
   "Automatically answers two or more visible pressuring enemies within four cells. One six-cell ice burst hits up to 24 enemies, pushes survivors up to four checked cells and freezes ordinary enemies for five seconds with icy-blue tint and mild capped hypothermia. Sovereigns thaw after 0.75 seconds and all victims gain shared thaw protection. Non-lethal keeps control without wounds or hypothermia.",
   "Guard against the next ordinary adjacent melee hit for two seconds. Crack into an ice mirage, retreat to a safe cell and rush back for one counter slash. Also reacts automatically when Auto Combat is enabled. Shares recovery with Still Water; counters cannot recursively parry other counters.",
   "Summon the Throne of Endless Winter at a fixed location within 28 cells. A one-second warning becomes a suspended crystalline crown beneath northern lights, then a sixteen-frame collapse commits one ten-cell impact against at most 32 enemies. Consumes only your Pressure for up to 20% shatter bonus. Snow and falling shards remain visual; walls protect victims."
  };
  public static bool Enemy(Pawn p,Pawn t){return IncarnateCombat.Enemy(p,t)&&!t.IsSlave;}
  public static List<Pawn> Targets(Pawn p,IntVec3 at,float radius,int cap){var result=IncarnateCombat.Targets(p,at,radius,cap);result.RemoveAll(t=>!Enemy(p,t));return result;}
  public static int Crowd(Pawn p){return Targets(p,p.Position,4,2).Count;}
  public static bool Landing(Pawn p,IntVec3 from,Pawn t,float range,out IntVec3 end){end=IntVec3.Invalid;return Enemy(p,t)&&TempestCombat.Landing(p,from,t,range,out end);}
  public static Job Plan(Pawn p,int n,LocalTargetInfo target,bool automatic){
   if(p==null||!p.Spawned||!target.IsValid||!target.Cell.InBounds(p.Map)||target.Cell.Fogged(p.Map)||p.Position.DistanceToSquared(target.Cell)>AuroraIncarnate.Ranges[n]*AuroraIncarnate.Ranges[n])return null;IntVec3 end=IntVec3.Invalid;
   if(n==0){if(target.Cell==p.Position||!LightningCombat.ClearLine(p.Position,target.Cell,p.Map))return null;}
   else if(n==2){if(Targets(p,p.Position,7,1).Count==0)return null;target=p;}
   else if(n==4){if(Crowd(p)<2)return null;target=p;}
   else if(n==1){if(!Landing(p,p.Position,target.Pawn,18,out end))return null;}
   else if(n==5){if(!Enemy(p,target.Pawn))return null;}
   else if(!LightningCombat.Open(target.Cell,p.Map)||!LightningCombat.ClearLine(p.Position,target.Cell,p.Map))return null;
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(AuroraIncarnate.CombatJob),target,IntVec3.Invalid,end);job.count=n;job.playerForced=!automatic;return job;
  }
  public static bool Sovereign(Pawn p){return TempestCombat.Sovereign(p)||AuroraIncarnate.Active(p);}
 }
 public sealed class JobDriver_AuroraTechnique:JobDriver {
  public int phase,ticks,elapsed,duration,links,counterLeg;public bool impacting;bool initialized,moving,committed,triggered,lone;
  IntVec3 start=IntVec3.Invalid,impactCell=IntVec3.Invalid;HashSet<int> struck=new HashSet<int>(),visited=new HashSet<int>();int effect;
  public int Skill{get{return job.count;}}Pawn Target{get{return job.targetA.Pawn;}}IntVec3 Destination{get{return Skill==5&&counterLeg==0?job.targetB.Cell:job.targetC.Cell;}}
  public Vector3 VisualAt{get{return moving?Vector3.Lerp(start.ToVector3Shifted(),Destination.ToVector3Shifted(),Mathf.SmoothStep(0,1,elapsed/(float)Math.Max(1,duration))):pawn.Position.ToVector3Shifted();}}
  public float Lift{get{return Skill==5&&counterLeg==0&&moving?.8f*Mathf.Sin(elapsed/(float)Math.Max(1,duration)*Mathf.PI):0;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref phase,"EH_auroraPhase");Scribe_Values.Look(ref ticks,"EH_auroraTicks");Scribe_Values.Look(ref elapsed,"EH_auroraElapsed");Scribe_Values.Look(ref duration,"EH_auroraDuration");Scribe_Values.Look(ref links,"EH_auroraLinks");Scribe_Values.Look(ref counterLeg,"EH_auroraCounterLeg");Scribe_Values.Look(ref initialized,"EH_auroraInitialized");Scribe_Values.Look(ref moving,"EH_auroraMoving");Scribe_Values.Look(ref committed,"EH_auroraCommitted");Scribe_Values.Look(ref triggered,"EH_auroraTriggered");Scribe_Values.Look(ref lone,"EH_auroraLone");Scribe_Values.Look(ref start,"EH_auroraStart",IntVec3.Invalid);Scribe_Values.Look(ref impactCell,"EH_auroraImpactCell",IntVec3.Invalid);Scribe_Collections.Look(ref struck,"EH_auroraStruck",LookMode.Value);Scribe_Collections.Look(ref visited,"EH_auroraVisited",LookMode.Value);if(struck==null)struck=new HashSet<int>();if(visited==null)visited=new HashSet<int>();}
  bool Valid(){return pawn.CurJob==job&&AuroraIncarnate.Active(pawn)&&HediffComp_ElementalTransform.Active(pawn)&&!HediffComp_ElementalTransform.Transforming(pawn)&&ElementalTeaching.Ready(pawn)&&(!ElementalCombatPolicy.Controlled(pawn)||pawn.Drafted)&&!pawn.WorkTagIsDisabled(WorkTags.Violent)&&!ElementalAnimationBridge.ExternallyControlled(pawn);}
  void StopEffect(){if(effect!=0&&pawn.Spawned)pawn.Map.GetComponent<MapComponent_AuroraVfx>().Stop(effect);effect=0;}
  void Visual(){StopEffect();if(!pawn.Spawned)return;if(Skill==3){int sprite=phase==0?5:phase==4?6:7;int total=phase==0?30:phase==4?24:30;effect=AuroraVfx.Owned(pawn,sprite,impactCell.ToVector3Shifted(),16,20,total,ticks);}else if(Skill==6){int sprite=phase==0?14:phase==4?15:16;int total=phase==0?60:phase==4?18:48;effect=AuroraVfx.Owned(pawn,sprite,impactCell.ToVector3Shifted(),phase==0?24:32,phase==0?24:38,total,ticks);}}
  void Finish(){moving=impacting=false;StopEffect();var water=HediffComp_WaterAbilities.On(pawn);if(water!=null)water.nextAutoCheck=Find.TickManager.TicksGame+6;if(pawn.Spawned&&pawn.Drawer!=null)pawn.Drawer.tweener.ResetTweenedPosToRoot();}
  bool Hit(Pawn victim,float multiplier,bool consume=false,int freeze=0,int push=0,IntVec3? origin=null,bool counter=false){
   if(!Valid()||!AuroraCombat.Enemy(pawn,victim)||!LightningCombat.ClearLine(origin??pawn.Position,victim.Position,pawn.Map)||!struck.Add(victim.thingIDNumber))return false;
   if(counter){AuroraCombat.counterDepth++;WaterDefense.counterDepth++;}
   try{
    if(WaterDefense.TryParry(pawn,victim))return false;var profile=LightningCombat.Profile(pawn,10);float amount=profile.damage*multiplier*ElementalTeaching.DamageFactor(pawn);bool nonLethal=ElementalNonLethal.Enabled(pawn);int marks=consume&&!nonLethal?WaterPressure.Stacks(pawn,victim):0;amount*=1+Math.Min(3,marks)/15f;
    bool landed;if(nonLethal){ElementalNonLethal.TryApply(pawn,victim,amount);landed=true;}else{var result=victim.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("EH_WaterDamage"),amount,Math.Max(.5f,profile.penetration+.3f),-1,pawn,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,victim));landed=result!=null&&result.totalDamageDealt>0;if(landed){if(consume){WaterPressure.Consume(pawn,victim);if(marks>=3)HediffComp_AuroraIncarnate.On(pawn).wardUntil=Find.TickManager.TicksGame+120;}else WaterPressure.Apply(pawn,victim);}}
    if(landed&&freeze>0&&!victim.Dead&&!victim.Downed&&victim.Spawned)GlacialFreeze.Apply(pawn,victim,!nonLethal&&Skill==4,freeze,push,true);return landed;
   }finally{if(counter){WaterDefense.counterDepth--;AuroraCombat.counterDepth--;}}
  }
  void MoveStart(){start=pawn.Position;elapsed=0;duration=Math.Max(4,Math.Min(10,(int)Math.Ceiling(start.DistanceTo(Destination)*.45f)+3));moving=true;phase=1;ticks=0;pawn.Rotation=Rot4.FromAngleFlat((Destination-start).ToVector3().AngleFlat());ElementalAudio.Ability(pawn,ElementalSound.Dash);if(Skill==1)AuroraVfx.Burst(pawn,2,pawn.Position.ToVector3Shifted(),3.5f,12);}
  bool Link(Pawn target,float range){IntVec3 end;if(!AuroraCombat.Landing(pawn,pawn.Position,target,range,out end)||!visited.Add(target.thingIDNumber))return false;job.targetA=target;job.targetC=end;links++;MoveStart();return true;}
  void Recover(){moving=false;phase=3;ticks=0;}
  void Impact(){
   if(committed)return;committed=true;impacting=true;try{
    ElementalAudio.Ability(pawn,Skill==0?ElementalSound.Slash:Skill==2?ElementalSound.Spin:ElementalSound.Burst);
    if(Skill==0){var direction=(job.targetA.Cell-pawn.Position).ToVector3().normalized;var last=pawn.Position;var end=(pawn.Position.ToVector3Shifted()+direction*22).ToIntVec3();foreach(var cell in LightningGeometry.Trace(pawn.Position,end)){if(!cell.InBounds(pawn.Map)||!LightningCombat.Open(cell,pawn.Map)||pawn.Position.DistanceToSquared(cell)>484)break;last=cell;}float length=pawn.Position.DistanceTo(last);foreach(var t in AuroraCombat.Targets(pawn,pawn.Position,22,24)){var off=(t.Position-pawn.Position).ToVector3();float along=Vector3.Dot(off,direction),across=Mathf.Abs(off.x*direction.z-off.z*direction.x);if(along>=0&&along<=length+.5f&&across<=1.75f)Hit(t,4f);}AuroraVfx.Wave(pawn,0,pawn.DrawPos,last.ToVector3Shifted(),26,6);AuroraVfx.Line(pawn,1,pawn.DrawPos,last.ToVector3Shifted(),24,1.1f);}
    else if(Skill==2){foreach(var t in AuroraCombat.Targets(pawn,pawn.Position,7,20))Hit(t,4.9f);AuroraVfx.Burst(pawn,4,pawn.DrawPos,20,36);}
    else if(Skill==4){foreach(var t in AuroraCombat.Targets(pawn,pawn.Position,6,24))Hit(t,5.5f,false,300,4);AuroraVfx.Burst(pawn,8,pawn.DrawPos,18,42);}
    else if(Skill==3||Skill==6){float radius=Skill==3?6:10;foreach(var t in AuroraCombat.Targets(pawn,impactCell,radius,Skill==3?24:32))Hit(t,Skill==3?6.7f:8.4f,true,45,0,impactCell);AuroraVfx.Burst(pawn,Skill==3?7:16,impactCell.ToVector3Shifted(),Skill==3?17:30,Skill==3?30:48);}
   }finally{impacting=false;}
  }
  void Contact(){impacting=true;try{if(AuroraCombat.Enemy(pawn,Target)&&pawn.Position.DistanceToSquared(Target.Position)<=2){pawn.rotationTracker.FaceTarget(Target);Hit(Target,Skill==5?7.2f:lone?5.4f:4.2f,false,0,0,null,Skill==5);AuroraVfx.Burst(pawn,Skill==5?13:3,Target.DrawPos,Skill==5?7:5,24);ElementalAudio.Ability(pawn,ElementalSound.Slash);}}finally{impacting=false;}if(Skill==5){committed=true;Recover();}else{phase=2;ticks=0;}}
  public bool Trigger(Pawn attacker){if(Skill!=5||phase!=0||triggered||ticks>=120||attacker!=Target||!Valid()||pawn.Position.DistanceToSquared(attacker.Position)>2||AuroraCombat.counterDepth>0||WaterDefense.counterDepth>0)return false;triggered=true;var defense=HediffComp_WaterDefense.On(pawn);if(defense!=null)defense.readyAt=Math.Max(defense.readyAt,Find.TickManager.TicksGame+defense.Props.recoveryTicks);AuroraVfx.Burst(pawn,11,pawn.DrawPos,4,20);ElementalAudio.Ability(pawn,ElementalSound.Parry);return true;}
  void Tick(){
   pawn.pather.StopDead();ticks++;
   if(Skill==3||Skill==6){if(!impactCell.InBounds(pawn.Map)||!LightningCombat.Open(impactCell,pawn.Map)){EndJobWith(JobCondition.Incompletable);return;}if(phase==0){if(ticks<(Skill==3?30:60))return;phase=4;ticks=0;Visual();return;}if(phase==4){if(ticks<(Skill==3?24:18))return;phase=5;ticks=0;Visual();if(Skill==3)Impact();return;}if(phase==5){if(Skill==6&&ticks>=18&&!committed)Impact();if(ticks>=(Skill==3?30:48))ReadyForNextToil();return;}}
   if(Skill==5&&phase==0){if(!AuroraCombat.Enemy(pawn,Target)){EndJobWith(JobCondition.Incompletable);return;}pawn.rotationTracker.FaceTarget(Target);if(!triggered){if(ticks>=120)ReadyForNextToil();return;}IntVec3 retreat,end;if(TempestCombat.Reversal(pawn,Target,out retreat,out end)){job.targetB=retreat;job.targetC=end;counterLeg=0;MoveStart();}else Contact();return;}
   if(Skill==1&&phase==0){if(ticks<4)return;lone=AuroraCombat.Targets(pawn,pawn.Position,18,2).Count<2;if(!Link(Target,18))EndJobWith(JobCondition.Incompletable);return;}
   if(phase==0){if(ticks<(Skill==4?10:6))return;Impact();Recover();return;}
   if(phase==1){
    if(!Destination.IsValid||!ThunderstormBlitz.Landing(pawn,Destination)||!AuroraCombat.Enemy(pawn,Target)){EndJobWith(JobCondition.Incompletable);return;}var from=VisualAt;int next=Math.Min(duration,elapsed+1);var to=Vector3.Lerp(start.ToVector3Shifted(),Destination.ToVector3Shifted(),Mathf.SmoothStep(0,1,next/(float)duration));if(!TempestCombat.Route(pawn,pawn.Position,to.ToIntVec3(),Skill==5&&counterLeg==0?null:Target)){EndJobWith(JobCondition.Incompletable);return;}
    AuroraVfx.Line(pawn,Skill==5&&counterLeg==0?12:1,from,to,20,1.5f);if(elapsed%2==0)AuroraVfx.Echo(pawn,from);if(ThunderstormBlitz.Landing(pawn,to.ToIntVec3()))ThunderstormBlitz.Move(pawn,to.ToIntVec3());elapsed=next;if(elapsed<duration)return;moving=false;ThunderstormBlitz.Move(pawn,Destination);
    if(Skill==5&&counterLeg==0){counterLeg=1;IntVec3 end;if(!AuroraCombat.Landing(pawn,pawn.Position,Target,8,out end)){EndJobWith(JobCondition.Incompletable);return;}job.targetC=end;MoveStart();return;}Contact();return;
   }
   if(phase==2){if(ticks<4)return;if(links<5)foreach(var target in AuroraCombat.Targets(pawn,pawn.Position,8,16))if(!visited.Contains(target.thingIDNumber)&&Link(target,8))return;Recover();return;}
   if(phase==3&&ticks>=8)ReadyForNextToil();
  }
  protected override IEnumerable<Toil> MakeNewToils(){AddFinishAction(c=>Finish());this.FailOn(()=>!Valid());var form=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};form.initAction=()=>{pawn.pather.StopDead();pawn.stances.CancelBusyStanceHard();AuroraVfx.Track(pawn);if(initialized){if(Skill==3||Skill==6)Visual();return;}initialized=true;start=pawn.Position;impactCell=job.targetA.Cell;phase=0;ticks=0;ElementalAudio.Ability(pawn,Skill>=3?ElementalSound.Charge:ElementalSound.Slash);if(Skill==3||Skill==6)Visual();};form.tickAction=Tick;yield return form;}
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class AuroraDrawPosition {public static void Postfix(Pawn ___pawn,ref Vector3 __result){var d=___pawn==null||___pawn.jobs==null?null:___pawn.jobs.curDriver as JobDriver_AuroraTechnique;if(d==null)return;var at=d.VisualAt;at.y=__result.y;at.z+=d.Lift;__result=at;}}
 [HarmonyPatch(typeof(Verb_MeleeAttackDamage),"ApplyMeleeDamageToTarget")]
 public static class MirrorfrostGuard {
  [HarmonyPriority(Priority.First+250)]public static bool Prefix(Verb_MeleeAttackDamage __instance,LocalTargetInfo __0,ref DamageWorker.DamageResult __result){var victim=__0.Pawn;var c=HediffComp_AuroraIncarnate.On(victim);var attacker=__instance.CasterPawn;if(c==null||attacker==null||AuroraCombat.counterDepth>0||WaterDefense.counterDepth>0||!AuroraCombat.Enemy(victim,attacker)||victim.Position.DistanceToSquared(attacker.Position)>2)return true;var d=victim.jobs.curDriver as JobDriver_AuroraTechnique;if(d==null){var defense=HediffComp_WaterDefense.On(victim);if(ElementalCombatPolicy.Controlled(victim)&&victim.CurJobDef!=JobDefOf.AttackMelee&&victim.CurJobDef!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(victim,victim.CurJob))return true;if(defense==null||!defense.Active||defense.PendingCounter||!c.CanAct||c.Remaining(5)>0||IncarnateCombatFlow.BlockingOrders(victim)||!c.Start(5,attacker,true))return true;d=victim.jobs.curDriver as JobDriver_AuroraTechnique;}if(d==null||!d.Trigger(attacker))return true;__result=new DamageWorker.DamageResult();return false;}
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class AuroraWard {public static void Prefix(Thing __instance,ref DamageInfo dinfo){var c=HediffComp_AuroraIncarnate.On(__instance as Pawn);if(c!=null&&c.wardUntil>Find.TickManager.TicksGame&&dinfo.Amount>0)dinfo.SetAmount(dinfo.Amount*.8f);}}
}
