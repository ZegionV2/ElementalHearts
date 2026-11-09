using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public sealed class WorldComponent_SeveredCourt:WorldComponent {
  bool introduced,setupComplete;int strongholdMask,strongholdVersion,redemptionVersion,placedCount,settlementGoal;
  List<CourtBaseRecord> nativeBases=new List<CourtBaseRecord>();
  public bool StrongholdsSeeded{get{return strongholdVersion>=1;}}public bool SetupComplete{get{return setupComplete;}}
  public int PlacedCount{get{int n=strongholdMask,bits=0;while(n>0){bits+=n&1;n>>=1;}var faction=Existing();return Math.Max(Math.Max(placedCount,bits),faction==null?0:CourtSettlements.All(faction).Count);}}
  public WorldComponent_SeveredCourt(World world):base(world){}
  public CourtBaseRecord Record(Settlement s,bool create){var r=nativeBases.Find(b=>b!=null&&b.settlement==s);if(r==null&&create){r=new CourtBaseRecord(s);nativeBases.Add(r);}return r;}
  public CourtSite NativeSite(Settlement s){return new CourtSite(s,Record(s,true));}
  public override void ExposeData(){Scribe_Values.Look(ref introduced,"EH_courtIntroduced");Scribe_Values.Look(ref strongholdMask,"EH_courtStrongholdMask");Scribe_Values.Look(ref strongholdVersion,"EH_courtStrongholdVersion");Scribe_Values.Look(ref redemptionVersion,"EH_courtRedemptionVersion");Scribe_Values.Look(ref setupComplete,"EH_courtSettlementSetupComplete");Scribe_Values.Look(ref placedCount,"EH_courtSettlementsPlaced");Scribe_Values.Look(ref settlementGoal,"EH_courtSettlementGoal");Scribe_Collections.Look(ref nativeBases,"EH_courtNativeBaseRecords",LookMode.Deep);if(nativeBases==null)nativeBases=new List<CourtBaseRecord>();}
  public override void FinalizeInit(bool fromLoad){
   base.FinalizeInit(fromLoad);
   LongEventHandler.ExecuteWhenFinished(()=>{
    if(Find.World==null||Find.World.GetComponent<WorldComponent_SeveredCourt>()!=this)return;
    if(redemptionVersion<1){CourtRedemption.ExistingColonists();redemptionVersion=1;}
    nativeBases.RemoveAll(r=>r==null||r.settlement==null||r.settlement.Destroyed);
    var faction=Existing();if(faction!=null)foreach(var s in CourtSettlements.All(faction)){var hold=s as CourtStronghold;if(hold!=null)CourtStrongholdArchitecture.UpgradeExisting(hold);else{var record=Record(s,true);if(fromLoad)record.AdoptExistingMap();}}
    if(!fromLoad){introduced=true;setupComplete=true;if(faction!=null&&!faction.defeated)ConfigureSettlements(Math.Max(5,PlacedCount),false);}
    else if(faction!=null&&faction.defeated){setupComplete=true;introduced=true;}
    else if(!setupComplete&&ElementalHeartsMod.Settings!=null&&(ElementalHeartsMod.Settings.addCourtToExistingSaves||faction!=null&&ElementalHeartsMod.Settings.addCourtStrongholdsToExistingSaves))OpenSetup();
   });
  }
  Faction Existing(){return Find.FactionManager.AllFactionsListForReading.FirstOrDefault(SeveredCourt.IsFaction);}
  public void OpenSetup(){if(Existing()!=null&&Existing().defeated){Messages.Message("The defeated Court will not be recreated.",MessageTypeDefOf.NeutralEvent,false);return;}if(!Find.WindowStack.Windows.Any(w=>w is Dialog_CourtSettlementSetup))Find.WindowStack.Add(new Dialog_CourtSettlementSetup(this));}
  public void DeferSetup(){setupComplete=true;introduced=true;placedCount=PlacedCount;if(Existing()!=null)strongholdVersion=1;}
  public void EnsureIntroduced(){if(introduced)return;if(Existing()==null)CreateCourt();introduced=true;}
  Faction CreateCourt(){
   var def=DefDatabase<FactionDef>.GetNamedSilentFail(SeveredCourt.DefName);if(def==null)return null;var faction=FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(def));Find.FactionManager.Add(faction);
   // The native factory's first settlement is kept; it now uses the same fort map generator.
   foreach(var s in CourtSettlements.All(faction))s.Name="The Sundered Seat";
   return faction;
  }
  public int EnsureStrongholds(bool manual){return ConfigureSettlements(settlementGoal>0?settlementGoal:Math.Max(5,PlacedCount),manual);}
  public int ConfigureSettlements(int requested,bool manual){
   requested=Math.Max(0,Math.Min(50,requested));var faction=Existing();if(faction!=null&&faction.defeated)return 0;
   int before=faction==null?0:CourtSettlements.All(faction).Count;placedCount=PlacedCount;settlementGoal=Math.Max(placedCount,requested);setupComplete=true;introduced=true;
   if(faction==null){if(!manual||settlementGoal==0)return 0;faction=CreateCourt();if(faction==null)return 0;placedCount=Math.Max(placedCount,CourtSettlements.All(faction).Count);}
   var def=DefDatabase<WorldObjectDef>.GetNamed(CourtStrongholds.DefName);var elements=new int[5];foreach(var s in CourtSettlements.All(faction))elements[CourtSettlements.Site(s).Element]++;
   for(int i=placedCount;i<settlementGoal;i++){
    var tile=TileFinder.RandomSettlementTileFor(faction,true,t=>!Find.WorldObjects.AnyWorldObjectAt(t));if(!tile.Valid)break;int element=Array.IndexOf(elements,elements.Min());elements[element]++;
    var hold=(CourtStronghold)WorldObjectMaker.MakeWorldObject(def);hold.element=element;hold.SetFaction(faction);hold.Tile=tile;hold.Name=CourtStrongholds.Names[element]+(i<5?"":" — "+(i/5+1));Find.WorldObjects.Add(hold);strongholdMask|=1<<element;placedCount++;
   }
   foreach(var s in CourtSettlements.All(faction)){var hold=s as CourtStronghold;if(hold==null)Record(s,true);else strongholdMask|=1<<hold.Element;}
   strongholdVersion=1;int added=CourtSettlements.All(faction).Count-before;
   if(added>0)Find.LetterStack.ReceiveLetter("Court fortresses established",added+" Court settlement"+(added==1?" has":"s have")+" been established. Every Court base uses the detailed fortress map and an initial garrison of one Herald, two Knights and six Oathlings.\n\nYour chosen total includes existing and previously cleared bases. Reloading does not add replacements. Visited maps keep their existing buildings until regenerated. Surviving defenders are retained and troops are never replenished.",LetterDefOf.NeutralEvent,CourtSettlements.All(faction).FirstOrDefault());
   return added;
  }
 }
 public sealed class Dialog_CourtSettlementSetup:Window {
  readonly WorldComponent_SeveredCourt state;readonly int minimum;int count;
  public override Vector2 InitialSize{get{return new Vector2(640,440);}}
  public Dialog_CourtSettlementSetup(WorldComponent_SeveredCourt state){this.state=state;minimum=state.PlacedCount;count=Math.Max(5,minimum);forcePause=true;absorbInputAroundWindow=true;doCloseX=true;closeOnAccept=false;}
  public override void DoWindowContents(Rect rect){
   Text.Font=GameFont.Medium;Widgets.Label(new Rect(0,0,rect.width,35),"The Severed Court — world setup");Text.Font=GameFont.Small;
   Widgets.Label(new Rect(0,48,rect.width,110),"Choose the total number of Court bases in this world. Every new or regenerated base has a detailed fortress, nine defenders and an evolved Herald. Existing settlements keep their location and identity. Cleared bases remain counted and are never rebuilt.");
   Widgets.Label(new Rect(0,163,rect.width,28),"Court bases: "+count+"   (already placed: "+minimum+")");count=Mathf.RoundToInt(Widgets.HorizontalSlider(new Rect(0,200,rect.width,28),count,minimum,Math.Max(20,minimum),true));
   Widgets.Label(new Rect(0,244,rect.width,68),"Five is recommended for one seat per element. Larger counts repeat elemental commanders. Already visited maps keep their current layout until regenerated. This choice is saved with the world.");
   if(Widgets.ButtonText(new Rect(0,rect.height-44,rect.width*.62f-8,40),count==0?"Keep the Court out":"Apply settlement count")){int selected=count;Close();LongEventHandler.QueueLongEvent(()=>state.ConfigureSettlements(selected,true),"GeneratingWorld",false,null);}
   if(Widgets.ButtonText(new Rect(rect.width*.62f+4,rect.height-44,rect.width*.38f-4,40),"Keep world as it is")){state.DeferSetup();Close();}
  }
 }
}
