# Shared living-flight art

Generated with the built-in image generation tool for the 15 November 2026 travel update. The original generated PNGs are retained in the workspace's `work/flight-launch-sites/AssetsSources/` folder. Export preserves alpha and the aligned grid.

Installed assets:

- `Textures/UI/Commands/ElementalHearts/LivingFlight/FlyingPawn.png` — 256px flying-pawn world marker; also used for launch-site targeting and old saved journeys.
- `Textures/Effects/ElementalHearts/LivingFlight/Frames/Launch_00.png` through `Launch_15.png` — sixteen aligned 256px charge, lift and dissipation frames.

The same art is tinted gold for Celestial, yellow for Thunder, warm crimson for Hell Flame and pale storm blue for Tempest. A bounded pool, cached materials, camera/fog culling and frame blending support the existing elemental beams. There is no gameplay damage or persistent map object in these visuals.

[Generation prompts](LivingFlightArtPrompts.json). [Player controls and loading rules](GroupTravel.md).
