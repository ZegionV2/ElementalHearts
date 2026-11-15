# Legacy solo world flight

**Current controls:** new journeys use [loadable group transports](GroupTravel.md). This document describes the retained solo-flight implementation for journeys already in progress in existing saves.

Light Travel, Thunder Travel, Hell Travel and Tempest Travel now work as **solo living vessels**. Choose a settlement or land tile, take off, follow the moving world-map icon and choose a landing cell **after arriving**. The world leg has real travel time; it does not teleport or prepare the destination map before departure. This follows the local Demon Summoning Infernal Flight model without adding a dependency on it.

## Controls

- Celestial: press **Light Travel** to choose the destination. Right-click offers manual or automatic landing.
- Incarnates: keep the existing shared utility button. Right-click cycles **Step Auto → Step Manual → Travel**. In Travel mode, left-click chooses a destination; Shift-left-click chooses automatic landing on arrival.
- Step remains the separate instant map-wide blink. World travel requires a **different tile**.
- Launch **outdoors**, with empty hands. Equipment, personal clothing, bound gear and inventory stay with the original pawn. No silver, fuel, resource charge or cooldown.
- Flight time is **eight ticks per world tile**, clamped to 60–6,000 ticks (one to one hundred seconds at normal game speed). Game speed affects its real-time duration. Oceans can be crossed; land on a usable land tile or an existing map on the same world layer.

The pawn visibly rises during departure, becomes a selectable moving world icon and follows a spherical route. Selecting it shows the remaining journey and a route line. Landing commands remain disabled until arrival. **Return to origin** turns it around from its current world position and takes a real return journey; it does not snap back.

## Arrival and cancellation

Canceling destination selection leaves the pawn, camera and time state at the origin. The departure ceremony remains interruptible; interruptions retain the original pawn and gear on the map.

After the journey, existing maps are reused and new destinations generate a temporary landing map once. Manual arrival pauses and opens the map: **yellow** marks an empty, walkable, unroofed landing cell; **red** marks a blocked cell. Incarnates retain their existing freedom to choose unseen cells; Celestial landing requires revealed terrain. Arrival descends through the lineage's own light, thunder, hellfire or cloud effects.

Canceling landing keeps the original pawn safely held at the destination. Select the world icon and choose a cell again, land automatically, or return to origin. A newly blocked or roofed cell cannot consume or duplicate the pawn. Temporary origin/destination maps remain reserved during departure and transit.

## Existing saves

Existing pawn, heart, clothing, bound-weapon and utility-mode save identities remain intact. New saved flights retain route origin, position, total/remaining ticks, return direction, landing intent, drafting and the single original passenger. Loading an en-route flight does not finish it early. Older instant-travel objects migrate directly to their already-arrived landing state and keep their original passenger; they are not replayed backward.

No physical pod item, transporter building, fuel supply or new faction dependency is required. Flight is solo; put down carried pawns/items before departure.

[Native flight, wardrobe and save/load verification](WardrobeAndLivingFlightVerification.md).
