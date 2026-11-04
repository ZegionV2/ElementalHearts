using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class EclipseAscendant {
  public const string DefName="EH_EclipseAscendant",Technique="EH_EclipseTechnique",Evolution="EH_EvolveEclipse";
  public static bool Active(Pawn p){return HediffComp_EclipseAbilities.On(p)!=null;}
  public static bool Busy(Pawn p){return p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==Technique;}
  public static HediffComp_EclipseMastery Mastery(Pawn p){if(p!=null&&p.health!=null)foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_EclipseMastery>();if(c!=null)return c;}return null;}
  public static bool Milestones(Pawn p){var old=HediffComp_LunarAbilities.On(p);var mastery=Mastery(p);return old!=null&&(old.eclipseReady||mastery!=null&&AzureRules.Ready(mastery.kills,mastery.parent.ageTicks));}
  public static string Problem(Pawn p){
   if(HediffComp_LunarAbilities.On(p)==null)return "Requires normal Lunar ascension.";
   if(!Milestones(p))return "Requires 150 unique hostile non-animal kills and ten Lunar Ascendant days, or Celestial Bestow readiness.";
   if(!ElementalTeaching.Ready(p)||ElementalTeachingSession.Busy(p)||ElementalBestow.Busy(p)||LunarAscendant.Busy(p)||AzureEvolution.Evolving(p)||HediffComp_ElementalTransform.Transforming(p)||ElementalAnimationBridge.Controlled(p))return "Finish the current action, lesson or transformation first.";
   return null;
  }
  public static bool Upgrade(Pawn p){
   var lunar=HediffComp_LunarAbilities.On(p);if(lunar==null)return Active(p);
   var root=(HediffWithComps)lunar.parent;
   bool auto=lunar.autoCombat,nonLethal=lunar.nonLethal;
   var deadlines=lunar.ExportDeadlines();var marks=lunar.ExportMarks();
   if(p.Spawned)p.Map.GetComponent<MapComponent_LunarCombat>().Forget(p);
   AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));
   var evolved=root.TryGetComp<HediffComp_EclipseAbilities>();evolved.Inherit(auto,nonLethal,deadlines,marks);
   p.health.Notify_HediffChanged(root);
   var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();
   if(p.Spawned)foreach(var c in root.comps)c.Notify_Spawned();
   return true;
  }
  public static bool Complete(Pawn p){
   if(!Milestones(p)||!ElementalTeaching.Ready(p)||!Upgrade(p))return false;
   p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Ability(p,ElementalSound.Impact);
   Messages.Message(p.LabelShort+" has evolved into an Eclipse Ascendant!",p,MessageTypeDefOf.PositiveEvent,false);return true;
  }
 }
 public sealed class HediffCompProperties_EclipseMastery:HediffCompProperties {public HediffCompProperties_EclipseMastery(){compClass=typeof(HediffComp_EclipseMastery);}}
 public sealed class HediffComp_EclipseMastery:HediffComp {
  public int kills;HashSet<int> victims=new HashSet<int>();
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_eclipseMasteryKills");Scribe_Collections.Look(ref victims,"EH_eclipseMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){if(kills<150&&HediffComp_LunarAbilities.On(Pawn)!=null&&victim!=null&&AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn)&&victims.Add(victim.thingIDNumber))kills++;}
  public override string CompTipStringExtra{get{return EclipseAscendant.Active(Pawn)?"Eclipse evolution complete.":EclipseAscendant.Milestones(Pawn)?"Eclipse evolution ready. Press Evolve: Eclipse whenever you choose.":"Eclipse mastery: "+kills+" / 150 unique hostile non-animal kills; "+(parent.ageTicks/60000f).ToString("0.0")+" / 10 Lunar Ascendant days.";}}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.CurJobDef!=null&&Pawn.CurJobDef.defName==EclipseAscendant.Evolution)EvolutionCinematic.Track(Pawn);}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn)||HediffComp_LunarAbilities.On(Pawn)==null)yield break;
   var command=new Command_Action{defaultLabel="Evolve: Eclipse",defaultDesc=CompTipStringExtra+"\nA free 4.5-second eclipse coronation grants six black-and-crimson techniques, regal armor, an ivory half-mask, Eventide, a floating halo and Lunar Bestow. During an actual eclipse, core combat power rises to Celestial's tier. Interrupted ceremonies grant nothing. Controls, active cooldowns, marks and personal clothing carry over.",icon=EclipseVfx.Icon(6),groupable=false,action=()=>{if(EclipseAscendant.Problem(Pawn)==null){Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(EclipseAscendant.Evolution),Pawn),JobTag.Misc);Pawn.stances.CancelBusyStanceHard();}}};
   string problem=EclipseAscendant.Problem(Pawn);if(problem!=null)command.Disable(problem);yield return command;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>HediffComp_LunarAbilities.On(Pawn)!=null&&EclipseAscendant.Mastery(Pawn)==this,defaultLabel="DEV: Evolution-Ready",defaultDesc="God Mode only. Fulfill Eclipse milestones, then press Evolve: Eclipse.",icon=EclipseVfx.Icon(6),groupable=false,action=()=>{if(DebugSettings.godMode){kills=Math.Max(150,kills);parent.ageTicks=Math.Max(600000,parent.ageTicks);}}};
  }
 }
}
