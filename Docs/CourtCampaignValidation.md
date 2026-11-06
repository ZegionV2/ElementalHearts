# Court campaign verification

The compiled build includes the five Heralds, shared faction apparel, native map-name badges, dedicated Herald assaults, writs, dossiers, sanctuary encounters and five permanent strongholds. Recruitment removes the Severed Oath and restores normal elemental gear, keeping powers, saved controls and the existing sword.

Native verification against RimWorld 1.6.4871 with Harmony and optional Melee Animation passed **127 checks** for this campaign update:

- 112 campaign/recruitment/event/map/retreat/save checks, including all five generated stronghold maps and the native caravan Attack action.
- 7 compatibility checks using a pre-stronghold save with a pending Herald writ.
- 8 manual introduction checks using a pre-Court world with automatic introduction disabled.

Verified native defensive lords, exactly one Herald/two Knights/six Oathlings per initial garrison, gate routes, roof supports, matching serum and finite treasure. All defender IDs survived saving/loading. Retreat followed by save/load preserved unclaimed loot without restoring removed silver. Clearing a stronghold cancelled its pending expedition and did not recreate the site on later loads or manual introduction. Existing factions and settlements were retained. The native faction factory's first settlement is reused when adding a missing Court.

The preceding Herald update passed 115 native integration checks and 25 combat/halo/interruption checks. The first Court release passed 175 native faction/gear/UI/awakening checks. Native apparel fitting checked all 28 body/direction combinations; the Herald art exports include 27 runtime RGBA textures. These are recorded manual/native fixture results, not a claim that all combinations of third-party mods have been tested.

Existing transformation, sword-bond and story save keys remain compatible. Strongholds use separately saved placement progress and bounded provision storage. The previous six-argument story-order API is retained for compatibility. Combat effects keep the established bounded pools, cached materials and camera/fog culling.

The prebuilt assembly is included for normal installation. Native fixture mods, isolated save games, local logs and temporary assemblies are excluded from the playable release. See [the campaign guide](SeveredCourt.md) for controls and encounter rules.
