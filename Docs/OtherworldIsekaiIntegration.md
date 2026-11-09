# Otherworld Calling — ISEKAI compatibility design

Status: implemented optional compatibility bridge, verified in native isolated saves. Inspected on 2026-11-07 against installed ISEKAI RPG LEVELING v1.1.8.3 (`JellyCreative.IsekaiLeveling`). Do not bundle or modify that mod's DLL, art or definitions.

## Promised bundle

One identity plus two distinct talents. Identity weights: Summoned Hero 60%, Protagonist 25%, Regressor 15%. Pick at least one talent from Predator Instinct, Undying, Lucky, Craftsman's Soul or Healer's Touch for an immediate benefit; the other may be any compatible talent below. If ordinary traits prevent a selected identity, selection falls back to the bounded compatible bundle pool.

| Native identifier | Benefit in the inspected version |
| --- | --- |
| `Isekai_SummonedHero` | 3× progression XP; +1 stat point/level; mental break threshold −0.10; social impact +0.25; native −8 displaced mood drawback |
| `Isekai_Protagonist` | 10× progression XP |
| `Isekai_Regressor` | 4× progression XP; free constellation respec |
| `Isekai_NaturalTalent` | One additional constellation star point every five levels |
| `Isekai_Prodigy` | 2× progression XP; research speed ×1.5 |
| `Isekai_PowerSpike` | Five additional stat points every ten levels |
| `Isekai_PredatorInstinct` | Melee hit and shooting accuracy offsets +0.15; native trap avoidance benefit |
| `Isekai_Undying` | Native lethal-hit survival mechanic with a three-day cooldown; pain shock threshold +0.20; native mood consequence after activation |
| `Isekai_Lucky` | Surgery +0.15, shooting +0.10, trade improvement +0.10; native +5 mood |
| `Isekai_CraftsmanSoul` | General work speed +25%; native crafting-quality improvement chance |
| `Isekai_HealerTouch` | Tend quality +0.30, surgery success +0.25; native immunity-gain benefit |

These are ISEKAI progression XP multipliers, not promises of the same multiplier to vanilla skill XP. Native offsets remain offsets, not guaranteed final probabilities. The mod retains ownership of caps, stacking, cooldowns and actual trait behavior. In the inspected helper, identity and growth XP multipliers multiply, so Protagonist + Prodigy provides 20× progression XP.

## Source inspected

- `Defs/TraitDefs/IsekaiTraits.xml`: real defNames, descriptions, conflicts and stat modifiers.
- `Source/IsekaiLeveling/Core/IsekaiTraitHelper.cs`: `AddIsekaiTrait(Pawn, string)`, `GetXPMultiplier`, one-time effects and conflict checks.
- `Source/IsekaiLeveling/Items/CompUseEffect_IsekaiTrait.cs`: native item granting path checks the progression component and calls `AddIsekaiTrait`.
- `Source/IsekaiLeveling/Core/IsekaiComponent.cs`: cached component access, initialization and saved progression state.

## Optional bridge, not a hard dependency

Detect the active package, real loaded type `IsekaiLeveling.IsekaiTraitHelper`, compatible static grant method, all required TraitDefs and a live initialized ISEKAI component before charging. Cache reflected methods once. Keep ISEKAI types out of Elemental Hearts' mandatory class signatures and startup loading. If the contract is absent, disable only Otherworld Calling with a specific reason.

Grant through the native helper; do not instantiate raw Trait objects as a substitute. The helper checks both forward and reverse conflicts, prevents duplicates and applies one-time effects. Its Boolean return alone is insufficient: verify the final real trait list and initialized progression component before committing the pawn/payment. Do not reapply native one-time effects after loading.

Select a complete compatible bundle before mutation. Limit candidate enumeration to the fixed three identities and eight talents. Preserve unrelated traits, remove only incompatible ISEKAI-generated archetypes/curses on this newly generated summon, then grant the selected bundle once. Track successful steps so a controlled pre-commit failure can clean up its owned additions without touching unrelated traits. Fail cleanly and refund escrow if the promised bundle cannot be formed. Never run an unbounded generation/reroll loop.

Explicitly prevent more than one selected identity even where the installed XML does not make every possible identity pair mutually exclusive. Check every selected trait pair and both directions against existing traits. Save the fixed bundle and the real pawn once; recovery uses that same person. The source XML may change, so rerun compatibility validation against the active definitions at runtime.

Do not force `Isekai_Godhood`, `Isekai_Reincarnated`, rank traits or title traits. Godhood is an earned external progression reward; Reincarnated resets progression; ranks and titles are managed by ISEKAI. Do not duplicate Undying's death interception, medical modifiers, XP awards or rank refresh logic inside Elemental Hearts.

## First implementation checks

Test all three identities and all eight talents, pairwise conflicts, preexisting negative ISEKAI choices, helper failure, component initialization, missing/removed ISEKAI mod and post-load rank refresh. Confirm exactly three promised compatible traits, no identity stacking, no automatic elemental lineage, no pre-commit payment loss and no save/load reroll. The installed definitions can impose their native drawbacks; do not silently rewrite them.
