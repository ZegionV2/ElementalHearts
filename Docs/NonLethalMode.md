# Three-state Auto Combat

The shared Auto Combat icon cycles On -> Off -> Non-lethal -> On for Lightning and Flame Hearts. Old saves retain their On/Off setting; the new restraint preference is saved per Heart. Non-player-controlled pawns retain always-enabled automation.

Non-lethal mode replaces elemental injury packets and ordinary melee wounds with restrained suppression. Each eligible hit has a 75% chance to incapacitate the target for 1,800 ticks (30 seconds at normal speed), otherwise it briefly stuns for 45-90 ticks. Reduced force (20%) determines stun duration. Abilities, passive strikes, chains, counters and manual casts respect restraint. Downed targets receive no further damage from these restrained hits. Ordinary ranged weapons are not covered.

Suppression caps consciousness at 20% and movement at 10%, then expires automatically. Vanilla random death-on-downing is suppressed only while applying this status; the prior setting is restored immediately. Existing wounds, diseases, other attackers and other mods can still cause death.

Validation: rebuilt against the installed game; Verify.ps1, VerifyFlame.ps1, VerifyRefinements.ps1 and VerifyNonLethal.ps1 pass. No live-game test performed. Restart RimWorld, cycle the button, test both Hearts against a healthy hostile pawn, and verify recovery and mode persistence after save/reload.
