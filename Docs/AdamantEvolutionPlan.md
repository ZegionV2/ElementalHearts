# Adamant Ascendant — proposed Earth evolution

Historical design proposal. Adamant is now playable; AdamantIntegration.md describes the shipped implementation. Proposed tuning below is retained for design context.

## Identity and progression

Earth becomes a royal living fortress: compressed basalt, adamantine facets, warm gold fault-light and decisive sword-driven seismic attacks. Keep the effects visibly rocky and warm rather than icy-blue, and keep the weight in sharp cracks, deep pressure and falling debris.

Use the existing evolution milestones: 150 additional credited unique hostile non-animal kills and ten days as a normal Earth Ascendant. Existing normal Earth age counts; begin any newly introduced mastery tracking with a clear migration rule. A free Evolve: Adamant button appears when ready. DEV: Evolution-Ready follows God Mode, including cached gizmos, and only sets readiness. A roughly 4.5-second interruptible coronation compresses rising stone fragments into a faceted halo, then manifests the evolved regalia. Cancelling retains readiness and grants no partial upgrade.

Upgrade the existing Earth condition in place to Adamant. The normal Earth Ascendant label must disappear; never retain two heart stat stacks. Preserve pawn identity, injury/recovery timers, personal equipment, drafted/transformed state, automation/non-lethal preference, four existing cooldown deadlines, Resolve and combat state. Unlock only the fifth command. No heart item or material payment.

## Outfit and weapon

- Use the accepted evolved regal template: white/ivory plate, compact royal cloak, bronze-gold edgework and warm amber/diamond accents. Low collar, tucked shoulders and complete native body coverage. Seven body types, south/east/north, native mirrored west and a separate inventory icon. Fit front and both side silhouettes explicitly.
- No evolved helmet. Normal Earth keeps its new basalt helmet. The evolved kit restores visible original hair and beard through Transform/Revert.
- A subtle six-frame faceted gold/amber halo appears only while transformed, drafted, alive and standing. No permanent ambient stone cloud.
- Worldbreaker: an indestructible fitted broad sword with ivory/bronze fittings, a dense charcoal blade and a bright adamant channel. Supply the native Melee Animation grip/blade profile.
- Downing restores personal clothes; recovery reuses the same hidden projections. No dropped or duplicated summoned armor/sword.

## Five techniques

These are starting tuning proposals, to be adjusted after native combat testing. The first four retain the same input roles as normal Earth. All use one shared Off / On / Non-lethal control, with the new evolved pawn defaulting to On only when no saved preference exists.

| Technique | Proposed behavior | Initial range / cooldown |
| --- | --- | --- |
| Adamant Severance | Upgrade Faultline Cleave into a broad, bright fault cut. A short sword windup precedes one weapon-scaled hit and stagger per enemy along the clear lane. Gain one Resolve per successful hostile cast. Walls and closed doors stop the cut. | 10-cell lane; 6 seconds |
| Mountain King's Advance | A heavy sword-first charge to an empty enemy flank, followed by one crushing arrival strike. Leave a brief gravel/adamant streak. Recheck each step and allow only one bounded route correction. No wall crossing or repeated contact damage. | 8-cell navigation envelope; 9 seconds |
| Sovereign Quake | An evolved ground stab sends a large circular amber fault ring outwards. One impact per enemy, roughly 25% above normal Quake's technique multiplier, a 90-tick stagger and up to three safe outward cells. Prioritize automatically when two enemies are close. | 5-cell radius; 18 seconds |
| Worldspine Eruption | A ground stab marks a fixed target cell, then drives up a taller crown of adamant-edged basalt spires. Telegraph first; one burst hit per victim, then up to four checked outward cells. Spend Resolve for stronger impact. The spires collapse into fading visual fragments. | 10-cell mark / 3.5-cell burst; 30 seconds |
| Adamant Bulwark | The additional reactive fortress skill. When at least two enemies are within three cells, a short brace forms floating faceted shield plates for three seconds. Reduce remaining personal damage by 40% and nearby same-faction allies' by 15%, using only the strongest guard. Resist this mod's forced pushes while braced; continue ordinary melee. No reflected damage, damage absorption pool, hit-triggered retaliation or extra attack pulses. | 3-cell ally radius; 35 seconds |

Bulwark can also be pressed manually when its crowd condition is met. It does not cancel an accepted ground stab. Automatic evaluation uses it at the next safe recovery point. Explicit movement orders, incapacitation, reversion, lost weapon, removal or external animation ownership clean up the brace immediately; the pawn is never held by a permanent movement lock. Stronger unrelated crowd control remains intact. Multiple Bulwarks cannot multiply together, and a caster never receives both its own and an allied copy.

## Power and automatic flow

Use the shared evolved stat tier: 500% elemental movement, +50 mood, stronger defense than normal Ascendants, improved recovery, disease resistance, 10x carrying and a 3x individual caravan contribution. Retain evolved Melee Animation knockout protection. Adamant should be an especially durable peer of the other evolutions; Celestial keeps its superior tier.

Adamant Resolve keeps the existing three-stack meter. Proposed upgrade: a twelve-second expiry, 7% remaining-damage reduction per stack and +15% Worldspine damage per consumed stack. Keep hit-source filtering, the existing gain throttle and one gain per successful Cleave cast. Do not add a second independent passive-defense condition.

Automatic priorities: finish the accepted action; Bulwark under close pressure at a safe boundary; Quake to create space; Advance if a useful clear route exists; Worldspine for a cluster or full Resolve; otherwise Severance and ordinary melee. No endless retarget loop. Manual attacks retain the selected intent and fixed eruption mark; automatic movement can replan once within its budget. All abilities are free of resource costs and use cooldowns for pacing.

Replace Teach with Bestow: Earth. It grants normal Earth ascension directly, skipping discipleship, through the existing paired wait/facing animation. Reuse Bestow's icon and cleanup rules. Do not randomize this evolved Bestow; Celestial alone retains Random Bestow.

## Art, audio and performance scope

Plan 21 regal worn textures plus inventory icon, fitted sword/profile, five skill icons, an evolution icon and six halo frames. Start with eight registered frames each for the severance trail, gravel charge, quake ring, spire eruption, Bulwark formation and Bulwark fade. Reuse normal Earth effect structure and sound-family routing; Adamant gets denser stone cracks and a longer restrained rumble, not thunder or ice shattering. Reuse the shared evolution/manifestation C# sequence and Bestow icon rather than introducing a large new cutscene overlay.

Retain cached materials, the bounded Earth effect pool, camera/fog culling, short lifetimes and bounded local searches. Cap each attack at 16 unique victims, with a fixed inspected-thing budget. All fissures, spires and shield plates are visuals. Spawn no ice/stone walls, buildings, tornado pawns or uncontrolled projectiles. Change no terrain, roofs, global weather or ambient temperature. Do no building or collision damage.

## Save migration and acceptance checks

Keep existing Earth save keys and numeric skill slots. Extend the cooldown list only for Adamant, preserving the first four deadlines. Save Bulwark expiry/ownership and each action's fixed center, movement budget, deadline and impact commitment before applying hits. Do not reroll mastery or reset saved automation on load. Gear replacement allocates the new kit before retiring old projections and preserves sword quality and original clothes.

Before shipping: test normal/Earth-disciple isolation; God Mode readiness visibility; in-place evolution and interruption; all seven body types/four directions; progressive hair reveal; downed reversion; Bestow cancellation; clear/blocked/occupied routes; wall and ally filtering; parry/counter recursion; non-lethal suppression; Bulwark stacking and expiry; and saving during brace, ground stab, charge, marked eruption and after impact. A loaded attack must never hit twice or leave a permanent skill lock.
