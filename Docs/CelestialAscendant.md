# Celestial Ascendant

Celestial is a standalone fully evolved lineage of sun, solar energy and light. It has no disciple or normal Celestial form, no mastery grind and no Evolve command. Existing elemental disciples and evolutions remain available.

Acquire a **Celestial Heart** by crafting it at a drug lab after Fabrication, with Crafting 10, 25 gold, two advanced components and four industrial medicine. Inject it or administer it through a medical bill to an unascended humanlike pawn; a disciple skips its milestones. The one-lineage rule prevents a second heart. Developer tools also include **Elemental Hearts → Grant Celestial ascension** for an eligible pawn.

Transform manifests the helmetless ivory/gold regalia and **Dawnbringer**, using the existing smooth apparel reveal and additional solar seal, light column and drifting motes. Revert restores the original clothes and weapon. The same indestructible projected gear is stored and reused, including after saving/loading; downing restores ordinary clothes. Hair and beard use the shared manifestation rules. Seven fitted body textures cover south/east/north with native mirrored west.

## Strength

Celestial supplies a 6.5× Move Speed stat factor, +75 permanent mood, 15× carrying capacity and a 4× individual caravan speed contribution. Caravan travel uses the existing average of walking humanlike members. Its 3.2× melee damage, 0.55× melee cooldown factor, improved capacities, 0.04 incoming-damage factor, stronger armor and four points of ordinary injury healing per second put it above the existing evolutions. Actual movement includes normal capacity, equipment and other mod modifiers.

Lower elemental Ascendants and evolutions deal a further 80% less damage to Celestial; the ordinary evolved-versus-normal ward also remains applicable. Heavenly Zenith grants a brief additional defensive corona. Shared illness protection and Melee Animation knockout protection apply. This adds no blanket immortality or missing-limb regrowth.

## Techniques

All six combat commands are available while drafted. Transform, wield a melee weapon and remain conscious to use them. There is no resource cost. Auto Combat starts on for a newly granted Celestial, supports the shared master setting and preserves saved On/Off/Non-lethal choices. Non-lethal replaces active and normal-melee wounds with the existing suppression behavior.

| Technique | Behavior | Cooldown |
|---|---|---|
| Dawncleaver | A twelve-cell solar crescent. One strike per distinct victim along its moving lane, adding Radiance. | 3.5 seconds |
| Sunstride | A dash along up to ten clear cells, light trails, afterimages and one finishing strike. | 4.5 seconds |
| Corona Dance | A 4.5-cell spinning sword form. The frames share one committed impact per victim. | 8 seconds |
| Solar Judgment | A short telegraph followed by a descending solar pillar up to fourteen cells away. Spends the primary victim's caster-owned Radiance, with a bounded nearby burst. | 18 seconds |
| Heavenly Zenith | Two nearby visible hostiles trigger a gathering sun, radiant strike, safe outward knockback and a three-second defensive corona. May also be pressed manually under the same crowd condition. | 25 seconds |
| Fake Rainbow | Intercept an eligible incoming hit, leave a rainbow mirage, retreat safely, then immediately counter-dash with light trails and one rainbow slash. Automatic with automation on; pressing it manually prepares a three-second parry window. | 8 seconds after a parry |

Radiance is capped at three marks per caster/victim pair and eight owners per victim, expires after eight seconds and survives loading. Solar Judgment consumes only the active caster's marks. Automatic form selection prioritizes surrounded defense and marked finishers, then combines the available sword forms.

Fake Rainbow requires a nearby hostile attacker and a clear retreat/return route. It intercepts whole native melee attacks and eligible direct damage. It has no indefinite invulnerability window. Counter-depth guards stop recursive parry/counter chains. Movement checks walls, diagonal corners and occupied cells, including late occupancy. New orders, incapacitation, lost targets or invalid routes end the owned job and release its skill gate. Cooldowns remain spent after an accepted cast is interrupted.

## Random Bestow

**Bestow (Random)** is manual. Both participants stop and face each other for four seconds. The recipient receives normal Lightning, Flame, Wind, Water or Earth ascension, each with equal chance. Celestial also fulfills the recipient's 150 mastery kills and ten Ascendant days. The recipient remains a normal Ascendant until you choose the ordinary Evolve command. Readiness uses the existing saved mastery and age values, so it survives saving/loading without another hediff or save key. Interrupted ceremonies grant neither ascension nor readiness. Other evolved Ascendants' Bestow keeps the normal evolution milestones. Disciples of any element can receive Celestial's gift; existing Ascendants cannot. Celestial is never a Bestow outcome.

Celestial Regalia's native apparel coverage includes torso, waist, shoulders, arms, hands, legs and feet. It satisfies RimWorld's upper- and lower-body clothing checks, including the groin/legs, and provides its existing armor ratings on those parts. Head coverage is excluded. This is a mechanical coverage correction; the garment textures and fitted head position are unchanged. Existing manifested regalia inherits the corrected definition when the game reloads.

The result is selected once during the held ceremony and saved. Loading does not reroll it. Interrupted ceremonies grant nothing. Completion removes old discipleship before granting exactly one normal heart and uses the shared bound-sword transition.

## Light Travel

**Light Travel** is manual and free: no fuel, psyfocus, distance payment or cooldown. Choose another usable land tile on the same world layer, then choose an unoccupied arrival cell on its existing or generated map. There is no distance-based flight timer. Departure takes 1.2 seconds; the large arrival beam lasts 0.9 seconds. Equipment, inventory, the original pawn and transformation state are retained. Put down carried pawns/items first.

Canceling destination selection does nothing. After departure the original pawn is deeply held by a saved world travel object until safe placement succeeds. Canceling or failing a landing leaves the pawn in transit, with **Land** and **Return to origin** controls. In-transit free colonists count toward colony survival. Empty destinations use temporary landing maps, which follow the normal removal checks when no blocking pawns or active travel references remain.

The white/gold departure and arrival columns use the generated Milira-inspired beam art. They do not damage buildings, break roofs, ignite terrain or simulate ambient heat.

## Presentation and save behavior

The subtle solar halo and six-frame golden light-sword fan appear only while transformed, drafted, awake and standing. The sword fan draws behind the pawn; the halo sits above the visible head. The existing aura/afterimage switches and effect-opacity setting apply.

The visual controller uses cached frame/body materials, fixed per-map budgets of 128 effects, 32 afterimages and 128 adornment registrations, finite lifetimes, camera/fog checks and bounded local target searches. Floor wisps and sigils are temporary visual meshes. Rendering never applies damage. No tornado/ice-wall pawns, terrain edits, global weather changes, temperature simulation or uncontrolled projectiles are added.

Gameplay saves store cooldowns, controls, queued parry geometry, job stage, damage snapshots, strike commitment, deduplicated victim IDs, Radiance ownership, random Bestow outcome and travel passenger ownership. Active jobs reconstruct their remaining effects after loading. Replacing the job clears its busy gate; no permanent skill-disable hediff is used.

Restart RimWorld after installing the rebuilt assembly. Earlier saves are supported; saves containing Celestial or an in-transit Celestial require this version or a later compatible version to resolve the new definitions/classes.

## Validation

All 42 automated verification scripts pass. A clean native RimWorld run with Melee Animation passes 66 gameplay checks, 307 asset/profile/placement checks and 139 elemental audio checks. It loads an existing pre-Celestial save, then saves and loads during crescent, dash, telegraph, committed impact, queued/mid-counter, paired Bestow, departure, transit, arrival and completed state. Checks cover interrupted orders, wall/late-occupied routes, non-lethal wounds, four-form automated combos, random outcome persistence, restored personal gear/hair, downing, fixed visual budgets, round-trip travel and released skill gates.

The isolated fixture acknowledges the headless loading-window display gate and cancels the native arrival selector after confirming it opens, then checks saved transit and calls the same safe placement method used by the selector. Gameplay/map generation remain production code. The native drawing path is exercised; normal-play artistic review and compatibility with additional unrelated mods remain separate from these automated checks. No personal saves or preferences are modified. Temporary QA content is removed from the mod loader after testing.
