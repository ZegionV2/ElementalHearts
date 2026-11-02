# Azure power and teaching update

- In DEV mode, select an unevolved Flame Ascendant and press **DEV: Evolution-Ready**. This raises its mastery kills and ascendant days to the required minimum without lowering existing progress. It does not evolve the pawn. Press **Evolve: Azure Flame** afterward.
- Azure's combined elemental movement-speed multipliers are now **500% (5×)**, replacing the previous small boost. Gear, health, terrain and other mods can still change actual movement speed.
- Azure's standalone incoming-damage factor is **0.1**, giving **90% incoming-damage reduction before armor**. The old Flame condition is replaced, rather than stacked. Hits from an unevolved Elemental Ascendant receive a further **75% reduction**. The extra ward does not apply to attacks from another Azure Ascendant.
- Azure ascension mood is now **+50 total**, replacing the ordinary Flame +30 stage. It does not stack to +80. Ordinary ascendants retain their existing +30 mood bonus.
- Teaching now uses paired jobs. The teacher approaches; then both pawns stop and face one another for the full three-second lesson. Ordinary movement/work orders do not interrupt the lesson. The pupil's prior task is suspended and can resume afterward if still valid. Death, downing, invalid targets or separation cancel the session and release the waiting pupil. Active elemental abilities, counters, evolution and transformation are gated during teaching so they cannot move the participants out of place.

Existing Azure pawns receive the new defense, speed and mood after a full restart and save load; no re-evolution is necessary. Existing teaching jobs from an older save may cancel safely if they lack the new paired receiving job; start the lesson again.

Validation: native-reference build; all 14 existing verification scripts; new production-method tests for DEV readiness, normal/evolved attacker defense scope, +50 mood selection, paired-job ownership/distance checks, native toil ordering and held receiving-job setup; XML checks for combined movement/defense factors and uninterruptible teaching jobs. Earlier Azure/mechanics/serialization-output tests also pass. Tests do not execute a live lesson or a full in-game save/reload; actual facing, task resumption and combat balance still require gameplay verification.

Original changed files are backed up separately.
