using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ElementalHearts {
 // Optional cached bridge: no ISEKAI assembly is required to load Elemental Hearts.
 public static class OtherworldIsekai {
  public static readonly string[] Identities={"Isekai_SummonedHero","Isekai_Protagonist","Isekai_Regressor"};
  public static readonly string[] Talents={"Isekai_NaturalTalent","Isekai_Prodigy","Isekai_PowerSpike","Isekai_PredatorInstinct","Isekai_Undying","Isekai_Lucky","Isekai_CraftsmanSoul","Isekai_HealerTouch"};
  static bool initialized;static MethodInfo grant,remove,isTrait,getComp;static string problem;
  static void Init(){if(initialized)return;initialized=true;
   var helper=AccessTools.TypeByName("IsekaiLeveling.IsekaiTraitHelper");var component=AccessTools.TypeByName("IsekaiLeveling.IsekaiComponent");
   if(helper==null||component==null){problem="Otherworld Calling requires ISEKAI RPG LEVELING.";return;}
   grant=AccessTools.Method(helper,"AddIsekaiTrait",new[]{typeof(Pawn),typeof(string)});remove=AccessTools.Method(helper,"RemoveIsekaiTrait",new[]{typeof(Pawn),typeof(string)});isTrait=AccessTools.Method(helper,"IsIsekaiTrait",new[]{typeof(string)});getComp=AccessTools.Method(component,"GetCached",new[]{typeof(Pawn)});
   if(grant==null||grant.ReturnType!=typeof(bool)||remove==null||isTrait==null||getComp==null){problem="The installed ISEKAI RPG LEVELING version does not expose the required compatibility methods.";return;}
   foreach(string name in Identities.Concat(Talents))if(DefDatabase<TraitDef>.GetNamedSilentFail(name)==null){problem="ISEKAI trait definition unavailable: "+name;return;}
  }
  public static string Problem{get{Init();return problem;}}
  public static bool Compatible(TraitDef a,TraitDef b){return a!=b&&(a.conflictingTraits==null||!a.conflictingTraits.Contains(b))&&(b.conflictingTraits==null||!b.conflictingTraits.Contains(a));}
  static bool Immediate(string trait){return Array.IndexOf(Talents,trait)>=3;}
  public static List<string[]> Bundles(Pawn p){
   var result=new List<string[]>();foreach(string identity in Identities)for(int a=0;a<Talents.Length;a++)for(int b=a+1;b<Talents.Length;b++){
    if(!Immediate(Talents[a])&&!Immediate(Talents[b]))continue;
    var names=new[]{identity,Talents[a],Talents[b]};var defs=names.Select(n=>DefDatabase<TraitDef>.GetNamed(n)).ToArray();bool ok=true;
    for(int i=0;i<defs.Length;i++){for(int j=i+1;j<defs.Length;j++)if(!Compatible(defs[i],defs[j]))ok=false;if(p!=null)foreach(var old in p.story.traits.allTraits)if(!(bool)isTrait.Invoke(null,new object[]{old.def.defName})&&!Compatible(defs[i],old.def))ok=false;}
    if(ok)result.Add(names);
   }return result;
  }
  public static Pawn Generate(OtherworldCallRecord record){
   if(Problem!=null)throw new InvalidOperationException(Problem);Pawn p=null;
   try{
    Gender gender=Rand.Bool?Gender.Male:Gender.Female;int age=Rand.RangeInclusive(18,24);
    p=PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist,Faction.OfPlayer,forceGenerateNewPawn:true,canGeneratePawnRelations:false,mustBeCapableOfViolence:true,forceNoGear:true,allowDowned:false,fixedGender:gender,fixedBiologicalAge:age,fixedChronologicalAge:age,forcedXenotype:ModsConfig.BiotechActive?XenotypeDefOf.Baseliner:null,developmentalStages:DevelopmentalStage.Adult));
    if(p==null||p.def!=ThingDefOf.Human||p.story==null||p.health==null||p.Dead||p.Downed||p.RaceProps.IsMechanoid||!p.RaceProps.Humanlike||getComp.Invoke(null,new object[]{p})==null)throw new InvalidOperationException("A healthy human with ISEKAI progression could not be prepared.");
    if(ElementalHeartUtility.HasHeart(p)||ElementalTeaching.Trained(p))throw new InvalidOperationException("The generated traveler unexpectedly already has an elemental lineage.");
    p.gender=gender;p.story.bodyType=Rand.Chance(.30f)?BodyTypeDefOf.Thin:gender==Gender.Female?BodyTypeDefOf.Female:BodyTypeDefOf.Male;
    // Preserve ordinary personality traits; replace only the new pawn's randomly rolled ISEKAI traits.
    foreach(var trait in p.story.traits.allTraits.ToList())if((bool)isTrait.Invoke(null,new object[]{trait.def.defName}))remove.Invoke(null,new object[]{p,trait.def.defName});
    var candidates=Bundles(p);if(candidates.Count==0)throw new InvalidOperationException("No compatible strong ISEKAI bundle is available.");
    int roll=Rand.Range(0,100);string wanted=Identities[roll<60?0:roll<85?1:2];var weighted=candidates.Where(b=>b[0]==wanted).ToList();if(weighted.Count==0)weighted=candidates;
    var chosen=weighted[Rand.Range(0,weighted.Count)];record.identity=chosen[0];record.talents=new List<string>{chosen[1],chosen[2]};
    foreach(var name in chosen)if(!(bool)grant.Invoke(null,new object[]{p,name})||!p.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed(name)))throw new InvalidOperationException("ISEKAI did not grant "+name+".");
    if(Identities.Count(n=>p.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed(n)))!=1)throw new InvalidOperationException("The traveler has incompatible ISEKAI identities.");
    foreach(var apparel in p.apparel.WornApparel.ToList())apparel.Destroy();foreach(var weapon in p.equipment.AllEquipmentListForReading.ToList())weapon.Destroy();
    var def=DefDatabase<ThingDef>.GetNamed(gender==Gender.Female?"EH_OtherworldSailor":"EH_OtherworldGakuran");var uniform=(Apparel)ThingMaker.MakeThing(def,ThingDefOf.Cloth);p.apparel.Wear(uniform,false,false);if(p.outfits!=null)p.outfits.forcedHandler.SetForced(uniform,true);
    if(p.needs.food!=null)p.needs.food.CurLevelPercentage=1;if(p.needs.rest!=null)p.needs.rest.CurLevelPercentage=1;
    CodexOrigin.Record(p);return p;
   }catch{if(p!=null&&!p.Destroyed)p.Destroy(DestroyMode.Vanish);throw;}
  }
 }
}

