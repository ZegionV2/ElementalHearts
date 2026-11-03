# Celestial clothing coverage and evolution-ready Bestow

Updated 2026-11-03.

Celestial Regalia now declares torso, waist, shoulders, arms, hands, legs and feet in RimWorld's native apparel body-part groups. Legs supplies the missing lower-body/groin clothing coverage. Its existing armor ratings now cover these parts too. It remains helmetless. The textures, head position, Thin chest fit, apparel layer and equipment IDs are unchanged. Existing manifested regalia receives the definition correction after a full game restart; replacement gear is unnecessary.

Celestial's Bestow (Random) still chooses one normal Lightning, Flame, Wind, Water or Earth lineage for its four-second paired ceremony. Successful completion also fulfills the existing 150 mastery kills and ten Ascendant days for the gifted lineage. The recipient stays a normal Ascendant; select Evolve to perform the ordinary evolution ritual whenever desired. Consciousness and other current-action checks still apply. The existing mastery and age save keys store readiness. No extra hediff, save flag, automatic evolution or separate UI command is added. Other evolved givers' Bestow retains ordinary evolution requirements. Interrupted or invalid ceremonies grant neither ascension nor readiness.

## Verification

Built against the installed RimWorld 1.6 and Harmony assemblies. Existing Bestow, Celestial combat/state and hair/manifestation regression suites and Celestial definition verification passed.

A fresh isolated RimWorld instance with Melee Animation loaded an existing pre-update save and completed **118 native checks**, including:

- Native apparel body-part coverage and male/female psychological nudity checks while wearing only Celestial Regalia.
- Paired stationary/facing Bestow, cross-lineage disciple cleanup, all five possible gifted lineages and default automation.
- Both milestones filled and the ordinary Evolve command enabled with God Mode off, while the recipient remains unevolved.
- Save/load during Bestow, after all five recipients become ready, during their chosen evolution rituals and at the final transformed-regalia checkpoint.
- All five ordinary evolution rituals complete once, preserving one lineage and releasing skill gates.
- Invalid outcomes, interrupted ceremonies, duplicate-grant rejection and ordinary evolved Bestow retaining its requirements.
- Revert restores original clothing/nudity state; remanifestation reuses the same regalia without putting a duplicate in inventory.

The native game log contains no exception, XML error or unresolved-reference match. The isolated test companion and process are removed after verification. Personal saves and settings are not used. This task changes mechanical coverage and Bestow only; it does not regenerate artwork or claim new visual QA.
