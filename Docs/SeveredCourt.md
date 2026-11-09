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

Attackable Court fortresses, configurable settlement totals and their campaign events are implemented. Lunar/Eclipse Court bosses remain planned.

## Shared Herald regalia and faceguard

Heralds wear a distinct shared command uniform: broad obsidian shoulders, an ivory short mantle, pale-gold plate edges and fastenings, and a crimson cloak. The lower-face guard leaves eyes, forehead and hair visible. Its east-facing sprite is registered to the front of the mouth/chin rather than the center of the cheek; west uses the native mirror. Its mechanical coverage is the mouth; it hides only the beard. All five use the same artwork rather than five recolors.

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
- From **day 60** and **8,000 points**, eligible combat raids have an **18% commander roll**. At most **one Herald** appears; its cost is deducted before the escort is assembled. The five elements rotate through the remaining squad. Ordinary settlement groups and forced-downed groups do not roll Heralds. All fresh Court bases have their own fixed Herald garrisons, independent of raid budgets.
- Herald raid letters identify the evolved commander. Raids use native walking arrivals and raid lords, existing automatic abilities and allied synchronization. No siege, staging strategy or additional damage system is introduced.

New rolled Herald commanders draw from surviving, unengaged strongholds once the campaign has been seeded. Clearing every Court base prevents further Herald dispatches. Ordinary ranks remain controlled by the native faction/raid rules.

At **10,000 original incident points or more**, a Court combat raid guarantees exactly one Herald when commanders are enabled and a surviving, unengaged base can dispatch one. This maximum-budget guarantee does not require day 60. Original points are carried through native arrival/strategy adjustments; the final group still respects its actual budget and sixteen-pawn cap. Forced ordinary recovery pursuits, noncombat groups, forced-downed groups and disabled Herald settings do not receive a commander.

These thresholds describe eligibility; storyteller rolls, faction conditions and available threat budgets still decide when a raid actually occurs.

## Story encounters

**Herald Assault:** a dedicated native storyteller threat from day 60 and 8,000 points, guaranteeing one Herald with a budgeted escort. It uses a surviving stronghold’s lineage and a twelve-day cooldown. The raid budget is capped at 10,000. It does not interrupt a settlement’s already pending Court encounter.

**Stronghold Dossier:** from day 30, intercepted orders identify one of the real strongholds, its Herald, garrison and treasury. The location is a permanent settlement that a caravan can attack. Each dossier reveals one previously unreported stronghold, with a twelve-day event cooldown.

**Herald's Writ:** a rare late-game threat with a sealed warning **two days** before a Herald-led force. It requires day 60 and at least 8,000 points, fixes element/budget/seed/due time when issued, and has a fifteen-day event cooldown. The announced budget is capped at 10,000 points. A settlement cannot hold another pending Court arrival at the same time. The dispatching stronghold is saved with the order; attacking it recalls the expedition, and destroying it cancels the pending arrival. Legacy writs without an origin remain compatible and still arrive normally.

**A Broken Oath:** from day 30, a Court disciple can ask for sanctuary. The one-day choice explicitly states the consequences and can be postponed until its deadline. Accepting brings one wounded, redeemed disciple into the colony and schedules an ordinary **1,200–3,600-point** recovery squad after **one and a half days**. That pursuit has no Herald. Refusal or expiry grants no pawn and schedules no pursuit. The event has a thirty-day cooldown.

Pending orders and choice pawns are saved. Arrival is committed before invoking the native raid worker, preventing replay after a load. Unavailable approaches receive at most two hourly retries; an exceptional failure is never replayed. Orders are capped at two across the world and one per map. Deleted maps, defeated/non-hostile Court factions and disabled encounter settings cancel their pending forces. Disabling Heralds withdraws an already announced Herald order with a notice.

## Settings and world integration

- **Allow Severed Court raids:** controls Court combat selection and story pursuits; settlement inhabitants remain available.
- **Allow Court Herald commanders:** enables Herald rolls, the maximum-budget guarantee, dedicated assaults, writs and ultimatums. Stronghold garrisons remain their fixed challenge.
- **Enable Court encounters and stories:** enables dedicated Herald assaults, writs, dossiers and deserter stories.
- **Show elemental badges beside Court pawn names:** toggles native map-label icons.
- **Add the Court to existing saves when loading:** offers the saved, one-time settlement-count menu before creating a missing faction.
- **Add Court strongholds to existing saves when loading:** offers the setup menu to worlds that already contain a living Court faction.
- **Configure Court settlements in this world:** reopens the menu in an active game. Select the total to place, counting existing and cleared bases. It can explicitly introduce a missing faction. Lower totals never delete existing sites; repeated applications never duplicate or replenish them.

New worlds offer one Court faction in faction selection; deliberately selecting zero is respected. With the Court enabled, initialization brings its native bases to a minimum of five, distributing missing elemental commanders first. A larger native-generated base count is kept. Every Court settlement uses the custom fortress map generator, including ordinary settlements created by the game. Native world-object IDs are retained.

Existing saves display a one-time settlement-count menu before adding a faction or sites. Five is recommended; zero keeps the Court absent. Existing bases and cleared sites count toward the saved total. The menu allows totals up to twenty; worlds already exceeding that count retain their existing sites. Reopen it through settings to add more. Closing the window postpones the choice; Keep world as it is records a decision without adding bases. A defeated faction is never recreated. There is no per-tick world search or automatic replacement of destroyed bases.

Badges draw beside native names without editing them, track the actual saved lineage and use cached textures with normal label alpha, scaling and fog visibility. Twelve icons support recruited/evolved oath bearers; they do not add Lunar, Eclipse or Celestial to the faction roster. This release adds no pawn skill-menu commands.

RimWorld 1.6, Core and Harmony are required. Melee Animation is optional. The faction, backstories and encounters do not require a DLC.

## Elemental fortress identities

| Stronghold | Commander |
| --- | --- |
| The Cinder Throne | Azure Herald |
| The Scarlet Spire | Storm Herald |
| The Winter Bastion | Frost Herald |
| The Highwind Citadel | Gale Herald |
| The Adamant Seat | Adamant Herald |

Select a stronghold on the world map, inspect its elite warning and send a prepared caravan. Use the native **Attack** action. Newly generated maps use a 59×59 granite enclosure with a double-depth three-door front gate, rear postern, four corner watch posts and two front gatehouses. Lineage-colored runners and heraldic floor inlays lead through courtyard barricades to a furnished, structurally supported command hall. The barracks has six beds and furniture; the infirmary has three medical beds; the armory has shelving and a machining station; the treasury has a second internal door and vestibule. Generated Court standards and warm, fuel-free braziers decorate the rooms and approaches. The nine-person initial garrison is **one Herald, two Knights and six Oathlings**, using native defensive lords and the existing automatic skills. The Herald garrison is present even before late-game raid eligibility; attacking is voluntary.

A fresh treasury starts with **2,400 silver, 120 gold, eight advanced components, twenty industrial medicine, thirty survival meals, two doses of the matching normal elemental serum, twelve matching essences and one Herald crucible sigil**. Existing treasuries are not refilled on upgrade. These are physical items to collect, not a repeating completion reward. Unclaimed provisions are held when the map unloads and restored on returning; taken/destroyed items do not regenerate. Surviving defenders keep their identities across retreat and saving/loading. All Court forts share one architecture; the commander, elemental garrison, serum and biome distinguish them.

Defeating the defenders uses native settlement defeat and caravan exit rules. A cleared stronghold is never recreated by automatic loading or the settings button. Its pending expedition is cancelled and it can no longer supply new Herald dispatches. Other surviving Court fortresses can still supply ordinary raids and eligible Herald dispatches. Strongholds are persistent world settlements, not expiring quest sites.


## Faceguard and stronghold detail update

The side faceguard is fitted against native head bounds, retaining the original south/north views, native west mirroring, mouth-only coverage and beard-only hiding. It does not change armor stats or redemption behavior.

Loaded older stronghold maps receive a versioned, additive furnishing/floor upgrade once. It skips occupied furniture footprints, loose loot, pawns, player construction and painted terrain; it does not rebuild the perimeter or interior walls. Unvisited or regenerated stronghold maps receive the full new layout. The existing nine defenders, finite treasury, expedition rules and saved identities remain unchanged.

See [Court detail and native verification](CourtDetail.md) for the updated screenshots and test scope.


## Crucibles, ultimatums and elemental spoils

**The Severed Crucible:** after day 30, a choice offers twelve matching essences and one Herald sigil. Securing it schedules one explicitly warned, ordinary 1,200–3,600-point recovery force for one day later. Refusal/expiry yields no reward and no raid. The twenty-day cooldown begins when the offer is made.

**The Herald's Ultimatum:** after day 60 and 8,000 points, a commander announces a two-day assault from a surviving base. Pay six matching essences and 600 silver from visible, unforbidden on-map supplies to cancel that exact force. Refusal or timeout leaves the warned assault in place. Payment does not make the faction friendly. Attacking/clearing its origin recalls it. Cooldown: thirty days.

Hostile Court deaths drop matching essence: Oathling 1–2, Knight 3–4, Herald 6–8 plus one sigil. A saved per-pawn claim prevents repeats after resurrection and reload. Recruited/redeemed pawns are excluded. No butchering or corpse processing is needed. Deaths off a loaded map do not create remote item spawns.

At a drug lab after Fabrication with Crafting 8, eight matching essences plus two industrial medicine and one industrial component produce the corresponding ordinary serum. One sigil can instead be refined into four essences of any chosen ordinary element. The ten recipes supplement existing recipes; they do not create Celestial, Lunar or evolution serums.

See [campaign update and verification](CourtEnemyUpdate.md).


## Battle training, formations and body compatibility

New Oathlings have Melee 8–12, Knights 14–18 and Heralds 18–20. Court squads stand their ground. With **Enable Court battle tactics and formations** enabled, bounded local thinking favors armed/Ascendant threats, supports a nearby commander's target without unlimited piling, and restores melee pursuit without interrupting techniques, paired rituals or external animations. No new damage packets are added.

Four nearby hostile Court warriors within ten clear cells can maintain one temporary Court battle formation while a visible enemy is present. It grants 12% damage resistance and 8% faster movement. The condition never stacks. Breaking the group removes it on reevaluation; captivity, downing, recruitment or disabling tactics removes it promptly. It expires naturally if not maintained. Recruitment immediately removes the enemy condition.

Thinking caches at most 32 warriors and 64 potential targets, scans at most 512 local pawns, and processes four warriors every twenty ticks; the cache refreshes every 120 ticks. Disabling the setting stops the map thinker. No per-tick world scan or faction-wide global target search is added.

Court clothing maps unsupported custom body types to the nearest standard fitted sprite, including FemaleBB → Female. This applies to both conventional disciple uniforms and manifested armor, retaining native directional graphics and west mirroring without changing the pawn's actual body. Court afterimages use the same body mapping.
