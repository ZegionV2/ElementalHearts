# Bestow, disciple progress and closed regal armor

## Bestow

Azure Flame and Thunderstorm Ascendants now have **Bestow Flame** or **Bestow Lightning** in place of Teach. Select an eligible unascended humanlike pawn. The giver walks adjacent, then both pawns stop and face each other for **four game seconds**. The recipient receives normal Flame or Lightning ascension when the ceremony finishes, skipping discipleship and its kill/day requirements. A disciple of the same element can receive the gift too. Existing Ascendants and disciples of another lineage are ineligible. Normal Ascendants continue teaching disciples through the original three-second lesson.

Both participants have dedicated held jobs with repeated facing updates. Other elemental abilities and transformations wait for the paired action. The jobs resist ordinary interruptions; incapacitation or a forced replacement ends the ceremony and releases the owned receiving job. Incomplete ceremonies grant no ascension. The shared ascension application removes discipleship before adding a whole-body normal Ascendant condition, preserving the one-lineage rule. Bestow has no resource cost.

The C# ceremony gathers gold light around the giver, sends a flowing elemental ribbon and sparks to the recipient, forms an overhead diamond and orbiting blue/red motes, then releases a finishing ring. Sounds mark three stages. Azure uses blue/gold; Thunderstorm uses red/gold. The effect uses cached procedural meshes/materials, fixed 32-ceremony and 32-burst buffers, camera/fog/opacity checks and saved ritual timing/audio/commitment fields. No replacement evolution or apparel-transformation texture sequence is added.

## Automatic Tempest Dragon

Tempest Dragon now participates in Auto Combat against a **single eligible enemy**, removing its former two-nearby-enemy trigger. Set Auto Combat to On or Non-lethal and keep automation enabled. It uses the same allowed combat jobs, queued-order protections, readiness and cooldown gates as the other automated skills. It still needs a valid clear zigzag route and keeps its 18-second cooldown, five-visit cap and existing damage. The skill tooltip now describes its fantasy, ranges, chained movement, impacts, automation and interruptions.

## Dev controls and disciple progress

**DEV: Evolution-Ready** appears for eligible unevolved Flame/Lightning/Water/Wind Ascendants only in God Mode. Turning God Mode off hides cached commands and blocks input. It fills milestones; the ordinary Evolve button performs evolution.

Disciples receive **DEV: Ascend-Ready** only in God Mode. It fulfills **50 credited kills and five disciple days**; it does not grant ascension. Press the normal Ascend button to complete the animated 3.5-second ceremony. Player-controlled disciples wait for this choice, while NPCs begin automatically. Cached developer commands stop drawing and accepting input when God Mode is off.

The existing disciple progress button displays generated sword/shield artwork at **0, 10, 20, 30, 40 and 50 kills**. Five runes light in sequence. The button retains the exact kill count and the tooltip retains time progress; the final icon means the kill goal is met, while the five-day requirement still applies to Ascend. Kill-credit rules and saved counters are unchanged.

## Regal coverage

Azure and Thunderstorm regalia have rebuilt front, east and back art with closed opaque white lining, continuous shoulder/upper-back panels and side seams down to the hem. The neckline stays compact; there are no drawn limbs, sleeves or helmet. The sprites are fitted higher and wider using the corresponding normal armor bounds, removing the old fit's lowered shoulder area. All **seven body types** have separate directional variants. West uses RimWorld's existing east mirroring.

The rebuilt Azure front is centered in its sprite, so the old compensating front offset is reset to zero. Inventory icons are updated too. No global body-hiding patch is added. The original smooth apparel reveal, indestructible gear, downed reversion, existing halos and evolution choreography remain.

## Assets and validation

Generated with the built-in image_gen tool: two edited three-direction regalia sheets, Bestow icon and a six-stage progress sheet. Integrated outputs are **42 body/direction sprites, two inventory armor icons, six progress icons and one Bestow icon** (51 RGBA PNGs at 256×256). ArtSource retains exact prompts, original generated sheets and the six input reference images. Bestow-and-Regalia-Preview.png is an asset contact sheet rather than a game screenshot.

The assembly compiled against the installed RimWorld 1.6 and Harmony assemblies. **All 24 automated verification scripts passed.** New checks cover lineage/recipient rules, shared normal-ascension dispatch, Bestow command replacement, pair ownership, skill-gate cleanup, native held/facing jobs, timing/audio bounds, off-created dev-command toggling, production iterators always yielding those commands, off-mode promotion blocking, progress stages, single-target Dragon dispatch, 10,000 fixed-buffer registrations and native Scribe save output. Asset checks cover all 42 body/direction fits, opaque torso samples, centered fronts, distinct progress stages and held JobDefs. Existing combat, evolution, transformation, gear, teaching, non-lethal and sword-profile checks also passed.

Headless verification does not cover live ascension application, native save loading, actual facing/armor coverage with pawn rendering or third-party body mods, sound mix or GPU animation. **Fully restart RimWorld.** In game, check dev mode off/on/off without reselecting; promote a disciple; try Dragon with Auto Combat against one enemy; inspect both regal outfits from every direction and across body types; use Bestow on an untrained pawn and a same-element disciple; verify both wait/facing, normal ascension, interruption cleanup and save/reload during the ceremony.

The previous installed files are backed up in Backup-before-Bestow-and-Regalia. Install-Manifest.json records changed files and before/after hashes. There are no file removals in this release.
