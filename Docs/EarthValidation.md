# Earth update validation

Validated against the installed RimWorld 1.6.4871 game assemblies and Harmony.

## Build and automated checks
- Source/Build.ps1 compiled the included Assemblies/ElementalHearts.dll successfully.
- 40 runnable verification/regression scripts passed, including the new VerifyEarth.ps1 and Tests/RunEarth.ps1.
- Existing checks cover ascension, awakening, teaching/Bestow, non-lethal policy, combat orders, equipment ownership, transformation, hair transitions, and the other playable lineages.
- The Earth regression exercises native lineage objects and Scribe save output plus Resolve expiry/limits, defense multiplication, disciple cooldowns, promotion state, manual-target isolation, movement bounds and action replacement.
- Earth schema/assets checks validate native definition fields, component bindings, serum/recipe/job definitions, normal-tier mood, 70 RGBA 256px textures, all 21 body/direction garment exports and 42 distinct effect frames.
- The older projection-count check was updated from 25 to 27 to include stoneplate and Faultbreaker.
- NPC awakening expectations now include five elemental choices while retaining the same overall 20% default spawn probability.

## Native game smoke test
A disposable generated colony used isolated save/settings folders and a separate test-only mod copy. The user's installed ElementalHearts folder and existing saves were not changed.

Actual RimWorld execution passed:
- Earth whole-body ascension, helmetless transformation, stoneplate and bound Faultbreaker.
- Cleave damage, Resolve gain and cooldown start.
- Clear-route Advance to an adjacent melee flank.
- Bastion activation without replacing combat control, plus a 0.90 allied protection factor.
- Mountainfall accepted and genuinely in its wind-up before saving.
- A real save loaded in a fresh game process.
- Mountainfall resumed, consumed Resolve, kept its cooldown, and released its job.
- Earth identity, saved automation preference and bound sword survived loading.
- Automatic Advance followed by Cleave from a melee combat order, with control released afterward.
- Non-lethal Earth Cleave added no wound damage.

The native test found that manual techniques could inherit a prior melee recovery stance. Accepted Earth casts now cancel that old stance, matching the existing normal lineage approach.

## Limits
Five optional checks could not run because the local Melee Animation assembly was absent:
VerifyAzureSword, VerifySwordAnimations, VerifyThunderstormSword, VerifyWaterDefense and VerifyWaterSword.
Earth includes an optional sword profile; both exported grip anchors lie on opaque handle pixels, but the external loader/animation pairing remains unverified.

The native smoke test used batch/no-graphics mode. It does not validate subjective in-game animation quality, armor appearance with every modded pawn type, or frame-rate performance.
That mode emits Unity shader/texture-atlas warnings and exceptions. Existing Azure/Thunderstorm crafting-definition warnings also appeared; they are outside this change.
Static image inspection and native-sized coverage exports supplement the headless test, but do not replace a graphical playtest with the player's mod list.

Adamant, head halos, new recorded sounds, balance against every mod combination and a performance benchmark are not included.

