# Transformation update

Items are explicitly named Lightning Serum, Flame Serum, Wind Serum and Water Serum. Existing internal IDs remain unchanged.

Elemental Hair commands have been removed. Full Ascendants receive Transform/Revert through their whole-body condition. Lightning, Flame and Wind have complete kits; Water's button is disabled until its uniform, visor and sword exist.

Transform plays a one-second elemental effect, equips the corresponding uniform, visor and sword, and selects the elemental hairstyle. Previous apparel and the primary weapon are kept in the pawn's inventory and restored on Revert. Previous hair and equipment references, transformation status and the issued kit are saved. Each Ascendant receives one kit, which retains damage and quality. Repeated transformations do not create replacements; missing kit pieces must be returned to that pawn's inventory. Reverting while other equipment has been manually equipped stows that equipment before restoring the original set.

Auras require both transformation and drafting. The uniforms already cover feet; the request for 'boots' was interpreted as equipment boosts. Uniforms now have 65% base sharp armor, 35% base blunt armor and +0.4 movement speed. Visors add 5 percentage points of melee hit chance and 3 points of melee dodge chance. These are equipment bonuses and also apply when the items are worn manually.

Lightning and Flame swords now use generated curved blades with themed edges, guards and wrapped handles. Original sizing and compatible hand anchors were retained. The Transform button currently reuses the shared combat icon.

Build, definition, refinement, ascension-migration and sword-animation checks pass. Live transformation, equipment restoration, animation, aura visibility and save/reload still require in-game testing. Restart RimWorld to load the new assembly.

Pre-change code, definitions, assembly and original sword images are in work/transform-backup. Source artwork is in this folder's Sources directory.
