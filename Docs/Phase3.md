# Phase 3: passive lightning and charge combos

Phase 3 includes Phases 1 and 2. Requires RimWorld 1.6 and Harmony; no DLC dependency. Built against the local 1.6.4871 assemblies.

## Upgrade
Close RimWorld, back up your save, and replace the existing ElementalHearts mod folder with this package. Keep only one installed copy and keep Harmony enabled before this mod. Existing implanted hearts acquire the new passive component when the save loads. The installed game folder has not been changed by this delivery. Test on a copy of your colony first.

## Behavior and balance
- Electrified Strikes: damaging ordinary armed/unarmed melee hits against eligible enemies add 0.5 times the melee profile as electrical damage and one Static Charge. A 25% roll starts a chain: up to three additional enemies within four tiles of each hop, line of sight required, no repeat target. First hop deals 0.4 times the profile, subsequent hops multiply by 0.75; at least six ticks between hops. Extra damage and chain hits cannot recursively proc chains.
- Thunder Reflex: a successful vanilla melee dodge has a 50% counter chance, with a 120-tick (two-second) internal cooldown. The counter runs on the next tick if both pawns remain valid and adjacent, dealing one physical profile plus 0.5 electrical profile and applying charge. It cannot interrupt Heavenly Thunder's channel. It uses a guaranteed impact rather than another dodgeable melee verb, preventing counter loops.
- Static Charge: up to five stacks, refreshed to ten seconds. Ordinary electrified strikes, counters, chain hits and Thunder Sever apply it. The latest applier owns the mark; changing owners resets it to one stack. Lightning Dance consumes the caster's stacks for +0.6 electrical profile per stack; Heavenly Thunder consumes the primary target's stacks for +1.0 per stack. A caster-attributed kill of their charged target reduces each active cooldown by 90 ticks (1.5 seconds), once per death, including a consuming lethal strike. Other killers get no rebate.
- Resistance: self-attributed EH_Lightning is absorbed. Other EH_Lightning and EMP packets are reduced by 75%, before downstream damage handling. This combines with the implant's existing general incoming-damage modifier. Vanilla weather lightning uses Flame, so it is NOT covered by electrical resistance; ordinary fire is not classified as electricity. Other mods can explicitly opt their DamageDefs in through ElementalHearts.ElectricalDamageExtension or the implant's electricalDamageDefs list.
- Aura: idle, combat and activation states surround the normal pawn without replacing its body. Combat means drafted or damaged/activated recently. Activation briefly brightens the effect. Blink/counter afterimages borrow a clean native cached pawn frame where available; otherwise a lightning streak is used. Held weapons may be absent from cached silhouettes.

All balance values are centralized in Defs/LightningHeart.xml. Passive attacks do not require drafting, but do require a living, awake, standing, unstunned, non-mental pawn capable of violence. Friendly fire is disabled by default and applies to passive target selection too. It does not alter vanilla melee targeting. Electricity retains the Phase 2 EMP rider against mechanoids.

## Implementation and limits
Exactly three small Harmony postfixes observe ordinary melee damage, the successful-dodge sound branch, and pre-damage processing. The dodge hook was checked against the installed 1.6 method IL; another mod replacing that branch can bypass it. The melee hook sees the returned damage result; a modded multi-packet attack whose final packet deals zero damage can fail to proc even if an earlier packet injured the target. No entire vanilla method is replaced.

Chains and counters have saved queues with reference/eligibility checks. Chain searches occur only every sixth tick, capped at eight due searches per map service tick. Limits: 32 active chains, 64 pending counters, 96 transient visual slots per map and 64 visible pawn auras per draw. Completed queue objects and effect slots are reused. Aura pawns are registered by implant lifecycle hooks and rebuilt once on map load; no all-map target scan runs every tick. Offscreen/fogged effects are suppressed. These are implementation bounds, not a measured large-raid performance guarantee.

The supplied transparent atlas remains unchanged. Aura motion is procedural animation of static atlas art. The reference board is not a usable animation strip, so these are not newly illustrated frame-by-frame sprites. In-game visual acceptance is still needed; headless tests cannot verify graphics.

## In-game acceptance
1. Reload a copy of your Phase 2 save. Confirm the implanted pawn retains four buttons and gains the aura without losing normal hair, apparel or body rendering. Check idle, drafted and activated states; pan offscreen and back.
2. Fight with fists, then a melee weapon. Check extra electrical injuries, bounded chains and charge stacks/expiry. Put a wall between potential chain targets; check allies and prisoners with friendly fire off.
3. Let an enemy attack the implanted pawn. A genuine dodge can produce one counter/afterimage; repeated dodges within two seconds must not produce extra counters. Ranged misses are not dodges for this feature.
4. Build several charges and use Dance; repeat with Heavenly Thunder. Confirm stack removal and increased electrical impact. Kill a charged enemy and inspect the 1.5-second cooldown refund; have another pawn kill one and confirm no refund.
5. Save/reload during combat, then leave/re-enter the map or caravan. Confirm no duplicated implants, stuck charge, or stale effects. Remove the implant and confirm the aura/passive behavior stops.
6. Review the debug log with your normal mod list. Combat and rendering overhauls require their own compatibility test.

Phase 4 remains separate: automatic combat, optional equipment and the broader settings/compatibility/performance pass. See Phase3RuntimeChecks.txt for executed checks, distinct from this manual checklist.
