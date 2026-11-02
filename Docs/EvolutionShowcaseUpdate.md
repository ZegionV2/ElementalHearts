# Evolution showcase update

- Azure Reversal now accepts **two or more** hostile pawns within the existing three-cell pressure radius. Manual and Auto Combat share the same rule. Its route checks, cooldown, damage and separate fifth button are unchanged.
- Azure evolution now uses a **4.5-second C# ritual**: warm gathering rings and embers, increasingly blue spirals and rising flame filaments, a brief pawn lift, a converging core, then a finishing shockwave and a one-second afterglow. Existing sound cues mark the phases. Interruption releases the pose immediately and does not spend evolution resources.
- The halo still requires transformation and drafting. The ceremony uses ground rings/spirals rather than an always-visible head halo.
- The ritual's start time/audio phase are saved; old in-progress 180-tick jobs recover their existing progress. The animation is tick-derived, deterministic and bounded to 24 tracked ceremonies, with cached geometry/materials, opacity/fog/camera checks and no scene-object spawning or camera shake. Effects are cosmetic and do not deal damage.
- The **Thunderstorm Ascendant** red-lightning evolution is documented in ThunderstormAscendantPlan.md. It remains a plan; no Thunderstorm gameplay is added.
- The mod description now covers current ascension, disciples, Azure's five skills, gear/reversion and requirements. A generated banner is installed at **About/Preview.png**, the path read by the installed game's native mod loader. About/ModIcon.png reuses the existing Azure evolution emblem for the mod-list icon.

Restart RimWorld and select Elemental Hearts in the Mods screen to see the banner/description. Use DEV: Evolution-Ready on an unevolved Flame Ascendant, then Evolve: Azure Flame to view the new ritual.

The banner was created with the built-in image_gen tool using the saved evolution-regalia master as a design reference. The source and exact prompt are supplied in ArtSource. It shows currently playable forms; Thunderstorm is not presented as released.

Validation: native-reference build, updated Reversal and existing Regal fixtures, and new Evolution fixture. Evolution tests cover 321 sampled frames, deterministic bounded spirals, lift endpoints, timeline stages, finite completion effects, native held/facing toils, legacy timing recovery, job replacement gates, bounded/deduplicated ceremony tracking, native Scribe save output and the native mod-preview path. GPU rendering, live gameplay and full save/reload still require an in-game check.
