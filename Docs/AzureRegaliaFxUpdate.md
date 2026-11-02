Current visual rollback: the dedicated evolution/transformation textures below are removed. The corrected side armor is retained and the earlier smooth apparel reveal is restored. See ThunderstormUpdate.md.

# Azure side-profile armor and regal manifestation update

## Side-facing regalia

Azure's seven **east-facing** body textures are replaced with a right-facing side profile. One shoulder, a clasp line at the right/front edge and the white cloak down the left/back match the supplied side-view reference. The ivory panel under the shoulder is closed and opaque, removing the old armhole/side gaps. Textures are fitted slightly higher and wider against the ordinary Flame uniform bounds to cover the torso. West-facing pawns use RimWorld's native mirror of the east view.

Front and back armor textures, the front alignment correction, inventory icon and sword remain unchanged. Ordinary ascendant armor is unchanged. The new high-resolution east view is saved as `Docs/Templates/Evolution-Regalia-East-Master.png` for future evolution outfits. No helmet or limbs are added.

## Evolution effects

Six new generated textures support the existing 4.5-second C# evolution ritual, replacing its reused attack-effect textures:

| Index | Texture | Choreography |
|---|---|---|
| 00 | Ember gathering sigil | Warm orange/gold ground seal and inward gathering |
| 01 | Azure spiral | Rising and orbiting blue flame filaments |
| 02 | Ascension column | Narrow blue/white upward release |
| 03 | Convergence core | Pre-completion focus and the finishing core flash |
| 04 | Rebirth bloom | Outward curling blue flame petals at completion |
| 05 | Shockwave ring | Expanding blue afterglow ring |

Texture folder: `Textures/Effects/ElementalHearts/Azure/Evolution`. The C# controller animates their positions, rotation, scale and alpha alongside its cached ring geometry. These effects remain cosmetic.

## Unique evolved transformation

Azure's ordinary **Transform** button now uses a separate **two-second** regal manifestation, instead of the ordinary ascendants' 1.5-second effect:

1. A blue/gold royal seal ignites under the pawn.
2. Twin flame ribbons weave around the torso. The regalia appears **from the center outward**, followed by the cloak, while normal clothing is masked away by the complementary reveal.
3. A sword-forge beam appears beside the pawn and a sovereign lock flash finishes the manifestation.
4. The effect settles away. Reverting uses blue/gold dissolution embers and an outward cloth/flame sweep.

Six dedicated generated textures are loaded from `Textures/Effects/ElementalHearts/Azure/Manifestation`. Ordinary Lightning, Flame and Wind keep their existing animation, timing and bottom-up clothing reveal.

The existing transformation job waits for the actual finish signal. Azure's projected equipment is swapped only once the visual reveal is complete, with the same saved item ownership, original clothing storage, locks and downed reversion. Transformation duration is saved explicitly; older in-progress saves default to their prior 90-tick timing. The head halo still appears only when transformed and drafted, after the transformation has finished. No new skill-menu buttons or extra headgear are added.

## Verification and installation

Built against installed RimWorld/Harmony. New tests cover both 90/120-tick timing paths, monotonic reveals/reversion, complete reveal before item swap, phase alpha bounds, center-out mesh area/UV preservation and complementary clothing masking, bounded/deduplicated effect tracking, native held/facing toils and native Scribe save output. Existing regression suites and verification scripts are also run.

All 19 new/replaced game PNGs are transparent 256x256 RGBA: seven side body variants plus twelve VFX assets. All seven side textures have an opaque central torso panel. Other existing textures are byte-identical. Created using the built-in image_gen tool; the three source images and exact prompts are provided in the art-source archive.

**Restart RimWorld fully.** Actual pawn fit with your body/rendering mods, live effect placement, transition clipping and full save/reload still need an in-game check. Inspect a transformed Azure pawn facing east and west, Transform/Revert repeatedly, and down it during the animation. Its normal clothes should return and the same bound kit should remain stored. Native loading/GPU rendering are not covered by the headless tests.
