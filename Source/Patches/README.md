# Patch boundaries



LightningCombatPatches.cs contains three named postfixes:

- Verb_MeleeAttackDamage.ApplyMeleeDamageToTarget: observe damaging ordinary melee hits. Native hediff callbacks do not expose this attack result.

- Verb_MeleeAttack.SoundDodge: observe the verified successful dodge branch without changing the hit/dodge roll. Queue the counter outside the original attack stack.

- Pawn.PreApplyDamage: distinguish self-attributed lightning from explicitly identified external electrical damage; preserve existing absorption.



No prefixes/transpilers or full method replacements. Kill credit uses the native hediff death notification; aura rendering uses a native map component. See Docs/Phase3.md for modded multi-packet melee and replaced-dodge-branch limits.



Phase 4.1 adds LightningGearColor.cs: a ThingWithComps.DrawColor postfix limited to EH_LightningUniform and EH_LightningSword. It prevents saved dye/material tinting, including worn apparel. The optional Search and Destroy bridge reads enabled state and job provenance; it does not patch that mod or replace its AI.


Phase 4.3 adds LightningResilience.cs: a CalculatePartEfficiency postfix compensates only HP loss from non-permanent injuries on present body parts, and a StaggerHandler.StaggerFor prefix rejects stagger for implanted pawns. Scars, absent parts, disease stage offsets and prosthetic modifiers are retained. Normal stun is separate and remains supported.

Phase 4.4 adds LightningHairColor: PawnRenderNode.ColorFor postfix, restricted to hair nodes using the two Lightning HairDefs. Returns white tint to preserve the artwork. Does not change story.HairColor or other hairstyles.
