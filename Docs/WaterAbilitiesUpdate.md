Current lineage/aura update: **WaterLineageUpdate.md**. The notes below document an earlier phase. Water teaching and disciples are now playable; Glacial evolution remains planned.

# Water Ascendant — active techniques and Pressure

The four Water techniques are now playable manually and through the existing shared Auto Combat control. The Water armor, enclosed helmet, bound Tidal Sword, recovery, normal stats and original Still Water/Returning Tide component are retained. Water teaching and Glacial evolution remain future work.

## Skills

| Skill | Reach | Cooldown | Behavior |
| --- | --- | --- | --- |
| Torrent Cleave | 10-cell lane | 5 seconds | A travelling water crescent hits each eligible victim once, clips at walls, and applies Pressure on a damaging surviving hit. Up to 16 victims. |
| Flowing Step | 6-cell landing | 7.5 seconds | An eased dash to an empty melee flank leaves a water wake and afterimages, then strikes once on arrival if the target remains adjacent. Applies Pressure on a damaging surviving hit. |
| Whirlpool Dance | 4-cell radius | 12.5 seconds | One circular impact, shown with eight rotating water frames. Applies Pressure on damaging surviving hits. Up to 16 victims. |
| Ocean's Judgement | 12-cell target, 2.5-cell burst | 30 seconds | A 0.4-second target-following sigil precedes one water-column impact and a controlled burst. Consumes only the caster's Pressure after a damaging primary hit. Up to eight secondary victims. |

No resource cost. A conscious, drafted player-controlled pawn must wield a melee weapon and be free of transformation, evolution, paired lessons or another controlled action. These powers share the existing Water defense availability policy. Ranges are in map cells and durations use 60 game ticks per second; pausing/speed settings affect real elapsed time.

The techniques use one weapon-derived blunt water-pressure injury packet per eligible victim per cast. Base multipliers are 2.3 / 3 / 2.5 / 10, with Judgement gaining 0.75 per owned Pressure stack (maximum 12.25). Judgement's secondary burst uses 1.5. The existing global elemental damage reduction, ordinary armor, eligible parry rules and shared Non-lethal suppression still apply. These are damage multipliers, not guaranteed kills. No damaging persistent whirlpool, building damage or repeating column hits are added.

## Pressure and automatic combos

Successful damaging Cleave, Step, Dance and Returning Tide hits apply one Pressure to a surviving, standing target. Each caster owns up to three stacks, refreshed for eight seconds. The strongest caster's mark slows movement by 5% / 10% / 15%; marks do not add together across casters. Judgement consumes only its caster's stacks after positive primary damage. Suppressed, parried, fully absorbed and lethal/downing hits do not build Pressure. Each target stores at most eight caster entries, evicting the oldest expiry if necessary.

Use the existing **Auto Combat: On / Off / Non-lethal** button; no second automation toggle is added. On chooses Cleave → Step → Dance → Judgement when each is ready and valid, including against a single enemy. Three owned Pressure stacks prioritize Judgement; two nearby hostiles prioritize Dance. Cooldowns, clear routes, range and landing occupancy are respected, with fallback to another ready technique. Off leaves manual techniques available and disables automatic Water techniques, parries and counters under the existing player-control policy. Non-lethal uses the shared suppression behavior for the techniques, counters and normal melee.

Automation cooperates with melee attack orders, drafted combat waiting and optional Search and Destroy approach jobs. Queued player orders and paired lessons are protected. Pending Returning Tide counters get a short bounded opportunity before the next automatic technique. Interrupting a technique spends its cooldown. A new order, undrafting or incapacitation releases the job-based busy gate; there is no separate global busy flag.

## One-hit and performance bounds

Victim IDs are committed before damage and saved with the technique job. A parried/suppressed/blocked attempt cannot be rerolled by another visual frame. Judgement's primary is excluded from its secondary burst. Cleave checks only a small area around its moving front and stops hit queries at the 16-victim cap; all area queries have victim caps. Each cast snapshots its weapon-derived attack profile.

The visual system caches materials and meshes, limits each map to 128 effects and 32 afterimages, culls unseen/fogged visuals and drops expired references. These are structural bounds, not a measured FPS claim. Flowing Step checks its next route segment and final landing each tick, moves the pawn through the existing safe movement helper and smooths the drawn position. Ghosts use the existing Water apparel silhouette rather than capturing the pawn's face/hair.

## Frames and artwork

Generated with the built-in image_gen tool: **four icons and 40 transparent 256×256 RGBA effect frames**. Torrent, Flow, Whirlpool, Judgement and Sigil each have eight distinct frames. Wake/column baselines and ring centers are aligned across their sequences. The original armor, helmet, sword and transformation art is unchanged. Existing afterimage settings are respected. The preview is an asset contact sheet, not a game screenshot.

ArtSource/Generation-Prompts.json contains all nine exact generation prompts and original generated source paths. ArtSource holds the original generated sheets/icons. Water-Abilities-Art-and-Prompts.zip includes those sources, the exact prompts, all 44 integrated game sprites and the preview.

## Save compatibility

The existing Water defensive component and save keys remain: waterAuto, waterNonLethal, waterReadyAt, waterCounterTarget, waterCounterAt, waterElapsed and waterStruck. The existing same-object component upgrader appends the new abilities component without replacing the saved Water hediff, comp list, defense/transform instances or foreign components. Its saved mode, cooldown, pending counter, recovery and equipment references are retained.

New namespaced keys save active cooldowns, combo progression, Pressure owners/expiry, movement/impact cells, hit IDs and attack snapshots. Pending counters get a new bounded deadline with fallback for older pending saves. Cosmetic effects are reconstructed from live job progress rather than being saved as persistent objects. Save-output tests do not establish full native save-load compatibility.

## Validation and installation

Compiled against the installed RimWorld 1.6 and Harmony assemblies. **All 29 automated verification scripts passed**, including the existing evolution, Bestow, equipment, transformation, combat, teaching, non-lethal, Water defense/recovery and optional sword-animation profile checks. New checks cover combo order/fallback, Pressure ownership/expiry/bounds, all eight frame indices, one-hit commitments, native held-job cleanup, single-packet/parry/non-lethal ordering, bounded visual pools under 10,000 insertions, preserved Water component state and native Scribe save output for the new state.

Reference fixtures verify saved keys/IDs rather than loading complete Pawns. **Live combat, actual automated combo execution, GPU placement and full native save loading remain untested.** Fully restart RimWorld after installation.

In game: draft a melee-armed Water Ascendant, enable On and issue a melee attack. Check the four buttons and single-target rotation; use two or more enemies to observe Dance priority. Confirm Pressure reaches three, expires and is consumed by Judgement. Test Off/manual and Non-lethal, walls, a newly occupied dash landing, player interruption and downing. Save/reload during a dash, Cleave and Judgement windup, then check cooldowns and no repeated impacts. Inspect effect positioning with your rendering mods and original transformation in all directions.

Runtime ZIP includes the full mod and compiled assembly; source/tests are also available separately. The install changes no existing artwork and removes no files. Previous changed files are backed up in Backup-before-Water-Abilities, with before/after hashes recorded in Install-Manifest.json.
