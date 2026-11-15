# Aurora Incarnate — proposed third Water stage

Status: implemented. See [the shipped mechanics](AuroraIncarnate.md). This document retains the original design targets. The pack contains 234 runtime PNGs including shared Bestow, with 196 effect frames across 25 sequences and 32 native body/facing fit checks. Runtime integration is present. Numbers below remain proposed tuning targets.

## Direction

**Water → Glacial Ascendant → Aurora Incarnate.** A sovereign of winter and northern lights with rapid chain-dash swordsmanship, aurora curtains, snow wakes, ice-mirage counters, controlled freezing and powerful shatter finishers. Core strength should match Celestial, Thunder Incarnate, Hell Flame Incarnate and Tempest Incarnate. Its specialty is controlling an opening and exploiting it without permanently locking another sovereign.

Anomaly/Nociosphere-inspired effects have been set aside for a future new element. This lineage uses genuine aurora-borealis-inspired green/cyan/violet light curtains together with crystalline ice, frosted afterimages, snow wakes and refracted light. Frost and snow remain prominent in every major form; aurora is a light layer within the Water lineage. It does not use the shelved Anomaly travel presentation.

## Unlock

- 300 new credited hostile non-animal kills and 15 days as Glacial, matching the existing third-stage progression pattern.
- A free eight-second Transcend ceremony. Interruption preserves readiness.
- God Mode exposes DEV: Transcend-Ready; turning God Mode off immediately hides the command.
- Existing Glacials begin a separate saved third-stage mastery tracker when the feature ships. Do not count the earlier Water-to-Glacial kills again.
- Upgrade the existing heart rather than stacking a second Glacial/Incarnate hediff. Preserve original identity, personal clothing, Pressure ownership, combat preferences and relevant remaining cooldowns.

## Proposed seven combat forms

Numbers are initial design targets, not implemented values.

| Form | Behavior | Initial tuning |
| --- | --- | --- |
| **Rimefall Crescent** | A broad silver-white, cyan-edged sword crescent. One committed hit per eligible victim along a clear lane; builds owned Pressure. | About 22 cells; 3-second recharge. |
| **Aurora Blitz** | Chain between up to five distinct enemies along fast checked straight segments, with sharp direction changes, frosted afterimages, snow wakes and long aurora ribbons connecting the cuts. Each victim is hit once. With a lone elite, use one stronger committed dueling cut instead of repeating five hits. | Initial approach up to 18 cells; subsequent links within 8 cells; 5-second recharge. |
| **Frostcrown Waltz** | A sweeping spin with layered crescent frames and crystal glints. The animation can rotate repeatedly while each target is struck once. | About 7-cell radius; 7-second recharge. |
| **Aurora Dominion** | The signature northern-lights ability: tall translucent green/cyan/violet curtains unfurl above a frost seal, ripple around the selected area, and fold inward into a snow-laden crystalline impact. Formation, looping curtains and release have separate sequences. One committed area hit with brief freeze control; the moving lights never add damage ticks. | About 24-cell targeting, 6-cell burst, 0.5-second warning; 18-second recharge. |
| **Aurora Cataclysm** | Upgraded Glacial Explosion. Automatically answers two or more nearby pressuring enemies with an expanding ice blast, checked outward knockback, an icy-blue freeze and bounded hypothermia on suitable biological victims. | Two enemies within 4 cells; about 6-cell burst, up to 4-cell push, 5-second ordinary freeze; 24-second recharge. |
| **Mirrorfrost Reversal** | An ordinary adjacent melee hit meets a cracking ice mirage. The caster retreats to a safe cell, then rushes back through a refracted frost trail for one counter slash. Useful against a lone powerful opponent. | One parry/counter commitment; 9-second recharge. |
| **Throne of Endless Winter** | Grand finisher. A huge suspended crystalline crown forms above the chosen area within an arch of northern lights, opens into an ice-flower silhouette and collapses in one dramatic snow-and-shatter impact. The many falling shards remain visual. | Up to 28-cell targeting, 10-cell burst, 1-second warning; 40-second recharge. |

Proposed combo: **Aurora Blitz → Frostcrown Waltz → Rimefall Crescent → Aurora Dominion or Throne of Endless Winter**. Cataclysm interrupts crowd pressure when it is safe to act; Mirrorfrost supplies a separate defensive answer in a duel. Automation should select an appropriate shorter combo when range, obstacles, cooldowns or enemy strength demand it. Major offensive forms must remain available against one sovereign opponent.

## Pressure, defense and freezing

Retain and upgrade Water/Glacial's existing caster-owned **Pressure**, capped at three stacks per caster/target. Do not add a second stacking resource. Dominion and Throne can consume their caster's Pressure for a bounded shatter bonus, initially up to 20% on that same committed impact. They cannot consume an ally's marks or add a hidden extra damage packet.

Preserve Still Water and Returning Tide through the upgrade, coordinating their counters with Mirrorfrost so one incoming hit cannot start two counters. Counter damage cannot trigger another counter. Give the new reversal a clear owner, cooldown and cleanup path.

Ordinary enemies retain the signature five-second freeze from the crowd-response form. Sovereigns receive much shorter hard control, initially around 0.75–1 second, followed by victim-wide thaw protection. Different casters must not bypass that protection. No indefinite freeze refresh, permanent incapacity or deleted unrelated stun state.

Cold and hypothermia immunity suit this lineage, with resistance to hostile freezing/root effects. Avoid adding an extreme fire vulnerability which would undermine sovereign-tier duels. Hypothermia applies only to appropriate biological targets, with a capped direct hediff change; it does not alter map temperature.

## Sovereign baseline

Use the established healthy Incarnate baseline as the starting point:

- Moving 650%, Consciousness 200%, Manipulation 300%, Sight/Hearing 250%.
- +75 mood, ×3.2 melee stat, ×0.04 incoming damage, sovereign recovery and common-disease protection.
- Existing Melee Animation knockout protection, ×15 carrying capacity and ×4 individual walking-caravan-speed contribution.
- Skills derive damage from the existing melee profile and apply shared tuning once. Do not multiply the lineage stat into the profile again.

Cold defenses, Pressure, parry and freeze control provide its identity. Validate practical 1v1 and group performance against all existing sovereigns; matching displayed stats alone does not establish equal combat strength.

## Apparel, sword, halo and aura

- **Winter-prince regalia:** a fitted midnight-blue/black suit, silver fastenings, pale cyan crystal channels and an ivory split cloak with a restrained aurora-colored lining. Compact shoulders, low neckline, no helmet, no drawn neck or limbs. Keep the silhouette distinct from Thunder's black/yellow and Tempest's gray/white lightning.
- Cover all actual body parts below the neck, including groin, hands, fingers, legs, feet and toes. Use native south/east/north textures and west mirroring across native and supported custom bodies. Flat-chested Male/Thin variants; slim side profiles with correct head anchors and no exposed back skin.
- **Winterglass katana:** black grip, silver fittings, a white blade with pale-cyan fractures and a faint aurora channel, correctly oriented cutting edge and a native Melee Animation grip/blade profile. Bound and indestructible like the other Incarnate weapons.
- **Halo:** a thin tilted ice crown/ring with an emerald/cyan/violet inner ribbon with visible depth, six subtle looping frames and small floating crystal tips.
- **Back aura:** a quiet aurora fan behind hovering ice-blade silhouettes, with sparse snow and crystalline glints. Halo/aura require transformed, drafted, standing and conscious state.
- Downing/death uses the existing reversion and personal-clothing restoration path. Recovery/revival retains bound-sword ownership without duplicates.

## Utility and Bestow

**Rime Step / Aurora Travel** shares the established utility button. Right-click cycles **Step Auto → Step Manual → Travel**. Step uses a snowy frost dissolve and reappearing crystal streak wrapped in aurora light, with the same manual map-cell freedom and safe-cell checks as other Incarnates. It leaves only short-lived harmless visual wisps.

Aurora Travel uses the new native group transporter for passengers, animals and cargo, including settlement arrival choices and native ISEKAI Tower floor entry. No silver, fuel or world-travel cooldown. Do not revive the old solo teleport flow.

Bestow grants ordinary Water through the shared Bestow icon and paired ceremony. Keep the seven combat forms, utility, Bestow and Transform/Revert readable through the existing skill-menu layout and shared Auto/Off/Non-lethal controls.

## Prepared assets and animation contract

Prepared: fitted apparel and inventory art, Winterglass, seven combat icons, Transcend, Step/Travel and Pressure icons, six-frame halo/back-aura/freeze-shell/snow loops, and the shared Bestow icon. A registered native Melee Animation profile remains staged until the weapon definition exists.

The pack provides aligned eight-frame sequences for crescent, dash wake/flash/slash, spin, aurora formation/loop/release, radial burst, mirage, retreat, counter slash, warning, crown formation, frost-step, group travel and coronation. The grand crown collapse/shatter has sixteen frames, the outward frost streak has four, and small shared loops have six. C# should interpolate motion, facing, knockback and timing independently of frame count. A distinct frost coronation should use its own ice imagery.

Audio should combine clean blade cuts, ice cracking, restrained cold wind and a heavy crystalline shatter for the finisher. Ordinary movement and the looping halo should stay quiet.

## Implementation constraints and verification

Reuse bounded effect/afterimage pools, cached materials, camera/fog culling and bounded local searches. Save cast phase, committed target IDs, cooldowns, Pressure ownership and freeze/thaw timing. Every specified impact needs a per-cast victim set; animation frames do not create additional hits.

No spawned ice walls/buildings, permanent terrain changes, roof removal, ambient temperature simulation or uncontrolled damage/projectile spawns. Non-lethal retains controlled push/suppression without wounds, lethal shatter damage or hypothermia. Interruption must release only owned control and restore normal jobs/facing safely.

Before release: verify all body/direction fits; real native and Melee Animation duels; crowd response and sovereign thaw resistance; wall/occupied-cell routes; parry recursion; simultaneous allied casters; interruption; save/load during dash, telegraph, freeze, counter, coronation and group travel; original hediff/gear identity; God Mode button disappearance; and no permanent skill lock.

## Flashy motion contract

Aurora Blitz keeps Cyclone-style slicing momentum: short anticipation, rapid visible movement between targets, a brief hit-stop at each committed cut, then a clean final follow-through. C# moves the actual pawn along validated segments; pictures provide the flash, not a separate teleport or repeated damage. Revalidate each next target and its empty landing cell. A blocked or unreachable link finishes the current safe segment and ends the chain. At most five targets, bounded afterimages and one hit per victim.

Northern-light ribbons follow the completed route segments instead of filling the entire screen. Draw the curtains behind pawns at restrained opacity, keep the white/cyan blade and snow readable in front, and reserve the largest aurora display for Dominion and Throne. Frost wisps are temporary visuals with no climate or terrain simulation.
