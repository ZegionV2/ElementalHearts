# Phase 4 — new effects, optional gear and automatic combat

Updated by Phase 4.2 (single-toggle full melee rotation) and Phase 4.1; see Phase4.1.md for the compatibility fixes and test results. This package includes all earlier phases. Requires RimWorld 1.6 and Harmony, no DLC. The compiled DLL is included.

## Installation
Close RimWorld and back up your save. Replace the old ElementalHearts mod folder with this folder, keeping only one installed copy. The package retains the original package ID and Def names. The delivery does not overwrite your installed mod or normal saves. Existing implanted pawns start with auto-combat off; existing cooldowns remain saved.

## New artwork
The generated ability sheet and ThunderAura-v1 sheet are integrated as 42 separate 256x256 RGBA frames: six each for Slash, Dash, Spin, Pillar, Idle, Combat and Activation. Original sheets are preserved outside the mod. With the user's authorization, export used measured row/column cuts, shared row canvases, an alpha cutoff of 8/255 and eight-pixel edge feathering to suppress background haze and neighboring-frame bleed. Frames are not redrawn. Generated sequences are stylized pose sequences, not guaranteed seamless hand-animated loops.

Sever, Flash and Dance play their six-frame sequences over their existing effect lifetimes. Heaven's 12-tick telegraph uses pillar frames 0–1; the damage event starts the peak frame 2, then frames 3–5 decay. The aura loops idle/combat frames at five ticks per frame; activation plays once over 30 ticks. Camera visibility and pool limits remain in effect. Real pawn afterimages remain separate from the new artwork. Command icons retain the previous four-effect atlas.

## Automatic combat
Draft/select an implanted player pawn and toggle Auto combat. Four extra toggles permit Sever, Flash, Dance and Heaven individually. Heaven defaults off even when Auto combat is enabled. Global settings can disable the entire automatic system or set the default for newly installed hearts.

Automatic assistance runs during a melee-attack job, drafted waiting, or an enabled Search and Destroy AI approach job. Queued player orders suppress automatic casting. Movement, work, other ability jobs, undrafted/disabled pawns and attack warmups/other occupied combat stances are respected (ordinary melee cooldowns allow assistance). It never picks a new pursuit target from a hold-position order. Auto targeting always requires a hostile, non-prisoner target even if friendly fire is enabled; collateral ability damage still follows the friendly-fire setting.

Priority:
1. Dance when at least three eligible hostile enemies are within its radius and line of sight; threshold configurable from two to eight. Idle drafted pawns can use this defensive cast.
2. Heaven, if separately enabled, against the current melee target with at least 65% health and base health scale >=1.8, or a mechanoid of body size >=1.5. This is a conservative explicit heuristic, not a universal modded-enemy threat score; many human targets will never qualify.
3. Sever against the current target when at least two hostile pawns occupy the unobstructed traced line and the selected target is in range.
4. Flash when the current melee target is over three tiles away, within range, and a valid landing exists.

Instant casts preserve the original job, driver and queued orders. Flash restores the existing path after teleporting. Heaven uses the existing interruptible job; a new movement order interrupts it without refunding the cooldown. A manual cast postpones automatic decisions for one second. Manual casting and passive mechanics remain available when auto-combat is disabled.

## Optional cosmetics and equipment
- Lightning uniform: sleeveless middle-layer torso/shoulder clothing, made at tailoring benches with Complex Clothing, 60 cloth, 20 steel, Crafting 3. It grants modest ordinary clothing protection, no implant bonuses. Directional textures exported for Male, Female, Thin, Hulk, Fat, Child and Baby. Fits are scaled adaptations of the supplied artwork, not independently illustrated body variants.
- Lightning sword: ordinary longsword damage/cooldowns with the supplied lightning design. Made through the native smithing recipe after Long Blades, 100 metallic stuff, Crafting 5. No inherent lightning proc; an implanted wielder supplies that power.
- Lightning hair: native HairDef. A Lightning hair button on selected implanted pawns applies it; pressing again restores the previous style, including after save/load. Uses normal pawn hair-color tinting, so colors can darken. The style also participates in native Urban/Soldier hair selection. No Ideology styling station is required for the toggle.

Equipment never gates any ability. The hair toggle only changes appearance. Removing the implant restores the previous hair when the Lightning hairstyle is still active. An externally changed hairstyle is preserved.

## Settings and performance
Settings: friendly fire; global auto enable; default auto for new implants; Dance threshold; aura visibility; afterimages; effect opacity. Cosmetic controls do not change damage, targets or cooldowns. Balance multipliers remain in Defs/LightningHeart.xml.

Phase 4.1 adds one Def-specific DrawColor postfix for the custom uniform and sword, alongside the three Phase 3 combat postfixes. Auto evaluation uses the registered implant-pawn list: at most four pawns every six ticks, each no more often than every 30 ticks. Target selection is not run every tick. Aura draws cap at 64 pawns, transient visuals at 96 slots/map, chain queues at 32, counters at 64, and due chain searches at eight every six ticks. Materials and meshes are cached; effect slots and completed combat queue objects are reused.

## Compatibility and acceptance
Core-only loading and native APIs are checked against the installed 1.6.4871 game. Combat Extended and animation overhauls are not installed in this test environment, so compatibility with them is NOT certified. CE's armor, damage and melee changes may require a dedicated patch; the ordinary-melee and dodge hooks retain the limits documented in Phase3.md. Standard five adult body types have exported fits; modded body/head shapes may need dedicated textures. No DLC is required; optional child/baby graphic exports are not a DLC integration claim.

Run these checks on a copy of your colony:
1. Load a Phase 3 save: check implant bonuses, four manual abilities and unchanged cooldowns; confirm auto is off.
2. Cast all four abilities. Check frame direction, transparent edges, pawn visibility and Heaven's impact timing. Check aura idle, drafted and activated, then pan offscreen and back.
3. Equip the uniform/sword and toggle hair. Inspect south/east/north/west, all body types you use, hats, other apparel, sword grip and hair color. Save/reload and restore hair.
4. Enable Auto combat, issue a melee attack beyond three tiles, and observe Flash/resumed melee. Check Dance while surrounded and Sever with aligned enemies. Issue a movement order and confirm it wins.
5. Leave Auto Heaven off, then enable it against a large healthy threat. Interrupt its telegraph with movement. Confirm cooldown remains spent.
6. Change each visual setting while paused and during combat. Compare a crowded encounter at low/full opacity; no combat result should depend on visuals.
7. Remove the implant; check no remaining abilities, auto actions or aura. Test your actual combat/animation mod list separately and report the first error plus trigger.

Content-mod removal from a save cannot be guaranteed safe. Restore custom hair and remove custom gear/items, implants, marks, bills and queued abilities first, including on off-map pawns; preserve a backup. Phase 4 adds saved custom gear/hair Def references.

The actual Phase 3 test save upgraded successfully, preserving charges, cooldowns and queued combat. An isolated native camera render checked the four effect peaks and five adult pawn fits; terrain blending, full playback and equipment interactions still need acceptance in your colony. See Phase4RuntimeChecks.txt for executed tests and their limits. Older phase documents remain historical records.


