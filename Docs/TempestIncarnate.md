# Tempest Incarnate

> **World travel updated:** this lineage now flies along a visible world-map route and chooses its unroofed landing cell on arrival. Launch outdoors. No fuel or cooldown. See [Living world flight](LivingFlight.md) for current controls and saved-flight rules. Map Step remains an instant blink.

Implemented third stage of Wind: **Wind → Cyclone → Tempest Incarnate**. Dark grey cloud shapes and white lightning distinguish it from Thunder's yellow and Thunderstorm's crimson.

## Unlock and existing saves

Earn 300 **new** hostile, non-animal credited kills and spend 15 days as Cyclone. A separate saved tracker starts when an existing Cyclone loads this release, including caravan/world pawns. It never retroactively counts earlier kills or stacks another heart. Complete the free eight-second Transcend ceremony; interruption preserves readiness. God Mode exposes DEV: Transcend-Ready; disabling God Mode hides cached commands too.

Rebinding upgrades the same saved hediff identity. Wind Auto/Off/Non-lethal, Flow, personal clothing ownership and relevant remaining cooldowns survive. The old kit is replaced by bound Tempest gear. No helmet. Downing/death uses the established transformation cleanup and personal-clothing restore path.

## Seven forms

| Form | Mechanics | Cooldown |
| --- | --- | --- |
| Cloudrend Crescent | Clear 20-cell lane, broad right-facing crescent; cap 24 unique victims. | 3 s |
| Whitewake Pursuit | Checked dash up to 18 cells; empty adjacent landing, one cut. | 4 s |
| Stormwheel Waltz | Eight-cell spin, up to 20 unique victims once. | 7 s |
| Stormveil Reversal | Watch selected foe for up to 2 s; ordinary adjacent melee parry/dodge triggers cloud retreat and one counter-rush. No counter recursion. | 9 s |
| Heaven-Rending Squall | Approach up to 18 cells, three distinct timed cuts with checked footwork, brief lift and descending finisher. Three committed impacts maximum on the primary. | 14 s |
| Sovereign Maelstrom | 0.4 s formation, cap eight local victims, 1.25 s hold (0.5 s against sovereigns), one release impact and safe push up to three cells. | 22 s |
| Crown of the Tempest | Mark fixed location within 28 cells, 1 s telegraph, 10-cell impact cap 32; the rest of the funnel is visual. | 40 s |

Each impact uses one physical packet and one smaller pressure packet, both derived from the current melee profile. The lineage melee multiplier is already in that profile; it is never multiplied a second time. Common 20% damage tuning applies once per eligible packet. Cooldowns are spent when a valid job starts, including interrupted jobs. Non-lethal substitutes temporary suppression for wound packets.

Different forms build three Storm Flow stacks within eight seconds. Squall, Maelstrom or Crown consumes full Flow for 20% greater committed strike damage and a 120-tick ward reducing remaining incoming damage by 20%. This upgrades Wind's existing saved Flow rather than adding a second stacking resource.

## Sovereign benefits

Healthy native capacities: Moving 650%, Consciousness 200%, Manipulation 300%, Sight/Hearing 250%. +75 mood, ×3.2 melee stat, ×0.04 incoming damage, recovery up to four injury severity per second, common disease and Melee Animation knockout protection. ×15 carrying capacity and ×4 individual walking caravan-speed contribution; mixed caravans average contributions. Disperses 65% of remaining bomb damage and reduces electrical-shock chance/duration by 75%.

## Cloud Step / Tempest Travel

Right-click cycles **Step Auto → Step Manual → Travel**. Left-click Step toggles automatic stepping; Shift-left-click chooses any safe map cell without sight, fog or range limits. One-second map-step recharge; no resource cost. Auto follows actual movement paths and preserves work/carried passengers.

Travel left-click selects another world tile, flies there and offers exact unroofed landing on arrival; Shift-left-click flies with automatic landing. Canceling planning leaves the pawn at its origin. A blocked selected cell retains the original pawn safely in transit, with landing and return controls. World-flight destination reservations and saved departure/arrival use the shared original-pawn holder; same-map world-travel relocation has been removed. No fuel, distance cost or world cooldown. Put down carried things before world travel. Bestow grants ordinary Wind.

## Presentation and bounded work

Native south/east/north textures for Male, Female, Thin, Fat, Hulk, Child, Baby and FemaleBB; west uses native mirroring. Full mechanical below-neck coverage, indestructible gear, correctly oriented katana and native Melee Animation grip/blade profile. Halo/back aura require transformation, drafting, consciousness and standing.

Dedicated eight-frame storm sequences interpolate in C# using cached transparent materials, 192 effect slots and 64 afterimages. Airbound ribbons reuse six frames around the existing bounded 64-slot control registry. Camera/fog culling, bounded local target searches, saved commitment sets, phase/route/counter ownership, immunity and unrelated-stun preservation apply. New orders, loss of the caster or interruption clean owned captures and effects. No tornado pawns/buildings, permanent terrain, roof removal, building damage, climate/weather changes or uncontrolled projectiles.

## Verification

[Native combat, body fitting, travel and save/load results](TempestIncarnateVerification.md).
