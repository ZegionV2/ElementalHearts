# Flame Heart — RimWorld-style texture pack

Artwork-only package made with the built-in image-generation tool, using the installed Lightning assets as references. Supersedes the ornate fantasy concepts. No active game definitions or DLLs were changed.

## Included

- 21 worn armor textures: south, east and north for Male, Female, Thin, Fat, Hulk, Child and Baby; plus an inventory graphic.
- Three directional hair textures, aligned to the existing short Lightning hair bounds.
- Flame sword and Heart implant icons.
- Four separate skill icons: Ember Slash, Cinder Step, Inferno Ring and Phoenix Fall. Names are proposals, not implemented powers.
- Four separate effect sprites: Slash, Dash, Spin and Pillar.
- Three hollow aura sprites: Idle, Combat and Activation.
- Original generated sources, prompts, and a texture validation report.

## Compatibility and remaining integration

All 38 exported textures are 256x256 RGBA PNGs with transparent corners and nonempty artwork. Apparel and hair were fitted to the visible bounds of the corresponding installed Lightning textures. Body-type variants are fitted versions of the three generated views, not independently painted anatomy. West can use the game's mirrored east view. The inventory graphic and wornGraphicPath follow the existing mod's naming layout.

These are prepared source textures, not a playable Flame release or a guarantee of in-game fit. No gameplay has been enabled. Pawn/head alignment, other apparel overlap, body types, dye/hair tinting and rendering on maps must be checked in RimWorld. The existing Lightning color patches do not cover Flame; extend them during implementation if artwork colors should stay fixed.

Aura files are THREE STATIC STATES, not animation sequences. Effect files are also static sprites. They need a Flame-specific renderer or animation hookup. Do not point the existing six-frame Lightning renderer at this folder: its required frame sequence is not present. No artificial duplicate animation frames are supplied.

Integration/FlameUniform.xml.example is an INACTIVE standalone apparel template. It lists Torso, Shoulders, Arms, Hands, Waist, Legs and Feet from the installed Core body-part groups. Coverage is defined by game data, not by drawing realistic limbs on RimWorld's abstract pawn. It excludes Neck and Head as requested for below-neck coverage. The template must be installed and tested before this coverage takes effect; no active apparel definition was changed in this artwork pass.

TextureValidation.json records format checks; representative final-size armor, hair, aura and icon images were visually inspected. No in-game testing was performed.
