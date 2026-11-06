using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public static class AzureVfx {
  static readonly string[] names={"AzureCrescent","BlueComet","ControlledPyre","AzurePhoenix","PhoenixSeal","ReversalTrail","ReversalBurst"};
  static readonly Material[,] materials=new Material[7,8],bodies=new Material[7,3];
  static readonly Material[] halo=new Material[6];
  static readonly string[] bodyNames={"Male","Female","Thin","Fat","Hulk","Child","Baby"},directions={"south","east","north"};
  // Keep the old Material(4, frame) halo contract. Telegraphs use EffectMaterial.
  public static Material Material(int sequence,int frame){return sequence==4?Halo(frame):EffectMaterial(sequence,frame);}
  public static Material EffectMaterial(int sequence,int frame){sequence=Mathf.Clamp(sequence,0,6);frame=Mathf.Clamp(frame,0,7);return materials[sequence,frame]??(materials[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Azure/Polish/"+names[sequence]+"_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  public static Material Halo(int frame){frame=Mathf.Clamp(frame,0,5);return halo[frame]??(halo[frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Azure/Frames/AzureHalo_"+frame.ToString("00"),ShaderDatabase.Transparent));}
  static Material Body(Pawn p){if(CourtAppearance.WearingKit(p))return CourtAppearance.Body(p);int body=Array.IndexOf(bodyNames,p.story==null||p.story.bodyType==null?"Male":p.story.bodyType.defName);if(body<0)body=0;int facing=p.Rotation==Rot4.South?0:p.Rotation==Rot4.North?2:1;return bodies[body,facing]??(bodies[body,facing]=MaterialPool.MatFrom("Things/Pawn/Humanlike/Apparel/AzureFlameRegalia/AzureFlameRegalia_"+bodyNames[body]+"_"+directions[facing],ShaderDatabase.Transparent));}
  public static void Track(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_AzureVfx>().Track(p);}
  public static void Ghost(Pawn p,Vector3 at){if(p==null||!p.Spawned||!ElementalHeartsMod.Settings.showAfterimages||!StormBlitzFx.Visible(p.Map,at,at))return;p.Map.GetComponent<MapComponent_AzureVfx>().Ghost(p,at,Body(p));}
  public static void Emit(Pawn source,Map map,int sprite,Vector3 start,Vector3 end,int duration,float size,float angle=float.NaN){
   if(!AzureEvolution.Active(source)){FlameVfx.Add(map,sprite,start,end,duration,size,angle);return;}
   if(map==null||map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0||!StormBlitzFx.Visible(map,start,end))return;
   map.GetComponent<MapComponent_AzureVfx>().Add(Mathf.Clamp(sprite,0,6),start,end,duration,size,float.IsNaN(angle)?(end-start).AngleFlat():angle);
  }
 }
 public sealed class AzurePyre:IExposable {
  public Pawn source;public IntVec3 cell;public int due,pulses=3;public float damage;
  public void ExposeData(){Scribe_References.Look(ref source,"source");Scribe_Values.Look(ref cell,"cell");Scribe_Values.Look(ref due,"due");Scribe_Values.Look(ref pulses,"pulses",3);Scribe_Values.Look(ref damage,"damage");}
 }
 public sealed class MapComponent_AzureVfx:MapComponent {
  struct Effect{public bool active;public int sequence,startTick,duration;public Vector3 start,end;public float size,angle;}
  struct Echo{public bool active,west;public int start;public Vector3 at;public float width,height;public Material material;}
  readonly Effect[] effects=new Effect[96];readonly Echo[] echoes=new Echo[32];readonly List<Pawn> halos=new List<Pawn>();
  List<AzurePyre> pyres=new List<AzurePyre>();MaterialPropertyBlock block;int echoCursor,haloCursor;
  public MapComponent_AzureVfx(Map map):base(map){}
  public void Track(Pawn p){if(p==null||halos.Contains(p))return;if(halos.Count<128)halos.Add(p);else{halos[haloCursor]=p;haloCursor=(haloCursor+1)%128;}}
  public void Ghost(Pawn p,Vector3 at,Material material){float w=HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn(p),h=w;RegalSideFit.Echo(p,"EH_AzureFlameUniform",ref at,ref w,ref h);echoes[echoCursor]=new Echo{active=true,west=p.Rotation==Rot4.West,start=Find.TickManager.TicksGame,at=at,width=w,height=h,material=material};echoCursor=(echoCursor+1)%echoes.Length;}
  public void Add(int sequence,Vector3 start,Vector3 end,int duration,float size,float angle){int now=Find.TickManager.TicksGame;for(int i=0;i<effects.Length;i++)if(!effects[i].active||now>=effects[i].startTick+effects[i].duration){effects[i]=new Effect{active=true,sequence=Mathf.Clamp(sequence,0,6),start=start,end=end,startTick=now,duration=Math.Max(1,duration),size=size,angle=angle};return;}}
  public void QueuePyre(Pawn source,IntVec3 cell,float damage){pyres.Add(new AzurePyre{source=source,cell=cell,damage=damage,due=Find.TickManager.TicksGame});}
  // Existing gameplay/save contract: exactly the same three scheduled damage pulses.
  public override void ExposeData(){base.ExposeData();Scribe_Collections.Look(ref pyres,"EH_azurePyres",LookMode.Deep);if(pyres==null)pyres=new List<AzurePyre>();}
  public override void MapComponentTick(){
   int now=Find.TickManager.TicksGame;
   if(now%60==0)for(int i=halos.Count-1;i>=0;i--)if(halos[i]==null||!halos[i].Spawned||halos[i].Map!=map||halos[i].Dead)halos.RemoveAt(i);
   for(int i=pyres.Count-1;i>=0;i--){var pyre=pyres[i];var source=pyre.source;if(source==null||!source.Spawned||source.Map!=map||source.Dead||source.Downed||!AzureEvolution.Active(source)||pyre.pulses<=0){pyres.RemoveAt(i);continue;}if(now<pyre.due)continue;
    foreach(var victim in FlameCombat.Targets(source,pyre.cell,AzureEvolution.Tuning.danceRadius)){var profile=FlameCombat.Profile(source,10);profile.damage=pyre.damage;FlameCombat.Strike(source,victim,profile,0,AzureEvolution.Tuning.danceThermal*AzureRules.PulseShare(3));}
    AzureVfx.Emit(source,map,2,pyre.cell.ToVector3Shifted(),pyre.cell.ToVector3Shifted(),60,AzureEvolution.Tuning.danceRadius*2);
    pyre.pulses--;pyre.due=now+60;if(pyre.pulses<=0)pyres.RemoveAt(i);
   }
  }
  bool Visible(CellRect view,Vector3 pos){var cell=pos.ToIntVec3();return cell.InBounds(map)&&view.Contains(cell)&&!cell.Fogged(map);}
  void Draw(Effect e,Vector3 pos,int frame,float alpha){
   bool floor=e.sequence==2||e.sequence==4;pos.y=(floor?AltitudeLayer.MoteLow:AltitudeLayer.MoteOverhead).AltitudeFor();
   float width=e.size,height=e.size,angle=0;bool raised=false;
   if(e.sequence==0)angle=e.angle-90;
   else if(e.sequence==1||e.sequence==5){angle=e.angle-90;width*=1.6f;height*=.65f;}
   else if(e.sequence==3){height*=1.55f;raised=true;}
   if(e.sequence==4)alpha*=.55f;
   block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,alpha));Graphics.DrawMesh(FlameVfx.FrameMesh(raised),Matrix4x4.TRS(pos,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(width,1,height)),AzureVfx.EffectMaterial(e.sequence,frame),0,null,0,block);
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(block==null)block=new MaterialPropertyBlock();var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(8);int now=Find.TickManager.TicksGame;float opacity=ElementalHeartsMod.Settings.effectOpacity;
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!e.active)continue;int age=now-e.startTick;if(age<0)continue;if(age>=e.duration){effects[i].active=false;continue;}
    float t=age/(float)e.duration;Vector3 pos=Vector3.Lerp(e.start,e.end,t);if(!Visible(visible,pos))continue;
    float phase=e.sequence==2?t*8:Mathf.Clamp01(age/(float)Math.Max(1,e.duration-1))*7;int first=Mathf.Min(7,(int)phase),second=e.sequence==2?(first+1)%8:Mathf.Min(7,first+1);float blend=Mathf.SmoothStep(0,1,phase-first),alpha=opacity*Mathf.Min(1,(1-t)*5);
    if(blend<.999f)Draw(e,pos,first,alpha*(1-blend));if(blend>.001f&&second!=first)Draw(e,pos,second,alpha*blend);
   }
   for(int i=0;i<echoes.Length;i++){var e=echoes[i];if(!e.active)continue;float t=(now-e.start)/12f;if(t>=1){echoes[i]=new Echo();continue;}if(t<0||!Visible(visible,e.at))continue;var pos=e.at;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(.35f,.8f,1,.3f*(1-t)*(1-t)*opacity));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(e.west?-e.width:e.width,1,e.height)),e.material,0,null,0,block);}
   if(!ElementalHeartsMod.Settings.showAura)return;int drawn=0;
   foreach(var p in halos){if(drawn>=64)break;if(p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!p.Awake()||!Visible(visible,p.DrawPos))continue;if(!AzureHaloRules.Visible(AzureEvolution.Active(p),CourtHeralds.HaloDrafted(p),HediffComp_ElementalTransform.Active(p),HediffComp_ElementalTransform.Transforming(p)))continue;drawn++;
    float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);Vector3 pos=p.DrawPos+p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);pos.z+=(.43f+.025f*Mathf.Sin(now*.06f))*life;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();
    block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,opacity*.95f));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(1.1f*life,1,1.1f*life)),AzureVfx.Halo((now/5+p.thingIDNumber)%6),0,null,0,block);
   }
  }
 }
 public static class AzureAnimationRules {public const int Frames=8;public static int Frame(int sprite,float progress){return Math.Min(Frames-1,Math.Max(0,(int)(Mathf.Clamp01(progress)*Frames)));}}
 public static class AzureHaloRules {public static bool Visible(bool evolved,bool drafted,bool transformed,bool changing){return evolved&&drafted&&transformed&&!changing;}}
}
