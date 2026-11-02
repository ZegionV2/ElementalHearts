using System;using System.Collections.Generic;using RimWorld;using Verse;
namespace ElementalHearts {
 public static class WaterKitUpgrade {
  public const string DefName="EH_WaterHeart";
  public static bool HasAscension(Pawn pawn){if(pawn==null||pawn.health==null)return false;foreach(var h in pawn.health.hediffSet.hediffs)if(h.def.defName==DefName)return true;return false;}
  public static bool Ensure(HediffWithComps root,HediffDef definition){
   if(root==null||root.def==null||root.def.defName!=DefName||definition==null||definition.defName!=DefName||definition.comps==null)return false;
   bool changed=false;if(!object.ReferenceEquals(root.def,definition)){root.def=definition;changed=true;}if(root.comps==null){root.comps=new List<HediffComp>();changed=true;}
   // Retain the same root, comp list and all existing comp objects, including foreign extensions.
   foreach(var props in definition.comps){HediffComp found=null;foreach(var comp in root.comps)if(comp.GetType()==props.compClass){found=comp;break;}if(found==null){found=(HediffComp)Activator.CreateInstance(props.compClass);root.comps.Add(found);changed=true;}found.parent=root;found.props=props;}
   return changed;
  }
 }
 public sealed class GameComponent_WaterKitMigration:GameComponent {
  public GameComponent_WaterKitMigration(Game game){}
  public override void FinalizeInit(){
   var definition=DefDatabase<HediffDef>.GetNamedSilentFail(WaterKitUpgrade.DefName);if(definition==null)return;int updated=0;
   foreach(var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){if(pawn.health==null)continue;foreach(var h in pawn.health.hediffSet.hediffs){var root=h as HediffWithComps;if(root==null||root.def.defName!=WaterKitUpgrade.DefName)continue;bool changed=WaterKitUpgrade.Ensure(root,definition);if(ElementalAscension.Normalize(root))changed=true;if(changed){pawn.health.Notify_HediffChanged(root);updated++;}if(pawn.Spawned&&pawn.Drawer!=null)pawn.Drawer.renderer.SetAllGraphicsDirty();}}
   if(updated>0)Log.Message("[Elemental Hearts] Added missing Water kit/recovery components to "+updated+" saved Ascendants; existing defense and equipment state retained.");
  }
 }
}
