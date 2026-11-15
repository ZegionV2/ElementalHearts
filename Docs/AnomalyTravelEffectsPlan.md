# Shelved Anomaly-inspired travel effects proposal

Status: shelved at the user's request. Anomaly/Nociosphere-inspired effects are reserved for a future new element. Do not implement this earlier travel proposal. The original design below is retained as a reference only; no effects or gameplay changes were implemented.

The installed Anomaly definitions provide useful references: `NociosphereDeparting`, `NociosphereDepartComplete`, `NociosphereDepartingRing`, `NociosphereDepartCompleteDistortion`, `EnergyCrackle`, `HoraxianField`, `HoraxianSparks`, and the sound `Pawn_Nociosphere_Arc`. The native departure is a long sequence: its final flashes begin at tick 660. Applying that effect unchanged would make a short transport launch feel delayed and mismatched.

## Proposed sequence

| Phase | Proposed appearance | Timing and limits |
| --- | --- | --- |
| Loading | Existing temporary transport, with a quiet elemental seal | No continuously spawning particles while waiting for cargo. |
| Launch charge | A low ring draws inward beneath the pod, with a few arcs running up a compact energy envelope | About 30–45 ticks; one ring and one pooled emitter per transport. |
| Lift-off | The ring contracts to a bright point; a small distortion wave follows the departing vessel | One distortion pulse, 6–10 motes and a single sound. The launch and ownership transfer still happen exactly once. |
| World flight | Existing moving transport marker | No map effects or new objects along the route. |
| Arrival | A reverse ring opens beneath the incoming transport, then a short energy flare and fading sparks | About 20–30 ticks; one shared effect for the whole party, with no effect per cargo stack. |
| Dismiss/cancel | Seal gently fades | No flash, damage, explosion or lingering sound. |

Use a compact vessel envelope inspired by the Nociosphere's layered presentation; keep pawn silhouettes readable. Do not copy its segmented creature body, hostile AI, vaporization, heat-spike projectiles or departure delay.

## Later combat applications

After the travel presentation is approved, use the same restrained visual vocabulary on major sovereign techniques:

- **Thunderfall:** a contracting warning ring, branching electrical arcs along the existing pillar, and one outward distortion pulse at the existing impact tick.
- **Sovereign's Pyre:** an ember-dark pressure envelope with a crimson ring and a short heat-distortion release.
- **Crown of the Tempest:** a cloud-gray contraction ring, sparse white arcs and an expanding pressure ripple at the committed storm impact.
- **Celestial finishers:** an ivory/gold ring and a clean light-pressure ripple, with a few rising motes instead of the Nociosphere's ominous field.

These additions would be visual layers on existing telegraphs and single-impact rules. They must neither create additional hit packets nor turn harmless visuals into fires, explosions or terrain effects. Ordinary attacks should remain much quieter than finishers. Start with travel, then one grand technique per lineage, and review readability before expanding to additional abilities.

## Lineage treatment

| Carrier | Palette and motion | Sound direction |
| --- | --- | --- |
| Celestial | Ivory/gold ring, thin rising rays, soft amber distortion | Clear solar chime followed by a restrained impact. |
| Thunder Incarnate | Black/yellow arcs, quick branching streaks, white lightning core | A short electrical snap and bass thunder. |
| Hell Flame Incarnate | Black/ember envelope, curling red-orange embers | A low ignition rush and compact flame burst. |
| Tempest Incarnate | Dark cloud rim, white electricity and a spiral release | Wind pressure and a brief thunder roll. |

## Compatibility and performance

- Resolve native Anomaly references only when Anomaly is active. Without it, use the mod's existing rings, beams and motes; travel mechanics must remain identical.
- Reference installed DLC content at runtime. Do not redistribute DLC textures, shaders or sounds in the mod or GitHub repository.
- Cache definitions and materials. Use the existing bounded effect pools, camera/fog culling, and one transport-level effect rather than one per passenger.
- Avoid the native completion effect's full forty-particle field burst for every launch. Set an explicit small budget and avoid flickering full-screen flashes.
- Save sequence timing only. Rebuild transient visual handles after load; never replay launch, consume cargo, repeat an arrival or apply damage because an effect restarts.
- Cancel emitters on dismissal, launch interruption, despawn and map removal. Loading a save mid-charge must leave passengers in exactly one holder.
- Provide an intensity control and honor existing effects settings. Audio should not stack across multiple simultaneous transports.

## Verification before release

Compare all four lineages at normal and maximum game speed; check close and distant cameras, fog, multiple transports, a large passenger list, paused launches, saving/loading mid-charge and cancellation. Repeat with Anomaly disabled. Confirm zero gameplay effects, no duplicate passengers or cargo and no permanently running sounds or emitters.
