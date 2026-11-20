# Doors and openings

Sheets as received, in `source/`. Not cut or used yet. The design treats a door as a pair: press up at one to arrive at its partner (`docs/design.md`, "Map size, doors and edges").

| File | Pieces | Background | Verdict |
|---|---|---|---|
| `Meshy_AI_environment_doors_and_openings_sheet.png` | Stone arch with wooden door: closed, ajar, wide open; iron portcullis. Crystal cave mouth, timbered mine entrance, narrow rock crack. | Opaque dark grey (flood fill cuts it) | Keep. The four arches match each other in size (about 290 x 340). The ajar and open frames swing the door in different directions, so they are two states, not two frames of one swing. |
| `Meshy_AI_locked_doors_and_dormant_portal_sheet.png` | Chained and padlocked door, rune-sealed door, iron vault door, empty rune arch. | Opaque dark grey | Keep, with the mismatch below. |
| `Meshy_AI_shimmering_portal_loop_sheet.png` | Active portal, 8 frames of a swirl. | Transparent | Keep. Frames are consistent in size (about 330 x 362). |

## Mismatches

- The locked doors' arches are taller and narrower (about 365 x 475) than the plain doors' (290 x 340). A plain door cannot become a locked one in the same spot without its frame changing shape. The empty rune arch on the same sheet matches the locked doors, so it can serve as their opened state.
- The dormant (empty rune) arch does not match the active portal's arch: the portal is squatter, with three rune stones a side against two. Switching a portal on would visibly change its frame.

## Size in the game (agreed 2026-11-19)

Every doorway's opening is **2.4 tiles tall**, so a 1.9-tile character walks through with headroom. The door's width, the stretch you can enter from, is **1.25 tiles**. Each sheet is scaled to fit: the plain doors about 1.33 times (their opening is about 150 x 290 pixels), the locked doors about 1.1 times. At 160 pixels a tile, the plain door's whole arch is then about 2.4 x 2.8 tiles.
