# Azure Flame evolution — proposed design

Status: implemented in the Azure Flame update. This file preserves the design proposal; see AzureFlameUpdate.md for the final mechanics and validation.

## Progression

Disciple → Flame Ascendant: 50 credited kills earned as a disciple plus five in-game days of discipleship. This is implemented.

Flame Ascendant → Azure Flame Ascendant: proposed requirement of 150 additional hostile-pawn kills earned after Flame ascension, plus ten in-game days as a Flame Ascendant. Earlier disciple kills would not count twice. Animal hunting, allies, executions of friendly prisoners and repeated kills of the same resurrected target would not advance this evolution. These stricter evolution rules are proposals, distinct from the implemented vanilla-attributed disciple kill counter.

Once qualified, unlock an Evolve to Azure Flame command. A short, interruptible three-second meditation completes the evolution; interruption spends nothing. The player chooses when to evolve. Show both requirements in the progress tooltip. Preserve the pawn, sword, gear condition, relationships, existing ability cooldowns and automation preference.

## Identity and balance

Azure Flame is more focused and controlled than ordinary Flame: blue-white cores, cobalt fire, cyan edges and a restrained ember trail. Use the completed regal cobalt, ivory and gold sleeveless armor and matching Azure Flame sword. Keep the smooth manifestation sweep. No helmet, crown or other headgear is included. This takes inspiration from the requested elemental evolution theme without requiring a copy of another series' character design.

Make the evolution a new tactical kit, rather than a large multiplier stacked onto the existing powers. Its new moves replace the four Flame abilities; the old versions cannot be cast alongside them. Use the newly reduced damage values as the balance baseline. Do not increase attack speed, healing, maximum health and damage together.

## Proposed ability replacements

| New ability | Role | Initial balance target |
| --- | --- | --- |
| Azure Crescent | A narrow blue flame blade wave; rewards lining up enemies. | Same total damage budget as the reduced existing wave. Convert part of its damage to thermal and add modest armor penetration instead of another damage packet. |
| Blue Comet | Dash up to six clear tiles and cut one target, leaving a brief visual trail. | About 80% of the reduced current blink-strike damage; one-second slow. Walls still block it. No trail damage or extra automatic hits. |
| Controlled Pyre | A three-tile area of azure flames lasting three seconds. | Total damage across all pulses capped at the reduced current area-strike budget. Fewer simultaneous targets means no hidden per-target multiplication. Visual flames do not ignite terrain or buildings. |
| Azure Phoenix | A telegraphed finishing leap and tightly focused burst. | At most 110% of the reduced current finisher's total damage, with 40% longer cooldown. Keep movement/stun/target-escape interruption and clear warning effects. |

Retain a maximum of three owner-specific Azure Brand stacks. Brands improve control or penetration, not recursive explosions. A finisher consumes them once. Brand damage must not create another brand or trigger another explosion. Existing friendly-fire and non-lethal settings continue to apply to every pulse and secondary target.

## Art and presentation

Create four distinct blue-and-white ability icons and an Azure evolution icon. Recolor the elemental effects with new blue flame shapes, rather than simply tinting lightning effects. The sleeveless royal armor and matching sword assets are completed in AzureFlame-Assets. There is no helmet or other headgear. The future transformation reveals the royal body armor, then manifests the sword, using the existing smooth sequence. Limit visual particles when several ascendants act together.

## Delivery sequence

1. Add saved Flame mastery progress and the evolution command, without changing combat.
2. Add the azure appearance and manifestation effects.
3. Replace the four abilities and verify their damage budgets, cooldowns and interruption rules.
4. Test existing saves, evolution interruption, downing/death/revival, bound sword restoration, auto combat and non-lethal mode.
5. Balance against ordinary raiders, armored enemies and several ascendants fighting together. A flashy evolution should add decisions without restoring the previous excessive damage.

A later Lightning → Thunderstorm evolution can reuse the same progression structure, with its own red-lightning identity and separate balance pass. It is outside this implementation.
