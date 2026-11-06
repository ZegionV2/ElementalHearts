using UnityEngine;
using Verse;
namespace ElementalHearts {
 // One atlas and one six-quad mesh submission per visible pawn. No spawned things.
 public sealed class EarthRockOrbit {
  public const int VisibleLimit=64,StonesPerPawn=6;
  static Material material;
  readonly Mesh[] meshes=new Mesh[VisibleLimit];
  readonly Vector3[][] vertices=new Vector3[VisibleLimit][];
  readonly Vector2[][] uvs=new Vector2[VisibleLimit][];
  readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
  static readonly int[] triangles=Triangles();
  static int[] Triangles(){var result=new int[StonesPerPawn*6];for(int i=0;i<StonesPerPawn;i++){int v=i*4,t=i*6;result[t]=v;result[t+1]=v+1;result[t+2]=v+2;result[t+3]=v;result[t+4]=v+2;result[t+5]=v+3;}return result;}
  public void Release(){for(int i=0;i<meshes.Length;i++)if(meshes[i]!=null){Object.Destroy(meshes[i]);meshes[i]=null;vertices[i]=null;uvs[i]=null;}}
  public static Vector3 Offset(int tick,int pawnId,int stone){float a=tick*.017f+pawnId*.075f+stone*Mathf.PI*2/StonesPerPawn;return new Vector3(Mathf.Cos(a)*.72f,Mathf.Sin(a),.18f+Mathf.Sin(a)*.3f+.055f*Mathf.Sin(a*2+tick*.035f));}
  public void Draw(Pawn pawn,int tick,int slot,float opacity){
   if(slot<0||slot>=VisibleLimit)return;
   if(material==null)material=MaterialPool.MatFrom("Effects/ElementalHearts/Earth/Orbit/OrbitAtlas",ShaderDatabase.Transparent);
   bool first=meshes[slot]==null;
   if(first){meshes[slot]=new Mesh{name="EH six floating stones"};meshes[slot].MarkDynamic();vertices[slot]=new Vector3[StonesPerPawn*4];uvs[slot]=new Vector2[StonesPerPawn*4];}
   var v=vertices[slot];var uv=uvs[slot];float life=Mathf.Clamp(Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor),.55f,1.25f);
   for(int i=0;i<StonesPerPawn;i++){
    var offset=Offset(tick,pawn.thingIDNumber,i);float side=offset.y;offset.y=side>0?pawn.DrawPos.y-.02f:AltitudeLayer.MoteOverhead.AltitudeFor();
    float size=.39f*(1-.18f*side),tilt=.18f*Mathf.Sin(tick*.022f+i);float c=Mathf.Cos(tilt)*size*.5f,s=Mathf.Sin(tilt)*size*.5f;int n=i*4;
    v[n]=offset+new Vector3(-c-s,0,-c+s);v[n+1]=offset+new Vector3(-c+s,0,c+s);v[n+2]=offset+new Vector3(c+s,0,c-s);v[n+3]=offset+new Vector3(c-s,0,-c-s);
    int tile=(i+(pawn.thingIDNumber&7))%8;float left=(tile%4)/4f+.0005f,right=left+.249f,top=1-(tile/4)/2f-.001f,bottom=top-.498f;
    uv[n]=new Vector2(left,bottom);uv[n+1]=new Vector2(left,top);uv[n+2]=new Vector2(right,top);uv[n+3]=new Vector2(right,bottom);
   }
   var mesh=meshes[slot];mesh.vertices=v;mesh.uv=uv;if(first)mesh.triangles=triangles;mesh.RecalculateBounds();
   properties.SetColor(ShaderPropertyIDs.Color,new Color(1,1,1,opacity));var at=pawn.DrawPos;at.y=0;
   Graphics.DrawMesh(mesh,Matrix4x4.TRS(at,Quaternion.identity,new Vector3(life,1,life)),material,0,null,0,properties);
  }
 }
}
