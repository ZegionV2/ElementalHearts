# Combat flow update - 26 October 2026

Installed into ElementalHearts and rebuilt against the local RimWorld game.

## Combat transitions

- All Hearts use a 12-tick active decision interval and a 60-tick idle interval, while Lightning/Flame retain their bounded scheduler budget.
- Existing melee or Search and Destroy pursuit can continue to nearby hostiles after a target falls. Pursuit is local (six tiles), skips prisoners/downed/dead pawns, respects line of sight, and stops for explicit hold/move/work orders and queued orders. This short-lived pursuit memory resets on game load; saved technique targets and movement state remain intact.
- Normal melee resumes against the next target when no technique is ready. Automatic Lightning/Flame ultimates now resume the interrupted job.
- Automatic Wind Rising Gale and Tempest can redirect remaining strikes to nearby hostiles. This does not add strikes or reset cooldowns. Manual casts keep their selected pawn; non-lethal rules still apply.
- Wind resumes decision-making shortly after finishing a technique, has shorter gaps between cuts, and favors active attacks over waiting in Turning Swallow.
- Retreat rolls now require at least two nearby hostiles, avoiding unnecessary retreats during a single-opponent combo.
- Crowded flanks can fall back to a valid adjacent strike; footwork can pass downed pawns without attacking them.

## Optimization

- A single bounded Wind navigation tree covers all candidate flanks and returns the selected route directly to the job. The six-tile search contains at most 113 cells; routes remain capped at 12 steps.
- Wind jobs cache their ability component, with implant-removal validation retained.
- Static ability-priority arrays replace per-decision lists. Nearby enemy counting and selection avoid temporary pawn lists and stop counting once the relevant threshold is reached.
- Idle checks are less frequent. Lightning/Flame aura rendering rejects undrafted/offscreen pawns before looking up their components or consuming the visible-aura budget.
- Pursuit state uses weak pawn keys rather than a permanent global pawn cache.

## Validation

Build and all six scripts passed: Verify, VerifyFlame, VerifyWind, VerifyRefinements, VerifyNonLethal and VerifyCombatFlow.
The new source-algorithm checks exercise 64 order combinations, 32 combo-intent cases, active/idle timing, bounded navigation, blocked paths, wall-gate/corner handling and out-of-range rejection. Their helpers are compiled for the test host because RimWorld's Mono framework locates generic collections differently from Windows .NET.

No live-game combat, save/reload or frame-time profiling was performed. Performance improvements describe reduced code work; no FPS/TPS gain is claimed. Restart RimWorld to load the updated assembly.
