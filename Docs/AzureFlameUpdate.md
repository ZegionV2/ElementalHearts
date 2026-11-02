Azure Reversal adds a fifth independent ability alongside the four slots below; see AzureReversalUpdate.md.

Current outfit/lineage rules: see RegalAzureUpdate.md.

Latest power/teaching changes: see AzurePowerTeachingUpdate.md. Azure mood is +50 total, combined movement is 500%, and defenses are strengthened.

# Azure Flame evolution — installed update

## Evolve

Select a Flame Ascendant and use **Evolve: Azure Flame**. It unlocks after **150 additional hostile non-animal kills** and **ten in-game days as a Flame Ascendant**. Each target counts once, including after resurrection. Disciple kills are separate. Existing ascendant time counts; mastery kill tracking begins with this update because older versions did not record it.

The button starts a 4.5-second ritual with a progress bar and evolution effects. The head halo requires a completed evolution, transformation and drafting. Movement orders or incapacitation can interrupt it without spending a resource. Other Flame abilities and transformation cannot run during the meditation. DEV mode includes **DEV: Evolution-Ready** for testing; use the normal Evolve button afterward.

Evolution replaces the Flame condition with the permanent standalone **Azure Flame Ascendant** hediff. Existing dual-condition saves are consolidated on load. It grants **+10% melee damage**, **500% combined elemental movement speed**, and **+10 percentage points of Manipulation**. It preserves the pawn, the original ability component, current cooldowns, auto-combat preference and non-lethal setting. It does not add a second elemental heart.

## Azure abilities

The four existing Flame ability slots switch to:

- **Azure Crescent** — focused blade wave, improved armor penetration, up to three azure brands. Base strike budget matches the ordinary wave.
- **Blue Comet** — six-tile wall-respecting blink-strike, 80% of the ordinary blink's base damage and a one-second slow on a surviving hit.
- **Controlled Pyre** — a six-cell-radius contained pyre at the caster's position. Three pulses over three seconds share one damage budget. The zone stays where it was cast and does not ignite buildings or terrain. Pending pulses are saved.
- **Azure Phoenix** — stronger focused finisher and small burst, with 40% longer cooldown and an interruptible telegraph. The warning animation uses only its pre-impact frames. Consumes at most three brands.

The recent global 20% melee/elemental damage reduction still applies. Area hits, passives, chains and counters retain existing friendly-fire and non-lethal checks. The new pyre also uses those checks on every pulse. Ordinary Flame Ascendants and disciples retain their existing kit.

## Halo and manifestation

Azure Ascendants use an animated **blue flaming halo floating above the head**, replacing their former body aura. It appears only while transformed and drafted, awake and able to stand; it hides during transformation, sleep, downing or death. The effects-opacity and aura/halo setting control it. Rendering is camera/fog culled and uses cached materials; cosmetic effect slots are bounded independently of saved gameplay effects.

Transformation uses the compact white-cloaked, low-neck Azure armor and bound sovereign sword. **No Azure helmet, crown, sleeves or other headgear is created.** Existing hair remains visible. If already transformed when evolving, the projected kit switches to Azure gear; original personal clothes remain stored for reversion. Projected armor/sword quality carries over. Elemental gear is now indestructible, and downing/death restores personal clothes. See AzureReversalUpdate.md. Sword downing, death, recovery and revival behavior remains bound to the owner.

## Assets

Five generated icons and five six-frame animation sequences are included: crescent, comet trail, controlled pyre, phoenix impact and head halo. Created with the built-in image_gen tool; exact prompts and high-resolution sources are in the separate art-source download. The previous 23 royal armor/sword textures are now used by gameplay. An optional Melee Animation profile for the Azure sword is included, with no new dependency.

## Validation and remaining live checks

- Build against installed RimWorld/Harmony files passed.
- All 13 existing verification scripts passed.
- Azure tests passed for both evolution milestones, eligible/unique kills, preserved cooldown identity and automation, ordinary Flame isolation, helmet-free gear availability, pyre pulse budget, telegraph frame selection, sword recovery and prior mechanics.
- Native Scribe **save output** was checked for pending pyre timing/pulses and mastery victim IDs. Native loading/full game save-reload was not verified in the headless runtime.
- XML uniqueness and asset paths passed; five icons and 30 effect frames are 256px PNGs with transparent or near-zero-alpha corners.
- The optional Azure sword profile deserialized with the installed Melee Animation assembly and passed its grip/blade metadata checks.

**Live gameplay, the actual evolution sequence, halo placement, pawn/gear fit, Harmony startup and full save/reload still require an in-game check.** The animated HTML preview shows the actual generated frames, not gameplay footage.

Fully restart RimWorld to load the new assembly. The prior mod files are backed up. Keep this update enabled for saves containing Azure evolution and pending Azure effects.
