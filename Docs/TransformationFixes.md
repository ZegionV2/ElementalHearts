# Transformation fixes

- Removed the Head render-skip flag from all three helmets. Core's human render tree places ApparelHead beneath Head; the helmet now retains that parent branch while hiding Hair and Beard.
- Lightning and Flame sword sprites face the same direction as Wind. Transparent 256x256 placement and existing two-hand grip checks are preserved.
- Transform/Revert now starts EH_Transform, a dedicated job. The pawn stops pathing and faces South (front) for the 90-tick sequence. Normal player/casual interruptions are disabled. Heart automation remains paused. On completion, normal job selection resumes; forced interruption or incapacity clears the pending transformation timer without locking the pawn.
- Existing saves with an old running timer and no transformation job cancel that timer safely; already equipped items are retained.

Validation: assembly compiled; main definition validation, transformation validation, and installed Melee Animation sword checks passed. No live-game rendering or job test performed. Fully restart RimWorld to load the updated definitions, textures and assembly.

Backups: ../../work/transform-fixes-backup. Generated sword sources retained in Sources.

Transformation pulse: the current pawn atlas silhouette now pulses in its elemental color during Transform/Revert. The 90-tick envelope reaches full color at the 60-tick equipment reveal and fades before completion. The effect honors effectOpacity, does not recolor pawn/apparel data, and is tracked only while transforming. Cached textures/materials are reused; dirty atlas frames are skipped until refreshed normally. No texture readback or generated sprites are required.

Validation: assembly build, 101 pulse timing/boundary samples, reveal peak/end fade, distinct color checks and transformation validation passed. Visual rendering still needs in-game confirmation, especially alongside other pawn-rendering mods.

## Live silhouette correction
The earlier atlas overlay has been replaced. During transformation, Core's cached rendering is bypassed for that pawn only. The main-thread draw pass temporarily selects CutoutWithOverlay materials with a full white mask for the live body/head/apparel meshes. A per-draw property block supplies elemental color and pulse strength; an exception-safe finalizer restores every original material. This avoids reliance on stale atlas frames and preserves original sprite alpha. Portraits and invisible pawns are excluded. No gameplay color or apparel data is changed.

Repeated activation/aura bursts are removed from the transformation sequence, and normal auras stay hidden while transforming. Lightning helmet front/side eye lenses are now cyan-blue; compact aspect-preserving sizing is retained.

Checks: local build, pulse timing samples, transformation definitions/textures and live-render API bindings passed. The actual appearance still needs a running-game check after a full restart.

## Transformation equipment lock
While alive and transformed, the pawn's worn apparel is reported as locked through the native apparel lock API. Normal apparel drops, weapon drops/transfers, outfit optimization and standard Wear/Equip/RemoveApparel/DropEquipment jobs are blocked. Player orders receive a message to revert first. The lock uses the saved transformed state, so no separate lock flag can become stranded after load. Transformation's own Stow/Equip operations use a try/finally bypass, preserving migration and Revert. Death releases this extra lock; destruction and damage paths are not intercepted. Mods that directly manipulate item containers can bypass the normal game APIs.

Validation: build, eight lock lifecycle cases, ten job policy cases, patch signature checks and transformation checks passed. No live incident or gear-tab test was performed.
