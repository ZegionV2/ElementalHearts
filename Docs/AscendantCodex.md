# Ascendant Codex

Open **Ascendants** on RimWorld's bottom toolbar. The Codex is a separate colony window; it adds no commands to the pawn's elemental skill menu.

## Roster and progression

The roster includes living, player-owned, free humanlike disciples, all fourteen Ascendant forms, and recognized Otherworlders. It includes other maps, caravans, Light Travel and committed arrivals awaiting recovery. Prisoners and enemy pawns are excluded. Select a name to inspect it, or use **Locate pawn** to jump to its map/caravan.

Search names, forms or locations. Filter by stage, element, current map or fulfilled progression requirements. **Otherworlders** includes summoned people who have since received Bestow.

Progress reads the existing saved counters: disciples require **50 credited kills and five disciple days**; ordinary Ascendants/Lunar require **150 mastery kills and ten Ascendant days**. Celestial Bestow's immediate-readiness option remains supported, including Lunar's saved readiness flag. The Codex does not award kills or advance time. **Ready only** means the milestones are fulfilled; a busy, sleeping or incapacitated pawn must still become available before its ceremony can begin.

The inspection panel's **Ascend now / Evolve now** button starts the existing interruptible ceremony. All normal eligibility checks, animation, saved progress and gear ownership still apply. Evolved forms and Celestial have no further evolution displayed.

## Squad orders

Use row checkboxes or **Select filtered**, then choose:

| Order | Behavior |
| --- | --- |
| **Auto: On** | Enable the selected pawns' existing elemental automation. |
| **Auto: Off** | Disable elemental automation. Ordinary combat orders continue. |
| **Non-lethal** | Enable the existing non-lethal automation mode. |
| **Transform** | Start the native manifestation animation for eligible normal-clothed Ascendants. |
| **Revert** | Start the native reversion animation for eligible transformed Ascendants. |

Repeated Transform or Revert orders leave an already matching pawn alone. Transform/Revert skip off-map, contained, downed, sleeping, mentally unavailable, stunned, externally animated and busy elemental pawns, missing kits and pawns with queued orders. Disciples and unbestowed Otherworlders have no transformation. Combat-mode orders skip active elemental actions/ceremonies and pawns without elemental controls; idle off-map or incapacitated pawns can safely receive saved combat preferences. The footer reports updated, already-set and skipped counts with reasons.

Orders apply immediately to the selected roster entries; they are not persistent squad assignments or deferred retries. Changing filters does not clear previous selections. **Clear** clears them, and closing the window forgets the selection. No order automatically drafts pawns. The global **Player Auto Combat** mod setting still controls whether saved On/Non-lethal preferences run, and the window warns when it is disabled. Individual skill automation flags and cooldowns are preserved.

## Transcendant progression

Thunderstorm has a separate **Transcend** path: 300 new mastery kills and 15 Thunderstorm days. The roster offers a Transcendants filter and classifies Thunder Incarnate separately. Existing Thunderstorms start their new third-stage counters when the update first loads.

## Otherworlder origin and existing saves

New calls receive an invisible, stat-free origin marker. It survives outfit changes, Bestow, transformations and saving/loading. It grants no elemental powers, traits or bonuses.

Earlier Otherworlders can be recognized from a genuine ISEKAI identity plus an owned school uniform, including clothing stored by Transform. These entries are explicitly labeled **Legacy Otherworlder**. This is an inference, not a historical record: older people who have lost both their uniform evidence and their recorded origin cannot be identified reliably. The window does not rewrite their traits, clothes or lineage to guess.

Existing lineage, kill, age, cooldown, transformation and Calling save keys are unchanged. The roster refreshes only while the window is open, uses existing icons, and renders only visible list rows and the inspected pawn's portrait. This release introduces no combat roles, new powers or balance changes.
