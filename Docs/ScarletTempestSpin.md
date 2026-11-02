# Scarlet Tempest — spinning attack

Later update: Tempest Circuit has been replaced by the Tempest Dragon zigzag chain. See **TempestDragon.md**.

Scarlet Tempest now uses the same immediate circular attack path as Lightning's spinning attack. It strikes eligible pawns within five cells and clear line of sight once, detonates the caster's Charged stacks and briefly staggers survivors. Base damage multipliers remain 1.1 physical + 1.4 electrical, before existing modifiers and charge bonuses. Cooldown stays 750 ticks (12.5 game seconds). Existing targeting, non-lethal, armor and parry rules apply. There is no new button or resource cost.

Eight generated transparent frames show the bright leading slash rotating clockwise around the pawn. Each frame plays for three ticks, for a 24-tick sequence, with a short tail fade. All frames use the same crop, center and scale. The attack uses the existing fixed 96-effect pool, with eight cached materials and the existing opacity, camera and fog checks. No animation job or persistent busy flag is added.

Scarlet no longer leaves a three-pulse storm at its cast location. Any pending old Scarlet storm zones in an existing save are discarded on the next map tick. Tempest Circuit remains a separate targeted three-pulse zone and retains its saved pulses. Other Thunderstorm attacks, the halo, evolution and apparel reveal are unchanged.

The assembly compiled against the installed RimWorld 1.6/Harmony assemblies. Seven relevant automated verification scripts passed, including the new Scarlet regression, Thunderstorm, blitz, Def validation, combat flow, non-lethal and targeting refinements. The new check verifies compiled production calls use immediate strike/charge logic without a storm queue, all eight frames display at the intended cadence, legacy zone cleanup preserves Circuit, and existing range/cooldown/base tuning remain. An older combat-flow verifier needed Windows PowerShell's .NET Framework host rather than PowerShell 7; its supported-host run passed.

Live combat, GPU rendering and native save loading were not exercised. Fully restart RimWorld, draft a Thunderstorm Ascendant, cast Scarlet near several enemies and inspect the clockwise red slash, immediate damage and cooldown. Check a wall blocks hits, cast Circuit separately and verify its pulses still work. Toggle opacity to confirm the effect respects it.

Art generated with the built-in image_gen tool. ArtSource contains the original eight-frame sheet and exact prompt. GameSprites contains eight 256×256 RGBA frames. The preview is an asset contact sheet, not a game screenshot.
