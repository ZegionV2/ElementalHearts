# Hell Flame Incarnate integration verification

Verified against the locally installed RimWorld 1.6 game and Harmony, with optional Melee Animation, Humanoid Alien Races, FemaleBB support and ISEKAI in an isolated test configuration. Personal saves/settings were not used. The compiled assembly and definitions are installed with the mod.

**120 native assertions passed** across the main 67-check sequence, ten additional mechanics checks and 43 presentation/material checks. The final presentation sequence was also repeated in an interactive game window, allowing actual rendered apparel and held swords to be inspected. Automated batch-mode screenshots were blank and were not accepted as visual evidence.

## Progression, equipment and saves

- Existing Azure roots acquire separate third-stage counters. God-only readiness, cached-command hiding and the eight-second ceremony work.
- Saving/loading mid-coronation preserves the original heart identity, combat/non-lethal preferences and prior cooldowns. Completion replaces Azure rather than stacking its stage.
- Manifestation equips one Hell Flame suit and Ashreign katana. Native groin-coverage checks pass. Fire damage, heatstroke and Melee Animation knockout protection resolve correctly; Moving is 650% and core damage/defense match the sovereign tier.
- Downing restores personal clothing. Recovery manifests the same owned sword without duplication.
- Saving/loading before and after Pyre's committed impact does not repeat damage. Interrupted movement releases control while retaining its spent cooldown.

## Combat and utility

- All seven techniques start and finish. Each eligible victim receives at most one physical/thermal pair. Reversal rejects fewer than two nearby threats; Furnace safely pushes standing targets outward.
- The actual Auto Combat scheduler executed all seven forms. A separate live damage check produced real injury; Non-lethal produced no new wounds.
- Paired Bestow grants ordinary Flame, enables the recipient's default Auto Combat and releases both participants.
- Dash planning rejects a solid wall and a blocked diagonal corner. Routes also reject closed doors and recheck moving destinations. These are bounded local checks, not an exhaustive test of all third-party map objects.
- The utility's Auto → Manual → Travel cycle saves its settings and turns Auto off in Manual/Travel. A map-wide blink reaches its exact safe cell.
- Same-map travel saved mid-departure and cross-map travel preserve the original pawn, exact destination, drafted state and sword identity, then release the action lock.

## Presentation and static checks

- All 21 runtime material slots resolve their dedicated cached sequences. All 146 unique authored effect frames, eleven new command icons and 24 worn textures are present.
- The Melee Animation registry resolves the native fitted katana profile, scale, rotation and cutting-edge flip. Twenty adult body/facing cases select animated sword idles and pass mechanical groin coverage.
- Live native and animated screenshots were inspected for Male, Female, Thin, Fat and Hulk in north/east/south/west views. Fitting, cape coverage and sword placement look consistent. The seven commands and utility fit the native wrapping command panel.
- The earlier asset-stage geometry checks cover 32 body/facing combinations across seven native bodies plus FemaleBB, with zero uncovered native-body pixels. Child/Baby assets were checked as textures; this report does not claim live combat/rendering tests of every custom race or developmental stage.
- All installed XML parses; no duplicate typed Def names were found. Both bound gear definitions are indestructible and the suit includes Torso, Shoulders, Arms, Hands, Waist, Legs and Feet. Installed source/assembly/profile/documentation hashes match the staged result.

The temporary test mod is archived outside RimWorld's Mods directory. Development fixtures, test saves and their logs remain in the local workspace rather than the playable package. Restart RimWorld after installing this update.
