# Cargo helpers, launch presentation and impact audio

## Passengers and hauling

Selected companions gather beside the chosen launch site. They are excluded from the free-helper search, including passengers who can haul and have Hauling enabled. A native loading lord also receives a gathering job for a selected companion instead of taking them away to fetch cargo.

The loading menu can assign native work synchronously before its accepted crew is recorded. Once the manifest is accepted, an empty-handed selected passenger is redirected to gather. An actual carry operation already in progress may finish safely. Future loading jobs then gather that passenger. Existing preparations and saves retain their original crew, cargo and jobs.

Only an actual native cargo-hauling job for this flight counts as an outside helper's contribution. Drafted pawns, disabled Hauling, other controlled preparations and explicit orders are respected. When no eligible outside helper is available, the captain fetches the cargo, delivers it and returns to the chosen site. Hauling state remains saved. No extra pawn or physical transport pod is created.

## Shared launch icon and world journey marker

The five supported carriers—Celestial, Thunder, Hell Flame, Tempest and Aurora—share the new **Launch living flight** icon. Onward caravan flight uses it too. The original lineage-specific utility icons and elemental departure/arrival effects remain.

The generated transparent asset is [LaunchShared.png](../Textures/UI/Commands/ElementalHearts/LivingFlight/LaunchShared.png). It depicts an ivory/gold RimWorld-shaped pawn rising above a launch seal. [The generation record](SharedLaunchIcon.json) contains the complete final prompt and built-in generation method.

The flying-pawn journey marker retains its lineage tint. The shared group-flight definition and four older solo-flight definitions now use native **2.2 expanded-icon scale** and **200 icon priority**, compared with the shared flight's old default size and priority 60. The native draw/hit rectangle is at least 66 pixels square before additional native expansion. This updates journeys already in flight without creating a new world object or changing arrival actions.

## More distinct impact audio

| Lineage | Sound identity |
| --- | --- |
| Glacial | Short ice-spike fractures, cold rushes, crystalline parries and a denser explosion body. |
| Aurora | Its own resonant ice/crystal profile, faster bright cuts, a lower grand shatter and a quiet rising charge. |
| Tempest | Its own rushing-air/electric profile, sharp discharge cuts and heavy storm impacts. |
| Earth Adept | Stone fracture, pick/ground impacts, pressure buildup and a separate geological quake release. |
| Adamant Ascendant | Denser stone/crystal impacts, distinct quake/spire releases and a dedicated Bulwark formation sound. |

Sources are referenced at runtime, not copied out of their source packs. The [sound manifest](Audio/ImpactSoundManifest.json) records each authored definition, its layers, pitch/volume ranges and required source pack.

- **Vanilla Psycasts Expanded**, when active: Frostshaper ice casts/shatters, Staticlord Thunderbolt/Chain Bolt and Warlord Power Leap sounds, with restrained existing body layers.
- **Royalty**, when active: wall-raise and Skip pulse layers for suitable stone formations and sovereign charges.
- **Anomaly**, when active: a geological collapse layer for Earth Quake and a Nociosphere attack layer for Tempest impact.
- **Core fallback:** all thirty-eight base definitions remain available without those packs, using Core stone, crystal/glass, wind, short pressure-blast bodies, EMP/deflection and the mod's own short support clips. Tempest avoids long ambient thunder clips in its attack mix; the older Lightning/Thunderstorm banks retain their routing.

The resolver selects an available optional variant, then falls back to the base definition. Optional XML requires its actual source package. These are finite one-shots, with at most two layers per authored definition, three voices/two simultaneous copies per definition, unchanged pawn/map throttles and spatial/camera/fog culling. No new sustainer, damage tick or combat-power multiplier is added. Other elemental sound profiles retain their routing.

Garnet's proposed art, combat and audio are documented separately in [GarnetIncarnatePlan.md](GarnetIncarnatePlan.md); none of its abilities are shipped by this update.

## Verification

Verified in isolated RimWorld 1.6 profiles on 15 November 2026. Personal saves, enabled mods and settings were not used as test targets.

- **180 native cargo/icon/routing assertions:** a lone carrier collected 137 silver while its selected companion stayed gathered; saving during pickup resumed and delivered exactly the selected amount. A genuinely free outside colonist delivered cargo while the carrier waited. Drafted, Hauling-disabled and explicitly ordered helpers caused carrier fallback; the explicit order remained intact. All five carrier forms used the shared cached Launch icon, and all five shared/legacy world-flight definitions produced a native draw/hit rectangle of at least 66 pixels.
- **668 Core-only sound assertions:** all thirty-eight base definitions and their clip variants loaded with only Core, Harmony and this mod. The resolver chose the base sounds without DLC/Psycasts. The longest raw fallback source was approximately three seconds.
- **1,152 sound-source assertions with the optional packs active:** all seventy-two authored base/optional definitions had actual non-empty AudioClips, finite one-shot behavior and at most two layers. Pitched tails were checked against action-specific bounds: 3.5 seconds for slash/dash/parry, eight seconds for other cues and nine seconds for the deliberate large-quake rockfall tail. Fast ice sounds and storm cuts use short cast/shatter sources rather than long breath/hurricane buildup recordings.
- Static checks validated all referenced source paths, the transparent RGBA launch asset, XML, five marker definitions and unchanged existing save expressions. The production assembly compiled against the installed game and Harmony.

The original pawn/cargo state and arrival routing remain in use. The new native loading-job rule applies to future passenger jobs; it does not forcibly discard an actual carry already in progress. Audio checks establish valid sources, bounded playback and routing; final sound preference still depends on the player's volume mix and taste.
