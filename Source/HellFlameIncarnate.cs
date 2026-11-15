using System;using System.Collections.Generic;using System.Linq;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public static class HellFlameIncarnate {
  public const string DefName="EH_HellFlameIncarnate",Ritual="EH_TranscendHellFlame",CombatJob="EH_HellTechnique";
  public const float ShockFactor=.25f;
  public const int Kills=300,Days=900000,RitualTicks=480,SkillCount=7;
  public static readonly string[] Names={"Hellscar Crescent","Ashen Pursuit","Infernal Waltz","Ruinous Phoenix","Hellfire Reversal","Furnace Dominion","Sovereign's Pyre"};
  public static readonly string[] Icons={"HellscarCrescent","AshenPursuit","InfernalWaltz","RuinousPhoenix","HellfireReversal","FurnaceDominion","SovereignsPyre","TranscendHellFlame","Hellbrands"};
  public static readonly int[] Cooldowns={180,240,420,720,900,1320,2400};
  public static readonly float[] Ranges={18,16,8,18,16,8,28};
  public static bool Active(Pawn p){return p!=null&&p.health!=null&&p.health.hediffSet.hediffs.Any(h=>h.def.defName==DefName);}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&(p.CurJobDef.defName==CombatJob||p.CurJobDef.defName==Ritual||p.CurJobDef.defName==HellUtility.Depart||p.CurJobDef.defName==HellUtility.Arrive);}
  public static bool Impacting(Pawn p){var d=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_HellTechnique;return d!=null&&d.impacting;}
  public static Texture2D Icon(int n){return ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/HellFlameIncarnate/"+Icons[n]);}
  public static HediffComp_HellMastery Mastery(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_HellMastery>()).FirstOrDefault(c=>c!=null);}
  public static string Problem(Pawn p){var c=Mastery(p);if(!AzureEvolution.Active(p)||c==null)return "Requires Azure Flame ascension.";c.EnsureStarted();if(c.kills<Kills||c.Age<Days)return "Requires 300 new hostile non-animal kills and 15 days as Azure Flame.";return CodexOrders.OrderProblem(p);}
  public static bool Begin(Pawn p){return Problem(p)==null&&p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(Ritual),p),JobTag.Misc);}
  public static bool Complete(Pawn p){
   var m=Mastery(p);if(!AzureEvolution.Active(p)||m==null||m.kills<Kills||m.Age<Days||!ElementalTeaching.Ready(p))return false;
   var old=HediffComp_FlameAbilities.On(p);var reversal=HediffComp_AzureReversal.On(p);var remaining=Enumerable.Range(0,4).Select(i=>old.Remaining((FlamePower)i)).ToArray();int dragon=reversal==null?0:Math.Max(0,reversal.readyAt-Find.TickManager.TicksGame);
   var root=m.parent;AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));p.health.Notify_HediffChanged(root);
   var next=HediffComp_HellFlameIncarnate.On(p);for(int i=0;i<4;i++)next.readyAt[i]=Find.TickManager.TicksGame+remaining[i];next.readyAt[4]=Find.TickManager.TicksGame+dragon;
   var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();foreach(var c in root.comps)c.Notify_Spawned();p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Ability(p,ElementalSound.Impact);
   Messages.Message(p.LabelShort+" has transcended as Hell Flame Incarnate!",p,MessageTypeDefOf.PositiveEvent,false);return true;
  }
 }
 public sealed class HediffCompProperties_HellMastery:HediffCompProperties{public HediffCompProperties_HellMastery(){compClass=typeof(HediffComp_HellMastery);}}
 public sealed class GameComponent_HellMasteryInit:GameComponent {
  public GameComponent_HellMasteryInit(Game game){}
  public override void FinalizeInit(){foreach(var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){var m=HellFlameIncarnate.Mastery(p);if(m!=null)m.EnsureStarted();}}
 }
 public sealed class HediffComp_HellMastery:HediffComp {
  public int kills,startedAt=-1;HashSet<int> victims=new HashSet<int>();
  public int Age{get{return startedAt==-1?0:Math.Max(0,Find.TickManager.TicksGame-startedAt);}}
  public void EnsureStarted(){if(startedAt==-1&&AzureEvolution.Active(Pawn))startedAt=Find.TickManager.TicksGame;}
  public override void Notify_Spawned(){LongEventHandler.ExecuteWhenFinished(EnsureStarted);}
  public override void CompPostTickInterval(ref float adjustment,int delta){EnsureStarted();}
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_hellMasteryKills");Scribe_Values.Look(ref startedAt,"EH_hellMasteryStarted",-1);Scribe_Collections.Look(ref victims,"EH_hellMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){EnsureStarted();if(kills<300&&AzureEvolution.Active(Pawn)&&victim!=null&&AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn)&&victims.Add(victim.thingIDNumber))kills++;}
  public void MakeReady(){EnsureStarted();kills=Math.Max(kills,HellFlameIncarnate.Kills);startedAt=Find.TickManager.TicksGame-HellFlameIncarnate.Days;if(startedAt==-1)startedAt=-2;}
  public override string CompTipStringExtra{get{return HellFlameIncarnate.Active(Pawn)?"Hell Flame Incarnate transcendence complete.":"Hell Flame Incarnate mastery: "+kills+" / 300 kills; "+(Age/60000f).ToString("0.0")+" / 15 Azure Flame days.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn)||!AzureEvolution.Active(Pawn))yield break;EnsureStarted();var c=new Command_Action{defaultLabel="Transcend: Hell Flame Incarnate",defaultDesc=CompTipStringExtra+"\nAn eight-second interruptible infernal coronation grants seven hellfire techniques, Celestial-tier power, 650% Moving capacity, a black suit/cape, Ashreign katana and an infernal crown. No resource cost. Interrupted rituals retain readiness.",icon=HellFlameIncarnate.Icon(HellFlameIncarnate.SkillCount),groupable=false,action=()=>HellFlameIncarnate.Begin(Pawn)};string why=HellFlameIncarnate.Problem(Pawn);if(why!=null)c.Disable(why);yield return c;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>AzureEvolution.Active(Pawn),defaultLabel="DEV: Transcend-Ready",defaultDesc="God Mode only. Unlock the normal Transcend ceremony; this does not perform it.",icon=HellFlameIncarnate.Icon(HellFlameIncarnate.SkillCount),groupable=false,action=()=>{if(DebugSettings.godMode)MakeReady();}};
  }
 }
 public sealed class JobDriver_TranscendHell:JobDriver {
  int started=-1,cue=-1;bool committed;
  public float Progress{get{return started<0?0:Mathf.Clamp01((Find.TickManager.TicksGame-started)/(float)HellFlameIncarnate.RitualTicks);}}
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref started,"EH_hellRitualStart",-1);Scribe_Values.Look(ref cue,"EH_hellRitualCue",-1);Scribe_Values.Look(ref committed,"EH_hellRitualCommitted");}
  void Hold(){int now=Find.TickManager.TicksGame;if(started<0)started=now;pawn.pather.StopDead();pawn.Rotation=Rot4.South;HellVfx.Track(pawn);int elapsed=now-started;int next=elapsed<120?0:elapsed<300?1:elapsed<420?2:3;if(next>cue){cue=next;HellVfx.Burst(pawn,20,pawn.DrawPos,8+next*2,80);ElementalAudio.Ability(pawn,next<3?ElementalSound.Charge:ElementalSound.Impact);}}
  protected override IEnumerable<Toil> MakeNewToils(){this.FailOn(()=>!ElementalTeaching.Ready(pawn)||!AzureEvolution.Active(pawn)||ElementalAnimationBridge.ExternallyControlled(pawn));var wait=Toils_General.Wait(HellFlameIncarnate.RitualTicks);wait.handlingFacing=true;wait.initAction=Hold;wait.tickAction=Hold;wait.WithProgressBarToilDelay(TargetIndex.A);yield return wait;yield return Toils_General.Do(()=>{if(committed)return;committed=true;if(HellFlameIncarnate.Complete(pawn)){HellVfx.Burst(pawn,7,pawn.DrawPos,18,36);HellVfx.Burst(pawn,6,pawn.DrawPos,7,32);}});}
 }
 public sealed class HediffCompProperties_HellFlameIncarnate:HediffCompProperties{public HediffCompProperties_HellFlameIncarnate(){compClass=typeof(HediffComp_HellFlameIncarnate);}}
 public sealed class HediffComp_HellFlameIncarnate:HediffComp {
  public bool autoStep=true,thunderTravelMode;public int stepReadyAt,nextStepCheck;public PlanetTile travelDestination=PlanetTile.Invalid;
  internal int lastMeleeVisualTick=-9999;
  public List<int> readyAt=new List<int>{0,0,0,0,0,0,0};public int nextThink,combo,wardUntil;
  public static HediffComp_HellFlameIncarnate On(Pawn p){return p==null||p.health==null?null:p.health.hediffSet.hediffs.Select(h=>h.TryGetComp<HediffComp_HellFlameIncarnate>()).FirstOrDefault(c=>c!=null);}
  public override void Notify_Spawned(){HellVfx.Track(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){HellVfx.Track(Pawn);}
  public void EnsureCooldowns(){if(readyAt==null)readyAt=new List<int>();while(readyAt.Count<HellFlameIncarnate.SkillCount)readyAt.Add(0);if(readyAt.Count>HellFlameIncarnate.SkillCount)readyAt.RemoveRange(HellFlameIncarnate.SkillCount,readyAt.Count-HellFlameIncarnate.SkillCount);}
  public override void CompExposeData(){Scribe_Collections.Look(ref readyAt,"EH_hellCooldowns",LookMode.Value);Scribe_Values.Look(ref nextThink,"EH_hellNextThink");Scribe_Values.Look(ref combo,"EH_hellCombo");Scribe_Values.Look(ref wardUntil,"EH_hellWardUntil");Scribe_Values.Look(ref autoStep,"EH_hellStepAuto",true);Scribe_Values.Look(ref thunderTravelMode,"EH_hellTravelMode");Scribe_Values.Look(ref stepReadyAt,"EH_hellStepReadyAt");Scribe_Values.Look(ref nextStepCheck,"EH_hellStepCheck");Scribe_Values.Look(ref travelDestination,"EH_hellTravelDestination",PlanetTile.Invalid);if(Scribe.mode==LoadSaveMode.PostLoadInit&&thunderTravelMode)autoStep=false;EnsureCooldowns();combo=Math.Max(0,combo)%HellFlameIncarnate.SkillCount;}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned&&!Pawn.Dead)HellStep.Tick(this);}
  public override IEnumerable<Gizmo> CompGetGizmos(){if(ElementalCombatPolicy.Controlled(Pawn)&&Pawn.Spawned&&!Pawn.Dead)yield return HellStep.Command(Pawn);}
  public int Remaining(int n){EnsureCooldowns();return Math.Max(0,readyAt[n]-Find.TickManager.TicksGame);}
  public bool CanAct{get{return HellFlameIncarnate.Active(Pawn)&&HediffComp_ElementalTransform.Active(Pawn)&&ElementalTeaching.Ready(Pawn)&&(!ElementalCombatPolicy.Controlled(Pawn)||Pawn.Drafted)&&!Pawn.WorkTagIsDisabled(WorkTags.Violent)&&!AzureEvolution.Evolving(Pawn)&&!ElementalAnimationBridge.Controlled(Pawn)&&!ElementalBestow.Busy(Pawn);}}
  public bool Start(int n,LocalTargetInfo target,bool automatic=false){if(n<0||n>=HellFlameIncarnate.SkillCount||!CanAct||Remaining(n)>0)return false;var job=HellCombat.Plan(Pawn,n,target,automatic);if(job==null)return false;Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:IncarnateCombatFlow.SuspendOnce(Pawn,automatic));if(Pawn.CurJob!=job)return false;readyAt[n]=Find.TickManager.TicksGame+HellFlameIncarnate.Cooldowns[n];nextThink=Find.TickManager.TicksGame+12;return true;}
  public override string CompTipStringExtra{get{return "Hellbrands: opening strikes build up to three caster-owned brands. Phoenix and Sovereign's Pyre consume them for a capped bonus to the same impact. Consuming three empowers that victim's strike by 20% and grants two seconds of 20% additional protection. Infernal Resolve reduces electrical shock chance and duration by 75%. No extra detonation packets.";}}
  public IEnumerable<Gizmo> Commands(){for(int i=0;i<HellFlameIncarnate.SkillCount;i++){int power=i;Command command;if(i==2||i==4||i==5)command=new Command_Action{action=()=>Start(power,Pawn)};else command=new Command_Target{targetingParams=new TargetingParameters{canTargetPawns=true,canTargetLocations=i==0||i==5||i==6,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>Pawn.Spawned&&t.Cell.InBounds(Pawn.Map)&&!t.Cell.Fogged(Pawn.Map)&&Pawn.Position.DistanceToSquared(t.Cell)<=HellFlameIncarnate.Ranges[power]*HellFlameIncarnate.Ranges[power]},action=t=>{if(!Start(power,t))Messages.Message("No clear route, valid target or safe landing.",MessageTypeDefOf.RejectInput,false);},onUpdate=t=>{if(Pawn.Spawned){GenDraw.DrawRadiusRing(Pawn.Position,HellFlameIncarnate.Ranges[power]);if(power==6&&t.IsValid&&t.Cell.InBounds(Pawn.Map))GenDraw.DrawRadiusRing(t.Cell,HellCombat.BeamRadius);}}};
    command.defaultLabel=HellFlameIncarnate.Names[i];command.icon=HellFlameIncarnate.Icon(i);command.groupable=false;command.defaultDesc=HellCombat.Descriptions[i]+"\nNo resource cost. "+(HellFlameIncarnate.Cooldowns[i]/60f)+"s cooldown, spent on interruption. Shared Auto Combat and Non-lethal controls apply. One committed hit per eligible target. Hellfire Reversal requires two hostile pawns within three cells. Allies/prisoners are excluded; floor effects expire and do not ignite the map.\n"+CompTipStringExtra;if(!CanAct)command.Disable("Transform, draft and finish the current action first.");else if(Remaining(i)>0)command.Disable("Recharging: "+Math.Ceiling(Remaining(i)/60f)+"s");yield return command;
   }}
  public void Auto(){int now=Find.TickManager.TicksGame;if(now<nextThink)return;nextThink=now+12;var c=HediffComp_FlameAbilities.On(Pawn);bool controlled=ElementalCombatPolicy.Controlled(Pawn);if(c==null||!ElementalCombatPolicy.AutomationEnabled(controlled,ElementalHeartsMod.Settings.enableAutomation,c.autoCombat)||!CanAct||Pawn.CurJob==null||controlled&&IncarnateCombatFlow.BlockingOrders(Pawn))return;if(controlled&&Pawn.CurJobDef!=JobDefOf.AttackMelee&&Pawn.CurJobDef!=JobDefOf.Wait_Combat&&!SearchAndDestroyBridge.IsApproach(Pawn,Pawn.CurJob))return;
   var targets=HellCombat.Targets(Pawn,Pawn.Position,18,16);if(targets.Count==0)return;var target=IncarnateCombatFlow.Target(Pawn,targets[0]);if(Remaining(4)==0&&HellCombat.Targets(Pawn,Pawn.Position,3,2).Count>=2&&Start(4,target,true))return;
   // An evolved rival is a worthwhile single target. Crowd-only gates previously left
   // three offensive forms unused in duels; Reversal still requires two pressuring foes.
   bool duelist=EvolvedLineage.Active(target);int brands=HellBrands.Count(Pawn,target);
   for(int count=0;count<7;count++){int n=(combo+count)%7;if(n==4||Remaining(n)>0||n==0&&!c.autoSever||n==1&&!c.autoFlash||n==2&&!c.autoDance)continue;if((n==2||n==5)&&!duelist&&brands<2&&HellCombat.Targets(Pawn,Pawn.Position,8,2).Count<2)continue;if(n==6&&!duelist&&brands<3&&HellCombat.Targets(Pawn,target.Position,10,2).Count<2)continue;if(Start(n,n==2||n==5?new LocalTargetInfo(Pawn):new LocalTargetInfo(target),true)){combo=(n+1)%7;return;}}
  }

 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class InfernalWard {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){var c=HediffComp_HellFlameIncarnate.On(__instance as Pawn);if(c!=null&&c.wardUntil>Find.TickManager.TicksGame&&dinfo.Amount>0)dinfo.SetAmount(dinfo.Amount*.8f);}
 }
}
