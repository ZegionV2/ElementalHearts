# Earth Ascendant — combat and presentation polish

This update builds on the normal Earth lineage. Adamant and head halos remain outside its scope.

## Combat feel
- Charge drawing interpolates between checked movement cells instead of snapping two ticks at a time. Logical positions and collision checks still use the same safe cells.
- Advance stops early when its opponent is already in a clear adjacent position.
- A moving target or blocked route can cause one bounded route adjustment. Total movement remains limited to 12 checked steps and a six-cell displacement from the original charge start.
- Manual charges retain their selected opponent. Automatic charges retain the existing permission to choose a nearby eligible replacement.
- Adjusting the route does not reset cooldowns, grant another strike or extend the movement budget.
- A new melee order reduces the next Earth decision delay to at most 12 ticks, instead of inheriting the full idle delay. Changing Auto Combat mode schedules a fresh decision.
- Mountainfall evaluates the hostile cluster around its actual target. Bastion still evaluates pressure around the caster and nearby allies.
- Inactive Bastion no longer performs an unnecessary status lookup/removal on every pawn tick.

## Effects
- Adjacent frames cross-fade with eased blend weights.
- Cleave shows a short opening cut during its wind-up, then its strong cutting frames when the hit commits.
- Mountainfall's target-following marker advances through the gathering/eruption frames; the committed hit starts on the peak impact frame.
- The charge dust wake follows the actual pawn and facing, including a curved or adjusted route. It ends on interruption.
- Idle/combat auras transition with engagement timing; repeated hits do not restart the transition.
- Activation settles into the ordinary aura, while Bastion ramps in and fades before expiry.
- Timed effects fade at the end. Materials, frame counts, bounded pools and aura limits are retained.

These are rendering and combat-flow changes. Ability damage, cooldowns, Resolve limits, protection strength, disciple progression and equipment ownership retain their existing values.

## Saves and interruption
Additional saved fields retain the last movement segment, interpolation tick, original charge position, route-adjustment count and steps already spent.
Missing fields in older saves use safe defaults.
Replacing the owned job immediately removes its visual movement override and prevents an uncommitted delayed strike.
Committed-hit state and the original cooldown deadlines are retained.

## Verification
See EarthPolishValidation.md for checks and native test results.

