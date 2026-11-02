# Flame Heart combat release

Flame now mirrors Lightning combat, tuning and boosts with separate component types, damage, job, mark, schedulers and visuals. This supersedes the cosmetic-only notes in FlameVisualIntegration.md. The one-Heart rule still applies across both elements.

Abilities (same ranges, damage multipliers and cooldowns as Lightning):
- Ember Cleave: directional melee/burn wave, 10 tiles, 5-second cooldown; applies Searing Brand.
- Cinder Step: blink and melee/burn strike, 15 tiles, 6-second cooldown.
- Inferno Waltz: 4-tile area strike and brand detonation, 9-second cooldown.
- Phoenix Descent: channelled melee impact and 3-tile burn burst, 12-tile targeting, 22.5-second cooldown. Same interruption/refund rules as Lightning.

Sovereign Flame Aura supplies identical capacity/stat boosts. Phoenix Vitality seals wounds after 3 seconds and heals ordinary injuries at 2 HP/second, with the same missing-part/scar/disease limits. Furnace Resolve grants +20 mood. Cinder Reflex mirrors the dodge counter. Searing Brand stacks to five, expires after ten seconds and grants the same owner-specific detonation and kill cooldown refund. Ember chains mirror Lightning chain chance/range/damage. Flame has resistance factor 0.25 against EH_Flame, Flame and Burn; its existing incoming-damage stat factor also applies. No EMP bonus or electrical resistance is granted.

EH_Flame inherits Core's Flame damage definition but uses DamageWorker_AddInjury, like Core Burn: Heat armor and Burn injuries, no attached/spreading fires. Physical weapon-derived damage remains in abilities just as for Lightning. No ongoing burn-over-time or fire immunity is implied. Friendly fire, auto combat/default, area threshold, aura, afterimages and opacity share the existing settings.

Appearance: Flame textures, orange afterimages, flame sounds and 2x2 icon atlas. The existing generated static sprites use the copied VFX timing and movement with fading rather than newly generated six-frame flame animation. Aura state now follows combat/ability activation, not drafting alone. Hair restoration retains the cosmetic release's saved key. A no-op legacy map component remains to load saves from that release without a missing-type error.

Validation: built against installed game/Harmony; both Verify.ps1 and VerifyFlame.ps1 pass; stage boosts and active ability tuning compared for equality; all Flame VFX paths checked; no EMP branch remains. Not runtime-tested. Restart RimWorld and test in a disposable save: all four manual powers, walls/line of sight, friendly-fire off/on, automation and queued movement, brand ownership/expiry, kill refunds, interrupted ultimate, heat resistance, save/load mid-cooldown/channel, and removal restoring hair/disabling powers. Test alongside a Lightning pawn to confirm independent marks and cooldowns.
