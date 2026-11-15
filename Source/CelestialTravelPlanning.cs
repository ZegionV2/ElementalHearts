using System;using System.Collections.Generic;using System.Reflection;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static partial class CelestialLightTravel {
  sealed class Plan {
   public Pawn pawn;public Map origin;public World world;public PlanetTile destination=PlanetTile.Invalid;public bool precise,paused;public TimeSpeed speed;public IntVec3 view;public float zoom;public Func<GlobalTargetInfo,bool> worldAction;
  }
  static Plan planning;
  static readonly FieldInfo worldActionField=AccessTools.Field(typeof(WorldTargeter),"action");
  public static int TravelTicks(float distance){if(float.IsNaN(distance)||distance<=0)return 60;return Math.Max(60,Math.Min(6000,(int)Math.Ceiling(Math.Min(750f,distance)*8f)));}
  static Plan NewPlan(Pawn p,bool precise,TimeSpeed speed){return new Plan{pawn=p,origin=p.Map,world=Find.World,precise=precise,speed=speed,paused=speed==TimeSpeed.Paused,view=Find.CurrentMap==p.Map?Find.CameraDriver.MapPosition:p.Position,zoom=Find.CameraDriver.RootSize};}
  static bool ValidPlan(Plan p){return p!=null&&p.world==Find.World&&p.pawn!=null&&p.pawn.Spawned&&p.pawn.Map==p.origin&&Problem(p.pawn)==null;}
  public static bool PlanningAt(PlanetTile tile){return ValidPlan(planning)&&planning.destination==tile;}
  public static void ClearPlanningOnLoad(){planning=null;}
  public static void UpdatePlanning(){var p=planning;if(p==null)return;if(p.world!=Find.World){planning=null;return;}if(!ValidPlan(p)){CancelPlan(p,true);return;}if(worldActionField==null||!ReferenceEquals(worldActionField.GetValue(Find.WorldTargeter),p.worldAction)){CancelPlan(p,!Find.WorldTargeter.IsTargeting);return;}Find.TickManager.Pause();}
  static void RestoreView(Plan p){if(p.world!=Find.World||p.pawn==null||!p.pawn.Spawned||p.pawn.Map!=p.origin)return;Current.Game.CurrentMap=p.origin;CameraJumper.TryJump(p.view.InBounds(p.origin)?p.view:p.pawn.Position,p.origin);Find.CameraDriver.SetRootPosAndSize((p.view.InBounds(p.origin)?p.view:p.pawn.Position).ToVector3Shifted(),p.zoom);Find.Selector.ClearSelection();Find.Selector.Select(p.pawn);}
  static void CancelPlan(Plan p,bool restoreView){if(planning!=p)return;planning=null;if(p.world!=Find.World)return;if(restoreView)RestoreView(p);Find.TickManager.CurTimeSpeed=p.speed;}
  static void ReplacePlan(){var p=planning;if(p==null)return;CancelPlan(p,true);if(Find.WorldTargeter.IsTargeting&&worldActionField!=null&&ReferenceEquals(worldActionField.GetValue(Find.WorldTargeter),p.worldAction))Find.WorldTargeter.StopTargeting();}
  public static Command_Action Command(Pawn p){var c=new Command_CelestialLightTravel{pawn=p,defaultLabel="Light Travel",defaultDesc=LivingGroupTransport.Description,icon=CelestialVisualAssets.Icon(6),groupable=false,action=()=>Begin(p)};var gathering=LivingGroupTransport.Existing(p);string problem=gathering==null?PreparationProblem(p):gathering.launching?"The party is taking flight.":null;if(problem!=null)c.Disable(problem);return c;}
  public static void Begin(Pawn p){LivingGroupTransport.PickSite(p);}
  public static void Begin(Pawn p,bool precise){LivingGroupTransport.PickSite(p);}
  public static void BeginLegacySolo(Pawn p,bool precise){
   string problem=Problem(p);if(problem!=null){Messages.Message(problem,MessageTypeDefOf.RejectInput,false);return;}ReplacePlan();var plan=NewPlan(p,precise,Find.TickManager.CurTimeSpeed);planning=plan;Find.TickManager.Pause();CameraJumper.TryJump(new GlobalTargetInfo(p.Map.Tile));
   plan.worldAction=t=>{if(!ValidPlan(plan)||planning!=plan||!t.IsValid||!DestinationAllowed(plan.origin.Tile,t.Tile))return false;plan.destination=t.Tile;Commit(plan);return true;};
   Find.WorldTargeter.BeginTargeting(plan.worldAction,true,IncarnateTransit.Icon(p),true,()=>Find.TickManager.Pause(),t=>{if(!t.IsValid||!DestinationAllowed(plan.origin.Tile,t.Tile))return new TaggedString("Choose another settlement or land tile on this world.");float seconds=TravelTicks(Find.WorldGrid.ApproxDistanceInTiles(plan.origin.Tile,t.Tile))/60f;return new TaggedString("Free flight - "+seconds.ToString("0.0")+" seconds at normal speed. "+(precise?"Choose landing cell on arrival.":"Find safe landing automatically on arrival."));},t=>t.IsValid&&DestinationAllowed(plan.origin.Tile,t.Tile),plan.origin.Tile,true);
  }
  public static void QueuePlan(Pawn p,PlanetTile destination,bool precise,bool paused){if(Problem(p)!=null||!DestinationAllowed(p.Map.Tile,destination))return;ReplacePlan();var speed=paused?TimeSpeed.Paused:Find.TickManager.CurTimeSpeed==TimeSpeed.Paused?TimeSpeed.Normal:Find.TickManager.CurTimeSpeed;var plan=NewPlan(p,precise,speed);plan.destination=destination;planning=plan;Commit(plan);}
  // Retain the public entry point for integrations, but world flight no longer teleports locally.
  public static void BeginLocal(Pawn p){Messages.Message("World flight needs another settlement or land tile. Use ordinary movement or an Incarnate's Step for movement on this map.",MessageTypeDefOf.RejectInput,false);}
  static void Commit(Plan plan){if(planning!=plan||!ValidPlan(plan))return;planning=null;RestoreView(plan);Find.TickManager.CurTimeSpeed=plan.speed;if(!TryDepart(plan.pawn,plan.destination,IntVec3.Invalid,plan.paused,plan.precise)){RestoreView(plan);Find.TickManager.CurTimeSpeed=plan.speed;Messages.Message("Cannot take flight right now. The pawn remains at the origin.",MessageTypeDefOf.RejectInput,false);}}
 }
 public sealed class Command_CelestialLightTravel:Command_Action {
  public Pawn pawn;
  public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions{get{if(Disabled)yield break;yield return new FloatMenuOption("Create or load party transport",()=>CelestialLightTravel.Begin(pawn));}}
 }
}
