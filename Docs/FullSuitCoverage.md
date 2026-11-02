# Armor visibility hotfix

The full-suit body-suppression patch was removed after it caused invisible bodies/armor in-game. All three suits use normal pawn rendering again, with their existing directional body-type textures. The old extension type remains inert for compatibility; it is no longer assigned to suits.

Build, absence-of-patch validation, 63 texture checks and transformation checks passed. Fully restart RimWorld to unload the old Harmony patch. Hulk edge coverage still needs an artwork-based correction; the body-hiding workaround must not be reinstated.
