using UnityEngine;
using Verse;

namespace ElementalHearts
{
    [StaticConstructorOnStartup]
    public static class WindVfx
    {
        private static Texture2D[] icons=new Texture2D[4]; public static Texture2D Icon(int i){if(icons[i]==null){string[] names={"DustCuttingDraw","RisingGale","TurningSwallow","MountainScouringTempest"};icons[i]=ContentFinder<Texture2D>.Get("UI/Abilities/ElementalHearts/Wind/"+names[i]);}return icons[i];}
        private static Material[,] animationMaterials=new Material[7,6];
        private static Mesh centered, grounded;
        public static Material FrameMaterial(int sequence,int frame) {
            frame=Mathf.Clamp(frame,0,5);
            if(animationMaterials[sequence,frame]==null) {
                string[] names={"DustCuttingDraw","RisingGale","TurningSwallow","MountainScouringTempest","Idle","Combat","Activation"};
                var texture=ContentFinder<Texture2D>.Get("Effects/ElementalHearts/Wind/Frames/"+names[sequence]+"_"+frame.ToString("00"));
                animationMaterials[sequence,frame]=MaterialPool.MatFrom(texture,ShaderDatabase.Transparent,Color.white);
            }
            return animationMaterials[sequence,frame];
        }
        public static Mesh FrameMesh(bool ground) {
            if(centered==null) {centered=Quad(0f);grounded=Quad(.38f);}
            return ground?grounded:centered;
        }
        private static Mesh Quad(float offset) {
            var m=new Mesh();m.vertices=new[]{new Vector3(-.5f,0,-.5f+offset),new Vector3(-.5f,0,.5f+offset),new Vector3(.5f,0,.5f+offset),new Vector3(.5f,0,-.5f+offset)};
            m.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};m.triangles=new[]{0,1,2,0,2,3};m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        public static void Add(Map map, int sprite, Vector3 start, Vector3 end, int duration, float size, float angle = float.NaN)
        {
            if (ElementalHeartsMod.Settings.effectOpacity<=0f || map != Find.CurrentMap || Find.CameraDriver == null) return;
            CellRect visible = Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
            if (!visible.Contains(start.ToIntVec3()) && !visible.Contains(end.ToIntVec3())) return;
            map.GetComponent<MapComponent_WindVfx>().Add(sprite, start, end, duration, size, angle);
        }
        public static void Afterimage(Pawn pawn, Vector3 position)
        {
            if (!ElementalHeartsMod.Settings.showAfterimages || ElementalHeartsMod.Settings.effectOpacity<=0f || !pawn.Spawned || pawn.Map != Find.CurrentMap || Find.CameraDriver == null ||
                !Find.CameraDriver.CurrentViewRect.ExpandedBy(2).Contains(pawn.Position)) return;
            PawnTextureAtlasFrameSet frame; bool created;
            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frame, out created, false) && !created)
            {
                int index = frame.GetIndex(pawn.Rotation, PawnDrawMode.BodyAndHead);
                if (!frame.isDirty[index])
                {
                    Material mat = MaterialPool.MatFrom(new MaterialRequest(frame.atlas, ShaderDatabase.Transparent));
                    pawn.Map.GetComponent<MapComponent_WindVfx>().AddGhost(frame.meshes[index], mat, position);
                    return;
                }
            }
            // A pawn not yet drawn has no valid cache frame. Never force a render/readback.
            Add(pawn.Map, 1, position, position, 12, 1.4f);
        }
    }

    public sealed class MapComponent_WindVfx : MapComponent
    {
        private struct Effect
        {
            public bool active;
            public int sprite, startTick, duration;
            public Vector3 start, end;
            public float size, angle;
            public Mesh ghostMesh;
            public Material ghostMaterial;
        }
        // Fixed slots are reused. No effect GameObjects, target scans or saved visual state.
        private readonly Effect[] pool = new Effect[96];
        private MaterialPropertyBlock ghostColor;
        public MapComponent_WindVfx(Map map) : base(map) { }
        public void AddGhost(Mesh mesh, Material material, Vector3 position)
        {
            int now = Find.TickManager.TicksGame;
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i].active && now < pool[i].startTick + pool[i].duration) continue;
                pool[i] = new Effect { active = true, sprite = -1, ghostMesh = mesh, ghostMaterial = material,
                    start = position, end = position, startTick = now, duration = 12, size = 1f };
                return;
            }
        }
        public void Add(int sprite, Vector3 start, Vector3 end, int duration, float size, float angle)
        {
            int now = Find.TickManager.TicksGame;
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i].active && now < pool[i].startTick + pool[i].duration) continue;
                pool[i] = new Effect { active = true, sprite = sprite, start = start, end = end,
                    startTick = now, duration = System.Math.Max(1, duration), size = size,
                    angle = float.IsNaN(angle) ? (end - start).AngleFlat() : angle };
                return;
            }
            // Pool full: omit cosmetics only. Gameplay has already happened independently.
        }
        public override void MapComponentDraw()
        {
            if (ElementalHeartsMod.Settings.effectOpacity<=0f || map != Find.CurrentMap || Find.CameraDriver == null) return;
            int now = Find.TickManager.TicksGame;
            CellRect visible = Find.CameraDriver.CurrentViewRect.ExpandedBy(6);
            for (int i = 0; i < pool.Length; i++)
            {
                Effect e = pool[i]; if (!e.active) continue;
                float t = (now - e.startTick) / (float)e.duration;
                if (t >= 1f) { pool[i].active = false; continue; }
                Vector3 position = Vector3.Lerp(e.start, e.end, Mathf.Clamp01(t * 2f));
                if (!visible.Contains(position.ToIntVec3())) continue;
                position.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                if (e.sprite == -1)
                {
                    if (e.ghostMesh != null && e.ghostMaterial != null)
                    {
                        if (ghostColor == null) ghostColor = new MaterialPropertyBlock();
                        ghostColor.SetColor("_Color", new Color(0.7f, 1f, 0.85f, 0.45f * (1f - t) * ElementalHeartsMod.Settings.effectOpacity));
                        Graphics.DrawMesh(e.ghostMesh, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one), e.ghostMaterial, 0, null, 0, ghostColor);
                    }
                    continue;
                }
                int sequence=e.sprite;
                int frame=Mathf.Min(5,(int)(t*6));
                float angle=e.sprite==0?e.angle-90f:0f;
                Matrix4x4 matrix=Matrix4x4.TRS(position,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(e.size,1f,e.size));
                if(ghostColor==null)ghostColor=new MaterialPropertyBlock();
                ghostColor.SetColor("_Color",new Color(1f,1f,1f,ElementalHeartsMod.Settings.effectOpacity));
                Graphics.DrawMesh(WindVfx.FrameMesh(false),matrix,WindVfx.FrameMaterial(sequence,frame),0,null,0,ghostColor);
            }
        }
    }
}



