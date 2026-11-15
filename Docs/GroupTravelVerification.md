# Group travel verification

Verified against the installed RimWorld 1.6.4871 assemblies, Harmony, Melee Animation, ISEKAI RPG Leveling and the ISEKAI Dungeons add-on. The release assembly compiles against Core and Harmony without an ISEKAI or Anomaly assembly dependency.

**69 functional assertions passed across saved test checkpoints.** Tests ran in isolated save-data folders and used the original pawn and item instances.

- All four carrier lineages create a single temporary transporter, use their current enhanced capacity and need no refuelable component or fueling port.
- The native loading dialog preselects the carrier and accepts companions plus loose allowed silver. Real EnterTransporter and HaulToTransporter jobs board two pawns and load 137 silver.
- Saving/loading during loading, after boarding and during world flight preserves manifests, passengers, destination and original bound sword identity.
- Missing carrier, changed allegiance and a newly added roof block launch. Dismissing a loaded transport unloads its carrier safely.
- Native launch skyfallers and world flight deliver passengers and supplies. Form-caravan arrival and a second flight launched directly from that caravan preserve the whole party.
- Settlement choices include native edge/center attacks and the added chosen-cell option. The check caught and corrected a native helper which dispatched shuttle menus instead of pod menus.
- Chosen-cell cancellation retains the caravan. Pending landing survives a save/reload; roofed destinations reject without transferring passengers, and successful placement gives each pawn a separate safe cell and clears the pending state.
- The Tower's native Floor 1 menu action is captured without creating an empty world caravan. Its own floor-discovery, picker and God Mode rules remain in the native menu implementation.
- A saved Tower journey restores the native floor arrival action, generates and enters the real Floor 1 with the complete party and original sword, and creates no generic ground map at the Tower tile.
- When the Tower is already occupied, a second arriving party stays in a caravan instead of entering the existing live floor. Completed native arrivals leave no elemental combat-action lock.

The background test harness skipped menu font measurements and the loading screen's wait-for-display flag because no GUI frames are drawn in batch mode. Native menu labels/actions, loading jobs, saved ownership, world ticking, map generation and arrival callbacks executed normally. These are functional checks; no visual layout or frame-time benchmark is claimed.

Existing solo-flight classes, jobs and serialized keys remain present for journeys already in progress. The previous solo-flight verification is documented in [WardrobeAndLivingFlightVerification.md](WardrobeAndLivingFlightVerification.md).

Anomaly/Nociosphere effects remain [a plan](AnomalyTravelEffectsPlan.md). No new gameplay code references or spawns those effects.
