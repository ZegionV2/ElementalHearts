using UnityEngine;
using Verse;
namespace ElementalHearts {
 [StaticConstructorOnStartup]

 public static class FlameAura {
  private static MaterialPropertyBlock color;
  public static void Draw(Pawn pawn,int state) {
   if(AzureEvolution.Active(pawn))return;
   if(pawn==null||!pawn.Drafted||!HediffComp_ElementalTransform.Active(pawn)||HediffComp_ElementalTransform.Transforming(pawn))return;
   if(color==null)color=new MaterialPropertyBlock();
   int frame=(Find.TickManager.TicksGame/5+pawn.thingIDNumber)%6;
   if(state==2){var comp=HediffComp_FlamePassives.On(pawn);frame=Mathf.Clamp((30-comp.ActivationRemaining)/5,0,5);}
   Vector3 pos=pawn.DrawPos;pos.y-=.01f;pos.z-=.35f;
   color.SetColor("_Color",new Color(1,1,1,ElementalHeartsMod.Settings.effectOpacity*(.55f+.35f*Mathf.Sin(Find.TickManager.TicksGame*.045f+ pawn.thingIDNumber))));
   Graphics.DrawMesh(FlameVfx.FrameMesh(true),Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(2.65f,1,2.65f)),FlameVfx.FrameMaterial(4+state,frame),0,null,0,color);
  }
 }
}





