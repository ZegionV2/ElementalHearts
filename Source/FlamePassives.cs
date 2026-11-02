using System;

using System.Collections.Generic;

using RimWorld;

using Verse;



namespace ElementalHearts

{

    public sealed class ThermalDamageExtension : DefModExtension { }



    public sealed class HediffCompProperties_FlamePassives : HediffCompProperties

    {

        public float strikeThermal = 0.5f, chainChance = 0.25f, chainRadius = 4f, chainDamageFactor = 0.4f;

        public int chainTargets = 3, chainInterval = 6;

        public int maxCharge = 5, chargeDuration = 600, killCooldownReduction = 90;

        public float dancePerCharge = 0.6f, heavenPerCharge = 1f;

        public float counterChance = 0.5f, resistanceFactor = 0.25f;

        public int counterCooldown = 120;

        public List<DamageDef> thermalDamageDefs;

        public HediffCompProperties_FlamePassives() { compClass = typeof(HediffComp_FlamePassives); }

    }



    public sealed class HediffComp_FlamePassives : HediffComp

    {

        private int counterReadyAt, activationUntil, combatUntil;

        public HediffCompProperties_FlamePassives Props { get { return (HediffCompProperties_FlamePassives)props; } }

        public int ActivationRemaining { get { return Math.Max(0,activationUntil-Find.TickManager.TicksGame); } }

        public int CounterRemaining { get { return Math.Max(0, counterReadyAt - Find.TickManager.TicksGame); } }

        public bool Active

        {

            get { return !AzureEvolution.Evolving(Pawn) && Pawn.Spawned && !Pawn.Dead && !Pawn.Downed && Pawn.Awake() && !Pawn.InMentalState &&

                !Pawn.WorkTagIsDisabled(WorkTags.Violent) && !Pawn.stances.stunner.Stunned && Pawn.health.hediffSet.hediffs.Contains(parent); }

        }

        public static HediffComp_FlamePassives On(Pawn pawn)

        {

            if (pawn == null || pawn.health == null) return null;

            foreach (Hediff h in pawn.health.hediffSet.hediffs)

            {

                HediffWithComps hc = h as HediffWithComps;

                if (hc == null) continue;

                HediffComp_FlamePassives c = hc.TryGetComp<HediffComp_FlamePassives>();

                if (c != null) return c;

            }

            return null;

        }

        public override void CompExposeData()

        {

            Scribe_Values.Look(ref counterReadyAt, "EH_counterReadyAt", 0);

            Scribe_Values.Look(ref activationUntil, "EH_activationUntil", 0);

            Scribe_Values.Look(ref combatUntil, "EH_combatUntil", 0);

        }

        private void Register() { if (Pawn.Spawned) Pawn.Map.GetComponent<MapComponent_FlamePassives>().Register(Pawn); }

        public override void CompPostPostAdd(DamageInfo? dinfo) { Register(); }

        public override void Notify_Spawned() { Register(); }

        public override void CompPostPostRemoved()

        {

            if (Pawn.Spawned) Pawn.Map.GetComponent<MapComponent_FlamePassives>().Forget(Pawn);

        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)

        {

            if (totalDamageDealt > 0f) combatUntil = Find.TickManager.TicksGame + 180;

        }

        public void MarkCombat() { combatUntil=Find.TickManager.TicksGame+180; Register(); }

        public void Activate()

        {

            int now = Find.TickManager.TicksGame;

            activationUntil = now + 30; combatUntil = now + 180; Register();

        }

        public int AuraState

        {

            get { int now = Find.TickManager.TicksGame; return activationUntil > now ? 2 : (combatUntil > now ? 1 : 0); }

        }

        public void MeleeHit(Pawn victim)

        {

            if (!Active || !FlameCombat.Eligible(Pawn, victim)) return;

            MarkCombat();

            FlameCharge.Apply(Pawn, victim);

            FlameCombat.AttackProfile profile = FlameCombat.Profile(Pawn, 10f);

            IntVec3 origin = victim.Position;

            FlameCombat.Strike(Pawn, victim, profile, 0f, Props.strikeThermal);

            AzureVfx.Emit(Pawn,Pawn.Map, 1, origin.ToVector3Shifted(), origin.ToVector3Shifted(), 12, 0.8f);

            if (Rand.Chance(Props.chainChance))

                Pawn.Map.GetComponent<MapComponent_FlamePassives>().QueueChain(Pawn, victim, origin, profile.damage * Props.chainDamageFactor, Props.chainTargets, Props.chainInterval);

        }

        public void SuccessfulDodge(Pawn attacker)

        {

            if (!Active || CounterRemaining > 0 || !FlameCombat.Eligible(Pawn, attacker) ||

                Pawn.Position.DistanceToSquared(attacker.Position) > 2 ||

                !FlameCombat.ClearLine(Pawn.Position, attacker.Position, Pawn.Map) || Channeling || !Rand.Chance(Props.counterChance)) return;

            if (!Pawn.Map.GetComponent<MapComponent_FlamePassives>().QueueCounter(Pawn, attacker)) return;

            counterReadyAt = Find.TickManager.TicksGame + Props.counterCooldown;

            MarkCombat();

            FlameVfx.Afterimage(Pawn, Pawn.DrawPos);

        }

        public bool Channeling { get { return ElementalTeachingSession.Busy(Pawn) || AzureEvolution.Evolving(Pawn) || Pawn.CurJob != null && Pawn.CurJob.def == FlameDefs.EH_PhoenixDescent; } }

    }



    public sealed class HediffCompProperties_SearingBrand : HediffCompProperties

    {

        public HediffCompProperties_SearingBrand() { compClass = typeof(HediffComp_SearingBrand); }

    }

    public sealed class HediffComp_SearingBrand : HediffComp

    {

        public Pawn owner;

        public int stacks, expiresAt;

        private bool killClaimed;

        // Only set during a synchronous consuming strike. Never survives across ticks/saves.

        internal int consumingStacks;

        public bool Live { get { return owner != null && expiresAt > Find.TickManager.TicksGame && (stacks > 0 || consumingStacks > 0); } }

        public override bool CompShouldRemove { get { return !Live || Pawn.Dead; } }

        public override string CompLabelInBracketsExtra { get { return stacks + (AzureEvolution.Active(owner)?" azure brands":" embers"); } }

        public override string CompTipStringExtra

        {

            get { return "Branded by " + (owner == null ? "unknown" : owner.LabelShort) + ". Expires in " + Math.Max(0, expiresAt - Find.TickManager.TicksGame) / 60f + "s."; }

        }

        public override void CompExposeData()

        {

            Scribe_References.Look(ref owner, "EH_chargeOwner");

            Scribe_Values.Look(ref stacks, "EH_chargeStacks", 0);

            Scribe_Values.Look(ref expiresAt, "EH_chargeExpiry", 0);

            Scribe_Values.Look(ref killClaimed, "EH_chargeKillClaimed", false);

        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit)

        {

            if (killClaimed || !Live || !dinfo.HasValue || dinfo.Value.Instigator != owner) return;

            killClaimed = true; // Set before modifying cooldowns: one death, one rebate.

            HediffComp_FlamePassives source = HediffComp_FlamePassives.On(owner);

            HediffComp_FlameAbilities powers = HediffComp_FlameAbilities.On(owner);

            if (source != null && powers != null) powers.ReduceCooldowns(source.Props.killCooldownReduction);

        }

        public void Refresh(Pawn source, HediffCompProperties_FlamePassives settings)

        {

            if (owner != source || !Live) { stacks = 0; killClaimed = false; }

            owner = source; stacks = Math.Min(AzureEvolution.Active(source)?3:settings.maxCharge, stacks + 1);

            expiresAt = Find.TickManager.TicksGame + settings.chargeDuration;

        }

    }

    public static class FlameCharge

    {

        public static HediffComp_SearingBrand Find(Pawn pawn)

        {

            if (pawn == null || pawn.health == null) return null;

            HediffWithComps h = pawn.health.hediffSet.GetFirstHediffOfDef(FlameDefs.EH_SearingBrand) as HediffWithComps;

            return h == null ? null : h.TryGetComp<HediffComp_SearingBrand>();

        }

        public static void Apply(Pawn caster, Pawn target)

        {

            HediffComp_FlamePassives source = HediffComp_FlamePassives.On(caster);

            if (source == null || !FlameCombat.Eligible(caster, target)) return;

            HediffComp_SearingBrand mark = Find(target);

            if (mark == null)

            {

                HediffWithComps h = (HediffWithComps)HediffMaker.MakeHediff(FlameDefs.EH_SearingBrand, target);

                mark = h.TryGetComp<HediffComp_SearingBrand>();

                mark.Refresh(caster, source.Props); // Set before AddHediff can inspect removal conditions.

                target.health.AddHediff(h);

            }

            else mark.Refresh(caster, source.Props);

        }

        public static HediffComp_SearingBrand BeginConsume(Pawn caster, Pawn target)

        {

            HediffComp_SearingBrand mark = Find(target);

            if (mark == null || !mark.Live || mark.owner != caster) return null;

            mark.consumingStacks = AzureEvolution.Active(caster)?Math.Min(3,mark.stacks):mark.stacks; mark.stacks = 0;

            return mark;

        }

        public static void FinishConsume(HediffComp_SearingBrand mark)

        {

            if (mark == null) return;

            mark.consumingStacks = 0;

            if (mark.Pawn.health.hediffSet.hediffs.Contains(mark.parent)) mark.Pawn.health.RemoveHediff(mark.parent);

        }

    }

}

