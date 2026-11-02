using HarmonyLib;using RimWorld;using Verse;
namespace ElementalHearts {
 public static class AzureDefenseRules {
  public const float NormalAscendantDamageFactor=.25f;
  public static bool WardApplies(bool targetEvolved,bool sourceAscendant,bool sourceEvolved){return targetEvolved&&sourceAscendant&&!sourceEvolved;}
  public static int MoodStage(bool evolved){return evolved?1:0;}
 }
 [HarmonyPatch(typeof(Thing),"TakeDamage")]
 public static class AzureAscendantWard {
  public static void Prefix(Thing __instance,ref DamageInfo dinfo){
   var target=__instance as Pawn;var source=dinfo.Instigator as Pawn;if(target==null||source==null||dinfo.Amount<=0)return;
   if(AzureDefenseRules.WardApplies(EvolvedLineage.Active(target),ElementalHeartUtility.HasHeart(source),EvolvedLineage.Active(source)))dinfo.SetAmount(dinfo.Amount*AzureDefenseRules.NormalAscendantDamageFactor);
  }
 }
 public sealed class ThoughtWorker_FlameResolve:ThoughtWorker_Hediff {
  protected override ThoughtState CurrentStateInternal(Pawn p){if(AzureEvolution.Active(p)&&AzureEvolution.HasFlame(p))return ThoughtState.ActiveAtStage(AzureDefenseRules.MoodStage(true));return base.CurrentStateInternal(p);}
 }
}
