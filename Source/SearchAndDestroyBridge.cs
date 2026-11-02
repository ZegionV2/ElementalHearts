using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class LightningJobUtility {
  private static readonly FieldInfo pathMode=AccessTools.Field(typeof(Pawn_PathFollower),"peMode");
  public static PathEndMode PathMode(Pawn p){return (PathEndMode)pathMode.GetValue(p.pather);}
 }
 // Optional, read-only bridge. No Search and Destroy assembly reference or writes to its state.
 public static class SearchAndDestroyBridge {
  private static bool initialized;
  private static PropertyInfo instance,storage;
  private static MethodInfo getData;
  private static FieldInfo enabled;
  private static void Init(){
   initialized=true;Type t=AccessTools.TypeByName("SearchAndDestroy.Base");if(t==null)return;
   instance=AccessTools.Property(t,"Instance");storage=AccessTools.Property(t,"ExtendedDataStorage");
   if(storage==null)return;getData=AccessTools.Method(storage.PropertyType,"GetExtendedDataFor",new[]{typeof(Pawn)});
   if(getData!=null)enabled=AccessTools.Field(getData.ReturnType,"SD_enabled");
  }
  public static bool Enabled(Pawn pawn){
   if(!initialized)Init();if(instance==null||storage==null||getData==null||enabled==null)return false;
   try {object mod=instance.GetValue(null,null);if(mod==null)return false;object dataStore=storage.GetValue(mod,null);return dataStore!=null&&(bool)enabled.GetValue(getData.Invoke(dataStore,new object[]{pawn}));}
   catch(Exception e){Log.WarningOnce("[Elemental Hearts] Search and Destroy bridge unavailable: "+e.Message,824614);return false;}
  }
  public static bool IsApproach(Pawn pawn,Job job){
   return job!=null&&!job.playerForced&&job.def==RimWorld.JobDefOf.Goto&&
    job.jobGiver!=null&&job.jobGiver.GetType().Namespace=="SearchAndDestroy"&&Enabled(pawn);
  }
 }
}
