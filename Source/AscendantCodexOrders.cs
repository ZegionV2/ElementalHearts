using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public sealed class CodexOrderReport {
  public int changed,unchanged;public readonly Dictionary<string,int> skipped=new Dictionary<string,int>();
  public void Skip(string reason){int n;skipped.TryGetValue(reason,out n);skipped[reason]=n+1;}
  public string Describe(string action){string result=action+": "+changed+" updated"+(unchanged>0?", "+unchanged+" already set":"");if(skipped.Count>0)result+=". Skipped "+string.Join("; ",skipped.Select(k=>k.Value+" "+k.Key).ToArray());return result+".";}
 }
 public static class CodexOrders {
  public static bool Busy(Pawn p){return p!=null&&(ElementalAnimationBridge.Controlled(p)||ElementalTeachingSession.Busy(p)||ElementalBestow.Busy(p)||AzureEvolution.Evolving(p)||HediffComp_ElementalTransform.Transforming(p)||DiscipleAscension.Busy(p)||p.CurJobDef!=null&&p.CurJobDef.defName.StartsWith("EH_"));}
  public static string OrderProblem(Pawn p){if(!AscendantCodex.Owned(p))return "Not a living, owned free pawn";if(!p.Spawned)return "Off map or contained";if(!ElementalTeaching.Ready(p))return "Asleep, downed or unable to act";if(Busy(p))return "Busy with an ability or ceremony";if(p.jobs==null||p.jobs.jobQueue.Count>0)return "Has queued orders";return null;}
  public static ElementalAutoMode? Mode(Pawn p){
   var eclipse=HediffComp_EclipseAbilities.On(p);if(eclipse!=null)return ElementalAutoModes.FromFlags(eclipse.autoCombat,eclipse.nonLethal);var lunar=HediffComp_LunarAbilities.On(p);if(lunar!=null)return ElementalAutoModes.FromFlags(lunar.autoCombat,lunar.nonLethal);var celestial=HediffComp_CelestialAbilities.On(p);if(celestial!=null)return ElementalAutoModes.FromFlags(celestial.autoCombat,celestial.nonLethal);var earth=HediffComp_EarthAbilities.On(p);if(earth!=null)return ElementalAutoModes.FromFlags(earth.autoCombat,earth.nonLethal);var water=HediffComp_WaterDefense.On(p);if(water!=null)return ElementalAutoModes.FromFlags(water.autoCombat,water.nonLethal);var lightning=HediffComp_LightningAbilities.On(p);if(lightning!=null)return ElementalAutoModes.FromFlags(lightning.autoCombat,lightning.nonLethal);var flame=HediffComp_FlameAbilities.On(p);if(flame!=null)return ElementalAutoModes.FromFlags(flame.autoCombat,flame.nonLethal);var wind=HediffComp_WindAbilities.On(p);return wind==null?(ElementalAutoMode?)null:ElementalAutoModes.FromFlags(wind.autoCombat,wind.nonLethal);
  }
  public static bool SetMode(Pawn p,ElementalAutoMode mode){
   if(p==null||p.health==null||!Enum.IsDefined(typeof(ElementalAutoMode),mode))return false;
   bool found=false,on=mode!=ElementalAutoMode.Off,nonLethal=mode==ElementalAutoMode.NonLethal;
   foreach(var h in p.health.hediffSet.hediffs){var root=h as HediffWithComps;if(root==null||root.comps==null)continue;foreach(var c in root.comps){
    var eclipse=c as HediffComp_EclipseAbilities;if(eclipse!=null){eclipse.autoCombat=on;eclipse.nonLethal=nonLethal;found=true;}
    var lunar=c as HediffComp_LunarAbilities;if(lunar!=null){lunar.autoCombat=on;lunar.nonLethal=nonLethal;found=true;}
    var celestial=c as HediffComp_CelestialAbilities;if(celestial!=null){celestial.autoCombat=on;celestial.nonLethal=nonLethal;found=true;}
    var earth=c as HediffComp_EarthAbilities;if(earth!=null){earth.autoCombat=on;earth.nonLethal=nonLethal;found=true;}
    var water=c as HediffComp_WaterDefense;if(water!=null){water.autoCombat=on;water.nonLethal=nonLethal;found=true;}
    var lightning=c as HediffComp_LightningAbilities;if(lightning!=null){lightning.autoCombat=on;lightning.nonLethal=nonLethal;found=true;}
    var flame=c as HediffComp_FlameAbilities;if(flame!=null){flame.autoCombat=on;flame.nonLethal=nonLethal;found=true;}
    var wind=c as HediffComp_WindAbilities;if(wind!=null){wind.autoCombat=on;wind.nonLethal=nonLethal;found=true;}
   }}return found;
  }
  public static CodexOrderReport Modes(IEnumerable<Pawn> pawns,ElementalAutoMode mode){
   var result=new CodexOrderReport();foreach(var p in pawns.Distinct()){
    if(!AscendantCodex.Owned(p)){result.Skip("not owned");continue;}var current=Mode(p);if(!current.HasValue){result.Skip("no elemental combat controls");continue;}
    if(Busy(p)){result.Skip("busy");continue;}if(current.Value==mode){result.unchanged++;continue;}if(SetMode(p,mode))result.changed++;
   }return result;
  }
  public static CodexOrderReport Transform(IEnumerable<Pawn> pawns,bool desired){
   var result=new CodexOrderReport();foreach(var p in pawns.Distinct()){
    string why=OrderProblem(p);if(why!=null){result.Skip(why);continue;}var comp=HediffComp_ElementalTransform.On(p);if(comp==null){result.Skip("no transformation");continue;}
    if(comp.transformed==desired){result.unchanged++;continue;}why=comp.CodexProblem();if(why!=null){result.Skip(why);continue;}
    if(p.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_Transform")),JobTag.Misc))result.changed++;else result.Skip("order rejected");
   }return result;
  }
 }
}
