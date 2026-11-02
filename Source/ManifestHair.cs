using RimWorld;using Verse;using HarmonyLib;
namespace ElementalHearts {
 public static class ManifestHairRules {
  public static bool Conceals(ApparelProperties apparel,bool beard){
   if(apparel==null)return false;
   if(apparel.renderSkipFlags!=null)return apparel.renderSkipFlags.Contains(beard?RenderSkipFlagDefOf.Beard:RenderSkipFlagDefOf.Hair);
   return apparel.bodyPartGroups!=null&&(apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead)||!beard&&apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead));
  }
  public static bool MayUnmask(bool changing,bool portrait,bool statue,bool headgearVisible,bool differentEndpoints,bool incomingHidden,bool otherApparelHidden){return changing&&!portrait&&!statue&&headgearVisible&&differentEndpoints&&!incomingHidden&&!otherApparelHidden;}
  public static bool IsStyle(PawnRenderNode node,out bool beard){beard=node is PawnRenderNode_Beard||node!=null&&node.Props.skipFlag==RenderSkipFlagDefOf.Beard;return beard||node is PawnRenderNode_Hair||node!=null&&node.Props.skipFlag==RenderSkipFlagDefOf.Hair;}
 }
 // Let native workers generate eligible style requests during a headwear transition.
 // Only the native worn-apparel style mask is relaxed; caller masks and unrelated headwear remain authoritative.
 [HarmonyPatch(typeof(PawnRenderTree),"AdjustParms")]
 public static class ElementalManifestStyleMask {
  public static void Prefix(PawnDrawParms parms,out ulong __state){__state=parms.skipFlags;}
  public static void Postfix(Pawn ___pawn,ref PawnDrawParms parms,ulong __state){
   var c=HediffComp_ElementalTransform.On(___pawn);if(c==null||!HediffComp_ElementalTransform.Transforming(___pawn)||parms.Portrait||parms.Statue||!PawnRenderNodeWorker_Apparel_Head.HeadgearVisible(parms))return;
   for(int i=0;i<2;i++){bool beard=i==1;ulong flag=beard?RenderSkipFlagDefOf.Beard:RenderSkipFlagDefOf.Hair;bool kitHides;bool transition=c.StyleTransition(beard,out kitHides);if(ManifestHairRules.MayUnmask(true,false,false,true,transition,(__state&flag)!=0,c.OtherApparelConceals(beard)))parms.skipFlags&=~flag;}
  }
 }
}
