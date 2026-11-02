# Transform / Revert hair correction

Hair and beard now follow the progressive helmet reveal during Transform and Revert. Previously, apparel was clipped gradually while RimWorld hid hair from the helmet's actual worn state. Reverting could leave the head bare until the equipment swap, and transforming could leave hair fully visible underneath a partially manifested helmet.

The rendering change allows native eligible hair/beard requests during a transition between concealing and non-concealing headwear, then clips them to the complementary helmet region. It keeps the same head reveal timing and draw matrices. Coverage endpoints are now explicitly zero/full to avoid a floating-point sliver at full helmet coverage in the game's Mono runtime.

Personal headwear remains authoritative: reverting into a concealing hat does not force hair visible. A helmetless evolved outfit with no headwear transition keeps native hair behavior. Caller-supplied hair masks, unrelated worn headwear, portrait/statue rendering and hidden-headgear mode retain their own visibility rules. Other rendering mods may impose additional rules and need an in-game compatibility check.

This change does not assign a new hair/beard definition, alter gear ownership, add save fields or change transformation durations and job controls. Existing transform save keys and the legacy hair migration remain. The same rendering path serves the normal elemental helmets and evolved regalia.

## Verification

The DLL compiled against the installed RimWorld 1.6/Harmony assemblies. All **32 automated verification scripts passed**, including the existing Water, evolution, Bestow, equipment and combat regressions. The new fixture checks native concealment semantics, all 128 mask scopes, helmet-to-bare and helmet-to-helmet policies, and the complementary head-reveal curve.

An isolated native RimWorld harness loaded a previously generated test save and passed **62 render assertions**. It sampled both transition directions at fixed animation states, before and after the real equipment swap. Native meshes/materials confirmed hair and beard visibility, partial clipping, retained draw matrices, restored draw-request lists, unchanged style definitions, caller masks, portrait behavior, hidden-headgear behavior and cancellation cleanup. These are renderer-state checks, not visual screenshots or a full time-driven animation recording. No personal save was loaded. Third-party rendering compatibility and subjective visual appearance remain unverified.

The temporary QA companion is excluded from the playable mod and removed from the Mods folder after testing. Previous changed production files are backed up in Backup-before-Transform-Hair-Fix. Install-Manifest.json records the changes; no production files or artwork are removed.

Fully restart RimWorld. Check Transform and Revert on a pawn with visible hair, then on one whose personal outfit includes a hat. Inspect the early head reveal, the equipment swap and the end of the animation.

## Glacial evolution plan

Glacial-Ascendant-Plan.md is a design document only. It proposes Water's five-skill ice evolution, the shared white regal outfit without a helmet, a fractured-ice halo, progression, Bestow Water and a save-preserving implementation order. Glacial powers are not enabled by this hair fix.
