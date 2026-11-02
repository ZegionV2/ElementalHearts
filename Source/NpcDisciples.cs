using System;using System.Reflection;using HarmonyLib;using RimWorld;using Verse;
namespace ElementalHearts {
 public static class NpcDiscipleRules {
  public const float AwakeningChance=.05f,CriticalHealth=.25f;public const int GraceTicks=180;
  public static bool SpawnEligible(bool humanlike,bool mech,bool adult,bool player,bool faction,bool dead,bool downed,bool violent,bool prisoner,bool slave,bool elemental){return humanlike&&!mech&&adult&&!player&&faction&&!dead&&!downed&&violent&&!prisoner&&!slave&&!elemental;}
  public static bool ShouldRoll(bool disciple,bool tried,bool living,bool combat,bool critical){return disciple&&!tried&&living&&combat&&critical;}
 }
 public static class NpcDisciples {
  public static readonly string[] Elements={"Lightning","Flame","Wind","Water"};
  public static bool Eligible(Pawn p){return p!=null&&p.health!=null&&NpcDiscipleRules.SpawnEligible(p.RaceProps.Humanlike,p.RaceProps.IsMechanoid,p.DevelopmentalStage.Adult(),p.Faction!=null&&p.Faction.IsPlayer,p.Faction!=null,p.Dead,p.Downed,!p.WorkTagIsDisabled(WorkTags.Violent),p.IsPrisoner,p.IsSlave,ElementalHeartUtility.HasHeart(p)||ElementalTeaching.Trained(p));}
  public static void OnGenerated(Pawn p){if(!Eligible(p)||ElementalHeartsMod.Settings==null||!Rand.Chance(ElementalHeartsMod.Settings.npcDiscipleChance))return;Grant(p,Elements[Rand.Range(0,Elements.Length)]);}
  public static bool Grant(Pawn p,string element){if(p==null||ElementalHeartUtility.HasHeart(p)||ElementalTeaching.Trained(p))return false;var def=DefDatabase<HediffDef>.GetNamedSilentFail("EH_"+element+"Training");if(def==null)return false;p.health.AddHediff(def);EnableAuto(p);return true;}
  public static bool AutoNpc(Pawn p){return p!=null&&p.Spawned&&!ElementalCombatPolicy.Controlled(p)&&p.Faction!=null&&!p.Faction.IsPlayer&&!p.IsPrisoner&&!p.IsSlave;}
  public static void EnableAuto(Pawn p){var celestial=HediffComp_CelestialAbilities.On(p);if(celestial!=null)celestial.autoCombat=true;var l=HediffComp_LightningAbilities.On(p);if(l!=null)l.autoCombat=true;var f=HediffComp_FlameAbilities.On(p);if(f!=null)f.autoCombat=true;var w=HediffComp_WindAbilities.On(p);if(w!=null)w.autoCombat=true;var a=HediffComp_WaterDefense.On(p);if(a!=null)a.autoCombat=true;}
  public static void EquipDisciple(Pawn p,string element){if(!AutoNpc(p)||!ElementalTeaching.Ready(p)||ElementalTeachingSession.Busy(p)||ElementalAnimationBridge.Controlled(p))return;EnableAuto(p);ElementalBoundSwords.EnsureCalled(p,element);}
 }
 // This is the successful NEW-pawn factory, not the reusable world-pawn lookup or retry factory.
 [HarmonyPatch(typeof(PawnGenerator),"GenerateNewPawnInternal")]
 public static class GenerateNpcDisciple {public static void Postfix(Pawn __result){NpcDisciples.OnGenerated(__result);}}
}
