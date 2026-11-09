using System.Collections.Generic;

using System.Linq;

using HarmonyLib;

using RimWorld;

using Verse;

using UnityEngine;



namespace ElementalHearts

{

    public sealed class ElementalHeartsMod : Mod

    {

        public static ElementalHeartsSettings Settings;
        private Vector2 settingsScroll;private float settingsHeight=760f;

        public ElementalHeartsMod(ModContentPack content) : base(content)

        {

            Settings = GetSettings<ElementalHeartsSettings>();

            // Seven narrow patches, all documented in Patches/.

            new Harmony("elementalhearts.lightningheart").PatchAll(typeof(ElementalHeartsMod).Assembly);

        }

        public override string SettingsCategory() { return "Elemental Hearts"; }

        public override void DoSettingsWindowContents(Rect rect)

        {

            Listing_Standard listing = new Listing_Standard();

            var contentRect=new Rect(0,0,rect.width-20,settingsHeight);Widgets.BeginScrollView(rect,ref settingsScroll,contentRect);listing.Begin(contentRect);

            listing.CheckboxLabeled("Elemental effects can harm allies, neutral pawns and prisoners", ref Settings.friendlyFire);

            listing.Label("Off by default. The caster, dead pawns and downed pawns are always excluded. A manually selected pawn can always be targeted regardless of faction. Other active-ability targets, passives, chains and counters use this setting. It does not change vanilla melee attacks.");

            listing.Gap();

            listing.CheckboxLabeled("Enable auto combat for player-controlled pawns",ref Settings.enableAutomation);

            listing.Label("New disciples and Ascendants start with Auto Combat on.");

            listing.Label("Player-controlled pawns keep their saved toggle after loading or evolution. Non-player-controlled disciples and Ascendants always use auto combat, even when this master switch is off.");

            listing.Label("Auto area-attack enemy threshold: "+Settings.danceThreshold);

            Settings.danceThreshold=(int)listing.Slider(Settings.danceThreshold,2,8);

            listing.CheckboxLabeled("Show elemental auras / Azure head halos",ref Settings.showAura);

            listing.CheckboxLabeled("Show pawn afterimages",ref Settings.showAfterimages);

            listing.Label("Elemental effect opacity: "+Mathf.RoundToInt(Settings.effectOpacity*100f)+"%");

            Settings.effectOpacity=listing.Slider(Settings.effectOpacity,0f,1f);

            listing.Label("Visual settings do not change combat damage.");
            listing.CheckboxLabeled("Thunder Incarnate scenery damage and fires",ref Settings.incarnateTerrainDamage);
            listing.Label("Incarnate skill impacts cause real nearby structural blast damage, ash and ignition. Non-lethal mode suppresses this destruction. Thunder Step and Thunder Travel never cause terrain damage.");
            listing.Gap();listing.CheckboxLabeled("Enable allied Ascendant synchronization",ref Settings.enableConcord);
            listing.Label("Two allied transformed Ascendants striking the same enemy within four seconds and eight clear cells gain brief shared bonuses. Earth shields, Water supports recovery, Flame/Wind strengthens one hit, and Water/Lightning can discharge. Each participant has a shared six-second cooldown. No additional skill buttons.");

            listing.Gap();listing.Label("New humanoid NPCs spawning as disciples: "+Mathf.RoundToInt(Settings.npcDiscipleChance*100)+"%");
            Settings.npcDiscipleChance=Mathf.Round(listing.Slider(Settings.npcDiscipleChance,0f,1f)*100f)/100f;listing.Label("Eligible adult humanlike pawns from other factions can be Lightning, Flame, Wind, Water or Earth disciples. Awakening is one 5% roll at a near-death hostile combat hit. NPCs automatically summon their swords and Ascendant gear.");
            listing.Gap();listing.Label("Otherworld Calling cooldown: "+Settings.callingCooldownHours.ToString("0.0")+" in-game hours");
            Settings.callingCooldownHours=Mathf.Round(listing.Slider(Settings.callingCooldownHours,0f,24f)*10)/10f;listing.Label("Default: one in-game hour. Calling uses 1,000 reachable, visible, allowed silver anywhere on the current map; storage and a home area are not required. Zero disables only the cooldown. Recruits remain 18–24 with matching academy uniforms and no Fat/Hulk bodies.");
            listing.Gap();listing.Label("The Severed Court");
            listing.CheckboxLabeled("Show elemental badges beside Court pawn names",ref Settings.showCourtBadges);
            listing.CheckboxLabeled("Allow Severed Court raids",ref Settings.courtRaids);
            listing.CheckboxLabeled("Allow Court Herald commanders",ref Settings.courtHeralds);
            listing.CheckboxLabeled("Enable Court battle tactics and formations",ref Settings.courtTactics);
            listing.CheckboxLabeled("Enable Court encounters and stories",ref Settings.courtStories);
            listing.CheckboxLabeled("Add the Court to existing saves when loading",ref Settings.addCourtToExistingSaves);
            listing.CheckboxLabeled("Add Court strongholds to existing saves when loading",ref Settings.addCourtStrongholdsToExistingSaves);
            if(Current.Game!=null&&Find.World!=null&&listing.ButtonText("Configure Court settlements in this world"))Find.World.GetComponent<WorldComponent_SeveredCourt>().OpenSetup();
            listing.Label("Court raids begin after day 30 and use a bounded squad budget. At day 60 and 8,000 points, rare assaults can include one evolved Herald. Raids of 10,000 points or more guarantee one when a surviving base can dispatch it. Herald stories provide advance warnings; accepting a deserter can bring a warned pursuit. Existing saves show a settlement-count menu once. The button reopens it to raise the saved total or add a missing Court faction. All Court bases use the detailed fort layout. Cleared strongholds and defeated factions are not regenerated. Recruitment removes the Severed Oath and restores the normal elemental outfit.");
            settingsHeight=listing.CurHeight+12;listing.End();Widgets.EndScrollView();

        }

    }



    public static class ElementalHeartUtility

    {

        public const string HeartTag = "EH_ElementalHeart";



        public static bool HasHeart(Pawn pawn)

        {

            return pawn != null && pawn.health != null && pawn.health.hediffSet.hediffs.Any(

                h => h.def.tags != null && h.def.tags.Contains(HeartTag));

        }

    }



    // A normal recipe subclass avoids patching any game's surgery method.

    // Shared tag identifies future hearts without implementing those hearts now.

    public sealed class Recipe_InstallElementalHeart : Recipe_InstallImplant

    {

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)

        {

            Pawn pawn = thing as Pawn;

            return pawn != null && pawn.RaceProps.Humanlike && !pawn.Dead &&

                !ElementalHeartUtility.HasHeart(pawn) && base.AvailableOnNow(thing, part);

        }



        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)

        {

            if (!AvailableOnNow(pawn)) yield break;

            foreach (BodyPartRecord part in base.GetPartsToApplyOn(pawn, recipe)) yield return part;

        }



        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer,

            List<Thing> ingredients, Bill bill)

        {

            // Recheck at completion: a queued second bill or another mod may have added a heart.

            if (!AvailableOnNow(pawn, part) || part == null ||

                !base.GetPartsToApplyOn(pawn, recipe).Contains(part))

            {

                Log.Warning("[Elemental Hearts] Installation cancelled: recipient is no longer eligible. No second heart was added.");

                return;

            }

            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

        }

    }

}





