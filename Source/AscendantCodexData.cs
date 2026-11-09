using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ElementalHearts {
 public enum CodexTier {Otherworlder,Disciple,Ascendant,Evolved,Transcendant,Celestial}
 public sealed class Hediff_OtherworldOrigin:Hediff {
  public override bool Visible{get{return false;}}
  public override bool ShouldRemove{get{return false;}}
 }
 public static class CodexOrigin {
  public const string DefName="EH_OtherworldOrigin";
  public static bool Recorded(Pawn p){return p!=null&&p.health!=null&&p.health.hediffSet.hediffs.Any(h=>h.def.defName==DefName);}
  public static void Record(Pawn p){var def=DefDatabase<HediffDef>.GetNamedSilentFail(DefName);if(p!=null&&p.health!=null&&def!=null&&!Recorded(p))p.health.AddHediff(def);}
  static bool Uniform(Thing t){return t!=null&&(t.def.defName=="EH_OtherworldGakuran"||t.def.defName=="EH_OtherworldSailor");}
  public static bool Legacy(Pawn p){
   if(p==null||p.story==null||!p.story.traits.allTraits.Any(t=>Array.IndexOf(OtherworldIsekai.Identities,t.def.defName)>=0))return false;
   if(p.apparel!=null&&p.apparel.WornApparel.Any(a=>Uniform(a)))return true;
   if(p.inventory!=null&&p.inventory.innerContainer.Any(Uniform))return true;
   var transform=HediffComp_ElementalTransform.On(p);return transform!=null&&transform.PersonalApparel().Any(a=>Uniform(a));
  }
  public static bool Otherworlder(Pawn p){return Recorded(p)||Legacy(p);}
 }
 public sealed class CodexProgress {
  public string action,jobName,nextForm;public int kills,requiredKills,ageTicks,requiredTicks;public bool grantedReady,available;
  public bool Ready{get{return available&&(grantedReady||kills>=requiredKills&&ageTicks>=requiredTicks);}}
  public int KillsRemaining{get{return grantedReady?0:Math.Max(0,requiredKills-kills);}}
  public int TicksRemaining{get{return grantedReady?0:Math.Max(0,requiredTicks-ageTicks);}}
  public string Summary{get{return !available?"Progress unavailable":grantedReady?"Bestow unlocked":Ready?action+" ready":kills+" / "+requiredKills+" kills";}}
  public string Days{get{return grantedReady?"Milestones fulfilled":(Math.Floor(Math.Max(0,ageTicks)/6000d)/10d).ToString("0.0")+" / "+(requiredTicks/60000f).ToString("0")+" days";}}
  static CodexProgress Of(string action,string job,string next,int kills,Hediff root,int requiredKills,int ticks){return new CodexProgress{action=action,jobName=job,nextForm=next,kills=kills,ageTicks=root==null?0:root.ageTicks,requiredKills=requiredKills,requiredTicks=ticks,available=root!=null};}
  public static CodexProgress Read(Pawn p){
   var disciple=DiscipleAscension.Apprentice(p);if(disciple!=null&&!ElementalHeartUtility.HasHeart(p))return Of("Ascend",DiscipleAscension.JobName,disciple.DiscipleElement+" Ascendant",disciple.discipleKills,disciple.parent,DiscipleProgressRules.RequiredKills,DiscipleProgressRules.RequiredTicks);
   if(ThunderstormEvolution.Active(p)){var mastery=ThunderIncarnate.Mastery(p);if(mastery!=null){mastery.EnsureStarted();var next=Of("Transcend",ThunderIncarnate.Ritual,"Thunder Incarnate",mastery.kills,mastery.parent,ThunderIncarnate.Kills,ThunderIncarnate.Days);next.ageTicks=mastery.Age;return next;}}
   if(EvolvedLineage.Active(p))return null;
   var flame=AzureEvolution.Mastery(p);if(flame!=null)return Of("Evolve","EH_EvolveAzure","Azure Flame",flame.kills,flame.parent,150,600000);
   var storm=ThunderstormEvolution.Mastery(p);if(storm!=null)return Of("Evolve","EH_EvolveThunderstorm","Thunderstorm",storm.kills,storm.parent,150,600000);
   var glacial=GlacialEvolution.Mastery(p);if(glacial!=null)return Of("Evolve","EH_EvolveGlacial","Glacial",glacial.kills,glacial.parent,150,600000);
   var cyclone=CycloneEvolution.Mastery(p);if(cyclone!=null)return Of("Evolve","EH_EvolveCyclone","Cyclone",cyclone.kills,cyclone.parent,150,600000);
   var adamant=AdamantEvolution.Mastery(p);if(adamant!=null)return Of("Evolve","EH_EvolveAdamant","Adamant",adamant.kills,adamant.parent,150,600000);
   var lunar=HediffComp_LunarAbilities.On(p);if(lunar!=null){var mastery=EclipseAscendant.Mastery(p);var result=Of("Evolve",EclipseAscendant.Evolution,"Eclipse",mastery==null?0:mastery.kills,mastery==null?lunar.parent:mastery.parent,150,600000);result.grantedReady=lunar.eclipseReady;return result;}
   return ElementalHeartUtility.HasHeart(p)?new CodexProgress{action="Evolve",available=false}:null;
  }
  public static string Problem(Pawn p){
   var progress=Read(p);if(progress==null)return "This form has no further ascension.";string common=CodexOrders.OrderProblem(p);if(common!=null)return common;
   if(progress.action=="Ascend")return DiscipleAscension.Problem(p);
   switch(progress.jobName){case ThunderIncarnate.Ritual:return ThunderIncarnate.Problem(p);case "EH_EvolveAzure":return AzureEvolution.Problem(p);case "EH_EvolveThunderstorm":return ThunderstormEvolution.Problem(p);case "EH_EvolveGlacial":return GlacialEvolution.Problem(p);case "EH_EvolveCyclone":return CycloneEvolution.Problem(p);case "EH_EvolveAdamant":return AdamantEvolution.Problem(p);case EclipseAscendant.Evolution:return EclipseAscendant.Problem(p);default:return "Progression data is unavailable.";}
  }
  public static bool Begin(Pawn p){if(Problem(p)!=null)return false;var progress=Read(p);if(progress.action=="Ascend")return DiscipleAscension.Begin(p);return p.jobs.TryTakeOrderedJob(Verse.JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(progress.jobName),p),Verse.AI.JobTag.Misc);}
 }
 public sealed class CodexEntry {
  public Pawn pawn;public CodexTier tier;public string name,element,form,location,status;public bool origin,legacyOrigin;public CodexProgress progress;public ElementalAutoMode? mode;
  public static CodexEntry Read(Pawn p){
   bool heart=ElementalHeartUtility.HasHeart(p);var disciple=DiscipleAscension.Apprentice(p);bool origin=CodexOrigin.Recorded(p),legacy=!origin&&CodexOrigin.Legacy(p);
   if(!heart&&disciple==null&&!origin&&!legacy)return null;
   string element=heart?ElementalTeaching.Element(p):disciple==null?null:disciple.DiscipleElement;string form=heart?EvolvedLineage.Equipment(p,element):element;
   return new CodexEntry{pawn=p,name=p.LabelShortCap,element=element,form=form=="ThunderIncarnate"?"Thunder Incarnate":form=="AzureFlame"?"Azure Flame":form??"Unbestowed",tier=ThunderIncarnate.Active(p)?CodexTier.Transcendant:CelestialAscendant.Active(p)?CodexTier.Celestial:EvolvedLineage.Active(p)?CodexTier.Evolved:heart?CodexTier.Ascendant:disciple!=null?CodexTier.Disciple:CodexTier.Otherworlder,origin=origin||legacy,legacyOrigin=legacy,progress=CodexProgress.Read(p),mode=CodexOrders.Mode(p),location=Location(p),status=Status(p)};
  }
  public static string Location(Pawn p){
   if(p.MapHeld!=null)return (p.Spawned?"":"Held · ")+p.MapHeld.Parent.LabelCap;
   var caravan=p.GetCaravan();if(caravan!=null)return "Caravan · "+caravan.LabelCap;
   foreach(var w in Find.WorldObjects.AllWorldObjects){var flight=w as WorldObject_CelestialLightTravel;if(flight!=null&&flight.Container.Contains(p))return "Light Travel";}
   var calling=OtherworldCalling.State;if(calling!=null&&calling.Records.Any(r=>r.committed&&r.person==p))return "Arrival pending";
   return "Off map";
  }
  public static string Status(Pawn p){if(p.Dead)return "Dead";if(p.Downed)return "Downed";if(p.CurJobDef!=null&&p.CurJobDef.defName==ThunderIncarnate.Ritual)return "Transcending";if(!p.Spawned)return "Off map";if(p.InMentalState)return "Mental state";if(HediffComp_ElementalTransform.Transforming(p))return "Transforming";if(AzureEvolution.Evolving(p)||DiscipleAscension.Busy(p))return "Ascending";if(ElementalTeachingSession.Busy(p)||ElementalBestow.Busy(p))return "Teaching / Bestow";if(OtherworldCalling.Busy(p))return "Calling";if(CodexOrders.Busy(p))return "Using technique";if(!p.Awake())return "Asleep";return HediffComp_ElementalTransform.Active(p)?"Transformed":"Normal clothes";}
 }
 public static class AscendantCodex {
  public static bool Owned(Pawn p){return p!=null&&!p.Destroyed&&!p.Dead&&p.RaceProps.Humanlike&&p.Faction==Faction.OfPlayer&&!p.IsPrisoner;}
  public static List<CodexEntry> Gather(){
   var pawns=new HashSet<Pawn>(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction);
   foreach(var obj in Find.WorldObjects.AllWorldObjects){var flight=obj as WorldObject_CelestialLightTravel;if(flight!=null)foreach(var t in flight.Container){var pawn=t as Pawn;if(pawn!=null)pawns.Add(pawn);}}
   var calls=OtherworldCalling.State;if(calls!=null)foreach(var r in calls.Records)if(r.committed&&r.person!=null)pawns.Add(r.person);
   var result=new List<CodexEntry>();foreach(var p in pawns)if(Owned(p)){var entry=CodexEntry.Read(p);if(entry!=null)result.Add(entry);}result.Sort((a,b)=>string.Compare(a.name,b.name,StringComparison.CurrentCultureIgnoreCase));return result;
  }
 }
}
