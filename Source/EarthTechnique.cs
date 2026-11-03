using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class EarthCombat {
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EarthTechnique";}
  public static bool OtherAction(Pawn p){return p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName.StartsWith("EH_")&&!Busy(p);}
  public static bool Anchored(Pawn p){var c=HediffComp_EarthAbilities.On(p);return c!=null&&c.GuardActive;}
  public static bool SafeStep(Pawn p,IntVec3 from,IntVec3 to){
   if(!to.InBounds(p.Map)||!to.Standable(p.Map)||!LightningCombat.ClearLine(from,to,p.Map))return false;
   foreach(var cell in LightningGeometry.Trace(from,to)){
    if(!cell.InBounds(p.Map)||!cell.Standable(p.Map)||!LightningCombat.Open(cell,p.Map))return false;
    var things=cell.GetThingList(p.Map);for(int i=0;i<things.Count;i++)if(things[i] is Fire||things[i] is Pawn&&things[i]!=p)return false;
   }
   var at=to.GetThingList(p.Map);for(int i=0;i<at.Count;i++)if(at[i] is Fire||at[i] is Pawn&&at[i]!=p)return false;
   return true;
  }
  public static bool FindLanding(Pawn p,Pawn target,out IntVec3 landing){
   landing=IntVec3.Invalid;if(!WindCombat.AliveTarget(p,target)||p.Position.DistanceToSquared(target.Position)<=2)return false;
   // Single bounded navigation tree; no obstacle, pawn or diagonal corner bypass.
   var tree=WindNavigation.Tree(p.Position,(a,b)=>SafeStep(p,a,b));int best=int.MaxValue;
   foreach(var cell in GenAdj.CellsAdjacent8Way(target)){
    int d=p.Position.DistanceToSquared(cell);if(d>36||d>=best||!tree.ContainsKey(cell)||!LightningCombat.ClearLine(cell,target.Position,p.Map))continue;
    best=d;landing=cell;
   }
   return landing.IsValid;
  }
  public static bool Strike(Pawn source,Pawn victim,float factor){
   if(!WindCombat.Eligible(source,victim)||WaterDefense.TryParry(source,victim))return false;
   var profile=LightningCombat.Profile(source,10f);float damage=profile.damage*factor*ElementalTeaching.DamageFactor(source);
   var c=HediffComp_EarthAbilities.On(source);if(c==null||!c.CanAct)return false;c.MarkCombat();
   if(ElementalNonLethal.TryApply(source,victim,damage))return true;
   return victim.TakeDamage(new DamageInfo(DamageDefOf.Blunt,damage,Math.Max(.35f,profile.penetration),-1f,source,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,victim)).totalDamageDealt>0;
  }
  public static List<Pawn> Targets(Pawn source,IntVec3 center,float radius){
   var list=new List<Pawn>();int searched=0;
   foreach(var cell in GenRadial.RadialCellsAround(center,radius,true)){
    if(!cell.InBounds(source.Map)||!LightningCombat.ClearLine(center,cell,source.Map))continue;
    var things=cell.GetThingList(source.Map);
    for(int i=0;i<things.Count&&searched++<512;i++){var p=things[i] as Pawn;if(!WindCombat.Eligible(source,p)||list.Contains(p))continue;list.Add(p);if(list.Count==EarthRules.VictimLimit)return list;}
    if(searched>=512)break;
   }
   return list;
  }
  public static void Cleave(Pawn source,Pawn target){
   var victims=new List<Pawn>();var direction=(target.Position-source.Position).ToVector3().normalized;
   var end=(source.Position.ToVector3Shifted()+direction*8f).ToIntVec3();
   foreach(var cell in LightningGeometry.Trace(source.Position,end)){
    if(!cell.InBounds(source.Map)||!LightningCombat.Open(cell,source.Map)||!LightningCombat.ClearLine(source.Position,cell,source.Map))break;
    var things=cell.GetThingList(source.Map);
    for(int i=0;i<things.Count;i++){var p=things[i] as Pawn;if(WindCombat.Eligible(source,p)&&!victims.Contains(p)){victims.Add(p);if(victims.Count>=EarthRules.VictimLimit)break;}}
    if(victims.Count>=EarthRules.VictimLimit)break;
   }
   bool earned=false;
   foreach(var victim in victims){bool hostile=WindCombat.Hostile(source,victim);if(Strike(source,victim,2.6f)){earned|=hostile;if(victim.Spawned&&!victim.Dead&&!victim.Downed)victim.stances.stagger.StaggerFor(30,.85f);}}
   var earth=HediffComp_EarthAbilities.On(source);if(earned&&earth!=null)earth.GainResolve();
   EarthVfx.Emit(source,0,source.DrawPos,target.DrawPos,18,3.5f,(target.DrawPos-source.DrawPos).AngleFlat()-90f);
   ElementalAudio.Ability(source,ElementalSound.Slash);
  }
  public static void Push(Pawn victim,IntVec3 center,int cells){
   if(victim==null||!victim.Spawned||victim.Dead||victim.Downed||Anchored(victim)||GlacialFreeze.Active(victim)||CycloneAirbound.Active(victim)||ElementalAnimationBridge.Controlled(victim))return;
   var start=victim.Position;var direction=(start-center).ToVector3().normalized;var end=start;
   for(int i=1;i<=cells;i++){var cell=(start.ToVector3Shifted()+direction*i).ToIntVec3();if(cell==end)continue;if(!SafeStep(victim,end,cell))break;end=cell;}
   if(end!=start){victim.pather.StopDead();victim.Position=end;victim.Notify_Teleported(false,true);}
  }
  public static void Impact(Pawn source,Pawn primary,int stacks){
   var center=primary.Position;var targets=Targets(source,center,3f);
   EarthVfx.Emit(source,3,center.ToVector3Shifted(),center.ToVector3Shifted(),30,7f,0);
   foreach(var victim in targets){Strike(source,victim,4f*EarthRules.Impact(stacks));Push(victim,center==victim.Position?source.Position:center,2);}
   ElementalAudio.Ability(source,ElementalSound.Impact);
  }
  public static bool ThreatenedAlly(Pawn p){
   foreach(var cell in GenRadial.RadialCellsAround(p.Position,3,true)){if(!cell.InBounds(p.Map))continue;var things=cell.GetThingList(p.Map);for(int i=0;i<things.Count;i++){var ally=things[i] as Pawn;if(ally==null||ally==p||ally.Faction==null||ally.Faction!=p.Faction||ally.Dead||ally.Downed||!LightningCombat.ClearLine(p.Position,ally.Position,p.Map))continue;if(ElementalCombatFlow.CountHostiles(ally,1.5f,1)>0)return true;}}
   return false;
  }
 }
 public sealed class JobDriver_EarthTechnique:JobDriver {
  int startedAt=-1,nextStep,routeIndex,impactAt,finishAt,stacks;
  bool impactCommitted;
  IntVec3 expected=IntVec3.Invalid,center=IntVec3.Invalid;
  List<IntVec3> route=new List<IntVec3>();
  HediffComp_EarthAbilities Comp{get{return HediffComp_EarthAbilities.On(pawn);}}
  EarthPower Power{get{return (EarthPower)job.count;}}
  public bool PendingMountainfall{get{return startedAt>=0&&!impactCommitted&&Power==EarthPower.Mountainfall;}}
  public Pawn TelegraphTarget{get{return TargetA.Pawn;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref startedAt,"EH_earthStarted",-1);Scribe_Values.Look(ref nextStep,"EH_earthNextStep");Scribe_Values.Look(ref routeIndex,"EH_earthRouteIndex");Scribe_Values.Look(ref impactAt,"EH_earthImpactAt");Scribe_Values.Look(ref finishAt,"EH_earthFinishAt");Scribe_Values.Look(ref stacks,"EH_earthImpactResolve");Scribe_Values.Look(ref impactCommitted,"EH_earthCommitted");Scribe_Values.Look(ref expected,"EH_earthExpected");Scribe_Values.Look(ref center,"EH_earthCenter");Scribe_Collections.Look(ref route,"EH_earthRoute",LookMode.Value);if(route==null)route=new List<IntVec3>();}
  bool EnsureTarget(float range){
   if(WindCombat.Eligible(pawn,TargetA.Pawn)&&pawn.Position.DistanceToSquared(TargetA.Cell)<=range*range&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map))return true;
   if(job.playerForced||!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(pawn),ElementalHeartsMod.Settings.enableAutomation,Comp.autoCombat)||pawn.jobs.jobQueue.Count>1)return false;
   var next=ElementalCombatFlow.Nearest(pawn,range,TargetA.Pawn);if(next==null)return false;job.targetA=next;return true;
  }
  void Start(){
   if(startedAt>=0)return;var c=Comp;if(c==null||!c.CanAct||Power==EarthPower.Bastion||job.count<0||job.count>3){EndJobWith(JobCondition.Incompletable);return;}
   int now=Find.TickManager.TicksGame;startedAt=now;expected=pawn.Position;pawn.pather.StopDead();pawn.rotationTracker.FaceTarget(TargetA);
   int windup=Power==EarthPower.Mountainfall?42:6;impactAt=now+windup;finishAt=impactAt+8;
   if(Power==EarthPower.Advance){
    IntVec3 landing;if(!EnsureTarget(6f)||!EarthCombat.FindLanding(pawn,TargetA.Pawn,out landing)){EndJobWith(JobCondition.Incompletable);return;}
    route=WindNavigation.Path(WindNavigation.Tree(pawn.Position,(a,b)=>EarthCombat.SafeStep(pawn,a,b)),pawn.Position,landing);
    if(route==null){EndJobWith(JobCondition.Incompletable);return;}routeIndex=0;nextStep=now+2;impactAt=now+Math.Max(2,route.Count*2+2);finishAt=impactAt+8;
    EarthVfx.Emit(pawn,1,pawn.DrawPos,landing.ToVector3Shifted(),Math.Max(12,route.Count*2),2.5f,(landing-pawn.Position).ToVector3().AngleFlat()-90f);ElementalAudio.Ability(pawn,ElementalSound.Dash);
   }
   if(Power==EarthPower.Mountainfall){
    center=TargetA.Cell;
    ElementalAudio.Ability(pawn,ElementalSound.Charge);
   }
  }
  void Tick(){
   var c=Comp;if(c==null||!c.CanAct||pawn.Position!=expected){EndJobWith(JobCondition.Incompletable);return;}
   int now=Find.TickManager.TicksGame;
   if(!impactCommitted&&Power==EarthPower.Advance&&now>=nextStep&&routeIndex<route.Count){
    var cell=route[routeIndex];if(!EarthCombat.SafeStep(pawn,pawn.Position,cell)){EndJobWith(JobCondition.Incompletable);return;}
    pawn.Position=cell;pawn.Notify_Teleported(false,true);expected=cell;routeIndex++;nextStep=now+2;
   }
   if(!impactCommitted&&now>=impactAt){
    float range=Power==EarthPower.Advance?1.5f:8f;
    if(!EnsureTarget(range)){EndJobWith(JobCondition.Incompletable);return;}
    // Commit before any hit, so nested damage/death/save callbacks cannot replay an impact.
    impactCommitted=true;
    if(Power==EarthPower.Cleave)EarthCombat.Cleave(pawn,TargetA.Pawn);
    else if(Power==EarthPower.Advance){EarthCombat.Strike(pawn,TargetA.Pawn,3f);ElementalAudio.Ability(pawn,ElementalSound.Slash);}
    else{stacks=c.Resolve;c.resolve=0;c.resolveUntil=0;EarthCombat.Impact(pawn,TargetA.Pawn,stacks);}
    c.nextAutoCheck=now+8;
   }
   if(now>=finishAt){c.nextAutoCheck=now+1;ReadyForNextToil();}
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   this.FailOn(()=>Comp==null||!Comp.CanAct);
   var action=new Toil{defaultCompleteMode=ToilCompleteMode.Never,handlingFacing=true};action.initAction=Start;action.tickAction=Tick;
   yield return action;
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class EarthDamageProtection {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){
   var p=__instance as Pawn;if(p==null||!p.Spawned||p.Dead||dinfo.Amount<=0||dinfo.Def==null||!dinfo.Def.ExternalViolenceFor(p))return;
   var c=HediffComp_EarthAbilities.On(p);float personal=c==null?1f:EarthRules.Defense(c.Resolve,false,1f);
   float allied=p.Map.GetComponent<MapComponent_Earth>().AlliedProtection(p);
   // An Earth caster's own stance and a teammate's aura never double-count Bastion.
   if(c!=null&&c.GuardActive)allied=Math.Min(allied,1f-.20f*ElementalTeaching.DamageFactor(p));
   dinfo.SetAmount(dinfo.Amount*personal*allied);
  }
 }
}

