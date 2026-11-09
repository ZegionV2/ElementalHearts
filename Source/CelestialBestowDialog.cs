using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class CelestialBestowArt {
  public static readonly Color Gold=new Color(.96f,.81f,.48f);
  public static readonly Color[] Accents={new Color(1,.42f,.23f),new Color(1,.84f,.28f),new Color(.38f,.74f,1),new Color(.37f,.87f,.67f),new Color(.82f,.65f,.39f),new Color(.8f,.74f,1)};
  public static readonly Texture2D Emblem=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Bestow");
  public static readonly Texture2D[] Elements=new Texture2D[6],Evolutions=new Texture2D[6];
  public static readonly string[] Notes={
   "Flame sword forms, rushing strikes and sweeping fire. Evolve into the royal blue flames of Azure Flame.",
   "Lightning steps, electrical shocks and rapid sword strikes. Evolve into Thunderstorm's red-lightning blitzes.",
   "Flowing sword forms, Pressure and defensive parries. Evolve into Glacial's frost and crowd control.",
   "Swift footwork, ranged wind cuts and flowing combos. Evolve into Cyclone's emerald airbound techniques.",
   "Ground stabs, stone surges and seismic circles. Evolve into Adamant's stronger defense and earth forms.",
   "The rare moonlit lineage, with five swift sword techniques. Evolve into Eclipse's six black/crimson forms."};
  static CelestialBestowArt(){
   string[] paths={"UI/Commands/ElementalHearts/Azure/AzureEvolution","UI/Commands/ElementalHearts/Thunderstorm/ThunderstormEvolution","UI/Commands/ElementalHearts/Glacial/GlacialEvolution","UI/Commands/ElementalHearts/Cyclone/CycloneEvolution","UI/Commands/ElementalHearts/Adamant/EvolveAdamant","UI/Abilities/ElementalHearts/Eclipse/EvolveEclipse"};
   for(int i=0;i<6;i++){Elements[i]=ContentFinder<Texture2D>.Get("UI/ElementalHearts/Court/Badges/"+CelestialBestowOptions.Elements[i]);Evolutions[i]=ContentFinder<Texture2D>.Get(paths[i]);}
  }
 }
 public sealed class Dialog_CelestialBestow:Window {
  readonly Pawn giver;int selected;bool ready;Vector2 scroll;
  public int SelectedIndex{get{return selected;}}public bool EvolutionReady{get{return ready;}}
  public override Vector2 InitialSize{get{return new Vector2(Math.Min(780,UI.screenWidth-40),Math.Min(710,UI.screenHeight-60));}}
  public Dialog_CelestialBestow(Pawn giver){this.giver=giver;var c=HediffComp_CelestialAbilities.On(giver);selected=c==null?0:Math.Max(0,Array.IndexOf(CelestialBestowOptions.Elements,c.bestowElement));ready=c!=null&&c.bestowEvolutionReady;forcePause=true;absorbInputAroundWindow=true;doCloseX=true;closeOnAccept=false;}
  static void Box(Rect rect,Color fill,Color outline){Widgets.DrawBoxSolid(rect,fill);Widgets.DrawBoxSolid(new Rect(rect.x,rect.y,rect.width,1),outline);Widgets.DrawBoxSolid(new Rect(rect.x,rect.yMax-1,rect.width,1),outline);Widgets.DrawBoxSolid(new Rect(rect.x,rect.y,1,rect.height),outline);Widgets.DrawBoxSolid(new Rect(rect.xMax-1,rect.y,1,rect.height),outline);}
  static void Label(Rect rect,string text,GameFont font,Color color,TextAnchor anchor=TextAnchor.UpperLeft){Text.Font=font;Text.Anchor=anchor;GUI.color=color;Widgets.Label(rect,text);GUI.color=Color.white;Text.Anchor=TextAnchor.UpperLeft;}
  void Card(Rect rect,int index){
   bool active=selected==index;Color accent=CelestialBestowArt.Accents[index];bool hover=Mouse.IsOver(rect);
   Box(rect,active?new Color(accent.r*.16f+.055f,accent.g*.16f+.055f,accent.b*.16f+.07f):hover?new Color(.14f,.16f,.2f):new Color(.095f,.105f,.14f),active?accent:new Color(.27f,.29f,.34f));
   Widgets.DrawBoxSolid(new Rect(rect.x,rect.y,3,rect.height),active?accent:accent*.6f);
   Widgets.DrawTextureFitted(new Rect(rect.x+10,rect.y+15,46,46),CelestialBestowArt.Elements[index],1);
   Label(new Rect(rect.x+66,rect.y+12,rect.width-76,25),CelestialBestowOptions.Elements[index],GameFont.Small,active?Color.white:new Color(.85f,.87f,.91f));
   Label(new Rect(rect.x+66,rect.y+38,rect.width-76,40),"→ "+CelestialBestowOptions.Evolutions[index],GameFont.Tiny,accent);
   Label(new Rect(rect.x+12,rect.yMax-24,rect.width-24,18),active?"SELECTED":index==5?"RARE MOON LINEAGE":"ELEMENTAL LINEAGE",GameFont.Tiny,active?accent:new Color(.54f,.58f,.65f));
   TooltipHandler.TipRegion(rect,CelestialBestowArt.Notes[index]);if(Widgets.ButtonInvisible(rect))selected=index;
  }
  void EvolutionChoice(Rect rect,bool value,string title,string detail){
   bool active=ready==value;bool hover=Mouse.IsOver(rect);Box(rect,active?new Color(.19f,.16f,.10f):hover?new Color(.14f,.15f,.18f):new Color(.09f,.10f,.13f),active?CelestialBestowArt.Gold:new Color(.27f,.29f,.34f));
   Label(new Rect(rect.x+12,rect.y+8,rect.width-24,23),(active?"● ":"○ ")+title,GameFont.Small,active?CelestialBestowArt.Gold:Color.white);
   Label(new Rect(rect.x+12,rect.y+34,rect.width-24,35),detail,GameFont.Tiny,new Color(.73f,.76f,.82f));if(Widgets.ButtonInvisible(rect))ready=value;
  }
  void Choose(){if(!CelestialBestowOptions.CanOpen(giver))return;string element=CelestialBestowOptions.Elements[selected];Close();CelestialBestowOptions.Target(giver,element,ready);}
  public override void DoWindowContents(Rect rect){
   var oldFont=Text.Font;var oldAnchor=Text.Anchor;var oldColor=GUI.color;
   try{
    var navigation=Event.current;if(navigation.type==EventType.KeyDown&&(navigation.keyCode==KeyCode.RightArrow||navigation.keyCode==KeyCode.LeftArrow)){selected=(selected+(navigation.keyCode==KeyCode.RightArrow?1:5))%6;navigation.Use();}
    Box(new Rect(0,0,rect.width,84),new Color(.09f,.10f,.14f),new Color(.44f,.36f,.21f));
    Widgets.DrawTextureFitted(new Rect(12,9,66,66),CelestialBestowArt.Emblem,1);
    Label(new Rect(92,11,rect.width-110,29),"Celestial Bestow",GameFont.Medium,CelestialBestowArt.Gold);
    Label(new Rect(93,45,rect.width-115,30),"Choose a lineage. Shape their ascension.",GameFont.Small,new Color(.75f,.78f,.85f));
    Rect view=new Rect(0,96,rect.width,Math.Max(80,rect.height-158));int columns=rect.width<620?2:3;int rows=(6+columns-1)/columns;float width=rect.width-18;float gridHeight=rows*108;
    Rect content=new Rect(0,0,width,gridHeight+258);Widgets.BeginScrollView(view,ref scroll,content);
    try{
     float cardWidth=(width-(columns-1)*10)/columns;for(int i=0;i<6;i++)Card(new Rect(i%columns*(cardWidth+10),i/columns*108,cardWidth,98),i);
     float y=gridHeight+6;Color accent=CelestialBestowArt.Accents[selected];
     Box(new Rect(0,y,width,91),new Color(.075f,.085f,.115f),new Color(.24f,.26f,.31f));
     Widgets.DrawTextureFitted(new Rect(width-76,y+13,64,64),CelestialBestowArt.Evolutions[selected],1);
     Label(new Rect(14,y+8,width-98,24),CelestialBestowOptions.Elements[selected]+" Ascendant  →  "+CelestialBestowOptions.Evolutions[selected],GameFont.Small,accent);
     Label(new Rect(14,y+35,width-100,49),CelestialBestowArt.Notes[selected],GameFont.Tiny,new Color(.78f,.80f,.85f));
     y+=105;Label(new Rect(0,y,width,24),"EVOLUTION PATH",GameFont.Tiny,CelestialBestowArt.Gold);y+=24;
     float half=(width-10)/2;EvolutionChoice(new Rect(0,y,half,73),false,"Earn through mastery","Earn the normal evolution requirements.");EvolutionChoice(new Rect(half+10,y,half,73),true,"Ready to evolve","Unlock Evolve; choose when to use it.");
     y+=83;Label(new Rect(2,y,width-4,36),"Bestow grants the normal form first. The recipient keeps their faction. No resource cost.",GameFont.Tiny,new Color(.65f,.69f,.76f));
    }finally{Widgets.EndScrollView();}
    Rect button=new Rect(0,rect.height-49,rect.width,43);bool canChoose=CelestialBestowOptions.CanOpen(giver);Color border=canChoose?CelestialBestowArt.Gold:new Color(.34f,.35f,.39f);bool over=Mouse.IsOver(button);
    Box(button,canChoose?(over?new Color(.34f,.27f,.14f):new Color(.23f,.19f,.11f)):new Color(.12f,.13f,.15f),border);
    Label(button,"Choose recipient  ·  "+CelestialBestowOptions.Elements[selected]+(ready?"  ·  Evolution ready":"  ·  Earn evolution"),GameFont.Small,canChoose?new Color(1,.9f,.65f):Color.gray,TextAnchor.MiddleCenter);
    if(canChoose&&Widgets.ButtonInvisible(button))Choose();if(!canChoose)TooltipHandler.TipRegion(button,"The Celestial must be conscious and finish their current action.");
    var ev=Event.current;if(ev.type==EventType.KeyDown&&(ev.keyCode==KeyCode.Return||ev.keyCode==KeyCode.KeypadEnter)){if(canChoose)Choose();ev.Use();}
   }finally{Text.Font=oldFont;Text.Anchor=oldAnchor;GUI.color=oldColor;}
  }
 }
}
