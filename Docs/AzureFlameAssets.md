Current visual rollback: the dedicated evolution/transformation textures below are removed. The corrected side armor is retained and the earlier smooth apparel reveal is restored. See ThunderstormUpdate.md.

Current side art and effects: see AzureRegaliaFxUpdate.md. The seven east-facing Azure body textures now use the closed right-facing profile; west is mirrored. Front/back remain unchanged.

Current art supersedes the historical design below: see RegalAzureUpdate.md. Azure now wears compact ivory/white cloaked regalia with a low neckline, tucked shoulders, gold trim and blue accents. All 22 armor textures are replaced; the sword is unchanged. Only Azure uses this outfit.

Current status: these assets are now used by the Azure Flame evolution. See AzureFlameUpdate.md. The notes below describe the original standalone art delivery.

# Azure Flame — Royal Regalia asset pack

Design: a sleeveless royal cuirass and short tabard in cobalt blue, ivory and gold, with a cyan flame crest and sapphire clasp. Matching straight royal sword with a blue-white blade, flame inlay and gold guard. No helmet, crown, headgear, sleeves, arm sprites or hand sprites are included.

## Contents

- 21 transparent worn armor textures: south/front, east/side and north/back for Male, Female, Thin, Fat, Hulk, Child and Baby body sizes. RimWorld mirrors the east view for west-facing pawns.
- One transparent inventory armor icon.
- One transparent sword texture.
- Four original high-resolution generated sources, the exact prompts, and a preview sheet.

All game textures are 256 × 256 RGBA PNGs. Body-size versions are fitted derivatives of the three generated directional views, aligned against the installed Flame Uniform textures. The silhouette is narrower than that reference to avoid its shoulder/arm armor shapes.

## Texture paths for future integration

Worn graphic base: Things/Pawn/Humanlike/Apparel/AzureFlameRegalia/AzureFlameRegalia

Inventory icon: Things/Pawn/Humanlike/Apparel/AzureFlameRegalia/AzureFlameRegalia

Sword: Things/Item/Equipment/WeaponMelee/AzureFlameSword

The sword grip is prepared around pixel (128, 195). It still needs an in-game hand/animation fit check when the future Azure sword definition is added. Armor fit and facing also need live pawn inspection.

These assets are staged in Elemental Hearts' texture folders. They do not add an equipable item, replace current Flame gear or enable Azure evolution. Future Azure transformation logic must explicitly omit the helmet instead of using the current three-piece uniform/helmet/sword requirement.

## Generation

Created using the built-in image_gen tool. Exact generation prompts are preserved in ArtSource/Prompts.json. The installed Flame Uniform and Flame Sword were references for style/alignment. The new front design was then used as the reference for side and back views. Final textures were resized/fitted and checked for dimensions, nonempty artwork and actual transparent corners. No gameplay code or damage values changed for this asset request.
