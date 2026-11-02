# NPC disciples and awakening

New eligible humanoid NPCs have a **20% chance** to become a Lightning, Flame, Wind or Water disciple. Each element is equally likely. Change the rate from 0–100% in the scrollable Elemental Hearts settings.

Generation applies to newly created, adult, violence-capable humanlike pawns with a non-player faction. Mechanoids, animals, children, prisoners, slaves, dead/downed pawns and existing elemental characters are excluded. Other mods' races qualify when they use RimWorld's Humanlike race flag. Existing pawns and reused world pawns are not retroactively rerolled.

NPC disciples start with Auto Combat enabled and automatically call their bound sword when able. NPC Ascendants automatically manifest their armor, helmet and sword when spawned and after recovery, without needing a draft command. Downing still returns them to ordinary clothes. The same bound sword returns after recovery; permanently destroyed pawns have their orphaned sword bonds removed. A dead pawn with a recoverable corpse retains its bond for revival. Player-controlled characters keep their manual transformation controls.

## Awakening

An eligible disciple gets **one lifetime 5% chance** during its first critical hostile combat hit. A hit is critical when health reaches 25% or lower, or the native health checks would down or kill the pawn. Environmental damage, self damage and non-hostile damage do not qualify. The attempt is saved immediately, including a failed roll. Player-taught disciples can also awaken under the same combat rules.

A successful roll promotes the disciple directly to its matching **normal Ascendant**, without the usual five-day/50-kill requirement. Azure Flame and Thunderstorm still use their separate evolution progression. Glacial remains a plan.

Awakening immediately repairs non-permanent injuries and blood loss, including body parts lost in the triggering hit. Older missing parts, permanent scars and diseases remain. It equips the normal Ascendant kit and grants a separate, visible three-second protection hediff: incoming damage is multiplied by 0.1 for 180 game ticks. This protection expires normally.

An elemental explosion follows on the next game tick, with an expanding double ring and radial streaks drawn in C#. The shockwave reaches 3.5 cells through clear lines, hits each target at most once, and briefly stuns survivors. It uses 18 base blunt damage, the shared 20% elemental damage reduction, the existing Non-lethal mode and friendly-fire setting. The default policy excludes the source and allies. It does not damage buildings, terrain or start fires.

The explosion queue is saved, removes each entry before applying its impact and processes at most four explosions per map per tick, with at most 16 victims per explosion. Cosmetic flashes use a fixed 32-entry pool and respect the effect-opacity setting, camera and fog. No new animation textures replace the existing transformation artwork.

## Saves and verification

Existing disciple kill/progression keys and Water state are retained. The additive `EH_awakeningRolled` field defaults to false in older saves. Pending bursts use a map-local saved queue. Promotion retains the existing shared lineage upgrade path, automation/Non-lethal state and bound sword identity. The temporary fatal/downing hold exists only inside the matching damage operation and is released in a finalizer.

The update passes 34 automated regression/verification scripts and an isolated native RimWorld fixture covering generation across factions, all four successful lineages, a failed roll and attempted reroll, native save/load before explosions, target deduplication, allied exclusions, downing/recovery, grace expiry, gear identity and destroyed-pawn cleanup. The native fixture drives the game's own ticks and uses controlled chance seeds; it does not replace the production probabilities.

The user's personal saves/configuration were not used. The complete third-party mod list and on-screen appearance of the new procedural explosion remain unverified. Restart RimWorld completely to load the new assembly.
