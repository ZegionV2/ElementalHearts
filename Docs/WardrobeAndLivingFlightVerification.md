# Wardrobe and living-flight verification

Installed in Elemental Hearts for RimWorld 1.6.4871. Verified in isolated profiles with Harmony, the native Melee Animation integration, HAR/FemaleBB, ISEKAI and the installed DLC. Personal saves/settings were not edited. Temporary test mods are removed after verification.

## Artwork and fit

Built-in image generation produced a new Thunder east-facing suit/cape and two new three-view academy uniform masters. Native east textures mirror west. Fitting exports **58 PNG replacements**: eight Thunder side textures and two sets of 24 worn body/facing textures plus inventory icons. The rest of Thunder's front/back artwork remains intact.

All seven native body textures plus FemaleBB are covered. **80 texture/body/facing checks** found zero uncovered native-body pixels, with neckline centering within three texture pixels. School clothing uses one-pixel silhouette padding instead of the former broad padding. Thunder's Fat/Hulk body and cloth side scale narrow together (0.78/0.82); side heads use their native anchor. New school uniforms also use the native head anchor, with flat chests and tucked shoulder caps.

Gendered wear rules remain active. Calling still generates ordinary 18–24-year-old humans with Male/Female/Thin bodies and the existing strong ISEKAI identity/talent bundle. School garment texture fallbacks and native wearing support cover all developmental/body shapes; recruitment remains adult-only.

## Native verification

**524 in-game assertions passed:**

- **102 flight assertions:** all four lineages, distance/time bounds, outdoor takeoff, canceled targeting, actual native destination callback, rising takeoff, one original passenger, correct themed vessel, gradual world position, disabled early landing, mid-flight save/load, continuous position, return from the current position, saved return, manual arrival/cancellation, saved waiting state, roofed/blocked landing retention/retry, descending arrival, exact original sword/heart/drafting, no duplicate vessel and no permanent skill lock. Old instant-travel data migrated safely to its already-arrived state.
- **394 wardrobe/presentation assertions:** 96 family/body/facing wearing cases, actual chest/groin coverage, opposite-gender outfit rejection, native head anchors, matching Thunder body/clothes scaling, eight generated adults retaining age/body/gender/ISEKAI rules and an actual moving-world-flight showcase. All 12 native body/facing screenshots were reviewed, including Fat/Hulk side views and school uniform profiles.
- **28 world-flight presentation assertions:** all four readable expanding markers, landing/return controls, clean remaining-time labels, the stock world Jump control focusing the actual moving traveler and suppression of the stock origin-settlement caravan button while a flight is selected. The four native world-map screenshots were reviewed.

World travel follows a spherical route with eight ticks per tile, bounded to 60–6,000 ticks, and prepares destination maps only on arrival. Original world-object classes, pawn/gear identity and prior saved transit keys remain compatible. Map Step, combat skills, the three-state Incarnate utility cycle and Calling's silver/cooldown mechanics retain their established behavior. No frame-time benchmark is claimed.

See [Living world flight](LivingFlight.md) for controls and saved-flight rules.
