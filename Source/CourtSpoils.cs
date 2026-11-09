using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using Verse;
namespace ElementalHearts {
 public sealed class CourtSpoilsState {public bool claimed;}
 public sealed class CourtSpoilsDrop {public Map map;public IntVec3 cell;public int element,rank;}
 public static class CourtSpoils {
  static readonly ConditionalWeakTable<Pawn,CourtSpoilsState> states=new ConditionalWeakTable<Pawn,CourtSpoilsState>();
  public static CourtSpoilsState State(Pawn pawn){return states.GetOrCreateValue(pawn);}
  public static CourtSpoilsDrop Capture(Pawn pawn){
   if(pawn==null||pawn.Dead||pawn.Destroyed||!SeveredCourt.IsFaction(pawn.Faction)||!pawn.HostileTo(Faction.OfPlayer)||!CourtAppearance.Bound(pawn)||State(pawn).claimed)return null;
   int element=CourtHeralds.Index(pawn),rank=element>=0?2:ElementalHeartUtility.HasHeart(pawn)?1:0;
   if(element<0){var role=pawn.kindDef.GetModExtension<CourtSoldier>();element=role==null?-1:Array.IndexOf(SeveredCourt.Elements,role.element);if(element<0)element=CourtNameBadges.Index(pawn);}
   if(element<0||element>=5)return null;return new CourtSpoilsDrop{map=pawn.MapHeld,cell=pawn.PositionHeld,element=element,rank=rank};
  }
  public static void Drop(Map map,IntVec3 cell,string name,int amount){if(map==null||!cell.InBounds(map)||amount<=0)return;var def=DefDatabase<ThingDef>.GetNamed(name);var thing=ThingMaker.MakeThing(def);thing.stackCount=Math.Min(amount,def.stackLimit);if(!GenPlace.TryPlaceThing(thing,cell,map,ThingPlaceMode.Near))thing.Destroy();}
  public static void Complete(Pawn pawn,CourtSpoilsDrop drop){if(drop==null||!pawn.Dead||State(pawn).claimed)return;State(pawn).claimed=true;int amount=drop.rank==2?Rand.RangeInclusive(6,8):drop.rank==1?Rand.RangeInclusive(3,4):Rand.RangeInclusive(1,2);Drop(drop.map,drop.cell,"EH_Essence"+SeveredCourt.Elements[drop.element],amount);if(drop.rank==2)Drop(drop.map,drop.cell,"EH_CourtHeraldSigil",1);}
 }
 [HarmonyPatch(typeof(Pawn),"Kill")]
 public static class CourtSpoilsDeath {
  public static void Prefix(Pawn __instance,out CourtSpoilsDrop __state){__state=CourtSpoils.Capture(__instance);}
  public static void Postfix(Pawn __instance,CourtSpoilsDrop __state){CourtSpoils.Complete(__instance,__state);}
 }
 [HarmonyPatch(typeof(Pawn),"ExposeData")]
 public static class CourtSpoilsSave {
  public static void Postfix(Pawn __instance){var state=CourtSpoils.State(__instance);Scribe_Values.Look(ref state.claimed,"EH_courtSpoilsClaimed",__instance.Dead);}
 }
}
