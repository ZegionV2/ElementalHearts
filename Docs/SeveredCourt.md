# The Severed Court

The Severed Court is an enemy order of elemental sword warriors who believe Ascendants have the right to rule. Oathlings, Knights and evolved Heralds share its broken-crown insignia, obsidian plate, ivory trim and dark crimson cloaks.

## Shipped roster

| Rank | Lineages | Equipment and behavior |
| --- | --- | --- |
| Oathling | Flame, Lightning, Water, Wind, Earth disciples | Conventional Court uniform, bound elemental sword, existing disciple skills and automatic combat |
| Court Knight | The same five normal Ascendants | Manifested Court plate and enclosed helmet, original elemental sword and four techniques |
| Azure Herald | Azure Flame | Elite regalia, open-brow faceguard, blue fire and reactive Azure Reversal |
| Storm Herald | Thunderstorm | The same elite outfit, crimson lightning, Scarlet Tempest and Tempest Dragon |
| Frost Herald | Glacial | The same elite outfit, Pressure/parries and Glacial Explosion |
| Gale Herald | Cyclone | The same elite outfit, flowing sword combinations and Sovereign Cyclone |
| Adamant Herald | Adamant | The same elite outfit, Resolve, seismic impacts and Adamant Bulwark |

All five Heralds have their lineage's existing five combat techniques, stats, recovery, resistances, sword and optional Melee Animation profile. There is no extra faction damage multiplier. Swords, badges, effects and elemental halos identify each lineage. NPC Herald halos appear during active hostile combat while transformed; redeemed player Heralds wear their normal elemental outfits and follow the normal drafted halo requirement. The aura setting, fog/camera culling and existing bounded effect pools still apply.

Court generation produces adult Human-race pawns. Ordinary ranks use ages 20–58; Heralds 28–65. Each receives one assigned lineage, bypassing the generic NPC disciple roll. All start with automatic combat enabled. Other factions retain their existing configurable random-disciple behavior.

Five attackable Herald strongholds and their campaign events are implemented. Lunar/Eclipse Court bosses remain planned.

## Shared Herald regalia and faceguard

Heralds wear a distinct shared command uniform: broad obsidian shoulders, an ivory short mantle, pale-gold plate edges and fastenings, and a crimson cloak. The lower-face guard leaves eyes, forehead and hair visible. Its mechanical coverage is the mouth; it hides only the beard. All five use the same artwork rather than five recolors.

The outfit has seven fitted native body textures × south/east/north, with native west mirroring. There are separate armor/faceguard inventory icons and a new heraldic event seal. The armor retains each corresponding evolved outfit's stat and coverage rules. The small localized faceguard grants no extra melee or movement stats. Manifested pieces are bound and indestructible.

Hostile first-release Court warriors with an existing two-piece evolved kit gain one faceguard when reconciled. Recruited Court warriors instead renounce the oath and use normal elemental apparel. Their original bound sword, powers, quality, controls, cooldowns and personal clothing are retained; the projected Court faceguard is removed.

## Severed Oath

This trait selects Court transformed apparel and grants no powers or direct stat bonus. Court uniform variants retain the corresponding lineage's armor protection, equipped bonuses, coverage and quality. The oath remains while captured, awakening or evolving as a Court member. **Joining the player faction removes it as redemption.** The pawn immediately uses the normal elemental armor; a controlled animation finishes before the safe outfit change. Existing recruited pawns receive the same one-time correction when loading an older save. The trait has zero random commonality and is assigned to Court members explicitly.

Adding or removing the oath replaces matching projected apparel without replacing the bound sword or skill state. The change waits for the current transformation or controlled animation to finish. Reversion/downing restores personal clothing and parks projections in the existing hidden kit; it does not fill inventory with duplicates. Original transformation and sword-bond save keys are retained. Conventional travel clothes remain ordinary removable/lootable apparel.

## Backstories

New Court pawns receive native childhood/adulthood backstories, with small appropriate skill gains:

- **Crown ward / siege survivor:** two childhood backgrounds shared across ranks.
- **Oathling initiate / crown enforcer:** training and service under the Court.
- **Cinder prelate, scarlet courier, winter warden, sky duelist, citadel marshal:** five Herald careers, one for each element.
- **Oathbreaker:** a disciple who deserted after a punitive expedition.

These appear in the ordinary pawn biography. Existing saved pawns' personal backstories are preserved.

## Raids

- Ordinary Court raids become eligible from **day 30**, at **600 threat points**. Selection remains uncommon at small budgets.
- Oathlings cost **600**, Knights **2,400**, Heralds **6,000** points. Every generated squad contains at most **16** pawns and stays within its supplied budget.
- A Knight requires budget for at least two supporting Oathlings. Ordinary Knight caps are one from 3,600 points, two from 10,000, and three from 20,000.
- From **day 60** and **8,000 points**, eligible combat raids have an **18% commander roll**. At most **one Herald** appears; its cost is deducted before the escort is assembled. The five elements rotate through the remaining squad. Ordinary settlement groups and forced-downed groups do not roll Heralds. The five named strongholds have their own fixed Herald garrisons.
- Herald raid letters identify the evolved commander. Raids use native walking arrivals and raid lords, existing automatic abilities and allied synchronization. No siege, staging strategy or additional damage system is introduced.

New rolled Herald commanders draw from surviving, unengaged strongholds once the campaign has been seeded. Destroying all five prevents further Herald dispatches; ordinary Court ranks can still raid from any surviving regular settlements.

These thresholds describe eligibility; storyteller rolls, faction conditions and available threat budgets still decide when a raid actually occurs.

## Story encounters

**Herald Assault:** a dedicated native storyteller threat from day 60 and 8,000 points, guaranteeing one Herald with a budgeted escort. It uses a surviving stronghold’s lineage and a twelve-day cooldown. The raid budget is capped at 10,000. It does not interrupt a settlement’s already pending Court encounter.

**Stronghold Dossier:** from day 30, intercepted orders identify one of the real strongholds, its Herald, garrison and treasury. The location is a permanent settlement that a caravan can attack. Each dossier reveals one previously unreported stronghold, with a twelve-day event cooldown.

**Herald's Writ:** a rare late-game threat with a sealed warning **two days** before a Herald-led force. It requires day 60 and at least 8,000 points, fixes element/budget/seed/due time when issued, and has a fifteen-day event cooldown. The announced budget is capped at 10,000 points. A settlement cannot hold another pending Court arrival at the same time. The dispatching stronghold is saved with the order; attacking it recalls the expedition, and destroying it cancels the pending arrival. Legacy writs without an origin remain compatible and still arrive normally.

**A Broken Oath:** from day 30, a Court disciple can ask for sanctuary. The one-day choice explicitly states the consequences and can be postponed until its deadline. Accepting brings one wounded, redeemed disciple into the colony and schedules an ordinary **1,200–3,600-point** recovery squad after **one and a half days**. That pursuit has no Herald. Refusal or expiry grants no pawn and schedules no pursuit. The event has a thirty-day cooldown.

Pending orders and choice pawns are saved. Arrival is committed before invoking the native raid worker, preventing replay after a load. Unavailable approaches receive at most two hourly retries; an exceptional failure is never replayed. Orders are capped at two across the world and one per map. Deleted maps, defeated/non-hostile Court factions and disabled encounter settings cancel their pending forces. Disabling Heralds withdraws an already announced Herald order with a notice.

## Settings and world integration

- **Allow Severed Court raids:** controls Court combat selection and story pursuits; settlement inhabitants remain available.
- **Allow rare Court Herald commanders:** enables late-game Herald rolls, dedicated assaults and writs. Stronghold garrisons remain their fixed challenge.
- **Enable Court encounters and stories:** enables dedicated Herald assaults, writs, dossiers and deserter stories.
- **Show elemental badges beside Court pawn names:** toggles native map-label icons.
- **Add the Court to existing saves when loading:** enables the saved, one-time faction introduction.
- **Add Court strongholds to existing saves when loading:** seeds missing strongholds once when a living Court faction is present.
- **Add Court settlements / strongholds to this world:** available in an active game; explicitly adds a missing Court faction and seeds its five strongholds. Repeated clicks do not duplicate them.

New worlds offer one Court faction in faction selection; deliberately selecting zero is respected on later loads. The existing-save introduction adds one faction and one settlement on an unused suitable tile, preserving other factions/maps/settlements. A defeated Court is never recreated. Stronghold seeding uses independent saved progress, so worlds that already had the Court from the previous release still receive the new bases. Successful placements are recorded per lineage; cleared strongholds are never reseeded, and unsuitable/occupied tiles are skipped. There is no per-tick world search or settlement regeneration. Deliberately choosing zero Court factions still prevents automatic introduction; the explicit settings action lets you opt in later.

Badges draw beside native names without editing them, track the actual saved lineage and use cached textures with normal label alpha, scaling and fog visibility. Twelve icons support recruited/evolved oath bearers; they do not add Lunar, Eclipse or Celestial to the faction roster. This release adds no pawn skill-menu commands.

RimWorld 1.6, Core and Harmony are required. Melee Animation is optional. The faction, backstories and encounters do not require a DLC.

## Five permanent strongholds

| Stronghold | Commander |
| --- | --- |
| The Cinder Throne | Azure Herald |
| The Scarlet Spire | Storm Herald |
| The Winter Bastion | Frost Herald |
| The Highwind Citadel | Gale Herald |
| The Adamant Seat | Adamant Herald |

Select a stronghold on the world map, inspect its elite warning and send a prepared caravan. Use the native **Attack** action. Each generates a granite perimeter with a three-door front gate and rear postern, a central courtyard, a supported command hall, barracks, stores, an infirmary and a treasury. The nine-person initial garrison is **one Herald, two Knights and six Oathlings**, using native defensive lords and the existing automatic skills. The Herald garrison is present even before late-game raid eligibility; attacking is voluntary.

The treasury starts with **2,400 silver, 120 gold, eight advanced components, twenty industrial medicine, thirty survival meals and two doses of the matching normal elemental serum**. These are physical items to collect, not a repeating completion reward. Unclaimed provisions are held when the map unloads and restored on returning; taken/destroyed items do not regenerate. Surviving defenders keep their identities across retreat and saving/loading. All five forts share one architecture; the commander, elemental garrison, serum and biome distinguish them.

Defeating the defenders uses native settlement defeat and caravan exit rules. A cleared stronghold is never recreated by automatic loading or the settings button. Its pending expedition is cancelled and it can no longer supply new Herald dispatches. Other surviving Court settlements and their ordinary ranks remain active. Strongholds are persistent world settlements, not expiring quest sites.
