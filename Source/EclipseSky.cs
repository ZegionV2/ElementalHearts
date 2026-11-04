using HarmonyLib;
using RimWorld;
using Verse;
namespace ElementalHearts {
 public static class EclipseSky {
  public const int Duration=15000,Cooldown=60000;
  public const float Chance=.15f;
  public static bool Empowered(Pawn p){return EclipseAscendant.Active(p)&&p.Spawned&&p.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse);}
  public static void RemoveBoost(Pawn p){if(p==null||p.health==null)return;for(int i=p.health.hediffSet.hediffs.Count-1;i>=0;i--)if(p.health.hediffSet.hediffs[i].def.defName=="EH_EclipseDominion")p.health.RemoveHediff(p.health.hediffSet.hediffs[i]);}
  public static void Refresh(Pawn p){if(p==null||p.health==null)return;var def=DefDatabase<HediffDef>.GetNamed("EH_EclipseDominion");var boost=p.health.hediffSet.GetFirstHediffOfDef(def);if(Empowered(p)){if(boost==null)p.health.AddHediff(def);}else if(boost!=null)RemoveBoost(p);}
 }
 public sealed class MapComponent_EclipseSky:MapComponent {
  public int nextAllowed;
  public MapComponent_EclipseSky(Map map):base(map){}
  public override void ExposeData(){Scribe_Values.Look(ref nextAllowed,"EH_eclipseMapNextSummon");}
  public bool TrySummon(Pawn pawn,bool guaranteed){
   int now=Find.TickManager.TicksGame;var comp=HediffComp_EclipseAbilities.On(pawn);
   if(comp==null||!pawn.Spawned||pawn.Map!=map||!comp.CanAct||comp.combatUntil<=now||!EclipseMovement.Pressured(pawn,5)||now<nextAllowed||now<comp.nextSummonAt||map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))return false;
   if(!guaranteed&&!Rand.Chance(EclipseSky.Chance))return false;
   map.gameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(GameConditionDefOf.Eclipse,EclipseSky.Duration));
   nextAllowed=comp.nextSummonAt=now+EclipseSky.Cooldown;
   foreach(var other in map.mapPawns.AllPawnsSpawned)if(EclipseAscendant.Active(other))EclipseSky.Refresh(other);
   EclipseVfx.Emit(pawn,9,pawn.DrawPos,pawn.DrawPos,48,8f);ElementalAudio.Ability(pawn,ElementalSound.Impact);
   Messages.Message(pawn.LabelShort+" has called an eclipse. Eclipse Dominion rises for six in-game hours.",pawn,MessageTypeDefOf.NeutralEvent,false);return true;
  }
 }
 public sealed class GameComponent_EclipseBoostCleanup:GameComponent {
  public GameComponent_EclipseBoostCleanup(Game game){}
  public override void FinalizeInit(){foreach(var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)EclipseSky.Refresh(pawn);}
 }
 [HarmonyPatch(typeof(Pawn),"DeSpawn")]
 public static class EclipseDepartedMap {
  public static void Postfix(Pawn __instance){if(EclipseAscendant.Active(__instance))EclipseSky.RemoveBoost(__instance);}
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class EclipseCombatContact {
  public static void Postfix(Thing __instance,DamageInfo dinfo,DamageWorker.DamageResult __result){
   var victim=__instance as Pawn;var source=dinfo.Instigator as Pawn;if(__result==null||__result.totalDamageDealt<=0||victim==null||source==null||!source.HostileTo(victim))return;
   int until=Find.TickManager.TicksGame+600;var attacker=HediffComp_EclipseAbilities.On(source);var defender=HediffComp_EclipseAbilities.On(victim);if(attacker!=null)attacker.combatUntil=until;if(defender!=null)defender.combatUntil=until;
  }
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class EclipseSovereignWard {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){
   var pawn=__instance as Pawn;var source=dinfo.Instigator as Pawn;
   if(dinfo.Amount>0&&EclipseSky.Empowered(pawn)&&source!=null&&ElementalHeartUtility.HasHeart(source)&&!CelestialAscendant.Active(source)&&!EclipseSky.Empowered(source))dinfo.SetAmount(dinfo.Amount*.2f);
  }
 }
}
