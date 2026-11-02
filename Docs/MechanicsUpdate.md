# Elemental Hearts mechanics update

## Implemented

- Disciples automatically gain their own element's full ascension when they have 50 credited pawn kills and five in-game days (300,000 ticks) as a disciple. Promotion waits until the pawn is spawned, conscious and free of a controlled animation. Existing disciple age is retained; the new saved kill counter starts at zero because the previous release did not track kills during discipleship. The counter follows vanilla kill attribution and counts animals as well as other pawn kills. Promotion replaces the training condition, without granting a second element or consuming serum.
- Each disciple can Call elemental sword or Dismiss elemental sword. Calling stores the existing weapon in inventory. Dismissing restores it when available. Sword ownership, condition, call state and hidden storage are saved. Promotion reuses the same sword for the full transformation.
- Bound elemental swords cannot be dropped, stripped, transferred or replaced by ordinary equipment actions. They vanish into their saved owner storage before knockdown/death equipment drops and return within 30 ticks after the pawn is no longer dead or downed. A disciple's sword must have been called; an ascendant must still be transformed. Ordinary personal weapons keep their normal behavior.
- Melee and elemental damage packets from disciples and ascendants are reduced by 20%, once per final hit. This also covers secondary elemental hits. Ordinary ranged attacks are unaffected. Cooldowns, healing, defenses, non-lethal behavior and weapon artwork are unchanged.
- Existing smooth armor manifestation and reversion remain. Transformation kit stays out of inventory after reverting.

Azure Flame is a separate design proposal in Azure-Flame-Evolution-Plan.md, not an enabled evolution.

## Validation

Build against the installed RimWorld and Harmony assemblies passed, as did all 13 existing verification scripts. Additional headless checks passed for threshold boundaries, vanilla kill notification ordering, self/null/already-ascended kill guards, damage scope, equipped-to-hidden sword transfer, repeated recovery and revival cycles, duplicate binding prevention, previous manifestation coverage, hidden-kit identity and personal inventory preservation.

The ownership harness suppresses callbacks that require the running Unity game. Live Harmony startup, actual equipment UI actions, promotion during combat, visual rendering, and save/reload including corpses still require in-game verification. No live save round-trip is claimed.

## Loading the update

Fully restart RimWorld. Existing disciple time is recognized automatically; kill tracking begins with this update. No save editing is required. Previous runtime/source files are backed up. Keep this update enabled for saves containing its new sword bindings and hidden storage.
