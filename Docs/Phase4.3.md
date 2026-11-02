# Phase 4.3 — sound, resilience and recovery

Includes earlier releases and keeps the single Auto combat toggle. Requires Core and Harmony; no DLC or audio downloads. Close RimWorld and replace the previous ElementalHearts folder, keeping one installed copy. The installed mod and normal saves were not overwritten by this delivery.

## Ability audio and cooldowns

| Ability | Sound palette (Core audio) | New cooldown |
| --- | --- | --- |
| Thunder Sever | Sharp metal strike + electrical shield crack | 5 seconds |
| Lightning Flash | Electrical charge/dash pulse | 6 seconds |
| Thunderstorm Dance | EMP burst | 9 seconds |
| Heavenly Thunder | Charge at telegraph; thunder + stun burst at impact | 22.5 seconds |

These reuse RimWorld's native positional sounds, not newly recorded samples. Manual and automatic casts share the audio. Rejected casts do not play ability sounds; a cancelled Heaven telegraph does not play the impact. Audio follows the game's sound settings and is only emitted on the viewed map. Ordinary passive melee does not repeatedly trigger these ability sounds.

Cooldowns are exactly half their former values. Older saved remaining cooldowns are halved once when loaded; subsequent saves/reloads retain the new values without repeatedly halving them. Existing automatic-combat preferences remain intact.

## Stronger heart

| Bonus | New value |
| --- | --- |
| Moving, Manipulation, Sight, Hearing | +50 percentage points each |
| Breathing, Blood pumping, Blood filtration | +50 percentage points each |
| Consciousness | +20 percentage points |
| Metabolism, Talking, Eating | +25 percentage points each |
| Mood | +20 while the heart is installed |
| Mental break threshold | −25 percentage points (ordinary 35% becomes 10%, before other modifiers) |
| Melee damage | ×2 (previously ×1.5) |
| Global work speed | ×1.75 (previously ×1.5) |
| Incoming damage | ×0.5 (previously ×0.7) |
| Immunity gain speed | ×2 (previously ×1.5) |
| Natural healing | ×6 (previously ×3) |
| Bleeding amount | ×0.25 (previously ×0.5) |
| Pain | Suppressed |

Capacity offsets combine with native dependencies and other health effects, so final health-screen percentages can exceed these simple additions. Some jobs/stats cap the benefit they take from a capacity. Existing ×1.6 movement speed and melee attack-speed bonus remain, in addition to the capacity boosts. Hunger/rest/recreation reductions remain unchanged.

Mood stability greatly reduces ordinary mood-driven breaks; it does not erase thoughts, end an existing mental state, or guarantee immunity to forced/scripted mental states.

## Wounds and movement

The heart compensates for body-part efficiency lost to ordinary, non-permanent injuries and blocks hit-induced stagger. Pain is zero. This prevents ordinary wounds from slowing the pawn through damaged legs or pain. Scars, missing limbs, disease effects, terrain, encumbrance, anesthesia and actual stuns still apply; the heart does not make the pawn immortal.

After a wound is at least 180 game ticks old (3 seconds at normal speed), the heart checks it on a one-second recovery interval and automatically tends bleeding injuries/amputation wounds at 75% quality. Thus sealing occurs after approximately 3–4 seconds. It then heals the most severe eligible ordinary injury at 2 HP per second, alongside native natural healing. Sealing is saved as normal tending; removal of the heart does not reopen a sealed wound.

Recovery works without drafting and does not require sleep, medicine or a doctor. It stops on death or implant removal. Scars, diseases, lost limbs and existing blood loss are not directly erased; blood-loss recovery follows native rules. Severe wounds can still kill before recovery acts.

## Verification

See Phase43RuntimeChecks.txt for executed in-game assertions and Phase43StaticChecks.txt for XML/build validation. Audio checks verify real cast hooks and resolved native sound samples, not a human listening test. Full mod-list compatibility and large-raid performance remain unverified.
