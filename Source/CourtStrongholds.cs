using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
namespace ElementalHearts {
 public sealed class CourtStronghold:Settlement,IThingHolder {
  public int element;public bool garrisonGenerated,provisionsGenerated,intelRevealed;
  ThingOwner<Thing> reservedLoot;List<Thing> provisionRefs=new List<Thing>();
  ThingOwner<Thing> ReservedLoot{get{return reservedLoot??(reservedLoot=new ThingOwner<Thing>(this,false,LookMode.Deep));}}
  ThingOwner IThingHolder.GetDirectlyHeldThings(){return ReservedLoot;}
  public override void GetChildHolders(List<IThingHolder> children){base.GetChildHolders(children);ThingOwnerUtility.AppendThingHoldersFromThings(children,ReservedLoot);}
  public void TrackProvision(Thing thing){if(thing!=null&&!thing.Destroyed&&!provisionRefs.Contains(thing)&&provisionRefs.Count<32)provisionRefs.Add(thing);}
  public void StoreUnclaimedLoot(Map map){foreach(var t in provisionRefs)if(t!=null&&!t.Destroyed&&t.Spawned&&t.Map==map){t.DeSpawn();ReservedLoot.TryAdd(t);}provisionRefs.Clear();}
  public void RestoreUnclaimedLoot(Map map){foreach(var t in ReservedLoot.ToList()){ReservedLoot.Remove(t);if(GenPlace.TryPlaceThing(t,map.Center+new IntVec3(17,0,-18),map,ThingPlaceMode.Near))TrackProvision(t);else ReservedLoot.TryAdd(t);}}
  public int Element{get{return Mathf.Clamp(element,0,4);}}
  public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref element,"EH_holdElement");Scribe_Values.Look(ref garrisonGenerated,"EH_holdGarrisonGenerated");Scribe_Values.Look(ref provisionsGenerated,"EH_holdProvisionsGenerated");Scribe_Values.Look(ref intelRevealed,"EH_holdIntelRevealed");if(Scribe.mode==LoadSaveMode.Saving)provisionRefs.RemoveAll(t=>t==null||t.Destroyed||!t.Spawned||t.Map!=Map);Scribe_Deep.Look(ref reservedLoot,"EH_holdReservedLoot",this);Scribe_Collections.Look(ref provisionRefs,"EH_holdProvisions",LookMode.Reference);if(provisionRefs==null)provisionRefs=new List<Thing>();if(Scribe.mode==LoadSaveMode.PostLoadInit)element=Element;}
  public override void Destroy(){var state=CourtStories.State;if(state!=null)state.CancelFrom(this);base.Destroy();}
  public override string GetInspectString(){return base.GetInspectString()+"\nCourt stronghold — "+CourtHeralds.Lineages[Element]+"\nInitial garrison: one "+CourtHeralds.Titles[Element]+", two Knights and six Oathlings.\nFortified command hall, barracks, infirmary and treasury. Elite encounter; bring your own Ascendants.";}
 }
 public static class CourtStrongholds {
  public const string DefName="EH_CourtStronghold";
  public static readonly string[] Names={"The Cinder Throne","The Scarlet Spire","The Winter Bastion","The Highwind Citadel","The Adamant Seat"};
  public static List<CourtStronghold> Active(){return Find.WorldObjects==null?new List<CourtStronghold>():Find.WorldObjects.Settlements.OfType<CourtStronghold>().Where(h=>!h.Destroyed&&SeveredCourt.IsFaction(h.Faction)&&!h.Faction.defeated&&h.Faction.HostileTo(Faction.OfPlayer)).ToList();}
  public static bool CampaignSeeded{get{var c=Find.World==null?null:Find.World.GetComponent<WorldComponent_SeveredCourt>();return c!=null&&c.StrongholdsSeeded;}}
  public static bool TryCommander(out int element,out CourtStronghold origin){
   var holds=Active().Where(h=>!h.HasMap).ToList();origin=holds.Count==0?null:holds.RandomElement();element=origin==null?Rand.Range(0,5):origin.Element;
   // Once a campaign exists, destroyed strongholds cannot dispatch replacement expeditions.
   return origin!=null||!CampaignSeeded;
  }
  public static void Garrison(CourtStronghold hold,Map map){
   var members=new List<Pawn>();
   if(hold.garrisonGenerated){
    foreach(var pawn in hold.previouslyGeneratedInhabitants.ToList())if(pawn!=null&&!pawn.Destroyed&&!pawn.Dead&&!pawn.Spawned&&pawn.Faction==hold.Faction&&!pawn.IsPrisoner){if(pawn.IsWorldPawn())Find.WorldPawns.RemovePawn(pawn);members.Add(pawn);}
   }else{
    // The selected element leads; escorts rotate elements. Exactly nine defenders, never a second Herald.
    for(int i=0;i<9;i++){string rank=i==0?"Herald":i<=2?"Knight":"Oathling";string element=SeveredCourt.Elements[(hold.Element+i)%5];var kind=DefDatabase<PawnKindDef>.GetNamed("EH_Court"+rank+element);var pawn=PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind,hold.Faction,PawnGenerationContext.NonPlayer,map.Tile,forceGenerateNewPawn:true,inhabitant:true,mustBeCapableOfViolence:true,canGeneratePawnRelations:true));members.Add(pawn);}
    hold.garrisonGenerated=true;hold.previouslyGeneratedInhabitants=members.ToList();
   }
   var center=map.Center;int index=0;foreach(var pawn in members){IntVec3 anchor=index++==0?center+new IntVec3(0,0,13):center+new IntVec3((index%3-1)*4,0,-3+(index/3)*3);IntVec3 at=CellFinder.RandomClosewalkCellNear(anchor,map,4,c=>c.GetFirstPawn(map)==null);GenSpawn.Spawn(pawn,at,map);}
   if(members.Count>0)LordMaker.MakeNewLord(hold.Faction,new LordJob_DefendBase(hold.Faction,center,25000,true),map,members);
  }
 }
 public sealed class GenStep_CourtStronghold:GenStep {
  public override int SeedPart{get{return 916310783;}}
  static ThingDef Granite{get{return ThingDefOf.BlocksGranite;}}
  static IntVec3 At(Map map,int x,int z){return map.Center+new IntVec3(x,0,z);}
  static Thing Building(Map map,string name,int x,int z,ThingDef stuff,Faction faction,Rot4? rot=null){var def=DefDatabase<ThingDef>.GetNamed(name);var item=ThingMaker.MakeThing(def,def.MadeFromStuff?stuff:null);if(item.def.CanHaveFaction)item.SetFaction(faction);return GenSpawn.Spawn(item,At(map,x,z),map,rot??Rot4.North,WipeMode.Vanish);}
  static void Room(Map map,int x,int z,int w,int h,Faction faction,bool southDoor=true){
   var rect=new CellRect(map.Center.x+x,map.Center.z+z,w,h);foreach(var cell in rect){bool edge=cell.x==rect.minX||cell.x==rect.maxX||cell.z==rect.minZ||cell.z==rect.maxZ;if(edge){var wall=ThingMaker.MakeThing(ThingDefOf.Wall,Granite);wall.SetFaction(faction);GenSpawn.Spawn(wall,cell,map,WipeMode.Vanish);}else{map.terrainGrid.SetTerrain(cell,DefDatabase<TerrainDef>.GetNamed("TileMarble"));map.roofGrid.SetRoof(cell,RoofDefOf.RoofConstructed);}}
   int doorX=x+w/2;int doorZ=southDoor?z:z+h-1;Building(map,"Door",doorX,doorZ,Granite,faction);Building(map,"TorchLamp",x+2,z+2,null,faction);
  }
  static void Loot(Map map,string name,int amount,int x,int z){var d=DefDatabase<ThingDef>.GetNamedSilentFail(name);if(d==null)return;while(amount>0){var t=ThingMaker.MakeThing(d);t.stackCount=Math.Min(amount,d.stackLimit);amount-=t.stackCount;if(GenPlace.TryPlaceThing(t,At(map,x,z),map,ThingPlaceMode.Near))((CourtStronghold)map.Parent).TrackProvision(t);}}
  public override void Generate(Map map,GenStepParams parms){
   var hold=map.Parent as CourtStronghold;if(hold==null||!SeveredCourt.IsFaction(hold.Faction))return;var bounds=CellRect.CenteredOn(map.Center,51,51);if(!bounds.FullyContainedWithin(new CellRect(4,4,map.Size.x-8,map.Size.z-8)))throw new InvalidOperationException("Court stronghold map is too small.");
   // Only a newly generated stronghold map reaches this step. Existing maps are never remodeled.
   foreach(var cell in bounds.ExpandedBy(2)){foreach(var t in cell.GetThingList(map).ToList())if(!(t is Pawn))t.Destroy(DestroyMode.Vanish);map.roofGrid.SetRoof(cell,null);map.terrainGrid.SetTerrain(cell,TerrainDefOf.Concrete);}
   MapGenerator.SetVar("SettlementRect",bounds);MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(bounds);
   foreach(var cell in bounds.EdgeCells){var wall=ThingMaker.MakeThing(ThingDefOf.Wall,Granite);wall.SetFaction(hold.Faction);GenSpawn.Spawn(wall,cell,map,WipeMode.Vanish);}
   // Three front doors, a rear postern and a broad courtyard keep entry and all room approaches usable.
   for(int x=-1;x<=1;x++)Building(map,"Door",x,-25,Granite,hold.Faction);Building(map,"Door",0,25,Granite,hold.Faction);
   Room(map,-10,5,21,17,hold.Faction);Room(map,-23,-8,15,12,hold.Faction);Room(map,9,-8,15,12,hold.Faction);Room(map,-23,-22,15,11,hold.Faction,false);Room(map,9,-22,15,11,hold.Faction,false);
   foreach(int x in new[]{-5,5})foreach(int z in new[]{10,16})Building(map,"Column",x,z,Granite,hold.Faction);
   for(int i=0;i<6;i++)Building(map,"Bed",-20+(i%3)*4,-5+(i/3)*4,ThingDefOf.WoodLog,hold.Faction);
   for(int i=0;i<3;i++)Building(map,"Bed",-20+i*4,-19,ThingDefOf.Steel,hold.Faction);
   Building(map,"Table2x2c",0,14,ThingDefOf.WoodLog,hold.Faction);Building(map,"DiningChair",-2,14,ThingDefOf.WoodLog,hold.Faction,Rot4.East);Building(map,"DiningChair",2,14,ThingDefOf.WoodLog,hold.Faction,Rot4.West);
   Building(map,"Shelf",13,-4,ThingDefOf.WoodLog,hold.Faction);Building(map,"Shelf",19,-4,ThingDefOf.WoodLog,hold.Faction);
   for(int x=-6;x<=6;x++)for(int z=-24;z<=4;z++)if(Math.Abs(x)<=1)map.terrainGrid.SetTerrain(At(map,x,z),DefDatabase<TerrainDef>.GetNamed("TileSlate"));
   foreach(int x in new[]{-6,6})for(int z=-18;z<=0;z+=9)Building(map,"Sandbags",x,z,ThingDefOf.Cloth,hold.Faction);
   if(!hold.provisionsGenerated){hold.provisionsGenerated=true;Loot(map,"Silver",2400,13,-18);Loot(map,"Gold",120,17,-18);Loot(map,"ComponentSpacer",8,21,-18);Loot(map,"MedicineIndustrial",20,-18,-17);Loot(map,"MealSurvivalPack",30,17,-2);Loot(map,"EH_"+SeveredCourt.Elements[hold.Element]+"Heart",2,17,-17);}else hold.RestoreUnclaimedLoot(map);
   CourtStrongholds.Garrison(hold,map);
  }
 }
 [HarmonyPatch(typeof(Game),"DeinitAndRemoveMap")]
 public static class CourtStrongholdLootStorage {
  public static void Prefix(Map map){var hold=map==null?null:map.Parent as CourtStronghold;if(hold!=null&&!hold.Destroyed)hold.StoreUnclaimedLoot(map);}
 }
}
