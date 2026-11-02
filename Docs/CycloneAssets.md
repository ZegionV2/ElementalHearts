# Cyclone Ascendant asset preparation

Status: **Cyclone is now fully playable.** See [CycloneIntegration.md](CycloneIntegration.md) for progression, techniques, shared controls and save behavior. The remaining sections record the original asset preparation and its bounded presentation interface.

## Artwork

The set contains **97 new transparent 256×256 runtime PNGs**:

| Asset | Count |
| --- | --- |
| Regal armor: Male, Female, Thin, Fat, Hulk, Child, Baby × south/east/north | 21 |
| Armor inventory icon | 1 |
| Emerald-channel sword | 1 |
| Five skill icons and one evolution icon | 6 |
| Draw trail, rising spiral, counter sweep, finisher slash, cyclone formation, cyclone loop, release burst: eight frames each | 56 |
| Airbound ribbon/glint loop | 6 |
| Subtle wind-ring halo | 6 |

The white/ivory short-cloaked outfit uses emerald/mint wind-ribbon inlays, gold trim, low neckline and closed shoulder/back coverage. It has no helmet or drawn limbs. Each body/direction sprite is registered to the accepted fitted regal template; west uses RimWorld's native east mirroring. Original selected atlases and the prompt set are supplied with the asset package. The built-in image generation tool produced the artwork; technical extraction, fitting, resizing and packing prepare the game files.

Bestow uses the **existing shared icon unchanged**. The 68 animation frames retain equal-cell registration and shared scaling within each sequence. The counter sweep visibly rotates, formation builds up, the tornado loop cycles wind bands, and release expands and fades. The small airbound loop is positioned around each pawn by the renderer.

## Gear and optional Melee Animation

`EH_CycloneUniform` and `EH_CycloneSword` are real prepared gear definitions with the existing shared indestructibility extension/components. They are integrated into Cyclone transformation. Cyclone evolution now grants access to their shared manifestation kit.

The sword has a native profile at `WeaponTweakData/EH_CycloneSword_elementalhearts.lightningheart.json`. The long ivory grip is centered at x128; the primary animation hand is at y188 and the second near y219. Uniform scale1.2 and a90-degree rotation align it with native melee animation space. Its blade sweep projection runs from texture row148 to row21, accounting for the curved tip. Long sharp/stab animations and both hands are enabled. All seven established sword profiles and existing artwork are unchanged.

## Visual provider contract

`CycloneVfx` supplies cached icons, frame materials and fitted directional body materials. Its map component is presentation-only and is inactive until a caller emits or registers visuals. The separate Cyclone gameplay code owns its jobs, saved stages, targets, movement, damage and temporary action holds.

- `Emit(owner, effect, from, to, duration, size, follow, ageOffset)` emits a one-shot or looping effect and returns a token. Save stage timing in the future ability job; reconstruct an effect with `ageOffset` after loading rather than restarting it. Already-expired effects are rejected.
- `Stop(token)` stops an owned effect and is safe to call repeatedly. Store tokens for ground-centered casts so interruption can stop them explicitly.
- `Ghost(pawn, position)` emits a short-lived fitted-regalia afterimage, including native west mirroring.
- `TrackAirbound(pawn, absoluteExpiryTick)` follows a pawn with the six-frame ribbon loop. Repeated registration reuses that pawn's entry. This draws ribbons only; it does not lift, stun, move or injure the pawn.
- `TrackHalo(pawn)` registers a halo candidate. Drawing additionally requires the `EH_CycloneAscendant` lineage, transformation, drafting and a conscious standing pawn, outside a transformation transition. Ordinary Wind does not acquire a Cyclone halo.
- `Forget(pawn)` releases followed effects, airbound overlays and halo registration. Unfollowed floor wisps naturally expire, or can be stopped by their tokens.

Budgets per map: **128 effects, 32 afterimages, 64 airbound followers and 128 halo registrations**. Transient effects are capped at300 ticks and12 cells of draw size. All pools overwrite bounded old slots rather than growing indefinitely. Afterimages last14 ticks. Materials are cached; draws use one reusable material property block. Effect opacity, afterimage/aura preferences, camera bounds and fog are respected. Halo and airbound opacity are deliberately restrained.

The provider does not spawn pawns, buildings or projectiles; change terrain, roofs, weather or wind; alter health; issue jobs; or apply damage. Floor wisps are temporary draw calls. Its arrays are cosmetic and are not gameplay save state. The gameplay implementation restores live visual timelines from saved job/controller state.

## Verification

The update passes **38 automated scripts**, including all earlier regression checks, an eight-sword native-parser/handle check, asset/definition validation and visual timing/eligibility tests. All97 textures are transparent and correctly sized; all68 sequence frames are distinct.

An isolated game with Melee Animation enabled passes **194 native checks**: all seven body types in four directions, material caching, every icon/frame, the shared Bestow icon, all eight sword profiles and their attack selections, bounded emissions, repeat-safe stopping and registration, off-screen culling and expiry through native game ticks. A1000-emission stress check stays within128 effect slots, and100 afterimage emissions stay within32 slots.

The full third-party mod list and on-screen placement are not visually/play-tested. Personal saves/settings are not used. Restart RimWorld completely to load the new visual provider and prepared gear definitions.
