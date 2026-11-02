Current lineage/aura update: **WaterLineageUpdate.md**. The notes below document an earlier phase. Water teaching and disciples are now playable; Glacial evolution remains planned.

Current active-skills update: **WaterAbilitiesUpdate.md**. The notes below document an earlier phase; the four active Water techniques and Pressure are now implemented.

# Water Ascendant — equipment and recovery phase

Water now has a complete normal transformation kit: charcoal/navy armor with cyan-white wave inlays, an enclosed water helmet and the bound **Tidal Sword**. This release implements the equipment/recovery phase of the Water plan. Its existing parry and counter remain the available Water combat techniques; the four planned active Water techniques, Water disciples/teaching and Glacial evolution are later phases.

## Transformation and equipment

Use **Transform** on a Water Ascendant. The existing 90-tick held transformation job stops the pawn facing forward and performs the original smooth bottom-up apparel reveal. The kit uses the existing staged body/head reveal and cyan pulse. **Revert** restores personal apparel and the original weapon. Summoned items are parked in the heart's hidden holder rather than remaining in personal inventory. Downing/death uses the existing emergency reversion and bound-sword storage; use Transform again after recovery.

Armor, helmet and sword are indestructible and inherit the same equipment locks and saved quality/ownership behavior as the normal Ascendant kits. The uniform covers Torso, Shoulders, Arms, Hands, Waist, Legs and Feet in the apparel definitions. Its artwork is a compact torso without drawn limbs; all seven body types have front/east/back sprites, with native west mirroring. The opaque full-head helmet hides hair and beard while retaining the head-apparel render branch. Existing personal hair/gear returns on reversion.

The Tidal Sword uses normal melee verbs, the normal Ascendant sword's existing 23-point sharp/stab base tuning and 2.6-second tool cooldown before native material/quality/stat modifiers. A fitted optional Melee Animation profile is active. Its primary hand is anchored at texture pixel (128,195), with a second anchor around y226; both land within the handle. The profile's blade span is fitted to the generated blade. Melee Animation remains optional.

## Normal Water powers

The whole-body Water condition now uses the ordinary Lightning/Flame Ascendant stat stage rather than the stronger evolved tier. These bonuses apply through the condition:

- +0.2 Consciousness; +0.5 Moving, Manipulation, Sight, Hearing, Breathing, Blood Pumping and Blood Filtration; +0.25 Metabolism, Talking and Eating.
- MoveSpeed stat factor 1.6, MeleeDamageFactor 2, MeleeCooldownFactor 0.7142857, WorkSpeedGlobal 1.75, IncomingDamageFactor 0.5, ImmunityGainSpeed 2 and JoyFallRateFactor 0.6.
- Pain factor 0, bleed factor 0.25, natural healing factor 6, hunger/rest-loss factors 0.6, MentalBreakThreshold offset -0.25 and the same normal melee accuracy/dodge offsets.
- Existing **+30 Water mood** is retained. Actual movement and damage combine with native capacities, equipment, quality, global elemental damage tuning and other modifiers.

Water uses the shared ordinary Ascendant recovery component: older wounds seal after **180 ticks / three game seconds**, with 75% tending quality where tending is available. The worst eligible non-permanent ordinary injury heals **2 HP per game second**, after the age threshold. This does not regrow missing parts or cure scars/diseases. The normal resilience patches restore efficiency lost to non-permanent injuries and prevent ordinary hit stagger, while retaining missing-part, scar, disease and prosthetic limitations.

## Existing defense and saved state

The original **HediffComp_WaterDefense** stays in place. Still Water retains its **80% frontal parry chance**, **120-degree cone**, **45-tick / 0.75-second recovery per attempt** and **1.6 counter multiplier**. Failed parry attempts still consume recovery. Counter-loop prevention, hostile-only counters, queued-order protections, Auto Combat Off/On/Non-lethal behavior and the existing damage exclusions remain.

Defense now waits while the pawn is transforming, evolving or in a paired teaching/Bestow action, preventing a parry/counter animation from competing with that action. It resumes when the action ends. No Water active-skill buttons are added in this phase.

These existing keys are unchanged: **waterAuto**, **waterNonLethal**, **waterReadyAt**, **waterCounterTarget**, **waterCounterAt**, plus the counter driver's **waterElapsed** and **waterStruck**. The new recovery timer uses its normal independent **EH_recoveryElapsed** key. Transformation and bound-sword save keys remain unchanged.

The compatibility helper adds missing components on the same saved Water hediff and retains the same comp list and existing component instances. It preserves load ID, age, severity, defense modes/timing/pending target, transformation/kit references and foreign extensions. Repeating the upgrade does not duplicate components or reset recovery. It scans loaded/world/dead pawns once during game finalization and leaves other elemental conditions alone.

## Artwork

Generated with the built-in image_gen tool: a three-direction armor sheet, three-direction helmet sheet and a sword. The integrated kit contains **27 transparent 256×256 RGBA PNGs**: 21 body/direction armor sprites, one armor inventory icon, three helmet directions, one helmet icon and the sword. ArtSource retains the original generated sheets, six style references and exact prompts. Water-Kit-Preview.png is an asset contact sheet, not a game screenshot.

## Validation and in-game check

The assembly compiled against the installed RimWorld 1.6 and Harmony assemblies. **All 27 automated verification scripts passed.** New checks cover same-object/repeat-safe Water component upgrades, preserved defense/transform/foreign state, normal recovery binding, gear-dependent production transform availability, Water equipment identity, indestructible gear handling, stagger protection and native Scribe save output for old defense keys and the new recovery timer. Asset checks cover every Water PNG, apparel/head coverage fields, hair concealment, normal stat-stage equality, +30 mood and migration/resilience bindings. The installed optional Melee Animation library deserialized Water's real profile and both grip anchors passed. Existing equipment, evolution, Bestow, combat, teaching and elemental regressions also passed.

The save-output fixture uses a minimal deep-saved target reference stub to satisfy native reference checking. It verifies emitted keys/IDs rather than loading a full Pawn. Native loading, actual healing, creation/equipping of the new gear, GPU rendering, head/body fit, optional animated grip and live combat remain untested in the headless environment.

**Fully restart RimWorld.** Load an existing Water Ascendant and check that its automation mode/cooldown remains, then Transform and inspect all directions. Revert and confirm personal gear returns and no summoned items remain in inventory. Down/recover/retransform, test Still Water/Returning Tide, observe recovery on an ordinary injury, and save/reload while transformed and during the reveal/counter. Check a new Water Serum recipient too. Inspect child/other body shapes and any third-party rendering mods used in the save.

Previous installed files are backed up in Backup-before-Water-Ascendant-Kit. Install-Manifest.json records before/after hashes. This release removes no files.
