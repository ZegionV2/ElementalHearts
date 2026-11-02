using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class TempestDragonRules {
  public const int MaxTargets=5,CandidateLimit=12,Frames=8,TrailLifetime=24;
  public const float FirstRange=10,ChainRange=8;
  public static int Duration(float length){return Math.Max(4,Math.Min(10,(int)Math.Ceiling(length*.9f)+2));}
  public static int Frame(int age){return Math.Max(0,Math.Min(7,age/3));}
  public static bool CanVisit(int count,bool eligible,bool visited){return count<MaxTargets&&eligible&&!visited;}
  // Each route is two straight segments. Alternate the preferred bend side for each victim.
  public static bool Route(IntVec3 start,IntVec3 target,float range,int visit,Func<IntVec3,bool> free,Func<IntVec3,IntVec3,bool> path,out IntVec3 bend,out IntVec3 end){
   bend=end=IntVec3.Invalid;if(start.DistanceToSquared(target)>range*range)return false;var direction=(target-start).ToVector3();if(direction.sqrMagnitude<.1f)direction=Vector3.forward;direction.Normalize();float best=float.MinValue;
   for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){
    if(x==0&&z==0)continue;var landing=target+new IntVec3(x,0,z);if(!free(landing)||!path(landing,target))continue;
    for(int sideIndex=0;sideIndex<2;sideIndex++)for(int offset=2;offset>=1;offset--){
     int side=(visit%2==0?1:-1)*(sideIndex==0?1:-1);var turn=new IntVec3(Mathf.RoundToInt((start.x+landing.x)*.5f-direction.z*offset*side),0,Mathf.RoundToInt((start.z+landing.z)*.5f+direction.x*offset*side));
     if(turn==start||turn==landing||turn==target||!free(turn)||!path(start,turn)||!path(turn,landing))continue;
     var a=(turn-start).ToVector3();var b=(landing-turn).ToVector3();if(Math.Abs(a.x*b.z-a.z*b.x)<.5f)continue;
     float score=(x*direction.x+z*direction.z)*4+(sideIndex==0?2:0)+offset*.2f-(a.magnitude+b.magnitude)*.03f;
     if(score>best){best=score;bend=turn;end=landing;}
    }
   }return end.IsValid;
  }
 }
 public static class TempestDragon {
  public const string JobName="EH_TempestDragon";
  public static bool Busy(Pawn p){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&p.CurJob.def.defName==JobName;}
  public static bool Enemy(Pawn p,Pawn target){return LightningCombat.Eligible(p,target)&&target.HostileTo(p)&&!target.IsPrisoner&&!target.Position.Fogged(p.Map);}
  public static bool Plan(Pawn p,Pawn target,float range,int visit,out IntVec3 bend,out IntVec3 end){bend=end=IntVec3.Invalid;return Enemy(p,target)&&TempestDragonRules.Route(p.Position,target.Position,range,visit,c=>ThunderstormBlitz.Landing(p,c),(a,b)=>ThunderstormBlitz.Path(p,a,b),out bend,out end);}
  public static bool TryStart(Pawn p,Pawn target,bool automatic){
   var c=HediffComp_LightningAbilities.On(p);if(c==null||!c.CanAct||!ThunderstormEvolution.Active(p)||Busy(p))return false;IntVec3 bend,end;if(!Plan(p,target,TempestDragonRules.FirstRange,0,out bend,out end))return false;
   var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(JobName),target,bend,end);job.playerForced=!automatic;p.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);return p.CurJob==job;
  }
 }
 public sealed class JobDriver_TempestDragon:JobDriver {
  IntVec3 start;int elapsed,duration,leg;bool initialized,moving;HashSet<int> visited=new HashSet<int>(),struck=new HashSet<int>();int trail=-1;
  Pawn Target{get{return job.targetA.Pawn;}}
  IntVec3 Destination{get{return leg==0?job.targetB.Cell:job.targetC.Cell;}}
  public bool Moving{get{return moving;}}
  public Vector3 VisualAt(int tick){return Vector3.Lerp(start.ToVector3Shifted(),Destination.ToVector3Shifted(),StormBlitzRules.Progress(tick,duration));}
  public int Elapsed{get{return elapsed;}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref start,"EH_dragonStart");Scribe_Values.Look(ref elapsed,"EH_dragonElapsed");Scribe_Values.Look(ref duration,"EH_dragonDuration");Scribe_Values.Look(ref leg,"EH_dragonLeg");Scribe_Values.Look(ref initialized,"EH_dragonInitialized");Scribe_Values.Look(ref moving,"EH_dragonMoving");Scribe_Collections.Look(ref visited,"EH_dragonVisited",LookMode.Value);Scribe_Collections.Look(ref struck,"EH_dragonStruck",LookMode.Value);if(visited==null)visited=new HashSet<int>();if(struck==null)struck=new HashSet<int>();}
  bool Valid(){return pawn.Spawned&&!pawn.Dead&&!pawn.Downed&&pawn.Awake()&&!pawn.InMentalState&&!pawn.WorkTagIsDisabled(WorkTags.Violent)&&!pawn.stances.stunner.Stunned&&(!ElementalCombatPolicy.Controlled(pawn)||pawn.Drafted)&&ThunderstormEvolution.Active(pawn)&&!ElementalAnimationBridge.ExternallyControlled(pawn);}
  void BeginLeg(){ElementalAudio.Ability(pawn,ElementalSound.Dash);start=pawn.Position;elapsed=0;duration=TempestDragonRules.Duration((Destination-start).LengthHorizontal);trail=-1;moving=true;pawn.Rotation=Rot4.FromAngleFlat((Destination-start).ToVector3().AngleFlat());}
  void Settle(){moving=false;if(!pawn.Spawned||pawn.Dead||pawn.Downed||ElementalAnimationBridge.ExternallyControlled(pawn)||ThunderstormBlitz.Landing(pawn,pawn.Position))return;foreach(var cell in GenRadial.RadialCellsAround(pawn.Position,1.5f,false))if(ThunderstormBlitz.Landing(pawn,cell)&&ThunderstormBlitz.Path(pawn,pawn.Position,cell)){ThunderstormBlitz.Move(pawn,cell);return;}if(initialized&&ThunderstormBlitz.Landing(pawn,start)&&ThunderstormBlitz.Path(pawn,pawn.Position,start))ThunderstormBlitz.Move(pawn,start);}
  bool Next(){
   if(visited.Count>=TempestDragonRules.MaxTargets)return false;
   var candidates=LightningCombat.Targets(pawn,pawn.Position,TempestDragonRules.ChainRange);candidates.Sort((a,b)=>{int order=pawn.Position.DistanceToSquared(a.Position).CompareTo(pawn.Position.DistanceToSquared(b.Position));return order!=0?order:a.thingIDNumber.CompareTo(b.thingIDNumber);});
   int checkedRoutes=0;foreach(var target in candidates){if(!TempestDragonRules.CanVisit(visited.Count,TempestDragon.Enemy(pawn,target),visited.Contains(target.thingIDNumber)))continue;if(checkedRoutes++>=TempestDragonRules.CandidateLimit)break;IntVec3 bend,end;if(!TempestDragon.Plan(pawn,target,TempestDragonRules.ChainRange,visited.Count,out bend,out end))continue;job.targetA=target;job.targetB=bend;job.targetC=end;visited.Add(target.thingIDNumber);leg=0;BeginLeg();return true;}return false;
  }
  void Hit(){
   var target=Target;if(!TempestDragon.Enemy(pawn,target)||pawn.Position.DistanceToSquared(target.Position)>2||!LightningCombat.ClearLine(pawn.Position,target.Position,pawn.Map)||!struck.Add(target.thingIDNumber))return;
   pawn.Rotation=Rot4.FromAngleFlat((target.Position-pawn.Position).ToVector3().AngleFlat());
   // Record commitment before damage, including parried/non-lethal attempts.
   bool hit=LightningCombat.Strike(pawn,target,LightningCombat.Profile(pawn,10),1,1.4f,true);if(hit&&!target.Dead&&!target.Destroyed)LightningCharge.Apply(pawn,target);StormBlitzFx.Impact(pawn,target.Position.ToVector3Shifted());ElementalAudio.Ability(pawn,ElementalSound.Slash);
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   AddFinishAction(condition=>Settle());this.FailOn(()=>!Valid());var chain=new Toil();chain.defaultCompleteMode=ToilCompleteMode.Never;chain.handlingFacing=true;
   chain.initAction=()=>{pawn.stances.CancelBusyStanceHard();pawn.pather.StopDead();if(!initialized){if(Target==null){EndJobWith(JobCondition.Incompletable);return;}visited.Add(Target.thingIDNumber);initialized=true;leg=0;BeginLeg();}else moving=true;StormBlitzFx.Launch(pawn,pawn.Position.ToVector3Shifted());};
   chain.tickAction=()=>{
    pawn.pather.StopDead();if(!ThunderstormBlitz.Landing(pawn,Destination)){EndJobWith(JobCondition.Incompletable);return;}
    int next=Math.Min(duration,elapsed+1);var from=VisualAt(elapsed);var to=VisualAt(next);if(!ThunderstormBlitz.Path(pawn,pawn.Position,to.ToIntVec3())){EndJobWith(JobCondition.Incompletable);return;}
    TempestDragonVfx.Trace(pawn,ref trail,start.ToVector3Shifted(),to);if(elapsed%2==0&&ElementalHeartsMod.Settings.showAfterimages)StormBlitzFx.Echo(pawn,from);ThunderstormBlitz.Move(pawn,to.ToIntVec3());elapsed=next;
    if(elapsed<duration)return;ThunderstormBlitz.Move(pawn,Destination);if(leg==0){leg=1;BeginLeg();return;}moving=false;Hit();if(!Valid()){EndJobWith(JobCondition.Incompletable);return;}if(Next())return;StormBlitzFx.Arrive(pawn,pawn.Position.ToVector3Shifted());ReadyForNextToil();
   };yield return chain;var recovery=Toils_General.Wait(6);recovery.handlingFacing=true;yield return recovery;
  }
 }
 [HarmonyPatch(typeof(Pawn_DrawTracker),"get_DrawPos")]
 public static class TempestDragonDrawPosition {
  public static void Postfix(Pawn ___pawn,ref Vector3 __result){if(!TempestDragon.Busy(___pawn))return;var driver=___pawn.jobs.curDriver as JobDriver_TempestDragon;if(driver==null||!driver.Moving)return;var pos=driver.VisualAt(driver.Elapsed);pos.y=__result.y;__result=pos;}
 }
}
