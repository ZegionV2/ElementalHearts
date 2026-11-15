# Renown: the stories people tell

**Status: design proposal only. No renown, fear rolls or social bonuses are implemented by this naming update.**

The progression is **Disciple → Adept → Ascendant → Incarnate**. Reaching Ascendant should make a pawn a person others remember. Reaching Incarnate should make them the subject of legends. Celestial Ascendant uses the Incarnate reaction category because of its established power.

## Core idea

Build reputation from **witnessed deeds**, then let other people spread those stories. A powerful pawn can inspire allies, frighten enemies, attract challengers or become known for mercy. A new title alone does not give every pawn in the world instant knowledge of them.

Keep three things separate:

| Record | Meaning |
| --- | --- |
| Renown, 0–100 | How noteworthy the pawn's achievements are. Does not increase combat damage. |
| Knowledge | Which witnesses and factions have actually heard of this person. |
| Reputation | What the surviving stories suggest: protector, merciful duelist, ruthless conqueror, etc. These are earned descriptions rather than compulsory traits. |

Suggested bands: **Unknown** 0–19, **Spoken Of** 20–39, **Renowned** 40–59, **Famed** 60–79, **Legendary** 80–100. Numbers below are initial tuning proposals.

## Earning a story

| Feat | Proposed recognition |
| --- | --- |
| First evolution from Adept to Ascendant | +20 personal renown; witnesses can remember the ceremony. |
| First transcendence to Incarnate | +30; create a lineage-specific legend when witnessed. |
| Defeat a Court Herald or comparable stronger opponent | +5–8, credited once per defeated pawn. |
| Win a dangerous fight while outnumbered | +3–5 once for the encounter. |
| Rescue a downed ally during serious danger | +2–3; supports a protector reputation. |
| Spare a defeated named rival | A mercy story; only modest renown, with a long repeat limit. |

Adepts can earn a few early battle stories, but Ascendant and Incarnate milestones unlock the stronger reactions. Existing Ascendants/Incarnates receive a one-time stage baseline when the feature is added; do not invent past victories, witnesses or moral conduct.

Routine attacks, farming weak animals, beating prisoners, repeated knockdowns of the same enemy and repeatedly switching transformation do not generate feats. Cap ordinary battle gains per encounter and per day. Grant each milestone once per pawn, including after resurrection or an interrupted ritual.

## Witnesses and spreading rumors

At a completed milestone or qualifying encounter, search a bounded nearby set of awake, conscious humanlike witnesses with appropriate sight and faction relationships. Record actual witnesses rather than scanning every pawn each tick. A witness interrupted or downed before the decisive event does not receive knowledge of its outcome.

Witnesses can share a short recollection during normal social interactions. Visitors, traders, rescued prisoners and surviving raiders can gradually carry stories to their factions. Use a saved cooldown per tale/faction so one caravan does not duplicate a rumor dozens of times. Knowledge should survive travel, recruitment and saving/loading; it should not depend on a spawned map.

Show the storyteller and whether a claim was seen or heard:

- **Eyewitness:** “I saw Aiko's blade part the storm. The Herald fell before the thunder reached us.”
- **Rumor:** “The traders say Aiko has become an Incarnate. I don't know how much of it is true.”
- **Protector:** “Ren crossed the courtyard to pull Mara out. The Court couldn't get past him.”
- **Mercy:** “She could have finished me. Instead she put away her sword.”

Generate text from curated element/feat templates using the recorded pawn and event names. Templates add flavor without inventing unsupported casualties or victories. Water/Aurora stories mention northern lights, frost and impossible sword routes; Thunder stories mention the delayed boom; Hell Flame stories mention black fire; Tempest stories mention the quiet center of the storm.

## Awe, confidence and fear

Reactions depend on what a pawn knows, what they can currently see, their relationship and the difference in power. A known legend arriving transformed can trigger recognition. An unknown Incarnate revealing a grand technique can cause immediate awe and create a first eyewitness story.

| Observer | Proposed response |
| --- | --- |
| Allied ordinary human or disciple | Brief reassurance or awe; occasional favorable social memory. |
| Allied Adept | Respect for an Ascendant; aspiration or a wish to train. |
| Ordinary hostile who recognizes a stronger enemy | A chance of hesitation or a short retreat to nearby cover. |
| Hostile disciple/Adept facing an Incarnate | Higher fear chance, reduced by courage, discipline and allied leadership. |
| Ascendant facing a peer | Rivalry/respect; greatly reduced fear. |
| Incarnate or Celestial facing a peer | Recognition and challenge; no routine fear disruption. |

Do not make every bystander afraid of a friendly hero. Friendly fear should require witnessed cruelty, friendly fire or an established threatening reputation. A merciful legend may intimidate enemies while reassuring colonists. Renown is not automatically goodness or evil.

Nervous/Cowardly-like traits can increase fear, steadfast/Brave-like traits can reduce it, and loyal Court leaders can steady their escorts. Resolve optional ISEKAI traits only when their definitions are present; never require that mod. Do not assume a Psychopath or Bloodlust pawn is automatically immune to self-preservation.

For the first release, fear should mean a **short hesitation or safe retreat**, not instant surrender, capture or an entire raid fleeing. A successful fear check might cause roughly 0.5–1 second of hesitation or a 1–2 second movement to valid nearby cover. Starting chances could be 10–20% against a recognized stronger Ascendant and 20–35% against an Incarnate, with a strict per-observer recovery of at least one in-game hour. These values need live combat tuning.

Never interrupt a committed elemental skill, Melee Animation sequence, flight/loading, teaching, medical rescue, prisoner handling or another controlled action. Skip the disruptive response if no safe retreat exists. Respect existing fear/mental-state immunities. No recursion through damage/parry hooks and no extra damage packets. Mechanoids and insects do not hear human rumors or use these social fear reactions.

## How the player sees it

Add a **Renown** section to the Elemental Codex with the band, known epithets, recent feats and a small list of “Witnessed by…” / “Heard by…” entries. Hovering a known pawn can show a restrained recognition line. Keep renown off the combat skill grid so the existing ability menu stays readable.

Examples of earned epithets: **Courtbreaker** for defeating a Herald, **Shelter in the Storm** for rescues, **Merciful Blade** for repeatedly sparing dangerous rivals. Element-specific titles such as **The Winter Crown** should require actual feats after transcendence, not merely wearing an outfit. Titles should be optional display text, never overwrite the pawn's real name.

Use occasional social-log remarks or short floating dialogue, with one shared map chatter budget and a per-speaker cooldown. Reserve letters for major first-time milestones. Avoid a popup or speech line every time a skill fires.

## Recommended first release

1. Saved stage baselines, five renown bands and a bounded feat ledger.
2. Witnessed ascension/transcendence, Herald victories and dangerous rescues.
3. Curated witness/rumor dialogue and the Codex Renown panel.
4. Ally reassurance and limited enemy hesitation/retreat, with settings to disable reactions or chatter separately.
5. Basic rumor transfer when a knowledgeable visitor or survivor returns to their faction.

Keep challengers, bounties, worship, formal rivals and named legendary encounters for a later release. Renown should first make existing fights and colony relationships feel remembered.

## Implementation and verification boundaries

Proposed persistence: stable pawn references/IDs, milestone flags, renown value, at most 12 recent feats per notable pawn, a small bounded witness list per feat, aggregated faction knowledge and observer recovery timestamps. Truncate old detail while keeping the aggregate reputation. Never keep growing per-hit histories or references to every victim forever.

Trigger updates from successful progression commits, credited death/encounter outcomes, rescues and normal social interactions. Deduplicate paired/area kills and shared-team attacks. Cache stage classification and use bounded local witness/cover searches. No map-wide scan for every slash and no extra effect spawns per observer.

Verify old-save initialization, mid-ceremony loading, death/resurrection, faction changes, visitors leaving, off-map/caravan pawns, duplicate kill credit, unwitnessed feats, repeated rumor delivery and cleanup of discarded pawns. Combat tests must establish that fear cannot chain-lock targets, cancel skills, strand haulers or override launch sites. Test dialogue with missing/dead participants and optional mods absent. Compare combat with reactions disabled to make sure the reputation feature adds no hidden stat boosts.
