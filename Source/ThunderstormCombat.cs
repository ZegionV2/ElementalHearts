using System;using System.Collections.Generic;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class ThunderstormVfx {
  static readonly Material[] mats=new Material[6];
  public static Material Material(int sprite){return mats[sprite]??(mats[sprite]=MaterialPool.MatFrom("Effects/ElementalHearts/Thunderstorm/Storm_"+sprite.ToString("00"),ShaderDatabase.Transparent));}
  public static void Track(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_Thunderstorm>().Track(p);}
  public static void Emit(Pawn p,Map map,int sprite,Vector3 start,Vector3 end,int duration,float size,float angle=float.NaN){
   if(!ThunderstormEvolution.Active(p)){LightningVfx.Add(map,sprite,start,end,duration,size,angle);return;}
   if(map==null||map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(sprite<2&&(end-start).sqrMagnitude>.1f)StormBlitzFx.Streak(p,start,end);
   map.GetComponent<MapComponent_Thunderstorm>().Effect(sprite==2?ScarletTempestRules.Sprite:sprite==4?5:Math.Min(4,sprite),start,end,duration,size,float.IsNaN(angle)?(end-start).AngleFlat():angle);
  }
  public static void Afterimage(Pawn p,Vector3 pos){if(!ThunderstormEvolution.Active(p)){LightningVfx.Afterimage(p,pos);return;}StormBlitzFx.Echo(p,pos);Emit(p,p.Map,1,pos,pos,12,1.3f,p.Rotation.AsAngle);}
 }
 public sealed class ThunderstormZone:IExposable {
  public Pawn source;public IntVec3 cell;public bool circuit;public int due,pulses=3;public float damage,physical,electrical;
  public void ExposeData(){Scribe_References.Look(ref source,"source");Scribe_Values.Look(ref cell,"cell");Scribe_Values.Look(ref circuit,"circuit");Scribe_Values.Look(ref due,"due");Scribe_Values.Look(ref pulses,"pulses",3);Scribe_Values.Look(ref damage,"damage");Scribe_Values.Look(ref physical,"physical");Scribe_Values.Look(ref electrical,"electrical");}
 }
 public sealed class MapComponent_Thunderstorm:MapComponent {
  struct Sprite{public bool active;public int index,tick,duration;public Vector3 start,end;public float size,angle;}
  readonly Sprite[] sprites=new Sprite[96];readonly List<Pawn> halos=new List<Pawn>();List<ThunderstormZone> zones=new List<ThunderstormZone>();MaterialPropertyBlock block;
  public MapComponent_Thunderstorm(Map map):base(map){}
  public void Track(Pawn p){if(p!=null&&!halos.Contains(p))halos.Add(p);}
  // Legacy saved zone data remains readable, but the replaced skill never schedules pulses.
  public bool Queue(Pawn source,IntVec3 cell,bool circuit){return false;}
  public void Effect(int index,Vector3 start,Vector3 end,int duration,float size,float angle){int now=Find.TickManager.TicksGame;for(int i=0;i<sprites.Length;i++)if(!sprites[i].active||now-sprites[i].tick>=sprites[i].duration){sprites[i]=new Sprite{active=true,index=index,start=start,end=end,tick=now,duration=Math.Max(1,duration),size=size,angle=angle};return;}}
  public override void ExposeData(){base.ExposeData();Scribe_Collections.Look(ref zones,"EH_thunderstormZones",LookMode.Deep);if(zones==null)zones=new List<ThunderstormZone>();}
  public static List<Pawn> SelectTargets(List<Pawn> candidates,bool circuit){var selected=new List<Pawn>();var seen=new HashSet<int>();foreach(var p in candidates)if(p!=null&&seen.Add(p.thingIDNumber)){selected.Add(p);if(circuit&&selected.Count>=3)break;}return selected;}
  public void DiscardLegacyZones(){zones.Clear();}
  public override void MapComponentTick(){
   int now=Find.TickManager.TicksGame;if(now%60==0)for(int i=halos.Count-1;i>=0;i--){var p=halos[i];if(p==null||!p.Spawned||p.Map!=map||p.Dead||!ThunderstormEvolution.Active(p))halos.RemoveAt(i);}
   if(zones.Count>0)DiscardLegacyZones();
  }

  void Draw(int sprite,Vector3 pos,float size,float angle,float alpha){if(!pos.ToIntVec3().InBounds(map)||pos.ToIntVec3().Fogged(map))return;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha*ElementalHeartsMod.Settings.effectOpacity));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(size,1,size)),ThunderstormVfx.Material(sprite),0,null,0,block);}
  void DrawSpin(Vector3 pos,float size,int elapsed,int duration){if(!pos.ToIntVec3().InBounds(map)||pos.ToIntVec3().Fogged(map))return;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,ScarletTempestRules.Opacity(elapsed,duration)*ElementalHeartsMod.Settings.effectOpacity));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(size,1,size)),ScarletTempestVfx.Material(ScarletTempestRules.Frame(elapsed,duration)),0,null,0,block);}
  void DrawHalo(int frame,Vector3 pos,float size,float alpha){if(alpha<=.001f)return;if(!pos.ToIntVec3().InBounds(map)||pos.ToIntVec3().Fogged(map))return;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,.9f*alpha*ElementalHeartsMod.Settings.effectOpacity));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(size,1,size)),StormBlitzFx.Halo(frame),0,null,0,block);}
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;if(block==null)block=new MaterialPropertyBlock();int now=Find.TickManager.TicksGame;var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
   for(int i=0;i<sprites.Length;i++){var e=sprites[i];if(!e.active)continue;float t=(now-e.tick)/(float)e.duration;if(t>=1){sprites[i].active=false;continue;}var pos=Vector3.Lerp(e.start,e.end,Mathf.Clamp01(t*2));if(!visible.Contains(pos.ToIntVec3()))continue;if(e.index==ScarletTempestRules.Sprite)DrawSpin(pos,e.size,now-e.tick,e.duration);else Draw(e.index,pos,e.size*(.9f+.15f*t),e.index<2?e.angle-90:0,Mathf.Min(1,(1-t)*4));}
   if(!ElementalHeartsMod.Settings.showAura)return;foreach(var p in halos){if(p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!p.Awake()||!visible.Contains(p.Position)||!AzureHaloRules.Visible(ThunderstormEvolution.Active(p),p.Drafted,HediffComp_ElementalTransform.Active(p),HediffComp_ElementalTransform.Transforming(p)))continue;float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var pos=p.DrawPos+p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);pos.z+=(.43f+.025f*(float)Math.Sin(now*.08f))*life;int frame=StormBlitzFx.HaloFrame(now,p.thingIDNumber);float blend=StormBlitzFx.HaloBlend(now,p.thingIDNumber);DrawHalo(frame,pos,1.1f*life,1-blend);DrawHalo((frame+1)%6,pos,1.1f*life,blend);}
  }
 }
 public sealed class HediffCompProperties_TempestCircuit:HediffCompProperties {public HediffCompProperties_TempestCircuit(){compClass=typeof(HediffComp_TempestCircuit);}}
 public sealed class HediffComp_TempestCircuit:HediffComp {
  public int readyAt;
  public static HediffComp_TempestCircuit On(Pawn p){if(p==null||p.health==null)return null;foreach(var h in p.health.hediffSet.hediffs){var c=h.TryGetComp<HediffComp_TempestCircuit>();if(c!=null)return c;}return null;}
  public override void CompExposeData(){Scribe_Values.Look(ref readyAt,"EH_tempestCircuitReadyAt");}
  bool Ready(){var c=HediffComp_LightningAbilities.On(Pawn);return ThunderstormEvolution.Active(Pawn)&&On(Pawn)==this&&c!=null&&c.CanAct&&Find.TickManager.TicksGame>=readyAt&&(Pawn.CurJob==null||Pawn.CurJob.def!=LightningDefs.EH_HeavenlyThunder);}
  public bool TryCast(Pawn target,bool automatic){if(!Ready()||!TempestDragon.TryStart(Pawn,target,automatic))return false;readyAt=Find.TickManager.TicksGame+ThunderstormRules.CircuitCooldown;return true;}
  public bool TryAutoCast(){if(!Ready())return false;var targets=LightningCombat.Targets(Pawn,Pawn.Position,TempestDragonRules.FirstRange);targets.Sort((a,b)=>Pawn.Position.DistanceToSquared(a.Position).CompareTo(Pawn.Position.DistanceToSquared(b.Position)));int checkedRoutes=0;foreach(var target in targets){if(!TempestDragon.Enemy(Pawn,target))continue;if(checkedRoutes++>=TempestDragonRules.CandidateLimit)break;if(TryCast(target,true))return true;}return false;}
  public override IEnumerable<Gizmo> CompGetGizmos(){
   if(!ThunderstormEvolution.Active(Pawn)||!ElementalCombatPolicy.Controlled(Pawn)||!Pawn.Drafted)yield break;
   var cmd=new Command_Target{defaultLabel="Tempest Dragon",defaultDesc="Become the red lightning dragon: vanish into a blazing flash, carve sharp zigzags through the battlefield, and strike each chosen foe once.\n\nChoose a hostile pawn within 10 cells. Chain through up to 5 different enemies; each later target must be within 8 cells of your last landing. Every approach uses two straight dash legs, leaving an animated white-hot crimson trail and regalia afterimages. Moving enemies can evade their planned landing.\n\nHits deal weapon-derived physical and electrical damage and apply Charged on a successful surviving hit. Walls and occupied stopping points block routes. Hostiles only; non-lethal mode is respected. Electrical hits also have a 40% chance to shock for 1.5 seconds, followed by three seconds of discharge recovery. No resource cost. 18-second cooldown, spent if interrupted.\n\nAuto Combat uses Tempest Dragon whenever enabled and a valid route is available, including against a single enemy. Incapacitation, undrafting or replacement orders end the chain. Normal movement and skills resume afterward.",icon=ThunderstormEvolution.Icon(4),groupable=false,targetingParams=new TargetingParameters{canTargetLocations=false,canTargetPawns=true,canTargetBuildings=false,canTargetItems=false,validator=t=>Pawn.Spawned&&t.Thing is Pawn&&TempestDragon.Enemy(Pawn,t.Thing as Pawn)&&Pawn.Position.DistanceToSquared(t.Cell)<=100},action=t=>{if(!TryCast(t.Pawn,false))Messages.Message("No clear Tempest Dragon zigzag route, or the ability is not ready.",MessageTypeDefOf.RejectInput,false);},onUpdate=t=>{if(Pawn.Spawned)GenDraw.DrawRadiusRing(Pawn.Position,10);}};
   var c=HediffComp_LightningAbilities.On(Pawn);if(c==null||!c.CanAct||Pawn.CurJob!=null&&Pawn.CurJob.def==LightningDefs.EH_HeavenlyThunder)cmd.Disable("Finish the current action first.");else if(readyAt>Find.TickManager.TicksGame)cmd.Disable("Recharging: "+Math.Ceiling((readyAt-Find.TickManager.TicksGame)/60f)+"s");yield return cmd;
  }
 }
}


