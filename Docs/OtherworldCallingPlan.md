# Otherworld Calling

Implemented for Elemental Hearts on 2026-11-07. This Celestial utility opens an ivory/gold doorway and recruits one permanent human colonist from another world.

## Use

Select a conscious, player-controlled Celestial on a map, press **Otherworld Calling**, then choose a visible safe cell. The caster stops and faces the doorway for three seconds. The traveler joins as a normal controllable colonist with strong ISEKAI traits. Grant elemental powers separately through the existing **Bestow** menu.

- **Price:** exactly **1,000 silver** per completed call.
- **Age:** **18–24** biological and chronological years, inclusive.
- **Bodies:** Male, Female or Thin only. Calling never generates Fat or Hulk recruits.
- **Men:** fitted boys' Japanese gakuran, with dark jacket and integrated trouser panel.
- **Women:** fitted schoolgirl sailor uniform, with ivory blouse and opaque navy pleated lower panel.
- **Traits:** one identity — Summoned Hero (60%), Protagonist (25%) or Regressor (15%) — plus two distinct compatible talents. At least one talent has an immediate combat/colony benefit. Native trait drawbacks remain intact.
- **Powers:** no Elemental Hearts lineage, disciple training, bound sword or evolution readiness until Bestow.
- **Cooldown:** one in-game hour (2,500 ticks) by default. Mod settings allow 0–24 in-game hours; zero removes the cooldown while keeping the silver price.
- **Control:** manual only; drafted/undrafted and transformed/untransformed are supported. Auto Combat never calls colonists.

ISEKAI RPG LEVELING (`JellyCreative.IsekaiLeveling`) is required for Calling's promised traits. Missing support disables only this command, with an explanation; the rest of Elemental Hearts stays usable. No Milira, VEF or DLC dependency is added. With Biotech active, called humans are Baseliner.

## Clothing fit

The outfits now use a narrow two-pixel fitting margin instead of the earlier 15–18-pixel padding. An outfit-specific head-origin adjustment raises the visible head above the low collar while keeping hair aligned. All native views retain opaque body coverage, including mirrored west. Both designs have flat chests. Only the matching gender can wear each uniform.

Clothing mechanically covers torso, shoulders, arms, waist and legs, with normal cloth durability and protection. It has no helmet and does not claim hand/foot protection. The arrival outfit is initially marked forced through the normal apparel policy so AI does not immediately exchange it; the player can clear forced apparel normally. Existing transformation storage/restoration handles it after Bestow.

Textures also include fitted Fat/Hulk/Child/Baby and FemaleBB fallback variants for compatibility. These do not expand the summon pool; generated recruits remain adults with the three allowed body types. No pawn's body type is changed by an apparel-rendering fallback.

## Payment, interruption and saved arrivals

Payment uses actual spawned, reachable, unfogged, unforbidden silver anywhere on the current map, including loose ground stacks outside storage and the home area. Inventory/caravan money and other maps are excluded. Currency is split into a saved native holder before channeling; simultaneous callers cannot reserve the same stack.

Canceling targeting does nothing. Interrupting before commitment returns reserved silver and spends no cooldown. At completion, a bounded nearby search rechecks the landing, prepares one person, fixes their identity/traits/uniform, consumes the 1,000 silver and sets cooldown once. Effects never create extra people or charge money.

If placement fails after commitment, that paid person stays in a saved holder and the caster is released. The **Otherworld arrival waiting** letter lets the player choose a safe cell on an open home map. A Celestial's Calling command also has a right-click recovery option. Recovery places the same person without rerolling or charging again. Eight pending calls/refunds are the maximum; records are removed after delivery or refund.

Saving/loading retains channel progress, real silver, fixed traits, pending person and cooldown. New keys are separate from existing combat, Bestow and Light Travel saves. Old saves initialize the feature ready. **DEV: Calling-Ready** appears only with God Mode on and clears only the cooldown; it does not waive cost or dependency checks.

## Presentation and performance

The command uses the generated doorway icon, eight opening frames, six hold/glint frames and eight closing frames, plus existing Celestial seals, beam, arrival ring and motes. Interrupted doorways close visually. Cached materials, a sixteen-slot closing-effect pool, existing bounded Celestial effects, camera/fog culling and a 128-cell local landing search keep presentation bounded. There are no damage packets, terrain changes, climate effects or ambient pawn scans.

## Validation

The implementation is built against the installed RimWorld 1.6 assemblies. Native isolated-save checks cover exact payment across fragmented stacks, shortages, cancellation/refund, concurrent reservations, mid-channel loading, completed arrivals, saved pending arrival/recovery, cooldown persistence, compatible ISEKAI bundles, age/body/gender rules and God Mode controls. The missing-ISEKAI path is checked separately. The matching outfit also passes through Bestow and Transform/Revert restoration. Native portraits and all-direction silhouette checks document the clothing fit.


## 2026-11-08 polish

Existing one-day cooldowns migrate once using the original cast time, so a call made more than an hour ago is already ready. The old saved cooldown/settings keys remain readable. The skill bar caches its funds preview for 0.3 seconds; casting always validates the current allowed stacks again. Drafted pawns use the player faction's actual forbidden flag. The girls' east texture has been repainted as a slimmer tucked profile and fitted directly to native body silhouettes; west mirrors it.

Celestial Bestow now presents six colored lineage cards, evolution artwork and descriptions, separate mastery/readiness choices and a fixed recipient button. The scrollable body supports shorter windows. Existing selected-element preferences, twelve choice encodings and paired Bestow mechanics are retained.
