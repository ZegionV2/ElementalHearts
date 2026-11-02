# Tempest Dragon

Tempest Circuit's fifth skill slot is now **Tempest Dragon**, with a new command icon and a native chain-dash job. Select a standing hostile pawn within ten cells. The Ascendant approaches in two straight legs with a sharp side bend, lands adjacent to/behind the enemy, strikes once and then searches for another unvisited hostile within eight cells of that landing. Up to five distinct enemies can be visited. The preferred bend side alternates at each approach, producing angular zigzags even against a row of enemies. Movement interpolates along straight segments; there are no curved leaps or homing arcs.

Each movement leg lasts 4–10 ticks depending on distance. Every leg checks its stopping point and its next step for obstructions. Bend and landing points must be empty, standable and unfogged; walls and blocked diagonal corners prevent passage. The attack stops if no safe two-leg route remains. No direct-line fallback skips the required bend. At each chain search, at most twelve nearby eligible unvisited enemies are considered for routing. Moving targets can escape their planned landing: the strike is skipped if they are no longer adjacent, and that visit still counts toward the five-visit limit.

Each eligible adjacent victim gets at most one committed strike per cast, using a weapon-derived attack profile (floor 10), 1.0 physical and 1.4 electrical multipliers, plus existing global modifiers. These are the former Circuit's per-victim total base multipliers, delivered in one strike. Successful surviving hits apply one Charged mark. Existing armor, parry and non-lethal rules apply. The chain targets hostiles only, excludes prisoners and standing-ineligible pawns, and does not graze bystanders. Cooldown remains **18 game seconds** and starts only after the job launches. There is no resource cost. An interrupted cast spends its cooldown.

New orders, downing, death, sleep, stun, loss of evolution, a controlled colonist becoming undrafted or external paired-animation control can interrupt the attack. The shared ability gate follows the current job; it releases immediately when that job ends or is replaced. Normal completion has a six-tick recovery. Busy state is not stored in a global flag. Native save output includes the current segment, endpoints, progress, visited enemies and committed-hit IDs. The job's three targets store the current victim, bend and landing. Committed IDs are recorded before damage to prevent repeat strikes after saving.

## Effects and menu

Eight generated RGBA frames animate a narrow white-hot flash with red lightning branches, a pointed leading edge and fading shards. A line grows from each segment's start to the pawn's current interpolated location, leaving the flash behind the pawn. The full line remains briefly after the leg ends. Red regalia afterimages and impact bursts accompany the chain. Existing afterimage and opacity settings apply.

The trail controller reuses 128 fixed slots with generation tokens, so an old handle cannot alter an overwritten trail. Eight materials are cached. No effect GameObjects, pawn portrait captures or global pawn scans are created. View/fog/opacity checks skip drawing where appropriate. This describes bounded implementation costs, not a measured FPS result.

The existing fifth component and saved cooldown key are retained internally for compatibility. There is one renamed fifth button, not an additional sixth skill. Auto Combat uses the chain whenever enabled, ready and a safe route exists in an allowed combat job, including against a single enemy. Other Thunderstorm skills, Scarlet's eight-frame spin, the halo, evolution and apparel reveal remain in place.

The old Circuit and Scarlet pulse-zone records can still be read from saves, but pending legacy zones are discarded on the next map tick. They do not continue damaging pawns after the overhaul. Existing cooldown progress is retained.

## Validation and installation

Built against the installed RimWorld 1.6 and Harmony assemblies. All 22 automated verification scripts passed. New tests cover alternating two-line routes, blocked/occupied/range rejection, native job/toil structure, linear draw positions, hit-commitment ordering, fifth-slot dispatch, replacement-order gate release, a 10,000-insertion fixed-pool check, stale handles, legacy-zone cleanup and native Scribe save output. Existing Scarlet, blitz, evolution, equipment, teaching, targeting, non-lethal and sword-profile checks also passed.

Native save loading, live combat, GPU rendering, visual placement and frame-time performance were not exercised headlessly. Fully restart RimWorld. In game: select a Thunderstorm Ascendant, draft/transform, select Tempest Dragon and choose an enemy within ten cells; test a group, enemies in a row, a lone enemy, walls, occupied stopping points, a moving target and a new order during the rush. Check each enemy is struck only once and all skills unlock after the chain. Test saving/loading during both legs and confirm the restored skill menu and cooldown.

The installed update is backed up under Backup-before-Tempest-Dragon. Install-Manifest.json records changed-file hashes and previous hashes.

## Art provenance

Generated with the built-in image_gen tool. The art archive includes the original dash sequence and command icon sources, exact prompts, eight 256×256 RGBA game frames and one 256×256 RGBA command icon. All frames use identical cell cropping and scaling. Tempest-Dragon-Preview.png is an asset contact sheet, not a screenshot from RimWorld.
