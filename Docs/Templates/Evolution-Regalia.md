# Future evolution outfit template

Use Evolution-Regalia-Master.png as the common visual base for all future elemental evolutions. Keep the compact low-neck torso, tucked shoulders, ivory/white short cloak, gold trim and royal fastenings. Change the elemental motifs, streaks and gems to each evolution's color; generate matching front/side/back views without limbs, long collars or helmets. Preserve the RimWorld body alignment of the existing Azure texture variants. Ordinary ascendants retain their current uniforms. Only Azure Flame is active now.

Front alignment note: current Azure regalia uses native apparel drawData/dataSouth offset (+0.0645, 0, 0), with scaleOffsetByBodySize=true, to align the neckline with the head. Carry this correction when reusing the identical template layout; inspect new art before choosing an offset. Other facings are not offset.

Side template: Evolution-Regalia-East-Master.png is now the canonical east-facing side view for future evolutions. Use a single shoulder, front clasp line at the right edge and cloak down the left/back. Keep the ivory undercoat opaque under the shoulder and through the torso. Fit to each body type; west is the native mirror of east. Do not reuse the earlier three-quarter Azure side art.

Current side-fit update: Azure Flame, Thunderstorm, Glacial and Cyclone east/west wear poses are corrected by Source/RegalSideFit.cs for all seven body textures, with native body-width scaling and mirrored west offsets. Its table is measured against the current texture layout; preserve that canvas layout when reusing this art, or remeasure the new family. See Docs/RegalSideFit.md. The earlier front-offset note above records an older template revision; current shipped front/back poses are retained by the side-only correction.
