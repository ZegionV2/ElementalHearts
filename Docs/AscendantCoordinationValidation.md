# Coordination, side-fit and Adamant validation

The installed build includes slimmer/recentered Lunar and Eclipse side textures, standard evolved-tier Adamant values, and allied synchronization. Source compiles against the installed RimWorld 1.6 and Harmony assemblies; no dependency or save-key rename is introduced.

## Results

- **359 static checks:** XML parsing, definition uniqueness, unchanged gameplay definitions outside the intended Adamant correction, UTF-8 documentation and links, side registration, coverage and slimming.
- **28 side coverage checks:** two families × seven native body textures × east/west. Required native body pixels remain opaque; collar drift stays within three texture pixels. Torso profiles are approximately 12–16% narrower and their centers match the native torso band within half a texture pixel.
- **97 native integration checks:** all thirteen playable lineages, eight strike implementations and Returning Tide, existing-packet Flame/Wind scaling, Water recovery, Earth ward, conductive shock/recovery, non-lethal control, Glacial Explosion, blocked lines, non-allies, reversion, despawning, bounded cooldown capacity, exception cleanup, UI commands, cached effect meshes and native save/load at primer, pending-support and active-buff stages.
- **Seven separate live-clock combat checks:** autonomous Flame/Wind fighters synchronize through actual combat, share the cooldown, synchronize again after it, refresh paired buffs, expire their bonuses and remain able to fight with clean scope/counter state. This independent run uses the normal game clock rather than manual single-tick simulation and logs no game exceptions, missing references or render-node errors.

The native stress fixture performs many staged actor/health changes and reloads in one rendering frame. It records native render-cache reset warnings during those test transitions; those warnings are not hidden or counted as passed checks. The separate live-clock run covers ordinary rendering/combat without those fixture transitions and is clean.

## Preservation

Adamant's intended changes are the melee factor, melee cooldown, incoming factor, bound sword powers and recovery/sealing values documented in [the tier guide](MoonSideFitAndAdamantBalance.md). Normal Earth, other lineage base values, old hediff/job/gear IDs, saved cooldowns, mastery and transformation keys remain unchanged. New synchronization state uses separate `EH_concord...` map fields and short-lived health conditions.

Only fourteen east apparel sprites change. Front/north sprites and native west mirroring remain. The native side-view comparison is a crop/layout of actual before/after game renders, not generated concept art. [Comparison](Images/Side-Fit-Comparison.png).

Synchronization adds no combat-menu buttons. It has one default-on mod setting, non-stacking short buffs, bounded saved state, bounded transient links, cached meshes/material and camera/fog culling. It introduces no extra damage packets, permanent terrain effects or world/weather simulation.

All test saves and harness assemblies run separately from regular colony data and are excluded from the installed mod. The temporary test mod is removed after verification. The native assertion transcript is retained in [the validation record](Validation/AlliedSynchronizationNative.txt); the independent combat transcript is [recorded separately](Validation/AlliedSynchronizationLive.txt).
