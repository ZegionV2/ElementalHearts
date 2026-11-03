using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class EarthVfx {
  public static readonly string[] Sequences={"FaultlineCleave","StonebreakerAdvance","BastionStance","Mountainfall","Idle","Combat","Activation"};
  static readonly Material[,] frames=new Material[7,6];
  static readonly Texture2D[] icons=new Texture2D[4];
  public static Texture2D Icon(int i){if(icons[i]==null)icons[i]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Earth/"+Sequences[i]);return icons[i];}
  public static Material Frame(int sequence,int frame){frame=Math.Max(0,Math.Min(5,frame));if(frames[sequence,frame]==null)frames[sequence,frame]=MaterialPool.MatFrom(ContentFinder<Texture2D>.Get("Effects/ElementalHearts/Earth/Frames/"+Sequences[sequence]+"_"+frame.ToString("00")),ShaderDatabase.Transparent,Color.white);return frames[sequence,frame];}
  public static void Emit(Pawn p,int sequence,Vector3 from,Vector3 to,int ticks,float size,float angle=0,bool telegraph=false){
   if(p==null||!p.Spawned||ElementalHeartsMod.Settings.effectOpacity<=0||p.Map!=Find.CurrentMap||Find.CameraDriver==null)return;
   var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(8);if(!visible.Contains(from.ToIntVec3())&&!visible.Contains(to.ToIntVec3()))return;
   p.Map.GetComponent<MapComponent_Earth>().Emit(sequence,from,to,ticks,size,angle,telegraph);
  }
 }
 public sealed class MapComponent_Earth:MapComponent {
  struct Effect {public bool active,telegraph;public int sequence,start,duration;public Vector3 from,to;public float size,angle;}
  const int EffectLimit=96,VisibleLimit=64;
  readonly Effect[] effects=new Effect[EffectLimit];
  readonly List<Pawn> pawns=new List<Pawn>();
  MaterialPropertyBlock color;
  public MapComponent_Earth(Map map):base(map){}
  public void Register(Pawn p){if(p!=null&&!pawns.Contains(p))pawns.Add(p);}
  public void Forget(Pawn p){pawns.Remove(p);}
  public override void FinalizeInit(){pawns.Clear();foreach(var p in map.mapPawns.AllPawnsSpawned)if(HediffComp_EarthAbilities.On(p)!=null)Register(p);}
  public override void MapComponentTick(){
   if(Current.Game==null||!Current.Game.Maps.Contains(map)||LongEventHandler.ShouldWaitForEvent)return;
   int now=Find.TickManager.TicksGame;
   // Remove invalid entries before ticking: native damage/promotion may mutate registration.
   for(int i=pawns.Count-1;i>=0;i--){
    if(i>=pawns.Count)continue;var p=pawns[i];var c=HediffComp_EarthAbilities.On(p);
    if(p==null||!p.Spawned||p.Map!=map||c==null){pawns.RemoveAt(i);continue;}c.AdvanceTick(now);
   }
  }
  public float AlliedProtection(Pawn victim){
   float best=1f;if(victim.Faction==null)return best;
   for(int i=0;i<pawns.Count;i++){var p=pawns[i];if(p==victim||p==null||!p.Spawned||p.Map!=map||p.Faction!=victim.Faction||p.HostileTo(victim))continue;
    var c=HediffComp_EarthAbilities.On(p);if(c==null||!c.GuardActive||p.Position.DistanceToSquared(victim.Position)>9||!LightningCombat.ClearLine(p.Position,victim.Position,map))continue;
    best=Math.Min(best,1f-.1f*ElementalTeaching.DamageFactor(p));
   }
   return best;
  }
  public void Emit(int sequence,Vector3 from,Vector3 to,int ticks,float size,float angle,bool telegraph){
   int now=Find.TickManager.TicksGame;
   for(int i=0;i<effects.Length;i++){if(effects[i].active&&now<effects[i].start+effects[i].duration)continue;
    effects[i]=new Effect{active=true,telegraph=telegraph,sequence=sequence,start=now,duration=Math.Max(1,Math.Min(180,ticks)),from=from,to=to,size=size,angle=angle};return;}
  }
  void Draw(int sequence,int frame,Vector3 at,float size,float angle,float opacity,bool ground){
   if(color==null)color=new MaterialPropertyBlock();
   color.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,opacity*ElementalHeartsMod.Settings.effectOpacity));
   Graphics.DrawMesh(WindVfx.FrameMesh(ground),Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(size,1,size)),EarthVfx.Frame(sequence,frame),0,null,0,color);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   int now=Find.TickManager.TicksGame;var view=Find.CameraDriver.CurrentViewRect.ExpandedBy(8);
   for(int i=0;i<effects.Length;i++){
    var e=effects[i];if(!e.active)continue;int age=now-e.start;
    if(age>=e.duration){effects[i].active=false;continue;}var at=e.sequence==1?Vector3.Lerp(e.from,e.to,Mathf.Clamp01(age/(float)e.duration)):e.from;
    var cell=at.ToIntVec3();if(!cell.InBounds(map)||!view.Contains(cell)||cell.Fogged(map))continue;
    at.y=AltitudeLayer.MoteOverhead.AltitudeFor();
    Draw(e.sequence,e.telegraph?0:EarthRules.Frame(age,e.duration),at,e.size,e.angle,e.telegraph?.35f:.85f,false);
   }
   int drawn=0;
   foreach(var p in pawns){
    var cast=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_EarthTechnique;
    if(cast!=null&&cast.PendingMountainfall){var target=cast.TelegraphTarget;if(target!=null&&target.Spawned&&target.Map==map&&view.Contains(target.Position)&&!target.Position.Fogged(map)){var marker=target.DrawPos;marker.y=AltitudeLayer.MoteOverhead.AltitudeFor();Draw(3,0,marker,6f,0,.4f,false);}}
    if(p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!view.Contains(p.Position)||p.Position.Fogged(map))continue;
    var c=HediffComp_EarthAbilities.On(p);if(c==null)continue;
    if(c.GuardActive){var ring=p.DrawPos;ring.y=AltitudeLayer.MoteOverhead.AltitudeFor();Draw(2,(now/5+p.thingIDNumber)%6,ring,6.3f,0,.45f,false);}
    if(!ElementalHeartsMod.Settings.showAura||!c.Full||!p.Drafted||!HediffComp_ElementalTransform.Active(p)||HediffComp_ElementalTransform.Transforming(p))continue;
    if(drawn++>=VisibleLimit)break;
    var pos=p.DrawPos;pos.y-=.01f;pos.z-=.35f;
    int state=c.activationUntil>now?6:c.combatUntil>now?5:4;
    int frame=state==6?EarthRules.Frame(30-(c.activationUntil-now),30):(now/5+p.thingIDNumber)%6;
    Draw(state,frame,pos,2.65f,0,.28f+.1f*Mathf.Sin(now*.045f+p.thingIDNumber),true);
   }
  }
 }
}

