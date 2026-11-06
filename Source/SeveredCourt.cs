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
    var scope=CourtRaidScope.Current;bool commander=scope!=null?scope.herald:!parms.inhabitants&&!parms.forceOneDowned&&parms.groupKind==PawnGroupKindDefOf.Combat&&CourtHeralds.Eligible(parms.points,Find.TickManager.TicksGame)&&Rand.Chance(CourtHeralds.RaidChance);
    int element=scope==null?Rand.Range(0,5):scope.element;if(scope==null&&commander){CourtStronghold origin;commander=CourtStrongholds.TryCommander(out element,out origin);}
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
 public sealed class WorldComponent_SeveredCourt:WorldComponent {
  bool introduced;int strongholdMask,strongholdVersion,redemptionVersion;
  public bool StrongholdsSeeded{get{return strongholdVersion>=1;}}
  public WorldComponent_SeveredCourt(World world):base(world){}
  public override void ExposeData(){Scribe_Values.Look(ref introduced,"EH_courtIntroduced");Scribe_Values.Look(ref strongholdMask,"EH_courtStrongholdMask");Scribe_Values.Look(ref strongholdVersion,"EH_courtStrongholdVersion");Scribe_Values.Look(ref redemptionVersion,"EH_courtRedemptionVersion");}
  public override void FinalizeInit(bool fromLoad){
   base.FinalizeInit(fromLoad);if(!fromLoad)introduced=true;
   LongEventHandler.ExecuteWhenFinished(()=>{
    if(redemptionVersion<1){CourtRedemption.ExistingColonists();redemptionVersion=1;}
    if(fromLoad&&!introduced&&ElementalHeartsMod.Settings!=null&&ElementalHeartsMod.Settings.addCourtToExistingSaves)EnsureIntroduced();
    if(!fromLoad||ElementalHeartsMod.Settings!=null&&ElementalHeartsMod.Settings.addCourtStrongholdsToExistingSaves)EnsureStrongholds(false);
   });
  }
  Faction Existing(){foreach(var f in Find.FactionManager.AllFactionsListForReading)if(SeveredCourt.IsFaction(f))return f;return null;}
  public void EnsureIntroduced(){
   if(introduced)return;if(Existing()!=null){introduced=true;return;}CreateCourt();introduced=true;
  }
  Faction CreateCourt(){
   var def=DefDatabase<FactionDef>.GetNamedSilentFail(SeveredCourt.DefName);if(def==null)return null;
   var faction=FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));Find.FactionManager.Add(faction);
   // RimWorld's native factory already creates a first settlement for a visible faction.
   foreach(var existing in Find.WorldObjects.Settlements)if(existing.Faction==faction){existing.Name="The Sundered Seat";return faction;}
   var tile=TileFinder.RandomSettlementTileFor(faction,true,t=>!Find.WorldObjects.AnyWorldObjectAt(t));
   if(tile.Valid){var home=(Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);home.SetFaction(faction);home.Tile=tile;home.Name="The Sundered Seat";Find.WorldObjects.Add(home);}
   return faction;
  }
  public int EnsureStrongholds(bool manual){
   var faction=Existing();if(faction==null&&manual){faction=CreateCourt();introduced=true;}
   if(faction==null||faction.defeated||strongholdVersion>=1)return 0;
   var def=DefDatabase<WorldObjectDef>.GetNamedSilentFail(CourtStrongholds.DefName);if(def==null)return 0;int added=0;
   foreach(var obj in Find.WorldObjects.AllWorldObjects){var hold=obj as CourtStronghold;if(hold!=null&&hold.Faction==faction)strongholdMask|=1<<hold.Element;}
   for(int element=0;element<5;element++){
    if((strongholdMask&(1<<element))!=0)continue;
    var tile=TileFinder.RandomSettlementTileFor(faction,true,t=>!Find.WorldObjects.AnyWorldObjectAt(t));if(!tile.Valid)continue;
    var hold=(CourtStronghold)WorldObjectMaker.MakeWorldObject(def);hold.element=element;hold.SetFaction(faction);hold.Tile=tile;hold.Name=CourtStrongholds.Names[element];Find.WorldObjects.Add(hold);strongholdMask|=1<<element;added++;
   }
   if(strongholdMask==31)strongholdVersion=1;
   if(added>0){var target=Find.WorldObjects.AllWorldObjects.Find(o=>o is CourtStronghold&&o.Faction==faction);Find.LetterStack.ReceiveLetter("Court strongholds established",added+" Court stronghold"+(added==1?" has":"s have")+" been located on the world map. Each is ruled by a Herald and defended by Knights and Oathlings. Select a stronghold to inspect its lineage, then send a caravan to attack when prepared.\n\nExisting settlements have been preserved. Cleared strongholds will not be rebuilt when you load the game.",LetterDefOf.NeutralEvent,target);}
   return added;
  }
 }
}
