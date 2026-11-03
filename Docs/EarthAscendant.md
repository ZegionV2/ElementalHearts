# Earth Ascendant

Implemented normal Earth lineage for RimWorld 1.6. Adamant evolution is not included.

## Acquire and progress
Craft Earth serum at a drug lab after Fabrication: 30 plasteel, 20 uranium, four industrial components and four industrial medicine; Crafting 8, 12,000 work.
Self-inject or administer through a medical bill. Existing one-element-per-pawn rules apply.
Normal Earth Ascendants Teach Earth. Disciples call a bound Faultbreaker, deal 40% technique damage and have 50% longer cooldowns.
Disciples ascend through the existing 50-kill/five-day ceremony or the shared saved near-death awakening roll.
Promotion preserves Earth automation, non-lethal preference and remaining cooldowns.
Earth is a fifth equally eligible NPC disciple/normal Celestial Bestow choice. The overall configured NPC disciple probability is unchanged.
Developer actions include Grant Earth ascension. Earth has no Evolve command.

## Equipment and normal-tier benefits
Transform manifests basalt stoneplate and Faultbreaker, without a helmet or head halo.
All seven native body textures have south/east/north exports; west uses native mirroring.
Directional artwork is aligned to the existing Wind coverage silhouettes and side placement.
Bound ownership, indestructibility, hidden kit storage, personal clothing restoration and downed reversion use the existing systems.
Earth keeps visible personal hair, +30 ascension mood, normal shared disease/carry/caravan benefits, wound sealing and ordinary-injury recovery.
The Earth stage uses 1.4x movement, 2x melee damage, 0.85x melee cooldown and 0.45x incoming damage before equipment and temporary defense.
The sword has 26-power edge/point attacks with 2.6-second base recovery; stoneplate adds modest ordinary armor.

## Techniques
| Technique | Range | Cooldown | Timing and effect |
| --- | --- | --- | --- |
| Faultline Cleave | Eight-cell lane | Six seconds | Six-tick wind-up, one 2.6x weapon-derived blunt impact per eligible victim, brief stagger, eight-tick recovery. Stops at opaque walls/closed doors and corner blockers. One Resolve for a successful hostile hit per cast. |
| Stonebreaker Advance | Six cells | Nine seconds | Bounded clear navigation to an unoccupied melee flank. One checked movement step per two ticks; one 3x arrival strike, then eight-tick recovery. |
| Bastion Stance | Three-cell ally aura | Eighteen seconds | Four seconds of 20% personal and 10% same-faction ally protection, with 25% slower movement. Does not replace the current melee order. Disciples receive 40% defensive strength. Full Ascendants must be transformed. |
| Mountainfall | Eight-cell primary, three-cell burst | Thirty seconds | A target-following marker precedes a 42-tick wind-up. One 4x impact per eligible target, modified by Resolve, then eight-tick recovery. Safe outward displacement up to two cells; no collision damage or permanent terrain. |

Damage uses the existing weapon profile, shared disciple scaling and global elemental damage adjustment.
Area attacks cap at 16 distinct victims; burst searches cap at 512 inspected things.
Friendly fire and manually selected primary targets follow the existing shared policy.
Non-lethal uses suppression in place of wounds, including ordinary melee.
Bastion and three Resolve stacks multiply to 0.68 of remaining incoming damage, before the ordinary Earth stage/armor.
Several allied Bastions cannot stack, and a caster's personal Bastion is not multiplied by another caster's aura.
Incapacitation, undrafting, reversion, lost weapon, expiry or removal invalidates the stance and cleans up its movement status.
Bastion resists the mod's Earth pushes, Glacial displacement and Cyclone airbound capture. It is not general immunity to stuns or arbitrary third-party teleports.

## Resolve and automatic combos
While transformed, qualifying hostile damage grants at most one Resolve every two seconds.
The direct instigator must be a hostile pawn or building, excluding self/friendly/environmental damage and prevented hits.
Faultline can add one further stack per successful hostile cast.
Up to three stacks expire eight seconds after the last gain; each reduces remaining damage by 5%.
Mountainfall consumes current Resolve only at a valid committed impact, adding 10% impact damage per stack.

Automatic behavior respects explicit movement/work/hold orders and queued orders.
It uses Bastion under local crowd/ally pressure, Advance to close a useful gap, Mountainfall against a crowd or with full Resolve, then Cleave or ordinary melee.
Active decisions use the shared 12-tick cadence, with an early check after technique recovery.
Automatic casts can acquire a nearby replacement target at impact; manual casts retain the clicked opponent.
The saved job owns movement and impact timing; there is no separate permanent busy flag.
Saving retains deadlines, route/index, expected position and the impact commitment bit.
Interrupted casts retain their spent cooldown and release their action ownership.

## Presentation
42 six-phase frames supply the four techniques and Idle/Combat/Activation body auras.
Auras require a living, standing, drafted, transformed full Earth Ascendant; disciples have none.
Use the existing aura toggle and effect-opacity controls. No evolved head halo is drawn.
Mountainfall's marker follows its current job target and reconstructs automatically after loading.
Materials are cached; transient effects use a fixed 96-slot pool and bounded lifetimes.
Camera/fog culling and a 64-visible-body-aura budget apply. Dropped visual effects never drop gameplay.
Earth sound definitions reuse existing Wind samples through the shared throttled mixer; no new sound recordings are included.
An optional Melee Animation sword profile is supplied; the optional dependency was unavailable for local integration validation.

## Validation
See EarthValidation.md for build, automated checks and isolated native smoke-test results.

