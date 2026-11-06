# Adamant Ascendant — playable Earth evolution

Normal Earth Ascendants can evolve after 150 unique hostile non-animal mastery kills and ten Ascendant days. Newly added mastery starts at zero on existing Earth saves; their existing Ascendant age counts. The free Evolve: Adamant command runs the shared interruptible 4.5-second C# coronation with golden rings and rising facets. God Mode supplies DEV: Evolution-Ready, which only fulfills requirements; the normal Evolve button still performs the ritual. Turning God Mode off hides cached readiness commands.

Evolution upgrades the existing Earth hediff in place. The normal Earth label is replaced by Adamant, without adding a second stat stack. The same saved ability component retains the four cooldown deadlines, Resolve, recovery state and Auto Combat/non-lethal preference; only the new fifth deadline is appended. Original clothes, transformation state and sword quality carry over. No resources are consumed.

## Five skills and automatic flow

| Skill | Behavior | Cooldown |
| --- | --- | --- |
| Adamant Severance | Ten-cell fault cut, one 3.2x weapon-derived blunt impact per eligible lane victim; one Resolve per successful hostile cast. Stops at walls. | 5 seconds |
| Mountain King's Advance | Eight-cell bounded route to an empty melee flank; interpolated two-tick steps and one 4x impact. One route correction, at most sixteen movement steps. | 7 seconds |
| Sovereign Quake | A 36-tick ground stab releases a five-cell radial 2.9x impact, a 90-tick stagger and up to three safe outward cells. Each victim is hit once. | 15 seconds |
| Worldspine Eruption | Mark a fixed ground cell within ten tiles. After a 42-tick stab windup, a 3.5-cell spiked burst applies one 5.6x impact per victim, a 75-tick stagger and up to four safe outward cells. Consume up to three Resolve for +15% damage each. | 25 seconds |
| Adamant Bulwark | With two hostiles within three cells, an 18-tick brace raises a three-second guard: 40% remaining personal damage reduction, 15% allied protection within three clear cells. Strongest guard applies once. No reflected damage. | 35 seconds |

Weapon quality, normal shared elemental adjustment and native melee stats also affect damage. Adamant Resolve has three stacks, a twelve-second expiry and 7% remaining-damage reduction per stack. Source filtering and the existing gain throttle remain.

One On / Off / Non-lethal button governs all five abilities. Automatic decisions honor explicit movement/work and queued commands. They brace under pressure at a safe action boundary, Quake crowds, Advance across a useful clear gap, use Severance to build Resolve before the finishing burst, then Worldspine on a cluster or full Resolve and ordinary melee between cooldowns. Ten-cell local acquisition covers the available follow-up range after pushes. Skills do not replace accepted casts every tick. Manual strikes retain their primary intent and fixed eruption center.

Bulwark allows ordinary melee and other Earth techniques. Explicit movement, displacement away from its origin, undrafting, incapacitation, reversion, external animation control or removal cleans it up. Brief transitions between ordinary wait/melee jobs do not cancel it. It prevents this mod's forced pushes while active; it grants no general immunity to unrelated crowd control.

## Evolved benefits and equipment

The evolved condition has a 5x MoveSpeed factor, +50 ascension mood, 2.2x melee damage, 0.7142857x melee cooldown and a 0.10 incoming-damage factor before armor/Resolve/Bulwark. It inherits 10x carrying, a 3x individual walking-caravan contribution, disease resistance and Melee Animation knockout protection. Ordinary injuries seal after 180 ticks and recover at two health per second. These are evolved-tier benefits; normal Earth/disciple definitions remain separate.

Transform/Revert uses the existing smooth apparel manifestation. The kit is helmetless white/ivory and bronze-gold regalia with Worldbreaker. Original hair stays intact. The seven body variants have south/east/north textures, native mirrored west and baked side fitting; no additional old regal-side scale/offset is applied. Armor and sword are bound indestructible projections. Evolving while transformed replaces the old helmeted Earth kit, preserving weapon quality and stored personal clothes. Downing restores those clothes; later manifestation reuses the same evolved kit.

The six-frame faceted amber halo follows the shared native head origin and only appears while alive, standing, awake, transformed and drafted, outside transformation. The aura toggle/opacity settings apply. Normal Earth's body aura is replaced by this evolved halo. Worldbreaker's grip and blade profile is loaded through the installed native Melee Animation schema.

Adamant replaces Teach with Bestow Earth: the existing four-second paired wait/facing ritual grants normal Earth ascension directly and skips discipleship. It reuses Bestow's icon, saved commitment, cancellation and recipient rules. The ritual is amber/gold.

## Effects, safety and saves

83 prepared textures are integrated: the complete fitted outfit, Worldbreaker, six command/evolution icons, six halo frames and 48 ability frames. Cached materials and the existing 96-slot Earth visual pool supply frame crossfades, charge streaks, guard formation and dismissal. Head adornments have the shared visible-pawn budget; camera/fog culling remains. A bounded 32-slot cosmetic draw layer eases checked knockback destinations over ten ticks; it owns no pawn jobs or delayed damage and resets harmlessly on load.

Each attack inspects at most 512 local things and hits at most 16 distinct victims. Paths and pushes recheck walls, closed doors, occupied cells and fire; range and replan budgets are bounded. Parried hits do not add displacement. Non-lethal uses the existing wound-free suppression system. There are no spawned spike buildings, terrain edits, roof changes, weather effects, collision damage or periodic extra impacts.

Existing Earth save keys and the first four numeric skill meanings remain. Bulwark uses appended slot 4 and a saved origin/expiry. Native jobs save their timing, expected cell, route budget, fixed eruption center and impact commitment before damage. A loaded committed impact cannot replay. Replacing a cast releases ownership instead of retaining a separate permanent busy flag.

## Validation

The build passed 5,593 Earth regression checks, 65 new compatibility/Scribe/navigation/pool checks, Adamant definition/asset validation, all eleven native Melee Animation sword-profile checks, and targeted shared hair, transformation, equipment, benefits, Water-lineage and Thunderstorm movement checks.

An isolated graphics-enabled native fixture loads a prior Earth save, tests in-place evolution and equipment replacement, every body/facing material, all effect materials, real hits/knockback/stagger, all five automatic skills, Bulwark strengths and cleanup, paired Bestow, downing and re-manifestation. Saves are exercised mid-coronation, charge, Quake, marked eruption, Bulwark and Bestow, plus after a committed impact. The native sequence completed 191 checks across the saved checkpoints. Final results are recorded with the installed-build manifest.

The fixture corrected a transient-job guard cleanup issue and verifies that ordinary wait/melee transitions retain Bulwark. It uses extra-durable isolated targets for a complete five-skill observation; actual gameplay damage is not reduced. Native material and draw-path checks are programmatic, not a visual screenshot review or frame-rate benchmark. Seven-body/four-facing alpha coverage was measured against native body silhouettes during asset preparation. Other mods that replace body meshes can still require individual fitting.

The current standard evolved-tier correction, including Worldbreaker's 23-power point/edge, is documented in [the balance and side-fit guide](MoonSideFitAndAdamantBalance.md).
