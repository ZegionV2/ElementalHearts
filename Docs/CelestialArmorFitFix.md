# Celestial head and Thin-body fit correction

Celestial's head now sits higher above the regalia's collar. The correction uses the shared native head origin, so head graphics, hair, beard and the solar halo follow the same alignment. Male/Female/Thin use a 0.10-world-unit correction, Fat uses 0.12 and Hulk uses 0.15, scaled through the native body mesh width for smaller life stages. The fit blends with the existing apparel manifestation during Transform and Revert and returns to the ordinary head origin in personal clothes. It adds no saved state.

The body armor's established coverage fit remains in place. Lowering the entire outfit was checked and rejected because it exposed shoulder pixels on several native body types. Keeping the covered body and correcting the head origin gives the intended head/collar fit without reopening those gaps.

The Thin south and east textures have been edited with the built-in image-generation tool to use a flat masculine/unisex ivory breastplate. Breast cups, cleavage and under-bust shading have been removed. The ivory/gold regalia, sun medallions, shoulder ornaments and split mantle remain. East supplies native mirrored west. Thin is a shared RimWorld body type: both sexes use this flat Thin armor, while the separate Female-body textures retain their existing design.

Validation: all 28 native body/direction poses retain complete opaque coverage, including one-pixel silhouette padding. Native tests check the corrected head origin and render node, unchanged ordinary heads, restoration after removing regalia, smooth Transform/Revert, hair preservation, both mid-animation saves/loads and personal-clothing restoration without inventory projections. Existing Celestial and manifestation/hair regression checks also pass. Testing uses isolated saves and the temporary test companion is removed afterward.

Gameplay, abilities, stats, sounds, weapons and the recent solar effects are preserved. Restart RimWorld to reload both the rebuilt assembly and cached apparel textures.
