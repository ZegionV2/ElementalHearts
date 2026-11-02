# Thunderstorm Ascendant update

For the latest movement, afterimages and animated halo, see **ThunderstormBlitzUpdate.md**.

## Restored visuals

The twelve recently added Azure evolution/manifestation textures are removed from the active mod. The prior 4.5-second C# evolution choreography is restored, and Transform/Revert uses the original smooth apparel-appearing reveal with its 90-tick timing. No new transformation/evolution texture sequences are added for Thunderstorm. Its evolution ritual uses procedural red bolts/rings.

The corrected Azure side armor remains installed. New Thunderstorm regalia follows the same compact royal template: white/ivory cloak, gold trim, red lightning marks and ruby gems, with front/side/back sprites for all seven body types. No helmet, sleeves or drawn limbs.

## Thunderstorm progression and powers

A **Lightning Ascendant** unlocks **Evolve: Thunderstorm** after **150 additional unique hostile non-animal kills** and **ten ascendant days**. Previous ascendant age counts; newly installed mastery tracking does not infer earlier kills from lifetime totals. Each victim counts once across resurrection. Disciple kills are separate. The evolution is chosen by the player, interruptible and free of resource costs.

Evolution replaces the old Lightning condition with a standalone **Thunderstorm Ascendant** condition. It preserves the saved hediff identity, severity, age, original four cooldowns, automation/non-lethal preferences, passive timers, mastery and transformation kit references. Evolved gear replaces old projections while preserving quality and the pawn's stored normal clothing.

Thunderstorm receives the same power tier as Azure: movement stat factor **5x**, **+50 total ascension mood**, incoming-damage factor **0.1 before armor**, and a further **75% ward against ordinary Ascendants**. Actual movement/damage depend on other modifiers. Azure and Thunderstorm are evolved peers: neither gets that extra ordinary-ascendant ward against the other.

The four Lightning slots switch to:

| Ability | Behavior | Cooldown |
|---|---|---|
| **Crimson Sever** | Red lightning slash up to 12 clear cells, improved penetration, applies charge. Stops at walls. | 5 seconds |
| **Redline Flash** | Blitz up to 8 clear cells into melee, graze up to three secondary hostiles, strike the primary target and briefly slow a surviving successful hit. No slow on a parried/non-lethal hit. | 7.5 seconds |
| **Scarlet Tempest** | Immediate five-cell circular spinning slash with eight sequential red lightning frames; detonates Charged stacks and briefly staggers survivors. | 12.5 seconds |
| **Sovereign Thunder** | Rush into melee for a focused finisher with an 18-tick warning ring and small controlled burst. Consumes at most three charge stacks. Movement/stun/undrafting can interrupt it. | 31.5 seconds |

The fifth button is now **Tempest Dragon**, replacing Tempest Circuit. Select a hostile within ten cells and chain up to five distinct enemies, each approach using two straight zigzag legs and leaving an animated red flash line. Later targets must be within eight cells of the current landing. Each target is struck at most once. Cooldown: **18 seconds**. Auto Combat can use the chain against a single eligible hostile whenever enabled and ready. Old pending pulse zones are discarded. See **TempestDragon.md** for exact mechanics and limits.

Skills have no resource cost and retain the current 20% damage reduction, friendly-fire rules and non-lethal mode. Targets are deduplicated per pulse. Normal Lightning skills/tuning remain unchanged. Normal Lightning Ascendants teach Lightning disciples; evolved Thunderstorm Ascendants use Bestow to grant normal Lightning ascension directly.

## Gear and halo

Thunderstorm armor and sword are indestructible. The sword is bound and stored safely when downed/reverted. Downing/death restores personal clothing; use Transform after recovery. An optional Melee Animation sword profile is included.

A floating **six-frame blended red lightning halo** replaces the ordinary Lightning body aura. It appears only while transformed and drafted, awake and standing; it hides during transformation, sleep, downing and death. Existing opacity/aura settings apply.

## DEV button fix

**DEV: Evolution-Ready** remains a developer-only button for unevolved Flame and Lightning Ascendants. Pressing it raises the milestones without reducing existing progress; use the ordinary Evolve button afterward.

The command now checks dev mode every time its visibility is queried, including when the menu caches the same button instance. It also blocks input outside dev mode and hides when its pawn is no longer eligible. Turning dev mode off does not leave an active developer command behind.

## Assets, saves and validation

Generated with the built-in image_gen tool: three regalia directions fitted into 21 body variants, one inventory armor icon, one sword, six command icons and six red combat/halo effects — **35 game PNGs**, all 256x256 RGBA. The six high-resolution sources and exact prompts are in the art-source archive. Combat sprites are animated through C# timing/rotation/fading. No new evolution/transformation sprite folders are used.

An empty compatibility component keeps saves containing the removed cosmetic controller loadable without retaining its visuals. Old saved transformation timing/ownership fields remain intact. Do not disable the mod for saves containing ascendant conditions or pending abilities.

Build and automated checks passed: the existing fourteen scripts; Regal, Reversal and Evolution regressions; the new Thunderstorm regression; and the Thunderstorm sword-profile check. Tests cover cached dev-mode visibility/input, readiness/unique kills, saved component identity/cooldowns, fifth-slot persistence, helmet-free gear selection, Lightning teaching identity, +50 mood, ordinary/evolved peer defense, bounded target/pulse budgets, procedural colors, native Scribe save output, removed textures and the compatibility shim. The native Melee Animation profile deserialized and both grip anchors fall on the handle.

**Restart RimWorld fully.** Live combat, actual pawn/gear/halo fit, effect placement, optional animated grip and full native save/reload still need an in-game check. Headless checks verify native save output, not native loading.

Quick check: select a Lightning Ascendant in dev mode → press DEV: Evolution-Ready → switch dev mode off and confirm that button disappears → press Evolve: Thunderstorm → draft/transform and inspect all five skills, red halo and gear → down/recover/retransform the pawn. Repeat a normal Flame/Azure transformation to confirm the smooth apparel reveal is back.
