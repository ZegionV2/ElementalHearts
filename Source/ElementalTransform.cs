using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public sealed class HediffCompProperties_ElementalTransform:HediffCompProperties {public HediffCompProperties_ElementalTransform(){compClass=typeof(HediffComp_ElementalTransform);}}
 public sealed class HediffComp_ElementalTransform:HediffComp,IThingHolder {
  internal bool changingEquipment;
  ThingOwner<Thing> dormantKit;bool reverting;
  internal ThingOwner<Thing> DormantKit{get{return dormantKit??(dormantKit=new ThingOwner<Thing>(this,false,LookMode.Deep));}}
  public IThingHolder ParentHolder{get{return Pawn;}}
  public ThingOwner GetDirectlyHeldThings(){return DormantKit;}
  public void GetChildHolders(List<IThingHolder> children){ThingOwnerUtility.AppendThingHoldersFromThings(children,DormantKit);}
  internal bool IsKit(Thing t){return kit.Contains(t);}
  internal float Coverage{get{return ManifestationRules.Coverage(PulseRemaining,reverting);}}
  internal IEnumerable<Apparel> PersonalApparel(){if(reverting&&stage>=2){if(Pawn.apparel!=null)foreach(var a in Pawn.apparel.WornApparel)yield return a;}else foreach(var t in previous){var a=t as Apparel;if(a!=null&&!a.Destroyed)yield return a;}}
  internal bool StyleTransition(bool beard,out bool kitHides){kitHides=false;foreach(var t in kit){var a=t as Apparel;if(a!=null&&!a.Destroyed&&ManifestHairRules.Conceals(a.def.apparel,beard)){kitHides=true;break;}}bool personalHides=false;foreach(var a in PersonalApparel())if(ManifestHairRules.Conceals(a.def.apparel,beard)){personalHides=true;break;}return kitHides!=personalHides;}
  internal bool OtherApparelConceals(bool beard){if(Pawn.apparel==null)return false;foreach(var a in Pawn.apparel.WornApparel){if(IsKit(a))continue;bool personal=false;foreach(var p in PersonalApparel())if(p==a){personal=true;break;}if(!personal&&ManifestHairRules.Conceals(a.def.apparel,beard))return true;}return false;}
  internal IEnumerable<Apparel> VisualApparel(){if(stage>=2)yield break;foreach(var t in reverting?previous:kit)if(t is Apparel&&!t.Destroyed&&!Wearing(t))yield return (Apparel)t;}
  public bool transformed;int finishAt,stage,kitVersion;HairDef oldHair;
  List<Thing> kit=new List<Thing>();List<Thing> previous=new List<Thing>();bool supplied;
  string Element{get{if(parent.def.defName==CelestialAscendant.DefName)return "Celestial";if(parent.def.defName==CycloneEvolution.DefName)return "Wind";if(parent.def.defName==GlacialEvolution.DefName)return "Water";if(parent.def.defName==AzureEvolution.DefName)return "Flame";if(parent.def.defName==ThunderstormEvolution.DefName)return "Lightning";return parent.def.defName.Replace("EH_","").Replace("Heart","");}}
  string EquipmentElement{get{return EvolvedLineage.Equipment(Pawn,Element);}}
  internal int PulseRemaining{get{return finishAt>0?finishAt-Find.TickManager.TicksGame:0;}}
  internal string PulseElement{get{return EquipmentElement;}}
  public static bool Active(Pawn p){var c=On(p);return c!=null&&c.transformed;}
  public static bool Transforming(Pawn p){var c=On(p);return c!=null&&c.finishAt>0;}
  internal static HediffComp_ElementalTransform On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_ElementalTransform>();if(c!=null)return c;}return null;}
  public override void CompExposeData(){Scribe_Deep.Look(ref dormantKit,"transformDormantKit",this);Scribe_Values.Look(ref reverting,"transformReverting");Scribe_Values.Look(ref transformed,"transformed");Scribe_Values.Look(ref supplied,"transformSupplied");Scribe_Values.Look(ref finishAt,"transformFinish");Scribe_Values.Look(ref stage,"transformStage");Scribe_Values.Look(ref kitVersion,"transformKitVersion");Scribe_Defs.Look(ref oldHair,"transformOldHair");Scribe_Collections.Look(ref kit,"transformKit",LookMode.Reference);Scribe_Collections.Look(ref previous,"transformPrevious",LookMode.Reference);if(kit==null)kit=new List<Thing>();if(previous==null)previous=new List<Thing>();}
  internal bool Ready{get{return !ElementalTeachingSession.Busy(Pawn)&&!AzureEvolution.Evolving(Pawn)&&ElementalTeaching.Ready(Pawn)&&Pawn.apparel!=null&&Pawn.equipment!=null&&Pawn.inventory!=null&&!ElementalAnimationBridge.Controlled(Pawn);}}
  bool Wearing(Thing t){return t!=null&&!t.Destroyed&&(Pawn.apparel.WornApparel.Contains(t as Apparel)||Pawn.equipment.Primary==t);}
  bool KitPresent{get{if(!supplied)return true;if(kit.Count!=(EvolvedLineage.Active(Pawn)?2:3))return false;foreach(var t in kit)if(t==null||t.Destroyed||(!DormantKit.Contains(t)&&!Pawn.inventory.innerContainer.Contains(t)&&!Wearing(t)&&!ElementalBoundSwords.Bound(Pawn,t)))return false;return true;}}
  bool Available{get{return DefDatabase<ThingDef>.GetNamedSilentFail("EH_"+EquipmentElement+"Uniform")!=null&&(EvolvedLineage.Active(Pawn)||DefDatabase<ThingDef>.GetNamedSilentFail("EH_"+Element+"Helmet")!=null)&&DefDatabase<ThingDef>.GetNamedSilentFail("EH_"+EquipmentElement+"Sword")!=null;}}
  void Stow(Thing t){changingEquipment=true;try{if(t is Apparel)Pawn.apparel.Remove((Apparel)t);else Pawn.equipment.Remove((ThingWithComps)t);Pawn.inventory.innerContainer.TryAdd(t);}finally{changingEquipment=false;}}
  void Equip(Thing t){changingEquipment=true;try{if(t==null||t.Destroyed)return;if(DormantKit.Contains(t))DormantKit.Remove(t);else if(Pawn.inventory.innerContainer.Contains(t))Pawn.inventory.innerContainer.Remove(t);else if(ElementalBoundSwords.Bound(Pawn,t)&&t.holdingOwner!=null)t.holdingOwner.Remove(t);else return;if(t is Apparel)Pawn.apparel.Wear((Apparel)t,false,false);else Pawn.equipment.AddEquipment((ThingWithComps)t);}finally{changingEquipment=false;}}
  Thing Create(string suffix){var def=DefDatabase<ThingDef>.GetNamed("EH_"+EquipmentElement+suffix);var t=ThingMaker.MakeThing(def,def.MadeFromStuff?ThingDefOf.Steel:null);DormantKit.TryAdd(t);return t;}
  void ParkKit(){changingEquipment=true;try{foreach(var t in kit){if(t==null||t.Destroyed)continue;if(Wearing(t)){if(t is Apparel)Pawn.apparel.Remove((Apparel)t);else Pawn.equipment.Remove((ThingWithComps)t);}if(Pawn.inventory.innerContainer.Contains(t))DormantKit.TryAddOrTransfer(t);else if(t.holdingOwner==null&&!t.Spawned)DormantKit.TryAdd(t);}}finally{changingEquipment=false;}}
  internal void BindKit(){foreach(var t in kit)if(t is ThingWithComps&&t.def!=null&&t.def.defName=="EH_"+EquipmentElement+"Sword"){ElementalBoundSwords.Register(Pawn,EquipmentElement,(ThingWithComps)t);break;}}
  public override void Notify_Spawned(){BindKit();if(NpcDisciples.AutoNpc(Pawn))ManifestInstant();}
  void EnsureKit(){
   if(!supplied){foreach(string suffix in EvolvedLineage.Active(Pawn)?new[]{"Uniform","Sword"}:new[]{"Uniform","Helmet","Sword"}){var bond=ElementalBoundSwords.Find(Pawn);kit.Add(suffix=="Sword"&&bond!=null&&bond.element==EquipmentElement&&bond.sword!=null&&!bond.sword.Destroyed?bond.sword:Create(suffix));}supplied=true;kitVersion=1;}
   BindKit();
  }

  internal void RebuildAzureKit(){
   if(!EvolvedLineage.Active(Pawn)||!Available)return;
   if(supplied&&kit.Count==2&&kit[0]!=null&&kit[0].def.defName=="EH_"+EquipmentElement+"Uniform"){BindKit();return;}
   var old=new List<Thing>(kit);var fresh=new List<Thing>();
   // Allocate replacement projections first; do not touch personal equipment or saved original clothing.
   try{fresh.Add(Create("Uniform"));fresh.Add(Create("Sword"));}catch{foreach(var item in fresh)if(!item.Destroyed)item.Destroy();throw;}
   foreach(var next in fresh){string suffix=next.def.IsWeapon?"Sword":"Uniform";foreach(var prior in old){if(prior==null||prior.Destroyed||!prior.def.defName.EndsWith(suffix))continue;float ratio=prior.HitPoints/(float)System.Math.Max(1,prior.MaxHitPoints);next.HitPoints=System.Math.Max(1,Mathf.RoundToInt(next.MaxHitPoints*ratio));var oldQ=prior.TryGetComp<CompQuality>();var newQ=next.TryGetComp<CompQuality>();if(oldQ!=null&&newQ!=null)newQ.SetQuality(oldQ.Quality,ArtGenerationContext.Colony);break;}}
   ParkKit();foreach(var prior in old)if(prior!=null&&!prior.Destroyed)prior.Destroy();kit=fresh;supplied=true;kitVersion=2;BindKit();if(transformed)foreach(var item in kit)Equip(item);Pawn.Drawer.renderer.SetAllGraphicsDirty();
  }
  // Upgrade old saves once. Cosmetic visors remain owned; do not destroy or duplicate them.
  void Migrate(){
   if(Pawn.inventory==null||Pawn.apparel==null||Pawn.equipment==null)return;
   if(kitVersion>=1||!Available){if(!transformed)ParkKit();return;}
   if(oldHair!=null&&Pawn.story!=null){Pawn.story.hairDef=oldHair;oldHair=null;Pawn.Drawer.renderer.SetAllGraphicsDirty();}
   if(supplied){for(int i=0;i<kit.Count;i++){var t=kit[i];if(t!=null&&t.def.defName=="EH_"+Element+"Visor"){if(Wearing(t))Stow(t);else if(DormantKit.Contains(t))Pawn.inventory.innerContainer.TryAddOrTransfer(t);kit[i]=Create("Helmet");if(transformed)Equip(kit[i]);}}}
   kitVersion=1;if(!transformed)ParkKit();
  }
  void Change(){
   if(!Ready||(!transformed&&!KitPresent))return;
   if(transformed){RestorePersonalEquipment();}
   else{
    EnsureKit();
    previous.Clear();foreach(var a in new List<Apparel>(Pawn.apparel.WornApparel)){previous.Add(a);Stow(a);}if(Pawn.equipment.Primary!=null){var weapon=Pawn.equipment.Primary;previous.Add(weapon);Stow(weapon);}
    foreach(var t in kit)Equip(t);transformed=true;
   }
   Pawn.Drawer.renderer.SetAllGraphicsDirty();
  }
  internal bool RestorePersonalEquipment(){
   if(!transformed||Pawn.apparel==null||Pawn.equipment==null||Pawn.inventory==null)return false;
   // Shared manual/emergency reversion preserves the same projections and personal equipment.
   changingEquipment=true;try{
    ParkKit();foreach(var a in new List<Apparel>(Pawn.apparel.WornApparel))Stow(a);
    if(Pawn.equipment.Primary!=null)Stow(Pawn.equipment.Primary);
    foreach(var item in previous)Equip(item);previous.Clear();transformed=false;
   }finally{changingEquipment=false;}
   return true;
  }
  internal bool ManifestInstant(){if(transformed)return true;if(!Ready||finishAt>0||!Available||!KitPresent)return false;Migrate();EnsureKit();reverting=false;Change();return transformed;}
  internal void RevertWhenDowned(){
   Cancel();if(!RestorePersonalEquipment())return;reverting=false;
   if(Pawn.Drawer!=null)Pawn.Drawer.renderer.SetAllGraphicsDirty();
  }
  public override void CompPostPostRemoved(){Cancel();if(Pawn.apparel!=null&&Pawn.equipment!=null&&Pawn.inventory!=null){changingEquipment=true;try{if(transformed){ParkKit();foreach(var t in previous)Equip(t);previous.Clear();transformed=false;}foreach(var t in kit)if(t!=null&&!t.Destroyed)t.Destroy();kit.Clear();}finally{changingEquipment=false;}}}
  public override void CompPostTickInterval(ref float adjustment,int delta){
   if(Pawn.Downed||Pawn.Dead){RevertWhenDowned();return;}
   if(EvolvedLineage.Active(Pawn)&&supplied&&(kit.Count!=2||kit[0]==null||kit[0].def.defName!="EH_"+EquipmentElement+"Uniform"))RebuildAzureKit();
   if(ElementalBoundSwords.Find(Pawn)==null)BindKit();
   if(kitVersion<2){if(finishAt>0)reverting=stage<2?transformed:!transformed;Migrate();if(kitVersion>=1)kitVersion=2;}if(finishAt==0){if(NpcDisciples.AutoNpc(Pawn)&&!transformed)ManifestInstant();return;}if(!Ready||Pawn.CurJob==null||Pawn.CurJob.def.defName!="EH_Transform"){Cancel();return;}
   Pawn.Map.GetComponent<MapComponent_TransformPulse>().Track(Pawn);
   int left=finishAt-Find.TickManager.TicksGame;
   if(stage==0&&left<=60){stage=1;ElementalAudio.Play(Pawn,"EH_TransformCharge");}
   if(stage==1&&left<=30){stage=2;Change();ElementalAudio.Transformation(Pawn,Element,transformed);}
   if(left<=0)Cancel();
  }
  internal void Cancel(){bool dirty=finishAt>0;finishAt=0;stage=0;if(dirty&&Pawn.Drawer!=null)Pawn.Drawer.renderer.SetAllGraphicsDirty();}
  internal void Begin(){
   Migrate();if(!Ready||!Available||(!transformed&&!KitPresent))return;
   EnsureKit();reverting=transformed;
   if(!transformed){previous.Clear();foreach(var a in Pawn.apparel.WornApparel)previous.Add(a);if(Pawn.equipment.Primary!=null)previous.Add(Pawn.equipment.Primary);}
   Pawn.pather.StopDead();Pawn.Rotation=Rot4.South;
   Pawn.Map.GetComponent<MapComponent_TransformPulse>().Track(Pawn);
   finishAt=Find.TickManager.TicksGame+90;stage=0;if(CelestialAscendant.Active(Pawn))CelestialVfx.Manifest(Pawn,90);Pawn.Drawer.renderer.SetAllGraphicsDirty();ElementalAudio.Play(Pawn,"EH_TransformStart");
  }
  public override IEnumerable<Gizmo> CompGetGizmos(){if(!ElementalCombatPolicy.Controlled(Pawn))yield break;
   var c=new Command_Action{defaultLabel=transformed?"Revert":"Transform",defaultDesc=(EvolvedLineage.Active(Pawn)?"Manifest your evolved royal armor and your bound sword, without a helmet. The elemental halo floats above your visible head. ":"Transform into your elemental armor, full-face helmet and sword. ")+" Stand still facing forward until the transformation finishes. Hair is unchanged. Previous equipment is kept in inventory and restored on reverting. Equipment is locked while transformed; revert before changing clothes or weapons. The manifested kit disappears into the heart on reverting, without occupying inventory; quality persists; manifested armor and swords are indestructible. A downed pawn reverts to personal clothing and can transform again after recovery. "+(EvolvedLineage.Active(Pawn)?"The halo appears only while transformed and drafted.":"Auras require transformation and drafting."),icon=ElementalIcons.Transform,groupable=false,action=()=>{if(!Ready||finishAt>0)return;Pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("EH_Transform")),JobTag.Misc);}};
   if(!Available)c.Disable("This element's transformation equipment is not available yet.");else if(!Ready)c.Disable("Must be awake and free of a controlled animation.");else if(!transformed&&!KitPresent)c.Disable("Recover the issued transformation equipment into this pawn's inventory first.");else if(finishAt>0)c.Disable("Transforming...");yield return c;
  }
 }
}

namespace ElementalHearts {
 [HarmonyPatch(typeof(Pawn),"GetChildHolders")]
 public static class ElementalHiddenKitHolders {
  public static void Postfix(Pawn __instance,List<IThingHolder> outChildren){var c=HediffComp_ElementalTransform.On(__instance);if(c!=null)outChildren.Add(c);}
 }
 public static class ManifestationRules {
  public static float Coverage(int remaining,bool reverting){float t=System.Math.Max(0f,System.Math.Min(1f,(90-remaining)/60f));float smooth=t*t*(3f-2f*t);return reverting?1f-smooth:smooth;}
 }
}
