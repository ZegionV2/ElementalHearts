using System;using System.Collections.Generic;using System.Reflection;using HarmonyLib;using RimWorld;using RimWorld.Planet;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static partial class CelestialLightTravel {
  enum PlanStage {World,Preparing,Cell}
  sealed class Plan {
   public Pawn pawn;public Map origin,map;public World world;public PlanetTile destination;public bool precise,paused;public TimeSpeed speed;public IntVec3 view;public float zoom;public PlanStage stage;
   public Func<GlobalTargetInfo,bool> worldAction;public Action<LocalTargetInfo> cellAction;public readonly List<IntVec3> highlight=new List<IntVec3>{IntVec3.Invalid};
  }
  static Plan planning;
  static readonly FieldInfo worldActionField=AccessTools.Field(typeof(WorldTargeter),"action"),cellActionField=AccessTools.Field(typeof(Targeter),"action");
  static Plan NewPlan(Pawn p,bool precise,TimeSpeed speed){return new Plan{pawn=p,origin=p.Map,world=Find.World,destination=PlanetTile.Invalid,precise=precise,speed=speed,paused=speed==TimeSpeed.Paused,view=Find.CurrentMap==p.Map?Find.CameraDriver.MapPosition:p.Position,zoom=Find.CameraDriver.RootSize};}
  static bool ValidPlan(Plan p){return p!=null&&p.world==Find.World&&p.pawn!=null&&p.pawn.Spawned&&p.pawn.Map==p.origin&&Problem(p.pawn)==null;}
  public static bool PlanningAt(PlanetTile tile){if(planning!=null&&planning.world!=Find.World)planning=null;return ValidPlan(planning)&&planning.destination==tile;}
  public static void ClearPlanningOnLoad(){planning=null;}
  public static void UpdatePlanning(){var p=planning;if(p==null)return;if(p.world!=Find.World){planning=null;return;}if(!ValidPlan(p)){CancelPlan(p,true);return;}
   if(p.stage==PlanStage.World&&(worldActionField==null||!ReferenceEquals(worldActionField.GetValue(Find.WorldTargeter),p.worldAction))){CancelPlan(p,!Find.WorldTargeter.IsTargeting);return;}
   if(p.stage==PlanStage.Cell&&(cellActionField==null||!ReferenceEquals(cellActionField.GetValue(Find.Targeter),p.cellAction))){CancelPlan(p,!Find.Targeter.IsTargeting);return;}Find.TickManager.Pause();
  }
  static void RestoreView(Plan p){if(p.world!=Find.World||p.pawn==null||!p.pawn.Spawned||p.pawn.Map!=p.origin)return;Current.Game.CurrentMap=p.origin;CameraJumper.TryJump(p.view.InBounds(p.origin)?p.view:p.pawn.Position,p.origin);Find.CameraDriver.SetRootPosAndSize((p.view.InBounds(p.origin)?p.view:p.pawn.Position).ToVector3Shifted(),p.zoom);Find.Selector.ClearSelection();Find.Selector.Select(p.pawn);}
  static void CancelPlan(Plan p,bool restoreView){if(planning!=p)return;planning=null;if(p.world!=Find.World)return;if(restoreView)RestoreView(p);Find.TickManager.CurTimeSpeed=p.speed;}
  static void ReplacePlan(){var p=planning;if(p==null)return;CancelPlan(p,true);if(p.stage==PlanStage.World&&Find.WorldTargeter.IsTargeting&&worldActionField!=null&&ReferenceEquals(worldActionField.GetValue(Find.WorldTargeter),p.worldAction))Find.WorldTargeter.StopTargeting();if(p.stage==PlanStage.Cell&&Find.Targeter.IsTargeting&&cellActionField!=null&&ReferenceEquals(cellActionField.GetValue(Find.Targeter),p.cellAction))Find.Targeter.StopTargeting();}
  public static Command_Action Command(Pawn p){var c=new Command_CelestialLightTravel{pawn=p,defaultLabel="Light Travel",defaultDesc="Choose a world destination and arrive automatically in a safe cell. No fuel, distance cost or cooldown. Mountains and existing maps are supported. Right-click for a precise landing or relocation on this map. The destination is prepared before departure, so canceling leaves the traveler at the origin. Short solar departure/arrival beams accompany the transfer. Put down carried pawns or items first.",icon=CelestialVisualAssets.Icon(6),groupable=false,action=()=>Begin(p)};string problem=Problem(p);if(problem!=null)c.Disable(problem);return c;}
  public static void Begin(Pawn p){Begin(p,IncarnateTransit.Thunder(p));}
  public static void Begin(Pawn p,bool precise){string problem=Problem(p);if(problem!=null){Messages.Message(problem,MessageTypeDefOf.RejectInput,false);return;}ReplacePlan();var plan=NewPlan(p,precise,Find.TickManager.CurTimeSpeed);planning=plan;plan.stage=PlanStage.World;Find.TickManager.Pause();CameraJumper.TryJump(new GlobalTargetInfo(p.Map.Tile));
   plan.worldAction=t=>{if(!ValidPlan(plan)||planning!=plan||!t.IsValid||!DestinationAllowed(plan.origin.Tile,t.Tile))return false;plan.destination=t.Tile;plan.precise=precise||t.Tile==plan.origin.Tile;plan.stage=PlanStage.Preparing;PreparePlan(plan);return true;};
   Find.WorldTargeter.BeginTargeting(plan.worldAction,true,IncarnateTransit.Icon(p),true,()=>Find.TickManager.Pause(),t=>t.IsValid&&DestinationAllowed(plan.origin.Tile,t.Tile)?new TaggedString(precise?"Choose a world tile, then click your exact landing cell. No cost.":"Choose a destination for quick automatic landing. No cost."):new TaggedString("Choose a land tile or an existing map on this world."),t=>t.IsValid&&DestinationAllowed(plan.origin.Tile,t.Tile),plan.origin.Tile,true);
  }
  public static void BeginLocal(Pawn p){if(Problem(p)==null)QueuePlan(p,p.Map.Tile,true,Find.TickManager.CurTimeSpeed==TimeSpeed.Paused);}
  public static void QueuePlan(Pawn p,PlanetTile destination,bool precise,bool paused){if(Problem(p)!=null||!DestinationAllowed(p.Map.Tile,destination))return;ReplacePlan();var speed=paused?TimeSpeed.Paused:Find.TickManager.CurTimeSpeed==TimeSpeed.Paused?TimeSpeed.Normal:Find.TickManager.CurTimeSpeed;var plan=NewPlan(p,precise,speed);plan.destination=destination;plan.stage=PlanStage.Preparing;planning=plan;Find.TickManager.Pause();PreparePlan(plan);}
  static void PreparePlan(Plan plan){var parent=Find.WorldObjects.MapParentAt(plan.destination);if(parent!=null&&parent.HasMap){plan.map=parent.Map;FinishPlan(plan);return;}
   LongEventHandler.QueueLongEvent(()=>{try{if(planning!=plan||!ValidPlan(plan))return;plan.map=PrepareMap(plan.destination,IncarnateTransit.Thunder(plan.pawn));LongEventHandler.ExecuteWhenFinished(()=>FinishPlan(plan));}catch(Exception e){CancelPlan(plan,true);Log.Error("[Elemental Hearts] Travel preparation failed; pawn retained at origin: "+e);Messages.Message("This destination could not be prepared. The traveler remains at the origin.",MessageTypeDefOf.RejectInput,false);}},"GeneratingMap",true,null);
  }
  static bool ValidCell(Plan plan,IntVec3 cell){return planning==plan&&ValidPlan(plan)&&Find.CurrentMap==plan.map&&!WorldRendererUtility.WorldSelected&&LandingSafe(cell,plan.map,IncarnateTransit.Thunder(plan.pawn));}
  static void FocusLanding(Map map,Pawn p,float zoom){IntVec3 cell;if(map==p.Map)cell=p.Position;else if(!AutoCell(map,map.Center,out cell))cell=map.Center;float aspect=UI.screenWidth/(float)Math.Max(1,UI.screenHeight);float size=Math.Min(Mathf.Clamp(zoom,10,24),Math.Max(6,Math.Min(map.Size.z/2f-2,map.Size.x/(2*aspect)-2)));int mx=Math.Min((map.Size.x-1)/2,Mathf.CeilToInt(size*aspect)+2),mz=Math.Min((map.Size.z-1)/2,Mathf.CeilToInt(size)+2);cell.x=Mathf.Clamp(cell.x,mx,map.Size.x-mx-1);cell.z=Mathf.Clamp(cell.z,mz,map.Size.z-mz-1);Current.Game.CurrentMap=map;CameraJumper.TryJump(cell,map);Find.CameraDriver.SetRootPosAndSize(cell.ToVector3Shifted(),size);}
  static void FinishPlan(Plan plan){if(planning!=plan)return;if(!ValidPlan(plan)||plan.map==null){CancelPlan(plan,true);return;}
   if(plan.precise){plan.stage=PlanStage.Cell;Find.TickManager.Pause();FocusLanding(plan.map,plan.pawn,plan.zoom);var target=TargetingParameters.ForCell();target.validator=t=>Find.CurrentMap==plan.map&&t.Cell.InBounds(plan.map);bool chosen=false;
    plan.cellAction=t=>{if(!ValidCell(plan,t.Cell))return;chosen=true;CommitPlan(plan,t.Cell);};
    Find.Targeter.BeginTargeting(target,plan.cellAction,t=>{if(t.Cell.InBounds(plan.map)&&Find.CurrentMap==plan.map){plan.highlight[0]=t.Cell;GenDraw.DrawFieldEdges(plan.highlight,ValidCell(plan,t.Cell)?Color.yellow:Color.red);}},t=>ValidCell(plan,t.Cell),null,()=>{if(!chosen)CancelPlan(plan,true);},IncarnateTransit.Icon(plan.pawn),true,null,t=>{if(planning==plan)UpdatePlanning();});
    Messages.Message("Click an empty walkable landing cell. Yellow is valid; red is blocked. Right-click or Escape cancels safely.",MessageTypeDefOf.NeutralEvent,false);return;
   }
   IntVec3 cell;if(!AutoCell(plan.map,IntVec3.Invalid,out cell)){CancelPlan(plan,true);Messages.Message("No safe arrival cell was found. Try an exact landing or another tile.",MessageTypeDefOf.RejectInput,false);return;}CommitPlan(plan,cell);
  }
  static void CommitPlan(Plan plan,IntVec3 cell){if(planning!=plan||!ValidPlan(plan))return;planning=null;bool keepLandingView=IncarnateTransit.Thunder(plan.pawn)&&plan.precise;if(!keepLandingView)RestoreView(plan);Find.TickManager.CurTimeSpeed=plan.speed;if(!TryDepart(plan.pawn,plan.destination,cell,plan.paused,plan.precise)){RestoreView(plan);Find.TickManager.CurTimeSpeed=plan.speed;Messages.Message("The traveler cannot depart right now. Nothing has been moved.",MessageTypeDefOf.RejectInput,false);}}
 }
 public sealed class Command_CelestialLightTravel:Command_Action {
  public Pawn pawn;
  public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions{get{if(Disabled)yield break;yield return new FloatMenuOption("Choose precise world landing",()=>CelestialLightTravel.Begin(pawn,true));yield return new FloatMenuOption("Relocate on this map",()=>CelestialLightTravel.BeginLocal(pawn));}}
 }
}
