# Azure Reversal and elemental gear update

## Front alignment

Azure regalia now has a native **south/front-only** horizontal apparel offset of +0.0645 world units, approximately eleven texture pixels on the adult body mesh. This moves the neckline underneath the head. Body-size scaling remains enabled. Side, back, head position and every armor texture are unchanged.

## Fifth Azure ability: Azure Reversal

Select a drafted Azure Flame Ascendant. The new **Azure Reversal** button becomes usable with **at least two hostile pawns within three cells** and line of sight. It leaps three to five cells into a clear space, then rushes back up to twelve cells through the enemy line. The 24-tick leap follows a visible arc; the rush takes eighteen ticks. The current four Azure skills remain intact.

- No resource cost; **15-second separate cooldown**. No clear route means no cast or cooldown spent.
- Auto Combat uses the new move when surrounded; Auto Combat Off leaves manual use available.
- Each hostile in the narrow rush lane is struck at most once. Uses weapon-derived physical/thermal damage, existing damage reduction, Azure Brands and non-lethal mode. Allies, prisoners and downed pawns are excluded.
- Both phases recheck the route and destination. No wall/corner bypass, occupied landing, or rush through unwalkable terrain. Movement orders, undrafting, stun, incapacitation or an external controlled animation cancel the move; an already-started cooldown stays spent.
- Animation/job state and the one-hit guard are saved. The shared busy gate follows the current job, so ending/replacing that job releases other skills immediately.
- The fifth slot has its own saved component/cooldown. Existing four-slot arrays are unchanged. Use this independent-component pattern for future evolved forms' extra abilities.

## Indestructible equipment

All **14 current elemental armor, helmet, legacy visor and sword definitions** for Lightning, Flame, Wind and Azure Flame are marked indestructible. They take no item damage or durability wear, in any holder or on the ground. Existing marked items repair their underlying durability on load; new items initialize at full durability. Pawn damage/armor calculations retain the existing balance. Deliberate cleanup of obsolete projections still works.

Future gear should reuse `ElementalHearts.IndestructibleElementalGear` and `CompProperties_IndestructibleGear`, plus `useHitPoints=false`, so it receives the same protection.

## Revert when downed

Downing or death immediately cancels transformation and restores the pawn's normal clothes before vanilla drops equipment/inventory. The summoned kit is stored in the heart, retaining the same objects and quality. Repeated calls are safe. Already-downed transformed pawns from older saves are corrected during initialization; an interval fallback also covers later changes.

After recovery, **use Transform again** to manifest ascendant armor and sword. The kit is still bound and available. Disciples' separately called swords keep their existing automatic return-on-recovery behavior. Normal personal weapons still follow vanilla downing/death drop rules.

## Assets and verification

One generated command icon plus six transparent 256x256 effect textures: launch, leap, landing, rush, slash and impact. Effects are timed, scaled and faded with the actual pawn movement. Rendering uses cached materials, a fixed 48-effect pool, visibility/fog checks and the existing effect-opacity setting. Created using the built-in `image_gen` tool; source outputs and exact prompts are included in the art-source archive. The generated sheet's second row used adjusted crop boundaries to keep the complete sprites separated.

Build passed against the installed RimWorld/Harmony assemblies. All 14 existing verification scripts passed. The Regal and Reversal regression fixtures passed. New tests cover item-only protection and wear boundaries, shared production reversion/storage and personal-weapon restoration, blocked and occupied movement plans, range limits, corridor geometry, native toil durations/facing, visual interpolation, busy-gate release, independent fifth-slot rebinding, directional DrawData and native Scribe save output. All 112 existing apparel PNGs remain byte-identical.

Live combat, actual front alignment on rendered pawns, real apparel restoration and full save/reload are **not yet tested in RimWorld**. Native loading is not covered by the headless fixtures.

Restart RimWorld fully. In a test colony, transform an Azure pawn facing south, draft it near two hostile pawns, try the fifth button/Auto Combat, then down it. Confirm normal clothes return and the summoned kit stays absent until the next transformation. Repeat near walls and occupied landing cells; try non-lethal mode and save/reload during the move. Inspect ordinary ascendant gear for wear after combat.
