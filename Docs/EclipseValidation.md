# Eclipse implementation validation

Validated against the locally installed RimWorld 1.6.4871 assemblies, Harmony and Melee Animation. The final native run completed **248 checks**, using an isolated save-data folder and a genuine save created before Eclipse existed. The normal user save folder and active mod configuration were not used.

Verified:

- An existing Lunar job finishes and releases its skill gate; saved Celestial Bestow readiness unlocks Eclipse without new mastery kills.
- A real evolution ceremony resumes from a mid-ritual save, replaces Lunar in place, preserves its root load ID, controls, existing cooldowns, brands and personal gear.
- The readiness developer command follows God Mode even when the old command object remains cached.
- All seven body types resolve native regalia and half-mask graphics in south/east/north/west. The armor export's 28 native directional coverage checks have no uncovered body pixels. The front mask master contains no strap; native mask definitions preserve hair/beard flags.
- All 92 combat/halo/mark frames and seven command icons resolve. Eventide loads its native Melee Animation tweak and attack animation profile.
- Pursuit, crescent, spin, crossing waves, reversal and finishing wave each work. Mid-pursuit, telegraph, leap, rush and already-committed spin saves resume without duplicate impacts. Crossing waves share one impact per victim. New occupied landings cancel safely. Non-lethal attacks apply no new wounds.
- Auto Combat executes the four main combo forms. Replacing the job clears the owned ability gate.
- Surrounded combat can create one real eclipse. The map condition, cooldown and one Dominion boost survive saving/loading. Movement, melee damage, cooldown and defense factors match the intended Celestial core values.
- Leaving a map removes Dominion; returning below an eclipse restores one boost after spawning finishes. Ending an eclipse removes the boost. Natural eclipse expiry does the same, and the saved cooldown blocks chained summons.
- Paired Bestow stops both participants, resumes from a mid-ceremony save, grants normal Lunar exactly once and releases both participants.
- Native downing restores personal clothing and hides all three gear projections. Recovery and manual reversion reuse the kit without inventory duplicates or lost hair.

The movement planner was corrected to check the exact interpolated native-cell transitions, including diagonal corner cells. Eclipse boost refresh was deferred out of the health tracker's spawn enumeration to avoid changing that list while it is being iterated.

The final run's game log contained no missing-def, missing-texture, Harmony-patch or runtime exception reports. Material resolution and native coverage checks validate the assets; subjective appearance across every modded head/race combination remains dependent on that race's own render settings.
