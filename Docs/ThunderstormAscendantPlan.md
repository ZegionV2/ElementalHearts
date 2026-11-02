# Thunderstorm Ascendant — red lightning evolution plan

**Status: implemented. See ThunderstormUpdate.md for the current mechanics and validation.** The design proposal below is retained as historical context.

## Identity and progression

**Lightning Ascendant → Thunderstorm Ascendant.** Red/crimson lightning, white-hot cores and a restrained dark-red afterimage. Emphasize fast repositioning, deliberate chains and storm control.

Proposed unlock: **150 additional unique hostile non-animal kills after Lightning ascension, plus ten in-game ascendant days**, matching the Azure progression tier. Count each enemy once across resurrection. Earlier disciple kills, friendly prisoners, allies, self-kills and animal hunting do not advance evolution. Show both milestones in the tooltip. Add **Evolve: Thunderstorm** and the existing-style **DEV: Evolution-Ready** button for unevolved Lightning Ascendants. Readiness unlocks the button; the player chooses when to evolve.

Reuse the 4.5-second C# ritual: blue sparks gather, blue changes through violet into crimson, red lightning forms orbiting filaments, a brief lift resolves into a white/red flash and ground shockwave. Preserve the interruptible job, saved timeline, bounded rendering, effect-opacity setting and no-cost cancellation. No permanent weather change or camera disruption.

## Form, clothing and halo

Use the saved evolution-regalia master as the outfit base: compact low neckline, tucked shoulders, ivory/white short cloak, gold fastenings and trim. Replace Azure flame motifs with red lightning streaks and crimson gems. No drawn limbs, sleeves, long collar, helmet or crown. Ordinary Lightning armor stays unchanged.

The sword remains regal and straight, with a white/red energized blade and crimson lightning inlay. Armor, sword and any future variants use the existing indestructible-gear extension and component.

Replace the ordinary body aura with a **floating jagged red lightning halo** above the head. Show it only while transformed and drafted; hide it during transformation, sleep, downing and death. Keep the head and hairstyle visible. Downing restores personal clothing and stores the same bound kit; use Transform after recovery.

## Evolved power tier

Proposed baseline follows Azure's requested power tier: **5x elemental movement**, **+50 total ascension mood**, incoming-damage factor **0.1 before armor**, and an additional 75% reduction against ordinary Ascendants. Retain Lightning's capacities, healing and recovery identity; start melee power at roughly 10% above its ordinary ascendant modifier rather than multiplying every skill independently.

The extra ordinary-ascendant ward must treat **both Azure Flame and Thunderstorm as evolved peers**. Neither should receive that extra ward against the other. Generalize the current Azure-only defense check before enabling Thunderstorm. Keep existing armor calculations and the global 20% elemental/melee damage reduction.

## Five proposed abilities

The first four replace the four ordinary Lightning slots. The fifth uses its own saved component and button, following Azure Reversal's pattern. All remain free of resource costs and respect manual targeting, automation, friendly-fire protection, incapacitated-target exclusion and non-lethal mode.

| Slot | Ability | Proposed behavior | Starting cooldown |
|---|---|---|---|
| 1 | **Crimson Sever** | Red blade/lightning wave up to 12 cells, stopped by walls. Keep Thunder Sever's base damage budget; improve penetration slightly and apply one Storm Charge mark. | 5 seconds |
| 2 | **Redline Flash** | Clear-path dash up to 8 cells into melee, followed by one empowered strike. Slightly lower base hit budget than ordinary Lightning Flash; briefly slow a surviving target. | 7.5 seconds |
| 3 | **Scarlet Tempest** | Controlled red arcs within 5 cells. Up to three pulses share one total damage budget; marked enemies can become chain anchors, with each victim limited to one hit per pulse. | 12.5 seconds |
| 4 | **Sovereign Thunder** | A focused red/white thunderstrike with a visible interruptible warning and a small controlled burst. Consume at most three marks. Start near 110% of the ordinary finisher's total base budget, with the longer cooldown. | 31.5 seconds |
| 5 | **Tempest Circuit** | Place a red storm circuit within 10 cells. For three seconds, up to three hostile anchors inside a fixed 4-cell radius exchange controlled arcs. Three pulses share a single damage budget; the anchors and lines remain visible. Enemies can leave the zone. Auto Combat prefers it when two or more hostiles are pressuring the pawn. | 18 seconds |

**Storm Charge:** maximum three marks per target, short saved duration, no unlimited damage stacking. Mark consumption improves chain control or penetration instead of adding unbounded multipliers. Tempest Circuit must deduplicate victims even when several anchor links overlap, respect walls on each link and cancel remaining pulses if the caster dies, is downed, loses the evolution or leaves the map. No terrain fires, colony-wide EMP or unbounded chain propagation.

These are starting values for implementation/playtesting, not promises of final balance.

## Save and implementation design

- Add a standalone `EH_ThunderstormAscendant` based on the shared whole-body ascension definition. Upgrade the existing Lightning hediff in place; retain its load identity, age, severity, cooldown list, automation/non-lethal settings, passive timers and mastery state. Replace the old Lightning condition; do not stack two full sets of stats.
- Add a Lightning mastery component with unique-victim tracking. Do not infer historical kills from unrelated lifetime kill totals. Existing ascendant age can count; newly tracked mastery kills start when tracking is installed.
- Generalize elemental lineage, teaching, gear selection, mood and evolved-peer defense helpers. Thunderstorm teachers still create Lightning disciples; do not create evolved disciples directly.
- Keep ordinary Lightning's four-slot enum and saved cooldown array intact. Supply evolved names/tuning through the lineage's active form; add the fifth component independently. Hide the ordinary variants while evolved. Keep the skills menu's existing command layout.
- Rebuild projected gear without touching stored personal clothing. Preserve the bound sword owner/identity, quality, hidden holders and recovery behavior. Carry the appropriate front-only neckline alignment when reusing the same art layout.
- Save ritual timing/audio stages, movement jobs, marks, circuit anchors, due ticks and remaining pulses. Cosmetic particles may regenerate deterministically. Ensure interruption clears the busy/pose gate immediately.

## Asset list and rollout

Generate five command icons; front/side/back regalia for all seven current body types; inventory armor/sword art; and red slash, dash, chain, storm-circuit, finisher, halo and evolution effects. Match the current mod's clean outlines and compact RimWorld silhouettes. The new mod-loader thumbnail shows only currently playable forms; replace it when Thunderstorm ships.

Implementation order: lineage/mastery and migration → gear/halo and shared defense → four replacements → independent fifth skill → red ritual palette/effects → automated checks → live combat and full save/reload.

Acceptance checks: readiness boundaries and unique kills; repeated upgrade without duplicate comps; cooldown/quality preservation; no duplicate ordinary hediff; normal Lightning unaffected; five usable buttons; Azure-versus-Thunderstorm ward symmetry; bounded chain/circuit budgets; non-lethal and bystander protection; no wall bypass; downed reversion; halo state matrix; interruption during ritual/finisher; save/reload with pending circuit pulses; front-facing outfit alignment on adult/child body types.
