# Crystal platforms

`source/Meshy_AI_crystal_platform_tileset.png` is the sheet as received: 1536 x 864, opaque dark grey background, 25 pieces.

## What is on it

- Floating platforms 1, 2 and 3 segments wide, with hanging crystals underneath; the same again with crystal clusters on top.
- Solid blocks 1, 2 and 4 segments wide; a tall block; a stepped block with a dip in the middle.
- Thin floating platforms 1, 2 and 3 segments wide.
- Three ramps, all rising to the right, at roughly 33, 27 and 25 degrees.
- Two corner pieces, two small pillars, one tall pillar, two crystal clusters.

## Not yet usable as a tileset

- **These are finished chunks, not tiles.** Segments are not a fixed width (a 1-segment block is 128 wide, 2 is 220, 4 is 364), so pieces cannot be joined edge to edge to make a platform of any length. That needs separate left-end, middle and right-end pieces with a repeating middle.
- **Resolution is lower than the character.** A segment is about 100 pixels; Lucca's art is 160 pixels a tile. Shown one segment to a tile, the platforms are about 1.6 times softer than he is.
- **The ramps do not match the game's slopes.** The lab uses 27, 45 and 63 degrees (1:2, 1:1, 2:1).
- **The top face is drawn with depth.** A rule is needed for where on that face his feet go. The mock-up uses 12 pixels below the back edge at 1080p.
- The background needs cutting out before use. A flood fill from the border works.

`docs/mockups/crystal-cavern-mockup-1080p.png` shows these pieces with Lucca and the cavern background at game scale.
