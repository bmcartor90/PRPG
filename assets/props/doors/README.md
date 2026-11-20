# Doors and openings

Sheets as received, in `source/`. Not cut or used yet. The design treats a door as a pair: press up at one to arrive at its partner (`docs/design.md`, "Map size, doors and edges").

| File | Pieces | Background | Verdict |
|---|---|---|---|
| `Meshy_AI_environment_doors_and_openings_sheet.png` | Stone arch with wooden door: closed, ajar, wide open; iron portcullis. Crystal cave mouth, timbered mine entrance, narrow rock crack. | Opaque dark grey (flood fill cuts it) | Keep. The four arches match each other in size (about 290 x 340). The ajar and open frames swing the door in different directions, so they are two states, not two frames of one swing. |
| `Meshy_AI_locked_doors_and_dormant_portal_sheet.png` | Chained and padlocked door, rune-sealed door, iron vault door, empty rune arch. | Opaque dark grey | Keep, with the mismatch below. |
| `Meshy_AI_shimmering_portal_loop_sheet.png` | Active portal, 8 frames of a swirl. | Transparent | Keep. Frames are consistent in size (about 330 x 362). |
| `Meshy_AI_stone_arch_door_conditions_sheet_2.png` | The mundane door, second conditions sheet: portcullis down, portcullis half raised, door half open, gold keyhole, overgrown, iced, cobwebbed, empty arch. | Opaque mid grey (flood fill cuts it) | Keep. Every arch is 292 x 338 to within a few pixels, the stones are laid out the same, and it matches the plain doors on the first sheet. Frame 3 was asked for fully open and came back half open. Ivy and ice spill slightly past the arch on frames 5 and 6. |
| `Meshy_AI_rune_archway_portal_states_sheet.png` | The mystical door (rune portal), states sheet: dormant, waking, opening, active, sealed (red), unstable, closing, broken. | Opaque mid grey | Keep. Every arch is about 340 x 377, with the diamond keystone and three rune stones a side in the same places. Broken spills rubble past the arch. This replaces the earlier portal loop and dormant arch, which did not match each other; its active-loop sheet should use this sheet as the reference. |
| `Meshy_AI_active_portal_loop_animation_sheet.png` | The rune portal's active loop, 8 frames. | Transparent | Arch keep: every frame is about 338 x 376 and matches the states sheet. Swirl not usable as a loop: measured frame to frame it turns +18, +12, -3, -3, -3, +6, -18, -12 degrees, so it would jitter back and forth instead of turning. |

## Mismatches (in the earlier sheets, superseded by the conditions sheets above)

- The locked doors' arches are taller and narrower (about 365 x 475) than the plain doors' (290 x 340). A plain door cannot become a locked one in the same spot without its frame changing shape. The empty rune arch on the same sheet matches the locked doors, so it can serve as their opened state.
- The dormant (empty rune) arch does not match the active portal's arch: the portal is squatter, with three rune stones a side against two. Switching a portal on would visibly change its frame.

## Size in the game (agreed 2026-11-19)

Every doorway's opening is **2.4 tiles tall**, so a 1.9-tile character walks through with headroom. The door's width, the stretch you can enter from, is **1.25 tiles**. Each sheet is scaled to fit: the plain doors about 1.33 times (their opening is about 150 x 290 pixels), the locked doors about 1.1 times. At 160 pixels a tile, the plain door's whole arch is then about 2.4 x 2.8 tiles.
