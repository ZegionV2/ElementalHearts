# Glacial Ascendant

Glacial is now Water's playable evolution. An ordinary Water Ascendant qualifies after **150 additional unique hostile non-animal kills and ten ascendant days**. The Evolve button performs the shared interruptible 4.5-second ritual, with Glacial frost visuals. DEV: Evolution-Ready appears only while dev mode is on and sets the requirements; Evolve still performs the ritual.

The upgrade rebinds the existing Water hediff rather than adding a second elemental root. Its identity, age, defensive component, parry recovery, pending counter, four active cooldown entries, combo state, automation/Non-lethal flags, stored personal clothing and Pressure ownership retain their existing save formats. Existing Water saves acquire the new mastery component without losing their old state; historical eligible kills cannot be reconstructed, so unique kill tracking starts with this update.

Glacial receives the shared evolved defense, **5× elemental movement factor and +50 total ascension mood**. The mood replaces Water's +30. Still Water's 80% frontal parry and Returning Tide retain their existing recovery and recursion guards. Normal recovery is retained. Bestow Water reuses the shared icon and face-to-face ceremony, granting ordinary Water ascension directly.

## Regal kit and halo

The indestructible kit consists of one white/ivory regal outfit and a silver-white crystalline sword with a cyan core. The short cloak has gold trim and pale cyan faceted inlays. The compact, low-necked RimWorld silhouette has tucked shoulders and closed side/back coverage. There is **no helmet**.

All seven body types have south, east and north textures: Male, Female, Thin, Fat, Hulk, Child and Baby. Native west mirroring uses the east texture. Each texture is registered to the established fitted regal template's bounds. The inventory icon and weapon texture are separate runtime assets. The six-frame fractured-ice halo appears only while transformed and drafted; the normal Water ground aura does not stack beneath it. Transform/Revert retain the existing smooth apparel reveal and corrected hair handling. Downing restores personal clothing, and summoned gear stays bound rather than duplicating in inventory.

## Techniques

All five skills have no resource cost and share Water's existing Auto Combat and Non-lethal control.

| Technique | Reach | Cooldown | Behavior |
| --- | --- | --- | --- |
| Glacial Crescent | 12-cell lane | 5 seconds | One traveling crystalline blade front; walls stop it. Each victim is hit once; damaging surviving hits apply Pressure. |
| Frostwake Step | 7-cell landing | 7.5 seconds | Safe glide to a melee flank with a crystalline wake and fitted-regalia afterimages; one arrival hit. |
| Crystal Vortex | 5-cell radius | 12.5 seconds | One circular sword strike shown with eight rotating ice frames. Each target is hit once. |
| Absolute Zero | 14-cell target, 3-cell burst | 32 seconds | A half-second snowflake telegraph precedes one ice-column impact. Consumes up to three owned Pressure stacks; the primary is excluded from secondary hits. |
| Glacial Explosion | 4-cell radius | 40 seconds | Automatic crowd response when at least two visible living standing hostiles surround a transformed, combat-ready pawn. Safe outward knockback, a five-second freeze, icy-blue tint and mild real hypothermia. |

The first four techniques retain Water's saved jobs and one-hit commitments, with evolved reach, artwork and a modest 10% skill multiplier before the shared elemental adjustment. Pressure remains caster-owned, capped at three stacks and eight seconds, with the existing strongest-mark slow cap. Glacial Explosion has its own saved cooldown; it never expands or resets the old four-entry list.

## Glacial Explosion and freeze

The automatic response waits for the shared controlled-action gate, pending counters and queued player orders. It cannot trigger on a single enemy. NPCs do not need drafting. A manual button uses the same crowd condition, readiness and cooldown.

One explosion selects at most **16 victims** through clear lines, deduplicates target IDs and hits each once. Survivors are pushed up to three cells outward over nine ticks, with smooth draw positioning. Every traversed cell is checked for blocking terrain/buildings and other pawns. Pushes stop before walls, closed doors, occupied cells or the map edge, and add no collision damage. A blocked push still freezes the survivor in place.

Freeze lasts **300 ticks after the push ends**, stopping native movement and attacks through the native stun system. A per-map controller advances pushes every tick even when health updates are batched. Absolute push/freeze expiry and completion state are saved. Loading cannot restart the push or five-second timer. The state belongs to the victim and does not depend on the caster remaining alive or keeping a job. Downing, death or despawn ends the visual/control state. A separate three-second thaw immunity prevents another Glacial caster from refreshing an existing freeze.

The freeze tints native pawn render-node colors toward icy blue and draws a subtle looping shell/glint overlay. It does not change pawn skin/hair definitions, saved apparel colors or shared materials. Frozen rendering bypasses the native static cache, and the tint ceases with the status.

A successful harmful freeze adds up to **0.15 hypothermia severity**, with this skill's buildup capped at **0.30**. Higher existing severity is not lowered. Mechanoids and pawns with extreme cold comfort are excluded from the chill addition. It is a real native hypothermia condition, applied once rather than once per frame, and recovers normally afterward. **Non-lethal mode keeps the push and freeze but adds neither wounds nor hypothermia.** No ambient temperature simulation is used.

## Assets and budgets

The update adds **93 transparent 256×256 runtime PNGs**: 21 worn body/direction textures, one outfit inventory icon, one sword, six command icons and 64 animation frames. Bestow uses the existing shared icon unchanged.

- Eight frames each: crescent, wake, spin, telegraph, column and expanding explosion.
- Four frames: outward frost streak.
- Six frames each: freeze shell/glint and halo.

Animation frames retain equal cell registration and common scaling. Original generated atlases and prompts are supplied with the release. The artwork was generated with the built-in image generation tool; atlas extraction, fitting, resizing and packing prepare those generated images for the game.

VFX use cached materials, 128 transient effect slots, 32 afterimage slots and at most 256 tracked visual pawns per map. Gameplay freeze control is independent of the cosmetic budget, so crowded fights do not silently lose status cleanup. Drawing respects effect opacity, camera bounds and fog. All frost is temporary artwork; there are **no ice-wall buildings, building/terrain damage, permanent terrain changes, fires, ambient cooling or repeated frame-driven damage**.

## Verification and limits

The release includes automated regression results and an isolated native RimWorld fixture. Native checks exercise the actual evolution ritual, root/cooldown preservation, transformation, all seven body types in four directions, all 64 effect frames, all six icons, Bestow routing, the two-enemy trigger, wall/occupied-cell knockback, mid-push save/load, freeze expiry, color restoration, refreeze immunity, Non-lethal behavior and all four evolved techniques, including saving during Absolute Zero's telegraph.

Personal saves and settings were not used. Native rendering checks verify material resolution and shader behavior; on-screen placement/art appearance and the full third-party mod list have not been visually/play-tested. Existing unrelated recipe-maker/startup notices are recorded in the native log. Restart RimWorld completely before using the update.
