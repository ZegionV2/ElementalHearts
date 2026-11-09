using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
namespace ElementalHearts {
 public static class CourtRedemption {
  public static bool Apply(Pawn pawn){
   if(pawn==null||pawn.Faction!=Faction.OfPlayer||pawn.story==null||!CourtAppearance.Bound(pawn))return false;
   CourtTactics.RemoveFocus(pawn);pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(DefDatabase<TraitDef>.GetNamed(CourtAppearance.TraitName)));
   var transform=HediffComp_ElementalTransform.On(pawn);
   if(transform!=null){transform.ReconcileCourtStyle();transform.ReconcileHeraldFaceguard();}
   if(pawn.Drawer!=null)pawn.Drawer.renderer.SetAllGraphicsDirty();return true;
  }
  public static int ExistingColonists(){var seen=new HashSet<Pawn>();int count=0;
   if(Find.WorldPawns!=null)foreach(var p in Find.WorldPawns.AllPawnsAliveOrDead)if(seen.Add(p)&&Apply(p))count++;
   foreach(var map in Find.Maps)foreach(var p in map.mapPawns.AllPawns)if(seen.Add(p)&&Apply(p))count++;
   return count;
  }
 }
 [HarmonyPatch(typeof(Pawn),"SetFaction")]
 public static class CourtRecruitmentRedemption {
  public static void Postfix(Pawn __instance,Faction newFaction){if(newFaction==Faction.OfPlayer)CourtRedemption.Apply(__instance);}
 }
}

