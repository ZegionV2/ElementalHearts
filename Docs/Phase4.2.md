# Phase 4.2 — full automatic melee rotation

Auto combat now has one on/off button. Enabling it no longer adds Auto Sever, Auto Flash, Auto Dance or Auto Heaven buttons. All four original manual ability commands remain available.

The previous logic required two aligned enemies for Sever, several nearby enemies for Dance, and separate permission plus a large healthy target for Heaven. A duel could therefore produce Flash followed by ordinary melee only.

New priority, one ability per decision:
1. Dance when surrounded (the existing crowd threshold still applies to this priority).
2. Flash to close a gap greater than three tiles.
3. Sever against the current enemy, including one adjacent enemy.
4. Dance against a single nearby enemy when Sever is unavailable.
5. Heaven against the current valid enemy when the preceding abilities cannot be used.

Cooldowns, casting conditions and ordinary melee remain in effect. Automatic Heaven is now included in Auto combat. Old per-ability save values are retained for save compatibility but no longer gate casting, so existing saves need no reset. The existing Auto combat on/off state is preserved.

Search and Destroy approach assistance, non-forced AI jobs, manual-movement protection and queued-order priority remain. Drafted pawns holding position can use the melee rotation against an adjacent hostile, or defensive crowd Dance. They do not choose a distant pursuit target. All Phase 4.1 fixes remain included.

Close RimWorld and replace the previous ElementalHearts folder with this package. Keep only one installed copy. Enable Auto combat on your implanted pawn; enable Search and Destroy too if you want its autonomous pursuit.

See Phase42RuntimeChecks.txt for executed native-runtime checks. Earlier phase reports are historical and do not describe the new ability priority. Full mod-list compatibility and raid performance are not certified.
