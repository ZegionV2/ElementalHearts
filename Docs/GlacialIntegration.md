# Glacial Ascendant: playable integration

Glacial is installed as Water's complete evolution, including its regal kit, five techniques, normal recovery, shared defensive component, evolved stats and Bestow Water. The earlier asset set remains unchanged.

To evolve normally, select an ordinary Water Ascendant after **150 additional unique hostile non-animal kills and ten ascendant days**, then press **Evolve: Glacial**. The existing interruptible 4.5-second ritual performs the upgrade. For development testing, enable dev mode, press **DEV: Evolution-Ready**, then press the normal Evolve button. Turning dev mode off immediately hides the shortcut; a previously held shortcut cannot grant readiness afterward.

After evolving, Transform manifests helmetless white/cyan royal regalia and a crystalline sword. The six-frame fractured-ice halo appears only while transformed and drafted. The menu shows Glacial Crescent, Frostwake Step, Crystal Vortex and Absolute Zero, plus the separate Glacial Explosion command and one shared Auto Combat control. Ordinary evolution controls disappear after promotion. Bestow grants normal Water ascension directly through the existing paired ceremony.

Glacial Explosion automatically answers two or more nearby visible enemies while transformed and ready. It pushes survivors safely outward, freezes movement and attacks for five seconds, tints victims icy blue and applies mild hypothermia. Non-lethal retains the push/freeze without wounds or hypothermia. Effects use the existing bounded pools and add no terrain or building damage. See **GlacialUpdate.md** for exact reach, cooldowns, limits and cleanup rules.

This integration pass finishes Glacial-specific shared-control descriptions and skill rejection messages, avoids new range-array allocations during combat checks, and guards spawn registration. It preserves the existing serialized Water and Glacial keys. No existing artwork, progression thresholds or combat tuning changed.

The native fixture loads an older Water save, verifies the added mastery component without duplicate components or lost state, invokes the real Evolve command, checks dev-mode visibility in both directions and confirms that the evolved menu exposes exactly five named skills and one Auto Combat control. It also repeats the complete native Glacial combat, material, freeze, interruption and mid-action save/load checks.

Personal saves and configuration are not used for testing. The full third-party mod list and on-screen artwork placement remain unverified. Completely restart RimWorld to load the updated assembly.
