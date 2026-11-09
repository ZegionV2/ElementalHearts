using System;using System.Collections.Generic;using RimWorld;using UnityEngine;using Verse;using Verse.AI;
namespace ElementalHearts {
 public sealed class DevEvolutionReadyCommand:Command_Action {
  public Func<bool> isEligible;
  public bool developerModeOnly;
  public bool ModeEnabled{get{return developerModeOnly?Prefs.DevMode:DebugSettings.godMode;}}
  public override bool Visible{get{return ModeEnabled&&(isEligible==null||isEligible())&&base.Visible;}}
  // Also guard cached commands in skill panels that draw without rechecking Visible.
  protected override GizmoResult GizmoOnGUIInt(Rect rect,GizmoRenderParms parms){return Visible?base.GizmoOnGUIInt(rect,parms):default(GizmoResult);}
  public override void ProcessInput(Event ev){if(Visible)base.ProcessInput(ev);}
 }
 public static class EvolvedLineage {
  public static bool Active(Pawn p){return ThunderIncarnate.Active(p)||EclipseAscendant.Active(p)||AdamantEvolution.Active(p)||CelestialAscendant.Active(p)||CycloneEvolution.Active(p)||AzureEvolution.Active(p)||ThunderstormEvolution.Active(p)||GlacialEvolution.Active(p);}
  public static string Equipment(Pawn p,string normal){return ThunderIncarnate.Active(p)?"ThunderIncarnate":EclipseAscendant.Active(p)?"Eclipse":AdamantEvolution.Active(p)?"Adamant":CelestialAscendant.Active(p)?"Celestial":CycloneEvolution.Active(p)?"Cyclone":GlacialEvolution.Active(p)?"Glacial":ThunderstormEvolution.Active(p)?"Thunderstorm":AzureEvolution.Active(p)?"AzureFlame":normal;}
  public static bool RitualValid(Pawn p){if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==EclipseAscendant.Evolution)return HediffComp_LunarAbilities.On(p)!=null;if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveAdamant")return AdamantEvolution.Earth(p);if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveCyclone")return CycloneEvolution.Wind(p);return p!=null&&p.CurJob!=null&&p.CurJob.def.defName=="EH_EvolveGlacial"?WaterKitUpgrade.HasAscension(p):p!=null&&p.CurJob!=null&&p.CurJob.def.defName=="EH_EvolveThunderstorm"?ThunderstormEvolution.HasLightning(p)&&!ThunderstormEvolution.Active(p):AzureEvolution.HasFlame(p)&&!AzureEvolution.Active(p);}
  public static bool Complete(Pawn p){if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==EclipseAscendant.Evolution)return EclipseAscendant.Complete(p);if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveAdamant")return AdamantEvolution.Complete(p);if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName=="EH_EvolveCyclone")return CycloneEvolution.Complete(p);return p!=null&&p.CurJob!=null&&p.CurJob.def.defName=="EH_EvolveGlacial"?GlacialEvolution.Complete(p):p!=null&&p.CurJob!=null&&p.CurJob.def.defName=="EH_EvolveThunderstorm"?ThunderstormEvolution.Complete(p):AzureEvolution.Complete(p);}
 }
 public static class ThunderstormRules {
  public const int RequiredKills=150,RequiredTicks=600000,CircuitCooldown=1080;
  public static bool Ready(int kills,int ticks){return kills>=RequiredKills&&ticks>=RequiredTicks;}
  public static int MarkCap(int stacks){return Math.Max(0,Math.Min(3,stacks));}
  public static float PulseShare(int count){return 1f/Math.Max(1,count);}
 }
 public static class ThunderstormEvolution {
  public const string DefName="EH_ThunderstormAscendant";
  public static bool Active(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName==DefName)return true;return false;}
  public static bool HasLightning(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName==DefName||h.def.defName=="EH_LightningHeart")return true;return false;}
  public static HediffComp_ThunderstormMastery Mastery(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_ThunderstormMastery>();if(c!=null)return c;}return null;}
  public static Texture2D Icon(int index){return ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Thunderstorm/"+new[]{"CrimsonSever","RedlineFlash","ScarletTempest","SovereignThunder","TempestDragon","ThunderstormEvolution"}[index]);}
  public static readonly string[] Names={"Crimson Sever","Redline Flash","Scarlet Tempest","Sovereign Thunder"};
  public static readonly string[] Descriptions={"A focused red lightning slash up to twelve cells. Stops at walls, has improved penetration and applies electrical charge.","Blitz up to eight clear cells through the target with red streaks and regalia afterimages. Strike the primary target and graze up to three other hostiles along the lane once. Briefly slows a surviving primary hit. Walls and occupied landing cells block the route.","Spin a red lightning slash around the caster, striking eligible pawns within five clear cells once. Detonates Charged stacks and briefly staggers survivors, like the Lightning spin. Eight sequential frames show the rotating cut.","Blitz into melee with red afterimages, show a visible lightning warning, then deliver a focused thunderstrike and small controlled burst. Consumes at most three charge stacks. Movement, stun or undrafting interrupts it."};
  public static readonly HediffCompProperties_LightningAbilities Tuning=new HediffCompProperties_LightningAbilities{severRange=12,flashRange=8,danceRadius=5,heavenRange=12,severCooldown=300,flashCooldown=450,danceCooldown=750,heavenCooldown=1890,telegraphTicks=18,staggerTicks=60,fistFloor=10,explosionRadius=2.5f,severPhysical=1.2f,severElectrical=1.1f,flashPhysical=1.2f,flashElectrical=1.2f,dancePhysical=1.1f,danceElectrical=1.4f,heavenPhysical=6.6f,heavenElectrical=4.4f,explosionElectrical=1.65f};
  public static string Problem(Pawn p){var c=Mastery(p);if(c==null||!HasLightning(p))return "Requires Lightning ascension.";if(Active(p))return "Already evolved.";if(!ThunderstormRules.Ready(c.kills,c.parent.ageTicks))return "Requires 150 additional unique hostile non-animal kills and ten ascendant days.";if(!ElementalTeaching.Ready(p)||ElementalTeachingSession.Busy(p)||ElementalAnimationBridge.Controlled(p)||HediffComp_ElementalTransform.Transforming(p))return "Finish the current lesson, transformation or controlled action first.";if(p.CurJob!=null&&(p.CurJob.def.defName=="EH_HeavenlyThunder"||AzureEvolution.Evolving(p)))return "Finish the current ability first.";return null;}
  public static bool Upgrade(Pawn p){
   if(p==null||p.health==null)return false;HediffWithComps root=null;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName=="EH_LightningHeart")root=h as HediffWithComps;
   if(root==null)return Active(p);AzureLineage.Rebind(root,DefDatabase<HediffDef>.GetNamed(DefName));p.health.Notify_HediffChanged(root);
   var transform=root.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();
   if(p.Spawned){foreach(var c in root.comps)c.Notify_Spawned();ThunderstormVfx.Track(p);}return true;
  }
  public static bool Complete(Pawn p){if(p!=null&&p.CurJobDef!=null&&p.CurJobDef.defName==EclipseAscendant.Evolution)return EclipseAscendant.Complete(p);var c=Mastery(p);if(c==null||Active(p)||!ThunderstormRules.Ready(c.kills,c.parent.ageTicks)||!ElementalTeaching.Ready(p))return false;if(DefDatabase<ThingDef>.GetNamedSilentFail("EH_ThunderstormUniform")==null||DefDatabase<ThingDef>.GetNamedSilentFail("EH_ThunderstormSword")==null)return false;if(!Upgrade(p))return false;p.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Play(p,"EH_ThunderAscendance","EH_TransformLock");Messages.Message(p.LabelShort+" has evolved into a Thunderstorm Ascendant!",p,MessageTypeDefOf.PositiveEvent,false);return true;}
  public static void Slow(Pawn p){if(p==null||p.Dead||p.Downed||p.health==null)return;var def=DefDatabase<HediffDef>.GetNamed("EH_ThunderstormSlow");var h=p.health.hediffSet.GetFirstHediffOfDef(def);if(h==null)h=p.health.AddHediff(def);var timer=h.TryGetComp<HediffComp_Disappears>();if(timer!=null)timer.ticksToDisappear=60;}
 }
 public sealed class HediffCompProperties_ThunderstormMastery:HediffCompProperties {public HediffCompProperties_ThunderstormMastery(){compClass=typeof(HediffComp_ThunderstormMastery);}}
 public sealed class HediffComp_ThunderstormMastery:HediffComp {
  public int kills;HashSet<int> victims=new HashSet<int>();
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_stormMasteryKills");Scribe_Collections.Look(ref victims,"EH_stormMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  internal bool RecordKill(int id,bool eligible){if(!eligible||!victims.Add(id))return false;kills++;return true;}
  internal void MakeEvolutionReady(){kills=Math.Max(kills,150);parent.ageTicks=Math.Max(parent.ageTicks,600000);}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){if(victim!=null&&!ThunderstormEvolution.Active(Pawn)&&!ThunderIncarnate.Active(Pawn))RecordKill(victim.thingIDNumber,AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn));}
  public override string CompTipStringExtra{get{return ThunderstormEvolution.Active(Pawn)?"Thunderstorm evolution complete.":"Thunderstorm mastery: "+kills+" / 150 hostile kills; "+(parent.ageTicks/60000f).ToString("0.0")+" / 10 ascendant days. Each hostile non-animal target counts once.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn)||ThunderstormEvolution.Active(Pawn)||ThunderIncarnate.Active(Pawn))yield break;
   var c=new Command_Action{defaultLabel="Evolve: Thunderstorm",defaultDesc=CompTipStringExtra+"\nEvolve through a 4.5-second red lightning ritual. Grants five Thunderstorm skills, a standalone evolved condition, white/red royal regalia and a bound sword. No helmet. Interruption costs nothing.",icon=ThunderstormEvolution.Icon(5),groupable=false,action=()=>{if(ThunderstormEvolution.Problem(Pawn)==null)Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_EvolveThunderstorm"),Pawn),JobTag.Misc);}};
   var problem=ThunderstormEvolution.Problem(Pawn);if(problem!=null)c.Disable(problem);yield return c;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>!ThunderstormEvolution.Active(Pawn)&&ThunderstormEvolution.Mastery(Pawn)==this,defaultLabel="DEV: Evolution-Ready",defaultDesc="Set this Lightning Ascendant's requirements ready. God Mode only. Press Evolve: Thunderstorm afterward.",icon=ThunderstormEvolution.Icon(5),groupable=false,action=()=>{if(DebugSettings.godMode)MakeEvolutionReady();}};
  }
 }
 public sealed class ThoughtWorker_ThunderstormResolve:ThoughtWorker_Hediff {
  protected override ThoughtState CurrentStateInternal(Pawn p){return ThunderstormEvolution.Active(p)?ThoughtState.ActiveAtStage(1):base.CurrentStateInternal(p);}
 }
 public sealed class HediffCompProperties_ThunderstormHalo:HediffCompProperties {public HediffCompProperties_ThunderstormHalo(){compClass=typeof(HediffComp_ThunderstormHalo);}}
 public sealed class HediffComp_ThunderstormHalo:HediffComp {
  public override void Notify_Spawned(){ThunderstormVfx.Track(Pawn);}
  public override void CompPostPostAdd(DamageInfo? info){ThunderstormVfx.Track(Pawn);}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned)ThunderstormVfx.Track(Pawn);}
 }
}
