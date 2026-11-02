# Water disciples and subtle animated aura

Water uses the ordinary elemental teaching and disciple system. Glacial is now a playable evolution. See DiscipleCeremonies.md for the current shared readiness and ceremony behavior.

## Teaching, disciples and promotion

Select an ordinary Water Ascendant and use **Teach Water** on an eligible untrained humanlike pawn. The teacher walks to the pupil. Both then stop, face each other and wait through the existing three-second paired lesson. Completion grants one Water disciple condition. A new order, incapacitation, lost partner or other interruption ends the pair without awarding an incomplete lesson. Both teaching jobs now allow player interruption and cannot be suspended for later resumption.

A Water disciple has:

- Torrent Cleave, Flowing Step, Whirlpool Dance and Ocean's Judgement at **40% damage**, with **50% longer active cooldowns** (7.5 / 11.25 / 18.75 / 45 seconds).
- Shared **Auto Combat: On / Off / Non-lethal**, Still Water and Returning Tide. Counter damage also receives the disciple reduction; Pressure retains its existing caps, ownership and lifetime.
- **Call / Dismiss elemental sword**, using the existing indestructible bound Tidal Sword storage and recovery behavior.
- The existing six illustrated kill-progress stages and a kill/day tooltip.
- The **Ascend** button unlocks after **50 credited kills and five in-game disciple days** and starts a 3.5-second ceremony. NPCs start automatically; player-controlled disciples wait for the player's choice. Existing vanilla-attributed disciple kill rules are unchanged.
- **DEV: Ascend-Ready**, visible and usable only in God Mode, including cached commands. It fulfills the milestones; the disciple then uses Ascend for the normal animated ceremony.

Disciples receive no Ascendant stat stage, healing, mood bonus, armor transformation, aura or teaching power. On promotion, the disciple condition is removed before the full Water condition is added. Water's remaining ability cooldowns, combo state, non-lethal flag and defensive cooldown/pending state are copied to the new full condition; newly granted ascension enables Auto Combat. Promotion parks the existing bound sword; later transformation reuses the bond instead of minting a second sword. Normal Water Ascendants have the God-Mode-only Evolution-Ready command for Glacial.

Existing Water Ascendants retain the same saved defensive component and save keys. The existing same-object upgrade helper continues to preserve hediff identity, age, severity, component instances, transformation equipment and foreign components. Other elements retain their existing promotion behavior.

## Subtle Water aura

The aura has **six distinct transparent 256×256 frames**, advancing every five game ticks. The final art uses thin pale cyan ribbons, restrained droplets and a lighter lower ripple. Its default opacity gently pulses between **17% and 27%** before the shared effect-opacity setting. The drawing is smaller than the usual normal-element aura, cached, culled outside the camera/fog and drawn behind the pawn.

It appears only on a living, standing, **transformed and drafted full Water Ascendant**, after transformation finishes. It disappears when reverted, undrafted, downed or dead. Disciples have no aura. The existing Show Auras and effect-opacity settings control it; there is no additional pawn button.

Art was generated with the built-in image_gen tool, then revised with that tool after the request for a subtler style. ArtSource preserves both generated sources and both exact prompts. The game uses the subtle revision. Water-Aura-Preview.png shows full texture opacity; Water-Aura-Subtle-Preview.png shows the 22% midpoint. Both are asset contact sheets, not game screenshots; the in-game shader supplies the opacity pulse.

## Gear definition correction

Runtime dev validation exposed old burning/deterioration warnings on the indestructible equipment. All **19** protected elemental gear definitions now explicitly set Flammability and DeteriorationRate to zero and disable smelting. Their textures, defensive ratings, damage tuning and existing durability protection are retained. This prevents indefinitely burning projections and incorrect deterioration/smelting behavior. Two older evolved-uniform recipeMaker warnings and older cached-visual startup notices are separate existing diagnostics.

## Verification and limits

The production assembly was compiled against the installed RimWorld 1.6 and Harmony libraries. **All 31 automated verification scripts passed**, including the earlier equipment, transformation, defensive, evolution, Bestow, non-lethal, combat-order, asset and optional sword-profile checks. New checks cover Water training bindings, damage/cooldown tiers, promotion boundaries, defense-state copying, dev support, aura gates/opacity/frame coverage, six distinct transparent frames, shipped/planned descriptions and all 19 gear definitions.

An isolated native RimWorld test harness exercises actual pawns, jobs, damage and full game saving/loading. Its final evidence is in **Runtime-QA-Results.txt**, **Runtime-QA-Player.log** and **Water-BodyHead-Asset-Audit.csv**. It uses a separate save/config directory and controlled stationary hostile targets. It tests:

- Walls rejecting Step/Judgement, occupied landings rejecting Step, and a newly occupied landing interrupting an accepted Step while preserving its spent cooldown and releasing the job gate.
- Full native save/load mid-Step, during Judgement's telegraph, after Cleave has already hit, before a counter impact and after a counter impact. Cooldowns, marks, landing/progress and committed hits are checked, including no second impact on the saved victim.
- Frontal parry, counter recursion depth, counter cleanup and no permanent owned-skill lock.
- Paired lesson facing/stationary state, native save/load mid-lesson, one training grant, both-pawn cleanup and interruption without a training grant.
- Disciple damage/cooldown tier, dynamic cached dev-button visibility, sword calling and automatic promotion with retained mode/cooldowns and the existing sword bond.
- Ordinary transformation, full native save/load while transformed, native downing and recovery, personal shirt/pants restoration, hidden kit storage and retransformation without inventory projections.
- Native automatic execution of all four Water skills, interruption cleanup and Non-lethal suppression without new wounds or death.
- Native body and helmet material resolution in **all four directions for all seven body textures**, with an adult-reference head-offset CSV. This confirms asset resolution, not final visual fit at every life stage.

The runtime harness is a separate test companion and is excluded from the playable mod. No personal save was loaded or changed. The tests use the installed expansions and Harmony; they do not establish compatibility with the user's complete third-party mod list, optional combat-animation rendering, or a full older personal save. Pressure expiry/ownership, target caps/deduplication and repeat-safe legacy component migration also have focused automated fixtures.

**Visual armor/head alignment, skin coverage and aura placement remain unverified in the game window.** Automatic approval review rejected access to the RimWorld window with “Computer Use was not approved to use RimWorld.” The runtime material/head-offset audit and sprite checks cannot replace that visual check. An in-game check in every direction/body type remains necessary, especially with rendering mods or altered child/baby life stages.

Fully restart RimWorld to load the new DLL. Existing changed files are backed up in **Backup-before-Water-Lineage**; Install-Manifest.json records the before/after hashes. The install removes no production mod files.
