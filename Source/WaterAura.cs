using System.Collections.Generic;using UnityEngine;using Verse;
namespace ElementalHearts {
 public static class WaterAuraRules {
  public static int Frame(int tick,int pawnId){return ((tick/5+pawnId)%6+6)%6;}
  public static float Opacity(int tick,int pawnId){return .22f+.05f*Mathf.Sin(tick*.025f+pawnId);}
  public static bool Visible(bool ascended,bool drafted,bool transformed,bool transforming,bool alive,bool standing){return ascended&&drafted&&transformed&&!transforming&&alive&&standing;}
 }
 [StaticConstructorOnStartup]
 public sealed class MapComponent_WaterAura:MapComponent {
  readonly List<Pawn> pawns=new List<Pawn>();static readonly Material[] frames=new Material[6];MaterialPropertyBlock block;
  public MapComponent_WaterAura(Map map):base(map){}
  public void Register(Pawn pawn){if(WaterKitUpgrade.HasAscension(pawn)&&!pawns.Contains(pawn))pawns.Add(pawn);}
  public override void FinalizeInit(){pawns.Clear();foreach(var pawn in map.mapPawns.AllPawnsSpawned)Register(pawn);}
  public override void MapComponentTick(){if(Find.TickManager.TicksGame%250==0)pawns.RemoveAll(p=>p==null||!p.Spawned||p.Map!=map||!WaterKitUpgrade.HasAscension(p));}
  public override void MapComponentDraw(){
   var settings=ElementalHeartsMod.Settings;if(!settings.showAura||settings.effectOpacity<=0||map!=Find.CurrentMap||Find.CameraDriver==null)return;
   if(block==null)block=new MaterialPropertyBlock();var visible=Find.CameraDriver.CurrentViewRect.ExpandedBy(3);int now=Find.TickManager.TicksGame;
   foreach(var pawn in pawns){
    if(!pawn.Spawned||pawn.Map!=map||!WaterAuraRules.Visible(WaterKitUpgrade.HasAscension(pawn),pawn.Drafted,HediffComp_ElementalTransform.Active(pawn),HediffComp_ElementalTransform.Transforming(pawn),!pawn.Dead,!pawn.Downed)||!visible.Contains(pawn.Position)||pawn.Position.Fogged(map))continue;
    int frame=WaterAuraRules.Frame(now,pawn.thingIDNumber);if(frames[frame]==null)frames[frame]=MaterialPool.MatFrom("Effects/ElementalHearts/Water/Frames/Aura_"+frame.ToString("00"),ShaderDatabase.Transparent);
    var pos=pawn.DrawPos;pos.y-=.01f;pos.z-=.35f;
    block.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,settings.effectOpacity*WaterAuraRules.Opacity(now,pawn.thingIDNumber)));
    Graphics.DrawMesh(LightningVfx.FrameMesh(true),Matrix4x4.TRS(pos,Quaternion.identity,new Vector3(2.25f,1,2.25f)),frames[frame],0,null,0,block);
   }
  }
 }
}
