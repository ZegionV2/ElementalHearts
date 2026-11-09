using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public enum CourtRank {Oathling,Knight,Herald}
 public static class CourtHeralds {
  public const int Cost=6000,MinimumRaidPoints=8000,EarliestDay=60;
  public const float RaidChance=.18f;
  public const float MaximumRaidPoints=10000f;
  public static bool Guaranteed(float points){var s=ElementalHeartsMod.Settings;return s!=null&&s.courtRaids&&s.courtHeralds&&!float.IsNaN(points)&&!float.IsInfinity(points)&&points>=MaximumRaidPoints;}
  public static readonly string[] Equipment={"AzureFlame","Thunderstorm","Glacial","Cyclone","Adamant"};
  public static readonly string[] Conditions={"EH_AzureFlameAscendant","EH_ThunderstormAscendant","EH_GlacialAscendant","EH_CycloneAscendant","EH_AdamantAscendant"};
  public static readonly string[] Titles={"Azure Herald","Storm Herald","Frost Herald","Gale Herald","Adamant Herald"};
  public static readonly string[] Lineages={"Azure Flame","Thunderstorm","Glacial","Cyclone","Adamant"};
  static Texture2D seal;
  public static Texture2D Seal{get{return seal??(seal=ContentFinder<Texture2D>.Get("UI/ElementalHearts/Court/HeraldSeal"));}}
  public static int ElementIndex(string element){return Array.IndexOf(SeveredCourt.Elements,element);}
  public static string Condition(string element){int i=ElementIndex(element);return i<0?null:Conditions[i];}
  public static int Index(Pawn p){if(p==null||p.health==null)return -1;foreach(var h in p.health.hediffSet.hediffs){int i=Array.IndexOf(Conditions,h.def.defName);if(i>=0)return i;}return -1;}
  public static bool Active(Pawn p){return CourtAppearance.Bound(p)&&Index(p)>=0;}
  public static bool Eligible(float points,int tick){var s=ElementalHeartsMod.Settings;return s!=null&&s.courtRaids&&s.courtHeralds&&!float.IsNaN(points)&&!float.IsInfinity(points)&&points>=MinimumRaidPoints&&tick>=EarliestDay*60000;}
  public static bool HaloDrafted(Pawn p){if(p==null)return false;if(p.Drafted)return true;if(!Active(p)||ElementalCombatPolicy.Controlled(p)||p.IsPrisoner||p.IsSlave||p.Dead||p.Downed||p.InMentalState)return false;var enemy=p.mindState==null?null:p.mindState.enemyTarget;var target=p.CurJob==null?null:p.CurJob.targetA.Thing;return enemy!=null&&enemy.Spawned&&enemy.HostileTo(p)||target!=null&&target.Spawned&&target.HostileTo(p);}
  public static List<CourtRank> Plan(float points,bool herald){
   var result=new List<CourtRank>(CourtRaidRules.MaxPawns);if(float.IsNaN(points)||float.IsInfinity(points)||points<CourtRaidRules.DiscipleCost)return result;
   float remaining=Math.Min(points,1000000);if(herald&&remaining>=Cost+2*CourtRaidRules.DiscipleCost){result.Add(CourtRank.Herald);remaining-=Cost;}
   foreach(bool knight in CourtRaidRules.Plan(remaining)){if(result.Count>=CourtRaidRules.MaxPawns)break;result.Add(knight?CourtRank.Knight:CourtRank.Oathling);}return result;
  }
 }
 // A saved writ's element/seed determines its one commander. Scope never survives an impact or save.
 public sealed class CourtRaidScope:IDisposable {
  [ThreadStatic]public static CourtRaidScope Current;
  readonly CourtRaidScope previous;
  public readonly bool herald;public readonly int element;
  public CourtRaidScope(bool herald,int element){this.herald=herald;this.element=Mathf.Clamp(element,0,4);previous=Current;Current=this;}
  public void Dispose(){Current=previous;}
 }
 [HarmonyPatch(typeof(IncidentWorker_RaidEnemy),"GetLetterText")]
 public static class CourtHeraldRaidLetter {
  public static void Postfix(IncidentParms parms,List<Pawn> pawns,ref string __result){if(!SeveredCourt.IsFaction(parms.faction)||pawns==null)return;foreach(var p in pawns){int i=CourtHeralds.Index(p);if(i<0||!CourtAppearance.Bound(p))continue;__result="An evolved "+CourtHeralds.Titles[i]+" leads this Court assault. "+p.LabelShort+" wears the commander's ivory-and-obsidian regalia and faceguard, and commands five evolved techniques.\n\n"+__result;break;}}
 }
}
