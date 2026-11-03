# Adamant Ascendant asset pack

Art assets integrated into the playable Adamant Earth evolution. See AdamantIntegration.md for current gameplay and validation.

## Contents

- 21 fitted regal body textures: Male, Female, Thin, Fat, Hulk, Child and Baby; south/east/north with native west mirroring.
- One inventory garment icon; no helmet. Ivory/white cloak, bronze-gold trim, amber fault seams and a flat unisex charcoal breastplate.
- Worldbreaker sword and a prepared native Melee Animation profile for EH_AdamantSword.
- Five command icons and the Evolve Adamant icon. Bestow reuses the existing shared icon without alteration.
- Six subtle head-halo frames.
- Eight frames each for Adamant Severance, Mountain King's Advance, Sovereign Quake, Worldspine Eruption, Bulwark Formation and Bulwark Fade.

There are 83 new transparent PNGs plus one reused Bestow PNG. All native exports are 256 x 256. Generated high-resolution source art, complete prompts, previews and hashes accompany the pack.

## Integration notes

`Mod/` has merge-ready texture and WeaponTweakData paths. It deliberately contains no gameplay definitions or assembly, and is not a standalone playable mod. These files are now consumed by the installed Adamant definitions and renderer.

The outfit paths are `Things/Pawn/Humanlike/Apparel/AdamantRegalia/AdamantRegalia` with normal body/direction suffixes. `EH_AdamantUniform` uses that wornGraphicPath. The weapon is `Things/Item/Equipment/WeaponMelee/AdamantSword`; the profile expects `EH_AdamantSword`. Commands are under `UI/Commands/ElementalHearts/Adamant/`. Effects are under `Effects/ElementalHearts/Adamant/Frames/`.

The established regal side correction is baked into the new east textures. Do not apply the old Azure side offset/scale again to Adamant. Native body alpha coverage, including a one-pixel margin, was checked for all 28 body/facing combinations, including mirrored west. These are pixel-placement checks; final in-game head/cloak fit and Melee Animation grips still need validation when the actual apparel/weapon definitions are integrated.

Use the existing smooth apparel manifestation and shared evolution C# sequence. No new full-screen evolution overlay is supplied. Keep halo visibility tied to transformed + drafted + standing, and start with a small restrained draw size/opacity. Bestow keeps its shared icon and paired ritual.

Effect frames retain equal atlas-cell scale and registration. Read frames in numeric order. All rings and shield formations have transparent interiors. Worldspine is anchored low in its cell; position its ground contact at the target cell rather than treating the top of the spike as its origin. Recorded visual dimensions and frame bounds are included in the manifest. The eight-frame Bulwark formation can hold its final frame while the guard is active, then play the dismissal sequence in reverse with a descending material opacity. The dismissal reuses the exact formation frames, so the plates retain their positions and the center stays transparent.

Keep cached materials, camera/fog culling, bounded effect pools, local target limits and single-impact rules during implementation. All rocks, rings and shield plates are temporary visuals.

## Provenance

All original art was produced with the built-in image generator. `GenerationPrompts.json` records the final prompts, initial prompts for revised assets, and local source-image identifiers. Export fitted the generated designs to the established regal silhouettes and native body coverage, preserved alpha, split fixed atlas cells and reduced them to native size. One-level alpha residue from atlas resampling was quantized to zero; visible translucent artwork was preserved.
