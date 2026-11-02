# Elemental helmets and transformation

Implemented for Lightning, Flame and Wind in the installed Elemental Hearts mod.

- Fully enclosed directional samurai-inspired helmets. Lightning was revised to charcoal and electric blue to match its existing suit.
- Transform equips armor, helmet and sword; it no longer changes hair. Revert restores previous equipment.
- Existing saved transformation kits upgrade their visor to a helmet once. The old visor stays as a cosmetic item. Legacy transformation hair is restored to the saved original hairstyle.
- Helmets inherit the visor melee accuracy/dodge bonuses and have 1200 HP with full-head protection. Visors now have no armor ratings or equipped combat offsets.
- Three-stage, 90-tick transformation: charge, equipment reveal with elemental burst, and fade. Heart ability automation waits during the transition.
- Generated shared Transform icon.
- Stronger positional thunder cues for Lightning, flame rush/burst cues and Wind cutting sounds. Audio uses remixed Core sound definitions, with per-pawn throttling and three simultaneous instances per custom cue. No external audio dependency.
- Auras still require both transformation and drafting.

Water remains the existing defense prototype; this update does not add its missing full transformation equipment.

Validation: local game assembly build and all eleven Source/Verify*.ps1 checks passed. Live pawn fit, audio mix, transformation timing and saved-kit migration have not been tested in a running game. Restart RimWorld to load the updated assembly and definitions.

Original generated sheets and prompts are preserved here. Installed sprites are transparent 256x256 PNGs. Backups of pre-change source, definitions and assembly: ../../work/helmet-backup.
