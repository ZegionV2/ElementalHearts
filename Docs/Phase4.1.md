# Phase 4.1 — combat compatibility and presentation fixes

Historical Phase 4.1 notes: Phase 4.2 supersedes the individual ability permissions and opt-in Heaven described below. See Phase4.2.md.

This includes all earlier phases. Replace the old ElementalHearts folder while RimWorld is closed; keep one installed copy. Package ID, Def names and saved cooldown fields are unchanged. Requires RimWorld 1.6 and Harmony; Search and Destroy is optional. The installed mod and normal saves were not overwritten by this delivery.

## Search and Destroy

Tested against the supplied Search and Destroy (Continued), package `MemeGoddess.SearchAndDestroy`, using its actual 1.6 assembly. Load it before Elemental Hearts. Enable both Search and Destroy and Lightning Heart's **Auto combat** on the drafted pawn. Individual ability permissions still apply; automatic Heaven remains opt-in.

The optional bridge recognizes non-player-forced approach jobs created by Search and Destroy while that pawn's Search and Destroy toggle is enabled. It assists that mod's chosen target. Existing melee attack jobs also allow assistance. Ordinary manual movement remains protected, and queued orders suppress automatic casts.

Sever, Flash and Dance keep the exact current job, driver, player-forced status and queue. Flash restores the prior path after teleporting. Heaven is a separate interruptible channel job; automatic channels are not marked player-forced. Search and Destroy resumes ordinary decision-making when it ends. No Search and Destroy methods are replaced or patched, and no assembly dependency is introduced.

Native API verification confirmed that `StopAll()` clears queued jobs and `TryTakeOrderedJob()` marks a job player-forced. Instant casts no longer use either. Search and Destroy itself has a DetermineNextJob patch that selects the last queued order and clears the queue; this release does not change that independent behavior. Preserving the queue during an instant cast does not override what S&D later does when choosing its next job.

## Requested fixes

1. Ordinary melee hits extend combat timing without restarting the activation animation. Dodge counters likewise mark combat only. Successful active-ability cooldown start triggers the 30-tick activation state.
2. Drafting alone leaves the aura idle. Recent outgoing melee, successful counters or received damage keeps combat appearance for 180 ticks.
3. Manual Dance with no eligible targets is rejected with a message and spends no cooldown.
4. Instant abilities preserve ongoing movement and queued orders; Flash recalculates the existing path from its landing cell.
5. No replacement forced melee order is issued after an automatic cast. The existing AI job continues.
6. Heaven refunds its cooldown when its uncommitted channel fails because the target died or went down. Player interruption, an invalid path, lost range or other failure keeps it spent. A committed impact keeps its cooldown, even if it kills the target. The commitment flag is saved.
7. Removing the implant restores the stored prior hairstyle if the pawn still has Lightning hair. External hairstyle changes are respected. The hair toggle is available on selected player pawns while undrafted too.
8. Only active-ability strikes apply the mechanoid EMP rider. Passive melee electricity, chain bounces and counters retain electrical damage without EMP stun.
9. Charge ownership remains exclusive: a different hero resets the mark to one stack, including when the original owner takes it back. Another hero cannot consume that owner's mark.

## Appearance

Directional hair artwork is reduced from 190×190 to 156×156 within the same 256×256 texture canvas, retaining the same head anchor. This is approximately an 18% reduction. Original artwork is unchanged. Native hair-color tinting remains supported.

The uniform and sword now use their original artwork colors regardless of dye or metallic stuff. Both world graphics and worn-apparel color reads are covered. Other equipment is unaffected. Material stats, costs and crafting remain unchanged.

## Verification and limits

Native runtime behavior checks cover all nine reported issues plus S&D approach assistance, manual movement protection, queued-order priority, active versus passive EMP, and dyed uniform/gold sword colors. See Phase41RuntimeChecks.txt for the executed assertions. Tests use an isolated game profile and disposable fixtures; they do not modify the user's colony.

The source builds against the installed RimWorld 1.6 assemblies. XML/reference checks pass. A native camera preview checks smaller directional hair and dyed apparel on five adult body types. This does not certify every hat, alien head, animation overhaul or full mod list. Combat Extended and large-raid FPS remain untested.
