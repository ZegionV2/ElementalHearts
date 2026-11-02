# Phase 2 — four active abilities

## Upgrade

1. Quit RimWorld and back up your save.
2. Replace the existing **ElementalHearts** mod folder with the folder in the Phase 2 ZIP. Do not enable a second copy. Keep Harmony enabled.
3. Start RimWorld and load your save. Draft your implanted pawn: four buttons should appear. You should not need to remove/reinstall the implant. Vanilla HediffWithComps initializes the newly declared component when loading old hediffs; the new cooldown fields default to ready.
4. Open Options → Mod settings → Elemental Hearts if you want friendly fire. It is off by default.

The existing implant/stat Def names and package ID are unchanged. Your installed Phase 1 copy and normal game configuration were not overwritten by this implementation. The Phase 1 ZIP remains available separately.

## Ability behavior

| Ability | Range | Cooldown | Implemented behavior |
|---|---:|---:|---|
| Thunder Sever | 10 tiles | 10 s | Instant directional trace extending toward the selected point. Stops at opaque walls/closed doors, including corner blockers. Hits each eligible pawn once. Sweeping crescent visual. |
| Lightning Flash | 15 tiles | 12 s | Instant blink to a clear adjacent cell, followed immediately by an empowered strike. Both target and landing paths must be clear. Five fading lightning streaks mark the dash. |
| Thunderstorm Dance | 4-tile radius | 18 s | Strikes all eligible pawns in radius with clear lines from the caster. No arbitrary victim cap. Brief 60-tick stagger and expanding/rotating vortex. |
| Heavenly Thunder | 12 tiles | 45 s | Blinks beside the target, channels for 12 ticks (0.2 s), then strikes with a pillar and a 3-tile electrical burst. The primary target gets the main hit, not an additional splash hit. |

All four work with ordinary melee weapons or unarmed attacks. No cosmetic gear, mana, levels, genes, Royalty powers or Biotech features are needed. These are hediff commands, not psycasts. Targeting clicks must be within the listed range; Sever then extends in that direction up to its full range.

For a cast, base damage B is the larger of **10 × pawn MeleeDamageFactor** or the strongest currently usable weapon-derived native adjusted melee damage. Prefer equipped melee-weapon verbs; otherwise use available body/unarmed attacks, excluding terrain attacks. The native adjustment includes weapon quality/material and the pawn damage factor, so the +50% implant factor is not applied twice. The strongest usable weapon damage type and penetration are retained; bare-fist fallback is Blunt. The abilities are guaranteed hits once validly cast, with ordinary damage/armor processing, rather than ordinary hit/dodge rolls.

| Ability | Physical damage | Electrical damage |
|---|---:|---:|
| Sever | 1.5 × B | 0.8 × B |
| Flash | 2 × B | 1 × B |
| Dance | 1.5 × B | 1 × B |
| Heaven, primary | 6 × B | 4 × B |
| Heaven, nearby pawns | — | 1.5 × B |

Electrical injury uses a custom `EH_Lightning` DamageDef with native burn-injury/Heat armor handling and 0.5 penetration, without the fire-starting Flame worker. Surviving mechanoids also receive native EMP damage (30), which invokes the normal EMP stun/adaptation rules. Damage packets identify the casting pawn. The caster is always excluded from the ability's damage. General resistance to other electrical sources remains Phase 3.

By default only hostile, living, standing pawns are eligible; allies, neutrals and prisoners are skipped. With friendly fire enabled, those living standing pawns become eligible too. Dead/downed pawns and the caster remain excluded. Buildings/plants are not ability damage targets. Area bursts are individually filtered damage calls rather than a vanilla indiscriminate explosion, preventing friendly-fire or through-wall splash leaks. A pawn downed by the physical part can still receive the electrical part of that same already-committed strike; subsequent casts skip it.

## Cooldowns and interruption

- Cooldowns are saved as game-tick deadlines on the implant. They advance with game time, including while undrafted/off-map, and stop while the game is paused.
- Invalid targets, blocked paths and unavailable landing cells reject the cast without starting a cooldown.
- Sever, Flash and Dance commit immediately, then apply a brief 12-tick recovery stance. Cancelling targeting before clicking costs nothing.
- Heaven starts its cooldown when its job is accepted. Its target and telegraph duration use native job saving. Moving/cancelling the job, undrafting, losing the implant, becoming downed/stunned, or the target leaving adjacent clear melee range cancels the pending impact. No refund. Taking damage that does not interrupt the job or stun/down the caster does not automatically cancel it.
- While channeling Heaven, the other buttons are disabled. Changing a target's allegiance/protected status is rechecked before damage.
- Removal deletes the implant's cooldown component; a later surgical reinstall starts ready. There is no cooldown-reset button. Death does not forcibly reset saved deadlines on a retained implant. A newly upgraded Phase 1 implant starts ready.
- Cosmetic effects are not saved. Reloading during a telegraph may omit its already-started ring, but the job's scheduled impact creates a new pillar when it lands.

## Effects and performance

`AbilityAtlas.png` is a byte-identical copy of the supplied `LH-Abilities.png`. Four UV regions provide both icons and effect meshes. Source art is not repainted or resized. The static crescent travels, dash streaks fade, vortex scales/rotates, and pillar appears at full impact size on the same game tick as damage. These are procedural animations of static art, not the labeled board's illustrated frame sequences.

The source board still cannot supply transparent animation frames. Pawn-shaped afterimages are represented by fading streaks in this phase; they do not replace or duplicate the actual pawn body. True pawn afterimages and authored sequence timing remain a visual refinement. In particular, high-speed simulation can skip intermediate displayed frames even though damage timing remains tick-based.

Each map has a fixed 96-slot effect pool; slots are reused without per-effect GameObjects. Four meshes and twelve fade materials are cached. Effects outside the current camera view are not spawned/drawn. The pool drops excess cosmetics only. There are no idle target scans or periodic chain-lightning searches. Casts search only their line or bounded radius and calculate a shared damage profile once for area attacks. A large-raid frame-rate benchmark is still outstanding.

## Verification

Compiled without warnings against the installed 1.6 assemblies and installed Harmony. The local Version.txt says `1.6.4871 rev573`; the running game reports `1.6.4871 rev574`. Both refer to the same installation used for this work.

Verified by reflection/IL and compilation: `HediffComp.CompGetGizmos`, `CompExposeData`, `HediffWithComps.ExposeData/InitializeComps`, command texture coordinates/target validators, `Pawn_MeleeVerbs.GetUpdatedAvailableVerbsList`, `VerbProperties.AdjustedMeleeDamageAmount/AdjustedArmorPenetration`, `Pawn.Notify_Teleported`, the native job/toil extension points, `StaggerHandler.StaggerFor`, `DamageInfo/TakeDamage`, map component drawing, camera bounds and all referenced XML fields. Core Flame/Burn/EMP definitions and the `Burn` injury/Heat category were checked locally. No Harmony patches were added.

An isolated headless game was launched with a disposable map and separate save-data folder. A temporary test-only assembly exercised actual game APIs; it is not included in this mod. The final Core-only run explicitly confirmed all five DLCs were disabled. Passed: four buttons, all cooldown starts, Sever damage, wall blocking, invalid Flash rejection without cooldown, Flash landing/strike, Dance hits on both sides, caster exclusion, friendly-fire opt-in, and Heaven's damage occurring after its telegraph. See RuntimeChecks.txt for the recorded assertions.

Actual save/reload passed: after casting all four abilities, the test saved, then a fresh game process loaded that save. The implant component was restored and all four remaining cooldown values matched exactly. The initial in-process reload attempt stalled in the headless test harness; the fresh-process reload completed. Saving/reloading during the ultimate wind-up remains a manual test.

Headless mode produces Unity shader/texture warnings and cannot establish visual quality. This is not a full UI playtest or a compatibility benchmark. The user needs to verify icon framing, mesh orientation, timing feel, occupied landing scenarios, interruption cases, EMP against different mechs, and the normal save upgrade with their mod list. Combat Extended, melee animation mods and third-party on-hit effects remain untested. Direct ability damage does not execute ordinary melee-verb patches; Phase 3 will integrate our own charge/passive hooks explicitly.

## What to test now

1. Load your Phase 1 save, draft the implanted pawn, and confirm four icons/buttons. Undraft: buttons disappear. Existing passive bonuses remain.
2. Line up two hostile pawns, with another behind a wall. Cast Sever toward them: the first two should be hit once each, the wall should stop the trace, and the behind-wall pawn should remain untouched. Also test a closed door and diagonal wall corner.
3. Flash toward a hostile pawn within 15 tiles: land adjacent and strike immediately. Test out-of-range targets, walls, a completely surrounded target, and a landing cell occupied by another pawn: invalid casts should spend no cooldown.
4. Surround the caster with hostiles and one ally. Dance should hit every eligible hostile within four tiles, skip the ally and targets behind walls, and briefly stagger survivors. Confirm the spin is centered on the pawn.
5. Cast Heaven on a tough hostile: see blink → short ring → pillar and damage. Nearby hostiles in clear radius should take electrical splash. Cancel during the wind-up with a movement order or undraft; damage must not land and the 45-second cooldown must remain.
6. Repeat with fists, a sword and a blunt weapon. Stronger weapons should increase damage; abilities should never require the supplied sword. Test an EMP-vulnerable mechanoid and verify electrical stun, allowing for native EMP adaptation on repeated hits.
7. Confirm allies, neutrals, prisoners, dead/downed targets and the caster are protected by default. Toggle friendly fire and verify only the intended protection changes; then turn it off again.
8. Save immediately after using each ability; reload and check remaining cooldowns. Also save while Heaven is channeling, reload, and verify one impact at most. Remove the implant while channeling in a disposable test: no later damage or buttons should remain.
9. Inspect visuals at several zoom levels. Report clipped atlas edges, wrong slash/trail direction, pillar anchoring, or excessive opacity. Run a large raid if available and report frame-rate regressions. Off-screen damage must still work even when effects are suppressed.

Phase 3 (Electrified Strikes, Static Charge, Thunder Reflex and animated aura) has not begun.

