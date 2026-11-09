# Thunder Incarnate — art assets

Historical art-preparation notes. **Thunder Incarnate is now implemented**; see [the current mechanics and fitting guide](ThunderIncarnate.md). The later Fat/Hulk fitting corrections include native side compression and a small head-origin lift.

Asset preparation for the third stage of **Lightning → Thunderstorm → Thunder Incarnate**. This pack supplies artwork and a prepared sword profile; it does not implement the new lineage, stats or abilities.

## Outfit and sword

The outfit is a modern regal prince's black suit and angular split cloak, with minimal electric-yellow lightning seams. It has a low collar, tucked shoulders, a flat unisex chest and opaque torso/hip coverage. There is no helmet. A broken black/yellow storm crown supplies the halo.

- Seven native bodies: Male, Female, Thin, Fat, Hulk, Child and Baby; plus an additional FemaleBB compatibility set.
- South/east/north for each body: **24 worn textures**, with native west mirroring. One inventory icon.
- All **32 body/facing coverage checks** pass against native body alpha with a one-pixel margin. Collar center differs from the native head anchor by less than three texture pixels. Fitting previews use actual native body/head sprites at their recorded mesh widths and base head positions.
- No special head lift, side offset or extra side scale is required by these exports. Do not apply Thunderstorm's old `RegalSideFit` adjustment to this new outfit. The final in-game apparel definition still needs a render check when integrated; these checks validate the artwork's placement and coverage.
- Heavenbreaker: one vertical black sword with a narrow yellow channel, exported grip/blade landmarks and a prepared native Melee Animation profile for `EH_ThunderIncarnateSword`. Validate the grip in actual animations after adding its weapon definition.

Apparel path: `Things/Pawn/Humanlike/Apparel/ThunderIncarnateRegalia/ThunderIncarnateRegalia`. Sword path: `Things/Item/Equipment/WeaponMelee/ThunderIncarnateSword`.

The future apparel definition should retain the established full-body coverage groups: **Torso, Shoulders, Arms, Hands, Waist, Legs and Feet**, and the existing bound/indestructible gear handling. PNG coverage does not itself grant mechanical protection.

## Commands and effects

Six combat icons: **Divine Severance, Thunderstep, Blackbolt Waltz, Skybreaker Judgment, Heaven-Rending Dragon and Heavenfall Verdict**. Additional icons cover **Transcend** and the **Storm Dominion** passive. Bestow reuses the shared existing icon.

Commands: `UI/Commands/ElementalHearts/ThunderIncarnate/`. All effect frames are under `Effects/ElementalHearts/ThunderIncarnate/Frames/` and use zero-based two-digit suffixes.

| Sequence prefix | Frames | Intended use |
| --- | --- | --- |
| `DivineSeverance_` | 8 | Black/yellow forward crescent, right-facing by default. |
| `Thunderstep_` | 8 | Straight left-to-right attack wake; rotate onto a checked route segment. |
| `BlackboltWaltz_` | 8 | Rotating crescent with a transparent center. |
| `SkybreakerTelegraph_` | 8 | A restrained charging warning ring. |
| `SkybreakerStrike_` | 8 | Focused thunder contact and fade. |
| `HeavenRendingDragon_` | 8 | Angular dash-chain trace and fragmentation. |
| `HeavenfallColumn_` | 8 | Descending black/yellow lightning pillar. |
| `HeavenfallBurst_` | 8 | Expanding hollow shockwave and outward fragments. |
| `MovementTrail_` | 8 | Subtle horizontal travel-trail loop. |
| `Halo_` | 6 | Shallow tilted, segmented black/yellow storm crown loop. |
| `Resonance_` | 6 | Subtle charge-ring/glint loop. |

There are **84 effect/halo frames**. Every runtime PNG uses a transparent 256×256 canvas. Atlas extraction preserves one shared cell coordinate system per sequence and adds an equal safety inset; it does not individually crop/recenter growing and fading frames. `AnimationSpec.json` supplies suggested playback and pivot notes. `Preview.html` plays the exported sequences locally without external dependencies.

## Future integration

Keep the six combat commands readable in the existing skill UI, with Transcend in the progression controls and Storm Dominion as a passive. Preserve the existing saved combat/non-lethal flags, cooldowns and gear ownership. Reuse the shared smooth apparel manifestation and C# ceremony structure. No full-screen transformation overlays are supplied.

Use cached materials, bounded local effects/afterimages, camera/fog culling, checked dash routes and local target limits. Emit the movement trail only for actual movement while transformed, with a distance-based cap and short fade. Derive afterimages from the fitted pawn apparel/head rather than creating separate per-body ghost textures. Halo visibility follows transformed + drafted + standing. Black energy needs an alpha-capable material; an additive-only shader would discard its black center.

Frame playback is visual only. Drive damage, stun, knockback and invulnerability windows from saved C# action state, with one guarded impact per eligible target and bounded stun recovery. Do not deliver damage from animation callbacks or from every trail segment. All floor wisps/rings expire; no permanent terrain, buildings, weather changes or spawned actors are implied by this artwork.

## Provenance

New original art was created with the built-in image generator. `GenerationPrompts.json` records every prompt, source image and the selected final revisions. Earlier variants remain in `Generated/` for provenance; only selected exports appear in `Mod/`. Export preparation fits generated designs to native body geometry, splits atlas cells, preserves alpha and reduces to runtime size. Imperceptible alpha values 0–1 are quantized to zero. Bestow is reused unchanged from Elemental Hearts. No game/dependency assemblies or naked-body textures are included.


Held katana edge correction: the normal held UV mesh mirrors only its width, retaining the 90-degree aim correction. The native Melee Animation profile uses FlipY=true under its flip-aware local rotation rules; Rotation=90, Scale=1.38, OffX and grip/blade landmarks remain fitted. Inventory/ground artwork is unchanged. All eight supported body types now receive an additional east/west head-origin lift scaled to body/life stage; front/back fit is retained.
