using System;
using HarmonyLib;
using RimWorld;
namespace ElementalHearts {
 // Remember the original incident budget through vanilla arrival/strategy adjustments.
 // Finalization unwinds the context even if another raid patch throws.
 public sealed class CourtRaidBudgetContext:IDisposable {
  [ThreadStatic]public static CourtRaidBudgetContext Current;
  readonly CourtRaidBudgetContext previous;public bool maximum;
  public CourtRaidBudgetContext(){previous=Current;Current=this;}
  public void Dispose(){Current=previous;}
 }
 [HarmonyPatch(typeof(IncidentWorker_Raid),"TryGenerateRaidInfo")]
 public static class CourtRaidBudgetScope {
  public static void Prefix(out CourtRaidBudgetContext __state){__state=new CourtRaidBudgetContext();}
  public static Exception Finalizer(Exception __exception,CourtRaidBudgetContext __state){if(__state!=null)__state.Dispose();return __exception;}
 }
 [HarmonyPatch(typeof(IncidentWorker_Raid),"AdjustedRaidPoints")]
 public static class CourtOriginalRaidBudget {
  public static void Postfix(float points,Faction faction){if(CourtRaidBudgetContext.Current!=null&&SeveredCourt.IsFaction(faction)&&CourtHeralds.Guaranteed(points))CourtRaidBudgetContext.Current.maximum=true;}
 }
}
