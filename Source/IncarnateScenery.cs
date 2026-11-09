using System;using System.Collections.Generic;using HarmonyLib;using RimWorld;using Verse;
namespace ElementalHearts {
 public static class IncarnateScenery {
  public const string Damage="EH_IncarnateSceneryBlast";
  public static void Impact(Pawn source,IntVec3 at,int power){if(source==null||!source.Spawned||!ThunderIncarnate.Active(source)||!ElementalHeartsMod.Settings.incarnateTerrainDamage||ElementalNonLethal.Enabled(source)||!at.InBounds(source.Map))return;
   var map=source.Map;var budget=map.GetComponent<MapComponent_IncarnateScenery>();if(!budget.Admit())return;
   float radius=power==6?10:power==5?8:power==3?5:power==2?4.5f:power==4?1.4f:1.8f;int damage=power==6?110:power==5?75:power==3?55:power==2?35:22;
   GenExplosion.DoExplosion(at,map,radius,DefDatabase<DamageDef>.GetNamed(Damage),source,damAmount:damage,armorPenetration:1,weapon:source.equipment==null||source.equipment.Primary==null?null:source.equipment.Primary.def,damageFalloff:true,doVisualEffects:false,doSoundEffects:false,screenShakeFactor:0);
   int ash=0,fires=0,visited=0;foreach(var cell in GenRadial.RadialCellsAround(at,radius,true)){if(++visited>360)break;if(!cell.InBounds(map)||!cell.Standable(map)||!LightningCombat.ClearLine(at,cell,map))continue;
    if(ash<8&&(visited==1||visited%11==0)){FilthMaker.TryMakeFilth(cell,map,ThingDefOf.Filth_Ash);ash++;}
    if(fires<(power>=5?6:power>=2?4:2)&&visited%7==0&&cell.GetFirstPawn(map)==null&&cell.GetTerrain(map).passability!=Traversability.Impassable){if(FireUtility.TryStartFireIn(cell,map,.35f,source))fires++;}
   }
  }
 }
 public sealed class MapComponent_IncarnateScenery:MapComponent {
  int window=-9999,count;public MapComponent_IncarnateScenery(Map map):base(map){}
  public bool Admit(){int now=Find.TickManager.TicksGame;if(now-window>=60){window=now;count=0;}if(count>=12)return false;count++;return true;}
 }
 // Scenery damage is separate from the saved physical/electrical hit on each pawn.
 public sealed class DamageWorker_IncarnateScenery:DamageWorker_AddInjury {public override DamageResult Apply(DamageInfo dinfo,Thing victim){return victim is Pawn?new DamageResult():base.Apply(dinfo,victim);}}
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class IncarnateSceneryPawnGuard {
  [HarmonyPriority(Priority.First+1000)]public static bool Prefix(Thing __instance,DamageInfo dinfo,ref DamageWorker.DamageResult __result){if(!(__instance is Pawn)||dinfo.Def==null||dinfo.Def.defName!=IncarnateScenery.Damage)return true;__result=new DamageWorker.DamageResult();return false;}
 }
}
