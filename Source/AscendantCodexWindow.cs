using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class CodexArt {
  public static readonly Color Gold=new Color(.96f,.81f,.48f),Muted=new Color(.66f,.71f,.79f);
  public static readonly Texture2D Emblem=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Bestow");
  static readonly Dictionary<string,Texture2D> symbols=new Dictionary<string,Texture2D>();
  static CodexArt(){foreach(var form in new[]{"Flame","AzureFlame","Lightning","Thunderstorm","Water","Glacial","Wind","Cyclone","Earth","Adamant","Lunar","Eclipse"})symbols[form]=ContentFinder<Texture2D>.Get("UI/ElementalHearts/Court/Badges/"+form);symbols["Celestial"]=Emblem;}
  public static Texture2D Icon(CodexEntry entry){if(ThunderIncarnate.Active(entry.pawn))return ThunderIncarnate.Icon(ThunderIncarnate.SkillCount);Texture2D result;return symbols.TryGetValue((entry.form??"").Replace(" ",""),out result)?result:Emblem;}
  public static Color Accent(CodexEntry entry){if(ThunderIncarnate.Active(entry.pawn))return new Color(1,.89f,.08f);int i=Array.IndexOf(CelestialBestowOptions.Elements,entry.element);return i<0?Gold:CelestialBestowArt.Accents[i];}
 }
 public sealed class MainTabWindow_AscendantCodex:MainTabWindow {
  readonly List<CodexEntry> rows=new List<CodexEntry>();readonly HashSet<int> selected=new HashSet<int>();
  Vector2 rosterScroll,detailScroll;string search="",stage="All stages",lineage="All elements",feedback="";bool currentMapOnly,readyOnly;int detailId=-1;float refreshAt;
  public int RosterCount{get{return rows.Count;}}public int SelectedCount{get{return selected.Count;}}
  public override Vector2 InitialSize{get{return new Vector2(Math.Min(1200,UI.screenWidth-24),Math.Min(790,UI.screenHeight-80));}}
  public override void PostOpen(){base.PostOpen();Refresh();selected.Clear();foreach(var p in Find.Selector.SelectedObjects.OfType<Pawn>())if(rows.Any(r=>r.pawn==p))selected.Add(p.thingIDNumber);if(selected.Count>0)detailId=selected.First();}
  public override void PostClose(){rows.Clear();selected.Clear();detailId=-1;rosterScroll=detailScroll=Vector2.zero;base.PostClose();}
  public void Refresh(){rows.Clear();rows.AddRange(AscendantCodex.Gather());var alive=new HashSet<int>(rows.Select(r=>r.pawn.thingIDNumber));selected.RemoveWhere(id=>!alive.Contains(id));if(!alive.Contains(detailId))detailId=rows.Count>0?rows[0].pawn.thingIDNumber:-1;refreshAt=Time.realtimeSinceStartup+.75f;}
  bool Matches(CodexEntry entry){
   if(currentMapOnly&&(Find.CurrentMap==null||entry.pawn.MapHeld!=Find.CurrentMap))return false;
   if(readyOnly&&(entry.progress==null||!entry.progress.Ready))return false;
   if(lineage!="All elements"&&entry.element!=lineage)return false;
   if(stage!="All stages"&&(stage=="Otherworlders"?!entry.origin:stage=="Disciples"?entry.tier!=CodexTier.Disciple:stage=="Ascendants"?entry.tier!=CodexTier.Ascendant:stage=="Evolved"?entry.tier!=CodexTier.Evolved:stage=="Transcendants"?entry.tier!=CodexTier.Transcendant:entry.tier!=CodexTier.Celestial))return false;
   return string.IsNullOrWhiteSpace(search)||(entry.name+" "+entry.form+" "+entry.location).IndexOf(search,StringComparison.CurrentCultureIgnoreCase)>=0;
  }
  List<Pawn> Targets(){return rows.Where(r=>selected.Contains(r.pawn.thingIDNumber)).Select(r=>r.pawn).ToList();}
  static void Panel(Rect rect,Color? fill=null){Widgets.DrawBoxSolid(rect,fill??new Color(.08f,.09f,.12f));Widgets.DrawBoxSolid(new Rect(rect.x,rect.y,rect.width,1),new Color(.29f,.31f,.36f));}
  static void Label(Rect rect,string text,GameFont font,Color color){Text.Font=font;Text.Anchor=TextAnchor.UpperLeft;GUI.color=color;Widgets.Label(rect,text);GUI.color=Color.white;}
  void FilterButton(Rect rect,string label,string[] values,Action<string> changed){if(Widgets.ButtonText(rect,label))Find.WindowStack.Add(new FloatMenu(values.Select(value=>new FloatMenuOption(value,()=>{changed(value);rosterScroll=Vector2.zero;})).ToList()));}
  void Result(CodexOrderReport report,string action){feedback=report.Describe(action);Refresh();}
  void SquadButton(Rect rect,string label,Action action,string tooltip){bool old=GUI.enabled;GUI.enabled=old&&selected.Count>0;if(Widgets.ButtonText(rect,label))action();GUI.enabled=old;TooltipHandler.TipRegion(rect,tooltip);}
  void Progress(Rect rect,CodexProgress p){
   if(p==null)return;
   Color color=p.Ready?new Color(.50f,.89f,.66f):CodexArt.Muted;
   Label(new Rect(rect.x,rect.y,rect.width,20),p.Summary,GameFont.Tiny,color);
   if(p.grantedReady){Label(new Rect(rect.x,rect.y+23,rect.width,22),"Evolve whenever desired",GameFont.Tiny,CodexArt.Gold);return;}
   if(!p.available)return;
   Widgets.FillableBar(new Rect(rect.x,rect.y+20,rect.width,3),Mathf.Clamp01(p.kills/(float)p.requiredKills));
   Label(new Rect(rect.x,rect.y+27,rect.width,20),p.Days,GameFont.Tiny,CodexArt.Muted);
   Widgets.FillableBar(new Rect(rect.x,rect.y+49,rect.width,3),Mathf.Clamp01(p.ageTicks/(float)p.requiredTicks));
   TooltipHandler.TipRegion(rect,p.Ready?p.action+" milestones met. Choose when to perform the ceremony.":p.KillsRemaining+" kills remaining; "+p.TicksRemaining.ToStringTicksToPeriod()+" training time remaining. Existing lineage counters are used.");
  }
  void Row(Rect rect,CodexEntry entry){
   bool active=selected.Contains(entry.pawn.thingIDNumber);if(active||detailId==entry.pawn.thingIDNumber)Widgets.DrawBoxSolid(rect,active?new Color(.17f,.17f,.12f):new Color(.12f,.14f,.18f));else if(Mouse.IsOver(rect))Widgets.DrawHighlight(rect);
   bool check=active;Widgets.Checkbox(new Vector2(rect.x+6,rect.y+25),ref check,22);if(check!=active){if(check)selected.Add(entry.pawn.thingIDNumber);else selected.Remove(entry.pawn.thingIDNumber);}
   Widgets.DrawTextureFitted(new Rect(rect.x+35,rect.y+13,37,37),CodexArt.Icon(entry),1);
   float progressWidth=rect.width>=650?190:145,progressX=rect.xMax-96-progressWidth;float nameWidth=Math.Max(78,progressX-rect.x-90);
   Label(new Rect(rect.x+82,rect.y+7,nameWidth,25),entry.name,GameFont.Small,Color.white);
   Label(new Rect(rect.x+82,rect.y+33,nameWidth,20),entry.form+" · "+entry.tier,GameFont.Tiny,CodexArt.Accent(entry));
   Label(new Rect(rect.x+82,rect.y+55,nameWidth,18),entry.location,GameFont.Tiny,CodexArt.Muted);
   if(Widgets.ButtonInvisible(new Rect(rect.x+32,rect.y,nameWidth+50,rect.height))){detailId=entry.pawn.thingIDNumber;detailScroll=Vector2.zero;}
   if(entry.progress!=null)Progress(new Rect(progressX,rect.y+10,progressWidth-8,60),entry.progress);else Label(new Rect(progressX,rect.y+22,progressWidth-8,42),entry.tier==CodexTier.Otherworlder?"Awaiting Bestow":entry.tier==CodexTier.Celestial?"Sovereign form":"Evolution complete",GameFont.Tiny,CodexArt.Muted);
   Label(new Rect(rect.xMax-88,rect.y+12,82,23),entry.mode.HasValue?ElementalAutoModes.Label(entry.mode.Value):"—",GameFont.Tiny,entry.mode==ElementalAutoMode.NonLethal?new Color(.60f,.88f,.76f):CodexArt.Gold);
   Label(new Rect(rect.xMax-88,rect.y+36,82,41),entry.status,GameFont.Tiny,CodexArt.Muted);
  }
  void Roster(Rect rect,List<CodexEntry> filtered){
   Panel(rect);Rect view=new Rect(rect.x+4,rect.y+5,rect.width-8,rect.height-10),content=new Rect(0,0,rect.width-26,Math.Max(view.height,filtered.Count*83));Widgets.BeginScrollView(view,ref rosterScroll,content);
   try{if(filtered.Count==0)Label(new Rect(16,22,content.width-32,70),rows.Count==0?"No owned Ascendants, disciples or recognized Otherworlders yet.":"No pawns match these filters.",GameFont.Small,CodexArt.Muted);int first=Math.Max(0,(int)(rosterScroll.y/83)-1),last=Math.Min(filtered.Count,(int)((rosterScroll.y+view.height)/83)+2);for(int i=first;i<last;i++)Row(new Rect(0,i*83,content.width,80),filtered[i]);}finally{Widgets.EndScrollView();}
  }
  void Detail(Rect rect,CodexEntry entry){
   Panel(rect);if(entry==null){Label(rect.ContractedBy(18),"Choose a row to inspect its progression and controls.",GameFont.Small,CodexArt.Muted);return;}
   var p=entry.pawn;int traitCount=p.story==null?0:p.story.traits.allTraits.Count(t=>t.def.defName.StartsWith("Isekai_"));Rect view=rect.ContractedBy(12);Rect content=new Rect(0,0,view.width-16,Math.Max(view.height,(entry.progress==null?420:550)+Math.Min(10,traitCount)*27));Widgets.BeginScrollView(view,ref detailScroll,content);
   try{
    GUI.DrawTexture(new Rect(0,0,64,80),PortraitsCache.Get(p,new Vector2(128,160),Rot4.South,Vector3.zero,1.2f,true,false));
    Label(new Rect(76,4,content.width-76,28),entry.name,GameFont.Medium,CodexArt.Gold);
    Label(new Rect(76,36,content.width-76,40),entry.form+" · "+entry.tier,GameFont.Small,CodexArt.Accent(entry));
    Label(new Rect(0,91,content.width,44),entry.location+"\n"+entry.status,GameFont.Small,CodexArt.Muted);
    if(Widgets.ButtonText(new Rect(0,144,content.width,31),"Locate pawn"))Locate(p);
    float y=192;
    if(entry.progress!=null){
     Label(new Rect(0,y,content.width,25),entry.progress.action+" → "+(entry.progress.nextForm??"Unknown"),GameFont.Small,CodexArt.Gold);y+=31;Progress(new Rect(0,y,content.width,60),entry.progress);y+=67;
     if(!entry.progress.Ready&&entry.progress.available)Label(new Rect(0,y,content.width,47),entry.progress.KillsRemaining+" kills remaining\n"+entry.progress.TicksRemaining.ToStringTicksToPeriod()+" training time remaining",GameFont.Small,CodexArt.Muted);else Label(new Rect(0,y,content.width,47),entry.progress.Ready?"Milestones fulfilled. Start the ceremony when ready.":"Existing mastery data is unavailable.",GameFont.Small,CodexArt.Muted);y+=53;
     string why=CodexProgress.Problem(p);bool old=GUI.enabled;GUI.enabled=old&&why==null;if(Widgets.ButtonText(new Rect(0,y,content.width,32),entry.progress.action+" now")){if(CodexProgress.Begin(p))feedback="Ceremony started for "+entry.name+".";Refresh();}GUI.enabled=old;if(why!=null)TooltipHandler.TipRegion(new Rect(0,y,content.width,32),why);y+=49;
    }else{Label(new Rect(0,y,content.width,47),entry.tier==CodexTier.Otherworlder?"Receive a normal elemental lineage through a Celestial's Bestow.":"This form has completed its evolution path.",GameFont.Small,CodexArt.Muted);y+=64;}
    Label(new Rect(0,y,content.width,23),"ORIGIN & TRAITS",GameFont.Tiny,CodexArt.Gold);y+=26;
    Label(new Rect(0,y,content.width,53),entry.origin?(entry.legacyOrigin?"Legacy Otherworlder: recognized from academy clothing and an ISEKAI identity.":"Otherworlder: origin recorded when called."):"Colony pawn. No recorded otherworld origin.",GameFont.Small,CodexArt.Muted);y+=61;
    if(p.story!=null){var traits=p.story.traits.allTraits.Where(t=>t.def.defName.StartsWith("Isekai_")).Take(10).ToList();foreach(var trait in traits){Label(new Rect(0,y,content.width,25),"• "+trait.LabelCap,GameFont.Small,Color.white);y+=27;}}
    if(CelestialAscendant.Active(p)){OtherworldCalling.UpgradeCooldown(p);int left=HediffComp_CelestialAbilities.On(p).callingReadyAt-Find.TickManager.TicksGame;Label(new Rect(0,y+5,content.width,43),"Otherworld Calling: "+(left<=0?"Ready":left.ToStringTicksToPeriod()),GameFont.Small,CodexArt.Gold);}
   }finally{Widgets.EndScrollView();}
  }
  void Locate(Pawn p){if(p.MapHeld!=null){Current.Game.CurrentMap=p.MapHeld;Find.MainTabsRoot.EscapeCurrentTab(false);CameraJumper.TryJumpAndSelect(p);return;}var caravan=p.GetCaravan();if(caravan!=null){Find.MainTabsRoot.EscapeCurrentTab(false);CameraJumper.TryJumpAndSelect(caravan);return;}feedback="This pawn is off map. Use its travel or arrival recovery controls.";}
  public override void DoWindowContents(Rect rect){
   var oldFont=Text.Font;var oldAnchor=Text.Anchor;var oldColor=GUI.color;
   try{
    if(Time.realtimeSinceStartup>=refreshAt)Refresh();var filtered=rows.Where(Matches).ToList();
    Widgets.DrawTextureFitted(new Rect(0,0,51,51),CodexArt.Emblem,1);Label(new Rect(65,0,rect.width-180,32),"Ascendant Codex",GameFont.Medium,CodexArt.Gold);Label(new Rect(66,35,rect.width-180,24),rows.Count+" in roster  ·  "+selected.Count+" selected  ·  "+rows.Count(r=>r.progress!=null&&r.progress.Ready)+" ready to advance",GameFont.Small,CodexArt.Muted);
    if(Widgets.ButtonText(new Rect(rect.width-93,7,93,31),"Refresh"))Refresh();
    float filterY=70;Text.Font=GameFont.Small;var searchRect=new Rect(0,filterY,Math.Min(225,rect.width*.23f),31);GUI.SetNextControlName("EH_CodexSearch");search=Widgets.TextField(searchRect,search);
    if(string.IsNullOrEmpty(search)&&GUI.GetNameOfFocusedControl()!="EH_CodexSearch")Label(new Rect(searchRect.x+8,searchRect.y+6,searchRect.width-16,23),"Search name, form or location",GameFont.Tiny,CodexArt.Muted);
    TooltipHandler.TipRegion(searchRect,"Search pawn names, elemental forms or map/caravan locations.");float x=searchRect.width+9;Text.Font=GameFont.Small;FilterButton(new Rect(x,filterY,139,31),stage,new[]{"All stages","Disciples","Ascendants","Evolved","Transcendants","Celestial","Otherworlders"},value=>stage=value);x+=148;FilterButton(new Rect(x,filterY,133,31),lineage,new[]{"All elements","Flame","Lightning","Water","Wind","Earth","Lunar","Celestial"},value=>lineage=value);x+=143;
    bool wrapFilters=x+258>rect.width;float checkY=wrapFilters?filterY+37:filterY+3,checkX=wrapFilters?0:x;Widgets.CheckboxLabeled(new Rect(checkX,checkY,128,27),"Current map",ref currentMapOnly);Widgets.CheckboxLabeled(new Rect(checkX+138,checkY,120,27),"Ready only",ref readyOnly);
    float extra=wrapFilters?34:0;Label(new Rect(0,108+extra,rect.width,23),ElementalHeartsMod.Settings.enableAutomation?"Select squad members using the checkboxes. Click a name to inspect.":"Player Auto Combat is disabled in mod settings. Pawn toggle changes are still saved.",GameFont.Tiny,ElementalHeartsMod.Settings.enableAutomation?CodexArt.Muted:CodexArt.Gold);
    float detailWidth=Math.Min(342,rect.width*.34f),bodyHeight=Math.Max(100,rect.height-255-extra),listWidth=rect.width-detailWidth-14;
    Roster(new Rect(0,136+extra,listWidth,bodyHeight),filtered);Detail(new Rect(listWidth+14,136+extra,detailWidth,bodyHeight),rows.FirstOrDefault(r=>r.pawn.thingIDNumber==detailId));
    float bar=rect.height-105;float selectWidth=123;
    if(Widgets.ButtonText(new Rect(0,bar,selectWidth,31),"Select filtered"))foreach(var entry in filtered)selected.Add(entry.pawn.thingIDNumber);
    if(Widgets.ButtonText(new Rect(selectWidth+7,bar,72,31),"Clear"))selected.Clear();
    float bx=selectWidth+89;float buttonWidth=(rect.width-bx-28)/5;
    SquadButton(new Rect(bx,bar,buttonWidth,31),"Auto: On",()=>Result(CodexOrders.Modes(Targets(),ElementalAutoMode.On),"Auto Combat On"),"Set the saved Auto Combat toggle for selected elemental pawns. Global mod settings still apply.");bx+=buttonWidth+7;
    SquadButton(new Rect(bx,bar,buttonWidth,31),"Auto: Off",()=>Result(CodexOrders.Modes(Targets(),ElementalAutoMode.Off),"Auto Combat Off"),"Disable elemental automation. Existing ordinary melee orders continue normally.");bx+=buttonWidth+7;
    SquadButton(new Rect(bx,bar,buttonWidth,31),"Non-lethal",()=>Result(CodexOrders.Modes(Targets(),ElementalAutoMode.NonLethal),"Non-lethal"),"Enable the existing non-lethal combat mode. Busy techniques and ceremonies are skipped.");bx+=buttonWidth+7;
    SquadButton(new Rect(bx,bar,buttonWidth,31),"Transform",()=>Result(CodexOrders.Transform(Targets(),true),"Transform"),"Use each eligible pawn's normal transformation animation. Busy, downed, off-map and queued pawns are skipped.");bx+=buttonWidth+7;
    SquadButton(new Rect(bx,bar,buttonWidth,31),"Revert",()=>Result(CodexOrders.Transform(Targets(),false),"Revert"),"Restore personal clothes through the existing Revert animation. Repeating the order will not toggle a normal pawn back into armor.");
    Label(new Rect(0,bar+42,rect.width,54),string.IsNullOrEmpty(feedback)?"Squad actions report applied and skipped pawns here. Transform/Revert never force drafting or interrupt an elemental ceremony.":feedback,GameFont.Small,CodexArt.Muted);
   }finally{Text.Font=oldFont;Text.Anchor=oldAnchor;GUI.color=oldColor;}
  }
 }
}
