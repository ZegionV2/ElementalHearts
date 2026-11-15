# Aurora Incarnate integration verification

Verified in the installed RimWorld 1.6 runtime on 15 November 2026 using isolated test profiles. Personal saves and settings were not used as test targets. Harmony, Melee Animation, ISEKAI RPG and its Tower addon were loaded.

**363 checks passed:** 322 core integration checks and 41 clothing, recovery and presentation checks. Eight additional live sovereign duels completed. The production assembly tested is the installed Elemental Hearts assembly.

## Progression and existing saves

- A Glacial heart starts fresh Aurora mastery rather than inheriting earlier lineage milestones. Readiness requires 300 unique hostile non-animal kills and fifteen Glacial days.
- The free eight-second coronation can be interrupted and resumed. Saving during the ritual preserves its phase and readiness.
- Completion changes the original heart in place: one heart remains with the same saved identity. Water defense preferences, recovery/transform components, Pressure ownership and relevant cooldowns survive.
- The God Mode readiness command disappears immediately when God Mode is disabled, including a previously cached command.
- Codex, Water Bestow, seven replacement combat commands and sovereign benefits recognize Aurora.

## Combat and interruption

All seven forms started and finished through native jobs. Tested single-hit sets, five distinct Blitz targets, wall/occupied-cell routes, fixed Throne warning, caster-owned Pressure consumption, interruption cooldowns and cleanup. Saving during Blitz, the Throne warning and Mirrorfrost's counter resumed without repeated impacts or a permanent action lock.

Cataclysm's ordinary freeze lasts five seconds. Sovereign control is limited to 0.75 seconds; Aurora resists hostile Glacial freezing. Saving during knockback/freeze works. Different casters cannot bypass a victim's active freeze or shared thaw protection. Non-lethal Cataclysm produced no wound packets or hypothermia. Native Water/counter depth guards prevented recursive counters.

Automatic combat completed eight sixty-second-maximum duels, two each against Thunder Incarnate, Hell Flame Incarnate, Tempest Incarnate and Celestial, reversing sides for each pair. Aurora won four, lost two and reached two time-limit draws. Melee skills, weapon quality, starting profiles and ISEKAI stat progression were normalized; automatic grapples/executions were disabled. Aurora chained crescent, Blitz, spin, Dominion, Mirrorfrost and Throne automatically. Cataclysm's two-enemy trigger was exercised separately in the core checks. These are combat smoke tests, not a statistical balance guarantee.

## Gear and presentation

All 196 effect frames loaded. All 32 combinations of eight supported body textures and four facings initialized their native graphics; the Baby texture was exercised on an adult visual fixture, not an infant combatant. Native rendering was captured for Male, Female, Thin, Fat and Hulk in south/east/north/west. The covered-groin precept check passed for all twenty adult combinations. West uses native mirroring. These checks do not cover every third-party alien body or head renderer.

Ordinary revert restored the exact personal shirt, trousers and knife while leaving the two summoned projections outside inventory. Native incapacitation restored personal clothing; vanilla could drop the ordinary personal knife. Recovery and manifestation restored the same Winterglass and regalia identities, with no duplicate summoned gear or lingering action lock. The pawn's hair definition remained unchanged.

The installed Melee Animation data loader accepted all seventeen elemental sword profiles, including Winterglass's fitted grip, blade orientation and wider silhouette.

## Travel and bounds

Rime Step passed its Auto/Manual/Travel cycle, safe map jump and one-second recharge. Aurora used the shared launch-site party/cargo loader, reached the chosen site, saved during its own departure and world flight, and arrived on the actual destination map. The flying-pawn marker, original heart and Winterglass identity survived; arrival released all controls.

Aurora reuses the current native destination/Tower action routing. This integration test used a specific-cell landing; the settlement and Tower choices are shared with the already verified group-travel implementation.

Effects use fixed pools, cached materials, camera/fog culling and bounded local target lists. Frost/snow do not alter climate, terrain, roofs or buildings. Visual frame counts do not create extra damage packets.
