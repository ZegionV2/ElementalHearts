using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class CourtStrongholdArchitecture {
  public const int Version=2;
  static readonly Color crimson=new Color(.42f,.035f,.075f);
  static ThingDef Granite{get{return ThingDefOf.BlocksGranite;}}
  static IntVec3 At(Map map,int x,int z){return map.Center+new IntVec3(x,0,z);}
  static bool BasicFloor(TerrainDef def){return def==TerrainDefOf.Concrete||def.defName=="TileMarble"||def.defName=="TileSlate"||def.defName.StartsWith("EH_CourtFloor");}
  static void Floor(Map map,string def,int x,int z,int w,int h,bool preserve){var terrain=DefDatabase<TerrainDef>.GetNamed(def);foreach(var c in new CellRect(map.Center.x+x,map.Center.z+z,w,h)){if(!c.InBounds(map)||preserve&&(!BasicFloor(map.terrainGrid.TerrainAt(c))||map.terrainGrid.colorGrid!=null&&map.terrainGrid.colorGrid[map.cellIndices.CellToIndex(c)]!=null||c.GetThingList(map).Any(t=>t.def.category==ThingCategory.Building&&t.Faction==Faction.OfPlayer)))continue;map.terrainGrid.SetTerrain(c,terrain);}}
  static Thing Place(Map map,string name,int x,int z,ThingDef stuff,Faction faction,Rot4? rotation=null){
   var def=DefDatabase<ThingDef>.GetNamed(name);var at=At(map,x,z);var rot=rotation??Rot4.North;var rect=GenAdj.OccupiedRect(at,rot,def.size);
   foreach(var c in rect){if(!c.InBounds(map))return null;foreach(var t in c.GetThingList(map)){if(t.def==def&&t.Position==at)return t;if(t is Pawn||t.def.category==ThingCategory.Item||t.def.category==ThingCategory.Building&&t.def.altitudeLayer!=AltitudeLayer.FloorEmplacement)return null;}}
   var item=ThingMaker.MakeThing(def,def.MadeFromStuff?stuff:null);if(def.CanHaveFaction)item.SetFaction(faction);GenSpawn.Spawn(item,at,map,rot,WipeMode.Vanish);var tint=item.TryGetComp<CompColorable>();if(tint!=null&&(name=="Armchair"||name=="DiningChair"))tint.SetColor(crimson);return item;
  }
  static void Wall(Map map,int x,int z,Faction faction,bool door=false){var item=ThingMaker.MakeThing(door?ThingDefOf.Door:ThingDefOf.Wall,Granite);item.SetFaction(faction);GenSpawn.Spawn(item,At(map,x,z),map,WipeMode.Vanish);}
  static void Room(Map map,int x,int z,int w,int h,Faction faction,bool northDoor=false){
   var rect=new CellRect(map.Center.x+x,map.Center.z+z,w,h);foreach(var c in rect){if(c.x==rect.minX||c.x==rect.maxX||c.z==rect.minZ||c.z==rect.maxZ)Wall(map,c.x-map.Center.x,c.z-map.Center.z,faction);else{map.terrainGrid.SetTerrain(c,DefDatabase<TerrainDef>.GetNamed("TileMarble"));map.roofGrid.SetRoof(c,RoofDefOf.RoofConstructed);}}
   Wall(map,x+w/2,northDoor?z+h-1:z,faction,true);
  }
  public static void Build(Map map,CourtStronghold hold){Build(map,new CourtSite(hold));}
  public static void Build(Map map,CourtSite hold){
   var bounds=CellRect.CenteredOn(map.Center,59,59);if(!bounds.ExpandedBy(3).FullyContainedWithin(new CellRect(2,2,map.Size.x-4,map.Size.z-4)))throw new InvalidOperationException("Court stronghold map is too small.");
   foreach(var c in bounds.ExpandedBy(3)){foreach(var t in c.GetThingList(map).ToList())if(!(t is Pawn))t.Destroy(DestroyMode.Vanish);map.roofGrid.SetRoof(c,null);map.terrainGrid.SetTerrain(c,TerrainDefOf.Concrete);}
   MapGenerator.SetVar("SettlementRect",bounds);MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects").Add(bounds);
   foreach(var c in bounds.EdgeCells)Wall(map,c.x-map.Center.x,c.z-map.Center.z,hold.Faction);
   for(int x=-1;x<=1;x++){Wall(map,x,-29,hold.Faction,true);Wall(map,x,-28,hold.Faction,true);}Wall(map,0,29,hold.Faction,true);
   foreach(int x in new[]{-28,24})foreach(int z in new[]{-28,24})Room(map,x,z,5,5,hold.Faction,z<0);
   Room(map,-8,-29,6,6,hold.Faction,true);Room(map,3,-29,6,6,hold.Faction,true);
   Room(map,-10,5,21,17,hold.Faction);Room(map,-23,-8,15,12,hold.Faction);Room(map,9,-8,15,12,hold.Faction);Room(map,-23,-22,15,11,hold.Faction,true);Room(map,9,-22,15,11,hold.Faction,true);
   foreach(int x in new[]{-5,5})foreach(int z in new[]{10,16})Place(map,"Column",x,z,Granite,hold.Faction);
   // A second treasury door creates a small vestibule without constricting the central battle route.
   for(int x=10;x<=22;x++)Wall(map,x,-14,hold.Faction,x==16);
   Dress(map,hold,false);hold.layoutVersion=Version;
  }
  static void Dress(Map map,CourtSite hold,bool preserve){
   var faction=hold.Faction;string accent="EH_CourtFloor"+SeveredCourt.Elements[hold.Element];
   Floor(map,"TileSlate",-3,-29,7,35,preserve);Floor(map,"TileMarble",-2,-29,5,35,preserve);Floor(map,accent,-1,-28,3,33,preserve);
   Floor(map,"TileSlate",-9,6,19,15,preserve);Floor(map,"TileMarble",-8,7,17,13,preserve);Floor(map,accent,-1,6,3,14,preserve);Floor(map,accent,-7,17,15,3,preserve);
   Floor(map,"TileSlate",-4,17,9,4,preserve);Floor(map,"TileMarble",-3,18,7,3,preserve);Floor(map,accent,-2,18,5,2,preserve);
   Floor(map,accent,-4,-3,9,7,preserve);Place(map,"EH_CourtCrownInlay",0,0,null,faction);Place(map,"EH_CourtLineage"+SeveredCourt.Elements[hold.Element],0,9,null,faction);
   Place(map,"Armchair",0,19,ThingDefOf.Cloth,faction,Rot4.South);Place(map,"Table2x2c",0,14,ThingDefOf.WoodLog,faction);Place(map,"DiningChair",-2,14,ThingDefOf.WoodLog,faction,Rot4.East);Place(map,"DiningChair",2,14,ThingDefOf.WoodLog,faction,Rot4.West);
   foreach(int x in new[]{-3,3})Place(map,"EH_CourtStandard",x,20,null,faction);
   foreach(int x in new[]{-7,7}){Place(map,"EH_CourtBrazier",x,19,null,faction);Place(map,"EH_CourtBrazier",x,7,null,faction);for(int z=9;z<=15;z+=3)Place(map,"DiningChair",x,z,ThingDefOf.WoodLog,faction,x<0?Rot4.East:Rot4.West);Place(map,"BookcaseSmall",x,13,ThingDefOf.WoodLog,faction);}
   foreach(int x in new[]{-4,4}){Place(map,"EH_CourtStandard",x<0?-9:9,-27,null,faction);Place(map,"EH_CourtBrazier",x,-26,null,faction);}
   foreach(int x in new[]{-25,25})foreach(int z in new[]{-23,23})Place(map,"EH_CourtBrazier",x,z,null,faction);
   foreach(int x in new[]{-26,26})foreach(int z in new[]{-26,26})Place(map,"Stool",x,z,ThingDefOf.WoodLog,faction);
   foreach(int x in new[]{-6,6})for(int z=-18;z<=0;z+=9)Place(map,"Barricade",x,z,Granite,faction);
   Floor(map,accent,-18,-7,5,10,preserve);for(int i=0;i<6;i++){int x=-20+(i%3)*4,z=-5+(i/3)*4;Place(map,"Bed",x,z,ThingDefOf.WoodLog,faction);Place(map,"EndTable",x+1,z,ThingDefOf.WoodLog,faction);}
   Place(map,"Dresser",-19,2,ThingDefOf.WoodLog,faction);Place(map,"ShelfSmall",-22,1,ThingDefOf.WoodLog,faction);Place(map,"ChessTable",-22,-3,ThingDefOf.WoodLog,faction);Place(map,"EH_CourtBrazier",-21,-6,null,faction);
   Floor(map,"TileMarble",-22,-21,13,9,preserve);Floor(map,"TileSlate",-22,-16,13,1,preserve);
   for(int i=0;i<3;i++){var bed=Place(map,"Bed",-20+i*4,-19,ThingDefOf.Steel,faction) as Building_Bed;if(bed!=null)bed.Medical=true;Place(map,"EndTable",-19+i*4,-19,ThingDefOf.Steel,faction);}
   Place(map,"ShelfSmall",-21,-14,ThingDefOf.Steel,faction);Place(map,"ToolCabinet",-12,-15,ThingDefOf.Steel,faction);Place(map,"EH_CourtBrazier",-21,-20,null,faction);
   Floor(map,accent,10,-7,13,2,preserve);Place(map,"Shelf",13,-4,ThingDefOf.WoodLog,faction);Place(map,"Shelf",19,-4,ThingDefOf.WoodLog,faction);Place(map,"TableMachining",17,1,ThingDefOf.Steel,faction);Place(map,"ToolCabinet",11,1,ThingDefOf.Steel,faction);Place(map,"Stool",17,0,ThingDefOf.WoodLog,faction);Place(map,"EH_CourtStandard",22,2,null,faction);Place(map,"EH_CourtBrazier",10,-6,null,faction);
   Floor(map,"TileSlate",10,-21,13,8,preserve);Floor(map,"TileMarble",11,-20,11,6,preserve);Place(map,"EH_CourtStandard",11,-13,null,faction);Place(map,"EH_CourtBrazier",21,-15,null,faction);Place(map,"ShelfSmall",11,-16,ThingDefOf.Steel,faction);Place(map,"ShelfSmall",22,-18,ThingDefOf.Steel,faction);
  }
  public static void UpgradeExisting(CourtStronghold hold){if(hold==null||hold.Destroyed||!hold.HasMap||hold.layoutVersion>=Version||!SeveredCourt.IsFaction(hold.Faction))return;Dress(hold.Map,new CourtSite(hold),true);hold.layoutVersion=Version;}
 }
}
