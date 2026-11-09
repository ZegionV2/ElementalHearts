using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
namespace ElementalHearts {
 // Ordinary native Court settlements keep their world-object identity. Their persistent
 // fort state lives beside the old CourtStronghold fields so old references remain valid.
 public sealed class CourtBaseRecord:IExposable,IThingHolder {
  public Settlement settlement;public int element,layoutVersion;public bool garrisonGenerated,provisionsGenerated,intelRevealed;
  ThingOwner<Thing> reservedLoot;List<Thing> provisionRefs=new List<Thing>();
  public CourtBaseRecord(){}public CourtBaseRecord(Settlement owner){settlement=owner;element=owner.ID%5;}
  public void AdoptExistingMap(){if(settlement.HasMap&&!garrisonGenerated){garrisonGenerated=provisionsGenerated=true;settlement.previouslyGeneratedInhabitants=settlement.Map.mapPawns.AllPawnsSpawned.Where(p=>p.Faction==settlement.Faction).ToList();}}
  ThingOwner<Thing> Loot{get{return reservedLoot??(reservedLoot=new ThingOwner<Thing>(this,false,LookMode.Deep));}}
  public IThingHolder ParentHolder{get{return settlement;}}
  public ThingOwner GetDirectlyHeldThings(){return Loot;}
  public void GetChildHolders(List<IThingHolder> children){ThingOwnerUtility.AppendThingHoldersFromThings(children,Loot);}
  public void Track(Thing t){if(t!=null&&!t.Destroyed&&provisionRefs.Count<32&&!provisionRefs.Contains(t))provisionRefs.Add(t);}
  public void Store(Map map){foreach(var t in provisionRefs)if(t!=null&&!t.Destroyed&&t.Spawned&&t.Map==map){t.DeSpawn();Loot.TryAdd(t);}provisionRefs.Clear();}
  public void Restore(Map map){foreach(var t in Loot.ToList()){Loot.Remove(t);if(GenPlace.TryPlaceThing(t,map.Center+new IntVec3(17,0,-18),map,ThingPlaceMode.Near))Track(t);else Loot.TryAdd(t);}}
  public void ExposeData(){Scribe_References.Look(ref settlement,"settlement");Scribe_Values.Look(ref element,"element");Scribe_Values.Look(ref layoutVersion,"layoutVersion");Scribe_Values.Look(ref garrisonGenerated,"garrisonGenerated");Scribe_Values.Look(ref provisionsGenerated,"provisionsGenerated");Scribe_Values.Look(ref intelRevealed,"intelRevealed");Scribe_Deep.Look(ref reservedLoot,"reservedLoot",this);if(Scribe.mode==LoadSaveMode.Saving)provisionRefs.RemoveAll(t=>t==null||t.Destroyed||!t.Spawned||settlement==null||t.Map!=settlement.Map);Scribe_Collections.Look(ref provisionRefs,"provisions",LookMode.Reference);if(provisionRefs==null)provisionRefs=new List<Thing>();}
 }
 public sealed class CourtSite {
  public readonly Settlement Settlement;readonly CourtStronghold old;readonly CourtBaseRecord record;
  public CourtSite(CourtStronghold hold){Settlement=old=hold;}
  public CourtSite(Settlement settlement,CourtBaseRecord state){Settlement=settlement;record=state;}
  public Map Map{get{return Settlement.Map;}}public Faction Faction{get{return Settlement.Faction;}}public PlanetTile Tile{get{return Settlement.Tile;}}
  public int Element{get{return old!=null?old.Element:Math.Max(0,Math.Min(4,record.element));}}
  public int layoutVersion{get{return old!=null?old.layoutVersion:record.layoutVersion;}set{if(old!=null)old.layoutVersion=value;else record.layoutVersion=value;}}
  public bool garrisonGenerated{get{return old!=null?old.garrisonGenerated:record.garrisonGenerated;}set{if(old!=null)old.garrisonGenerated=value;else record.garrisonGenerated=value;}}
  public bool provisionsGenerated{get{return old!=null?old.provisionsGenerated:record.provisionsGenerated;}set{if(old!=null)old.provisionsGenerated=value;else record.provisionsGenerated=value;}}
  public bool intelRevealed{get{return old!=null?old.intelRevealed:record.intelRevealed;}set{if(old!=null)old.intelRevealed=value;else record.intelRevealed=value;}}
  public void TrackProvision(Thing t){if(old!=null)old.TrackProvision(t);else record.Track(t);}
  public void StoreUnclaimedLoot(Map map){if(old!=null)old.StoreUnclaimedLoot(map);else record.Store(map);}
  public void RestoreUnclaimedLoot(Map map){if(old!=null)old.RestoreUnclaimedLoot(map);else record.Restore(map);}
 }
 public static class CourtSettlements {
  public static CourtSite Site(Settlement settlement){if(settlement==null||!SeveredCourt.IsFaction(settlement.Faction))return null;var hold=settlement as CourtStronghold;if(hold!=null)return new CourtSite(hold);var state=Find.World.GetComponent<WorldComponent_SeveredCourt>();return state==null?null:state.NativeSite(settlement);}
  public static List<Settlement> All(Faction faction){return Find.WorldObjects.Settlements.Where(s=>s.Faction==faction&&!s.Destroyed).ToList();}
 }
 [HarmonyPatch(typeof(Settlement),"get_MapGeneratorDef")]
 public static class CourtSettlementMapGenerator {
  public static bool Prefix(Settlement __instance,ref MapGeneratorDef __result){if(!SeveredCourt.IsFaction(__instance.Faction))return true;__result=DefDatabase<MapGeneratorDef>.GetNamed("EH_CourtStrongholdMap");return false;}
 }
 [HarmonyPatch(typeof(Settlement),"GetInspectString")]
 public static class CourtNativeSettlementInspection {
  public static void Postfix(Settlement __instance,ref string __result){if(__instance is CourtStronghold||!SeveredCourt.IsFaction(__instance.Faction))return;var site=CourtSettlements.Site(__instance);if(site!=null)__result+="\nCourt fortress — "+CourtHeralds.Lineages[site.Element]+"\nInitial garrison: one Herald, two Knights and six Oathlings.\nDetailed Court architecture; finite elemental treasury.";}
 }
 [HarmonyPatch(typeof(Game),"DeinitAndRemoveMap")]
 public static class CourtNativeSettlementLootStorage {
  public static void Prefix(Map map){var settlement=map==null?null:map.Parent as Settlement;if(settlement==null||settlement is CourtStronghold||settlement.Destroyed)return;var site=CourtSettlements.Site(settlement);if(site!=null)site.StoreUnclaimedLoot(map);}
 }
 [HarmonyPatch(typeof(MapParent),"GetChildHolders")]
 public static class CourtNativeSettlementHolders {
  public static void Postfix(MapParent __instance,List<IThingHolder> __0){var settlement=__instance as Settlement;if(settlement==null||settlement is CourtStronghold||!SeveredCourt.IsFaction(settlement.Faction)||Find.World==null)return;var state=Find.World.GetComponent<WorldComponent_SeveredCourt>();var record=state==null?null:state.Record(settlement,false);if(record!=null&&!__0.Contains(record))__0.Add(record);}
 }
 [HarmonyPatch(typeof(MapParent),"Destroy")]
 public static class CourtNativeSettlementDefeated {
  public static void Prefix(MapParent __instance){var settlement=__instance as Settlement;if(settlement!=null&&SeveredCourt.IsFaction(settlement.Faction)&&CourtStories.State!=null)CourtStories.State.CancelFrom(settlement);}
 }
}
