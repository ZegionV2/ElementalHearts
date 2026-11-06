using System;

using System.Collections.Generic;

using System.Linq;

using RimWorld;

using UnityEngine;

using Verse;

using Verse.AI;



namespace ElementalHearts

{

    public enum LightningPower { Sever, Flash, Dance, Heaven }



    public sealed class ElementalHeartsSettings : ModSettings

    {

        public bool friendlyFire, autoDefault;

        public bool enableAutomation=true, showAura=true, showAfterimages=true;
        public bool enableConcord=true;
        public bool showCourtBadges=true,courtRaids=true,addCourtToExistingSaves=true;
        public bool courtHeralds=true,courtStories=true;
        public bool addCourtStrongholdsToExistingSaves=true;

        public float npcDiscipleChance=.2f;
        public float effectOpacity=1f;

        public int danceThreshold=3;

        public override void ExposeData() {

            Scribe_Values.Look(ref friendlyFire,"friendlyFire",false); Scribe_Values.Look(ref autoDefault,"autoDefault",false);

            Scribe_Values.Look(ref enableAutomation,"enableAutomation",true);Scribe_Values.Look(ref showAura,"showAura",true);Scribe_Values.Look(ref showAfterimages,"showAfterimages",true);
            Scribe_Values.Look(ref enableConcord,"enableConcord",true);
            Scribe_Values.Look(ref addCourtStrongholdsToExistingSaves,"addCourtStrongholdsToExistingSaves",true);
            Scribe_Values.Look(ref courtHeralds,"courtHeralds",true);Scribe_Values.Look(ref courtStories,"courtStories",true);
            Scribe_Values.Look(ref showCourtBadges,"showCourtBadges",true);Scribe_Values.Look(ref courtRaids,"courtRaids",true);Scribe_Values.Look(ref addCourtToExistingSaves,"addCourtToExistingSaves",true);

            Scribe_Values.Look(ref npcDiscipleChance,"npcDiscipleChance",.2f);npcDiscipleChance=float.IsNaN(npcDiscipleChance)?.2f:Math.Max(0f,Math.Min(1f,npcDiscipleChance));
            Scribe_Values.Look(ref effectOpacity,"effectOpacity",1f);Scribe_Values.Look(ref danceThreshold,"danceThreshold",3);

            effectOpacity=Math.Max(0f,Math.Min(1f,effectOpacity));danceThreshold=Math.Max(2,Math.Min(8,danceThreshold));

        }

    }



    [DefOf]

    public static class LightningDefs

    {

        public static DamageDef EH_Lightning;

        public static JobDef EH_HeavenlyThunder;

        public static HediffDef EH_Charged;

        static LightningDefs() { DefOfHelper.EnsureInitializedInCtor(typeof(LightningDefs)); }

    }



    public sealed class HediffCompProperties_LightningAbilities : HediffCompProperties

    {

        public float severRange = 10f, flashRange = 15f, danceRadius = 4f, heavenRange = 12f;

        public int severCooldown = 300, flashCooldown = 360, danceCooldown = 540, heavenCooldown = 1350;

        public int telegraphTicks = 12, staggerTicks = 60;

        public float fistFloor = 10f, explosionRadius = 3f;

        public float severPhysical = 1.5f, severElectrical = 0.8f;

        public float flashPhysical = 2f, flashElectrical = 1f;

        public float dancePhysical = 1.5f, danceElectrical = 1f;

        public float heavenPhysical = 6f, heavenElectrical = 4f, explosionElectrical = 1.5f;

        public HediffCompProperties_LightningAbilities() { compClass = typeof(HediffComp_LightningAbilities); }

    }



    public sealed class HediffComp_LightningAbilities : HediffComp

    {

        public bool autoCombat, autoSever=true, autoFlash=true, autoDance=true, autoHeaven;
        public bool nonLethal;
        public ElementalAutoMode CurrentAutoMode { get { return ElementalAutoModes.FromFlags(autoCombat || !ElementalCombatPolicy.Controlled(Pawn),nonLethal); } }
        public void CycleAutoMode() {
            ElementalAutoMode next=ElementalAutoModes.Next(CurrentAutoMode);
            autoCombat=next!=ElementalAutoMode.Off;nonLethal=next==ElementalAutoMode.NonLethal;
        }

        public int nextAutoCheck; public int nextRetreatCheck;

        private HairDef previousHair;

        private bool cooldowns43;

        public static bool IsLightningHair(HairDef hair){return hair!=null&&(hair.defName=="EH_LightningHair"||hair.defName=="EH_LightningHairShort");}
        public void ChooseHair(string name){
            if(Pawn.story==null)return;
            if(name==null){if(previousHair==null)return;Pawn.story.hairDef=previousHair;previousHair=null;}
            else {if(!IsLightningHair(Pawn.story.hairDef))previousHair=Pawn.story.hairDef;Pawn.story.hairDef=DefDatabase<HairDef>.GetNamed(name);}
            Pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
        public void ShowHairMenu(){Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>{
            new FloatMenuOption("Short lightning hair",()=>ChooseHair("EH_LightningHairShort")),
            new FloatMenuOption("Original lightning hair",()=>ChooseHair("EH_LightningHair")),
            new FloatMenuOption("Restore previous hairstyle",()=>ChooseHair(null))}));}
        public void ToggleLightningHair() {

            if(Pawn.story==null)return;

            HairDef lightning=DefDatabase<HairDef>.GetNamed("EH_LightningHair");

            if(Pawn.story.hairDef==lightning) {if(previousHair==null)return;Pawn.story.hairDef=previousHair;previousHair=null;}

            else {previousHair=Pawn.story.hairDef;Pawn.story.hairDef=lightning;}

            Pawn.Drawer.renderer.SetAllGraphicsDirty();

        }

        public override void CompPostPostRemoved() {

            if(previousHair!=null && Pawn.story!=null && IsLightningHair(Pawn.story.hairDef)) {

                Pawn.story.hairDef=previousHair;Pawn.Drawer.renderer.SetAllGraphicsDirty();

            }

            previousHair=null;

        }

        public void RefundHeaven() { readyAt[(int)LightningPower.Heaven]=Find.TickManager.TicksGame; }

        public override void CompPostPostAdd(DamageInfo? info) { autoCombat=true; cooldowns43=true; }

        private List<int> readyAt = new List<int> { 0, 0, 0, 0 };

        public HediffCompProperties_LightningAbilities Props { get { return ThunderstormEvolution.Active(Pawn)?ThunderstormEvolution.Tuning:(HediffCompProperties_LightningAbilities)props; } }

        public static HediffComp_LightningAbilities On(Pawn p)

        {

            if (p == null || p.health == null) return null;

            foreach (Hediff h in p.health.hediffSet.hediffs)

            {

                HediffWithComps hc = h as HediffWithComps;

                if (hc == null) continue;

                HediffComp_LightningAbilities comp = hc.TryGetComp<HediffComp_LightningAbilities>();

                if (comp != null) return comp;

            }

            return null;

        }



        public override void CompExposeData()

        {

            Scribe_Collections.Look(ref readyAt, "EH_readyAt", LookMode.Value);

            Scribe_Values.Look(ref autoCombat,"EH_autoCombat",false);Scribe_Values.Look(ref nonLethal,"EH_nonLethal",false);Scribe_Values.Look(ref autoSever,"EH_autoSever",true);

            Scribe_Values.Look(ref autoFlash,"EH_autoFlash",true);Scribe_Values.Look(ref autoDance,"EH_autoDance",true);Scribe_Values.Look(ref autoHeaven,"EH_autoHeaven",false);

            Scribe_Values.Look(ref nextAutoCheck,"EH_nextAutoCheck",0); Scribe_Values.Look(ref nextRetreatCheck,"EH_nextRetreatCheck",0);Scribe_Defs.Look(ref previousHair,"EH_previousHair");

            if (readyAt == null || readyAt.Count != 4) readyAt = new List<int> { 0, 0, 0, 0 };

            Scribe_Values.Look(ref cooldowns43,"EH_cooldowns43",false);

            if(Scribe.mode==LoadSaveMode.PostLoadInit && !cooldowns43) {

                int now=Find.TickManager.TicksGame;

                for(int i=0;i<readyAt.Count;i++)readyAt[i]=now+(Math.Max(0,readyAt[i]-now)+1)/2;

                cooldowns43=true;

            }

        }



        public int Remaining(LightningPower power) { return Math.Max(0, readyAt[(int)power] - Find.TickManager.TicksGame); }

        public void ReduceCooldowns(int ticks)

        {

            int now = Find.TickManager.TicksGame;

            for (int i = 0; i < readyAt.Count; i++) readyAt[i] = Math.Max(now, readyAt[i] - Math.Max(0, ticks));

        }

        public bool CanAct

        {

            get

            {

                return Pawn != null && !AzureEvolution.Evolving(Pawn) && !ElementalAnimationBridge.Controlled(Pawn) && !ElementalTeachingSession.Busy(Pawn) && !HediffComp_ElementalTransform.Transforming(Pawn) && Pawn.Spawned && !Pawn.Dead && !Pawn.Downed && (!ElementalCombatPolicy.Controlled(Pawn) || Pawn.Drafted) &&

                     !Pawn.InMentalState && Pawn.Awake() &&

                    !Pawn.WorkTagIsDisabled(WorkTags.Violent) && !Pawn.stances.stunner.Stunned &&

                    Pawn.health.hediffSet.hediffs.Contains(parent);

            }

        }



        private int Cooldown(LightningPower p)

        {

            switch (p) { case LightningPower.Sever: return Props.severCooldown; case LightningPower.Flash: return Props.flashCooldown;

                case LightningPower.Dance: return Props.danceCooldown; default: return Props.heavenCooldown; }

        }

        private float Range(LightningPower p)

        {

            switch (p) { case LightningPower.Sever: return Props.severRange; case LightningPower.Flash: return Props.flashRange;

                case LightningPower.Dance: return Props.danceRadius; default: return Props.heavenRange; }

        }

        private void StartCooldown(LightningPower p)

        {

            readyAt[(int)p] = Find.TickManager.TicksGame + Cooldown(p);

            HediffComp_LightningPassives passive = HediffComp_LightningPassives.On(Pawn);

            if (passive != null) passive.Activate();

        }

        private bool Busy { get { return Pawn.CurJob != null && (Pawn.CurJob.def == LightningDefs.EH_HeavenlyThunder||ThunderstormBlitz.Busy(Pawn)); } }



        public override IEnumerable<Gizmo> CompGetGizmos()

        {

            if (!ElementalCombatPolicy.Controlled(Pawn)) yield break;
   if(ElementalTeaching.IsHeart(parent)) yield return ElementalTeaching.Command(Pawn);


            if(!Pawn.Drafted) yield break;

            yield return new Command_Action {
                defaultLabel="Auto Combat: "+ElementalAutoModes.Label(CurrentAutoMode),
                defaultDesc="Click to cycle On → Off → Non-lethal. On uses normal attacks. Off disables automation. Non-lethal enables automation and converts elemental abilities, passives, chains, counters and ordinary melee into restrained suppression: 75% temporary knockdown chance, otherwise a brief stun. Causes no new wounds. Manual abilities also respect this mode. Existing injuries, ordinary ranged weapons and unrelated attacks remain dangerous.",
                icon=ElementalIcons.AutoCombat,action=CycleAutoMode,groupable=false
            };

            string[] names = ThunderstormEvolution.Active(Pawn)?ThunderstormEvolution.Names:new[]{ "Thunder Sever", "Lightning Flash", "Thunderstorm Dance", "Heavenly Thunder" };

            string[] desc = {

                "Instant directional melee shockwave. Extends to 10 tiles, stops at walls, and hits eligible pawns along the line. Physical + electrical damage; applies Charged.",

                "Instant blink to a clear adjacent cell, then an empowered weapon-derived melee strike. Cannot cross walls. Physical + electrical damage.",

                "Strike every eligible pawn within 4 tiles and line of sight. Physical + electrical damage, a brief stagger, and detonates your Charged stacks for bonus electrical damage.",

                "Blink into melee range, telegraph for 0.2 seconds, then deliver a devastating strike and a 3-tile electrical burst. Consumes your Charged stacks for bonus damage. Movement orders, stun, undrafting or target escape interrupt the strike; cooldown remains spent. Target death or incapacitation during the telegraph refunds it."

            };

            if(ThunderstormEvolution.Active(Pawn))desc=ThunderstormEvolution.Descriptions;
            for (int i = 0; i < 4; i++)

            {

                LightningPower power = (LightningPower)i;

                Command command;

                command = new Command_Target {
                    targetingParams = new TargetingParameters {
                        canTargetLocations = power == LightningPower.Sever, canTargetPawns = true,
                        canTargetBuildings = false, canTargetItems = false, canTargetSelf = false,
                        neverTargetIncapacitated = true,
                        validator = t => ValidTarget(power, t.Thing != null ? new LocalTargetInfo(t.Thing) : new LocalTargetInfo(t.Cell), t.Cell)
                    },
                    action = t => TryCast(power, t),
                    onUpdate = delegate { if (Pawn.Spawned) GenDraw.DrawRadiusRing(Pawn.Position, Range(power)); }
                };
                command.defaultLabel = names[i];

                command.defaultDesc = desc[i] + (ThunderstormEvolution.Active(Pawn) ? " Damaging electrical hits have a 40% chance to shock for 1.5 seconds." : " Damaging electrical hits have a 25% chance to shock for one second.") + " Shock grants three seconds of discharge recovery after the stun. Electrical resistance reduces its chance and duration." + "\nCooldown: " + (Cooldown(power) / 60f) + " seconds. Scales with usable melee attacks; works with fists. You can deliberately select any faction. Other targets follow friendly-fire settings. Downed/dead targets are excluded.";

                command.icon = ThunderstormEvolution.Active(Pawn)?ThunderstormEvolution.Icon(i):LightningVfx.Atlas;

                command.iconTexCoords = ThunderstormEvolution.Active(Pawn)?new Rect(0,0,1,1):LightningVfx.UV(i);

                command.groupable = false;

                if (!CanAct) command.Disable("Requires an awake, drafted, conscious colonist capable of violence.");

                else if (Busy) command.Disable(ThunderstormEvolution.Active(Pawn)?"Channeling Sovereign Thunder.":"Channeling Heavenly Thunder.");

                else if (Remaining(power) > 0) command.Disable("Recharging: " + Math.Ceiling(Remaining(power) / 60f) + "s");

                yield return command;

            }

        }



        private bool ValidTarget(LightningPower power, LocalTargetInfo target, IntVec3 cell, bool manual=true) {
            using(new ElementalManualTarget(Pawn,manual?target.Pawn:null))return ValidTargetCore(power,target,cell);
        }
        private bool ValidTargetCore(LightningPower power, LocalTargetInfo target, IntVec3 cell)

        {

            if (!CanAct || !cell.InBounds(Pawn.Map) || Pawn.Position.DistanceToSquared(cell) > Range(power) * Range(power)) return false;

            if (power == LightningPower.Sever) return cell != Pawn.Position;

            Pawn victim = target.Pawn;

            IntVec3 landing;

            if(power==LightningPower.Dance)return LightningCombat.Eligible(Pawn,victim)&&LightningCombat.ClearLine(Pawn.Position,cell,Pawn.Map);
            return LightningCombat.Eligible(Pawn, victim) && LightningCombat.ClearLine(Pawn.Position, cell, Pawn.Map) &&

                LightningCombat.TryLanding(Pawn, victim, Range(power), out landing);

        }



        public void TryCast(LightningPower power, LocalTargetInfo target, bool automatic=false) {
            using(new ElementalManualTarget(Pawn,automatic?null:target.Pawn))TryCastCore(power,target,automatic);
        }
        private void TryCastCore(LightningPower power, LocalTargetInfo target, bool automatic)

        {

            if (!CanAct || Busy || Remaining(power) > 0) return;

            if(power==LightningPower.Dance && LightningCombat.Targets(Pawn,Pawn.Position,Props.danceRadius).Count==0) {

                if(!automatic)Messages.Message("No eligible pawns within Thunderstorm Dance's range.",MessageTypeDefOf.RejectInput,false);

                return;

            }

            if(!automatic) nextAutoCheck=Find.TickManager.TicksGame+60;

            if ((!automatic || power != LightningPower.Dance) && !ValidTarget(power, target, target.Cell, !automatic))

            {

                if(!automatic) Messages.Message("No valid lightning path or clear landing cell.", MessageTypeDefOf.RejectInput, false);

                return;

            }

            if(ThunderstormEvolution.Active(Pawn)&&(power==LightningPower.Flash||power==LightningPower.Heaven)){
                if(ThunderstormBlitz.TryStart(Pawn,target,power==LightningPower.Heaven,automatic))StartCooldown(power);
                else if(!automatic)Messages.Message("No clear Thunderstorm blitz route or free landing cell.",MessageTypeDefOf.RejectInput,false);
                return;
            }
            if (power == LightningPower.Heaven)

            {

                // The native job persists its target and remaining telegraph through save/load.

                Job job = JobMaker.MakeJob(LightningDefs.EH_HeavenlyThunder, target);

                job.playerForced = !automatic;

                Pawn.jobs.StartJob(job,JobCondition.InterruptForced,resumeCurJobAfterwards:automatic);

                if(Pawn.CurJob==job) StartCooldown(power);

                return;

            }

            StartCooldown(power);

            // Instant impacts leave the live job/driver/shift queue untouched. Only a blink

            // stops the path follower; restore its existing destination afterward.

            Job continuingJob=Pawn.CurJob;

            bool wasMoving=Pawn.pather.Moving;

            LocalTargetInfo destination=Pawn.pather.Destination;

            PathEndMode pathMode=LightningJobUtility.PathMode(Pawn);

            Pawn.stances.CancelBusyStanceHard();

            switch (power)

            {

                case LightningPower.Sever: Sever(target.Cell); break;

                case LightningPower.Flash: Flash(target.Pawn); break;

                case LightningPower.Dance: Dance(); break;

            }

            // Brief recovery prevents the ability from simply resetting a normal melee swing.

            Pawn.stances.SetStance(new Stance_Cooldown(12, new LocalTargetInfo(Pawn.Position), null));

            if(power==LightningPower.Flash && wasMoving && Pawn.CurJob==continuingJob && destination.IsValid &&

                (!destination.HasThing || !destination.Thing.Destroyed)) Pawn.pather.StartPath(destination,pathMode);

        }



        public bool TryRetreat(Pawn threat) {
            if(!CanAct||Busy||Remaining(LightningPower.Flash)>0)return false;
            IntVec3 landing;
            if(!ElementalCombatPolicy.TryRetreat(Pawn,threat,Props.flashRange,LightningCombat.ClearLine,out landing))return false;
            StartCooldown(LightningPower.Flash);
            ElementalAudio.Ability(Pawn,ElementalSound.Dash);
            LightningCombat.Blink(Pawn,landing);
            Job pause=JobMaker.MakeJob(JobDefOf.Wait_Combat);pause.expiryInterval=45;pause.checkOverrideOnExpire=true;
            Pawn.jobs.StartJob(pause,JobCondition.InterruptForced,resumeCurJobAfterwards:true);
            nextAutoCheck=Find.TickManager.TicksGame+90;
            return true;
        }
        private void Sever(IntVec3 aimedCell)

        {

            ElementalAudio.Ability(Pawn,ElementalSound.Slash);

            Map map = Pawn.Map;

            LightningCombat.AttackProfile profile = LightningCombat.Profile(Pawn, Props.fistFloor);

            Vector3 direction = (aimedCell - Pawn.Position).ToVector3(); direction.Normalize();

            IntVec3 end = (Pawn.Position.ToVector3Shifted() + direction * Props.severRange).ToIntVec3();

            HashSet<Pawn> struck = new HashSet<Pawn>();

            IntVec3 last = Pawn.Position;

            foreach (IntVec3 cell in LightningGeometry.Trace(Pawn.Position, end))

            {

                if (!LightningCombat.Open(cell, map)) break;

                if (Pawn.Position.DistanceToSquared(cell) > Props.severRange * Props.severRange) break;

                last = cell;

                List<Thing> things = cell.GetThingList(map);

                for (int i = things.Count - 1; i >= 0; i--)

                {

                    Pawn victim = things[i] as Pawn;

                    if (LightningCombat.Eligible(Pawn, victim) && struck.Add(victim))

                    {

                        if(LightningCombat.Strike(Pawn, victim, profile, Props.severPhysical, Props.severElectrical,true)) LightningCharge.Apply(Pawn, victim);

                    }

                }

            }

            ThunderstormVfx.Emit(Pawn,map, 0, Pawn.Position.ToVector3Shifted(), last.ToVector3Shifted(), 18, 2.2f);

        }



        private void Flash(Pawn victim)

        {

            IntVec3 landing;

            if (!LightningCombat.TryLanding(Pawn, victim, Props.flashRange, out landing)) return;

            ElementalAudio.Ability(Pawn,ElementalSound.Dash);

            LightningCombat.Blink(Pawn, landing);

            if (LightningCombat.Eligible(Pawn, victim)){
                bool hit=LightningCombat.Strike(Pawn, victim, LightningCombat.Profile(Pawn, Props.fistFloor), Props.flashPhysical, Props.flashElectrical,true);
                if(hit&&ThunderstormEvolution.Active(Pawn)&&!victim.Dead&&!victim.Downed)ThunderstormEvolution.Slow(victim);
            }

        }



        private void Dance()

        {


            ElementalAudio.Ability(Pawn,ElementalSound.Spin);

            Map map = Pawn.Map; IntVec3 center = Pawn.Position;

            List<Pawn> targets = LightningCombat.Targets(Pawn, center, Props.danceRadius);

            LightningCombat.AttackProfile profile = LightningCombat.Profile(Pawn, Props.fistFloor);

            foreach (Pawn victim in targets)

            {

                HediffComp_Charged mark = LightningCharge.BeginConsume(Pawn, victim);

                HediffComp_LightningPassives passive = HediffComp_LightningPassives.On(Pawn);

                float bonus = mark == null || passive == null ? 0f : mark.consumingStacks * passive.Props.dancePerCharge;

                try { LightningCombat.Strike(Pawn, victim, profile, Props.dancePhysical, Props.danceElectrical + bonus,true); }

                finally { LightningCharge.FinishConsume(mark); }

                if (!victim.Dead && victim.Spawned && !victim.Downed) victim.stances.stagger.StaggerFor(Props.staggerTicks);

            }

            ThunderstormVfx.Emit(Pawn,map, 2, center.ToVector3Shifted(), center.ToVector3Shifted(), 24, Props.danceRadius * 2f);

        }



        public void HeavenImpact(Pawn victim)

        {

            if (!CanAct || !LightningCombat.Eligible(Pawn, victim) || Pawn.Position.DistanceToSquared(victim.Position) > 2 ||

                !LightningCombat.ClearLine(Pawn.Position, victim.Position, Pawn.Map)) return;

            ElementalAudio.Ability(Pawn,ElementalSound.Impact);

            Map map = Pawn.Map; IntVec3 center = victim.Position;

            // Snapshot neighbors before the main hit can down/kill the primary target.

            List<Pawn> neighbors = LightningCombat.Targets(Pawn, center, Props.explosionRadius);

            LightningCombat.AttackProfile profile = LightningCombat.Profile(Pawn, Props.fistFloor);

            ThunderstormVfx.Emit(Pawn,map, 3, center.ToVector3Shifted(), center.ToVector3Shifted(), 24, 4.5f);

            HediffComp_Charged mark = LightningCharge.BeginConsume(Pawn, victim);

            HediffComp_LightningPassives passive = HediffComp_LightningPassives.On(Pawn);

            float bonus = mark == null || passive == null ? 0f : mark.consumingStacks * passive.Props.heavenPerCharge;

            try { LightningCombat.Strike(Pawn, victim, profile, Props.heavenPhysical, Props.heavenElectrical + bonus,true); }

            finally { LightningCharge.FinishConsume(mark); }

            foreach (Pawn other in neighbors)

                if (other != victim && LightningCombat.Eligible(Pawn, other))

                    LightningCombat.Strike(Pawn, other, profile, 0f, Props.explosionElectrical,true);

            ThunderstormVfx.Emit(Pawn,map, 2, center.ToVector3Shifted(), center.ToVector3Shifted(), 18, Props.explosionRadius * 2f);

        }

    }



    public sealed class JobDriver_HeavenlyThunder : JobDriver

    {

        private bool impactCommitted;

        public override void ExposeData(){base.ExposeData();Scribe_Values.Look(ref impactCommitted,"EH_impactCommitted",false);}

        public override bool TryMakePreToilReservations(bool errorOnFailed) { return true; }

        protected override IEnumerable<Toil> MakeNewToils()

        {

            AddFinishAction(condition => {

                Pawn target=TargetA.Pawn;

                if(!impactCommitted && condition==JobCondition.Incompletable && target!=null && (target.Dead||target.Downed)) {

                    var abilities=HediffComp_LightningAbilities.On(pawn);if(abilities!=null)abilities.RefundHeaven();

                }

            });

            AddFailCondition(delegate {

                HediffComp_LightningAbilities c = HediffComp_LightningAbilities.On(pawn);

                return c == null || !c.CanAct || !LightningCombat.Eligible(pawn, TargetA.Pawn);

            });

            yield return Toils_General.Do(delegate {

                HediffComp_LightningAbilities c = HediffComp_LightningAbilities.On(pawn);

                IntVec3 landing;

                if (c == null || !LightningCombat.TryLanding(pawn, TargetA.Pawn, c.Props.heavenRange, out landing))

                { EndJobWith(JobCondition.Incompletable); return; }

                ElementalAudio.Ability(pawn,ElementalSound.Charge);

                pawn.stances.CancelBusyStanceHard();

                LightningCombat.Blink(pawn, landing);

                ThunderstormVfx.Emit(pawn,pawn.Map, 4, TargetA.Cell.ToVector3Shifted(), TargetA.Cell.ToVector3Shifted(), c.Props.telegraphTicks, 4.5f);

            });

            HediffComp_LightningAbilities comp = HediffComp_LightningAbilities.On(pawn);

            Toil wait = Toils_General.Wait(comp == null ? 12 : comp.Props.telegraphTicks);

            wait.AddFailCondition(() => !TargetA.HasThing || pawn.Position.DistanceToSquared(TargetA.Cell) > 2 ||

                !LightningCombat.ClearLine(pawn.Position, TargetA.Cell, pawn.Map));

            yield return wait;

            yield return Toils_General.Do(delegate {

                HediffComp_LightningAbilities c = HediffComp_LightningAbilities.On(pawn);

                if (c != null) { impactCommitted=true;c.HeavenImpact(TargetA.Pawn); }

            });

        }

    }



    public static class LightningCombat

    {

        public static bool Eligible(Pawn caster, Pawn victim)

        {

            return victim != null && victim != caster && victim.Spawned && victim.Map == caster.Map &&

                !victim.Dead && !victim.Downed &&

                ElementalCombatPolicy.TargetAllowed(ElementalManualTarget.Explicit(caster,victim),ElementalHeartsMod.Settings.friendlyFire,victim.IsPrisoner,victim.HostileTo(caster));

        }

        public static bool Open(IntVec3 cell, Map map) { return cell.InBounds(map) && cell.CanBeSeenOverFast(map); }

        public static bool ClearLine(IntVec3 a, IntVec3 b, Map map)

        {

            foreach (IntVec3 c in LightningGeometry.Trace(a, b)) if (!Open(c, map)) return false;

            return true;

        }

        public static bool TryLanding(Pawn caster, Pawn victim, float range, out IntVec3 landing)

        {

            landing = IntVec3.Invalid;

            if (!Eligible(caster, victim) || caster.Position.DistanceToSquared(victim.Position) > range * range ||

                !ClearLine(caster.Position, victim.Position, caster.Map)) return false;

            int best = int.MaxValue;

            for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)

            {

                if (x == 0 && z == 0) continue;

                IntVec3 c = victim.Position + new IntVec3(x, 0, z);

                if (!c.InBounds(caster.Map) || !c.Standable(caster.Map) || c.DistanceToSquared(caster.Position) > range * range ||

                    !ClearLine(caster.Position, c, caster.Map) || !ClearLine(c, victim.Position, caster.Map)) continue;

                if (c.GetThingList(caster.Map).Any(t => t is Pawn && t != caster)) continue;

                int dist = c.DistanceToSquared(caster.Position);

                if (dist < best) { landing = c; best = dist; }

            }

            return landing.IsValid;

        }

        public static void Blink(Pawn pawn, IntVec3 landing)

        {

            Map map = pawn.Map; Vector3 start = pawn.DrawPos;

            ThunderstormVfx.Afterimage(pawn, start);

            pawn.pather.StopDead();

            pawn.Position = landing;

            pawn.Notify_Teleported(false, true);

            Vector3 finish = landing.ToVector3Shifted();
            if(ThunderstormEvolution.Active(pawn)){StormBlitzFx.Streak(pawn,start,finish);for(int ghost=1;ghost<=3;ghost++)StormBlitzFx.Echo(pawn,Vector3.Lerp(start,finish,ghost/4f));}

            for (int i = 0; i < 5; i++)

            {

                Vector3 at = Vector3.Lerp(start, finish, i / 4f);

                ThunderstormVfx.Emit(pawn,map, 1, at, at, 12 + i * 2, 1.8f, (finish - start).AngleFlat());

            }

        }

        public static List<Pawn> Targets(Pawn caster, IntVec3 center, float radius)

        {

            // One bounded-radius cell search per cast, never every tick; no arbitrary victim cap.

            List<Pawn> result = new List<Pawn>();

            foreach (IntVec3 c in GenRadial.RadialCellsAround(center, radius, true))

            {

                if (!c.InBounds(caster.Map) || !ClearLine(center, c, caster.Map)) continue;

                foreach (Thing thing in c.GetThingList(caster.Map))

                {

                    Pawn p = thing as Pawn;

                    if (Eligible(caster, p)) result.Add(p);

                }

            }

            return result;

        }

        public struct AttackProfile

        {

            public float damage, penetration;

            public DamageDef physicalDef;

            public ThingDef weapon;

        }

        public static AttackProfile Profile(Pawn caster, float floor)

        {

            // Select from currently usable native melee verbs. Prefer the equipped melee weapon;

            // fall back to available unarmed/body attacks, independent of the cosmetic sword.

            Verb best = null; float bestDamage = 0f;

            List<VerbEntry> entries = caster.meleeVerbs.GetUpdatedAvailableVerbsList(false);

            bool hasWeapon = entries.Any(e => e.verb.EquipmentSource != null && e.verb.EquipmentSource.def.IsMeleeWeapon);

            foreach (VerbEntry entry in entries)

            {

                Verb verb = entry.verb;

                if (!verb.verbProps.IsMeleeAttack || verb.TerrainDefSource != null || (hasWeapon && (verb.EquipmentSource == null || !verb.EquipmentSource.def.IsMeleeWeapon))) continue;

                float value = verb.verbProps.AdjustedMeleeDamageAmount(verb, caster);

                if (value > bestDamage) { best = verb; bestDamage = value; }

            }

            float damage = Math.Max(floor * caster.GetStatValue(StatDefOf.MeleeDamageFactor), bestDamage);

            float penetration = best == null ? 0.1f : best.verbProps.AdjustedArmorPenetration(best, caster);

            ThingDef weapon = best == null || best.EquipmentSource == null ? null : best.EquipmentSource.def;

            return new AttackProfile { damage = damage, penetration = penetration, weapon = weapon,

                physicalDef = best == null ? DamageDefOf.Blunt : (best.verbProps.meleeDamageDef ?? DamageDefOf.Blunt) };

        }

        public static bool Strike(Pawn caster, Pawn victim, AttackProfile profile, float physical, float electrical, bool activeAbility=false)

        {

            if (!Eligible(caster, victim)) return false;
            if(activeAbility && physical>0f && WaterDefense.TryParry(caster,victim))return false;

            profile.damage *= ElementalTeaching.DamageFactor(caster);
            if(ThunderstormEvolution.Active(caster))profile.penetration+=.2f;
            if(ElementalNonLethal.TryApply(caster,victim,profile.damage*(Math.Max(0f,physical)+Math.Max(0f,electrical))))return false;

            // Ability impacts are deliberate guaranteed strikes, with ordinary armor handling.

            // They do not pretend to be a successful ordinary melee verb or trigger its postfixes.

            if (physical > 0f) victim.TakeDamage(new DamageInfo(profile.physicalDef,

                profile.damage * physical, profile.penetration, -1f, caster, null, profile.weapon, DamageInfo.SourceCategory.ThingOrUnknown, victim));

            if (victim.Dead || victim.Destroyed) return true;

            if (electrical > 0f)

            {

                var electricalResult = victim.TakeDamage(new DamageInfo(ThunderstormEvolution.Active(caster)?DefDatabase<DamageDef>.GetNamed("EH_ThunderstormDamage"):LightningDefs.EH_Lightning, profile.damage * electrical, 0.5f, -1f,

                    caster, null, profile.weapon, DamageInfo.SourceCategory.ThingOrUnknown, victim));

                if (electricalResult != null && electricalResult.totalDamageDealt > 0f) ElectricalShock.TryApply(caster, victim);

                if (activeAbility && !victim.Dead && !victim.Destroyed && victim.RaceProps.IsMechanoid)

                    victim.TakeDamage(new DamageInfo(DamageDefOf.EMP, 30f*ElementalTeaching.DamageFactor(caster), 0f, -1f, caster));

            }

            return true;
        }

    }



    public static class LightningGeometry

    {

        // Supercover: include both side cells at diagonal corners, preventing corner-wall bypass.

        public static IEnumerable<IntVec3> Trace(IntVec3 from, IntVec3 to)

        {

            int x = from.x, z = from.z;

            int nx = Math.Abs(to.x - x), nz = Math.Abs(to.z - z);

            int sx = Math.Sign(to.x - x), sz = Math.Sign(to.z - z), ix = 0, iz = 0;

            while (ix < nx || iz < nz)

            {

                long decision = (1L + 2L * ix) * nz - (1L + 2L * iz) * nx;

                if (decision == 0)

                {

                    yield return new IntVec3(x + sx, 0, z);

                    yield return new IntVec3(x, 0, z + sz);

                    x += sx; z += sz; ix++; iz++;

                }

                else if (decision < 0) { x += sx; ix++; }

                else { z += sz; iz++; }

                yield return new IntVec3(x, 0, z);

            }

        }

    }

}





