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
  public static bool FindLanding(Pawn p,Pawn target,out IntVec3 landing){List<IntVec3> route;return FindLanding(p,target,out landing,out route);}
  public static bool FindLanding(Pawn p,Pawn target,out IntVec3 landing,out List<IntVec3> route){
   route=null;
   landing=IntVec3.Invalid;if(!WindCombat.AliveTarget(p,target)||p.Position.DistanceToSquared(target.Position)<=2)return false;
   // Single bounded navigation tree; no obstacle, pawn or diagonal corner bypass.
   var tree=WindNavigation.Tree(p.Position,(a,b)=>SafeStep(p,a,b),AdamantEvolution.Active(p)?64:36,AdamantEvolution.Active(p)?197:113);int best=int.MaxValue;
   foreach(var cell in GenAdj.CellsAdjacent8Way(target)){
    int d=p.Position.DistanceToSquared(cell);if(d>(AdamantEvolution.Active(p)?64:36)||d>=best||!tree.ContainsKey(cell)||!LightningCombat.ClearLine(cell,target.Position,p.Map))continue;
    best=d;landing=cell;
   }
   if(!landing.IsValid)return false;route=WindNavigation.Path(tree,p.Position,landing,AdamantEvolution.Active(p)?16:12);return route!=null;
  }
  public static bool Strike(Pawn source,Pawn victim,float factor){
   if(!WindCombat.Eligible(source,victim)||WaterDefense.TryParry(source,victim))return false;
   var profile=LightningCombat.Profile(source,10f);float damage=profile.damage*factor*ElementalTeaching.DamageFactor(source);
   var c=HediffComp_EarthAbilities.On(source);if(c==null||!c.CanAct)return false;c.MarkCombat();
   if(ElementalNonLethal.TryApply(source,victim,damage))return true;
   var result=victim.TakeDamage(new DamageInfo(DamageDefOf.Blunt,damage,Math.Max(c.Evolved?.55f:.35f,profile.penetration),-1f,source,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,victim));return result!=null&&result.totalDamageDealt>0;
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
   int inspected=0;bool evolved=AdamantEvolution.Active(source);var victims=new List<Pawn>();var direction=(target.Position-source.Position).ToVector3().normalized;
   var end=(source.Position.ToVector3Shifted()+direction*(AdamantEvolution.Active(source)?10f:8f)).ToIntVec3();
   foreach(var cell in LightningGeometry.Trace(source.Position,end)){
    if(!cell.InBounds(source.Map)||!LightningCombat.Open(cell,source.Map)||!LightningCombat.ClearLine(source.Position,cell,source.Map))break;
    var things=cell.GetThingList(source.Map);
    for(int i=0;i<things.Count&&inspected++<512;i++){var p=things[i] as Pawn;if(WindCombat.Eligible(source,p)&&!victims.Contains(p)){victims.Add(p);if(victims.Count>=EarthRules.VictimLimit)break;}}
    if(victims.Count>=EarthRules.VictimLimit||inspected>=512)break;
   }
   bool earned=false;
   foreach(var victim in victims){bool hostile=WindCombat.Hostile(source,victim);if(Strike(source,victim,evolved?3.2f:2.6f)){earned|=hostile;if(victim.Spawned&&!victim.Dead&&!victim.Downed)victim.stances.stagger.StaggerFor(30,.85f);}}
   var earth=HediffComp_EarthAbilities.On(source);if(earned&&earth!=null)earth.GainResolve();
   EarthVfx.Emit(source,0,source.DrawPos,target.DrawPos,18,3.5f,(target.DrawPos-source.DrawPos).AngleFlat()-90f);
   ElementalAudio.Ability(source,ElementalSound.Slash);
  }
  public static void Push(Pawn victim,IntVec3 center,int cells){
   if(victim==null||!victim.Spawned||victim.Dead||victim.Downed||Anchored(victim)||GlacialFreeze.Active(victim)||CycloneAirbound.Active(victim)||ElementalAnimationBridge.Controlled(victim))return;
   var start=victim.Position;var direction=(start-center).ToVector3().normalized;var end=start;
   for(int i=1;i<=cells;i++){var cell=(start.ToVector3Shifted()+direction*i).ToIntVec3();if(cell==end)continue;if(!SafeStep(victim,end,cell))break;end=cell;}
   if(end!=start){victim.pather.StopDead();victim.Position=end;victim.Notify_Teleported(false,true);victim.Map.GetComponent<MapComponent_EarthPushVisual>().Track(victim,start,end);}
  }
  public static void Impact(Pawn source,Pawn primary,int stacks){
   Eruption(source,primary.Position,stacks);
  }
  public static void Eruption(Pawn source,IntVec3 center,int stacks){
   var targets=Targets(source,center,(AdamantEvolution.Active(source)?3.5f:EarthSeismicRules.SpireRadius));
   EarthVfx.Emit(source,7,center.ToVector3Shifted(),center.ToVector3Shifted(),42,AdamantEvolution.Active(source)?8.5f:6.5f);
   EarthVfx.Emit(source,8,center.ToVector3Shifted(),center.ToVector3Shifted(),26,9f);
   foreach(var victim in targets){if(!Strike(source,victim,(AdamantEvolution.Active(source)?5.6f*(1+.15f*Math.Min(3,stacks)):4f*EarthRules.Impact(stacks))))continue;Push(victim,center==victim.Position?source.Position:center,AdamantEvolution.Active(source)?4:3);if(WindCombat.AliveTarget(source,victim))victim.stances.stagger.StaggerFor(AdamantEvolution.Active(source)?75:45,.65f);}
   ElementalAudio.Play(source,(AdamantEvolution.Active(source)?"EH_Sound_Adamant_Spire_Impact":"EH_Sound_Earth_Spire_Impact"));
  }
  public static void Quake(Pawn source){
   var center=source.Position;var targets=Targets(source,center,(AdamantEvolution.Active(source)?5f:EarthSeismicRules.QuakeRadius));
   EarthVfx.Emit(source,8,source.DrawPos,source.DrawPos,32,AdamantEvolution.Active(source)?15f:12f);
   foreach(var victim in targets){if(!Strike(source,victim,AdamantEvolution.Active(source)?2.9f:2.2f))continue;Push(victim,center,AdamantEvolution.Active(source)?3:2);if(WindCombat.AliveTarget(source,victim))victim.stances.stagger.StaggerFor(AdamantEvolution.Active(source)?90:60,.65f);}
   ElementalAudio.Play(source,(AdamantEvolution.Active(source)?"EH_Sound_Adamant_Quake_Impact":"EH_Sound_Earth_Quake_Impact"));
  }
  public static int CrowdAt(Pawn source,IntVec3 center,float radius,int limit){
   int count=0,checkedThings=0;
   foreach(var cell in GenRadial.RadialCellsAround(center,radius,true)){
    if(!cell.InBounds(source.Map)||!LightningCombat.ClearLine(center,cell,source.Map))continue;
    var things=cell.GetThingList(source.Map);for(int i=0;i<things.Count;i++){
     if(++checkedThings>512)return count;
     if(WindCombat.Hostile(source,things[i] as Pawn)&&++count>=limit)return count;
    }
   }
   return count;
  }
  public static bool ThreatenedAlly(Pawn p){
   foreach(var cell in GenRadial.RadialCellsAround(p.Position,3,true)){if(!cell.InBounds(p.Map))continue;var things=cell.GetThingList(p.Map);for(int i=0;i<things.Count;i++){var ally=things[i] as Pawn;if(ally==null||ally==p||ally.Faction==null||ally.Faction!=p.Faction||ally.Dead||ally.Downed||!LightningCombat.ClearLine(p.Position,ally.Position,p.Map))continue;if(ElementalCombatFlow.CountHostiles(ally,1.5f,1)>0)return true;}}
   return false;
  }
 }
 public sealed class JobDriver_EarthTechnique:JobDriver {
  int startedAt=-1,nextStep,routeIndex,impactAt,finishAt,stacks,repaths,stepsTaken,stepAt;
  bool impactCommitted;
  IntVec3 expected=IntVec3.Invalid,center=IntVec3.Invalid,chargeOrigin=IntVec3.Invalid,stepFrom=IntVec3.Invalid;
  List<IntVec3> route=new List<IntVec3>();
  HediffComp_EarthAbilities Comp{get{return HediffComp_EarthAbilities.On(pawn);}}
  EarthPower Power{get{return (EarthPower)job.count;}}
  public bool PendingMountainfall{get{return startedAt>=0&&!impactCommitted&&Power==EarthPower.Mountainfall;}}
  public bool GroundStab{get{return startedAt>=0&&EarthSeismicRules.GroundStab(Power)&&!pawn.Dead&&!pawn.Downed;}}
  public bool PendingQuake{get{return startedAt>=0&&!impactCommitted&&Power==EarthPower.Bastion;}}
  public IntVec3 TelegraphCell{get{return Power==EarthPower.Bastion?pawn.Position:center;}}
  public float StabPhase{get{return EarthSeismicRules.StabProgress(Find.TickManager.TicksGame,startedAt,impactAt);}}
  public bool PendingCleave{get{return startedAt>=0&&!impactCommitted&&Power==EarthPower.Cleave;}}
  public float CleaveWindup{get{return EarthRules.Smooth((Find.TickManager.TicksGame-startedAt)/6f);}}
  public Pawn TelegraphTarget{get{return TargetA.Pawn;}}
  public float TelegraphPhase{get{return EarthRules.Smooth((Find.TickManager.TicksGame-startedAt)/(float)EarthSeismicRules.Windup(Power));}}
  public bool Moving{get{return Power==EarthPower.Advance&&!impactCommitted&&stepFrom.IsValid&&pawn.Position==expected&&Find.TickManager.TicksGame<stepAt+2;}}
  public Vector3 VisualAt(int now){return Vector3.Lerp(stepFrom.ToVector3Shifted(),expected.ToVector3Shifted(),Mathf.Clamp01((now-stepAt)/2f));}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref startedAt,"EH_earthStarted",-1);Scribe_Values.Look(ref nextStep,"EH_earthNextStep");Scribe_Values.Look(ref routeIndex,"EH_earthRouteIndex");Scribe_Values.Look(ref impactAt,"EH_earthImpactAt");Scribe_Values.Look(ref finishAt,"EH_earthFinishAt");Scribe_Values.Look(ref stacks,"EH_earthImpactResolve");Scribe_Values.Look(ref impactCommitted,"EH_earthCommitted");Scribe_Values.Look(ref expected,"EH_earthExpected");Scribe_Values.Look(ref center,"EH_earthCenter");Scribe_Values.Look(ref repaths,"EH_earthRepaths");Scribe_Values.Look(ref stepsTaken,"EH_earthStepsTaken");Scribe_Values.Look(ref stepAt,"EH_earthStepAt");Scribe_Values.Look(ref chargeOrigin,"EH_earthChargeOrigin",IntVec3.Invalid);Scribe_Values.Look(ref stepFrom,"EH_earthStepFrom",IntVec3.Invalid);Scribe_Collections.Look(ref route,"EH_earthRoute",LookMode.Value);if(route==null)route=new List<IntVec3>();}
  bool EnsureTarget(float range){
   if(WindCombat.Eligible(pawn,TargetA.Pawn)&&pawn.Position.DistanceToSquared(TargetA.Cell)<=range*range&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map))return true;
   if(job.playerForced||!ElementalCombatPolicy.AutomationEnabled(ElementalCombatPolicy.Controlled(pawn),ElementalHeartsMod.Settings.enableAutomation,Comp.autoCombat)||pawn.jobs.jobQueue.Count>1)return false;
   var next=ElementalCombatFlow.Nearest(pawn,range,TargetA.Pawn);if(next==null)return false;job.targetA=next;return true;
  }
  void Start(){
   if(startedAt>=0)return;var c=Comp;if(c==null||!c.CanAct||job.count<0||job.count>=c.SkillCount){EndJobWith(JobCondition.Incompletable);return;}
   int now=Find.TickManager.TicksGame;startedAt=now;expected=pawn.Position;pawn.pather.StopDead();if(Power!=EarthPower.Bastion&&Power!=EarthPower.Bulwark)pawn.rotationTracker.FaceTarget(TargetA);else pawn.Rotation=Rot4.South;
   int windup=Power==EarthPower.Bulwark?18:EarthSeismicRules.Windup(Power);impactAt=now+windup;finishAt=impactAt+8;
   if(Power==EarthPower.Advance){
    IntVec3 landing;if(!EnsureTarget(Comp.Range(EarthPower.Advance))||!EarthCombat.FindLanding(pawn,TargetA.Pawn,out landing,out route)){EndJobWith(JobCondition.Incompletable);return;}
    chargeOrigin=pawn.Position;
    if(route==null){EndJobWith(JobCondition.Incompletable);return;}routeIndex=0;nextStep=now+2;impactAt=now+Math.Max(2,route.Count*2+2);finishAt=impactAt+8;
    EarthVfx.Emit(pawn,1,pawn.DrawPos,landing.ToVector3Shifted(),Math.Max(12,route.Count*2),2.5f,(landing-pawn.Position).ToVector3().AngleFlat()-90f);ElementalAudio.Ability(pawn,ElementalSound.Dash);
   }
   if(EarthSeismicRules.GroundStab(Power)){
    center=Power==EarthPower.Bastion?pawn.Position:TargetA.Cell;
    ElementalAudio.Ability(pawn,ElementalSound.Charge);
   }
  }
  bool Repath(){
   if(repaths>=1||!EnsureTarget(Comp.Range(EarthPower.Advance)))return false;
   IntVec3 landing;List<IntVec3> fresh;
   if(!EarthCombat.FindLanding(pawn,TargetA.Pawn,out landing,out fresh))return false;
   var origin=chargeOrigin.IsValid?chargeOrigin:pawn.Position;
   if(!(Comp.Evolved?AdamantRules.RepathAllowed(repaths,Math.Max(stepsTaken,routeIndex),fresh.Count,origin.DistanceToSquared(landing)):EarthRules.RepathAllowed(repaths,Math.Max(stepsTaken,routeIndex),fresh.Count,origin.DistanceToSquared(landing))))return false;
   repaths++;stepsTaken=Math.Max(stepsTaken,routeIndex);route=fresh;routeIndex=0;int now=Find.TickManager.TicksGame;nextStep=now+2;impactAt=now+fresh.Count*2+2;finishAt=impactAt+8;
   return true;
  }
  void Tick(){
   var c=Comp;if(c==null||!c.CanAct||pawn.Position!=expected){EndJobWith(JobCondition.Incompletable);return;}
   int now=Find.TickManager.TicksGame;
   if(!impactCommitted&&Power==EarthPower.Advance&&routeIndex<route.Count&&WindCombat.Eligible(pawn,TargetA.Pawn)&&pawn.Position.DistanceToSquared(TargetA.Cell)<=2&&LightningCombat.ClearLine(pawn.Position,TargetA.Cell,pawn.Map)){
    routeIndex=route.Count;impactAt=Math.Min(impactAt,now+2);finishAt=impactAt+8;
   }
   if(!impactCommitted&&Power==EarthPower.Advance&&now>=nextStep&&routeIndex<route.Count){
    var cell=route[routeIndex];if(!EarthCombat.SafeStep(pawn,pawn.Position,cell)){if(!Repath())EndJobWith(JobCondition.Incompletable);return;}
    stepFrom=pawn.Position;stepAt=now;pawn.Rotation=Rot4.FromAngleFlat((cell-pawn.Position).ToVector3().AngleFlat());pawn.Position=cell;pawn.Notify_Teleported(false,true);expected=cell;routeIndex++;stepsTaken++;nextStep=now+2;
   }
   if(!impactCommitted&&now>=impactAt){
    if(Power==EarthPower.Mountainfall){
     if(!center.IsValid||!center.InBounds(pawn.Map)||pawn.Position.DistanceToSquared(center)>c.Range(Power)*c.Range(Power)||!LightningCombat.ClearLine(pawn.Position,center,pawn.Map)){EndJobWith(JobCondition.Incompletable);return;}
    }else if(Power==EarthPower.Bulwark){if(!c.Valid(Power,null,true)){EndJobWith(JobCondition.Incompletable);return;}}else if(Power!=EarthPower.Bastion){
     float range=Power==EarthPower.Advance?1.5f:c.Range(Power);
     if(!EnsureTarget(range)){if(Power!=EarthPower.Advance||!Repath())EndJobWith(JobCondition.Incompletable);return;}
    }
    // Commit before any hit, so nested damage/death/save callbacks cannot replay an impact.
    impactCommitted=true;
    using(new ElementalManualTarget(pawn,job.playerForced&&Power!=EarthPower.Bastion&&Power!=EarthPower.Bulwark?TargetA.Pawn:null)){
    if(Power==EarthPower.Cleave)EarthCombat.Cleave(pawn,TargetA.Pawn);
    else if(Power==EarthPower.Advance){EarthCombat.Strike(pawn,TargetA.Pawn,c.Evolved?4f:3f);ElementalAudio.Play(pawn,(c.Evolved?"EH_Sound_Adamant_Advance_Impact":"EH_Sound_Earth_Advance_Impact"));}
    else if(Power==EarthPower.Bastion)EarthCombat.Quake(pawn);
    else if(Power==EarthPower.Bulwark)c.BeginGuard();
    else{stacks=c.Resolve;c.resolve=0;c.resolveUntil=0;EarthCombat.Eruption(pawn,center,stacks);}
    }
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
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class EarthChargeDraw {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){
   var p=___pawn;var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_EarthTechnique;
   if(d==null||!d.Moving||!p.Spawned||p.Dead||p.Downed||p.stances.stunner.Stunned||ElementalAnimationBridge.ExternallyControlled(p))return;
   var at=d.VisualAt(Find.TickManager.TicksGame);at.y=__result.y;__result=at;
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class EarthDamageProtection {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){
   var p=__instance as Pawn;if(p==null||!p.Spawned||p.Dead||dinfo.Amount<=0||dinfo.Def==null||!dinfo.Def.ExternalViolenceFor(p))return;
   var c=HediffComp_EarthAbilities.On(p);float personal=c==null?1f:c.Evolved?1f-.07f*c.Resolve:EarthRules.Defense(c.Resolve,false,1f);
   float allied=p.Map.GetComponent<MapComponent_Earth>().AlliedProtection(p);
   // An Earth caster's own stance and a teammate's aura never double-count Bastion.
   if(c!=null&&c.GuardActive)allied=Math.Min(allied,1f-(c.Evolved?.40f:.20f)*ElementalTeaching.DamageFactor(p));
   dinfo.SetAmount(dinfo.Amount*personal*allied);
  }
 }
}
