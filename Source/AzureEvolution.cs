using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class AzureEvolution {
  public const string DefName="EH_AzureFlameAscendant";
  public static bool Active(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if(h.def.defName==DefName)return true;return false;}
  public static HediffComp_FlameMastery Mastery(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_FlameMastery>();if(c!=null)return c;}return null;}
  public static bool Evolving(Pawn p){return p!=null&&p.CurJob!=null&&p.CurJob.def!=null&&(p.CurJob.def.defName=="EH_EvolveAzure"||p.CurJob.def.defName=="EH_EvolveThunderstorm"||p.CurJob.def.defName=="EH_EvolveGlacial"||p.CurJob.def.defName=="EH_EvolveCyclone");}
  public static bool HasFlame(Pawn p){if(p==null||p.health==null)return false;foreach(var h in p.health.hediffSet.hediffs)if((h.def.defName=="EH_FlameHeart"||h.def.defName==DefName))return true;return false;}
  public static string Problem(Pawn p){var c=Mastery(p);if(c==null||!HasFlame(p))return "Requires Flame ascension.";if(Active(p))return "Already evolved.";if(!AzureRules.Ready(c.kills,c.parent.ageTicks))return "Requires 150 additional hostile non-animal kills and ten days as a Flame Ascendant.";if(ElementalTeachingSession.Busy(p))return "Finish the teaching session first.";if(!ElementalTeaching.Ready(p)||ElementalAnimationBridge.Controlled(p)||HediffComp_ElementalTransform.Transforming(p))return "Must be conscious and free of another transformation or controlled animation.";if(p.CurJob!=null&&(p.CurJob.def.defName=="EH_PhoenixDescent"||p.CurJob.def.defName=="EH_EvolveAzure"))return "Finish the current ability first.";return null;}
  public static bool Complete(Pawn p){
   // Called by the finishing toil; the active evolution job is expected here.
   var mastery=Mastery(p);if(mastery==null||Active(p)||!HasFlame(p)||!AzureRules.Ready(mastery.kills,mastery.parent.ageTicks)||!ElementalTeaching.Ready(p))return false;
   if(DefDatabase<ThingDef>.GetNamedSilentFail("EH_AzureFlameUniform")==null||DefDatabase<ThingDef>.GetNamedSilentFail("EH_AzureFlameSword")==null)return false;
   if(!AzureLineage.Upgrade(p))return false;
   var transform=HediffComp_ElementalTransform.On(p);if(transform!=null)transform.RebuildAzureKit();
   p.Drawer.renderer.SetAllGraphicsDirty();AzureVfx.Track(p);ElementalAudio.Play(p,"EH_TransformLock");
   Messages.Message(p.LabelShort+" has evolved into an Azure Flame Ascendant!",p,MessageTypeDefOf.PositiveEvent,false);return true;
  }
  public static Texture2D Icon(int index){return ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Azure/"+new[]{"AzureCrescent","BlueComet","ControlledPyre","AzurePhoenix","AzureEvolution"}[index]);}
  public static readonly string[] Names={"Azure Crescent","Blue Comet","Controlled Pyre","Azure Phoenix"};
  public static readonly string[] Descriptions={"A focused azure blade wave. Same base damage budget as Ember Cleave, with improved armor penetration; applies up to three Azure Brands.","Blink up to six clear tiles into melee, strike at 80% of Cinder Step's base damage and slow the survivor briefly. Cannot cross walls.","Create a contained six-tile pyre at your position for three seconds. Three pulses share one damage budget; it does not ignite terrain or buildings. Respects non-lethal mode and friendly-fire settings.","A focused azure phoenix strike and small burst. Stronger than the ordinary finisher, with 40% longer cooldown and a visible interruptible telegraph. Consumes up to three brands."};
  public static readonly HediffCompProperties_FlameAbilities Tuning=new HediffCompProperties_FlameAbilities {severRange=10,flashRange=6,danceRadius=6,heavenRange=12,severCooldown=300,flashCooldown=450,danceCooldown=750,heavenCooldown=1890,telegraphTicks=18,staggerTicks=60,fistFloor=10,explosionRadius=2.5f,severPhysical=1.2f,severThermal=1.1f,flashPhysical=1.2f,flashThermal=1.2f,dancePhysical=0,danceThermal=2.5f,heavenPhysical=6.6f,heavenThermal=4.4f,explosionThermal=1.65f};
 }
 public static class AzureRules {
  public static bool Ready(int kills,int ticks){return kills>=150&&ticks>=600000;}
  public static bool CountKill(bool hostile,bool animal,bool self){return hostile&&!animal&&!self;}
  public static float PulseShare(int pulses){return 1f/Math.Max(1,pulses);}
 }
 public sealed class HediffCompProperties_FlameMastery:HediffCompProperties {public HediffCompProperties_FlameMastery(){compClass=typeof(HediffComp_FlameMastery);}}
 public sealed class HediffComp_FlameMastery:HediffComp {
  public int kills;HashSet<int> victims=new HashSet<int>();
  public override void CompExposeData(){Scribe_Values.Look(ref kills,"EH_azureMasteryKills");Scribe_Collections.Look(ref victims,"EH_azureMasteryVictims",LookMode.Value);if(victims==null)victims=new HashSet<int>();}
  public override void Notify_KilledPawn(Pawn victim,DamageInfo? info){if(victim==null||AzureEvolution.Active(Pawn))return;RecordKill(victim.thingIDNumber,AzureRules.CountKill(victim.HostileTo(Pawn),victim.RaceProps.Animal,victim==Pawn));}
  internal void MakeEvolutionReady(){kills=Math.Max(kills,150);parent.ageTicks=Math.Max(parent.ageTicks,600000);}
  internal bool RecordKill(int id,bool eligible){if(!eligible||!victims.Add(id))return false;kills++;return true;}
  public override string CompTipStringExtra{get{return AzureEvolution.Active(Pawn)?"Azure Flame evolution complete.":"Azure mastery: "+kills+" / 150 hostile kills; "+(parent.ageTicks/60000f).ToString("0.0")+" / 10 ascendant days. Each hostile non-animal target counts once; disciple kills are excluded.";}}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ElementalCombatPolicy.Controlled(Pawn)||AzureEvolution.Active(Pawn))yield break;
   var cmd=new Command_Action{defaultLabel="Evolve: Azure Flame",defaultDesc=CompTipStringExtra+"\nEvolve through a dramatic 4.5-second ritual: gathering embers, blue flame spirals, a brief rise and a finishing shockwave. Grants a new power-boosting hediff, five Azure abilities, regal sleeveless armor, a bound sword and a floating blue flame halo. No helmet. Interruption spends nothing.",icon=AzureEvolution.Icon(4),groupable=false,action=()=>{if(AzureEvolution.Problem(Pawn)==null)Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_EvolveAzure"),Pawn),JobTag.Misc);}};
   string reason=AzureEvolution.Problem(Pawn);if(reason!=null)cmd.Disable(reason);yield return cmd;
   if(DebugSettings.godMode)yield return new DevEvolutionReadyCommand{isEligible=()=>!AzureEvolution.Active(Pawn)&&AzureEvolution.Mastery(Pawn)==this,defaultLabel="DEV: Evolution-Ready",defaultDesc="Unlock the evolution requirements for this Flame Ascendant only. God Mode only. The normal Evolve button still performs the meditation.",icon=AzureEvolution.Icon(4),groupable=false,action=()=>{if(DebugSettings.godMode)MakeEvolutionReady();}};
  }
  public override void CompPostTickInterval(ref float adjustment,int delta){if(Pawn.Spawned&&Pawn.CurJob!=null&&Pawn.CurJob.def.defName=="EH_EvolveAzure")EvolutionCinematic.Track(Pawn);}
 }
 public sealed class HediffCompProperties_AzureHalo:HediffCompProperties {public HediffCompProperties_AzureHalo(){compClass=typeof(HediffComp_AzureHalo);}}
 public sealed class HediffComp_AzureHalo:HediffComp {
  public override void CompPostPostAdd(DamageInfo? info){if(Pawn.Spawned)AzureVfx.Track(Pawn);}
  public override void Notify_Spawned(){AzureVfx.Track(Pawn);}
  public override void CompPostTickInterval(ref float adjustment,int delta){if(!AzureEvolution.HasFlame(Pawn)){Pawn.health.RemoveHediff(parent);return;}if(Pawn.Spawned)AzureVfx.Track(Pawn);}
 }
 public sealed class JobDriver_EvolveAzure:JobDriver {
  int ritualStartTick=-1,lastCue=-1;
  public int ElapsedAt(int tick){return ritualStartTick>=0?Math.Max(0,tick-ritualStartTick):Math.Max(0,EvolutionTimeline.Duration-ticksLeftThisToil);}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref ritualStartTick,"EH_evolutionRitualStart",-1);Scribe_Values.Look(ref lastCue,"EH_evolutionRitualCue",-1);}
  void TrackRitual(){
   int now=Find.TickManager.TicksGame;if(ritualStartTick<0)ritualStartTick=now-Math.Max(0,EvolutionTimeline.Duration-ticksLeftThisToil);
   pawn.pather.StopDead();pawn.Rotation=Rot4.South;EvolutionCinematic.Track(pawn);
   int cue=EvolutionTimeline.Cue(ElapsedAt(now));if(cue>lastCue){ElementalAudio.Play(pawn,cue==0?"EH_TransformStart":cue==1?"EH_TransformCharge":ElementalAudio.SoundName(pawn,ElementalSound.Impact));lastCue=cue;}
  }
  protected override IEnumerable<Toil> MakeNewToils(){
   this.FailOn(()=>!ElementalTeaching.Ready(pawn)||!EvolvedLineage.RitualValid(pawn)||ElementalAnimationBridge.Controlled(pawn));
   var wait=Toils_General.Wait(EvolutionTimeline.Duration);wait.handlingFacing=true;wait.WithProgressBarToilDelay(TargetIndex.A);wait.initAction=TrackRitual;wait.tickAction=TrackRitual;yield return wait;
   yield return Toils_General.Do(()=>{if(EvolvedLineage.Complete(pawn))EvolutionCinematic.Complete(pawn);});
  }
  public override bool TryMakePreToilReservations(bool errorOnFailed){return true;}

 }
}
