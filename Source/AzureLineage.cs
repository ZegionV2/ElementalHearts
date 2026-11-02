using System;using System.Collections.Generic;using RimWorld;using Verse;
namespace ElementalHearts {
 public static class AzureLineage {
  public static void Rebind(HediffWithComps root,HediffDef destination){
   var old=root.comps??new List<HediffComp>();var rebound=new List<HediffComp>();
   foreach(var props in destination.comps){HediffComp retained=null;foreach(var c in old)if(c.GetType()==props.compClass){retained=c;break;}if(retained==null)retained=(HediffComp)Activator.CreateInstance(props.compClass);retained.parent=root;retained.props=props;rebound.Add(retained);}
   // Upgrade the same saved hediff object, preserving its load ID, severity, age and comp state.
   root.def=destination;root.comps=rebound;AscendantBenefits.Cleanse(root.pawn);
  }
  public static bool Upgrade(Pawn pawn){
   if(pawn==null||pawn.health==null)return false;HediffWithComps flame=null,azure=null;
   foreach(var h in pawn.health.hediffSet.hediffs){if(h.def.defName=="EH_FlameHeart")flame=h as HediffWithComps;else if(h.def.defName==AzureEvolution.DefName)azure=h as HediffWithComps;}
   if(flame==null)return azure!=null;
   Rebind(flame,DefDatabase<HediffDef>.GetNamed(AzureEvolution.DefName));
   if(azure!=null){
    // Older versions stored an extra Azure marker. Its fresh/duplicate comps must not run removal callbacks against the retained kit.
    azure.comps=new List<HediffComp>();pawn.health.RemoveHediff(azure);
   }
   pawn.health.Notify_HediffChanged(flame);
   var transform=flame.TryGetComp<HediffComp_ElementalTransform>();if(transform!=null)transform.RebuildAzureKit();
   if(pawn.Spawned){foreach(var comp in flame.comps)comp.Notify_Spawned();AzureVfx.Track(pawn);}
   return true;
  }
 }
 public sealed class GameComponent_AzureLineageMigration:GameComponent {
  public GameComponent_AzureLineageMigration(Game game){}
  public override void FinalizeInit(){int migrated=0;foreach(var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){if(AzureEvolution.Active(pawn)){bool old=false;foreach(var h in pawn.health.hediffSet.hediffs)if(h.def.defName=="EH_FlameHeart"){old=true;break;}if(old&&AzureLineage.Upgrade(pawn))migrated++;}}if(migrated>0)Log.Message("[Elemental Hearts] Consolidated "+migrated+" Azure Ascendants into a single evolved condition; existing comp state preserved.");}
 }
}
