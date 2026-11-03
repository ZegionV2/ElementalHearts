# Elemental Hearts

**RimWorld 1.6 elemental ascension, disciples, evolved Ascendants and Celestial solar powers.**

Transform pawns into powerful elemental fighters with bound swords, indestructible projected armor, animated techniques and automatic combat combos.

## Download and install

1. Download **ElementalHearts.zip** from the [latest release](https://github.com/ZegionV2/ElementalHearts/releases/latest).
2. Extract the `ElementalHearts` folder into your RimWorld `Mods` directory.
3. Confirm the layout is `Mods/ElementalHearts/About/About.xml`.
4. Enable **Harmony**, then **Elemental Hearts**, in the mod loader and restart RimWorld.

RimWorld **1.6** and **Harmony** are required. DLC is optional. Melee Animation and Search and Destroy integrations are optional.

The [main branch download](https://github.com/ZegionV2/ElementalHearts/archive/refs/heads/main.zip) contains the newest playable build. Copy its contents into `Mods/ElementalHearts` so `About`, `Assemblies`, `Defs` and `Textures` sit directly inside that folder.

## Playable lineages

| Normal Ascendant | Evolution | Theme |
| --- | --- | --- |
| Lightning | Thunderstorm | Red lightning, spinning attacks and Tempest Dragon dash chains |
| Flame | Azure Flame | Blue fire, regal gear and reactive Azure Reversal |
| Wind | Cyclone | Emerald wind, sword forms and bounded airbound cyclone attacks |
| Water | Glacial | Flowing counters, Pressure combos, ice techniques and Glacial Explosion |
| Earth | Adamant | Resolve, Faultbreaker charges, faultline cuts, radial Seismic Quake and Spire Eruption |
| Lunar | Eclipse planned | Moonbrand, gravity pulls, counterattack and ranged protection |
| — | Celestial | Standalone fully evolved sun, solar and light lineage |

Earth is a normal Ascendant with basalt stoneplate, a closed mountain-crown helmet and Faultbreaker. Seismic Quake replaces Bastion; Spire Eruption replaces Mountainfall. Both use animated ground stabs and safe outward knockback. Adamant evolution is playable; see [Adamant mechanics and validation](Docs/AdamantIntegration.md). Craft Earth serum at a drug lab after Fabrication; see [Earth mechanics and controls](Docs/EarthAscendant.md).

## Progression and controls

- Acquire an elemental heart to grant its lineage. Use **Transform** to manifest the armor and bound sword, then draft the pawn for combat.
- **Auto Combat** starts enabled for newly granted disciples and Ascendants. Shared **Off / On / Non-lethal** controls let you choose automatic behavior.
- Normal Ascendants **Teach** disciples face to face. Disciples unlock **Ascend** after 50 credited kills and five disciple days.
- Eligible Flame, Lightning, Water, Wind and Earth Ascendants can evolve after 150 additional unique hostile non-animal kills and ten ascendant days.
- Evolved Ascendants **Bestow** normal ascension directly through a paired ceremony.
- New eligible humanoid NPCs from other factions have a configurable **20%** disciple spawn chance. They receive one saved **5%** near-death combat awakening roll, with injury healing, a shockwave and temporary protection on success.
- Downed Ascendants restore personal clothing and retain their projected gear for later transformation.
- God Mode supplies readiness commands; turn it off to hide them. Readiness buttons unlock the normal Ascend/Evolve action.

## Celestial Ascendant

Craft a **Celestial Heart** at a drug lab after Fabrication, or use **Elemental Hearts → Grant Celestial ascension** in the developer actions on an eligible pawn.

Celestial arrives fully evolved, with ivory/gold regalia, the **Dawnbringer** sword, a subtle solar halo and a golden light-sword aura while transformed and drafted. It has stronger defenses, 650% elemental movement, +75 mood, increased carrying capacity and a stronger caravan speed contribution.

Six combat techniques combine **Dawncleaver**, **Sunstride**, **Corona Dance**, **Solar Judgment**, **Heavenly Zenith** and the reactive **Fake Rainbow** parry/retreat/counter. Radiance supports automatic finishing combos.

**Bestow (Random)** grants Lunar ascension with a 2% chance, or Lightning, Flame, Wind, Water or Earth ascension with a 19.6% chance each. It fulfills the gifted lineage's evolution milestones, including saved readiness for Lunar's planned Eclipse evolution. The recipient stays a normal Ascendant until you press Evolve; readiness persists through saving/loading. Other evolved Ascendants' Bestow retains the ordinary evolution requirements. Celestial Regalia mechanically covers the torso, shoulders, arms, hands, waist, legs and feet, providing complete clothing coverage without a helmet. **Light Travel** manually selects a world destination and safe landing cell, with no resource cost or cooldown and large departure/arrival beams.

Recent presentation updates add expanding golden shockwaves, ivory impact flashes, directional particles, layered beam cores and prism accents. These use cached materials and bounded effect pools. Milira is not required.

The latest armor fix raises the visible head above the collar, keeps hair/beard/halo aligned, and gives Thin-body regalia a flat masculine/unisex chest. It preserves complete body coverage and blends through Transform/Revert.

## Lunar Ascendant

Celestial's **Bestow (Random)** grants Lunar at 2%. For sandbox testing, use **Elemental Hearts → Grant Lunar ascension** in the developer actions. Transform to manifest the moon-silver cloak armor, crescent mask and **Moonwake**, then draft the pawn to use its five techniques and show its aura.

**Crescent Draw** builds Moonbrands. **Moonfall Step** closes in on a marked enemy and pulls nearby foes together. **Falling Moon** spends the caster's marks in a telegraphed ground strike. **Tidal Parry** waits for an adjacent enemy's melee hit, then counters and briefly pins them. **Moonveil** wards the caster and nearby allies against ranged weapon damage for four seconds. Auto Combat can chain the three offensive moves, and the shared Non-lethal mode substitutes suppression for injury.

See the [Lunar art previews and source pack](ArtSource/Lunar). Lunar is a rare Bestow lineage. It is outside the ordinary NPC disciple pool and has no teaching command. A successful Celestial grant saves future Eclipse readiness with the pawn.

## Documentation

- [Adamant Earth evolution: five techniques, progression and saved state](Docs/AdamantIntegration.md)

- [Earth Ascendant mechanics, acquisition and combat](Docs/EarthAscendant.md)
- [Earth stone/gravel/seismic sound update](Docs/EarthAudio.md)
- [Adamant original design notes](Docs/AdamantEvolutionPlan.md)
- [Earth build and validation](Docs/EarthValidation.md)
- [Earth combat and presentation polish](Docs/EarthPolish.md)
- [Earth polish validation](Docs/EarthPolishValidation.md)
- [Celestial mechanics and save behavior](Docs/CelestialAscendant.md)
- [Solar effects and performance limits](Docs/CelestialLightPolish.md)
- [Head/collar and Thin-body armor correction](Docs/CelestialArmorFitFix.md)
- [Teaching and disciple ascension ceremonies](Docs/DiscipleCeremonies.md)
- [Cyclone integration](Docs/CycloneIntegration.md)

The `Docs` directory also retains historical development notes and future plans. The playable-feature summary above describes the current build.

## Source and validation

The prebuilt `Assemblies/ElementalHearts.dll` is included. C# source is in `Source`, definitions in `Defs`, and artwork/sound in `Textures` and `Sounds`.

Build against your locally installed game and Harmony assemblies:

```powershell
powershell -File .\Source\Build.ps1 -RimWorldDir "C:\Games\RimWorld"
```

The game and dependency assemblies are supplied by your local installation. `Tests` contains the regression scripts.

The solar-effects build passed 43 automated checks and native rendering, combat, interruption, mid-animation save/load, Bestow and round-trip travel tests. The latest armor correction passed 321 native head-fit checks, 307 asset/placement checks and all 28 body/direction coverage checks, plus targeted Celestial and hair/manifestation regressions. Native test fixtures use isolated saves and are excluded from this repository's runtime files.

Restart RimWorld after replacing the compiled assembly or apparel textures.
