# Illustrated guide and damage-description validation

The current guide documents all thirteen playable forms, acquisition, disciples, promotion/evolution, Bestow, combat controls, rare lineages, damage, NPC awakening and optional integrations. Historical plans remain separately identified under `Docs`.

This documentation pass updates thirteen lineage health-condition descriptions, seven damage-type descriptions and the mod-loader overview. A structural comparison against the installed build, ignoring description text, confirms all gameplay definitions are unchanged. The compiled runtime assembly is unchanged by this pass.

Validation completed:

- 86 checks covering XML structure, unchanged numerical definitions, all thirteen factor/tuning descriptions, unchanged runtime assembly, and the guide's local links/images.
- The full current C# source compiles against the installed RimWorld 1.6 and Harmony assemblies in a separate build-check folder.
- Four actual native map-camera screenshots captured and visually inspected: complete roster, Heavenly Zenith, Crimson Waltz and Glacial Explosion/freeze. These are staged scenes with durable test opponents, not damage benchmarks.
- The isolated screenshot save loads the current mod and completes those showcase actions without a logged game exception. Regular saves/settings are not used. Test assemblies and save data are excluded from the published mod.

Damage examples distinguish lineage Melee damage factors from skill coefficients and already-adjusted profiles. The existing common injury multiplier remains 0.8; no damage, cooldown, range, save key or ability behavior is changed by this documentation pass.

The GitHub refresh also includes the previously implemented Eclipse, elemental resistance/shock and Lunar/Eclipse presentation updates already present in the installed mod. Their dedicated native validation reports remain in this directory; the screenshot run does not replace those reports or claim a fresh full combat regression.
