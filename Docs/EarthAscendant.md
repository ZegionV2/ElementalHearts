# Earth Ascendant

Earth is a playable normal lineage for RimWorld 1.6. Adamant evolution is [playable](AdamantIntegration.md).

## Equipment and progression

Transform manifests basalt stoneplate, a closed bronze-crowned basalt helmet, and the bound Faultbreaker sword. The helmet conceals hair/beard through the shared smooth manifestation system; Revert and downed reversion restore personal clothes and hair. All three projections are indestructible. Seven body textures use south/east/north exports and native west mirroring; the helmet uses native head scaling and three directional textures plus inventory icon.

Existing two-piece Earth kits receive exactly one helmet in place. The original sword, quality, saved personal equipment and transformation save keys are retained. Dormant helmets remain in hidden kit storage.

Craft Earth serum at a drug lab after Fabrication: 30 plasteel, 20 uranium, four industrial components and four industrial medicine; Crafting 8, 12,000 work. Normal Earth Ascendants Teach Earth. Disciples call Faultbreaker and use the four skills at 40% damage with 50% longer cooldowns, then qualify for Ascend after 50 credited kills/five days or awaken through the shared near-death roll. Promotion retains controls and cooldowns. Earth remains a fifth NPC disciple and Celestial Random Bestow choice.

Normal stats, recovery, +30 mood, disease protection, carry/caravan benefits and Resolve defense remain unchanged. There is no evolved Earth head halo.

## Four techniques

| Technique | Range | Cooldown | Effect |
| --- | --- | --- | --- |
| Faultline Cleave | Eight-cell lane | 6 seconds | Six-tick windup; one 2.6x weapon-derived blunt hit per eligible victim, short stagger, one Resolve on a successful hostile cast. Walls stop the lane. |
| Stonebreaker Advance | Six cells | 9 seconds | Bounded checked route to an empty melee flank, interpolated two-tick steps, one 3x arrival strike. |
| Seismic Quake | Four cells around caster | 18 seconds | Raise Faultbreaker and stab the ground over 36 ticks. One circular 2.2x impact per eligible enemy, 60-tick stagger and up to two clear cells of outward push. Automatically used against two or more nearby enemies. Full Ascendants must be transformed. |
| Spire Eruption | Eight-cell ground mark, three-cell burst | 30 seconds | Stab the ground over 42 ticks. A fixed marker warns where a spiked basalt hill will erupt. One 4x hit per eligible enemy, up to three clear cells of outward push and a 45-tick stagger. Consumes Resolve for +10% damage per stack. The hill dissolves as a short visual effect. |

All techniques use shared Auto Combat and Non-lethal controls, weapon scaling, disciple scaling and friendly-fire policy. Every area cast caps at 16 distinct victims and 512 inspected local things. Parry consumes the attempted hit without a push. Downed/dead or externally animation-controlled pawns are not forcibly displaced. Push routes stop at walls, closed doors, fire, occupied cells and diagonal blockers. There is no collision damage.

Earth's four numeric skill slots and cooldown list stay compatible: legacy Bastion slot 2 now casts Quake, legacy Mountainfall slot 3 now casts Spire. A saved old Bastion status may finish its existing brief duration; no new defensive stance is cast. Resolve still grants 5% remaining-damage reduction per stack, up to three, expiring eight seconds after the last gain.

## Automation, saved casts and effects

Automation respects player movement/work orders and queued commands. It uses Quake against two nearby enemies, Advance to close a gap, Spire for a cluster or full Resolve, then Cleave and ordinary melee. There are still four combat commands; no skill-menu replacement.

The native Earth job owns each windup and its ground-stab drawing. Movement, interruption, downing, lost weapon, reversion or external animation ownership cancels the action. Save/load retains the fixed eruption cell, cast deadlines and commitment flag. Commitment is saved before damage; a loaded impact cannot replay. Interrupted casts retain their spent cooldown and release their action gate.

Spire and Quake each have eight aligned generated effect frames and a distinct command icon. Materials remain cached and all transient effects share the existing 96-slot pool, camera/fog checks and bounded lifetimes. The existing 42 effect/aura frames remain for Cleave, Advance, normal auras and legacy visuals. Effect opacity and aura settings remain available.

Spikes and cracks are short-lived visuals. These skills spawn no terrain/building obstacles, damage no buildings, alter no permanent terrain or weather, and create no damage-over-time packets. Faultbreaker retains its native optional Melee Animation grip/blade profile. Ground-stab drawing yields to external animation ownership.

## Validation

See EarthSeismicUpdate.md for this update's build, regression and isolated native combat/save-load results. EarthValidation.md and EarthPolishValidation.md retain the earlier implementation history.

## Earth sound family

See [EarthAudio.md](EarthAudio.md) for the dedicated stone-hit, grinding gravel, Quake rumble and Spire fracture sound bank. The new sounds preserve the existing spatial and crowd limits.
