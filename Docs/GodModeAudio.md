# God Mode controls and elemental sound update

**DEV: Evolution-Ready follows God Mode**, the skull toggle on the developer toolbar. Developer mode by itself does not reveal it. This applies to Flame → Azure Flame, Lightning → Thunderstorm and Water → Glacial. Turning God Mode off removes the button from fresh lists and prevents already cached buttons from drawing, accepting clicks or granting readiness. Readiness still only unlocks the normal Evolve button; it does not skip the evolution ritual.

DEV: Ascend-Ready also follows God Mode. It fulfills a disciple's 50-kill/five-day requirements; the player then presses Ascend for the normal 3.5-second ceremony. Cached buttons stop drawing and accepting input when God Mode is off.

Ability audio now restores the earlier native sound character. The eight original SoundDefs have their original source clips and settings again. Existing routed ability IDs remain valid, so saved games and ability calls use the matching elemental cues without migration. The custom WAV banks remain available; Glacial still uses its frost layers.

- Lightning and Thunderstorm: real thunder for attacks, a deeper thunder roll layered with a heavy boom for impacts, an electrical dash and shield-energy charge. Thunder cues have a per-cue 30-tick repeat limit.
- Flame and Azure Flame: native fire-spew rush/warmup and incendiary bursts.
- Wind and Cyclone: native air-cut whooshes; Cyclone uses a lower pitch.
- Water: native water splashes with a quiet metal accent for parries.
- Glacial: quieter frost layers with brittle shattering accents on strikes, parries and the ice burst.
- Manifestation: the original energy and locking cues.

Sounds fire at movement, telegraph or committed impact stages, not once per victim. Tempest Dragon cues follow its dash legs. Water/Glacial movement and finishers no longer reuse the Wind clip. Water/Glacial ritual sounds use their elemental profile.

Sounds use pitch variation, spatial falloff and native simultaneous-voice limits. A real-time crowd budget, per-pawn repeat limits, cached definitions and camera/fog checks reduce audio overlap at fast game speeds. No loop sustain or background audio source is added. RimWorld's normal sound-volume control applies. Native testing resolved every clip in all 51 definitions and passed 123 sound-definition/source/throttle checks. Subjective loudness in crowded live combat can still depend on the player's volume settings.

Cooldowns, damage, combo rules, movement routes, prior carrying/travel/resilience bonuses and gameplay save keys are preserved. Restart RimWorld to load the update.

Earlier procedural-audio previews describe the former mix; the current definitions use the sources listed above.
