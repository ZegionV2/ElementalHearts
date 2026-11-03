using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;
namespace ElementalHearts {
 public enum ElementalSound {Slash,Dash,Spin,Charge,Impact,Parry,Burst}
 // Real-time budget: fast game speeds and crowds cannot multiply voices unchecked.
 public sealed class ElementalAudioBudget {
  float started=-1f;int used;
  public bool Accept(float now,bool heavy){
   if(float.IsNaN(now)||float.IsInfinity(now))return false;
   if(now<started||now-started>=.1f){started=now;used=0;}
   if(used>=(heavy?8:6))return false;used++;return true;
  }
 }
 public static class ElementalAudio {
  sealed class Throttle {public Throttle(){}public readonly Dictionary<string,int> last=new Dictionary<string,int>();}
  static readonly ConditionalWeakTable<Pawn,Throttle> limits=new ConditionalWeakTable<Pawn,Throttle>();
  static readonly ConditionalWeakTable<Map,ElementalAudioBudget> budgets=new ConditionalWeakTable<Map,ElementalAudioBudget>();
  static readonly Dictionary<string,SoundDef> sounds=new Dictionary<string,SoundDef>();
  static readonly string[] profiles={"Lightning","Flame","Wind","Water","AzureFlame","Thunderstorm","Glacial","Cyclone","Celestial","Earth"};
  static readonly string[,] names=BuildNames();
  static string[,] BuildNames(){var result=new string[10,7];for(int p=0;p<10;p++)for(int c=0;c<7;c++){int use=c==5&&p!=3&&p!=6&&p!=8?0:c==6&&p!=6&&p!=8?4:c;result[p,c]="EH_Sound_"+profiles[p]+"_"+(ElementalSound)use;}return result;}
  public static int Profile(Pawn p){
   if(CelestialAscendant.Active(p))return 8;if(CycloneEvolution.Active(p))return 7;if(GlacialEvolution.Active(p))return 6;if(ThunderstormEvolution.Active(p))return 5;if(AzureEvolution.Active(p))return 4;
   string element=ElementalTeaching.Element(p);if(element==null&&ElementalTeaching.Trained(p)){foreach(var h in p.health.hediffSet.hediffs){var disciple=h.TryGetComp<HediffComp_ElementalApprentice>();if(disciple!=null){element=disciple.DiscipleElement;break;}}}
   return element=="Earth"?9:element=="Flame"?1:element=="Wind"?2:(element=="Water"||element=="Lunar")?3:0;
  }
  public static string SoundName(Pawn p,ElementalSound cue){string name=names[Profile(p),Math.Max(0,Math.Min(6,(int)cue))];return AdamantEvolution.Active(p)?name.Replace("_Earth_","_Adamant_"):name;}
  public static SoundDef Resolve(string name){SoundDef sound;if(sounds.TryGetValue(name,out sound))return sound;sound=DefDatabase<SoundDef>.GetNamedSilentFail(name);if(sounds.Count>=128)sounds.Clear();sounds[name]=sound;return sound;}
  public static int Delay(string name){if((name.StartsWith("EH_Sound_Lightning_")||name.StartsWith("EH_Sound_Thunderstorm_"))&&(name.EndsWith("_Slash")||name.EndsWith("_Spin")||name.EndsWith("_Impact")))return 30;return name.EndsWith("_Slash")?4:name.EndsWith("_Parry")?6:name.EndsWith("_Burst")?18:name.EndsWith("_Spin")||name.EndsWith("_Impact")||name.EndsWith("_Charge")?12:8;}
  public static void Ability(Pawn pawn,ElementalSound cue){Play(pawn,SoundName(pawn,cue));}
  public static void Play(Pawn pawn,params string[] requested){
   if(pawn==null||!pawn.Spawned||pawn.Map!=Find.CurrentMap||pawn.Position.Fogged(pawn.Map))return;
   if(Find.CameraDriver!=null&&!Find.CameraDriver.CurrentViewRect.ExpandedBy(8).Contains(pawn.Position))return;
   int now=Find.TickManager.TicksGame;var limit=limits.GetOrCreateValue(pawn);var budget=budgets.GetOrCreateValue(pawn.Map);
   foreach(string name in requested){
    if(string.IsNullOrEmpty(name))continue;int last;if(limit.last.TryGetValue(name,out last)&&now>=last&&now-last<Delay(name))continue;
    var sound=Resolve(name);if(sound==null)continue;
    bool heavy=name.EndsWith("_Impact")||name.EndsWith("_Burst");if(!budget.Accept(Time.realtimeSinceStartup,heavy))continue;
    limit.last[name]=now;sound.PlayOneShot(SoundInfo.InMap(new TargetInfo(pawn.Position,pawn.Map)));
   }
  }
  public static void Transformation(Pawn p,string element,bool active){Play(p,"EH_TransformLock");if(active)Ability(p,ElementalSound.Impact);}
 }
}

