# Hell Flame / Thunder Incarnate combat correction

The two lineages had matching health-panel stats but different effective combat power. Hell Flame's automation excluded three offensive forms against a lone enemy, its ordinary dodge counter was disabled, and Phoenix/Furnace used weaker coefficients. Thunder also had electrical disruption and charged-finisher protection that Hell Flame did not match.

The native duel tests additionally exposed a shared order-handling defect: a drafted pawn could return to `Wait_Combat` while its suspended `AttackMelee` remained queued. The blanket queued-order check then prevented further automatic skills. NPCs did not use that player-only gate. This affected either Incarnate when player-controlled.

## Installed changes

- Hell Flame uses spin, Furnace and Pyre against a lone evolved/sovereign opponent. Against weaker single targets, two owned Hellbrands enable spin/Furnace and three enable Pyre. Reversal retains its two-enemy trigger.
- Phoenix uses 6.0 physical / 4.4 thermal coefficients; Furnace uses 3.8 / 3.2. These match the corresponding Thunder finishers before individual bonuses and defenses. Base melee stats and recovery remain unchanged.
- Successful ordinary melee dodges have the existing 50% counter chance and two-second cooldown. The deferred bounded counter uses hellfire visuals and direct damage, preserving Non-lethal and avoiding recursive melee callbacks.
- **Infernal Resolve** reduces electrical-shock chance and duration by 75%. A standard Thunder Incarnate shock becomes 11.25% for 22 ticks, followed by the usual three-second recovery window. Ordinary electrical injury continues through its existing damage/armor handling.
- Consuming three owned Hellbrands empowers that victim's finisher by 20% and grants a two-second ward reducing incoming damage by another 20%. The ward refreshes one saved expiry and does not stack. It strengthens the same committed impact, without extra damage packets.
- Both Incarnates distinguish a suspended combat return order from a distinct new player order. They retain the intended target and at most one suspended return order. Queued movement and other distinct commands still block automatic interruption.

## Native verification

Sixteen fresh, matched 1v1 duels ran in RimWorld with normal-quality steel bound katanas, Melee 20, matched initial weapon profiles (96/96) and incoming factors (0.04/0.04). Traits/genes and optional ISEKAI allocations were normalized; test-only XP suppression kept outside RPG progression from changing the comparison. Player/NPC roles and starting sides alternated. Melee Animation was present, with automated executions/grapples disabled to isolate ordinary combat and elemental techniques. No manual healing occurred; both lineages retained their natural four-injury-points-per-second recovery. Each fresh pair had up to 3,600 ticks, ending on death/downing.

**Final result: Hell Flame 6 wins, Thunder 10 wins, zero draws.** Both used their complete available offensive kits; Hell's Reversal correctly remained unavailable against a lone opponent. This small sample demonstrates competitive fights and successful Hell Flame victories, not a guaranteed 50% matchup or a claim that different RPG builds, traits, injuries and weapon qualities are equivalent. In the earlier diagnostic series with the combat-order fix but without the final Resolve/ward tuning, Thunder won all eight duels.

Sixteen additional focused native assertions passed:

- Counter availability, one queued retaliation, one physical/thermal pair, cooldown enforcement and no recursive counter packets.
- Non-lethal counters produce no wound packets.
- Reversal still rejects one enemy, including under automation.
- A real queued movement order blocks automation and remains intact.
- A suspended melee order behind drafted waiting allows the next skill without duplicating queued orders.
- Native Auto Combat casts all six offensive forms against one sovereign; the skill panel retains seven commands.
- A full three-brand finisher grants the actual ward.
- Electrical-shock sampling produced 27 procs in 256 forced fresh opportunities, each with the expected 22-tick state. This test removed each previous shock to measure the chance; normal gameplay retains the recovery gate.
- Saving/loading preserves the ward expiry, caster-owned Hellbrands, all cooldown timestamps and original bound katana identity.

The final native test log contained no combat-test exceptions. The installed assembly/source/definitions/documentation are hash-verified. Test saves, fixtures, logs and the pre-patch backup remain in the local workspace; the temporary QA mod is archived outside the playable Mods directory.

Existing saves are supported. Restart RimWorld to load the updated definitions and assembly. Existing hearts, equipment and cooldowns are retained. See [Hell Flame mechanics](HellFlameIncarnate.md).
