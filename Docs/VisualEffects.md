# Elemental visual effects

Azure Flame uses seven dedicated eight-frame effect sequences: a blue-white crescent, a compact comet trail, a rotating three-flame pyre, a rising phoenix, an open phoenix warning seal, a braided Reversal trail and a dispersing Reversal burst. Adjacent frames crossfade. Comet and Reversal movement leave short-lived cyan apparel afterimages fitted to the pawn's body and facing.

The phoenix seal remains at the target during the existing interruptible warning. The phoenix rises from the impact position. Controlled Pyre retains exactly three damage pulses, sixty ticks apart; the animation creates no additional hits. Reversal still requires two nearby enemies and keeps its existing leap, rush, impact and cooldown rules.

Adamant's six-frame halo is an open elliptical circlet with a shaded rear wall, a thick illuminated front rim and small crystal glints. It floats above the head with a gentle bob. Its six-frame asset paths and visibility rules remain compatible with existing saves.

An ordinary Earth Ascendant's personal aura is now six floating basalt stones. Their orbit includes bobbing, mild rotation, perspective scaling and separate foreground/background depths. All six stones share one cached material and one reusable mesh per visible pawn. They are visual geometry, not spawned items, buildings or projectiles. Earth attack effects and Adamant's combat abilities retain their original behavior.

Eclipse's Dusk Crescent and Sovereign Eclipse wave art have their cutting edges on the left side of the source image. Their renderer accounts for that orientation in every firing direction. Dash, backstab, spinning and warning effects retain their established orientation.

## Limits and persistence

- Azure: 96 transient effects, 32 afterimages, 128 registered halo wearers; at most 64 visible halos.
- Reversal: 48 transient effects, eighteen-tick lifetime, eight frames per phase.
- Earth: at most 64 reusable orbit meshes, six stones and one draw submission per visible normal Ascendant. One 1024×512 atlas supplies eight stone variants.
- Existing camera/fog culling, aura/afterimage switches and effect opacity remain in effect.
- Existing `EH_azurePyres`, pulse fields, reversal job fields and `EH_azureReversalReadyAt` save keys are preserved. Transient artwork is not saved and cannot replay damage on load.

Animation frames are 256×256 RGBA with a common center, scale and transparent gutter within each sequence. West apparel afterimages use native east mirroring. Materials are cached and effects do not alter terrain, temperature or buildings.
