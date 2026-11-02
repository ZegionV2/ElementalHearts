# Compact Azure regalia update

Only **Azure Flame** receives this outfit. Ordinary Flame, Lightning and Wind uniforms are unchanged. The new white/ivory cloak, gold trim and azure accents use a compact RimWorld torso silhouette: low neckline, tucked shoulder folds, no neck tube, extended sleeves, arms, hands, legs or helmet.

The canonical front design is `Docs/Templates/Evolution-Regalia-Master.png`. Use this silhouette, short white cloak and clasp/trim arrangement as the base for **future evolved forms**, replacing the blue flame motifs and gems with the relevant elemental colors. It does not activate outfits for other evolutions. RimWorld mirrors the east texture for west-facing pawns. All seven body types have front, side and back textures; the inventory icon is separate.

Azure's halo appears only while **transformed and drafted**. It also hides during transformation, sleep, downing and death. Existing visual settings still apply.

Evolution now changes the existing Flame condition into the standalone **Azure Flame Ascendant** condition. The saved hediff identity, age, severity, ability cooldowns, automation/non-lethal settings and transformation component are retained. Existing saves containing both the old Flame and Azure conditions are consolidated when loaded. The base Flame hediff no longer remains alongside Azure. Azure retains its Flame lineage for teaching and passives.

The standalone Azure condition carries the complete bonuses directly: movement factor **5x**, incoming-damage factor **0.1** before armor, +50 total ascension mood, and the additional ward against ordinary Ascendants. Actual movement and damage depend on gear, capacities, terrain and other modifiers.

**Controlled Pyre**, Azure's circular flame attack, now reaches **six cells from the center**, with a **twelve-cell-wide** effect. Its targeting preview, pulse range and visual diameter agree. The three pulses still share one damage budget. Ordinary Flame's Inferno Waltz remains at its existing four-cell radius.

## Verification

- Compiled against the installed RimWorld 1.6 and Harmony assemblies.
- All 14 existing verification scripts passed.
- `Tests/RunRegal.ps1` passed: preserved hediff/component state, repeat-safe rebinding, standalone Flame lineage/teaching identity, all 16 halo policy combinations, DEV readiness, defense scope, mood, paired teaching toils, evolution thresholds, pyre budgets, bound swords and manifestation inventory handling.
- Native Scribe save output was verified; native loading was not exercised by these headless tests.
- All 22 new armor PNGs are 256x256 RGBA with transparent corners. Original ordinary uniform textures are byte-identical to the prior installed version.

**Restart RimWorld fully.** Live gameplay, actual pawn fit, the evolution/migration sequence and full save/reload still need an in-game check. Inspect a transformed Azure pawn from all four directions, draft/undraft it, revert and transform again; the halo must follow those states and the armor must sit below the head. Load an existing evolved pawn and confirm only Azure Flame Ascendant remains while its cooldowns and powers persist.

## Art provenance

Generated with the built-in `image_gen` tool using the user's compact torso outfit as the silhouette reference and the earlier white regal Flame design as the palette/layout reference. The three final source outputs and exact prompts are supplied in the art-source package. The north source contained a front/back pair; only its right-hand back view was used. Generated alpha was preserved when fitting the seven body types. No sword art changes are included in this update.
