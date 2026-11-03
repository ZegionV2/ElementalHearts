# Earth polish validation

## Build and targeted regressions
Built the included assembly against the installed RimWorld 1.6.4871 game assemblies and Harmony.

Passed:
- Tests/RunEarth.ps1: existing lineage/progression/save checks plus bounded replanning, interpolation endpoints/midpoint, persisted segment/budget fields, aura transition boundaries and impact-phase timing.
- Source/VerifyEarth.ps1: definition/component bindings, 70 RGBA textures, all 21 body/direction exports, 42 distinct effect frames, normal-tier equipment and sound references.
- Source/VerifyCombatFlow.ps1: combat-order/queue protection, active/idle cadence and bounded navigation.
- Tests/RunStormBlitz.ps1: existing movement/drawing/interruption and save-field regression.
- Tests/RunWaterLineage.ps1: existing disciple promotion and aura-policy regression.
- git diff --check.

Damage, cooldown, Resolve, disciple and defensive-strength values were not changed.

## Native RimWorld smoke test
A disposable colony ran in a separate test mod with isolated saves/settings.
The native test passed:
- Earth transformation and the bound two-piece helmetless kit.
- Real Cleave damage, Resolve and cooldown start.
- A target moved while Advance was already in progress.
- Native pawn drawing matched the interpolated position.
- The charge adjusted its route exactly once and reached a valid melee flank.
- Bastion and its 0.90 ally protection factor.
- A genuinely active Mountainfall wind-up saved to disk.
- Loading that save in a fresh game process, completing Mountainfall, consuming Resolve and releasing control.
- Automatic Advance followed by Cleave from a melee combat order.
- Non-lethal Cleave without added wound damage.
- An explicit move order replacing a newly accepted charge, with no delayed damage afterward.

The reload pass initialized Direct3D 11 on the local Intel UHD Graphics device.
The attempted automatic screenshot was blank, so it is not evidence of correct visual appearance.
Subjective animation smoothness and armor appearance still need a normal graphical playtest.
No FPS/TPS benchmark or external Melee Animation compatibility claim is made.
Test-only assemblies, settings and saves are excluded from the installable mod.

