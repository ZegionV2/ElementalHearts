# Otherworld Calling — prepared assets

Created 2026-11-07 with built-in ImageGen. These assets now support the implemented Celestial utility: spend **1,000 silver** to call one human aged **18–24**, wearing a Japanese academy uniform and carrying strong ISEKAI RPG LEVELING traits. Calling, wearable definitions, payment, trait assignment and timed C# presentation are implemented; existing sound cues are reused.

## Contents

| Asset | Count | Format |
| --- | --- | --- |
| Gakuran worn clothing | 21 native + 3 FemaleBB | 256×256 RGBA |
| Sailor worn clothing | 21 native + 3 FemaleBB | 256×256 RGBA |
| Clothing inventory icons | 2 | 256×256 RGBA |
| Otherworld Calling command icon | 1 | 256×256 RGBA |
| Portal opening | 8 | 256×256 RGBA |
| Portal hold/glint loop | 6 | 256×256 RGBA |
| Portal closing | 8 | 256×256 RGBA |
| **Total runtime PNGs** | **73** | Transparent backgrounds |

Seven native body variants: Male, Female, Thin, Fat, Hulk, Child and Baby. South/east/north are authored; west uses the engine's east mirroring. There are no duplicate west files. Technical Child/Baby fallback textures do not permit underage calling: generation remains strictly 18–24. Both designs have flat chest shading on every body type.

## Texture paths

- `Things/Pawn/Humanlike/Apparel/OtherworldGakuran/OtherworldGakuran` — inventory base; worn suffix `_<Body>_<direction>.png`.
- `Things/Pawn/Humanlike/Apparel/OtherworldSailor/OtherworldSailor` — equivalent inventory/worn layout.
- `UI/Abilities/OtherworldCalling` — command icon.
- `Effects/OtherworldCalling/PortalOpen_00` through `_07`.
- `Effects/OtherworldCalling/PortalLoop_00` through `_05`.
- `Effects/OtherworldCalling/PortalClose_00` through `_07`.

Use native `Graphic_Multi` body resolution and west mirroring for apparel. Custom-body fallbacks must be scoped to these new outfits. The new outfit-specific fallback handles these paths separately from the Court-only compatibility. FemaleBB has dedicated flat art fitted against the union of the installed male/female body masks; keep the pawn's actual BodyTypeDef unchanged.

## Apparel fit and mechanics for implementation

The gakuran is navy/charcoal with brass buttons and a short band collar, integrated dark trouser panel and tucked shoulders. The sailor outfit has an ivory blouse, compact navy flat yoke, burgundy bow and opaque navy pleated lower panel. No helmets, hands, boots, long collars or detached sleeves are painted into either torso.

One ordinary full outfit should mechanically cover **Torso, Shoulders, Arms, Waist and Legs**. Use ordinary cloth durability and clothing stats; do not grant Ascendant armor protection or indestructibility. Do not mark Head, Hands or Feet as protected. Use appropriate clothing layers so other outfits cannot clip through it; normal removal, storage and existing transformation clothes restoration must work. The sailor lower panel covers the same native lower torso/leg groups as the gakuran's trouser panel.

Previews use the native body mesh proportions and native head offsets with no Ascendant head lift. All fitted variants now use the native **1.0 worn draw scale**, narrow two-pixel padding and the outfit-specific head lift. Keep body-specific drawing scoped to this apparel and use the same scale in the real renderer. Recheck lying poses, portraits, belts, hair and normal apparel overlap in the actual game before shipping wearable definitions.

## Effects and rendering

Frames are named in chronological order. The upright portal uses a common ground pivot; preserve the exported whole frame, use the recorded registration, and do not crop every frame to its own visible bounds. Opening changes size intentionally. Use the frame manifest for registration/timing rather than deriving scale from alpha bounds at runtime.

Suggested C# timeline: 0–60 ticks draw the existing Solar Seal; 60–108 play eight opening frames at six ticks each; 108–156 hold eight ticks per loop frame; 156–180 reuse the existing arrival ring/beam and commit once; 180–212 play eight closing frames at four ticks each after releasing the caster. Visual tail lifetime must not own gameplay state. Blend opening/loop and loop/closing briefly so ring width does not pop between the separate generated sequences. No native damage packets, push, terrain, temperature, roof or weather changes.

Cache materials at load, reuse the existing bounded local effect pool and camera/fog culling. One visual owner per ritual, no per-frame Thing spawns or pawn generation. The paid pawn must arrive even if effects are culled or the pool is full. Reuse existing Celestial SolarSeal, ArrivalBeam, ArrivalRing and TravelMotes assets where needed; they are not duplicated in this pack. Select soft existing celestial sounds at gameplay integration; no new sound files were generated here.

## Verification and delivery

Every runtime PNG is 256×256 RGBA, nonempty and has transparent corners. There are **64 silhouette coverage checks** across two designs, eight body variants and four directions; all report zero uncovered required native body pixels. Native-proportion preview sheets were visually inspected, including side/rear and mirrored west. These are offline fit checks using exported game masks, supplemented by native all-direction portrait checks and gameplay/save-load tests.

The installed mod now includes the new textures, wearable definitions, compiled Calling gameplay and documentation. Masters, exact prompt set, fitting reports, animation preview and SHA-256 manifest are included in the deliverable pack. Do not install the pack as a separate duplicate Elemental Hearts mod; merge its `ElementalHearts/Textures` and `ElementalHearts/Docs` into the existing mod.
