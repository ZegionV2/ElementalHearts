using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace ElementalHearts {
 public sealed class CourtSoldier:DefModExtension {public string element;public bool ascendant,herald;}
 public static class SeveredCourt {
  public const string DefName="EH_SeveredCourt";
  public static readonly string[] Elements={"Flame","Lightning","Water","Wind","Earth"};
  public static bool IsFaction(Faction f){return f!=null&&f.def.defName==DefName;}
  // The native factory invokes this once per new pawn. This route excludes the generic 20% roll.
  public static bool OnGenerated(Pawn p){
   if(p==null)return false;var role=p.kindDef==null?null:p.kindDef.GetModExtension<CourtSoldier>();if(role==null&&!IsFaction(p.Faction))return false;
   if(p.RaceProps==null||!p.RaceProps.Humanlike||p.RaceProps.IsMechanoid||p.story==null||p.health==null)return true;
   var oath=DefDatabase<TraitDef>.GetNamed(CourtAppearance.TraitName);if(p.Faction==Faction.OfPlayer)CourtRedemption.Apply(p);else if(!p.story.traits.HasTrait(oath))p.story.traits.GainTrait(new Trait(oath));
   if(!ElementalHeartUtility.HasHeart(p)&&!ElementalTeaching.Trained(p)){
    string element=role!=null&&Array.IndexOf(Elements,role.element)>=0?role.element:Elements[Rand.Range(0,Elements.Length)];
    if(role!=null&&role.ascendant)ElementalAscension.Apply(p,DefDatabase<HediffDef>.GetNamed(role.herald?CourtHeralds.Condition(element):"EH_"+element+"Heart"));else NpcDisciples.Grant(p,element);
   }
   NpcDisciples.EnableAuto(p);return true;
  }
 }
 public static class CourtRaidRules {
  public const int DiscipleCost=600,KnightCost=2400,MaxPawns=16;
  // A knight requires a supporting pair of disciples. No generated group exceeds its budget.
  public static List<bool> Plan(float points){
   var result=new List<bool>(MaxPawns);if(float.IsNaN(points)||float.IsInfinity(points)||points<DiscipleCost)return result;
   float budget=Math.Min(points,1000000);int cap=budget>=20000?3:budget>=10000?2:budget>=3600?1:0;
   for(int i=0;i<cap&&budget>=KnightCost+2*DiscipleCost;i++){result.Add(true);budget-=KnightCost;}
   while(result.Count<MaxPawns&&budget>=DiscipleCost){result.Add(false);budget-=DiscipleCost;}return result;
  }
  public static void Generate(PawnGroupMakerParms parms,List<Pawn> result){
   bool seeded=parms.seed.HasValue;if(seeded)Rand.PushState(parms.seed.Value);
   try{
    var scope=CourtRaidScope.Current;bool commander=scope!=null?scope.herald:!parms.inhabitants&&!parms.forceOneDowned&&parms.groupKind==PawnGroupKindDefOf.Combat&&(CourtRaidBudgetContext.Current!=null&&CourtRaidBudgetContext.Current.maximum||CourtHeralds.Guaranteed(parms.points)||CourtHeralds.Eligible(parms.points,Find.TickManager.TicksGame)&&Rand.Chance(CourtHeralds.RaidChance));
    int element=scope==null?Rand.Range(0,5):scope.element;if(scope==null&&commander){Settlement origin;commander=CourtStrongholds.TryBaseCommander(out element,out origin);}
    var plan=CourtHeralds.Plan(parms.points,commander);
    foreach(var rank in plan){string name="EH_Court"+(rank==CourtRank.Herald?"Herald":rank==CourtRank.Knight?"Knight":"Oathling")+SeveredCourt.Elements[element];var kind=DefDatabase<PawnKindDef>.GetNamed(name);var request=new PawnGenerationRequest(kind,parms.faction,PawnGenerationContext.NonPlayer,parms.tile,forceGenerateNewPawn:true,inhabitant:parms.inhabitants,mustBeCapableOfViolence:true,canGeneratePawnRelations:true,allowFood:true,fixedIdeo:parms.ideo);var pawn=PawnGenerator.GeneratePawn(request);if(parms.forceOneDowned&&result.Count==0){pawn.health.forceDowned=true;if(pawn.guest!=null)pawn.guest.Recruitable=true;pawn.mindState.canFleeIndividual=false;}result.Add(pawn);element=(element+1)%5;}
   }finally{if(seeded)Rand.PopState();}
  }
 }
 // Keep the native group's relationship registry and exception/partial-group cleanup wrapper.
 [HarmonyPatch(typeof(PawnGroupKindWorker_Normal),"GeneratePawns")]
 public static class CourtPawnGroups {
  public static bool Prefix(PawnGroupMakerParms parms,List<Pawn> outPawns){if(!SeveredCourt.IsFaction(parms.faction)||(parms.groupKind!=PawnGroupKindDefOf.Combat&&parms.groupKind!=PawnGroupKindDefOf.Settlement))return true;CourtRaidRules.Generate(parms,outPawns);return false;}
 }
 [HarmonyPatch(typeof(PawnGroupMaker),"CanGenerateFrom")]
 public static class CourtRaidSwitch {
  public static bool Prefix(PawnGroupMakerParms parms,ref bool __result){if(SeveredCourt.IsFaction(parms.faction)&&parms.groupKind==PawnGroupKindDefOf.Combat&&!parms.inhabitants&&ElementalHeartsMod.Settings!=null&&!ElementalHeartsMod.Settings.courtRaids){__result=false;return false;}return true;}
 }
}
