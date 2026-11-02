# Phase 1 verification

Historical Phase 1 notes. The user subsequently confirmed successful implantation, bonuses and implant artwork. See Phase2.md and RuntimeChecks.txt for the current implementation and runtime combat/save verification; later-phase statements below describe the original Phase 1 boundary.

## Evidence and limits

Read local Version.txt: **1.6.4871 rev573**. Inspected Core Def XML and reflected/disassembled IL from the installed Assembly-CSharp.dll; did not rely on the limited bundled Source folder as a current decompile. Compiled against that assembly, its own managed framework assemblies, UnityEngine.CoreModule, and the installed Harmony 1.6 DLL. Final fallback-compiler build succeeds without warnings.

Verified API existence, XML names and the native consumption paths below. This is **not** a gameplay or save-roundtrip test. Runtime content loading, actual surgery, failure/interruption, rendering, mod interactions and save/reload remain on the acceptance checklist.

| Bonus | Exact implementation | Verification |
|---|---|---|
| Move +60% | stage.statFactors: MoveSpeed = 1.6 | Core StatDef and native hediff factor path in StatWorker.GetValueUnfinalized |
| Melee damage +50% | MeleeDamageFactor = 1.5 | Core StatDef; native pawn stat factor |
| Attack speed +40% | MeleeCooldownFactor = 1/1.4 = 0.7142857 | Core StatDef and both Verse.VerbProperties.AdjustedCooldown overloads read it |
| Hit rating +4 | stage.statOffsets: MeleeHitChance = 4 | Core curve transforms raw score; NOT +4 percentage points. At raw rating 30, 0.93 becomes 0.942. Benefits taper near upper curve end. |
| Dodge rating +12 | MeleeDodgeChance = 12 | Core curve transforms raw score; at raw rating 36, 0.38 becomes 0.44. At lower scores benefit is larger. Vanilla ranged-aiming restrictions remain. |
| Global work +50% | WorkSpeedGlobal = 1.5 | Core StatDef; native hediff factor path. Only work governed by this stat is affected. |
| Damage reduction 30% | IncomingDamageFactor = 0.7 | Core StatDef; DamageWorker_AddInjury.ApplyDamageToPart reads the pawn stat. Injury-producing damage is reduced; not immunity to arbitrary scripted health changes. |
| Natural healing ×3 | HediffStage.naturalHealingFactor = 3 | Field and Pawn_HealthTracker.HealthTickInterval multiplication verified. No additional InjuryHealingFactor multiplier, avoiding double-counting. |
| Immunity gain ×1.5 | ImmunityGainSpeed = 1.5 | Core StatDef; Verse.ImmunityRecord.ImmunityChangePerTick uses it |
| Pain ×0.5 | HediffStage.painFactor = 0.5 | Field and HediffSet.CalculatePain verified |
| Bleeding ×0.5 | HediffStage.totalBleedFactor = 0.5 | Field and HediffSet.CalculateBleedRate verified; not a fabricated BleedRateFactor stat |
| Hunger drain ×0.6 | HediffStage.hungerRateFactor = 0.6 | HediffSet.GetHungerRateFactor and Need_Food consumption verified |
| Rest drain ×0.6 | HediffStage.restFallFactor = 0.6 | HediffSet.RestFallFactor and Need_Rest path verified. Do not additionally multiply RestFallRateFactor. |
| Joy drain ×0.6 | JoyFallRateFactor = 0.6 | Exists in Core, no DLC requirement; Need_Joy.NeedInterval reads it |

All stage fields and all selected stats exist in this installed build. Mood and comfort are untouched. ArmorRating_Sharp/Blunt/Heat exist, but this phase chooses incoming injury reduction instead of stacking additional armor rolls. Flat final hit/dodge percentage-point offsets are not directly supplied by the chosen stage fields; using native raw ratings avoids an unnecessary patch.

## Implant, recipes and persistence

Verified Core parents: BodyPartBase, ImplantHediffBase, SurgeryInstallImplantBase and SurgeryRemoveImplantBase. Verified Torso body-part Def, Human recipe user, BodyPartsArchotech category, Medicine category and the referenced surgery defaults. Copied the same recipe inheritance pattern as Painstopper. Torso installation adds an implant without replacing the natural heart; heart-part placement would tie installation/removal to a more fragile internal organ and complicate replacement-heart interactions.

Compiled overrides against actual signatures:

```text
RecipeWorker.AvailableOnNow(Thing, BodyPartRecord) -> bool
Recipe_InstallImplant.GetPartsToApplyOn(Pawn, RecipeDef) -> IEnumerable<BodyPartRecord>
Recipe_InstallImplant.ApplyOnPawn(Pawn, BodyPartRecord, Pawn, List<Thing>, Bill) -> void
```

Base install runs CheckSurgeryFail then Pawn_HealthTracker.AddHediff. Base removal checks failure, finds recipe.removesHediff, spawns spawnThingOnRemoved, then removes that hediff. Shared EH_ElementalHeart tags and a recipe subclass gate one-heart installation, including completion-time rechecking. This guards normal surgery, not arbitrary dev-mode/third-party direct AddHediff calls. No custom serialization or static pawn state is introduced. Standard hediff save serialization is retained; successful roundtrips are still untested.

Harmony is declared with modDependencies and loadAfter, referenced with copying disabled, and initialized with a unique ID. Phase 1 contains zero Harmony patches because clean extension points exist. Future patches have a dedicated Source/Patches folder.

## Later features: verified facts versus plans

- **Aura:** HediffDef has the XML renderNodeProperties field and RenderNodeProperties getter; Core archotech-eye XML already uses the field. A custom node/worker can handle aura animation without replacing the pawn body. Three-state animation logic and performance are not yet implemented or tested.
- **Thunder Reflex:** actual hit/dodge branching lives inside Verb_MeleeAttack.TryCastShot; GetDodgeChance only computes a probability. A postfix on that probability method would not identify successful dodges. This is a medium/high compatibility-risk hook; assess a narrow result-branch hook in Phase 3 and do not infer dodge from zero damage. No patch is installed now.
- **Electrified strikes:** melee damage methods exist in Verb_MeleeAttackDamage. The exact patch location, recursion guards and external mod ordering must be verified in Phase 3; no claim of compatibility yet.
- **Four abilities:** range, wall checks, teleports/flyers, fists/weapon scaling, ally/prisoner filters, EMP, telegraph frames, interruption and saved cooldown behavior await Phase 2. No unverified DamageDefs or AbilityDefs are shipped in Phase 1.
- **Electrical resistance / Static Charge / auto combat / gear:** deferred to their requested phases. Phase 1 incoming damage reduction is not electrical immunity.
- **Performance:** no added per-tick scans or VFX in Phase 1. Later object pooling, effect caps, visibility checks and bounded searches remain requirements, not completed work.
