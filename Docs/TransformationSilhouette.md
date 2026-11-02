# Sprite-based transformation silhouette

Supersedes both earlier atlas and live-material pulse implementations.

The transformation now draws a generated white pawn-shaped PNG through the standard Transparent shader, tinted to the current element. Its alpha mask defines the silhouette; no white mask texture or pawn material replacement is used. Opacity and slight expansion pulse over the existing 90-tick transformation. Body type and life stage adjust its dimensions, and the sprite tracks the pawn's position. It is a universal energy silhouette rather than a pixel-exact outline of each hair/helmet combination.

Demon Summoning was inspected locally: its DemonCombatVisuals.Draw uses a separate textured plane and a color/alpha property block. This implementation follows that general approach with new artwork and independent code; no Demon Summoning dependency, binary, or texture is bundled.

The old TransformPulseMaterials, TransformPulseColor and TransformPulseLiveRendering patches are absent from the compiled assembly. Auras remain hidden during transformation. Gear locks are unchanged.

Validation: build, 101 timing samples, distinct elemental colors, transformation checks, absent-old-patch checks and PNG alpha checks passed. The 256x256 sprite contains 41,801 completely transparent pixels and 15,342 opaque silhouette pixels. Live-game placement and appearance have not been verified. Fully restart RimWorld to unload the old patches.
