using System;using System.Collections.Generic;using System.Linq;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class ThunderIncarnate {
  public const string DefName="EH_ThunderIncarnate",Ritual="EH_TranscendThunderIncarnate",CombatJob="EH_IncarnateTechnique";
  public const int Kills=300,Days=900000,RitualTicks=480,SkillCount=7;
  public static readonly string[] Names={"Divine Severance","Thunderstep Strike","Blackbolt Waltz","Skybreaker Judgment","Heaven-Rending Dragon","Heavenfall Verdict","Sovereign Thunderfall"};
  public static readonly string[] Icons={"DivineSeverance","Thunderstep","BlackboltWaltz","SkybreakerJudgment","HeavenRendingDragon","HeavenfallVerdict","SovereignThunderfall","TranscendThunderIncarnate","StormDominion"};
  public static readonly int[] Cooldowns={180,210,360,780,900,1320,2400};
  public static readonly float[] Ranges={18,16,8,18,18,18,28};
  public static bool Active(Pawn p){return p!=null&&p.health!=null&&p.health.hediffSet.hediffs.Any(h=>h.def.defName==DefName);}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&(p.CurJobDef.defName==CombatJob||p.CurJobDef.defName==Ritual||p.CurJobDef.defName==IncarnateTransit.Depart||p.CurJobDef.defName==IncarnateTransit.Arrive);}
  public static bool Impacting(Pawn p){var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_IncarnateTechnique;return d!=null&&d.impacting;}
  public static Texture2D Icon(int n){return ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/ThunderIncarnate/"+Icons[n]);}
  public static HediffComp_IncarnateMastery Mastery(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_IncarnateMastery>()).FirstOrDefault(c=>c!=null);}
  public static string Problem(Pawn p){var c=Mastery(p);if(!ThunderstormEvolution.Active(p)||c==null)return "Requires Thunderstorm ascension.";c.EnsureStarted();if(c.kills<Kills||c.Age<Days)return "Requires 300 new hostile non-animal kills and 15 days as Thunderstorm.";return CodexOrders.OrderProblem(p);}
  public static bool Begin(Pawn p){return Problem(p)==null&&p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Ritual),p),JobTag.Misc);}
  public static bool Complete(Pawn p){
   var m=Mastery(p);if(!ThunderstormEvolution.Active(p)||m==null||m.kills<Kills||m.Age<Days||!ElementalTeaching.Ready(p))return false;
   var old=HediffComp_LightningAbilities.On(p);var circuit=HediffComp_TempestCircuit.On(p);var remaining=Enumerable.Range(0,4).Select(i=>old.Remaining((LightningPower)i)).ToArray();int dragon=circuit==null?0:Math.Max(0,circuit.readyAt-Find.TickManager.TicksGame);
   var root=m.parent;AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));p.health.Notify_HediffChanged(root);
   var next=HediffComp_ThunderIncarnate.On(p);for(int i=0;i<4;i++)next.readyAt[i]=Find.TickManager.TicksGame+remaining[i];next.readyAt[4]=Find.TickManager.TicksGame+dragon;
   var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();foreach(var c in root.comps)c.Notify_Spawned();p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Ability(p,ElementalSound.Impact);
   Messages.Message(p.LabelShort+" has transcended as Thunder Incarnate!",p,MessageTypeDefOf.PositiveEvent,false);return true;
  }
 }
 public sealed class HediffCompProperties_IncarnateMastery:HediffCompProperties{public HediffCompProperties_IncarnateMastery(){compClass=typeof(HediffComp_IncarnateMastery);}}
 public sealed class GameComponent_IncarnateMasteryInit:GameComponent {
  public GameComponent_IncarnateMasteryInit(Game game){}
  public override void FinalizeInit(){foreach(var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){var m=ThunderIncarnate.Mastery(p);if(m!=null)m.EnsureStarted();}}
 }
 public sealed class HediffComp_IncarnateMastery:HediffComp {
  public int kills,startedAt=-1;HashSet<int> victims=new HashSet<int>();
  public int Age{get{return startedAt==-1?0:Math.Max(0,Find.TickManager.TicksGame-startedAt);}}
  public void EnsureStarted(){if(startedAt==-1&&ThunderstormEvolution.Active(Pawn))startedAt=Find.TickManager.TicksGame;}
  public override void Notify_Spawned(){LongEventHandler.ExecuteWhenFinished(EnsureStarted);}
  public override void CompPostTickInterval(ref float adjustment,int delta){EnsureStarted();}
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_incarnateMasteryKills");Scribe_Values.Look(ref startedAt,"EH_incarnateMasteryStarted",-1);Scribe_Collections.Look(ref victims,"EH_incarnateMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){EnsureStarted();if(ThunderstormEvolution.Active(Pawn)&&victim!=null&&AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn)&&victims.Add(victim.thingIDNumber))kills++;}
  public void MakeReady(){EnsureStarted();kills=Math.Max(kills,ThunderIncarnate.Kills);startedAt=Find.TickManager.TicksGame-ThunderIncarnate.Days;if(startedAt==-1)startedAt=-2;}
  public override string CompTipStringExtra{get{return ThunderIncarnate.Active(Pawn)?"Thunder Incarnate transcendence complete.":"Thunder Incarnate mastery: "+kills+" / 300 kills; "+(Age/60000f).ToString("0.0")+" / 15 Thunderstorm days.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn)||!ThunderstormEvolution.Active(Pawn))yield break;EnsureStarted();var c=new Command_Action{defaultLabel="Transcend: Thunder Incarnate",defaultDesc=CompTipStringExtra+"\nAn eight-second interruptible storm coronation grants seven grand black/yellow techniques, Celestial-tier power, 650% Moving capacity, princely regalia, Heavenbreaker and a storm crown. No resource cost. Interrupted rituals retain readiness.",icon=ThunderIncarnate.Icon(ThunderIncarnate.SkillCount),groupable=false,action=()=>ThunderIncarnate.Begin(Pawn)};string why=ThunderIncarnate.Problem(Pawn);if(why!=null)c.Disable(why);yield return c;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>ThunderstormEvolution.Active(Pawn),defaultLabel="DEV: Transcend-Ready",defaultDesc="God Mode only. Unlock the normal Transcend ceremony; this does not perform it.",icon=ThunderIncarnate.Icon(ThunderIncarnate.SkillCount),groupable=false,action=()=>{if(DebugSettings.godMode)MakeReady();}};
  }
 }
 public sealed class JobDriver_TranscendIncarnate:JobDriver {
  int started=-1,cue=-1;bool committed;
  public float Progress{get{return started<0?0:Mathf.Clamp01((Find.TickManager.TicksGame-started)/(float)ThunderIncarnate.RitualTicks);}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref started,"EH_incarnateRitualStart",-1);Scribe_Values.Look(ref cue,"EH_incarnateRitualCue",-1);Scribe_Values.Look(ref committed,"EH_incarnateRitualCommitted");}
  void Hold(){int now=Find.TickManager.TicksGame;if(started<0)started=now;pawn.pather.StopDead();pawn.Rotation=Rot4.South;IncarnateVfx.Track(pawn);int elapsed=now-started;int next=elapsed<120?0:elapsed<300?1:elapsed<420?2:3;if(next>cue){cue=next;IncarnateVfx.Burst(pawn,3,pawn.DrawPos,8+next*2,80);ElementalAudio.Ability(pawn,next<3?ElementalSound.Charge:ElementalSound.Impact);}}
  protected override IEnumerable<Toil> MakeNewToils(){this.FailOn(()=>!ElementalTeaching.Ready(pawn)||!ThunderstormEvolution.Active(pawn)||ElementalAnimationBridge.ExternallyControlled(pawn));var wait=Toils_General.Wait(ThunderIncarnate.RitualTicks);wait.handlingFacing=true;wait.initAction=Hold;wait.tickAction=Hold;wait.WithProgressBarToilDelay(TargetIndex.A);yield return wait;yield return Toils_General.Do(()=>{if(committed)return;committed=true;if(ThunderIncarnate.Complete(pawn)){IncarnateVfx.Burst(pawn,7,pawn.DrawPos,18,36);IncarnateVfx.Burst(pawn,6,pawn.DrawPos,7,32);}});}
 }
 public sealed class HediffCompProperties_ThunderIncarnate:HediffCompProperties{public HediffCompProperties_ThunderIncarnate(){compClass=typeof(HediffComp_ThunderIncarnate);}}
 public sealed class HediffComp_ThunderIncarnate:HediffComp {
  public bool autoStep=true,thunderTravelMode;public int stepReadyAt,nextStepCheck;public PlanetTile travelDestination=PlanetTile.Invalid;
  internal int lastMeleeVisualTick=-9999;
  public List<int> readyAt=new List<int>{0,0,0,0,0,0,0};public int resonance,lastPower=-1,lastUse,wardUntil,nextThink,combo;
  public static HediffComp_ThunderIncarnate On(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_ThunderIncarnate>()).FirstOrDefault(c=>c!=null);}
  public override void Notify_Spawned(){IncarnateVfx.Track(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){IncarnateVfx.Track(Pawn);}
  public void EnsureCooldowns(){if(readyAt==null)readyAt=new List<int>();while(readyAt.Count<ThunderIncarnate.SkillCount)readyAt.Add(0);if(readyAt.Count>ThunderIncarnate.SkillCount)readyAt.RemoveRange(ThunderIncarnate.SkillCount,readyAt.Count-ThunderIncarnate.SkillCount);}
  public override void CompExposeData(){Scribe_Collections.Look(ref readyAt,"EH_incarnateCooldowns",LookMode.Value);Scribe_Values.Look(ref resonance,"EH_incarnateResonance");Scribe_Values.Look(ref lastPower,"EH_incarnateLastPower",-1);Scribe_Values.Look(ref lastUse,"EH_incarnateLastUse");Scribe_Values.Look(ref wardUntil,"EH_incarnateWardUntil");Scribe_Values.Look(ref nextThink,"EH_incarnateNextThink");Scribe_Values.Look(ref combo,"EH_incarnateCombo");Scribe_Values.Look(ref autoStep,"EH_thunderStepAuto",true);Scribe_Values.Look(ref thunderTravelMode,"EH_thunderTravelMode");Scribe_Values.Look(ref stepReadyAt,"EH_thunderStepReadyAt");Scribe_Values.Look(ref nextStepCheck,"EH_thunderStepCheck");Scribe_Values.Look(ref travelDestination,"EH_thunderTravelDestination",PlanetTile.Invalid);if(Scribe.mode==LoadSaveMode.PostLoadInit&&thunderTravelMode)autoStep=false;EnsureCooldowns();resonance=Mathf.Clamp(resonance,0,4);combo=Math.Max(0,combo)%ThunderIncarnate.SkillCount;}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned&&!Pawn.Dead)ThunderStep.Tick(this);}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(ElementalCombatPolicy.Controlled(Pawn)&&Pawn.Spawned&&!Pawn.Dead)yield return ThunderStep.Command(Pawn);}
  public int CurrentResonance{get{return Find.TickManager.TicksGame-lastUse>360?0:resonance;}}
  public int Remaining(int n){EnsureCooldowns();return Math.Max(0,readyAt[n]-Find.TickManager.TicksGame);}
  public bool CanAct{get{return ThunderIncarnate.Active(Pawn)&&HediffComp_ElementalTransform.Active(Pawn)&&ElementalTeaching.Ready(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&!AzureEvolution.Evolving(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&!ElementalBestow.Busy(Pawn);}}
  public bool Start(int n,LocalTargetInfo target,bool automatic=false){if(n<0||n>=ThunderIncarnate.SkillCount||!CanAct||Remaining(n)>0)return false;var job=IncarnateCombat.Plan(Pawn,n,target,automatic);if(job==null)return false;Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:automatic);if(Pawn.CurJob!=job)return false;readyAt[n]=Find.TickManager.TicksGame+ThunderIncarnate.Cooldowns[n];nextThink=Find.TickManager.TicksGame+12;return true;}
  public bool Commit(int n){int now=Find.TickManager.TicksGame;if(now-lastUse>360)resonance=0;bool empowered=(n==3||n==5||n==6)&&resonance>=4;if(empowered){resonance=0;wardUntil=now+120;}else if(lastPower!=n)resonance=Math.Min(4,resonance+1);lastPower=n;lastUse=now;return empowered;}
  public override string CompTipStringExtra{get{return "Storm Dominion: "+CurrentResonance+" / 4 Resonance. Varied committed forms build charge. A full-charge Judgment, Verdict or Thunderfall gains 20% damage and two seconds of 20% additional protection. No extra impact packets.";}}
  public IEnumerable<Gizmo> Commands(){for(int i=0;i<ThunderIncarnate.SkillCount;i++){int power=i;Command command;if(i==2)command=new Command_Action{action=()=>Start(power,Pawn)};else command=new Command_Target{targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=i==0||i==5||i==6,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>Pawn.Spawned&&t.Cell.InBounds(Pawn.Map)&&!t.Cell.Fogged(Pawn.Map)&&Pawn.Position.DistanceToSquared(t.Cell)<=ThunderIncarnate.Ranges[power]*ThunderIncarnate.Ranges[power]},action=t=>{if(!Start(power,t))Messages.Message("No clear route, valid target or safe landing.",MessageTypeDefOf.RejectInput,false);},onUpdate=t=>{if(Pawn.Spawned){GenDraw.DrawRadiusRing(Pawn.Position,ThunderIncarnate.Ranges[power]);if(power==6&&t.IsValid&&t.Cell.InBounds(Pawn.Map))GenDraw.DrawRadiusRing(t.Cell,IncarnateCombat.BeamRadius);}}};
    command.defaultLabel=ThunderIncarnate.Names[i];command.icon=ThunderIncarnate.Icon(i);command.groupable=false;command.defaultDesc=IncarnateCombat.Descriptions[i]+"\nNo resource cost. "+(ThunderIncarnate.Cooldowns[i]/60f)+"s cooldown, spent on interruption. Shared Auto Combat and Non-lethal controls apply. One committed hit per eligible target. Real nearby scenery blasts, ash and fires are enabled in lethal mode; disable terrain impacts in mod settings if desired. Electrical injury has a 45% shock chance for 1.5 seconds, followed by three seconds of recovery.\n"+CompTipStringExtra;if(!CanAct)command.Disable("Transform, draft and finish the current action first.");else if(Remaining(i)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(i)/60f)+"s");yield return command;
   }}
  public void Auto(){int now=Find.TickManager.TicksGame;if(now<nextThink)return;nextThink=now+12;var c=HediffComp_LightningAbilities.On(Pawn);bool controlled=ElementalCombatPolicy.Controlled(Pawn);if(c==null||!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!CanAct||Pawn.CurJob==null||controlled&&Pawn.jobs.jobQueue.Count>0)return;if(controlled&&Pawn.CurJobDef!=JobDefOf.AttackMelee&&Pawn.CurJobDef!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,Pawn.CurJob))return;
   var targets=IncarnateCombat.Targets(Pawn,Pawn.Position,18,16);if(targets.Count==0)return;var target=targets[0];for(int count=0;count<ThunderIncarnate.SkillCount;count++){int n=(combo+count)%ThunderIncarnate.SkillCount;if(Remaining(n)>0||n==0&&!c.autoSever||n==1&&!c.autoFlash||n==2&&!c.autoDance)continue;if(n==2&&IncarnateCombat.Targets(Pawn,Pawn.Position,8,2).Count<2)continue;if(n==5&&CurrentResonance<4&&IncarnateCombat.Targets(Pawn,target.Position,8,2).Count<2)continue;if(n==6&&CurrentResonance<4&&IncarnateCombat.Targets(Pawn,target.Position,IncarnateCombat.BeamRadius,2).Count<2)continue;if(Start(n,n==2?new LocalTargetInfo(Pawn):new LocalTargetInfo(target),true)){combo=(n+1)%ThunderIncarnate.SkillCount;return;}}
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class IncarnateWard{public static void Prefix(Thing __instance,ref DamageInfo dinfo){var p=__instance as Pawn;var c=HediffComp_ThunderIncarnate.On(p);if(c!=null&&c.wardUntil>Find.TickManager.TicksGame&&dinfo.Amount>0)dinfo.SetAmount(dinfo.Amount*.8f);}}
}
