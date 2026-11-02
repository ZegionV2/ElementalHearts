using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ElementalHearts {
 public enum WindPower { Draw, Rise, Counter, Tempest }
 [DefOf] public static class WindDefs {
  public static HediffDef EH_WindHeart, EH_WindFlow, EH_WindGuard;
  public static JobDef EH_WindTechnique;
  static WindDefs(){DefOfHelper.EnsureInitializedInCtor(typeof(WindDefs));}
 }
 public static class WindRhythm {
  public static int NextStacks(int previous,int next,int stacks,bool expired){return expired||previous<0||previous==next?0:Math.Min(3,stacks+1);}
  public static int Cooldown(int baseline,int stacks){return Math.Max(1,(int)Math.Round(baseline*(1f-.08f*Mathf.Clamp(stacks,0,3))));}
 }
 public sealed class HediffCompProperties_WindAbilities:HediffCompProperties {
  public float drawRange=10f,tempestRange=6f,guardRange=4f;
  public float drawDamage=2.8f,riseFirstDamage=1.6f,riseSecondDamage=1.8f,counterDamage=3.5f,counterSweepDamage=1.5f;
  public float tempestOpeningDamage=2.5f,tempestSecondDamage=2.5f,tempestFinishDamage=5f,armorPenetrationFloor=.5f;
  public int staggerTicks=60;
  public int drawCooldown=240,riseCooldown=300,counterCooldown=420,tempestCooldown=900,flowDuration=480,guardDuration=120;
  public HediffCompProperties_WindAbilities(){compClass=typeof(HediffComp_WindAbilities);}
 }
 public sealed class HediffComp_WindAbilities:HediffComp {
  public bool autoCombat,nonLethal;
  private List<int> readyAt=new List<int>{0,0,0,0};
  private HairDef previousHair;
  public int nextAutoCheck,nextRetreatCheck,flowStacks,lastPower=-1,flowUntil,activationUntil,combatUntil,guardUntil;
  public Pawn guardTarget;
  public bool counterTriggered;
  public HediffCompProperties_WindAbilities Props {get{return (HediffCompProperties_WindAbilities)props;}}
  public ElementalAutoMode CurrentAutoMode {get{return ElementalAutoModes.FromFlags(autoCombat||!ElementalCombatPolicy.Controlled(Pawn),nonLethal);}}
  public void CycleAutoMode(){var next=ElementalAutoModes.Next(CurrentAutoMode);autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;}
  public static HediffComp_WindAbilities On(Pawn pawn){if(pawn==null||pawn.health==null)return null;foreach(var h in pawn.health.hediffSet.hediffs){var hc=h as HediffWithComps;if(hc!=null){var c=hc.TryGetComp<HediffComp_WindAbilities>();if(c!=null)return c;}}return null;}
  public bool HasWeapon {get{return Pawn.equipment!=null&&Pawn.equipment.Primary!=null&&Pawn.equipment.Primary.def.IsMeleeWeapon;}}
  public bool CanAct {get{return !ElementalAnimationBridge.Controlled(Pawn) && !ElementalTeachingSession.Busy(Pawn) && !HediffComp_ElementalTransform.Transforming(Pawn)&&Pawn.Spawned&&!Pawn.Dead&&!Pawn.Downed&&Pawn.Awake()&&!Pawn.InMentalState&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&!Pawn.stances.stunner.Stunned&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&HasWeapon&&Pawn.health.hediffSet.hediffs.Contains(parent);}}
  public bool Busy {get{return SovereignCyclone.Busy(Pawn)||Pawn.CurJob!=null&&Pawn.CurJob.def==WindDefs.EH_WindTechnique;}}
  public int Remaining(WindPower p){return Math.Max(0,readyAt[(int)p]-Find.TickManager.TicksGame);}
  public int BaseCooldown(WindPower p){switch(p){case WindPower.Draw:return Props.drawCooldown;case WindPower.Rise:return Props.riseCooldown;case WindPower.Counter:return Props.counterCooldown;default:return Props.tempestCooldown;}}
  public float Range(WindPower p){return p==WindPower.Draw?Props.drawRange:p==WindPower.Rise?1.5f:p==WindPower.Counter?Props.guardRange:Props.tempestRange;}
  public override string CompLabelInBracketsExtra {get{return flowStacks>0?"Flow "+flowStacks+"/3":null;}}
  public void MarkCombat(){combatUntil=Find.TickManager.TicksGame+180;}
  private void Register(){if(Pawn.Spawned && ElementalTeaching.IsHeart(parent))Pawn.Map.GetComponent<MapComponent_WindVisuals>().Register(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){autoCombat=true;Register();}
  public override void Notify_Spawned(){Register();}
  public override void Notify_PawnPostApplyDamage(DamageInfo info,float totalDamageDealt){if(totalDamageDealt>0)MarkCombat();}
  public override void CompPostPostRemoved(){
   if(Pawn.Spawned && ElementalTeaching.IsHeart(parent))Pawn.Map.GetComponent<MapComponent_WindVisuals>().Forget(Pawn);
   if(previousHair!=null&&Pawn.story!=null&&Pawn.story.hairDef.defName=="EH_WindHair"){Pawn.story.hairDef=previousHair;Pawn.Drawer.renderer.SetAllGraphicsDirty();}
   RemoveStatus(WindDefs.EH_WindFlow);RemoveStatus(WindDefs.EH_WindGuard);guardTarget=null;
  }
  public override void CompExposeData(){
   Scribe_Collections.Look(ref readyAt,"EH_WindReadyAt",LookMode.Value);if(readyAt==null||readyAt.Count!=4)readyAt=new List<int>{0,0,0,0};
   Scribe_Values.Look(ref autoCombat,"EH_autoCombat",false);Scribe_Values.Look(ref nonLethal,"EH_nonLethal",false);Scribe_Defs.Look(ref previousHair,"EH_WindPreviousHair");
   Scribe_Values.Look(ref flowStacks,"flowStacks",0);Scribe_Values.Look(ref lastPower,"lastPower",-1);Scribe_Values.Look(ref flowUntil,"flowUntil",0);
   Scribe_Values.Look(ref activationUntil,"activationUntil",0);Scribe_Values.Look(ref combatUntil,"combatUntil",0);Scribe_Values.Look(ref guardUntil,"guardUntil",0);
   Scribe_Values.Look(ref nextAutoCheck,"nextAutoCheck",0);Scribe_Values.Look(ref nextRetreatCheck,"nextRetreatCheck",0);
   Scribe_References.Look(ref guardTarget,"guardTarget");Scribe_Values.Look(ref counterTriggered,"counterTriggered",false);
  }
  public override void CompPostTickInterval(ref float severityAdjustment,int delta){
   int now=Find.TickManager.TicksGame;
   if(flowUntil<=now&&lastPower>=0){flowStacks=0;lastPower=-1;RemoveStatus(WindDefs.EH_WindFlow);}
   if(!Pawn.Spawned||Pawn.Dead)return;
   if(now>=nextAutoCheck){nextAutoCheck=now+60;WindAutocast.Evaluate(this);}
  }
  private void RemoveStatus(HediffDef def){var h=Pawn.health.hediffSet.GetFirstHediffOfDef(def);if(h!=null)Pawn.health.RemoveHediff(h);}
  public void Status(HediffDef def,int ticks,float severity){
   var h=Pawn.health.hediffSet.GetFirstHediffOfDef(def);
   if(h==null){h=HediffMaker.MakeHediff(def,Pawn);h.Severity=severity;Pawn.health.AddHediff(h);}else h.Severity=severity;
   var disappears=h.TryGetComp<HediffComp_Disappears>();if(disappears!=null)disappears.ticksToDisappear=ticks;
  }
  public void Commit(WindPower power){
   int now=Find.TickManager.TicksGame;
   flowStacks=WindRhythm.NextStacks(lastPower,(int)power,flowStacks,flowUntil<=now);lastPower=(int)power;flowUntil=now+Props.flowDuration;
   readyAt[(int)power]=now+WindRhythm.Cooldown(BaseCooldown(power),flowStacks);
   if(flowStacks>0)Status(WindDefs.EH_WindFlow,Props.flowDuration,flowStacks);else RemoveStatus(WindDefs.EH_WindFlow);
   activationUntil=now+30;MarkCombat();
  }
  public void EndGuard(){guardTarget=null;counterTriggered=false;guardUntil=0;}
  public bool TriggerCounter(Pawn attacker){
   if(!CanAct||!Busy||Pawn.CurJob.count!=(int)WindPower.Counter||counterTriggered||Find.TickManager.TicksGame>=guardUntil||attacker!=guardTarget||!WindCombat.AliveTarget(Pawn,attacker)||Pawn.Position.DistanceToSquared(attacker.Position)>2)return false;
   counterTriggered=true;guardUntil=0;MarkCombat();return true;
  }
  public bool Valid(WindPower p,Pawn target,bool manual){
   if(!WindCombat.AliveTarget(Pawn,target)||Pawn.Position.DistanceToSquared(target.Position)>Range(p)*Range(p)||!LightningCombat.ClearLine(Pawn.Position,target.Position,Pawn.Map))return false;
   if(!manual&&!WindCombat.Hostile(Pawn,target))return false;
   IntVec3 landing;
   if(p==WindPower.Draw)return WindCombat.DrawLanding(Pawn,target,Props.drawRange,out landing);
   if(p==WindPower.Tempest)return Pawn.Position.DistanceToSquared(target.Position)<=2||WindCombat.Flank(Pawn,target,Props.tempestRange,false,out landing);
   return p!=WindPower.Rise||Pawn.Position.DistanceToSquared(target.Position)<=2;
  }
  public bool TryCast(WindPower power,Pawn target,bool automatic){
   if(!CanAct||Busy||Remaining(power)>0||!Valid(power,target,!automatic))return false;
   Job job=JobMaker.MakeJob(WindDefs.EH_WindTechnique,target);job.count=(int)power;job.playerForced=!automatic;
   Commit(power);Pawn.stances.CancelBusyStanceHard();
   if(automatic)Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);else Pawn.jobs.TryTakeOrderedJob(job,JobTag.Misc);
   return true;
  }
  public bool TryRetreat(Pawn threat){
   if(!CanAct||Busy||Remaining(WindPower.Draw)>0)return false;
   IntVec3 landing;if(!ElementalCombatPolicy.TryRetreat(Pawn,threat,Props.drawRange,WindCombat.ClearPath,out landing))return false;
   Job job=JobMaker.MakeJob(WindDefs.EH_WindTechnique,threat,landing);job.count=4;
   Commit(WindPower.Draw);Pawn.stances.CancelBusyStanceHard();Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:true);return true;
  }
  public void ChooseHair(bool restore){
   if(Pawn.story==null)return;
   if(restore){if(previousHair==null)return;Pawn.story.hairDef=previousHair;previousHair=null;}
   else{if(Pawn.story.hairDef.defName!="EH_WindHair")previousHair=Pawn.story.hairDef;Pawn.story.hairDef=DefDatabase<HairDef>.GetNamed("EH_WindHair");}
   Pawn.Drawer.renderer.SetAllGraphicsDirty();
  }
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   if(ElementalTeaching.IsHeart(parent)) yield return ElementalTeaching.Command(Pawn);
   if(!Pawn.Drafted)yield break;
   yield return new Command_Action{defaultLabel="Auto Combat: "+ElementalAutoModes.Label(CurrentAutoMode),defaultDesc=(CycloneEvolution.Active(Pawn)?"Cyclone automation chains the four Wind techniques and automatically uses Sovereign Cyclone against two nearby hostiles while transformed. One shared control governs all five techniques. ":"")+"Cycle On / Off / Non-lethal. Non-lethal replaces technique and ordinary melee wounds with temporary suppression: 75% knockdown chance, otherwise a brief stun. Manual techniques respect this mode.",icon=ElementalIcons.AutoCombat,action=CycleAutoMode,groupable=false};
   string[] names={"Dust-Cutting Draw","Rising Gale","Turning Swallow","Mountain-Scouring Tempest"};
   string[] descriptions={"Advance through the target with a single line cut. Requires a clear exit beyond them; stops at walls and hazards.","Two rising cuts against an adjacent target, followed by a short Gale Guard. Briefly staggers the target.","Watch the selected opponent for two seconds. Deflect their next ordinary melee hit, sidestep, and sweep back. Also reacts to a successful dodge. Expires if they do not attack.","Three timed cuts against one opponent, changing flanks between strikes and ending with a heavy diagonal cut. Automatic casts can carry remaining cuts to nearby hostiles when the target falls or escapes. Manual casts stay on the selected pawn; new orders interrupt the combination."};
   if(CycloneEvolution.Active(Pawn)){names=CycloneEvolution.Names;descriptions=CycloneEvolution.Details;}
   for(int i=0;i<4;i++){
    WindPower power=(WindPower)i;
    var command=new Command_Target{defaultLabel=names[i],defaultDesc=descriptions[i]+"\nRequires a melee weapon. Any faction can be selected manually. Downed targets are excluded. Chain different techniques within eight seconds for Flow ("+flowStacks+"/3). Base cooldown: "+BaseCooldown(power)/60f+"s.",icon=CycloneEvolution.Active(Pawn)?CycloneVfx.Icon(i):WindVfx.Icon(i),groupable=false,
     targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,neverTargetIncapacitated=true,validator=t=>Valid(power,t.Thing as Pawn,true)},
     action=t=>TryCast(power,t.Pawn,false),onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,Range(power));}};
    if(!CanAct)command.Disable("Requires an awake, drafted pawn capable of violence, holding a melee weapon.");else if(Busy)command.Disable("Performing a Wind technique.");else if(Remaining(power)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(power)/60f)+"s");
    yield return command;
   }
  }
 }
 public static class WindCombat {
  public static bool AliveTarget(Pawn source,Pawn target){return source!=null&&source.Spawned&&target!=null&&source!=target&&target.Spawned&&source.Map==target.Map&&!target.Dead&&!target.Downed;}
  public static bool Eligible(Pawn source,Pawn target){return AliveTarget(source,target)&&ElementalCombatPolicy.TargetAllowed(ElementalManualTarget.Explicit(source,target),ElementalHeartsMod.Settings.friendlyFire,target.IsPrisoner,target.HostileTo(source));}
  public static bool Hostile(Pawn source,Pawn target){return AliveTarget(source,target)&&!target.IsPrisoner&&target.HostileTo(source);}
  public static bool Safe(IntVec3 c,Map map){if(!c.InBounds(map)||!c.Standable(map))return false;var things=c.GetThingList(map);for(int i=0;i<things.Count;i++)if(things[i] is Fire)return false;return true;}
  public static bool ClearPath(IntVec3 a,IntVec3 b,Map map){foreach(var c in LightningGeometry.Trace(a,b))if(!Safe(c,map))return false;return true;}
  public static bool Landing(Pawn p,IntVec3 c){if(!Safe(c,p.Map))return false;var things=c.GetThingList(p.Map);for(int i=0;i<things.Count;i++){var other=things[i] as Pawn;if(other!=null&&other!=p&&(CycloneEvolution.Active(p)||!other.Downed))return false;}return true;}
  public static bool DrawLanding(Pawn p,Pawn target,float range,out IntVec3 landing){
   landing=IntVec3.Invalid;if(!AliveTarget(p,target))return false;
   int dx=Math.Sign(target.Position.x-p.Position.x),dz=Math.Sign(target.Position.z-p.Position.z);
   IntVec3 c=target.Position+new IntVec3(dx,0,dz);
   if(!Landing(p,c)||p.Position.DistanceToSquared(c)>(range+1.5f)*(range+1.5f)||!ClearPath(p.Position,c,p.Map))return false;
   landing=c;return true;
  }
  public static bool Flank(Pawn p,Pawn target,float range,bool opposite,out IntVec3 landing){
   List<IntVec3> path;return Flank(p,target,range,opposite,out landing,out path);
  }
  public static bool Flank(Pawn p,Pawn target,float range,bool opposite,out IntVec3 landing,out List<IntVec3> path){
   landing=IntVec3.Invalid;path=null;if(!AliveTarget(p,target))return false;float best=float.MinValue;
   var tree=WindNavigation.Tree(p.Position,(a,b)=>Landing(p,b)&&ClearPath(a,b,p.Map));
   foreach(var c in GenAdj.CellsAdjacent8Way(target)){
    if(c==p.Position||c.DistanceToSquared(p.Position)>range*range||!tree.ContainsKey(c)||!LightningCombat.ClearLine(c,target.Position,p.Map))continue;
    int dx=p.Position.x-target.Position.x,dz=p.Position.z-target.Position.z;
    float dot=dx*(c.x-target.Position.x)+dz*(c.z-target.Position.z);
    float score=(opposite?-dot:0)-c.DistanceToSquared(p.Position)*.05f;
    if(score<=best)continue;var candidate=WindNavigation.Path(tree,p.Position,c);if(candidate==null)continue;
    best=score;landing=c;path=candidate;
   }
   return landing.IsValid;
  }
  // Short footwork routes around the opponent instead of passing through them.
  public static List<IntVec3> Route(Pawn p,IntVec3 destination,bool through){
   if(through){
    var path=new List<IntVec3>();int dx=destination.x-p.Position.x,dz=destination.z-p.Position.z,n=Math.Max(Math.Abs(dx),Math.Abs(dz));IntVec3 prev=p.Position;
    for(int i=1;i<=n;i++){var next=new IntVec3(p.Position.x+(int)Math.Round(dx*i/(double)n),0,p.Position.z+(int)Math.Round(dz*i/(double)n));if(!ClearPath(prev,next,p.Map))return null;path.Add(next);prev=next;}return path;
   }
   return WindNavigation.Path(WindNavigation.Tree(p.Position,(a,b)=>Landing(p,b)&&ClearPath(a,b,p.Map)),p.Position,destination);
  }
  public static void Step(Pawn p,IntVec3 c){var from=p.DrawPos;if(CycloneEvolution.Active(p))CycloneVfx.Ghost(p,from);else WindVfx.Afterimage(p,from);p.pather.StopDead();p.Rotation=Rot4.FromAngleFlat((c-p.Position).ToVector3().AngleFlat());p.Position=c;p.Notify_Teleported(false,true);}
  public static bool Strike(Pawn source,Pawn victim,float multiplier){
   if(!Eligible(source,victim))return false;
   if(WaterDefense.TryParry(source,victim))return false;
   var comp=HediffComp_WindAbilities.On(source);if(comp==null||!comp.CanAct)return false;comp.MarkCombat();
   var profile=LightningCombat.Profile(source,1f);
   float damage=profile.damage*multiplier*ElementalTeaching.DamageFactor(source);
   if(ElementalNonLethal.TryApply(source,victim,damage))return false;
   victim.TakeDamage(new DamageInfo(DamageDefOf.Cut,damage,Math.Max(profile.penetration,comp.Props.armorPenetrationFloor),-1f,source,null,profile.weapon,DamageInfo.SourceCategory.ThingOrUnknown,victim));
   return true;
  }
 }
}

