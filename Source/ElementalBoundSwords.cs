using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public sealed class ElementalSwordBond:IExposable,IThingHolder {
  public Pawn pawn;public ThingWithComps sword;public ThingWithComps previous;public string element;public bool called;
  internal bool changing;
  ThingOwner<Thing> hidden;
  public ElementalSwordBond(){hidden=new ThingOwner<Thing>(this,false,LookMode.Deep);}
  public IThingHolder ParentHolder{get{return pawn;}}
  public ThingOwner GetDirectlyHeldThings(){return hidden;}
  public void GetChildHolders(List<IThingHolder> children){ThingOwnerUtility.AppendThingHoldersFromThings(children,hidden);}
  public void ExposeData(){Scribe_References.Look(ref pawn,"pawn");Scribe_References.Look(ref sword,"sword");Scribe_References.Look(ref previous,"previous");Scribe_Values.Look(ref element,"element");Scribe_Values.Look(ref called,"called");Scribe_Deep.Look(ref hidden,"hiddenSword",this);if(Scribe.mode==LoadSaveMode.PostLoadInit&&hidden==null)hidden=new ThingOwner<Thing>(this,false,LookMode.Deep);}
  public void Park(){
   if(sword==null||sword.Destroyed||pawn==null)return;
   bool was=changing;changing=true;
   try{
    if(pawn.equipment!=null&&pawn.equipment.AllEquipmentListForReading.Contains(sword))pawn.equipment.Remove(sword);
    if(sword.Spawned)sword.DeSpawn();
    if(sword.holdingOwner!=hidden)hidden.TryAddOrTransfer(sword,false);
   }finally{changing=was;}
  }
  public void RestorePrevious(){
   if(previous==null||previous.Destroyed||pawn==null||pawn.Dead||pawn.Downed||pawn.equipment==null||pawn.inventory==null)return;
   if(pawn.equipment.Primary==previous){previous=null;return;}
   if(pawn.equipment.Primary!=null)return;
   if(!pawn.inventory.innerContainer.Contains(previous))return;
   bool was=changing;changing=true;try{pawn.inventory.innerContainer.Remove(previous);pawn.equipment.AddEquipment(previous);previous=null;}finally{changing=was;}
  }
  public void Equip(){
   if(sword==null||sword.Destroyed||pawn==null||pawn.Dead||pawn.Downed||pawn.equipment==null||pawn.inventory==null)return;
   if(pawn.equipment.Primary==sword)return;
   bool was=changing;changing=true;
   try{
    var old=pawn.equipment.Primary;
    if(old!=null){if(previous==null||previous.Destroyed)previous=old;pawn.equipment.Remove(old);if(!pawn.inventory.innerContainer.TryAdd(old,false)){pawn.equipment.AddEquipment(old);return;}}
    if(sword.Spawned)sword.DeSpawn();if(sword.holdingOwner!=null)sword.holdingOwner.Remove(sword);pawn.equipment.AddEquipment(sword);
   }finally{changing=was;}
  }
 }
 public sealed class GameComponent_ElementalSwords:GameComponent {
  sealed class PawnKeys:IEqualityComparer<Pawn>{public bool Equals(Pawn a,Pawn b){return object.ReferenceEquals(a,b);}public int GetHashCode(Pawn p){return RuntimeHelpers.GetHashCode(p);}}
  List<ElementalSwordBond> bonds=new List<ElementalSwordBond>();
  Dictionary<Pawn,ElementalSwordBond> byPawn=new Dictionary<Pawn,ElementalSwordBond>(new PawnKeys());
  public GameComponent_ElementalSwords(Game game){}
  public ElementalSwordBond Find(Pawn p){ElementalSwordBond b;return p!=null&&byPawn.TryGetValue(p,out b)?b:null;}
  public ElementalSwordBond Bind(Pawn p,string element,ThingWithComps existing=null){
   var b=Find(p);if(b==null){b=new ElementalSwordBond {pawn=p,element=element};bonds.Add(b);byPawn.Add(p,b);}if(b.element!=element){b.called=false;b.Park();var transform=HediffComp_ElementalTransform.On(p);if(transform==null||!transform.transformed)b.RestorePrevious();if(b.sword!=null&&!b.sword.Destroyed)b.sword.Destroy();b.sword=null;b.element=element;}
   if(existing!=null&&(b.sword==null||b.sword.Destroyed))b.sword=existing;return b;
  }
  public override void FinalizeInit(){foreach(var p in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead){var c=HediffComp_ElementalTransform.On(p);if(c!=null){c.BindKit();if(p.Dead||p.Downed)c.RevertWhenDowned();}var b=Find(p);if(b!=null&&(p.Dead||p.Downed))b.Park();}}
  public override void ExposeData(){if(Scribe.mode==LoadSaveMode.Saving){for(int i=bonds.Count-1;i>=0;i--){var b=bonds[i];if(b.pawn==null||b.pawn.Destroyed){if(b.sword!=null&&!b.sword.Destroyed)b.sword.Destroy();if(b.pawn!=null)byPawn.Remove(b.pawn);bonds.RemoveAt(i);}}}Scribe_Collections.Look(ref bonds,"EH_swordBonds",LookMode.Deep);if(bonds==null)bonds=new List<ElementalSwordBond>();if(Scribe.mode==LoadSaveMode.PostLoadInit){byPawn.Clear();foreach(var b in bonds)if(b.pawn!=null)byPawn[b.pawn]=b;}}
  public override void GameComponentTick(){
   if(FindTick()%30!=0)return;
   for(int i=bonds.Count-1;i>=0;i--){var b=bonds[i];
    var p=b.pawn;if(p==null||p.Destroyed){if(b.sword!=null&&!b.sword.Destroyed)b.sword.Destroy();bonds.RemoveAt(i);if(p!=null)byPawn.Remove(p);continue;}
    var transform=HediffComp_ElementalTransform.On(p);
    bool hasHeart=ElementalHeartUtility.HasHeart(p),disciple=ElementalTeaching.Trained(p)&&!hasHeart;
    bool wanted=disciple?b.called:hasHeart&&transform!=null&&transform.transformed;
    if(ElementalSwordRules.ShouldHide(p.Dead,p.Downed,wanted)){b.Park();if(!wanted&&!p.Dead&&!p.Downed)b.RestorePrevious();}
    else if(!ElementalAnimationBridge.Controlled(p))b.Equip();
   }
  }
  static int FindTick(){return Verse.Find.TickManager.TicksGame;}
 }
 public static class ElementalSwordRules {
  public static bool ShouldHide(bool dead,bool downed,bool wanted){return dead||downed||!wanted;}
  public static bool WeaponOrder(string name){return name=="Equip"||name=="DropEquipment";}
 }
 public static class ElementalBoundSwords {
  static GameComponent_ElementalSwords Store{get{return Current.Game==null?null:Current.Game.GetComponent<GameComponent_ElementalSwords>();}}
  public static ElementalSwordBond Find(Pawn p){var store=Store;return store==null?null:store.Find(p);}
  public static ElementalSwordBond Register(Pawn p,string element,ThingWithComps sword){return Store==null?null:Store.Bind(p,element,sword);}
  public static bool Internal(Pawn p){var b=Find(p);var c=HediffComp_ElementalTransform.On(p);return b!=null&&b.changing||c!=null&&c.changingEquipment;}
  public static bool Bound(Pawn p,Thing sword){var b=Find(p);return b!=null&&b.sword==sword&&sword!=null&&!sword.Destroyed;}
  public static bool Locked(Pawn p){return p!=null&&p.equipment!=null&&Bound(p,p.equipment.Primary)&&!Internal(p);}
  public static void Suspend(Pawn p){var b=Find(p);if(b!=null)b.Park();}
  public static void BeforeAscension(Pawn p){var b=Find(p);if(b!=null){b.called=false;b.Park();b.RestorePrevious();}}
  public static IEnumerable<Gizmo> Commands(Pawn p,string element){
   var b=Find(p);bool called=b!=null&&b.called;
   var cmd=new Command_Action {defaultLabel=called?"Dismiss elemental sword":"Call elemental sword",defaultDesc="Manifest your own bound "+element.ToLower()+" sword. It cannot be dropped or stripped. It vanishes while you are downed or dead and returns when you recover or revive. Dismiss it to restore your previous weapon.",icon=ElementalIcons.Transform,groupable=false,action=()=>Toggle(p,element)};
   if(!ElementalTeaching.Ready(p)||ElementalAnimationBridge.Controlled(p))cmd.Disable("Must be conscious and able to act.");yield return cmd;
  }
  internal static void EnsureCalled(Pawn p,string element){if(Store==null||p.equipment==null||p.inventory==null)return;var b=Store.Bind(p,element);if(b.sword==null||b.sword.Destroyed){var def=DefDatabase<ThingDef>.GetNamedSilentFail("EH_"+element+"Sword");if(def==null)return;b.sword=(ThingWithComps)ThingMaker.MakeThing(def,def.MadeFromStuff?ThingDefOf.Steel:null);b.Park();}b.called=true;b.Equip();}
  static void Toggle(Pawn p,string element){
   if(!ElementalTeaching.Ready(p)||ElementalAnimationBridge.Controlled(p)||Store==null)return;
   var b=Store.Bind(p,element);
   if(b.called){b.called=false;b.Park();b.RestorePrevious();return;}
   if(b.sword==null||b.sword.Destroyed){var def=DefDatabase<ThingDef>.GetNamedSilentFail("EH_"+element+"Sword");if(def==null)return;b.sword=(ThingWithComps)ThingMaker.MakeThing(def,def.MadeFromStuff?ThingDefOf.Steel:null);b.Park();}
   b.called=true;b.Equip();if(p.Spawned)ElementalAudio.Play(p,"EH_TransformLock");
  }
 }
 [HarmonyPatch(typeof(Pawn),"GetChildHolders")]
 public static class ElementalSwordHolders {
  public static void Postfix(Pawn __instance,List<IThingHolder> outChildren){var b=ElementalBoundSwords.Find(__instance);if(b!=null)outChildren.Add(b);}
 }
 [HarmonyPatch(typeof(Pawn_HealthTracker),"MakeDowned")]
 public static class BoundSwordDowned {public static void Prefix(Pawn ___pawn){var transform=HediffComp_ElementalTransform.On(___pawn);if(transform!=null)transform.RevertWhenDowned();ElementalBoundSwords.Suspend(___pawn);}}
 [HarmonyPatch(typeof(Pawn),"Kill")]
 public static class BoundSwordDeath {public static void Prefix(Pawn __instance){var transform=HediffComp_ElementalTransform.On(__instance);if(transform!=null)transform.RevertWhenDowned();ElementalBoundSwords.Suspend(__instance);}}
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"DropAllEquipment")]
 public static class BoundSwordDropAll {public static void Prefix(Pawn ___pawn){ElementalBoundSwords.Suspend(___pawn);}}
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"TryDropEquipment")]
 public static class BoundSwordNoDrop {
  [HarmonyPriority(Priority.First)]
  public static bool Prefix(Pawn ___pawn,ThingWithComps __0,ref ThingWithComps resultingEq,ref bool __result){if(!ElementalBoundSwords.Bound(___pawn,__0)||ElementalBoundSwords.Internal(___pawn))return true;resultingEq=null;__result=false;return false;}
 }
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"TryTransferEquipmentToContainer")]
 public static class BoundSwordNoTransfer {public static bool Prefix(Pawn ___pawn,ThingWithComps __0,ref bool __result){if(!ElementalBoundSwords.Bound(___pawn,__0)||ElementalBoundSwords.Internal(___pawn))return true;__result=false;return false;}}
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"Remove")]
 public static class BoundSwordNoRemove {public static bool Prefix(Pawn ___pawn,ThingWithComps __0){return !ElementalBoundSwords.Bound(___pawn,__0)||ElementalBoundSwords.Internal(___pawn);}}
 [HarmonyPatch(typeof(Pawn_EquipmentTracker),"AddEquipment")]
 public static class BoundSwordNoReplace {public static bool Prefix(Pawn ___pawn,ThingWithComps __0){return !ElementalBoundSwords.Locked(___pawn)||ElementalBoundSwords.Bound(___pawn,__0);}}
 [HarmonyPatch(typeof(Pawn_JobTracker),"TryTakeOrderedJob")]
 public static class BoundSwordRejectOrder {public static bool Prefix(Pawn ___pawn,Job job,ref bool __result){if(job==null||job.def==null||!ElementalSwordRules.WeaponOrder(job.def.defName)||!ElementalBoundSwords.Locked(___pawn))return true;Messages.Message("Dismiss your elemental sword before equipping another weapon.",___pawn,MessageTypeDefOf.RejectInput,false);__result=false;return false;}}
}
