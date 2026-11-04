# Ascendant damage explained

There is no single flat damage number for every Ascendant or skill. Health-condition descriptions separate **the lineage's Melee damage stat multiplier** from **actual technique injury packets**. This documentation pass changes no combat numbers.

## Calculation

The shared melee profile selects a usable native melee attack, preferring an equipped melee weapon. It includes adjusted damage, the pawn's Melee damage stat and applicable quality/modifiers, with an ability-specific fallback floor. Each form then applies its own coefficients and any implemented marks, Flow, charges or Resolve bonuses.

Eligible injury packets from disciples/Ascendants receive **×0.8** from `Source/ElementalBalance.cs`: the existing **20% reduction**, applied once per eligible packet. Armor, incoming-damage multipliers and elemental defenses determine the eventual injury; other mods may add modifiers.

Ordinary melee does not acquire a skill coefficient simply because the attacker is an Ascendant. A weapon-derived profile already includes the lineage's melee stat: **do not multiply the lineage into that profile again**.

| Lineage | Lineage Melee damage factor | Increase contributed to that stat | Identical eligible physical packet after ×0.8 |
| --- | --- | --- | --- |
| Lightning, Flame, Wind, Water, Earth | ×2.00 | +100% | ×1.60 |
| Azure Flame, Thunderstorm, Glacial, Cyclone | ×2.20 | +120% | ×1.76 |
| Lunar | ×2.30 | +130% | ×1.84 |
| Eclipse | ×2.65 | +165% | ×2.12 |
| Adamant | ×2.80 | +180% | ×2.24 |
| Celestial | ×3.20 | +220% | ×2.56 |
| Eclipse with real Eclipse Dominion | ×3.20 combined | +220% | ×2.56 |

The last column compares otherwise identical physical packets with their unascended values. It excludes armor, other pawn stats, technique coefficients, quality changes and extra elemental packets. It is **not final skill damage, DPS or guaranteed damage dealt**. A factor of two means 200% of the original stat, or **+100%**, not +200%.

### Examples

An otherwise 20-damage physical packet with a normal lineage: **20 × 2.0 × 0.8 = 32**, before other modifiers and mitigation. Lunar gives **36.8**, Adamant **44.8**, and Celestial **51.2** under those assumptions. Native rounding and target armor still matter.

If a skill takes an **already adjusted 40-damage profile** and requests a ×2 physical impact, the eligible packet is **40 × 2 × 0.8 = 64** before mitigation. Do not apply the lineage multiplier to the 40 again.

Some forms request physical and elemental injury as separate channels, each with its own type/penetration and target defenses. Do not apply a second global ×0.8 to the combined attack after tuning its eligible packets.

## Channels and statuses

| Channel | Meaning |
| --- | --- |
| Physical sword injury | Cut/Stab/Blunt or the selected native melee verb's physical type, depending on the form. Armor and penetration apply. |
| Flame / Azure flame | Thermal injury can accompany a separate physical sword hit. Thermal resistance does not erase every physical part of the attack. |
| Lightning / crimson lightning | Electrical injury can accompany physical damage. Damaging hits can trigger bounded shock stuns; some techniques separately handle mechanoid EMP. |
| Water pressure | Blunt injury driven by profile and technique/Pressure rules. Glacial reuses the lineage's system with its own coefficients and crowd control. |
| Solar cut | Celestial sharp injury, scaled by its profile, form and Radiance rules. Melee-weapon-tagged solar packets receive the common tuning. |
| Awakening shockwave | One bounded blunt impact per eligible nearby target; building damage is disabled. |
| Freeze, stun, push, marks, trails | Control, saved combo state or visuals. Looping shells, afterimages and particles are not repeated injury ticks. Glacial hypothermia is a distinct status effect. |

The balance helper explicitly includes the mod's electrical, thermal, water and awakening injury, melee-weapon-tagged packets, and ordinary Cut/Stab/Blunt injury. Ordinary ranged-weapon packets are generally excluded. Do not assume it scales every external mod's damage or every status/EMP packet.

Bounded impact rules prevent the same committed strike from repeating on every animation frame or after loading. A deliberately defined physical-plus-elemental attack may still contain multiple channels; its visual sequence does not add uncontrolled extra packets.

## Disciples and Eclipse Dominion

Disciples lack the full Ascendant stat package. Supported techniques apply **×0.4 disciple damage** and **50% longer base active cooldowns**. Common ×0.8 tuning still applies to eligible packets. The ×0.4 technique factor does not mean every ordinary melee hit also receives it.

During an actual native eclipse, Dominion raises Eclipse's combined core melee factor from **2.65 to 3.2**. Eclipse's ability strike path separately adds **×1.15**. The extra 15% applies to Eclipse abilities, not every ordinary melee hit. Dominion ends when no actual eclipse is active.

## Defense and Non-lethal

Incoming damage stats describe the **defender**, not outgoing power. Apparel, incoming factors, relevant elemental resistances, Resolve and wards influence survival. A 10% incoming factor does not promise exactly ten injury from every incoming 100-damage attack; other mitigation and injury handling matter.

**Non-lethal** replaces supported technique and ordinary melee wounds with suppression rather than merely reducing their size. Suppression has a **75% temporary-knockdown chance**, otherwise a brief stun. Existing bleeding, other weapons and unrelated damage retain their behavior. Glacial Non-lethal push/freeze omits injury and hypothermia.

## Implementation references

- Shared tuning: `Source/ElementalBalance.cs`, `DamageMultiplier = 0.8`.
- Melee profile: `LightningCombat.Profile` in `Source/LightningAbilities.cs`; Flame has its corresponding profile/strike code.
- Lineage factors: thirteen playable `HediffDef` entries under `Defs`.
- Disciple factor: `Source/ElementalTeaching.cs`.
- Suppression: `Source/ElementalNonLethal.cs`.
- Eclipse core boost: `Defs/EclipseAscendant.xml`; extra ability factor: `EclipseCombat.Strike` in `Source/EclipseTechnique.cs`.

Inspect the selected pawn's native stats, equipped weapon and individual move tooltip when assessing a fight. The mod does not implement a universal fixed skill-damage value.
