using System;using System.IO;using System.Reflection;using System.Runtime.Serialization;using System.Collections.Generic;
using ElementalHearts;using RimWorld;using Verse;
class RegalRegression {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static T Empty<T>(){return (T)FormatterServices.GetUninitializedObject(typeof(T));}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Set(object o,string f,object v){o.GetType().GetField(f,F).SetValue(o,v);}
 static object Get(object o,string f){return o.GetType().GetField(f,F).GetValue(o);}
 static int Main(string[] args){AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string n=new AssemblyName(e.Name).Name;string p=n=="ElementalHearts"?args[1]:n=="0Harmony"?args[2]:Path.Combine(args[0],n+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};try{typeof(RegalRegression).GetMethod("Run",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
 static void Run(){
  foreach(bool evolved in new[]{false,true})foreach(bool drafted in new[]{false,true})foreach(bool transformed in new[]{false,true})foreach(bool changing in new[]{false,true})Check(AzureHaloRules.Visible(evolved,drafted,transformed,changing)==(evolved&&drafted&&transformed&&!changing),"Halo transformed/drafted visibility policy");
  var lineagePawn=Empty<Pawn>();lineagePawn.health=Empty<Pawn_HealthTracker>();lineagePawn.health.hediffSet=Empty<HediffSet>();lineagePawn.health.hediffSet.hediffs=new List<Hediff>();
  var root=new HediffWithComps();root.pawn=lineagePawn;root.loadID=31337;root.ageTicks=777777;Set(root,"severityInt",1.25f);root.def=Empty<HediffDef>();root.def.defName="EH_FlameHeart";
  var oldAbilities=new HediffComp_FlameAbilities {autoCombat=true,nonLethal=true};oldAbilities.parent=root;oldAbilities.props=new HediffCompProperties_FlameAbilities();var oldCooldowns=(List<int>)Get(oldAbilities,"readyAt");oldCooldowns[1]=12345;
  var oldTransform=new HediffComp_ElementalTransform {transformed=true};oldTransform.parent=root;oldTransform.props=new HediffCompProperties_ElementalTransform();var oldKit=Get(oldTransform,"kit");
  var oldPassives=new HediffComp_FlamePassives();oldPassives.parent=root;oldPassives.props=new HediffCompProperties_FlamePassives();Set(oldPassives,"counterReadyAt",9876);
  var oldMastery=new HediffComp_FlameMastery {kills=153};oldMastery.parent=root;oldMastery.props=new HediffCompProperties_FlameMastery();
  root.comps=new List<HediffComp>{oldAbilities,oldTransform,oldPassives,oldMastery};lineagePawn.health.hediffSet.hediffs.Add(root);
  var evolvedDef=Empty<HediffDef>();evolvedDef.defName=AzureEvolution.DefName;evolvedDef.tags=new List<string>{ElementalHeartUtility.HeartTag};evolvedDef.comps=new List<HediffCompProperties>{new HediffCompProperties_ElementalTransform(),new HediffCompProperties_FlameAbilities(),new HediffCompProperties_FlamePassives(),new HediffCompProperties_FlameMastery(),new HediffCompProperties_AzureHalo()};
  AzureLineage.Rebind(root,evolvedDef);
  Check(root.def==evolvedDef&&root.loadID==31337&&root.ageTicks==777777&&root.Severity==1.25f,"Lineage upgrade reset saved hediff identity/age/severity");
  Check(root.TryGetComp<HediffComp_FlameAbilities>()==oldAbilities&&object.ReferenceEquals(Get(oldAbilities,"readyAt"),oldCooldowns)&&oldCooldowns[1]==12345&&oldAbilities.autoCombat&&oldAbilities.nonLethal,"Lineage upgrade reset ability state");
  Check(root.TryGetComp<HediffComp_ElementalTransform>()==oldTransform&&oldTransform.transformed&&object.ReferenceEquals(Get(oldTransform,"kit"),oldKit)&&root.TryGetComp<HediffComp_FlamePassives>()==oldPassives&&(int)Get(oldPassives,"counterReadyAt")==9876&&oldMastery.kills==153,"Lineage upgrade reset transformation/passive/mastery state");
  Check(root.TryGetComp<HediffComp_AzureHalo>()!=null&&root.comps.Count==5&&AzureEvolution.HasFlame(lineagePawn)&&ElementalTeaching.Element(lineagePawn)=="Flame"&&ElementalHeartUtility.HasHeart(lineagePawn),"Standalone Azure lost flame lineage, teaching or one-element eligibility");
  AzureLineage.Rebind(root,evolvedDef);Check(root.comps.Count==5&&root.TryGetComp<HediffComp_FlameAbilities>()==oldAbilities,"Repeated consolidation duplicated or replaced comps");
  Check(lineagePawn.health.hediffSet.hediffs.Count==1&&lineagePawn.health.hediffSet.hediffs[0].def.defName!= "EH_FlameHeart","Old Flame condition retained after in-place evolution");
  Console.WriteLine("PASS: standalone Azure upgrade preserves hediff ID/severity/age, cooldown list, automation, transformed kit references, passive timers and mastery; repeat-safe comp binding, lineage/teaching identity, and transformed/drafted-only halo policy.");

  foreach(bool a in new[]{false,true})foreach(bool b in new[]{false,true})foreach(bool c in new[]{false,true})foreach(bool d in new[]{false,true})foreach(int dist in new[]{1,2,3})Check(TeachingPairRules.Valid(a,b,c,d,dist)==(a&&b&&c&&d&&dist<=2),"Teaching pair ownership/distance gate");
  foreach(bool target in new[]{false,true})foreach(bool ascendant in new[]{false,true})foreach(bool evolved in new[]{false,true})Check(AzureDefenseRules.WardApplies(target,ascendant,evolved)==(target&&ascendant&&!evolved),"Ward scope");
  var devParent=new HediffWithComps();devParent.ageTicks=10;var devMastery=new HediffComp_FlameMastery();devMastery.parent=devParent;
  typeof(HediffComp_FlameMastery).GetMethod("MakeEvolutionReady",F).Invoke(devMastery,null);Check(devMastery.kills==150&&devParent.ageTicks==600000,"DEV readiness milestones");devMastery.kills=170;devParent.ageTicks=700000;typeof(HediffComp_FlameMastery).GetMethod("MakeEvolutionReady",F).Invoke(devMastery,null);Check(devMastery.kills==170&&devParent.ageTicks==700000,"DEV readiness lowered existing progress");
  Pawn victimAzure=Empty<Pawn>();victimAzure.health=Empty<Pawn_HealthTracker>();victimAzure.health.hediffSet=Empty<HediffSet>();victimAzure.health.hediffSet.hediffs=new List<Hediff>();var azureH=new HediffWithComps();azureH.def=Empty<HediffDef>();azureH.def.defName=AzureEvolution.DefName;victimAzure.health.hediffSet.hediffs.Add(azureH);
  Pawn regularAscendant=Empty<Pawn>();regularAscendant.health=Empty<Pawn_HealthTracker>();regularAscendant.health.hediffSet=Empty<HediffSet>();regularAscendant.health.hediffSet.hediffs=new List<Hediff>();var regularH=new HediffWithComps();regularH.def=Empty<HediffDef>();regularH.def.tags=new List<string>{ElementalHeartUtility.HeartTag};regularH.def.defName="EH_FlameHeart";regularAscendant.health.hediffSet.hediffs.Add(regularH);
  var wardHit=new DamageInfo(null,100,0,-1,regularAscendant);AzureAscendantWard.Prefix(victimAzure,ref wardHit);Check(wardHit.Amount==25,"Actual ward failed to reduce normal ascendant hit");
  regularAscendant.health.hediffSet.hediffs.Add(azureH);var peerHit=new DamageInfo(null,100,0,-1,regularAscendant);AzureAscendantWard.Prefix(victimAzure,ref peerHit);Check(peerHit.Amount==100,"Unevolved ward incorrectly applied to evolved attacker");
  victimAzure.health.hediffSet.hediffs.Add(regularH);var worker=new ThoughtWorker_FlameResolve();var state=(ThoughtState)typeof(ThoughtWorker_FlameResolve).GetMethod("CurrentStateInternal",F).Invoke(worker,new object[]{victimAzure});var thought=Empty<ThoughtDef>();thought.stages=new List<ThoughtStage>{new ThoughtStage {baseMoodEffect=30},new ThoughtStage {baseMoodEffect=50}};Check(state.StageIndexFor(thought)==1,"Azure mood did not choose +50 replacement stage");
  var teacherDriver=new JobDriver_ElementalTeach();teacherDriver.pawn=regularAscendant;teacherDriver.job=new Verse.AI.Job();var lessonToils=(IEnumerable<Verse.AI.Toil>)typeof(JobDriver_ElementalTeach).GetMethod("MakeNewToils",F).Invoke(teacherDriver,null);var steps=new List<Verse.AI.Toil>(lessonToils);Check(steps.Count==4&&steps[1].initAction!=null&&steps[2].defaultDuration==180&&steps[2].handlingFacing,"Teaching must initialize paired job in separate instant step before held lesson");
  var pupilDriver=new JobDriver_ReceiveElementalTeaching();pupilDriver.pawn=regularAscendant;pupilDriver.job=new Verse.AI.Job();var pupilToils=(IEnumerable<Verse.AI.Toil>)typeof(JobDriver_ReceiveElementalTeaching).GetMethod("MakeNewToils",F).Invoke(pupilDriver,null);var held=new List<Verse.AI.Toil>(pupilToils);Check(held.Count==1&&held[0].defaultCompleteMode==Verse.AI.ToilCompleteMode.Never&&held[0].handlingFacing&&held[0].tickAction!=null,"Pupil receiving job must hold still with facing updates");
  Console.WriteLine("PASS: DEV readiness/idempotence, actual normal-ascendant ward and evolved-peer exclusion, +50 mood stage, pair ownership gates, pre-initialized 180-tick lesson and held receiving job scaffolding.");

  Check(AzureAnimationRules.Frame(4,.9f)==1&&AzureAnimationRules.Frame(3,.9f)==5&&AzureAnimationRules.Frame(0,0)==0,"Telegraph must not play impact frames early");
  foreach(int kills in new[]{0,149,150,151})foreach(int age in new[]{0,599999,600000,720000})Check(AzureRules.Ready(kills,age)==(kills>=150&&age>=600000),"Azure evolution requires both milestones");
  foreach(bool hostile in new[]{false,true})foreach(bool animal in new[]{false,true})foreach(bool self in new[]{false,true})Check(AzureRules.CountKill(hostile,animal,self)==(hostile&&!animal&&!self),"Azure mastery eligibility");
  var mastery=new HediffComp_FlameMastery();var record=typeof(HediffComp_FlameMastery).GetMethod("RecordKill",F);
  Check((bool)record.Invoke(mastery,new object[]{101,true})&&!(bool)record.Invoke(mastery,new object[]{101,true})&&!(bool)record.Invoke(mastery,new object[]{102,false})&&mastery.kills==1,"Resurrected/repeated target counted twice or invalid target counted");
  Check(Math.Abs(3*AzureRules.PulseShare(3)-1)<.00001,"Pyre pulses multiply damage budget");
  Check(AzureEvolution.Tuning.flashRange==6&&AzureEvolution.Tuning.danceRadius==6&&AzureEvolution.Tuning.heavenCooldown==1890,"Azure range/cooldown tuning");
  Check(Math.Abs(AzureEvolution.Tuning.flashPhysical+AzureEvolution.Tuning.flashThermal-2.4f)<.0001,"Comet damage budget");
  Check(ElementalBalance.Affected("EH_AzureFlameDamage",false,true),"Azure damage bypasses existing reduction");
  Pawn azurePawn=Empty<Pawn>();azurePawn.health=Empty<Pawn_HealthTracker>();Set(azurePawn.health,"pawn",azurePawn);Set(azurePawn.health,"healthState",PawnHealthState.Mobile);azurePawn.health.hediffSet=Empty<HediffSet>();azurePawn.health.hediffSet.hediffs=new List<Hediff>();
  var baseH=new HediffWithComps();baseH.pawn=azurePawn;baseH.def=Empty<HediffDef>();baseH.def.defName="EH_FlameHeart";baseH.def.tags=new List<string>{ElementalHeartUtility.HeartTag};baseH.comps=new List<HediffComp>();azurePawn.health.hediffSet.hediffs.Add(baseH);
  var powers=new HediffComp_FlameAbilities();powers.parent=baseH;powers.props=new HediffCompProperties_FlameAbilities();var originalProps=powers.Props;var cooldowns=(List<int>)Get(powers,"readyAt");cooldowns[0]=100;cooldowns[3]=900;powers.autoCombat=true;powers.nonLethal=true;
  Check(!AzureEvolution.Active(azurePawn)&&!AzureEvolution.Active(null)&&AzureEvolution.HasFlame(azurePawn),"Ordinary Flame identity");
  var blue=new HediffWithComps();blue.pawn=azurePawn;blue.def=Empty<HediffDef>();blue.def.defName=AzureEvolution.DefName;azurePawn.health.hediffSet.hediffs.Add(blue);
  Check(AzureEvolution.Active(azurePawn)&&powers.Props==AzureEvolution.Tuning&&cooldowns[0]==100&&cooldowns[3]==900&&object.ReferenceEquals(cooldowns,Get(powers,"readyAt"))&&powers.autoCombat&&powers.nonLethal,"Azure upgrade reset cooldown list/automation or did not replace tuning");
  var transform=new HediffComp_ElementalTransform();transform.parent=baseH;Check((string)typeof(HediffComp_ElementalTransform).GetProperty("EquipmentElement",F).GetValue(transform,null)=="AzureFlame","Azure texture/equipment identity");
  foreach(string suffix in new[]{"Uniform","Sword"}){var def=Empty<ThingDef>();def.defName="EH_AzureFlame"+suffix;DefDatabase<ThingDef>.Add(def);}
  Check((bool)typeof(HediffComp_ElementalTransform).GetProperty("Available",F).GetValue(transform,null),"Azure transformation still requires a helmet");
  azurePawn.health.hediffSet.hediffs.Remove(blue);Check(object.ReferenceEquals(powers.Props,originalProps),"Ordinary Flame tuning changed");
  string savePath=Path.Combine(Path.GetTempPath(),"EH_AzureState_"+Guid.NewGuid().ToString("N")+".xml");
  var saved=new AzureSaveCase {pyre=new AzurePyre {cell=new IntVec3(7,0,8),due=1234,pulses=2,damage=15},mastery=mastery};
  Scribe.saver.InitSaving(savePath,"AzureTest");Scribe_Deep.Look(ref saved,"state");Scribe.saver.FinalizeSaving();
  var savedXml=new System.Xml.XmlDocument();savedXml.Load(savePath);
  Check(savedXml.SelectSingleNode("AzureTest/state/pyre/due").InnerText=="1234"&&savedXml.SelectSingleNode("AzureTest/state/pyre/pulses").InnerText=="2"&&savedXml.SelectSingleNode("AzureTest/state/EH_azureMasteryVictims/li").InnerText=="101","Native Scribe save output omitted pending state or unique victim IDs");File.Delete(savePath);
  Console.WriteLine("PASS: native Scribe save output includes pyre timing/remaining pulses and mastery count/unique-victim IDs. Native loading is not covered in this headless runtime.");
  Console.WriteLine("PASS: evolution threshold boundaries, hostile-only unique mastery, shared pyre budget, Azure tuning, preserved cooldown identity/automation, normal Flame isolation and helmet-free availability.");

  foreach(int kills in new[]{0,49,50,51})foreach(int ticks in new[]{0,299999,300000,360000})Check(DiscipleProgressRules.Eligible(kills,ticks)==(kills>=50&&ticks>=300000),"Both ascension requirements");
  foreach(bool dead in new[]{false,true})foreach(bool downed in new[]{false,true})foreach(bool wanted in new[]{false,true})Check(ElementalSwordRules.ShouldHide(dead,downed,wanted)==(dead||downed||!wanted),"Sword life-cycle gate");
  Check(ElementalBalance.DamageMultiplier==.8f && ElementalBalance.Affected("Cut",true,false) && ElementalBalance.Affected("EH_Flame",false,true) && !ElementalBalance.Affected("Bullet",false,true) && !ElementalBalance.Affected("Bomb",false,false),"Modest damage scope");
  Check(ManifestationRules.Coverage(90,false)==0 && ManifestationRules.Coverage(30,false)==1,"Reveal endpoints");
  float last=0;for(int remaining=90;remaining>=0;remaining--){float c=ManifestationRules.Coverage(remaining,false),r=ManifestationRules.Coverage(remaining,true);Check(c>=last&&c>=0&&c<=1&&Math.Abs(c+r-1)<.00001,"Smooth monotonic reversible coverage");last=c;}
  Check(ManifestMeshRules.PartCoverage(.2f,false)>0 && ManifestMeshRules.PartCoverage(.2f,true)==0,"Body manifests before helmet");
  Check(ManifestMeshRules.PartCoverage(1,true)==1 && ManifestMeshRules.PartCoverage(0,false)==0,"Part endpoints");
  Pawn pawn=Empty<Pawn>();pawn.inventory=new Pawn_InventoryTracker(pawn);pawn.apparel=new Pawn_ApparelTracker(pawn);pawn.equipment=new Pawn_EquipmentTracker(pawn);pawn.inventory.innerContainer=new TestOwner(pawn.inventory);
  var h=new HediffWithComps();h.pawn=pawn;var cpt=new HediffComp_ElementalTransform();cpt.parent=h;Set(cpt,"dormantKit",new TestOwner(cpt));
  var kit=(List<Thing>)Get(cpt,"kit");
  for(int i=0;i<3;i++){var t=new TestThing();t.def=Empty<ThingDef>();t.def.defName="TestKit"+i;t.def.stackLimit=1;t.def.category=ThingCategory.Item;t.thingIDNumber=900+i;t.stackCount=1;Check(pawn.inventory.innerContainer.TryAdd(t),"Inventory setup");kit.Add(t);}
  var personal=new TestThing();personal.def=Empty<ThingDef>();personal.def.defName="Personal";personal.def.stackLimit=1;personal.def.category=ThingCategory.Item;personal.thingIDNumber=950;personal.stackCount=1;pawn.inventory.innerContainer.TryAdd(personal);
  Set(cpt,"supplied",true);
  var park=typeof(HediffComp_ElementalTransform).GetMethod("ParkKit",F);park.Invoke(cpt,null);
  Check(cpt.GetDirectlyHeldThings().Count==3 && pawn.inventory.innerContainer.Count==1 && pawn.inventory.innerContainer.Contains(personal),"Kit parked without touching personal inventory");
  park.Invoke(cpt,null);Check(cpt.GetDirectlyHeldThings().Count==3,"Repeated parking duplicates items");
  foreach(var item in kit)Check(cpt.GetDirectlyHeldThings().Contains(item)&&!item.Destroyed,"Kit identity/durability preservation");
  Check((bool)typeof(HediffComp_ElementalTransform).GetProperty("KitPresent",F).GetValue(cpt,null),"Hidden kit recognized as available");
  Check(cpt.ParentHolder==pawn,"Container connected to pawn ownership tree");
  var children=new List<IThingHolder>();cpt.GetChildHolders(children);
  Check(!(bool)Get(cpt,"changingEquipment"),"Internal equipment-change guard leaked");
  pawn.health=Empty<Pawn_HealthTracker>();Set(pawn.health,"pawn",pawn);Set(pawn.health,"healthState",PawnHealthState.Mobile);pawn.mindState=Empty<Verse.AI.Pawn_MindState>();pawn.health.hediffSet=Empty<HediffSet>();pawn.health.hediffSet.hediffs=new List<Hediff>();
  var apprentice=new HediffComp_ElementalApprentice();apprentice.parent=h;
  Pawn victim=Empty<Pawn>();victim.health=Empty<Pawn_HealthTracker>();Set(victim.health,"healthState",PawnHealthState.Mobile);apprentice.Notify_KilledPawn(victim,null);Check(apprentice.discipleKills==1,"Vanilla kill notification occurs before victim SetDead; must count it");
  apprentice.Notify_KilledPawn(pawn,null);apprentice.Notify_KilledPawn(null,null);Check(apprentice.discipleKills==1,"Invalid/self kill counted");
  var ascDef=Empty<HediffDef>();ascDef.tags=new List<string>{ElementalHeartUtility.HeartTag};var ascH=new HediffWithComps();ascH.def=ascDef;pawn.health.hediffSet.hediffs.Add(ascH);
  apprentice.Notify_KilledPawn(victim,null);Check(apprentice.discipleKills==1,"Full ascendant keeps gaining disciple kills");pawn.health.hediffSet.hediffs.Clear();
  var eqOwner=new TestEquipmentOwner(pawn.equipment);Set(pawn.equipment,"equipment",eqOwner);
  var sword=new TestThing();sword.def=Empty<ThingDef>();sword.def.defName="BoundTestSword";sword.def.stackLimit=1;sword.def.equipmentType=EquipmentType.Primary;sword.stackCount=1;sword.thingIDNumber=990;
  eqOwner.TryAdd(sword,false);
  var store=new GameComponent_ElementalSwords(null);var bond=store.Bind(pawn,"Flame",sword);Set(bond,"hidden",new TestOwner(bond));
  bond.Park();Check(pawn.equipment.Primary==null&&bond.GetDirectlyHeldThings().Contains(sword),"Equipped sword did not vanish into saved owner storage");
  bond.Park();Check(bond.GetDirectlyHeldThings().Count==1,"Sword duplicated while repeatedly suspended");
  bond.Equip();Check(pawn.equipment.Primary==sword&&bond.GetDirectlyHeldThings().Count==0,"Original sword not restored");
  Set(pawn.health,"healthState",PawnHealthState.Down);bond.Park();bond.Equip();Check(pawn.equipment.Primary==null,"Sword restored while still downed");
  Set(pawn.health,"healthState",PawnHealthState.Mobile);bond.Equip();Check(pawn.equipment.Primary==sword,"Repeated knockdown/recovery cycle failed");
  Set(pawn.health,"healthState",PawnHealthState.Dead);bond.Park();bond.Equip();Check(pawn.equipment.Primary==null,"Sword restored while dead");
  Set(pawn.health,"healthState",PawnHealthState.Mobile);bond.Equip();Check(pawn.equipment.Primary==sword,"Revival did not restore original sword");
  Check(store.Bind(pawn,"Flame",sword)==bond,"Duplicate pawn binding");
  Check(!(bool)Get(bond,"changing"),"Sword internal-change guard leaked");
  Current.Game=Empty<Game>();Set(Current.Game,"components",new List<GameComponent>{store});
  bool dropResult=true;ThingWithComps dropped=sword;
  Check(!BoundSwordNoDrop.Prefix(pawn,sword,ref dropped,ref dropResult)&&!dropResult&&dropped==null,"Bound sword drop guard");
  bool transferResult=true;Check(!BoundSwordNoTransfer.Prefix(pawn,sword,ref transferResult)&&!transferResult,"Bound sword transfer guard");
  Check(!BoundSwordNoRemove.Prefix(pawn,sword)&&!BoundSwordNoReplace.Prefix(pawn,personal),"Direct remove/replace guard");
  var training=new HediffWithComps();training.def=Empty<HediffDef>();training.comps=new List<HediffComp>{apprentice};pawn.health.hediffSet.hediffs.Add(training);
  var cut=Empty<DamageDef>();cut.defName="Cut";var packet=new DamageInfo(cut,100f,0f,-1f,pawn);ElementalDamageBalance.Prefix(ref packet);Check(Math.Abs(packet.Amount-80f)<.001f,"Production melee damage reduction");
  var bullet=Empty<DamageDef>();bullet.defName="Bullet";var shot=new DamageInfo(bullet,100f,0f,-1f,pawn);ElementalDamageBalance.Prefix(ref shot);Check(shot.Amount==100f,"Ordinary ranged damage changed");
  pawn.health.hediffSet.hediffs.Clear();var ordinary=new DamageInfo(cut,100f,0f,-1f,pawn);ElementalDamageBalance.Prefix(ref ordinary);Check(ordinary.Amount==100f,"Non-elemental pawn damage changed");Current.Game=null;
  Console.WriteLine("PASS: threshold boundaries, pre-SetDead kill notification, self/null/full-ascendant guards, damage scope, sword suspension/restoration identity, repeated recovery cycles and duplicate-binding prevention.");
  Console.WriteLine("PASS: continuous reversible coverage; body/head sequencing; production inventory-to-heart transfer; personal inventory preserved; repeat-safe parking; kit identity preserved; hidden-kit availability and holder link.");
 }
}

class TestOwner:ThingOwner<Thing>{public TestOwner(IThingHolder owner):base(owner,false,LookMode.Deep){}protected override void NotifyAdded(Thing item){}protected override void NotifyRemoved(Thing item){}}

class TestThing:ThingWithComps {public override bool CanStackWith(Thing other){return false;}}

class TestEquipmentOwner:ThingOwner<ThingWithComps>{public TestEquipmentOwner(IThingHolder owner):base(owner,false,LookMode.Deep){}protected override void NotifyAdded(Thing item){}protected override void NotifyRemoved(Thing item){}}

public class AzureSaveCase:IExposable {public AzurePyre pyre;public HediffComp_FlameMastery mastery;public void ExposeData(){Scribe_Deep.Look(ref pyre,"pyre");if(mastery==null)mastery=new HediffComp_FlameMastery();mastery.CompExposeData();}}
