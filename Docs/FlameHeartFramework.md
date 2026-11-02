# Flame Heart framework

The framework is ready; Flame Heart gameplay is not activated.

Defs/ElementalHeartBases.xml provides four abstract parents shared by Lightning and future Hearts:
- EH_ElementalHeartItemBase: archotech implant category and default value/mass.
- EH_ElementalHeartHediffBase: shared EH_ElementalHeart tag and torso placement.
- EH_InstallElementalHeartBase: humanlike surgery, torso target, shared exclusion tag and Recipe_InstallElementalHeart completion check.
- EH_RemoveElementalHeartBase: standard humanlike implant removal.

Lightning now inherits these parents. Its defNames, component classes, saved fields, stats, art and abilities are unchanged. No new save migration is required by this change. The shared bases deliberately provide no combat bonuses or Lightning components.

Docs/Templates/FlameHeart.xml.example is an inactive starter outside Defs. It reserves EH_FlameHeart, EH_InstallFlameHeart and EH_RemoveFlameHeart as proposed names, without registering them in the game. It has temporary Lightning item artwork and no powers. To implement Flame later, copy it into Defs/FlameHeart.xml, replace the art, then add Flame-specific stages and components. Child fields may override inherited defaults such as MarketValue. Avoid duplicating inherited list entries such as the shared heart tag.

Keep Flame powers, cooldown/save keys, visuals and settings separate from Lightning. Existing Lightning patches select Lightning component types, so never give Flame those components merely to reuse a bonus. Reuse the shared surgery worker for every Heart: it rejects any recipient already carrying a tagged Heart, including queued second surgeries. Removal must return the matching element's item.

Validation: run Source/Build.ps1 and Source/Verify.ps1. The verifier now registers local abstract parents before checking definitions, regardless of file order. It is a static check, not an in-game test.

Before releasing Flame, test in a disposable colony: install each element independently; block Lightning-to-Flame and Flame-to-Lightning double implantation; queue mixed install bills; remove and recover the correct item; save/reload; confirm Flame receives no Lightning aura, abilities, recovery, hair controls or combat automation. Also confirm existing Lightning saves retain their effects and cooldowns. Define Flame balance, fire resistance, ally-fire behavior and acquisition before calling it playable.