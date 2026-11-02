# Wind Heart

Wind is an independent sword-technique implant built on the shared one-Heart surgery rules. Core and Harmony remain the only requirements. Vanilla Psycasts Expanded's installed Warlord Killskip and Nightstalker Assassinate sources were consulted for timed follow-up attack ideas; no code, art, sounds, Defs or dependency were imported from that mod.

## Techniques

All techniques require an equipped melee weapon, an awake conscious pawn capable of violence, and drafting for player-controlled pawns. They use weapon-derived damage with ordinary cut armor handling. Manual targets may belong to any faction. Unselected bystanders obey the shared friendly-fire setting; dead, downed and self targets are excluded.

| Technique | Behavior | Base cooldown |
|---|---|---|
| Dust-Cutting Draw | Short advance through the selected opponent, then a single 2.8x cut along the traversed corridor. Each victim is hit at most once. Requires a clear safe exit. | 4 seconds |
| Rising Gale | Adjacent double cut, 1.6x then 1.8x, eight ticks apart. Brief stagger and two seconds of Gale Guard. | 5 seconds |
| Turning Swallow | Watch a selected opponent for two seconds. Deflect their next ordinary melee hit or react to a successful dodge, sidestep, then counter at 3.5x. Nearby eligible bystanders receive a 1.5x sweep. Does not block ranged attacks or direct ability damage. | 7 seconds |
| Mountain-Scouring Tempest | Three timed cuts at 2.5x, 2.5x and 5x, changing flank between cuts when space permits. A single-target combination; no chained teleport kills or explosion. | 15 seconds |

Footwork advances one cell every two ticks with afterimages. Flanking routes avoid occupied cells, walls and fire, including diagonal corner checks. Routes, progress, timing and already-hit IDs are saved. New orders, disarming, stun, downing, undrafting, blocked routes or target escape interrupt the technique. Draw completes its traversal if its primary target is downed by someone else. Interrupted techniques keep their spent cooldown.

## Flow and boosts

The first technique establishes a rhythm. A different technique within eight seconds adds one Flow stack, up to three. Repeating the same technique resets stacks; waiting eight seconds expires the rhythm. Each stack shortens the newly used technique's cooldown by 8% and gives a temporary melee recovery improvement. There is no infinite stacking or passive chain damage.

Gale Breathing gives movement, manipulation and breathing bonuses, 1.75x move speed, 2x melee damage, 0.8x melee cooldown, 0.75x incoming damage, improved accuracy/evasion, reduced pain/bleeding, and modest injury recovery. Clear Sky Resolve adds +12 mood. All tuning is in Defs/WindHeart.xml.

## Shared integration

- On / Off / Non-lethal Auto Combat button, saved per pawn. Non-player-controlled Heart bearers always use automatic combat.
- Non-lethal replaces Wind cuts and ordinary melee wounds with shared suppression (75% temporary knockdown, otherwise a brief stun). Existing injuries, ordinary ranged weapons and unrelated damage are unaffected.
- Automation favors different techniques for Flow, uses the counter against an opponent attacking the bearer, and may use a retreating draw step when engaged in melee. Player movement/work orders and queued jobs are respected.
- Elemental Hair button selects compact Wind hair or restores the prior style. Hair is excluded from random style selection; uniforms have zero random-generation commonality.
- Uniforms have the same coverage, 600 HP and stats as Lightning/Flame, with 21 body/direction textures plus item texture. Katana and uniform use the existing crafting progression.
- All 73 Wind textures are transparent 256x256 PNGs. Seven animation sequences have six distinct frames each. Three aura states render behind the pawn with the existing opacity pulse and global visual settings. Wind does not require the flame/lightning atlas.

## Validation and use

Built against the installed RimWorld 1.6 assemblies. Verify.ps1, VerifyFlame.ps1, VerifyRefinements.ps1, VerifyNonLethal.ps1 and VerifyWind.ps1 pass. This is compilation, reflection, XML/asset and pure-policy validation, not a live game test.

Restart RimWorld. Obtain or spawn EH_WindHeart as with the existing Hearts, install it through surgery, equip a melee weapon and draft the pawn. Check all four moves, target movement/interruption, narrow corridors, guard expiry, non-lethal mode, aura toggles, and save/reload during a combination. No new heart acquisition/crafting system was added.

Artwork was generated with the built-in image generator using Flame/Lightning assets as style references, then cropped and scaled to the existing texture footprints. Original sheets, final prompts, an animation preview and exported textures are preserved in the WindHeart-Art output pack. Actual pawn rendering and combat still require live testing.

Balance update: Draw range is 10 tiles; Tempest approach range is 6 tiles. All Wind technique cuts have at least 50% armor penetration, with higher weapon penetration preserved. Staggers last 60 ticks. Tempest opening cuts stagger to help complete the combination. When already adjacent with no free flank, counter/Tempest follow-up cuts continue in place. All three Hearts now render auras only while drafted, including activation auras; NPC auto combat still works while undrafted, with no aura.

