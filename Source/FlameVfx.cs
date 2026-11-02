using UnityEngine;

using Verse;



namespace ElementalHearts

{

    [StaticConstructorOnStartup]

    public static class FlameVfx

    {

        private static Texture2D atlas;

        private static Mesh[] meshes;

        private static Material[] fades;

        public static Texture2D Atlas

        {

            get { if (atlas == null) atlas = ContentFinder<Texture2D>.Get("Effects/ElementalHearts/Flame/AbilityAtlas"); return atlas; }

        }

        // Original transparent montage; UV sampling changes no source pixels.

        public static Rect UV(int index)

        {

            return new Rect(index % 2 == 0 ? 0f : 0.5f, index < 2 ? 0.5f : 0f, 0.5f, 0.5f);

        }

        public static Mesh Mesh(int index)

        {

            if (meshes == null)

            {

                meshes = new Mesh[4];

                for (int i = 0; i < 4; i++)

                {

                    Rect uv = UV(i);

                    Mesh m = new Mesh(); m.name = "EH effect " + i;

                    m.vertices = new[] { new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f) };

                    m.uv = new[] { new Vector2(uv.xMin, uv.yMin), new Vector2(uv.xMin, uv.yMax), new Vector2(uv.xMax, uv.yMax), new Vector2(uv.xMax, uv.yMin) };

                    m.triangles = new[] { 0, 1, 2, 0, 2, 3 }; m.RecalculateNormals(); m.RecalculateBounds();

                    meshes[i] = m;

                }

            }

            return meshes[index];

        }

        public static Material Fade(int frame)

        {

            if (fades == null)

            {

                fades = new Material[12];

                for (int i = 0; i < fades.Length; i++)

                    fades[i] = MaterialPool.MatFrom(Atlas, ShaderDatabase.Transparent, new Color(1f, 1f, 1f, (12 - i) / 12f));

            }

            return fades[Mathf.Clamp(frame, 0, 11)];

        }

        private static Material[,] animationMaterials=new Material[7,6];

        private static Mesh centered, grounded;

        public static Material FrameMaterial(int sequence,int frame) {

            frame=Mathf.Clamp(frame,0,5);

            if(animationMaterials[sequence,frame]==null) {

                string[] names={"Slash","Dash","Spin","Pillar","Idle","Combat","Activation"};

                var texture=ContentFinder<Texture2D>.Get("Effects/ElementalHearts/Flame/Frames/"+names[sequence]+"_"+frame.ToString("00"));

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

            map.GetComponent<MapComponent_FlameVfx>().Add(sprite, start, end, duration, size, angle);

        }

        public static void Afterimage(Pawn pawn, Vector3 position)

        {
            if(AzureEvolution.Active(pawn)){if(ElementalHeartsMod.Settings.showAfterimages)AzureVfx.Emit(pawn,pawn.Map,1,position,position,12,1.4f);return;}

            if (!ElementalHeartsMod.Settings.showAfterimages || ElementalHeartsMod.Settings.effectOpacity<=0f || !pawn.Spawned || pawn.Map != Find.CurrentMap || Find.CameraDriver == null ||

                !Find.CameraDriver.CurrentViewRect.ExpandedBy(2).Contains(pawn.Position)) return;

            PawnTextureAtlasFrameSet frame; bool created;

            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frame, out created, false) && !created)

            {

                int index = frame.GetIndex(pawn.Rotation, PawnDrawMode.BodyAndHead);

                if (!frame.isDirty[index])

                {

                    Material mat = MaterialPool.MatFrom(new MaterialRequest(frame.atlas, ShaderDatabase.Transparent));

                    pawn.Map.GetComponent<MapComponent_FlameVfx>().AddGhost(frame.meshes[index], mat, position);

                    return;

                }

            }

            // A pawn not yet drawn has no valid cache frame. Never force a render/readback.

            Add(pawn.Map, 1, position, position, 12, 1.4f);

        }

    }



    public sealed class MapComponent_FlameVfx : MapComponent

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

        public MapComponent_FlameVfx(Map map) : base(map) { }

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

                        ghostColor.SetColor("_Color", new Color(1f, 0.4f, 0.08f, 0.45f * (1f - t) * ElementalHeartsMod.Settings.effectOpacity));

                        Graphics.DrawMesh(e.ghostMesh, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one), e.ghostMaterial, 0, null, 0, ghostColor);

                    }

                    continue;

                }

                int sequence=e.sprite==4?3:e.sprite;

                int frame=e.sprite==3?Mathf.Min(5,2+(int)(t*4)):e.sprite==4?Mathf.Min(1,(int)(t*2)):Mathf.Min(5,(int)(t*6));

                float angle=(e.sprite==2||e.sprite>=3)?0f:e.angle-90f;

                Matrix4x4 matrix=Matrix4x4.TRS(position,Quaternion.AngleAxis(angle,Vector3.up),new Vector3(e.size,1f,e.size));

                if(ghostColor==null)ghostColor=new MaterialPropertyBlock();

                ghostColor.SetColor("_Color",new Color(1f,1f,1f,ElementalHeartsMod.Settings.effectOpacity));

                Graphics.DrawMesh(FlameVfx.FrameMesh(e.sprite>=3),matrix,FlameVfx.FrameMaterial(sequence,frame),0,null,0,ghostColor);

            }

        }

    }

}



