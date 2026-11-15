# Regalia coverage and Eclipse side fit

The Azure Flame, Thunderstorm, Glacial and Cyclone uniforms, including their Court variants, now use the same full below-neck apparel groups as the other complete suits: Torso, Shoulders, Arms, Hands, Waist, Legs and Feet. Fingers and toes inherit Hands and Feet coverage. This fixes native uncovered-groin and chest/groin thoughts through actual apparel coverage, without suppressing mood rules or hiding the body renderer.

All 28 Ascendant/Court uniforms now have these groups. Head and neck coverage remain separate from the suits. Existing saved outfits gain the corrected coverage automatically when their definitions reload; their saved item identities and transformation state are retained.

Eclipse's east/west presentation is narrower, with body-specific widths for all seven native body textures. The body and garment stay aligned. South/north proportions and the head position are unchanged. The fit fades with Transform/Revert and is also used by Eclipse's garment afterimages. The shared Court uniform and Lunar outfit retain their previous proportions.

## Verification

- Built against the installed RimWorld 1.6 and Harmony assemblies.
- Loaded an isolated existing save containing 16 previously manifested Eclipse/evolved uniforms. Their identities survived the subsequent save/reload.
- Reproduced the original uncovered-groin thought with the old torso/waist groups, then verified native body-part coverage, psychological nudity, groin and chest/groin checks for all 28 uniforms.
- Rendered east, west, south and north for Male, Female, Thin, Fat, Hulk, Child and Baby textures. Reviewed before/after native camera captures. Baby remains in its native lying pose.
- Passed 1,772 native assertions, including coverage and fitting after reload.

The isolated historical fixture logs an existing HAR/addiction cross-reference warning while loading. No apparel-fit or coverage exception occurred during the successful run. Personal saves and mod settings were not changed.
