using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 // Independent alpha sprite: no render-tree patch, atlas dependency or white shader mask.
 public sealed class MapComponent_TransformPulse:MapComponent {
  readonly List<Pawn> pawns=new List<Pawn>();
  static Material silhouette;
  MaterialPropertyBlock properties;
  public MapComponent_TransformPulse(Map map):base(map){}
  public void Track(Pawn p){if(!pawns.Contains(p))pawns.Add(p);}
  public static float Strength(int remaining){
   if(remaining<=0||remaining>90)return 0f;
   float t=(90-remaining)/90f;
   float fadeIn=Mathf.SmoothStep(0f,1f,Mathf.Clamp01(t/.18f));
   float fadeOut=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((1f-t)/.32f));
   return .18f*fadeIn*fadeOut;
  }
  public static Color ElementColor(string element){
   switch(element){case "Earth":return new Color(.85f,.57f,.2f);case "Celestial":return new Color(1f,.78f,.28f);case "Thunderstorm":return new Color(1f,.08f,.16f);case "AzureFlame":return new Color(.12f,.65f,1f);case "Lightning":return new Color(.12f,.65f,1f);case "Flame":return new Color(1f,.3f,.04f);case "Cyclone":return new Color(.25f,1f,.62f);case "Wind":return new Color(.45f,1f,.65f);default:return new Color(.2f,.85f,1f);}
  }
  public override void MapComponentTick(){
   for(int i=pawns.Count-1;i>=0;i--)if(!pawns[i].Spawned||pawns[i].Map!=map||pawns[i].Dead||!HediffComp_ElementalTransform.Transforming(pawns[i]))pawns.RemoveAt(i);
  }
  public override void MapComponentDraw(){
   if(pawns.Count==0||map!=Find.CurrentMap||Find.CameraDriver==null||ElementalHeartsMod.Settings.effectOpacity<=0f)return;
   if(silhouette==null)silhouette=MaterialPool.MatFrom("Effects/ElementalHearts/Shared/TransformSilhouette",ShaderDatabase.Transparent);
   if(properties==null)properties=new MaterialPropertyBlock();
   var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(2);
   foreach(var pawn in pawns){
    if(!pawn.Spawned||pawn.Map!=map||pawn.Dead||!visible.Contains(pawn.Position))continue;
    var c=HediffComp_ElementalTransform.On(pawn);if(c==null)continue;
    float strength=Strength(c.PulseRemaining);if(strength<=0f)continue;
    float growth=Mathf.Lerp(.98f,1.05f,strength);
    float lifeScale=Mathf.Clamp(Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);
    float width=1f;var body=pawn.story==null?null:pawn.story.bodyType;
    if(body==BodyTypeDefOf.Fat)width=1.25f;else if(body==BodyTypeDefOf.Hulk)width=1.2f;else if(body==BodyTypeDefOf.Thin)width=.86f;
    var pos=pawn.DrawPos;pos.y=AltitudeLayer.MoteOverhead.AltitudeFor();pos.z+=.18f*lifeScale;
    Color color=ElementColor(c.PulseElement);color.a=strength*Mathf.Clamp01(ElementalHeartsMod.Settings.effectOpacity);
    properties.SetColor(ShaderPropertyIDs.Color,color);
    var size=new Vector3(1.8f*width*lifeScale*growth,1f,1.8f*lifeScale*growth);
    Graphics.DrawMesh(MeshPool.plane10,Matrix4x4.TRS(pos,Quaternion.identity,size),silhouette,0,null,0,properties);
   }
  }
 }
}
