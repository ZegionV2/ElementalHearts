using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class CourtNameBadges {
  public static readonly string[] Names={"Flame","Lightning","Water","Wind","Earth","Lunar","Azure Flame","Thunderstorm","Glacial","Cyclone","Adamant","Eclipse"};
  static readonly string[] files={"Flame","Lightning","Water","Wind","Earth","Lunar","AzureFlame","Thunderstorm","Glacial","Cyclone","Adamant","Eclipse"};
  static readonly Texture2D[] textures=new Texture2D[12];
  public static Texture2D Icon(int index){return textures[index]??(textures[index]=ContentFinder<Texture2D>.Get("UI/ElementalHearts/Court/Badges/"+files[index]));}
  public static int Index(string def){switch(def){case "EH_FlameHeart":case "EH_FlameTraining":return 0;case "EH_LightningHeart":case "EH_LightningTraining":return 1;case "EH_WaterHeart":case "EH_WaterTraining":return 2;case "EH_WindHeart":case "EH_WindTraining":return 3;case "EH_EarthHeart":case "EH_EarthTraining":return 4;case "EH_LunarHeart":return 5;case "EH_AzureFlameAscendant":return 6;case "EH_ThunderstormAscendant":return 7;case "EH_GlacialAscendant":return 8;case "EH_CycloneAscendant":return 9;case "EH_AdamantAscendant":return 10;case "EH_EclipseAscendant":return 11;default:return -1;}}
  public static int Index(Pawn p){int training=-1;if(p==null||p.health==null)return -1;foreach(var h in p.health.hediffSet.hediffs){int i=Index(h.def.defName);if(i<0)continue;if(!h.def.defName.EndsWith("Training"))return i;training=i;}return training;}
  public static bool Visible(Pawn p,float alpha){return ElementalHeartsMod.Settings!=null&&ElementalHeartsMod.Settings.showCourtBadges&&alpha>.01f&&p!=null&&p.Spawned&&p.Map==Find.CurrentMap&&p.RaceProps.Humanlike&&CourtAppearance.Bound(p)&&p.Position.InBounds(p.Map)&&!p.Position.Fogged(p.Map);}
  public static Rect Position(Rect nameRect,GameFont font){float size=font==GameFont.Tiny&&!Prefs.DisableTinyText?16:18;return new Rect(nameRect.x-size-2,nameRect.center.y-size*.5f,size,size);}
 }
 // Draw beside the native label only when that label is drawn. Names and their save data stay native.
 [HarmonyPatch(typeof(GenMapUI),"DrawPawnLabel",new Type[]{typeof(Pawn),typeof(Rect),typeof(float),typeof(float),typeof(Dictionary<string,string>),typeof(GameFont),typeof(bool),typeof(bool)})]
 public static class CourtPawnLabel {
  public static void Postfix(Pawn pawn,Rect bgRect,float alpha,GameFont font){if(!CourtNameBadges.Visible(pawn,alpha))return;int index=CourtNameBadges.Index(pawn);if(index<0)return;var rect=CourtNameBadges.Position(bgRect,font);var previous=GUI.color;try{GUI.color=new Color(1,1,1,Mathf.Clamp01(alpha));GUI.DrawTexture(rect,CourtNameBadges.Icon(index),ScaleMode.ScaleToFit);int herald=CourtHeralds.Index(pawn);TooltipHandler.TipRegion(rect,herald>=0&&CourtAppearance.Bound(pawn)?CourtHeralds.Titles[herald]:CourtNameBadges.Names[index]);}finally{GUI.color=previous;}}
 }
}
