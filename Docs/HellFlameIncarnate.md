# Hell Flame Incarnate

> **World travel updated:** this lineage now flies along a visible world-map route and chooses its unroofed landing cell on arrival. Launch outdoors. No fuel or cooldown. See [Living world flight](LivingFlight.md) for current controls and saved-flight rules. Map Step remains an instant blink.

**Flame Adept → Azure Flame Ascendant → Hell Flame Incarnate.** The third Flame form is implemented at Celestial/Thunder Incarnate core power, with seven hellfire combat forms, a black tailored suit/crimson-lined cape, the ivory Ashreign katana, a hollow infernal crown and a subtle back aura.

## Progression and existing saves

Earn **300 new credited hostile non-animal kills and 15 days as Azure Flame**, then press **Transcend: Hell Flame Incarnate**, including from the Elemental Codex. These are separate third-stage counters: existing Azure Flames begin counting when this update loads, including world/caravan pawns. Earlier Flame mastery and the original heart's age do not retroactively qualify. One victim is credited once, with the saved victim set bounded at the 300-kill requirement.

The free eight-second coronation holds the pawn still and charges a growing infernal seal before the final eruption. Interruption retains readiness. Saving/loading resumes the ceremony without repeating the upgrade. **God Mode** exposes **DEV: Transcend-Ready**, which unlocks the ordinary ceremony rather than applying the upgrade. Turning God Mode off hides the command, including an already cached selection. Developer mode alone does not show it.

The existing heart is rebound in place. Combat/non-lethal preferences, relevant four Flame cooldowns and Azure Reversal's cooldown carry across. Personal clothing and bound equipment use the shared transformation/reversion system. Completed Hell Flame replaces the Azure stage, without stacking both hediffs. Downing/death removes manifested gear and restores personal clothes; recovery/revival retains ownership of the same indestructible kit. No new save is required.

## Combat forms

| Form | Behavior | Base cooldown |
| --- | --- | --- |
| Hellscar Crescent | Aimed eighteen-cell hellfire slash through a clear three-cell lane. Up to 24 hostiles once; applies Hellbrands. | 3 seconds |
| Ashen Pursuit | Checked sixteen-cell dash into a free adjacent landing, followed by a focused katana strike. | 4 seconds |
| Infernal Waltz | Eight-cell spinning crown of burning sword sweeps. Up to 20 hostiles once. | 7 seconds |
| Ruinous Phoenix | Approach within eighteen cells, rise into a visible warning, then plunge into a focused strike and five-cell burst. The main target must remain in contact. Consumes the caster's Hellbrands on that target. | 12 seconds |
| Hellfire Reversal | Requires at least two hostiles within three cells. Leap up to five cells clear, then rush back through the enemy line. Each victim once. | 15 seconds |
| Furnace Dominion | Stab Ashreign into the ground, warn briefly, then release an eight-cell shockwave. Damages, staggers for 1.5 seconds and pushes standing enemies up to three safe cells. | 22 seconds |
| Sovereign's Pyre | Mark a fixed location within 28 cells. A 0.75-second warning precedes the eruption; after 0.3 seconds of discharge, a ten-cell blast strikes up to 32 unique hostiles once. Consumes each victim's caster-owned Hellbrands. The remaining column is visual only. | 40 seconds |

All forms have **no resource cost**. Cooldowns are spent on a valid start, including interruption. Shared **Auto Combat On / Off / Non-lethal** controls work in pawn commands and the Codex. Automatic combos prioritize Reversal when surrounded and cycle available forms. A lone evolved or sovereign rival also enables the spin, furnace and Pyre; against weaker single foes, two Hellbrands enable spin/furnace and three enable Pyre. Crowd activation remains supported. Reversal still requires two nearby enemies. Both Incarnates recognize the suspended attack left behind by their own automatic skill, so returning to drafted waiting does not lock further combos. They retain one return order, keep its intended target and still respect queued movement or other distinct player orders. Auto Combat's existing first-three technique switches remain respected. Bestow grants **ordinary Flame**, with normal first-evolution requirements.

Opening techniques and ordinary elemental melee build up to three **Hellbrands per caster**, expiring after ten seconds. Phoenix/Pyre consume only their own caster's brands, adding a capped thermal coefficient to the same impact. Marks from multiple casters coexist, bounded at eight caster entries per victim. Consuming the full three brands also empowers that victim's entire committed strike by 20% and grants the caster two seconds of 20% additional incoming-damage reduction, matching Thunder's charged-finisher scale. This ward has one saved expiry and refreshes rather than stacking. The visuals do not detonate for extra damage. A successful ordinary melee dodge has a 50% chance to queue one retaliatory katana/hellfire strike, with a two-second cooldown, matching Thunder Incarnate's inherited counter availability. The counter runs on the next tick through the bounded shared queue; it deals direct damage without creating another melee verb or recursive counter chain. Target filters exclude allies and prisoners, and line checks protect enemies behind walls. A technique commits its impact before dealing damage and saves its struck-target set, preventing repeated impacts after loading. Physical and thermal packets form one strike per eligible target. Non-lethal uses the existing suppression path instead of new wounds.

Dashes recheck routes and free landings during movement, rejecting walls, closed doors, diagonal corner clipping and newly occupied destinations. Incapacitation, undrafting, replacement orders and external animation ownership end the job without leaving a persistent busy flag. A fixed Pyre mark does not follow a fleeing victim. Short-lived floor effects are visual only: no spawned walls, permanent terrain, ambient heat simulation, scenery blasts or map ignition.

## Power and resistances

Core **×3.2 Melee damage, 0.04 incoming damage, ×0.55 melee cooldown, four ordinary injury recovery per second, ×10 natural healing, 0.1 bleeding factor and +75 mood** match Celestial scale. Carrying capacity is **×15**, and the individual walking-caravan contribution is **×4**, averaged normally with other eligible walkers. Fire/thermal damage and heatstroke immunity, shared illness protection and evolved Melee Animation knockout resistance are integrated. **Infernal Resolve** reduces electrical-shock chance and duration by 75%: Thunder Incarnate's normal 45% / 90-tick shock becomes 11.25% / 22 ticks against Hell Flame, followed by the existing three-second recovery window.

Healthy native capacities show **Moving 650%, Consciousness 200%, Manipulation 300%, Sight/Hearing 250%**. Moving/Manipulation have lineage ceilings to account for boosted consciousness and limb efficiency. Other health conditions still use RimWorld's capacity calculation. The separate movement-speed factor is ×1.75. Eligible physical and thermal packets receive the existing shared **20% damage tuning once**: an otherwise identical physical packet starts at **×2.56** before armor, technique coefficients and other modifiers. Do not multiply the weapon profile by the lineage stat twice. Heat armor and elemental thermal resistance still affect the thermal part independently. Phoenix uses a 6.0 physical / 4.4 thermal coefficient before its capped brand bonus; Furnace uses 3.8 physical / 3.2 thermal, matching the corresponding Thunder finishers in base output. These are weapon-profile coefficients, not fixed injury amounts. Equal lineage tiers do not erase differences in traits, implants, weapon quality, elemental resistances or optional RPG stats.

## Hell Step / Hell Travel

One shared utility command keeps the skill menu compact. **Right-click cycles Step: Auto → Step: Manual → Hell Travel → Step: Auto.** Manual and Travel both disable automatic stepping. Mode, destination and recharge save with the pawn; the toggle is independent of Auto Combat.

- **Step, left-click:** toggle Auto/Manual. Auto follows the current movement/work route, retaining the job, queued orders and carried items/passengers. It checks at most every fifteen ticks and has a one-second recharge.
- **Step, Shift + left-click:** choose any safe empty walkable cell on the current map. No sight, fog or distance limit. Walls, closed doors, pawns, traps and fire block the landing. Arrival reveals only nearby cells.
- **Travel, left-click:** choose another settlement or world tile, fly along a moving world-map route, then choose an exact unroofed landing cell on arrival. Yellow marks a valid cell; red marks a blocked one. **Shift + left-click** flies with automatic safe landing on arrival. There is no fuel, silver, distance cost or cooldown. Put down carried things before world travel.

Travel uses the shared original-pawn holder, map preparation, reservations, cancellation and saved transit recovery, with separate infernal jobs and effects. Unconfirmed destination selection keeps the pawn at the origin and restores prior map/camera/time state. Canceling landing keeps the original pawn safely in flight. Exact intent and drafted state survive departure, travel and saving/loading. If the exact cell becomes blocked, the picker opens again. The original pawn can land, pick another cell or return if placement fails. Loaded maps skip redundant generation. Combat, transformation, teaching, Bestow and external animation ownership block travel until complete.

## Appearance and performance

Twenty-four worn textures cover seven native bodies plus FemaleBB, in south/east/north with native west mirroring. The flat-chested suit has a low collar and fitted cape. Its actual apparel groups cover **Torso, Shoulders, Arms, Hands, Waist, Legs and Feet**, including native descendant parts such as fingers/toes. No helmet is added. The crown and back aura appear only while transformed, drafted, standing and awake. Shared apparel manifestation/reversion uses the infernal palette.

Ashreign has a black/crimson grip and ivory curved blade with orange etching. Held size is 1.35; the optional native Melee Animation profile uses scale 1.38, measured grip/blade landmarks, a 90-degree correction and the katana edge flip. Normal held rendering has the equivalent UV correction. A weapon-pose layer supports the spin, ground stab and Phoenix plunge. Ordinary melee clashes add visual embers without extra impacts.

All 146 dedicated effect frames are integrated. Attack effects use eight aligned frames, while halo/back aura/Hellbrands use six. C# handles movement, rotation, cross-fades, timing, warning/impact phases and saved commitments. Materials are cached; each map caps effects at **192** and afterimages at **64**, with camera/fog culling, short lifetimes and bounded local searches. Visible crowns cap at 64 and coronation seals at sixteen per draw. Teleports reset movement tracking to avoid giant trails. Effects do not control gameplay damage.

See [artwork and source assets](HellFlameIncarnateAssets.md), the [animation contract](HellFlameIncarnateAnimationSpec.json) and [damage guide](DamageGuide.md).
