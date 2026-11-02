# Thunderstorm blitz and animated halo

Later update: Tempest Circuit has been replaced by the Tempest Dragon zigzag chain. See **TempestDragon.md**.

Later update: Scarlet Tempest is now an immediate spinning attack; only Tempest Circuit retains pulse-zone mechanics. See **ScarletTempestSpin.md**.

Redline Flash and Sovereign Thunder now use a short native movement job with continuous draw positions. The pawn rushes along a clear route and prefers an empty landing beyond the target. Routes and landing occupancy are rechecked during movement; walls and blocked diagonal corners prevent passage. Travel takes 6–14 game ticks depending on distance. The target must still be adjacent at impact. Sovereign retains its 18-tick warning after arrival.

Redline keeps its existing primary damage and cooldown. It can also graze up to three other hostile, standing, non-prisoner pawns along the route, each once, using 0.4 physical plus 0.4 electrical attack-profile multipliers. Secondary hits honor existing non-lethal, parry and ordinary damage rules. Sovereign retains its existing charge/burst damage and cooldown. Ordinary Lightning abilities are unchanged.

Stun, downing, sleep, undrafting a controlled colonist, loss of evolution, external paired-animation control or an invalid target cancels the job. New player orders can interrupt it. Other skills are gated by the current job, so replacing or ending it releases the gate. Saved movement progress, committed-impact state and secondary-victim IDs prevent deliberate replay of already committed hits. Interrupted attempts normally spend their cooldown; the existing Sovereign refund policy for a target dying/downing before commitment remains.

## Effects and halo

Six new transparent combat sprites provide needle streaks, broad speed wakes, cross impacts, launch rings, skid arcs and aftersparks. Fading red afterimages use the existing directional regalia sprite for the pawn's body type and a shared silhouette, rather than capturing portraits. West-facing armor ghosts are mirrored. Retreats gain a streak and afterimages; storm/circuit pulses gain linking red arcs.

A new six-frame crimson lightning halo blends between aligned frames. It uses Azure's above-head placement, body-size scaling and gentle bob. It appears only while transformed and drafted, awake and standing, and hides during transformation, downing, death and sleep. Existing aura and opacity settings apply. No extra skill button is added.

The original smooth apparel reveal and earlier evolution choreography remain. No replacement transformation/evolution texture sequences are introduced. Azure armor sprites remain unchanged.

## Performance and validation

The new effects use fixed ring buffers of 128 combat sprites and 64 afterimages, cached materials and reusable property blocks. Off-camera, fogged and zero-opacity drawing is skipped. Dash hit searches use a small local radius and stop after three secondary hits. Storm pulses calculate their attack profile once per pulse, rather than once per victim. These are bounded implementation costs, not an FPS benchmark.

The assembly compiled against the installed RimWorld 1.6 and Harmony assemblies. All 20 automated verification scripts passed, including native job/toil structure, route rules, duration/progress, interruption-gate release, halo frame/blend bounds, a 10,000-insertion fixed-buffer check, native Scribe save output and existing evolution, gear, teaching, sword and combat regressions. Native loading, GPU rendering, actual combat feel and third-party rendering interactions were not exercised headlessly.

Restart RimWorld fully after installation. In game: evolve a Lightning Ascendant, transform and draft it; inspect the halo in all directions; use Redline through a group and Sovereign on a standing target; repeat near walls and occupied landing cells; interrupt a rush, undraft and down/recover the pawn; then test saving/reloading during movement. Confirm skills become available after the rush ends. Try ordinary Lightning and Azure to check their existing behavior.

## Art provenance

Generated with the built-in image_gen tool. Two source sheets and the exact prompt set are retained in ArtSource. Twelve cropped 256×256 RGBA game sprites are included under Textures/Effects/ElementalHearts/Thunderstorm/Blitz and HaloFrames. The preview is an asset sheet, not a screenshot from RimWorld.
