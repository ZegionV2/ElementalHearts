using System.Collections.Generic;
using RimWorld;
using Verse;
namespace ElementalHearts {
 public static class ElementalAscension {
  public static AcceptanceReport CanReceive(Pawn p){
   if(p==null||p.Dead||!p.RaceProps.Humanlike)return "Only living humanlike pawns can receive ascension serum.";
   if(ElementalHeartUtility.HasHeart(p))return "Already an Elemental Ascendant. Only one element is allowed.";
   return true;
  }
  public static bool Apply(Pawn p,HediffDef def){
   if(!CanReceive(p).Accepted||def==null)return false;
   var oldEarth=HediffComp_EarthAbilities.On(p);
   var oldWater=HediffComp_WaterDefense.On(p);var oldSkills=HediffComp_WaterAbilities.On(p);
   ElementalBoundSwords.BeforeAscension(p);
   // Replace discipleship before adding the full condition; no two sets of abilities coexist.
   for(int i=p.health.hediffSet.hediffs.Count-1;i>=0;i--){var h=p.health.hediffSet.hediffs[i];if(h.TryGetComp<HediffComp_ElementalApprentice>()!=null)p.health.RemoveHediff(h);}
   var ascension=HediffMaker.MakeHediff(def,p,null);ascension.Part=null;p.health.AddHediff(ascension);
   if(def.defName==WaterKitUpgrade.DefName){if(oldWater!=null)oldWater.CopyStateTo(ascension.TryGetComp<HediffComp_WaterDefense>());var skills=ascension.TryGetComp<HediffComp_WaterAbilities>();if(oldSkills!=null&&skills!=null){skills.readyAt=new List<int>(oldSkills.readyAt);skills.nextAutoCheck=oldSkills.nextAutoCheck;skills.comboStep=oldSkills.comboStep;skills.comboUntil=oldSkills.comboUntil;}}
   NpcDisciples.EnableAuto(p);
   if(def.defName=="EH_EarthHeart"&&oldEarth!=null)oldEarth.CopyStateTo(ascension.TryGetComp<HediffComp_EarthAbilities>());
   return true;
  }
  public static bool Normalize(Hediff h){if(h==null||!ElementalTeaching.IsHeart(h)||h.Part==null)return false;h.Part=null;if(h.pawn!=null&&h.pawn.health!=null)h.pawn.health.Notify_HediffChanged(h);return true;}
 }
 public sealed class CompProperties_ElementalSerum:CompProperties_UseEffect {
  public HediffDef hediff;
  public CompProperties_ElementalSerum(){compClass=typeof(CompUseEffect_ElementalSerum);}
 }
 public sealed class CompUseEffect_ElementalSerum:CompUseEffect {
  public CompProperties_ElementalSerum Props{get{return (CompProperties_ElementalSerum)props;}}
  public override AcceptanceReport CanBeUsedBy(Pawn p){return ElementalAscension.CanReceive(p);}
  public override void DoEffect(Pawn p){
   if(parent.Destroyed||parent.stackCount<1||!ElementalAscension.Apply(p,Props.hediff))return;
   // Consume exactly one dose only after successfully granting ascension.
   parent.SplitOff(1).Destroy(DestroyMode.Vanish);
   if(p.Spawned)LightningAudio.Play(p,"MechSerumUsed");
   Messages.Message(p.LabelShort+" has become a "+Props.hediff.label+".",p,MessageTypeDefOf.PositiveEvent);
  }
 }
 public sealed class Recipe_AdministerElementalSerum:Recipe_Surgery {
  public override bool AvailableOnNow(Thing thing,BodyPartRecord part=null){return ElementalAscension.CanReceive(thing as Pawn).Accepted&&base.AvailableOnNow(thing,part);}
  public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn,RecipeDef recipe){if(AvailableOnNow(pawn))yield return null;}
  public override void ApplyOnPawn(Pawn pawn,BodyPartRecord part,Pawn billDoer,List<Thing> ingredients,Bill bill){
   if(!ElementalAscension.CanReceive(pawn).Accepted)return;
   foreach(var item in ingredients){var c=item.TryGetComp<CompUseEffect_ElementalSerum>();if(c!=null&&c.Props.hediff==recipe.addsHediff){c.DoEffect(pawn);return;}}
  }
  // The use-effect consumes its own dose. Invalid/repeated administrations do not waste it.
  public override void ConsumeIngredient(Thing ingredient,RecipeDef recipe,Map map){}
 }
 // Retained def IDs keep old saved bills resolvable, but extraction is no longer an operation.
 public sealed class Recipe_ObsoleteElementalExtraction:Recipe_Surgery {
  public override bool AvailableOnNow(Thing thing,BodyPartRecord part=null){return false;}
  public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn,RecipeDef recipe){yield break;}
  public override void ApplyOnPawn(Pawn pawn,BodyPartRecord part,Pawn billDoer,List<Thing> ingredients,Bill bill){}
  public override void ConsumeIngredient(Thing ingredient,RecipeDef recipe,Map map){}
 }
 public sealed class HediffCompProperties_ElementalAscension:HediffCompProperties {public HediffCompProperties_ElementalAscension(){compClass=typeof(HediffComp_ElementalAscension);}}
 public sealed class HediffComp_ElementalAscension:HediffComp {
  public override void CompPostPostAdd(DamageInfo? dinfo){ElementalAscension.Normalize(parent);AscendantBenefits.Cleanse(Pawn);}
  public override void Notify_Spawned(){ElementalAscension.Normalize(parent);}
 }
 public sealed class GameComponent_ElementalAscensionMigration:GameComponent {
  public GameComponent_ElementalAscensionMigration(Game game){}
  public override void FinalizeInit(){
   int count=0;
   foreach(var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){if(p.health==null)continue;foreach(var h in p.health.hediffSet.hediffs)if(ElementalAscension.Normalize(h))count++;AscendantBenefits.Cleanse(p);}
   if(count>0)Log.Message("[Elemental Hearts] Migrated "+count+" existing elemental conditions to whole-body ascension; ability state preserved.");
  }
 }
}
