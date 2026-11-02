using System;using System.Collections.Generic;using RimWorld;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class AzureVfx {
  static readonly Material[,] materials=new Material[5,6];
  public static Material Material(int sequence,int frame){frame=Mathf.Clamp(frame,0,5);if(materials[sequence,frame]==null){string name=new[]{"AzureSlash","AzureDash","AzurePyre","AzureImpact","AzureHalo"}[sequence];materials[sequence,frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Azure/Frames/"+name+"_"+frame.ToString("00"),ShaderDatabase.Transparent);}return materials[sequence,frame];}
  public static void Track(Pawn p){if(p!=null&&p.Spawned)p.Map.GetComponent<MapComponent_AzureVfx>().Track(p);}
  public static void Emit(Pawn source,Map map,int sprite,Vector3 start,Vector3 end,int duration,float size,float angle=float.NaN){
   if(!AzureEvolution.Active(source)){FlameVfx.Add(map,sprite,start,end,duration,size,angle);return;}
   if(map==null||map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(!Find.CameraDriver.CurrentViewRect.ExpandedBy(6).Contains(start.ToIntVec3())&&!Find.CameraDriver.CurrentViewRect.ExpandedBy(6).Contains(end.ToIntVec3()))return;
   map.GetComponent<MapComponent_AzureVfx>().Add(Math.Min(4,sprite),start,end,duration,size,float.IsNaN(angle)?(end-start).AngleFlat():angle);
  }
 }
 public sealed class AzurePyre:IExposable {
  public Pawn source;public IntVec3 cell;public int due,pulses=3;public float damage;
  public void ExposeData(){Scribe_References.Look(ref source,"source");Scribe_Values.Look(ref cell,"cell");Scribe_Values.Look(ref due,"due");Scribe_Values.Look(ref pulses,"pulses",3);Scribe_Values.Look(ref damage,"damage");}
 }
 public sealed class MapComponent_AzureVfx:MapComponent {
  struct Effect{public bool active;public int sequence,startTick,duration;public Vector3 start,end;public float size,angle;}
  readonly Effect[] effects=new Effect[96];readonly List<Pawn> halos=new List<Pawn>();List<AzurePyre> pyres=new List<AzurePyre>();MaterialPropertyBlock block;
  public MapComponent_AzureVfx(Map map):base(map){}
  public void Track(Pawn p){if(p!=null&&!halos.Contains(p))halos.Add(p);}
  public void Add(int sequence,Vector3 start,Vector3 end,int duration,float size,float angle){int now=Find.TickManager.TicksGame;for(int i=0;i<effects.Length;i++)if(!effects[i].active||now>=effects[i].startTick+effects[i].duration){effects[i]=new Effect{active=true,sequence=sequence,start=start,end=end,startTick=now,duration=Math.Max(1,duration),size=size,angle=angle};return;}}
  public void QueuePyre(Pawn source,IntVec3 cell,float damage){pyres.Add(new AzurePyre{source=source,cell=cell,damage=damage,due=Find.TickManager.TicksGame});}
  public override void ExposeData(){base.ExposeData();Scribe_Collections.Look(ref pyres,"EH_azurePyres",LookMode.Deep);if(pyres==null)pyres=new List<AzurePyre>();}
  public override void MapComponentTick(){
   int now=Find.TickManager.TicksGame;
   if(now%60==0)for(int i=halos.Count-1;i>=0;i--)if(halos[i]==null||!halos[i].Spawned||halos[i].Map!=map||halos[i].Dead)halos.RemoveAt(i);
   for(int i=pyres.Count-1;i>=0;i--){var pyre=pyres[i];var source=pyre.source;if(source==null||!source.Spawned||source.Map!=map||source.Dead||source.Downed||!AzureEvolution.Active(source)||pyre.pulses<=0){pyres.RemoveAt(i);continue;}if(now<pyre.due)continue;
    foreach(var victim in FlameCombat.Targets(source,pyre.cell,AzureEvolution.Tuning.danceRadius)){
     var profile=FlameCombat.Profile(source,10);profile.damage=pyre.damage;
     FlameCombat.Strike(source,victim,profile,0,AzureEvolution.Tuning.danceThermal*AzureRules.PulseShare(3));
    }
    AzureVfx.Emit(source,map,2,pyre.cell.ToVector3Shifted(),pyre.cell.ToVector3Shifted(),60,AzureEvolution.Tuning.danceRadius*2);
    pyre.pulses--;pyre.due=now+60;if(pyre.pulses<=0)pyres.RemoveAt(i);
   }
  }
  public override void MapComponentDraw(){
   if(map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0)return;
   if(block==null)block=new MaterialPropertyBlock();var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(4);int now=Find.TickManager.TicksGame;
   for(int i=0;i<effects.Length;i++){var e=effects[i];if(!e.active)continue;float t=(now-e.startTick)/(float)e.duration;if(t>=1){effects[i].active=false;continue;}Vector3 pos=Vector3.Lerp(e.start,e.end,Mathf.Clamp01(t*2));if(!visible.Contains(pos.ToIntVec3())||pos.ToIntVec3().Fogged(map))continue;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,ElementalHeartsMod.Settings.effectOpacity*Mathf.Min(1,(1-t)*4)));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.AngleAxis(e.sequence<2?e.angle-90:0,Vector3.up),new Vector3(e.size,1,e.size)),AzureVfx.Material(e.sequence==4?3:e.sequence,AzureAnimationRules.Frame(e.sequence,t)),0,null,0,block);}
   if(!ElementalHeartsMod.Settings.showAura)return;
   foreach(var p in halos){if(p==null||!p.Spawned||p.Map!=map||p.Dead||p.Downed||!p.Awake()||!visible.Contains(p.Position)||p.Position.Fogged(map))continue;bool evolving=p.CurJob!=null&&p.CurJob.def.defName=="EH_EvolveAzure";if(!AzureHaloRules.Visible(AzureEvolution.Active(p),p.Drafted,HediffComp_ElementalTransform.Active(p),HediffComp_ElementalTransform.Transforming(p)))continue;
    float life=Mathf.Clamp(Mathf.Sqrt(p.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);Vector3 pos=p.DrawPos+p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);pos.z+=(.43f+.025f*Mathf.Sin(now*.06f))*life;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();float size=(evolving?1.5f:1.1f)*life;
    block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,ElementalHeartsMod.Settings.effectOpacity*.95f));Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(size,1,size)),AzureVfx.Material(4,(now/(evolving?3:5)+p.thingIDNumber)%6),0,null,0,block);
   }
  }
 }
}

namespace ElementalHearts {public static class AzureAnimationRules {public static int Frame(int sprite,float progress){return sprite==4?Math.Min(1,Math.Max(0,(int)(progress*2))):Math.Min(5,Math.Max(0,(int)(progress*6)));}}}

namespace ElementalHearts {public static class AzureHaloRules {public static bool Visible(bool evolved,bool drafted,bool transformed,bool changing){return evolved&&drafted&&transformed&&!changing;}}}
