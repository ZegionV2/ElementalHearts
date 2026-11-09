using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace ElementalHearts {
 public static class CelestialBestowOptions {
  public static readonly string[] Elements={"Flame","Lightning","Water","Wind","Earth","Lunar"};
  public static readonly string[] Evolutions={"Azure Flame","Thunderstorm","Glacial","Cyclone","Adamant","Eclipse"};
  public static int Encode(string element,bool ready){int i=Array.IndexOf(Elements,element);if(i<0)throw new ArgumentOutOfRangeException("element");return i+1+(ready?16:0);}
  public static bool Decode(int code,out string element,out bool ready){int index=(code&15)-1;ready=(code&16)!=0;element=index>=0&&index<Elements.Length?Elements[index]:null;return code>0&&code<32&&element!=null;}
  public static bool CanOpen(Pawn giver){return CelestialAscendant.Active(giver)&&ElementalTeaching.Ready(giver)&&ElementalCombatPolicy.Controlled(giver)&&!ElementalBestow.Busy(giver)&&!CelestialAscendant.Busy(giver)&&!ElementalTeachingSession.Busy(giver)&&!ElementalAnimationBridge.Controlled(giver)&&!HediffComp_ElementalTransform.Transforming(giver)&&!AzureEvolution.Evolving(giver);}
  public static bool Start(Pawn giver,Pawn pupil,string element,bool ready){if(!CanOpen(giver)||!ElementalBestow.Valid(giver,pupil)||!giver.CanReach(pupil,PathEndMode.Touch,Danger.Deadly))return false;var job=JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed(ElementalBestow.Giving),pupil);job.count=Encode(element,ready);job.playerForced=true;return giver.jobs.TryTakeOrderedJob(job,JobTag.Misc);}
  public static void Target(Pawn giver,string element,bool ready){
   if(!CanOpen(giver)||Array.IndexOf(Elements,element)<0)return;var c=HediffComp_CelestialAbilities.On(giver);c.bestowElement=element;c.bestowEvolutionReady=ready;
   var parameters=new TargetingParameters{canTargetPawns=true,canTargetLocations=false,canTargetBuildings=false,canTargetItems=false,canTargetSelf=false,validator=t=>ElementalBestow.Valid(giver,t.Thing as Pawn)&&giver.CanReach(t.Thing,PathEndMode.Touch,Danger.Deadly)};
   Find.Targeter.BeginTargeting(parameters,t=>Start(giver,t.Pawn,element,ready),(Action<LocalTargetInfo>)null,null,giver,null,ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Bestow"));
  }
 }
 public sealed class Command_CelestialBestow:Command_Target {
  public Pawn giver;
  public override void ProcessInput(Event ev){if(!Disabled&&CelestialBestowOptions.CanOpen(giver))Find.WindowStack.Add(new Dialog_CelestialBestow(giver));}
 }
}

