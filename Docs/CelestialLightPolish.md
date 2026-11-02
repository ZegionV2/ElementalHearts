# Celestial light polish

This presentation pass takes inspiration from Milira's layered flashes, expanding distortion waves, directional sparks and drop-arrival light columns. The additional geometry and animation code are original. All artwork remains the existing Elemental Hearts artwork; Milira and Ancot Library are not dependencies.

- **Dawncleaver:** an expanding golden pressure arc, short ivory glint and tapered outward sparks follow the moving crescent.
- **Sunstride:** a launch wave and directional light needles accompany the existing trail and fitted afterimages.
- **Corona Dance:** two counter-rotating pressure arcs and orbiting gold/ivory particles support the existing spinning frames.
- **Solar Judgment:** light spirals inward during the telegraph; the pillar gains an ivory core, warm outer light, staggered outward waves and a radial spark release.
- **Heavenly Zenith:** light gathers toward the miniature sun before two delayed golden/ivory waves and a short central glint accompany the existing burst.
- **Fake Rainbow:** prism glints, a short retreat wave, colored counter-dash sparks and a rainbow pressure arc reinforce the mirage/counter sequence.
- **Transform and Light Travel:** layered beam cores, gathering or rising photons, arrival rings and fading motes support the existing apparel reveal and departure/arrival frames.

The halo and sword aura retain their existing subtle presentation. The effect-opacity setting applies to every new layer.

## Limits and saves

The original 128 effect slots, 32 afterimages and 128 adornment registrations remain unchanged. New light layers are derived directly from each parent effect's sequence and elapsed age. They create no additional live effect objects, target lists, saved jobs, timers or serialized keys. Stopping, expiring or reconstructing a parent effect does the same for all its light layers.

Each map shares a maximum of **256 additional accent draws per rendered frame**, with no more than twelve photons sampled for an individual effect. Four meshes and one white glow material are created once and reused; beam highlights reuse cached Celestial frame materials. Animation uses deterministic arithmetic rather than gameplay random numbers. Camera bounds, fog and opacity cull the new layers. Meshes remain temporary visuals: they apply no damage, knockback, temperature, terrain, roof or building changes.

Only the Celestial presentation provider and its new helper change at runtime. Abilities, hit commitment, cooldowns, targeting, NPC behavior, sounds, armor, UI commands and all existing save keys retain their previous implementation.

## Reference locations

The installed Milira files inspected for timing/layering ideas are `C:/Games/RimWorld/Mods/3256974620/1.6/Defs/EffectDefs/Fleck_Visual.xml`, `Mote_Sparks.xml` and `EffecterDef_Misc.xml`. No files from that mod are distributed in this update.

Restart RimWorld to load the rebuilt assembly.

## Validation

All 43 automated verification scripts pass. A clean native RimWorld run passes 35 additional light-rendering checks, 66 Celestial gameplay checks, 307 asset/profile/placement checks and 139 audio checks. The new checks exercise all eighteen effect sequences, cached mesh topology, the shared draw cap under 128 simultaneous bursts, material reuse, opacity/off-camera culling, parent interruption/expiry cleanup and preservation of gameplay random state. The existing native suite includes mid-animation saves, counters, interrupted jobs, random Bestow, restored gear and round-trip world travel.

The isolated native fixture supplies logical viewport dimensions for headless drawing checks, acknowledges the loading-window display gate and cancels the native landing selector for automated placement tests. Melee Animation's automatic executions/lassos are disabled only in the fixture to prevent unrelated attacks between scripted assertions; its native sword profiles and animation bridge remain loaded. These test hooks are removed from the mod loader and are not part of the gameplay assembly. Native drawing and geometry are verified; direct artistic review during normal play remains separate.
