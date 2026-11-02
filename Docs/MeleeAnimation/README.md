# Optional Melee Animation support

All eight elemental swords now have native WeaponTweakData profiles: Lightning, Flame, Wind, Water, Azure Flame, Thunderstorm, **Glacial** and the prepared **Cyclone** sword. Melee Animation discovers these profiles from the Elemental Hearts mod folder. The compatibility remains optional; no dependency or gameplay assembly changes are required.

Glacial uses its own measured fit rather than the longer-handled Water profile. On its existing 256×256 texture, the primary grip is at (128,182). The uniform 1.5 scale places the second native hand near (128,207), inside the ivory grip. A 90-degree rotation makes the upward blade face right in animation space; the profile's horizontal offset places the primary grip at the animation hand origin. Sweep boundaries correspond to texture rows 154 and 21. Both hands and long sharp/stab animations are enabled, matching the existing elemental swords' classification.

The six established profiles and all sword artwork remain unchanged. The older Water template is historical reference material; the active Water profile already ships in WeaponTweakData.

`Source/VerifySwordAnimations.ps1` deserializes all eight profiles with the installed RimWorld 1.6 Melee Animation loader and verifies native fields, definition identities, orientation, hand anchors and blade bounds. The native game fixture checks automatic registry discovery, the correct texture/definition, and idle, movement and attack animation selection with both mods enabled.

Completely restart RimWorld with Elemental Hearts and Melee Animation enabled. Compatibility covers animated sword rendering and native animation selection; the existing optional ability/animation bridge remains in use. On-screen hand placement and combat with the complete third-party mod list are not visually verified.

Cyclone gear is integrated into its playable Wind evolution; its active sword profile fits the new emerald-channel sword independently. The primary hand is at (128,188), the second near (128,219), with 90-degree rotation and uniform 1.2 scale. Sweep bounds project to texture rows 148 and 21. Its evolution, transformation and five techniques are now integrated.
