# Glacial Ascendant — evolution plan

Status: **implemented in the Glacial update**. See **GlacialUpdate.md** for shipped behavior and verification. The original proposal follows for design context. Ordinary Water is playable with four techniques, Pressure, defense, recovery, transformation, disciples and teaching. This document proposes its future evolution; it does not enable Glacial powers or add an Evolve button yet.

## Identity and appearance

Glacial Ascendant is Water's royal ice evolution: a fast defensive swordsman who turns flowing water into sharp crystalline waves, ice-edged movement and short, controlled freezes.

Use the accepted evolved outfit as the base: **white/ivory regal armor, short cloak, gold trim and pale cyan crystal accents**. Keep the compact RimWorld silhouette, tucked shoulders, closed side/upper-back lining, low neckline and no drawn limbs. **No helmet.** Use an indestructible silver-white crystalline sword with a blue core.

A floating **fractured-ice halo** appears only while transformed and drafted. Give it six subtle animation frames: crystal fragments orbit and softly glint. The ordinary Water aura is replaced by the halo after evolution. Hair remains visible with the helmetless outfit and follows the corrected personal-headwear transition on Transform/Revert.

## Progression and power condition

- Require **150 additional unique hostile non-animal kills and ten in-game days as a full Water Ascendant**, matching the other implemented evolutions. Disciple kills, friendly prisoners, animals and repeated kills of a resurrected victim do not advance mastery.
- Add a saved Water mastery tracker when the evolution update ships. Existing Water pawns retain their Ascendant age; unique kill tracking begins with that update because old total-kill records cannot reliably reconstruct eligible unique victims.
- Unlock **Evolve: Glacial Ascendant** when both milestones are met. Use the existing interruptible **4.5-second C# ritual**: gather water, spiral frost around the pawn, crystallize the regalia, then finish with a brief ice-ring pulse. No resource cost; interruption retains readiness and grants no partial upgrade.
- Replace the Water condition with one standalone **Glacial Ascendant** condition through the same saved-root rebinding approach as the existing evolutions. Preserve hediff identity, age, cooldowns, mode flags, Pressure ownership, recovery/counter state, sword bond and stored personal gear. Keep one elemental lineage and one set of skill controls.
- Match the existing evolved tier: **5× elemental movement factor**, **+50 total ascension mood**, stronger general defense and the same additional protection against ordinary Ascendant attacks. The +50 replaces Water's +30; it does not add to +80. Effective movement still combines with capacities and equipment.
- Keep Still Water's **80% frontal parry**, **120-degree cone** and **45-tick recovery per attempt**. Avoid automatic perfect blocking. Preserve the Returning Tide recursion guard and hold/drop rules for pending counters.

## Five active abilities

These are proposed starting values. All abilities are free and share **Auto Combat: On / Off / Non-lethal**. Use the current weapon-derived damage system and global elemental damage adjustment. The improvement comes from evolved stats, modest damage/penetration increases, reach and control.

| Ability | Proposed reach / cooldown | Mechanic |
| --- | --- | --- |
| **Glacial Crescent** | 12-cell lane / 5 seconds | An ice-edged water crescent sheds small crystalline shards. One impact per eligible victim, at most 16. Stops at walls. Slightly stronger damage and penetration than Torrent Cleave; applies Pressure on a damaging surviving hit. |
| **Frostwake Step** | 7-cell landing / 7.5 seconds | Glide to an empty melee flank with a curved frost wake and fading regalia silhouettes. One arrival cut. Check walls and the landing continuously; interruption keeps the spent cooldown and releases control. Applies Pressure on a damaging surviving hit. |
| **Crystal Vortex** | 5-cell radius / 12.5 seconds | One spinning ice-sword attack, shown through eight sequential frames. At most 16 victims, each struck once. The fading crystal ring and floor frost are cosmetic. Applies Pressure; its existing slow remains capped. |
| **Absolute Zero** | 14-cell target, 3-cell burst / 32 seconds | A 0.5-second snowflake sigil follows the target, then a focused ice column lands once. Consume up to three of the caster's Pressure stacks for bonus primary damage. At most eight secondary victims; exclude the primary from the secondary burst. No map-temperature change or repeated impact damage. |
| **Glacial Explosion** — new fifth skill | Automatic crowd response; proposed 4-cell radius / 40 seconds | When **two or more eligible enemies** are close to the transformed pawn, release one radial ice explosion. Strike each victim once, knock survivors up to three cells outward, then **freeze them for five seconds (300 ticks)**. Frozen enemies turn **icy blue** and receive actual **hypothermia**. |

Keep **Pressure** at three stacks per caster, refreshed for eight seconds, with the strongest mark supplying at most 15% movement slowing across casters. Retain its existing ownership and save format. An ice-themed mark effect can change its appearance without adding another stacking slow.

### Glacial Explosion: fifth-skill behavior

- **Trigger:** Auto Combat is on, the pawn is transformed and combat-ready, the fifth cooldown is ready, and at least **two living, standing hostiles are within four cells with a clear line**. No single-target fallback. It works for NPCs without requiring drafting. Prisoners and allies never count toward the trigger.
- **Timing:** Recheck the crowd when the cast commits; spend the cooldown once and release one radial explosion. Do not interrupt an active movement, counter, transformation, evolution or paired ceremony to force the cast. Check again as soon as the shared action gate is free. There is no separate permanent busy flag.
- **Knockback:** Push each surviving target up to **three cells away** along a checked outward route. Stop at walls, closed doors, occupied cells or the map edge; never teleport through blockers or stack pawns. Freeze the target at its safe final position, even if the push has no room. The push has no extra collision-damage packets.
- **Freeze:** For **300 ticks after the push**, the victim cannot move or attack. Use one temporary target-side status and owned action hold; save its expiry and release it independently of the caster's current job or survival. Death, despawn and invalid state must clean up the hold. Restoring a save cannot restart or extend the duration.
- **Appearance:** Blend the victim's rendered body, apparel, hair and weapon toward pale **icy blue**, with small crystalline glints and a compact ice shell. Keep the pawn recognizable. The tint follows the freeze status and clears immediately when it ends; do not change skin/hair colors, apparel colors or saved graphics permanently.
- **Hypothermia:** Apply a real, mild hypothermia increase on the successful freeze, rather than a cosmetic label or a map-temperature change. The proposed first tuning is **+0.15 severity**, capped at **0.30 from this skill**; never lower a target's existing higher severity. Apply once per cast, not every animation frame. Cold/status immunities are respected. The frozen hold/tint end after five seconds; hypothermia then recovers through ordinary game rules.
- **Bounds and stacking:** At most **16 victims**, deduplicated by pawn ID and committed before impact, including blocked or suppressed attempts. An already frozen target cannot have its timer refreshed by another Glacial caster. Use a shared **three-second refreeze immunity after thawing** to prevent continuous multi-caster locks. The source is always excluded; the existing friendly-fire policy governs incidental allied hits without allowing allies to trigger the ability.
- **Non-lethal:** Preserve the shared suppression behavior. Keep the bounded push and five-second blue freeze, but suppress wound damage and harmful hypothermia buildup in this mode. Cleanup and immunity work the same way.

The user-specified crowd threshold, knockback, five-second freeze, blue tint and hypothermia define the skill. Radius, push distance, damage, hypothermia strength and cooldown are starting tuning proposals for implementation.

## Automatic combos and controls

Start with **Crescent → Step → Vortex → Absolute Zero**, selecting only ready, valid techniques. Three owned Pressure stacks prioritize Absolute Zero. **Glacial Explosion takes priority at the next free action gate whenever at least two nearby enemies satisfy its trigger.** It has no ordinary-rotation or single-target fallback.

Use the existing combat-order policy, safe movement helpers and queued-order protection. Each technique owns control through its current job. No separate persistent busy flag, duplicate automation toggle or new resource bar. Save the fifth cooldown separately, for example `EH_glacialExplosionReadyAt`, so the current four-entry Water cooldown list keeps its existing format. Frozen Dominion has not shipped, so no Dominion save-key migration is needed.

Replace **Teach** with **Bestow Water** on evolved pawns. The existing four-second paired ceremony grants ordinary Water ascension directly, skipping discipleship. Both pawns stop and face each other. Ordinary Water Ascendants continue teaching disciples normally.

Add **DEV: Evolution-Ready** for unevolved Water Ascendants when this evolution actually ships. It sets the milestones; the normal Evolve button performs the ritual. Visibility and action eligibility must read dev mode dynamically, as with the existing fixed commands.

## Assets and implementation

Generate the regal armor in south/east/north for all seven body textures, with native west mirroring, plus an inventory icon and fitted sword. Produce five skill icons, an evolution icon and six halo frames. Reuse the shared Bestow icon. Start with eight aligned frames each for crescent, wake, spin, telegraph and column. For Glacial Explosion, generate a dedicated radial ice-burst icon, **eight expanding explosion frames**, a short **four-frame outward frost streak**, and **six subtle looping freeze-shell/glint frames**. Drive knockback, the blue tint and their timing in C#; reuse effects through positioning rather than generating separate art per victim.

Keep the existing bounded effect/afterimage pools, cached materials, camera/fog culling and bounded local target searches. All frost decals are short-lived visuals. Avoid spawned ice walls, building damage, permanent terrain changes, ambient temperature simulation and uncontrolled extra damage packets.

Implementation order:

1. Add mastery tracking and the standalone Glacial lineage identity; extend Water recovery, defense, teaching/Bestow and shared evolved-tier recognition.
2. Add the regal kit, sword and halo while preserving hidden ownership and downed reversion.
3. Rebind the four existing Water techniques to Glacial tuning/art without resetting their saved cooldowns.
4. Add Glacial Explosion with its automatic two-enemy trigger, separate saved cooldown, safe outward knockback, bounded one-hit commitments, saved five-second freeze, reversible blue tint, mild hypothermia and expiring refreeze immunity.
5. Add the evolution ritual, dynamic dev command and clear shipped/planned descriptions.
6. Verify old Water saves, mid-ritual and mid-technique saving/loading, cancellation, parry recursion, occupied routes, target deduplication, multiple Glacial casters, Non-lethal mode and automatic combos. For the fifth skill, test one enemy versus two, blocked sight lines, safe pushes around walls/doors/occupied cells, saving during knockback/freeze, caster death, victim despawn, exactly five seconds of freeze, tint restoration and one hypothermia application without lethal buildup in Non-lethal mode. Inspect armor/head/halo fit in every direction/body type, including hair during both Transform and Revert, and confirm no duplicate gear or permanent skill/movement lock.

The gameplay numbers above remain tuning proposals until Glacial is implemented and tested.
