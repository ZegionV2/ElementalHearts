# Elemental Ascendants

Empowered pawns are now named Lightning Ascendant, Flame Ascendant, Wind Ascendant and Water Ascendant. The mod remains Elemental Hearts, with its existing package and internal definition IDs retained for save compatibility.

## Injectable serums

All four former implant items now appear as ascension serums, with newly generated syringe textures. Select a pawn and use the serum's Inject action for self-administration, or use the corresponding administer-serum bill in a patient's Health/Operations panel. One dose takes three seconds at base speed and grants a permanent whole-body condition. No organ is replaced and no anesthesia is required. Existing Ascendants cannot receive another elemental dose; a refused dose is not consumed.

Existing saved elemental conditions are moved from their body part to Whole body on load without recreating them. Ability components, cooldowns, combat settings and progression remain on the same condition. Existing loose implant items resolve as the new serums. The old removal operations are unavailable: ascension cannot be extracted into a reusable item. Teaching, abilities, +30 mood bonuses, gear durability and animation support remain in place. Water is still the defensive prototype, now delivered by its own serum.

## Visual adjustments

- Mirrored the Wind sword horizontally, keeping the tip up and handle down. Installed at its original 256px size and bounds. Both Melee Animation hand anchors still pass the handle check.
- Raised the Lightning, Flame and Wind visor draw positions by 0.07 map units, preserving their size, eye-cover layer and hair visibility.
- Installed four transparent 256px syringe assets. Sources contains the original generated images; Textures contains the installed versions; Preview.png shows all four serums and the mirrored sword.

## Validation

The assembly builds against the installed RimWorld version. All ten verification scripts pass, including new checks for whole-body migration, preserved ability state and identity, migration idempotence, untouched unrelated conditions, duplicate-dose rejection, disabled legacy extraction, serum mappings and visor offsets. Existing combat, Water defense, teaching, non-lethal, animation compatibility and asset checks also pass.

Restart RimWorld to load the update. Live injection, migration of a real save and visor placement have not been tested in the running game. The pre-change mod files and Wind sword are backed up in work/ascension-serums-backup in this workspace.
