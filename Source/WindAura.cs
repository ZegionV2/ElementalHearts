using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace ElementalHearts {
 public sealed class MapComponent_WindVisuals:MapComponent {
  private readonly List<Pawn> pawns=new List<Pawn>();
  private MaterialPropertyBlock color;
  public MapComponent_WindVisuals(Map map):base(map){}
  public void Register(Pawn p){if(!pawns.Contains(p))pawns.Add(p);}
  public void Forget(Pawn p){pawns.Remove(p);}
  public override void FinalizeInit(){pawns.Clear();foreach(var p in map.mapPawns.AllPawnsSpawned)if(HediffComp_WindAbilities.On(p)!=null)Register(p);}
  public override void MapComponentTick(){if(Find.TickManager.TicksGame%250==0)pawns.RemoveAll(p=>p==null||!p.Spawned||p.Map!=map||HediffComp_WindAbilities.On(p)==null);}
  public override void MapComponentDraw(){
   var settings=ElementalHeartsMod.Settings;
   if(!settings.showAura||settings.effectOpacity<=0||map!=Find.CurrentMap||Find.CameraDriver==null)return;
   if(color==null)color=new MaterialPropertyBlock();
   var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(3);int now=Find.TickManager.TicksGame;
   foreach(var pawn in pawns){
    if(CycloneEvolution.Active(pawn)||!pawn.Drafted||!HediffComp_ElementalTransform.Active(pawn)||HediffComp_ElementalTransform.Transforming(pawn)||!pawn.Spawned||pawn.Map!=map||pawn.Dead||pawn.Downed||!visible.Contains(pawn.Position))continue;
    var comp=HediffComp_WindAbilities.On(pawn);if(comp==null)continue;
    int state=comp.activationUntil>now?2:comp.combatUntil>now?1:0;
    int frame=state==2?Mathf.Clamp((30-(comp.activationUntil-now))/5,0,5):(now/5+pawn.thingIDNumber)%6;
    Vector3 pos=pawn.DrawPos;pos.y-=.01f;pos.z-=.35f;
    color.SetColor("_Color",new Color(1,1,1,settings.effectOpacity*(.55f+.35f*Mathf.Sin(now*.045f+pawn.thingIDNumber))));
    Graphics.DrawMesh(WindVfx.FrameMesh(true),Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(2.65f,1,2.65f)),WindVfx.FrameMaterial(4+state,frame),0,null,0,color);
   }
  }
 }
}
