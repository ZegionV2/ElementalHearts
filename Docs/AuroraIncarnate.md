# Aurora Incarnate

**Water → Glacial Ascendant → Aurora Incarnate.** Aurora combines fast sword routes, frost and snow with green/cyan/violet northern lights. It is a sovereign alongside Celestial, Thunder, Hell Flame and Tempest.

## Transcend

Earn **300 new unique hostile non-animal kills** and spend **15 days as Glacial**, then press **Transcend: Aurora Incarnate**. The free frost coronation lasts eight seconds. Interruption keeps readiness. Existing Glacials start a separate saved tracker when this feature is installed; Water-to-Glacial kills and age are not reused. God Mode exposes **DEV: Transcend-Ready**, which unlocks the normal ceremony and hides immediately when God Mode is off.

The existing heart is upgraded in place. Water defense preferences, owned Pressure, recovery state, personal clothes and relevant remaining cooldowns survive. There is no stacked Glacial hediff.

## Seven forms

| Form | Action | Recharge |
| --- | --- | --- |
| Rimefall Crescent | Broad checked 22-cell sword lane; up to 24 enemies, one hit each. | 3 s |
| Aurora Blitz | Fast straight cuts between up to five distinct enemies; 18-cell first approach and eight-cell links. A lone enemy gets one stronger dueling cut. | 5 s |
| Frostcrown Waltz | Seven-cell frost spin, up to twenty enemies, one hit each. | 7 s |
| Aurora Dominion | Half-second formation, rippling aurora curtains and one six-cell shatter at a fixed point within 24 cells. | 18 s |
| Aurora Cataclysm | Automatic response to two hostiles within four cells: one six-cell burst, checked push up to four cells, icy-blue freeze and mild capped hypothermia. | 24 s |
| Mirrorfrost Reversal | Guard for two seconds against ordinary adjacent melee; ice mirage, safe retreat and one returning rush/counter. Also reacts with Auto Combat on. | 9 s |
| Throne of Endless Winter | One-second warning within 28 cells, crown formation and sixteen-frame collapse; one ten-cell impact against up to 32 enemies. | 40 s |

Techniques cost no resources. Interruption spends the chosen cooldown and releases owned movement/effects. Automatic combos follow the existing attack or drafted-wait order and respect other queued orders. Off disables automatic skills and defense while manual skills remain usable. Non-lethal uses suppression and bounded control without wound packets or hypothermia.

## Pressure, parry and thaw

Successful non-finisher hits build the existing **caster-owned Pressure**, capped at three stacks per caster/victim for eight seconds. Dominion and Throne consume only the caster's marks for at most **20% extra damage on the same impact**. Consuming three marks also grants a two-second 20% ward. There is no extra shatter damage packet.

Still Water and Returning Tide remain. Mirrorfrost shares recovery with native Water defense, and counter depth prevents recursive counter chains.

Cataclysm freezes ordinary enemies for five seconds. Sovereigns receive only 0.75 seconds after their checked push. All casters share each victim's thaw protection; another caster cannot refresh it. Dominion/Throne apply brief 0.75-second control. Aurora resists hostile Glacial freezing and cannot acquire hypothermia. Frost changes no map temperature, terrain, roof or building.

## Power and equipment

Healthy capacities use the established sovereign baseline: **Moving 650%, Consciousness 200%, Manipulation 300%, Sight/Hearing 250%**; **+75 mood**, ×3.2 melee stat, ×0.04 incoming damage, rapid recovery, common-disease protection, Melee Animation knockout protection, ×15 caravan carrying capacity and ×4 personal walking-caravan-speed contribution. Electrical-shock chance/duration are reduced by 75%.

Skill damage derives from the current melee profile with its multiplier and the shared tuning applied once. It is not an additional multiplication of the lineage's melee stat.

The bound, indestructible **Winterglass katana** has a wider native held silhouette and a fitted Melee Animation profile. The winter-prince suit and ivory split cloak cover real body parts below the neck, including groin, hands/fingers, legs and feet/toes. South/east/north artwork uses native west mirroring for supported bodies. No helmet. The six-frame ice halo and subtle aurora/ice-blade back aura appear only while transformed, drafted, standing and awake. Downing reverts to personal clothing; recovery preserves bound gear without duplicates.

## Utility

**Rime Step / Aurora Travel** shares one button. Right-click cycles **Step Auto → Step Manual → Travel**. Left-click Step toggles automation; Shift-left-click chooses a safe unrestricted map cell. One-second Step recharge, no resource cost. Automatic stepping preserves work routes, jobs and carried cargo.

Aurora Travel uses the current [group-flight controls](GroupTravel.md): choose an outdoor launch site, select people/items, gather/load (including carrier self-hauling), then launch with the shared flying-pawn world marker and dedicated aurora departure/arrival effects. Native settlement and ISEKAI Tower choices are preserved. No fuel, silver or world-travel cooldown.

**Bestow** grants ordinary Water through the shared paired ceremony.

## Visual and save bounds

The prepared artwork supplies 196 frames across 25 sequences, including a sixteen-frame grand shatter and six-frame halo, aura, snow and freeze loops. C# handles actual movement, one-hit sets, Pressure, freeze/thaw and timing. Effects use fixed pools, cached materials, local target searches and camera/fog culling. No spawned ice structures, weather simulation or permanent decals.

Mastery, ritual progress, skills/cooldowns, dash links and struck targets, fixed telegraphs, counter phases, freeze/thaw, Step/Travel state and equipment ownership are saved. See [asset inventory](AuroraIncarnateAssets.md).
