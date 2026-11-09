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

## Configured Bestow

Press Bestow to open a menu. Choose Flame, Lightning, Water, Wind, Earth or Lunar, toggle Ready to evolve immediately, then choose a conscious humanlike recipient. Prisoners and hostile pawns are eligible and retain their faction/status. Existing Ascendants cannot receive another lineage. The four-second paired ceremony is stationary and facing; interruption grants nothing. With readiness off, the recipient earns normal evolution milestones. With it on, they still choose when to press Evolve.

The configured outcome/readiness are captured once in the saved job. Last-used menu preferences are separate. Older saved random ceremonies retain their original selected result/readiness. Other evolved givers still bestow their own matching normal lineage with ordinary evolution requirements.

## Light Travel

Left-click to select a world tile and land automatically in a safe cell. The map and landing are prepared while the pawn remains at the origin. Right-click for precise destination placement or relocation on the current map. Canceling a precise selection leaves the original pawn and equipment at the origin. Destination maps are held while planning, departing and in transit.

There is no fuel, distance cost or cooldown. Travel supports land tiles including impassable mountains and existing maps on the same world layer. Empty ocean tiles remain unavailable. New departure/arrival jobs last 42/24 ticks. A request made while paused briefly runs the animation, then restores pause. Gear ownership, transformation and the same pawn reference are preserved. If a selected cell becomes occupied, use a bounded nearby safe fallback. Automatic transit recovers on loading; legacy manual travelers retain Land safely, precise landing and Return to origin controls. Put down carried pawns/items first.

## Presentation and save behavior

The subtle solar halo and six-frame golden light-sword fan appear only while transformed, drafted, awake and standing. The sword fan draws behind the pawn; the halo sits above the visible head. The existing aura/afterimage switches and effect-opacity setting apply.

The visual controller uses cached frame/body materials, fixed per-map budgets of 128 effects, 32 afterimages and 128 adornment registrations, finite lifetimes, camera/fog checks and bounded local target searches. Floor wisps and sigils are temporary visual meshes. Rendering never applies damage. No tornado/ice-wall pawns, terrain edits, global weather changes, temperature simulation or uncontrolled projectiles are added.

Gameplay saves store cooldowns, controls, queued parry geometry, job stage, damage snapshots, strike commitment, deduplicated victim IDs, Radiance ownership, configured Bestow outcome and readiness and travel passenger ownership. Active jobs reconstruct their remaining effects after loading. Replacing the job clears its busy gate; no permanent skill-disable hediff is used.

Restart RimWorld after installing the rebuilt assembly. Earlier saves are supported; saves containing Celestial or an in-transit Celestial require this version or a later compatible version to resolve the new definitions/classes.

## Validation

All 42 automated verification scripts pass. A clean native RimWorld run with Melee Animation passes 66 gameplay checks, 307 asset/profile/placement checks and 139 elemental audio checks. It loads an existing pre-Celestial save, then saves and loads during crescent, dash, telegraph, committed impact, queued/mid-counter, paired Bestow, departure, transit, arrival and completed state. Checks cover interrupted orders, wall/late-occupied routes, non-lethal wounds, four-form automated combos, random outcome persistence, restored personal gear/hair, downing, fixed visual budgets, round-trip travel and released skill gates.

The isolated fixture acknowledges the headless loading-window display gate and cancels the native arrival selector after confirming it opens, then checks saved transit and calls the same safe placement method used by the selector. Gameplay/map generation remain production code. The native drawing path is exercised; normal-play artistic review and compatibility with additional unrelated mods remain separate from these automated checks. No personal saves or preferences are modified. Temporary QA content is removed from the mod loader after testing.


The current changes and native validation are documented in [Solar and Court update](SolarCourtUpdate.md). Older validation counts above describe the earlier release. Otherworld Calling is [planned separately](OtherworldCallingPlan.md).
