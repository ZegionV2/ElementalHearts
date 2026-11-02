# Ascendant armor east/west fit correction

Lightning, Flame, Wind and Water uniforms, plus Azure Flame, Thunderstorm, Glacial and Cyclone regalia, use separate side-view pose corrections for Male, Female, Thin, Fat, Hulk, Child and Baby. The fit shifts the garments back over the body and adds the width/height required to cover the upper back, torso and lower body. West mirrors the east correction, including the horizontal offset.

The correction is applied by the apparel body renderer. Its offsets use the native pawn body mesh width, so life-stage and native size changes scale the placement with the pawn. It also applies to apparel-preview nodes used during manifestation/reversion. Fitted regal afterimages use the same side placement and scale.

The existing PNG artwork, palettes, inventory icons and sword profiles are retained byte-for-byte. Front/back poses and unrelated vanilla clothing use their existing render settings. Body and head rendering remain active; the garment's placement supplies coverage.

Measurements use the seven native RimWorld body alpha silhouettes, the original garment alpha and a one-pixel padding margin on the normalized 256-pixel canvas. Runtime pose values are exported from the actual apparel worker for all 112 family/body/direction combinations and checked against those silhouettes. All measured cases have zero uncovered body pixels. The native fixture also passes 588 pose/material/exclusion checks. Third-party replacement body graphics are not included in this fit measurement.

This is a rendering-only correction; it adds no gameplay save keys or stat/ability changes. Restart RimWorld to load the rebuilt assembly.
