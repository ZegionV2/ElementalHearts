using System;using System.Collections.Generic;using System.Reflection;using HarmonyLib;using RimWorld;using Verse;
namespace ElementalHearts {
 public sealed class AwakeningHit {
  public Pawn pawn;public DamageInfo damage;public HediffComp_ElementalApprentice disciple;public HashSet<BodyPartRecord> missingBefore=new HashSet<BodyPartRecord>();public AwakeningHit previous;public bool queued,committing,finished;
 }
 public static class DiscipleAwakening {
  [ThreadStatic]static AwakeningHit current;
  static readonly MethodInfo shouldDie=AccessTools.Method(typeof(Pawn_HealthTracker),"ShouldBeDead"),shouldDown=AccessTools.Method(typeof(Pawn_HealthTracker),"ShouldBeDowned"),check=AccessTools.Method(typeof(Pawn_HealthTracker),"CheckForStateChange");
  public static HediffComp_ElementalApprentice On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_ElementalApprentice>();if(c!=null)return c;}return null;}
  public static bool Combat(Pawn p,DamageInfo damage){return p!=null&&p.Spawned&&!p.Dead&&p.RaceProps.Humanlike&&!p.RaceProps.IsMechanoid&&p.DevelopmentalStage.Adult()&&!p.IsPrisoner&&!p.IsSlave&&!p.WorkTagIsDisabled(WorkTags.Violent)&&damage.Amount>0&&damage.Def!=null&&damage.Def.ExternalViolenceFor(p)&&damage.Instigator!=null&&damage.Instigator!=p&&damage.Instigator.HostileTo(p);}
  public static AwakeningHit Enter(Thing thing,DamageInfo damage){var p=thing as Pawn;var disciple=On(p);if(disciple==null||disciple.awakeningRolled||!Combat(p,damage))return null;var hit=new AwakeningHit{pawn=p,damage=damage,disciple=disciple,previous=current};foreach(var h in p.health.hediffSet.hediffs){var m=h as Hediff_MissingPart;if(m!=null)hit.missingBefore.Add(m.Part);}current=hit;return hit;}
  public static bool HoldState(Pawn p,bool killing){var hit=current;if(hit==null||hit.pawn!=p)return false;if(hit.queued||hit.committing)return true;bool critical=killing||p.health.summaryHealth.SummaryHealthPercent<=NpcDiscipleRules.CriticalHealth||(bool)shouldDie.Invoke(p.health,null)||(bool)shouldDown.Invoke(p.health,null);if(!NpcDiscipleRules.ShouldRoll(On(p)==hit.disciple,hit.disciple.awakeningRolled,!p.Dead,Combat(p,hit.damage),critical))return false;hit.disciple.awakeningRolled=true;if(!Rand.Chance(NpcDiscipleRules.AwakeningChance))return false;hit.queued=true;return true;}
  public static Exception Finish(AwakeningHit hit,Exception error){if(hit==null||hit.finished)return error;hit.finished=true;bool held=hit.queued,awakened=false;try{if(hit.queued&&!hit.pawn.Dead){hit.committing=true;awakened=Awaken(hit);}}catch(Exception e){Log.Error("[Elemental Hearts] Awakening failed: "+e);}finally{hit.committing=false;hit.queued=false;current=hit.previous;}
   // Re-evaluate once after the atomic injury repair/promotion; no lasting death/downing bypass.
   if(held&&!hit.pawn.Dead)check.Invoke(hit.pawn.health,new object[]{null,null});if(awakened&&hit.pawn.Spawned){var p=hit.pawn;RestUtility.WakeUp(p,true);if(p.jobs!=null&&p.CurJob!=null&&p.CurJob.def.defName.StartsWith("EH_"))p.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced,false);var c=HediffComp_ElementalTransform.On(p);if(c!=null)c.ManifestInstant();p.Map.GetComponent<MapComponent_AwakeningBurst>().Enqueue(p,hit.disciple.DiscipleElement);Messages.Message(p.LabelShort+" has awakened as a "+hit.disciple.DiscipleElement+" Ascendant!",p,ElementalCombatPolicy.Controlled(p)?MessageTypeDefOf.PositiveEvent:MessageTypeDefOf.NeutralEvent,false);}return error;
  }
  static bool Awaken(AwakeningHit hit){var p=hit.pawn;var def=DefDatabase<HediffDef>.GetNamedSilentFail("EH_"+hit.disciple.DiscipleElement+"Heart");if(def==null)return false;
   // Repair only injuries and parts lost in this hit; retain older missing parts, scars and illnesses.
   foreach(var h in new List<Hediff>(p.health.hediffSet.hediffs)){var missing=h as Hediff_MissingPart;if(h is Hediff_Injury&&!((Hediff_Injury)h).IsPermanent()||h.def==HediffDefOf.BloodLoss||missing!=null&&!hit.missingBefore.Contains(missing.Part))p.health.RemoveHediff(h);}
   var l=HediffComp_LightningAbilities.On(p);var f=HediffComp_FlameAbilities.On(p);var w=HediffComp_WindAbilities.On(p);var water=HediffComp_WaterDefense.On(p);bool auto=l!=null?l.autoCombat:f!=null?f.autoCombat:w!=null?w.autoCombat:water!=null&&water.autoCombat,nonLethal=ElementalNonLethal.Enabled(p);
   if(!ElementalAscension.Apply(p,def))return false;auto=true;l=HediffComp_LightningAbilities.On(p);f=HediffComp_FlameAbilities.On(p);w=HediffComp_WindAbilities.On(p);water=HediffComp_WaterDefense.On(p);if(l!=null){l.autoCombat=auto;l.nonLethal=nonLethal;}if(f!=null){f.autoCombat=auto;f.nonLethal=nonLethal;}if(w!=null){w.autoCombat=auto;w.nonLethal=nonLethal;}if(water!=null){water.autoCombat=auto;water.nonLethal=nonLethal;}
   var earth=HediffComp_EarthAbilities.On(p);if(earth!=null)earth.nonLethal=nonLethal;
   p.health.AddHediff(DefDatabase<HediffDef>.GetNamed("EH_AwakeningGrace"));if(p.stances!=null)p.stances.stunner.StopStun();return true;
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class AwakeningDamageScope {
  [HarmonyPriority(Priority.First+100)]public static void Prefix(Thing __instance,DamageInfo dinfo,out AwakeningHit __state){__state=DiscipleAwakening.Enter(__instance,dinfo);}
  public static Exception Finalizer(Exception __exception,AwakeningHit __state){return DiscipleAwakening.Finish(__state,__exception);}
 }
 [HarmonyPatch(typeof(Pawn_HealthTracker),"CheckForStateChange")]
 public static class AwakeningHealthState {public static bool Prefix(Pawn ___pawn){return !DiscipleAwakening.HoldState(___pawn,false);}}
 [HarmonyPatch(typeof(Pawn),"Kill")]
 public static class AwakeningFatalHit { [HarmonyPriority(Priority.First+100)]public static bool Prefix(Pawn __instance){return !DiscipleAwakening.HoldState(__instance,true);}}
}
