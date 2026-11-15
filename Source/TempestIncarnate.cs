using System;using System.Collections.Generic;using System.Linq;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class TempestIncarnate {
  public const string DefName="EH_TempestIncarnate",Ritual="EH_TranscendTempest",CombatJob="EH_TempestTechnique";
  public const float ShockFactor=.25f;
  public const int Kills=300,Days=900000,RitualTicks=480,SkillCount=7;
  public static readonly string[] Names={"Cloudrend Crescent","Whitewake Pursuit","Stormwheel Waltz","Stormveil Reversal","Heaven-Rending Squall","Sovereign Maelstrom","Crown of the Tempest"};
  public static readonly string[] Icons={"CloudrendCrescent","WhitewakePursuit","StormwheelWaltz","StormveilReversal","HeavenRendingSquall","SovereignMaelstrom","CrownOfTheTempest","TranscendTempest","StormFlow"};
  public static readonly int[] Cooldowns={180,240,420,540,840,1320,2400};
  public static readonly float[] Ranges={20,18,8,5,18,8,28};
  public static bool Active(Pawn p){return p!=null&&p.health!=null&&p.health.hediffSet.hediffs.Any(h=>h.def.defName==DefName);}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&(p.CurJobDef.defName==CombatJob||p.CurJobDef.defName==Ritual||p.CurJobDef.defName==TempestUtility.Depart||p.CurJobDef.defName==TempestUtility.Arrive);}
  public static bool Capturing(Pawn p){var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_TempestTechnique;return d!=null&&d.CaptureJob;}
  public static bool Impacting(Pawn p){var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_TempestTechnique;return d!=null&&d.impacting;}
  public static Texture2D Icon(int n){return ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/TempestIncarnate/"+Icons[n]);}
  public static HediffComp_TempestMastery Mastery(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_TempestMastery>()).FirstOrDefault(c=>c!=null);}
  public static string Problem(Pawn p){var c=Mastery(p);if(!CycloneEvolution.Active(p)||c==null)return "Requires Cyclone ascension.";c.EnsureStarted();if(c.kills<Kills||c.Age<Days)return "Requires 300 new hostile non-animal kills and 15 days as Cyclone.";return CodexOrders.OrderProblem(p);}
  public static bool Begin(Pawn p){return Problem(p)==null&&p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Ritual),p),JobTag.Misc);}
  public static bool Complete(Pawn p){
   var m=Mastery(p);if(!CycloneEvolution.Active(p)||m==null||m.kills<Kills||m.Age<Days||!ElementalTeaching.Ready(p))return false;
   var old=HediffComp_WindAbilities.On(p);var cyclone=HediffComp_SovereignCyclone.On(p);var remaining=Enumerable.Range(0,4).Select(i=>old.Remaining((WindPower)i)).ToArray();int dragon=cyclone==null?0:Math.Max(0,cyclone.readyAt-Find.TickManager.TicksGame);
   var root=m.parent;AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));p.health.Notify_HediffChanged(root);
   var next=HediffComp_TempestIncarnate.On(p);int now=Find.TickManager.TicksGame;next.readyAt[0]=now+remaining[0];next.readyAt[1]=now+remaining[1];next.readyAt[3]=now+remaining[2];next.readyAt[4]=now+remaining[3];next.readyAt[5]=now+dragon;
   var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();foreach(var c in root.comps)c.Notify_Spawned();p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Ability(p,ElementalSound.Impact);
   Messages.Message(p.LabelShort+" has transcended as Tempest Incarnate!",p,MessageTypeDefOf.PositiveEvent,false);return true;
  }
 }
 public sealed class HediffCompProperties_TempestMastery:HediffCompProperties{public HediffCompProperties_TempestMastery(){compClass=typeof(HediffComp_TempestMastery);}}
 public sealed class GameComponent_TempestMasteryInit:GameComponent {
  public GameComponent_TempestMasteryInit(Game game){}
  public override void FinalizeInit(){foreach(var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){var m=TempestIncarnate.Mastery(p);if(m!=null)m.EnsureStarted();}}
 }
 public sealed class HediffComp_TempestMastery:HediffComp {
  public int kills,startedAt=-1;HashSet<int> victims=new HashSet<int>();
  public int Age{get{return startedAt==-1?0:Math.Max(0,Find.TickManager.TicksGame-startedAt);}}
  public void EnsureStarted(){if(startedAt==-1&&CycloneEvolution.Active(Pawn))startedAt=Find.TickManager.TicksGame;}
  public override void Notify_Spawned(){LongEventHandler.ExecuteWhenFinished(EnsureStarted);}
  public override void CompPostTickInterval(ref float adjustment,int delta){EnsureStarted();}
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_tempestMasteryKills");Scribe_Values.Look(ref startedAt,"EH_tempestMasteryStarted",-1);Scribe_Collections.Look(ref victims,"EH_tempestMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){EnsureStarted();if(kills<300&&CycloneEvolution.Active(Pawn)&&victim!=null&&AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn)&&victims.Add(victim.thingIDNumber))kills++;}
  public void MakeReady(){EnsureStarted();kills=Math.Max(kills,TempestIncarnate.Kills);startedAt=Find.TickManager.TicksGame-TempestIncarnate.Days;if(startedAt==-1)startedAt=-2;}
  public override string CompTipStringExtra{get{return TempestIncarnate.Active(Pawn)?"Tempest Incarnate transcendence complete.":"Tempest Incarnate mastery: "+kills+" / 300 kills; "+(Age/60000f).ToString("0.0")+" / 15 Cyclone days.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn)||!CycloneEvolution.Active(Pawn))yield break;EnsureStarted();var c=new Command_Action{defaultLabel="Transcend: Tempest Incarnate",defaultDesc=CompTipStringExtra+"\nAn eight-second interruptible storm coronation grants seven cloud-and-white-lightning techniques, Celestial-tier power, 650% Moving capacity, a dark princely suit/cape, Skyrend katana and a floating storm halo. No resource cost. Interrupted rituals retain readiness.",icon=TempestIncarnate.Icon(TempestIncarnate.SkillCount),groupable=false,action=()=>TempestIncarnate.Begin(Pawn)};string why=TempestIncarnate.Problem(Pawn);if(why!=null)c.Disable(why);yield return c;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>CycloneEvolution.Active(Pawn),defaultLabel="DEV: Transcend-Ready",defaultDesc="God Mode only. Unlock the normal Transcend ceremony; this does not perform it.",icon=TempestIncarnate.Icon(TempestIncarnate.SkillCount),groupable=false,action=()=>{if(DebugSettings.godMode)MakeReady();}};
  }
 }
 public sealed class JobDriver_TranscendTempest:JobDriver {
  int started=-1,cue=-1;bool committed;
  public float Progress{get{return started<0?0:Mathf.Clamp01((Find.TickManager.TicksGame-started)/(float)TempestIncarnate.RitualTicks);}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref started,"EH_tempestRitualStart",-1);Scribe_Values.Look(ref cue,"EH_tempestRitualCue",-1);Scribe_Values.Look(ref committed,"EH_tempestRitualCommitted");}
  void Hold(){int now=Find.TickManager.TicksGame;if(started<0)started=now;pawn.pather.StopDead();pawn.Rotation=Rot4.South;TempestVfx.Track(pawn);int elapsed=now-started;int next=elapsed<120?0:elapsed<300?1:elapsed<420?2:3;if(next>cue){cue=next;TempestVfx.Burst(pawn,20,pawn.DrawPos,8+next*2,80);ElementalAudio.Ability(pawn,next<3?ElementalSound.Charge:ElementalSound.Impact);}}
  protected override IEnumerable<Toil> MakeNewToils(){this.FailOn(()=>!ElementalTeaching.Ready(pawn)||!CycloneEvolution.Active(pawn)||ElementalAnimationBridge.ExternallyControlled(pawn));var wait=Toils_General.Wait(TempestIncarnate.RitualTicks);wait.handlingFacing=true;wait.initAction=Hold;wait.tickAction=Hold;wait.WithProgressBarToilDelay(TargetIndex.A);yield return wait;yield return Toils_General.Do(()=>{if(committed)return;committed=true;if(TempestIncarnate.Complete(pawn)){TempestVfx.Burst(pawn,7,pawn.DrawPos,18,36);TempestVfx.Burst(pawn,6,pawn.DrawPos,7,32);}});}
 }
 public sealed class HediffCompProperties_TempestIncarnate:HediffCompProperties{public HediffCompProperties_TempestIncarnate(){compClass=typeof(HediffComp_TempestIncarnate);}}
 public sealed class HediffComp_TempestIncarnate:HediffComp {
  public bool autoStep=true,thunderTravelMode;public int stepReadyAt,nextStepCheck;public PlanetTile travelDestination=PlanetTile.Invalid;
  internal int lastMeleeVisualTick=-9999;
  public List<int> readyAt=new List<int>{0,0,0,0,0,0,0};public int nextThink,combo,wardUntil;
  public static HediffComp_TempestIncarnate On(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_TempestIncarnate>()).FirstOrDefault(c=>c!=null);}
  public override void Notify_Spawned(){TempestVfx.Track(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){TempestVfx.Track(Pawn);}
  public void EnsureCooldowns(){if(readyAt==null)readyAt=new List<int>();while(readyAt.Count<TempestIncarnate.SkillCount)readyAt.Add(0);if(readyAt.Count>TempestIncarnate.SkillCount)readyAt.RemoveRange(TempestIncarnate.SkillCount,readyAt.Count-TempestIncarnate.SkillCount);}
  public override void CompExposeData(){Scribe_Collections.Look(ref readyAt,"EH_tempestCooldowns",LookMode.Value);Scribe_Values.Look(ref nextThink,"EH_tempestNextThink");Scribe_Values.Look(ref combo,"EH_tempestCombo");Scribe_Values.Look(ref wardUntil,"EH_tempestWardUntil");Scribe_Values.Look(ref autoStep,"EH_tempestStepAuto",true);Scribe_Values.Look(ref thunderTravelMode,"EH_tempestTravelMode");Scribe_Values.Look(ref stepReadyAt,"EH_tempestStepReadyAt");Scribe_Values.Look(ref nextStepCheck,"EH_tempestStepCheck");Scribe_Values.Look(ref travelDestination,"EH_tempestTravelDestination",PlanetTile.Invalid);if(Scribe.mode==LoadSaveMode.PostLoadInit&&thunderTravelMode)autoStep=false;EnsureCooldowns();combo=Math.Max(0,combo)%TempestIncarnate.SkillCount;}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned&&!Pawn.Dead)CloudStep.Tick(this);}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(ElementalCombatPolicy.Controlled(Pawn)&&Pawn.Spawned&&!Pawn.Dead)yield return CloudStep.Command(Pawn);}
  public int Remaining(int n){EnsureCooldowns();return Math.Max(0,readyAt[n]-Find.TickManager.TicksGame);}
  public bool CanAct{get{return TempestIncarnate.Active(Pawn)&&HediffComp_ElementalTransform.Active(Pawn)&&ElementalTeaching.Ready(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&HediffComp_WindAbilities.On(Pawn)!=null&&HediffComp_WindAbilities.On(Pawn).HasWeapon&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&!AzureEvolution.Evolving(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&!ElementalBestow.Busy(Pawn);}}
  public bool Start(int n,LocalTargetInfo target,bool automatic=false){if(n<0||n>=TempestIncarnate.SkillCount||!CanAct||Remaining(n)>0)return false;var job=TempestCombat.Plan(Pawn,n,target,automatic);if(job==null)return false;Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:IncarnateCombatFlow.SuspendOnce(Pawn,automatic));if(Pawn.CurJob!=job)return false;readyAt[n]=Find.TickManager.TicksGame+TempestIncarnate.Cooldowns[n];nextThink=Find.TickManager.TicksGame+12;return true;}
  public override string CompTipStringExtra{get{var c=HediffComp_WindAbilities.On(Pawn);return "Storm Flow: "+(c==null?0:c.flowStacks)+" / 3. Different successful forms build charge for eight seconds. At full Flow, Squall, Maelstrom or Crown consumes charge for 20% stronger committed strikes and two seconds of 20% additional defense. Blast damage is reduced by 65%; electrical shock chance and duration by 75%.";}}
  public bool Commit(int power){var c=HediffComp_WindAbilities.On(Pawn);if(c==null)return false;int now=Find.TickManager.TicksGame;bool empowered=power>=4&&c.flowUntil>now&&c.flowStacks>=3;if(empowered){c.flowStacks=0;c.lastPower=-1;wardUntil=now+120;}else{c.flowStacks=WindRhythm.NextStacks(c.lastPower,power,c.flowStacks,c.flowUntil<=now);c.lastPower=power;}c.flowUntil=now+480;c.MarkCombat();if(c.flowStacks>0)c.Status(WindDefs.EH_WindFlow,480,c.flowStacks);else{var h=Pawn.health.hediffSet.GetFirstHediffOfDef(WindDefs.EH_WindFlow);if(h!=null)Pawn.health.RemoveHediff(h);}return empowered;}

  public IEnumerable<Gizmo> Commands(){for(int i=0;i<TempestIncarnate.SkillCount;i++){int power=i;Command command;if(i==2||i==5)command=new Command_Action{action=()=>Start(power,Pawn)};else command=new Command_Target{targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=i==0||i==6,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>Pawn.Spawned&&t.Cell.InBounds(Pawn.Map)&&!t.Cell.Fogged(Pawn.Map)&&Pawn.Position.DistanceToSquared(t.Cell)<=TempestIncarnate.Ranges[power]*TempestIncarnate.Ranges[power]},action=t=>{if(!Start(power,t))Messages.Message("No clear route, valid target or safe landing.",MessageTypeDefOf.RejectInput,false);},onUpdate=t=>{if(Pawn.Spawned){GenDraw.DrawRadiusRing(Pawn.Position,TempestIncarnate.Ranges[power]);if(power==6&&t.IsValid&&t.Cell.InBounds(Pawn.Map))GenDraw.DrawRadiusRing(t.Cell,TempestCombat.BeamRadius);}}};
    command.defaultLabel=TempestIncarnate.Names[i];command.icon=TempestIncarnate.Icon(i);command.groupable=false;command.defaultDesc=TempestCombat.Descriptions[i]+"\nNo resource cost. "+(TempestIncarnate.Cooldowns[i]/60f)+"s cooldown, spent on interruption. Shared Auto Combat and Non-lethal controls apply. Each form commits once per victim; Squall explicitly commits three separate timed cuts to its primary target. Allies/prisoners are excluded; floor effects expire without changing terrain.\n"+CompTipStringExtra;if(!CanAct)command.Disable("Transform, draft and finish the current action first.");else if(Remaining(i)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(i)/60f)+"s");yield return command;
   }}
  public void Auto(){int now=Find.TickManager.TicksGame;if(now<nextThink)return;nextThink=now+12;var c=HediffComp_WindAbilities.On(Pawn);bool controlled=ElementalCombatPolicy.Controlled(Pawn);if(c==null||!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!CanAct||Pawn.CurJob==null||controlled&&IncarnateCombatFlow.BlockingOrders(Pawn))return;if(controlled&&Pawn.CurJobDef!=JobDefOf.AttackMelee&&Pawn.CurJobDef!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,Pawn.CurJob))return;
   var targets=TempestCombat.Targets(Pawn,Pawn.Position,28,16);if(targets.Count==0)return;var target=IncarnateCombatFlow.Target(Pawn,targets[0]);
   for(int count=0;count<7;count++){int n=(combo+count)%7;if(Remaining(n)>0||n==3&&!TempestCombat.Threatening(Pawn,target))continue;if(Start(n,n==2||n==5?new LocalTargetInfo(Pawn):new LocalTargetInfo(target),true)){combo=(n+1)%7;return;}}
  }

 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class TempestWard {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){var c=HediffComp_TempestIncarnate.On(__instance as Pawn);if(c!=null&&c.wardUntil>Find.TickManager.TicksGame&&dinfo.Amount>0)dinfo.SetAmount(dinfo.Amount*.8f);}
 }
}
