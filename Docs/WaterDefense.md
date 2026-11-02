Current lineage/aura update: **WaterLineageUpdate.md**. The notes below document an earlier phase. Water teaching and disciples are now playable; Glacial evolution remains planned.

Current active-skills update: **WaterAbilitiesUpdate.md**. The notes below document an earlier phase; the four active Water techniques and Pressure are now implemented.

Current equipment/recovery update: **WaterKitUpdate.md**. The defensive prototype notes below describe the earlier release; Water now has its normal kit and recovery/stat package.

# Water defensive prototype

This update adds a usable Water Heart implant with Still Water automatic parries and Returning Tide counters. It is the defensive core, not the complete four-technique Water release. Its item temporarily uses the existing Wind Heart icon; Water sword, hair, uniform, visor, aura and teaching are not added here. The Water mood bonus is +30, matching the other Hearts. Install/remove surgeries use the shared one-Heart-per-pawn restriction. For a focused test, spawn EH_WaterHeart in developer mode and install it, or add its hediff directly.

## Behavior

- Equipped melee weapon, awake, unstunned, not downed, capable of violence; player-controlled bearers must be drafted.
- A 120-degree frontal cone, 80% success chance and 45-tick recovery per attempt. Failed attempts also consume recovery.
- Ordinary melee hits and direct Lightning, Flame and Wind technique strikes are intercepted before damage. A blocked direct strike loses its physical, elemental and EMP packets together. Sever's mark and Wind's stagger are also withheld. Combo follow-ups remain separate strikes, subject to the recovery window. Existing burns, ambient damage, passive elemental effects and explosion splash are not parried.
- Successful parries show an animated cyan arc and metal clash sound. With Melee Animation enabled, its existing dodge reaction provides the deflection movement. This is not a newly authored paired sword-clash animation.
- A nearby hostile attacker can trigger Returning Tide on the next tick. Friendly/neutral attacks can be defended against without automatic retaliation. New queued player orders and unrelated player jobs take precedence. Counters do not teleport or pursue targets that leave melee reach.
- Returning Tide uses 1.6 times weapon-derived damage. Melee Animation supplies an existing sword swing when the weapon and settings support it. The swing advances on simulation ticks, and damage occurs once when the blade direction crosses the target. Without an eligible animation, a vanilla melee visual and a 12-tick impact/24-tick recovery sequence are used.
- Counters respect On / Off / Non-lethal. Non-lethal uses the shared suppression system. Off disables Water's parries and counters. NPC Heart bearers retain forced automation. Global automation settings apply to player-controlled bearers.
- Counter damage cannot recursively trigger Water counters. The job saves its elapsed time and whether damage already occurred. A pending unlanded swing restarts after loading; a landed counter cannot hit again.
- All three existing Hearts refuse ability actions while Melee Animation has registered control of their pawn (such as a paired animation). Wind movement jobs and delayed ultimates recheck this through their existing action checks. Ordinary idle weapon animations do not block Heart abilities.

## Verification

Built against the installed RimWorld 1.6 assemblies. VerifyWaterDefense.ps1 passes 96 eligibility/cooldown cases, frontal-cone boundaries, blade-crossing/wraparound cases and checks the installed Melee Animation hooks and snapshot schema. Existing definition, Flame, presentation, non-lethal, Wind, combat-flow and teaching checks also pass. No hard Melee Animation assembly reference was added.

Live-game behavior is not yet verified. Test ordinary frontal/rear hits, Lightning Flash, Flame direct strikes versus splash, Wind multi-hit attacks, a counter target stepping away, non-lethal mode, manual orders, paired animations, disabled/missing Melee Animation, off-screen combat, and saves before/after counter impact. Restart RimWorld before testing.

The optional adapter uses Melee Animation's OnMeleeDodge, NotifyPawnDidMeleeAttack, TryGetAnimator, Seek and weapon snapshots; changes to those APIs may require an adapter update. It does not use execution animations or invoke their damage events.
