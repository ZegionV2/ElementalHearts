**Phase 4 update:** generated ability and aura sheets now supply 42 exported animation frames. The user authorized automated cropping, resizing and alpha cleanup. Original art remains unchanged. Clothing, hair and sword exports are included; see Phase4.md for fitting, playback, settings and limits. The inventory below records earlier phase decisions.
# Asset inventory and preparation plan

All six supplied PNGs were visually inspected. Originals remain untouched.

**Phase 2 update:** the transparent LH-Abilities.png is now copied unchanged to `Textures/Effects/ElementalHearts/Lightning/AbilityAtlas.png`. Runtime UV coordinates select its four regions for command icons and procedurally animated effects. No frame extraction or image editing was performed. The proposed individual-frame exports below remain the plan for higher-fidelity animation assets.

| File | Dimensions | Contents / readiness |
|---|---|---|
| LH-Abilities.png | 1448 × 1086 | Transparent montage: crescent slash, dash streak, spin vortex, pillar. Four static images, not animation frames. Separate crops needed. |
| LH-Abilities&Effects.png | 1536 × 1024 | Labeled reference board: five icons; slash, dash, cyclone, pillar sequences; five aura intensity previews; supporting bolts, streaks, rings and rocks. Not a regular-grid runtime spritesheet. Alpha ranges 51–251: no fully transparent background. |
| LH-Apparel1.png | 1448 × 1086 | Transparent front, side and back montage. Separate south/east/north exports needed, with consistent torso anchors and scale. Shoulder plates are present; no sleeves. Body-type fitting remains unverified. |
| LH-Hair1.png | 1448 × 1086 | Transparent front, side and back montage. Separate directional exports and head-template alignment needed. Side facing must be confirmed in game. |
| LH-Heart.png | 1254 × 1254 | Single transparent implant icon. Usable unchanged for Phase 1; copied byte-for-byte. Resolution is excessive for an inventory icon but not a blocker. Optional later 256/512-pixel export. |
| LH-Weapon1.png | 1448 × 1086 | Transparent three-view sword montage. Select one broad blade view for Graphic_Single, with grip alignment checked in game. The narrow side view is not a useful default world icon. |

## Proposed texture layout

Paths below are relative to Textures/. Only the heart is populated in Phase 1.

```text
Things/Item/Health/ElementalHearts/LightningHeart.png
UI/Abilities/ElementalHearts/ThunderSever.png
UI/Abilities/ElementalHearts/LightningFlash.png
UI/Abilities/ElementalHearts/ThunderstormDance.png
UI/Abilities/ElementalHearts/HeavenlyThunder.png
UI/Abilities/ElementalHearts/ThunderGodsAura.png
Effects/ElementalHearts/Lightning/Slash/Slash_00.png ...
Effects/ElementalHearts/Lightning/Dash/Dash_00.png ...
Effects/ElementalHearts/Lightning/Spin/Spin_00.png ...
Effects/ElementalHearts/Lightning/Pillar/Pillar_00.png ...
Effects/ElementalHearts/Lightning/Aura/Idle_00.png ...
Effects/ElementalHearts/Lightning/Aura/Combat_00.png ...
Effects/ElementalHearts/Lightning/Aura/Activation_00.png ...
Things/Pawn/Humanlike/Apparel/LightningUniform/LightningUniform_Male_south.png
Things/Pawn/Humanlike/Apparel/LightningUniform/LightningUniform_Male_east.png
Things/Pawn/Humanlike/Apparel/LightningUniform/LightningUniform_Male_north.png
# Repeat fitted apparel for Female, Thin, Hulk and Fat body types.
Things/Pawn/Humanlike/Hair/LightningHair_south.png
Things/Pawn/Humanlike/Hair/LightningHair_east.png
Things/Pawn/Humanlike/Hair/LightningHair_north.png
Things/Item/Equipment/WeaponMelee/LightningSword.png
```

No supplied file is an animated PNG or a set of separate frame files. The board depicts animation sequences but its frame sizes, anchors and spacing vary. Export each effect as separate equal-canvas RGBA frames, without text/background/grid, preserving glow alpha. Use a shared floor anchor for pillar/spin and a shared center for slash. The pillar is clipped at the top of its row: restore the intended top or deliberately author a beam continuation. Strip the illustrative pawn silhouettes from aura frames; actual pawn afterimages should be rendered from the pawn, not baked silhouettes. Cropping alone cannot cleanly remove the board background without damaging glow.

Planned playback in Phase 2: explicitly indexed frames with tick-based timing, rotation/translation for slash and dash, expansion for spin, and a designated impact frame for pillar damage. Aura states will loop separately in Phase 3. These are plans, not implemented animations. Static transparent montage crops or vanilla effects can stand in for unsuitable exports. No assets have been silently cropped, resized or repainted.

