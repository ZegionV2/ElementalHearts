# Cyclone Ascendant — playable Wind evolution

Normal Wind Ascendants can now choose **Evolve: Cyclone** after **150 unique hostile non-animal mastery kills and ten Ascendant days**. Existing Wind pawns start tracking these kills with this update; their existing Ascendant age counts. The 4.5-second emerald wind coronation can be interrupted without losing readiness. **DEV: Evolution-Ready follows God Mode**, the skull toggle.

Evolution upgrades the same saved condition, retaining the four Wind cooldown deadlines, Flow, automation/non-lethal choices, recovery timers, original clothing and transformation state. An already transformed Wind pawn receives the new outfit and sword without creating another ascension condition. No resource or ingredient cost.

| Skill | Behavior |
| --- | --- |
| Vacuum Draw | Upgraded twelve-cell line advance. Requires a clear, empty exit; each eligible lane victim is cut once, up to twelve victims. |
| Skyward Gale | Two rising cuts; a successful damaging cut briefly lifts a surviving target in emerald ribbons. A target-side recovery window prevents immediate repeated lift. |
| Cyclone Reversal | Watch the selected opponent, deflect an ordinary melee hit or react to a dodge, sidestep and counter. The primary receives one countercut; up to six distinct secondaries can receive one sweep hit each. Saved guard deadlines do not restart on load. |
| Heaven-Rending Tempest | Three timed cuts with safe flanking footwork, ending in a heavy emerald slash. Automatic combos can spend their remaining strikes on nearby enemies; manual casts keep the selected target. |
| Sovereign Cyclone | Automatically responds to at least two nearby hostiles while transformed. A 24-tick formation warns before at most six targets are held aloft for 120 ticks. Release commits one impact per target, then pushes survivors up to three safe cells outward. Forty-second cooldown, no resource cost. |

One **On / Off / Non-lethal Auto Combat** button governs all five techniques. Off stops new automatic casts; manual techniques remain available. Non-lethal release uses the shared wound-free suppression system. Sovereign Cyclone can also be pressed when its crowd condition is met. Explicit move/hold orders take priority over automation.

Sovereign's caster remains stationary. Replacement orders, forced movement, undrafting, incapacitation, death, loss of the caster or external animation control end the cast safely. Owned captures are released on interruption; stronger unrelated stuns are preserved. Victim despawning cancels control and prevents a later stale release impact. Glacial freeze takes precedence over Cyclone control. Actual capture/control is bounded to 64 active slots per map; each pawn has at most one airbound status and a short recapture recovery window.

Both actual world cells and smooth draw positions follow the short release push. Walls, pawns and unsafe cells stop it without collision damage. The controller uses bounded local searches; it creates no tornado pawn, building or projectile, changes no terrain/roof/weather/temperature and deals no periodic hold damage. Each release impact is recorded before applying damage and survives saving/loading, so a loaded release cannot strike again.

Cyclone receives the shared evolved benefits: **500% elemental movement, +50 ascension mood, 10× hauling/cargo bonus, 3× walking caravan-speed contribution**, disease resistance, stronger defense against normal Ascendants and Melee Animation knockout immunity. Other evolutions remain defense peers. Recovery is improved. Bestow grants **normal Wind ascension**, skipping discipleship, using the existing shared icon and a paired emerald investiture.

The accepted helmetless white/emerald regalia and indestructible fitted sword are integrated into Transform/Revert and the optional native Melee Animation profile. Downing restores personal clothing; recovery can reuse the same summoned outfit and sword. The subtle six-frame halo appears only while transformed and drafted. Ordinary Wind keeps its existing armor, aura and four techniques.

All existing 97 Cyclone textures are reused: seven body types in south/east/north with native west mirroring, the inventory/sword art, six icons and 68 effect/halo frames. Materials are cached. Existing visual pools remain 128 effects / 32 echoes / 64 airbound followers / 128 halos, with camera/fog culling and short-lived floor wisps. Ten new original audio clips give Cyclone its own slash/dash/spin/charge/impact variants, using the same spatial and crowd limits as the other elements.

Restart RimWorld to load the new definitions and assembly.
