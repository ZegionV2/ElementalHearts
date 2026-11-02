using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 // Build visual-only apparel through the native factory; inventory/equipment ownership never changes here.
 public sealed class DynamicPawnRenderNodeSetup_ElementalManifest:DynamicPawnRenderNodeSetup {
  internal static readonly ConditionalWeakTable<PawnRenderNode,object> previews=new ConditionalWeakTable<PawnRenderNode,object>();
  static readonly MethodInfo factory=AccessTools.Method(typeof(DynamicPawnRenderNodeSetup_Apparel),"ProcessApparel");
  public override bool HumanlikeOnly{get{return true;}}
  public override List<Type> SetupAfter{get{return new List<Type>{typeof(DynamicPawnRenderNodeSetup_Apparel)};}}
  public override IEnumerable<ValueTuple<PawnRenderNode,PawnRenderNode>> GetDynamicNodes(Pawn pawn,PawnRenderTree tree){
   var c=HediffComp_ElementalTransform.On(pawn);if(c==null||!HediffComp_ElementalTransform.Transforming(pawn)||factory==null)yield break;
   PawnRenderNode head,body;tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelHead,out head);tree.TryGetNodeByTag(PawnRenderNodeTagDefOf.ApparelBody,out body);
   var offsets=new Dictionary<PawnRenderNode,int>();
   foreach(var apparel in c.VisualApparel()){
    var nodes=(IEnumerable<ValueTuple<PawnRenderNode,PawnRenderNode>>)factory.Invoke(null,new object[]{pawn,tree,apparel,head,body,offsets});
    foreach(var pair in nodes){if(pair.Item2!=null){int n;offsets.TryGetValue(pair.Item2,out n);offsets[pair.Item2]=n+1;}previews.GetOrCreateValue(pair.Item1);yield return pair;}
   }
  }
 }
 [HarmonyPatch(typeof(PawnRenderer),"ParallelGetPreRenderResults")]
 public static class ElementalManifestUncached {
  public static void Prefix(Pawn ___pawn,ref bool disableCache){if(HediffComp_ElementalTransform.Transforming(___pawn))disableCache=true;}
 }
 [HarmonyPatch(typeof(PawnRenderTree),"Draw")]
 public static class ElementalManifestDraw {
  public static void Prefix(PawnDrawParms parms,List<PawnGraphicDrawRequest> ___drawRequests,out PawnGraphicDrawRequest[] __state){
   __state=null;var c=HediffComp_ElementalTransform.On(parms.pawn);if(c==null||!HediffComp_ElementalTransform.Transforming(parms.pawn) )return;
   __state=___drawRequests.ToArray();
   for(int i=0;i<___drawRequests.Count;i++){
    var r=___drawRequests[i];
    if(parms.Portrait||parms.Statue){object marker;if(r.node!=null&&DynamicPawnRenderNodeSetup_ElementalManifest.previews.TryGetValue(r.node,out marker)){r.material=null;___drawRequests[i]=r;}continue;}
    if(r.node==null||r.mesh==null)continue;
    bool beard,kitHides;if(ManifestHairRules.IsStyle(r.node,out beard)){
     if(!PawnRenderNodeWorker_Apparel_Head.HeadgearVisible(parms)||!c.StyleTransition(beard,out kitHides))continue;
     float styleReveal=ManifestMeshRules.PartCoverage(c.Coverage,true);
     if(kitHides?styleReveal>=1f:styleReveal<=0f){r.material=null;___drawRequests[i]=r;continue;}
     if(kitHides?styleReveal<=0f:styleReveal>=1f)continue;
     var style=new PawnGraphicDrawRequest(r.node,ElementalManifestMeshes.Clip(r.mesh,styleReveal,kitHides),r.material);style.preDrawnComputedMatrix=r.preDrawnComputedMatrix;___drawRequests[i]=style;continue;
    }
    if(r.node.apparel==null)continue;
    bool kit=c.IsKit(r.node.apparel);float coverage=c.Coverage;
    bool head=r.node.apparel.def.apparel.LastLayer==ApparelLayerDefOf.Overhead;
    float reveal=ManifestMeshRules.PartCoverage(coverage,head);
    if(kit?reveal<=0f:reveal>=1f){r.material=null;___drawRequests[i]=r;continue;}
    if(kit?reveal>=1f:reveal<=0f)continue;
    var changed=new PawnGraphicDrawRequest(r.node,ElementalManifestMeshes.Clip(r.mesh,reveal,!kit),r.material);changed.preDrawnComputedMatrix=r.preDrawnComputedMatrix;___drawRequests[i]=changed;
   }
  }
  static void Restore(List<PawnGraphicDrawRequest> requests,PawnGraphicDrawRequest[] state){if(state==null)return;requests.Clear();requests.AddRange(state);}
  public static void Postfix(List<PawnGraphicDrawRequest> ___drawRequests,PawnGraphicDrawRequest[] __state){Restore(___drawRequests,__state);}
  public static Exception Finalizer(Exception __exception,List<PawnGraphicDrawRequest> ___drawRequests,PawnGraphicDrawRequest[] __state){if(__exception!=null)Restore(___drawRequests,__state);return __exception;}
 }
 public static class ManifestMeshRules {
  public static float PartCoverage(float coverage,bool head){if(coverage<=0f)return 0f;if(coverage>=1f)return 1f;return Math.Max(0f,Math.Min(1f,head?(coverage-.4f)/.6f:coverage/.72f));}
 }
 static class ElementalManifestMeshes {
  struct Vertex {public Vector3 pos;public Vector2 uv;public Vertex(Vector3 p,Vector2 t){pos=p;uv=t;}}
  static readonly Dictionary<Mesh,Dictionary<int,Mesh>> cache=new Dictionary<Mesh,Dictionary<int,Mesh>>();
  public static Mesh Clip(Mesh source,float amount,bool upper){
   int step=Mathf.Clamp(Mathf.RoundToInt(amount*64f),1,63),key=step+(upper?65:0);Dictionary<int,Mesh> variants;Mesh result;
   if(!cache.TryGetValue(source,out variants)){variants=new Dictionary<int,Mesh>();cache.Add(source,variants);}
   if(variants.TryGetValue(key,out result))return result;
   var vertices=source.vertices;var uv=source.uv;var indices=source.triangles;
   float low=float.MaxValue,high=float.MinValue;foreach(var v in vertices){low=Math.Min(low,v.z);high=Math.Max(high,v.z);}float cut=Mathf.Lerp(low,high,Mathf.Lerp(.1f,.85f,step/64f));
   var positions=new List<Vector3>();var texcoords=new List<Vector2>();var triangles=new List<int>();
   for(int i=0;i<indices.Length;i+=3){
    var polygon=new List<Vertex>(4);for(int k=0;k<3;k++){int index=indices[i+k];polygon.Add(new Vertex(vertices[index],uv.Length>index?uv[index]:Vector2.zero));}
    var clipped=new List<Vertex>(4);
    for(int k=0;k<polygon.Count;k++){var a=polygon[k];var b=polygon[(k+1)%polygon.Count];bool inside=upper?a.pos.z>=cut:a.pos.z<=cut,next=upper?b.pos.z>=cut:b.pos.z<=cut;
     if(inside)clipped.Add(a);if(inside!=next){float t=(cut-a.pos.z)/(b.pos.z-a.pos.z);clipped.Add(new Vertex(Vector3.Lerp(a.pos,b.pos,t),Vector2.Lerp(a.uv,b.uv,t)));}}
    int start=positions.Count;foreach(var v in clipped){positions.Add(v.pos);texcoords.Add(v.uv);}for(int k=1;k+1<clipped.Count;k++){triangles.Add(start);triangles.Add(start+k);triangles.Add(start+k+1);}
   }
   result=new Mesh();result.name="EH_Manifest_"+key;result.SetVertices(positions);result.SetUVs(0,texcoords);result.SetTriangles(triangles,0);result.RecalculateNormals();result.RecalculateBounds();variants.Add(key,result);return result;
  }
 }
}
