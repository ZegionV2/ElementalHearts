# Elemental Hearts — animation and control refinements

Implemented in the installed mod on 2026-10-24. Restart RimWorld to load the assembly and textures.

## Appearance and assets
- Flame now loads 42 distinct 256x256 RGBA frames: Slash, Dash, Spin, Pillar, Idle, Combat and Activation, six each. Ability timing and aura five-tick frame cadence follow Lightning. Sheet extraction uses consistent scale per sequence and center/ground anchors. Both elemental auras use the existing six-frame machinery.
- Both auras render at pawn.DrawPos.y minus 0.01, behind the pawn, at scale 2.65 instead of 2.2. Ground offset is -0.35; the grounded mesh remains unchanged. Opacity pulses between 20% and 90% of the shared opacity setting, over about 2.33 seconds, with a pawn-specific phase.
- New generated element-neutral icons are installed at Textures/UI/Commands/ElementalHearts/ElementalHair.png and AutoCombat.png. ElementalIcons exposes them for current/future Hearts. Hair button now reads Elemental Hair.
- All nine elemental hair direction textures were scaled to 80% around canvas center (128,128), preserving the existing head anchor and alpha.
- Lightning uniform now has closed sleeves and lower-body panels, plus the same below-neck groups as Flame. Both uniforms have identical stats, layer, crafting settings and 600 MaxHitPoints (was 160). Their visible pixel bounds match across all 21 body/direction combinations. Existing items' saved current HP is not automatically healed by the higher maximum.
- Starting-apparel generation commonality is zero, and generic apparel tags are removed. All elemental hair style tags are removed; narrow patches reject these three hairstyles from random style choice/weighting. Explicit implant hair controls and crafted/spawned clothing remain available. Existing pawns already wearing these assets are not stripped.

## Combat controls
- Each manual ability button now targets a pawn (the directional wave can still target a location). Faction does not block a deliberately selected target. Range, walls, state and landing checks still apply. The caster, dead and downed pawns remain excluded.
- Area abilities remain centered on the caster with their existing radius. Selecting a neutral/allied pawn inside that radius explicitly includes that pawn; other bystanders still obey the friendly-fire setting.
- Scoped primary-target permission is restored after synchronous casts. Delayed ultimate jobs use saved playerForced and targetA intent, so their selected target remains valid through the channel/save-load. No global friendly-fire switch is changed.
- Auto combat gets one 25% retreat roll every 180 ticks when an enemy is adjacent and dash is ready. A successful escape searches clear, unoccupied, fire-free cells 3–5 tiles away and requires more separation from every nearby hostile. It consumes normal dash cooldown, pauses for 45 ticks, and resumes the suspended combat job. Queued player orders and ordinary movement are protected. Failed landing searches spend no cooldown.
- Non-player-controlled Heart bearers always have autoCombat enabled, independent of the player automation switch. They do not need to be drafted. They select hostile targets from their current job, combat target, or a bounded nearby search. They still require awake, living, standing, unstunned and violence-capable state. Player-controlled pawns retain their saved toggle and master switch.

## Validation
Build.ps1, Verify.ps1, VerifyFlame.ps1 and VerifyRefinements.ps1 passed. Checks cover referenced definitions/assets, six unique frames per sequence, both uniforms' stats/coverage, random style patch signatures, explicit target policy and scope cleanup, NPC automation enforcement and player toggles. Apparel pixel bounds were compared for all body types; final icon/hair/apparel/animation samples were visually inspected.

No live-game test was performed. In a disposable save verify aura depth (all rotations and downed pawns), hair fit, new pawn generation, manual ally/neutral targets for all four abilities, ultimate save/reload, player and NPC retreat/job resumption, and toggling player auto combat. Third-party forced outfit/style generators can bypass vanilla selection rules.

