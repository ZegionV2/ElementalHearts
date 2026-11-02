using System;

using System.Collections.Generic;

using RimWorld;

using Verse;



namespace ElementalHearts

{

    public sealed class LightningChain : IExposable

    {

        public Pawn source;

        public IntVec3 origin;

        public int nextTick, remaining, interval;

        public float damage;

        public List<int> visited = new List<int>();

        public void ExposeData()

        {

            Scribe_References.Look(ref source, "source"); Scribe_Values.Look(ref origin, "origin");

            Scribe_Values.Look(ref nextTick, "nextTick"); Scribe_Values.Look(ref remaining, "remaining");

            Scribe_Values.Look(ref interval, "interval", 6); Scribe_Values.Look(ref damage, "damage");

            Scribe_Collections.Look(ref visited, "visited", LookMode.Value);

            if (visited == null) visited = new List<int>();

        }

    }

    public sealed class LightningCounter : IExposable

    {

        public Pawn source, target;

        public int due;

        public void ExposeData()

        {

            Scribe_References.Look(ref source, "source"); Scribe_References.Look(ref target, "target");

            Scribe_Values.Look(ref due, "due");

        }

    }

    public sealed class MapComponent_LightningPassives : MapComponent

    {

        private List<LightningChain> chains = new List<LightningChain>();

        private List<LightningCounter> counters = new List<LightningCounter>();

        private readonly Stack<LightningChain> chainPool = new Stack<LightningChain>();

        private readonly Stack<LightningCounter> counterPool = new Stack<LightningCounter>();

        private int autoCursor;

        private readonly List<Pawn> auraPawns = new List<Pawn>();

        public int PendingChains { get { return chains.Count; } }

        public int PendingCounters { get { return counters.Count; } }

        public MapComponent_LightningPassives(Map map) : base(map) { }

        public override void ExposeData()

        {

            Scribe_Collections.Look(ref chains, "EH_chains", LookMode.Deep);

            Scribe_Collections.Look(ref counters, "EH_counters", LookMode.Deep);

            if (chains == null) chains = new List<LightningChain>();

            if (counters == null) counters = new List<LightningCounter>();

        }

        public override void FinalizeInit()

        {

            auraPawns.Clear();

            foreach (Pawn p in map.mapPawns.AllPawnsSpawned) if (HediffComp_LightningPassives.On(p) != null) Register(p);

        }

        public void Register(Pawn p) { if (!auraPawns.Contains(p)) auraPawns.Add(p); }

        public void Forget(Pawn p) { auraPawns.Remove(p); }

        public bool QueueCounter(Pawn source, Pawn target)

        {

            if (counters.Count >= 64) return false;

            LightningCounter item = counterPool.Count > 0 ? counterPool.Pop() : new LightningCounter();

            item.source = source; item.target = target; item.due = Find.TickManager.TicksGame + 1;

            counters.Add(item); return true;

        }

        public void QueueChain(Pawn source, Pawn first, IntVec3 origin, float damage, int targets, int interval)

        {

            if (chains.Count >= 32 || targets <= 0) return;

            LightningChain item = chainPool.Count > 0 ? chainPool.Pop() : new LightningChain();

            item.source = source; item.origin = origin; item.damage = damage;

            item.remaining = Math.Min(3, targets); item.interval = Math.Max(6, interval);

            item.nextTick = Find.TickManager.TicksGame + item.interval;

            item.visited.Clear(); item.visited.Add(first.thingIDNumber);

            chains.Add(item);

        }

        public override void MapComponentTick()

        {

            int now = Find.TickManager.TicksGame;

            // Maintain the registry even when aura rendering is disabled or the map is offscreen.

            if(now%250==0) for(int i=auraPawns.Count-1;i>=0;i--) {

                Pawn p=auraPawns[i];if(p==null||!p.Spawned||p.Map!=map||HediffComp_LightningPassives.On(p)==null)auraPawns.RemoveAt(i);

            }

            for (int i = counters.Count - 1; i >= 0; i--)

            {

                LightningCounter c = counters[i]; if (now < c.due) continue;

                counters.RemoveAt(i);

                HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(c.source);

                if (comp != null && comp.Active && !comp.Channeling && c.source.Map == map &&

                    LightningCombat.Eligible(c.source, c.target) && c.source.Position.DistanceToSquared(c.target.Position) <= 2 &&

                    LightningCombat.ClearLine(c.source.Position, c.target.Position, map))

                {

                    LightningCharge.Apply(c.source, c.target);

                    LightningCombat.Strike(c.source, c.target, LightningCombat.Profile(c.source, 10f), 1f, comp.Props.strikeElectrical);

                    ThunderstormVfx.Emit(c.source,map, 0, c.source.DrawPos, c.target.DrawPos, 12, 1.4f);

                }

                c.source = null; c.target = null; counterPool.Push(c);

            }

            // No target searches on the other five ticks. At most eight searches on a service tick.

            if (now % 6 != 0) return;

            for(int n=0;n<Math.Min(4,auraPawns.Count);n++) {

                if(autoCursor>=auraPawns.Count)autoCursor=0;

                Pawn autoPawn=auraPawns[autoCursor++];

                if(autoPawn!=null&&autoPawn.Spawned&&autoPawn.Map==map) {var abilities=HediffComp_LightningAbilities.On(autoPawn);if(abilities!=null)LightningAutocast.Evaluate(abilities);}

            }

            int budget = 8;

            for (int i = chains.Count - 1; i >= 0 && budget > 0; i--)

            {

                LightningChain c = chains[i]; if (now < c.nextTick) continue;

                budget--;

                HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(c.source);

                Pawn next = null;

                if (comp != null && comp.Active && c.source.Map == map && c.remaining > 0)

                {

                    foreach (Pawn p in LightningCombat.Targets(c.source, c.origin, Math.Min(4f, comp.Props.chainRadius)))

                    {

                        if (c.visited.Contains(p.thingIDNumber)) continue;

                        if (next == null || p.Position.DistanceToSquared(c.origin) < next.Position.DistanceToSquared(c.origin) ||

                            (p.Position.DistanceToSquared(c.origin) == next.Position.DistanceToSquared(c.origin) && p.thingIDNumber < next.thingIDNumber)) next = p;

                    }

                }

                if (next != null)

                {

                    IntVec3 previous = c.origin; c.origin = next.Position; c.visited.Add(next.thingIDNumber);

                    LightningCharge.Apply(c.source, next);

                    LightningCombat.Strike(c.source, next, new LightningCombat.AttackProfile { damage = c.damage }, 0f, 1f);

                    ThunderstormVfx.Emit(c.source,map, 1, previous.ToVector3Shifted(), c.origin.ToVector3Shifted(), 12, 1.2f);

                    c.damage *= 0.75f; c.remaining--; c.nextTick = now + c.interval;

                }

                if (next == null || c.remaining <= 0)

                {

                    chains.RemoveAt(i); c.source = null; c.visited.Clear(); chainPool.Push(c);

                }

            }

        }

        public override void MapComponentDraw()

        {

            if (!ElementalHeartsMod.Settings.showAura || ElementalHeartsMod.Settings.effectOpacity<=0f || map != Find.CurrentMap || Find.CameraDriver == null) return;

            int drawn = 0;

            CellRect view = Find.CameraDriver.CurrentViewRect.ExpandedBy(2);

            for (int i = auraPawns.Count - 1; i >= 0; i--)

            {

                Pawn p = auraPawns[i];

                if(p!=null&&p.Spawned&&p.Map==map&&(!p.Drafted||p.Dead||p.Downed||!view.Contains(p.Position)))continue;
                HediffComp_LightningPassives comp = HediffComp_LightningPassives.On(p);

                if (p == null || !p.Spawned || p.Map != map || comp == null) { auraPawns.RemoveAt(i); continue; }

                if (p.Dead || p.Position.Fogged(map) || !view.Contains(p.Position) || drawn >= 64) continue;

                LightningAura.Draw(p, comp.AuraState); drawn++;

            }

        }

    }

}


