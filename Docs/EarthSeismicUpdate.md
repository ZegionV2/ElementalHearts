# Earth helmet and seismic techniques

The Earth kit now includes a closed basalt helmet with bronze crown, amber visor, south/east/north sprites, native mirrored west and an inventory icon. Existing two-piece kits gain one helmet in place. Uniform, sword identity/quality, personal clothes and the shared save keys are preserved.

Seismic Quake replaces Bastion's active command in saved slot 2. A 36-tick ground stab creates a four-cell radial attack: one 2.2x weapon-derived hit, a 60-tick stagger and up to two safe outward cells per eligible victim. Auto Combat chooses it against two nearby enemies. Its old 18-second cooldown and disciple scaling remain.

Spire Eruption replaces Mountainfall in saved slot 3. A 42-tick ground stab marks a fixed enemy position, then raises a temporary spiked hill. A three-cell burst applies one 4x impact per victim, consumes Resolve for the existing +10% damage per stack, staggers for 45 ticks and pushes up to three safe cells. The existing 30-second cooldown remains.

Both use generated eight-frame sequences and distinct icons. Sword motion derives from the native job's saved tick deadlines and yields to external animation control. Shared non-lethal/friendly-fire policies, local search bounds, parry prevention, cached materials, the 96-slot visual pool and camera/fog culling remain. There are still four combat buttons. No building/terrain obstacles, building damage or repeated damage pulses are created.

## Checks performed

- Compiled against the installed RimWorld 1.6 and Harmony assemblies.
- 5,592 Earth regressions: prior lineage, progression, route bounds, saved controls/cooldowns and commit fields, plus stable replacement slots, ground-stab timing and the eight-frame endpoints.
- Earth asset/definition validation: 92 RGBA textures, 21 body exports, three helmet directions, 58 distinct effect frames and native component/sound bindings.
- Shared hair/manifestation, transformation lifecycle and combat-order/navigation checks passed.
- 67 isolated native checks passed with Melee Animation loaded: transformation, legacy two-piece migration, seven body types/four directional materials, native sword drawing and restoration, real Quake/Spire injuries, knockback and stagger, wall/ally protection, non-lethal suppression, interrupted casts and automatic Quake selection.
- Native save/load tested during each windup and after a committed Spire impact. No repeated hit, stuck Earth action, duplicate projected gear or lost bound sword was observed. Revert restored personal shirt and hair.

The first headless run passed mechanics but produced base-game texture-atlas/shader startup errors without a graphics device. The final run initialized Direct3D 11. An initial fixture-only failure removing locked projected apparel was corrected to use the internal equipment-change scope. Neither issue required a production workaround.

The graphical run checked native materials and drawing calls programmatically. It did not capture or visually inspect the game window, measure frame rates or exercise every possible third-party animation. Generated PNGs were inspected separately.

## Artwork provenance

Built-in image generation supplied the helmet directions, icons and both aligned effect sheets. Generated transparency was preserved through native-size export. Final prompts and source-image identifiers are recorded in `Art/EarthSeismic/GenerationPrompts.json`.
