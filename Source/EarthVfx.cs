using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 [StaticConstructorOnStartup]
 public static class EarthVfx {
  public static readonly string[] Sequences={"FaultlineCleave","StonebreakerAdvance","BastionStance","Mountainfall","Idle","Combat","Activation","SpireEruption","SeismicQuake"};
  static readonly Material[,] frames=new Material[9,8];
  static readonly Texture2D[] icons=new Texture2D[4];
  public static Texture2D Icon(int i){if(icons[i]==null)icons[i]=ContentFinder<Texture2D>.Get("UI/Commands/ElementalHearts/Earth/"+(i==2?"SeismicQuake":i==3?"SpireEruption":Sequences[i]));return icons[i];}
  public static Material Frame(int sequence,int frame){frame=Math.Max(0,Math.Min(sequence>=7?7:5,frame));if(frames[sequence,frame]==null)frames[sequence,frame]=MaterialPool.MatFrom(ContentFinder<Texture2D>.Get("Effects/ElementalHearts/Earth/Frames/"+Sequences[sequence]+"_"+frame.ToString("00")),ShaderDatabase.Transparent,Color.white);return frames[sequence,frame];}
  public static void Emit(Pawn p,int sequence,Vector3 from,Vector3 to,int ticks,float size,float angle=0,bool telegraph=false){
   if(p==null||!p.Spawned||ElementalHeartsMod.Settings.effectOpacity<=0||p.Map!=Find.CurrentMap||Find.CameraDriver==null)return;
   var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(8);if(!visible.Contains(from.ToIntVec3())&&!visible.Contains(to.ToIntVec3()))return;
   bool evolved=AdamantEvolution.Active(p);p.Map.GetComponent<MapComponent_Earth>().Emit(evolved?AdamantVfx.MapSequence(sequence):sequence,from,to,ticks,size,angle,telegraph,sequence==1?p:null,evolved);
  }
 }
 public sealed class MapComponent_Earth:MapComponent {
  struct Effect {public bool active,telegraph,adamant;public int sequence,start,duration,ownerJob;public Vector3 from,to;public float size,angle;public Pawn owner;}
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
    best=Math.Min(best,1f-(c.Evolved?.15f:.1f)*ElementalTeaching.DamageFactor(p));
   }
   return best;
  }
  public void Emit(int sequence,Vector3 from,Vector3 to,int ticks,float size,float angle,bool telegraph,Pawn owner=null,bool adamant=false){
   int now=Find.TickManager.TicksGame;
   for(int i=0;i<effects.Length;i++){if(effects[i].active&&now<effects[i].start+effects[i].duration)continue;
    effects[i]=new Effect{active=true,telegraph=telegraph,adamant=adamant,sequence=sequence,start=now,duration=Math.Max(1,Math.Min(180,ticks)),from=from,to=to,size=size,angle=angle,owner=owner,ownerJob=owner==null||owner.CurJob==null?-1:owner.CurJob.loadID};return;}
  }
  public void EmitAdamant(int sequence,Vector3 at,int ticks,float size){Emit(sequence,at,at,ticks,size,0,false,null,true);}
  void Draw(int sequence,int frame,Vector3 at,float size,float angle,float opacity,bool ground,bool adamant=false){
   if(color==null)color=new MaterialPropertyBlock();
   color.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,opacity*ElementalHeartsMod.Settings.effectOpacity));
   Graphics.DrawMesh(WindVfx.FrameMesh(ground),Matrix4x4.TRS(at,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(size,1,size)),(adamant?AdamantVfx.Frame(sequence,frame):EarthVfx.Frame(sequence,frame)),0,null,0,color);
  }
  void DrawPhase(int sequence,float phase,Vector3 at,float size,float angle,float opacity,bool ground,bool loop,bool adamant=false){
   int count=adamant||sequence>=7?8:6;
   phase=loop?Mathf.Repeat(phase,count):Mathf.Clamp(phase,0f,count-1);
   int first=(int)phase,second=loop?(first+1)%count:Math.Min(count-1,first+1);float blend=EarthRules.Smooth(phase-first);
   if(opacity<=.001f)return;
   if(first==second){Draw(sequence,first,at,size,angle,opacity,ground,adamant);return;}
   if(blend<.999f)Draw(sequence,first,at,size,angle,opacity*(1f-blend),ground,adamant);
   if(blend>.001f)Draw(sequence,second,at,size,angle,opacity*blend,ground,adamant);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   int now=Find.TickManager.TicksGame;var view=Find.CameraDriver.CurrentViewRect.ExpandedBy(8);
   for(int i=0;i<effects.Length;i++){
    var e=effects[i];if(!e.active)continue;int age=now-e.start;
    if(age>=e.duration){effects[i]=default(Effect);continue;}
    if(e.owner!=null&&(!e.owner.Spawned||e.owner.Map!=map||e.owner.Dead||e.owner.Downed||!EarthCombat.Busy(e.owner)||e.owner.CurJob.loadID!=e.ownerJob)){effects[i]=default(Effect);continue;}
    var at=e.owner!=null?e.owner.DrawPos:(e.sequence==1||e.adamant&&e.sequence==0)?Vector3.Lerp(e.from,e.to,Mathf.Clamp01(age/(float)e.duration)):e.from;
    if(e.owner!=null)e.angle=e.owner.Rotation.AsAngle-90f;
    var cell=at.ToIntVec3();if(!cell.InBounds(map)||!view.Contains(cell)||cell.Fogged(map))continue;
    at.y=AltitudeLayer.MoteOverhead.AltitudeFor();
    float fade=EarthRules.Smooth((e.duration-age)/6f);
    DrawPhase(e.sequence,e.telegraph?0:(e.adamant||e.sequence>=7)?EarthSeismicRules.FramePhase(age,e.duration):EarthRules.EffectPhase(e.sequence,age,e.duration),at,e.size,e.angle,(e.telegraph?.35f:.85f)*fade,e.adamant?e.sequence==3:e.sequence==7,false,e.adamant);
   }
   int drawn=0;
   foreach(var p in pawns){
    var cast=p==null||p.jobs==null?null:p.jobs.curDriver as JobDriver_EarthTechnique;
    if(cast!=null&&(cast.PendingMountainfall||cast.PendingQuake)){var cell=cast.TelegraphCell;if(cell.IsValid&&cell.InBounds(map)&&view.Contains(cell)&&!cell.Fogged(map)){var marker=cell.ToVector3Shifted();marker.y=AltitudeLayer.MoteOverhead.AltitudeFor();bool evolved=AdamantEvolution.Active(p);float size=cast.PendingQuake?(evolved?15f:13f):(evolved?11f:10f);DrawPhase(evolved?2:8,4f,marker,size,0,.15f+.12f*cast.TelegraphPhase,false,false,evolved);}}
    if(p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!view.Contains(p.Position)||p.Position.Fogged(map))continue;
    var c=HediffComp_EarthAbilities.On(p);if(c==null)continue;
    if(cast!=null&&cast.PendingCleave){var target=cast.TelegraphTarget;if(target!=null&&target.Spawned&&target.Map==map){var cut=p.DrawPos;cut.y=AltitudeLayer.MoteOverhead.AltitudeFor();DrawPhase(0,cast.CleaveWindup,cut,c.Evolved?4.2f:3.5f,(target.DrawPos-p.DrawPos).AngleFlat()-90f,.55f,false,false,c.Evolved);}}
    if(c.Evolved&&c.GuardActive){var shield=p.DrawPos;shield.y=AltitudeLayer.MoteOverhead.AltitudeFor();float guardPhase=Math.Min(7,(now-(c.guardUntil-180))/2f);DrawPhase(4,guardPhase,shield,3.6f,0,.5f*EarthRules.Smooth((c.guardUntil-now)/10f),false,false,true);}
    if(!c.Evolved&&c.GuardActive){var ring=p.DrawPos;ring.y=AltitudeLayer.MoteOverhead.AltitudeFor();float ramp=EarthRules.Smooth((now-(c.guardUntil-EarthRules.BastionDuration))/10f)*EarthRules.Smooth((c.guardUntil-now)/12f);DrawPhase(2,(now%30)/5f+(p.thingIDNumber%6),ring,6.3f,0,.45f*ramp,false,true);}
    if(c.Evolved){
     if(drawn++>=VisibleLimit)break;
     if(ElementalHeartsMod.Settings.showAura&&p.Drafted&&p.Awake()&&!p.InMentalState&&HediffComp_ElementalTransform.Active(p)&&!HediffComp_ElementalTransform.Transforming(p)){
      float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);var head=p.DrawPos+p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);head.z+=(.43f+.015f*Mathf.Sin(now*.05f))*life;head.y=AltitudeLayer.MoteOverhead.AltitudeFor();Draw(6,(now/6+p.thingIDNumber)%6,head,1.55f*life,0,.7f,false,true);
     }
     continue;
    }
    if(!ElementalHeartsMod.Settings.showAura||!c.Full||!p.Drafted||!HediffComp_ElementalTransform.Active(p)||HediffComp_ElementalTransform.Transforming(p))continue;
    if(drawn++>=VisibleLimit)break;
    var pos=p.DrawPos;pos.y-=.01f;pos.z-=.35f;
    float opacity=.28f+.1f*(float)Math.Sin(now*.045+p.thingIDNumber),phase=(now%30)/5f+(p.thingIDNumber%6);
    float activation=EarthRules.Smooth((c.activationUntil-now)/8f);
    float combat=EarthRules.CombatWeight(now,c.combatStarted,c.combatUntil);
    DrawPhase(4,phase,pos,2.65f,0,opacity*(1f-activation)*(1f-combat),true,true);
    DrawPhase(5,phase,pos,2.65f,0,opacity*(1f-activation)*combat,true,true);
    if(activation>0)DrawPhase(6,(30-(c.activationUntil-now))/5f,pos,2.65f,0,opacity*activation,true,false);
   }
  }
 }
}

