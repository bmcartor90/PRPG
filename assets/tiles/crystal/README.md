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

# Solid wall set

`source/Meshy_AI_crystal_cavern_solid_wall_tileset.png`: 1536 x 864, opaque dark grey background, 6 pieces: plain rock block, rock block with crystals, rock block braced with mine timbers, a ceiling underside with stalactites, a base strip with rubble and crystals, a narrow column.

- **Usable as fill and cliff face.** Each block has a dark outline. Trimmed by about 10 pixels it tiles side to side with no visible seam; top to bottom leaves a faint light line, which a short blend at the seam removes.
- Same resolution problem as the platforms: a block is about 440 pixels wide, so its scale against the characters has to be chosen rather than read off.
- Still missing: a walkable top edge (the surface you stand on), and inside and outside corners.

# Ground edge set

`source/Meshy_AI_crystal_ground_edge_tileset.png`: 1536 x 864, transparent, clean alpha. 9 pieces: top surface (plain and with crystals), two outside corners, two cliff-face columns, two inside corners, an underside with stalactites.

- **Keep.** The top surface tiles left to right with only a faint join. The cliff columns need their joins blended to stack.
- Brighter and more saturated than the wall set; toned down when cut.
- The outside corners carry an overhang on their open side, and the cliff columns are outlined on both sides, so each is cut to the part that is needed.
- In the lab (`tools/sprites/lab_art.py`): the top surface becomes `edge_top.png`, drawn along every piece of solid ground and sheared to follow slopes, with its row 3 on the walking line; a cliff column becomes `edge_face.png`, drawn down every cliff face. Corners and the underside are not used yet.

# Climbable cliff edges

`source/Meshy_AI_climbable_cliff_edge_tileset.png`: 1536 x 864, transparent, clean alpha. Four climbables on a vertical cliff face seen side-on, rock on the left and open air on the right (mirror for the other side). Each has a top piece (row 1) finishing it at the cliff top and a repeating strip (row 2): ladder bolted to the rock, rope pinned with pitons under a ring anchor, very rough rock (big gilded ledges, easy), slightly rough rock (a thin gilded crack tracing the route, hard).

- **Keep.** Rock matches the edge set; the gold-leaf cue reads clearly without shouting.
- Stacked, the ladder and rope repeat cleanly. The very rough strip's ledges line up well enough. The slightly rough strip's gold line does not meet itself at the join; it needs a hand fix where the strips meet.
- Strip widths differ (about 140 to 220 pixels), so each is pinned to the cliff by its rock edge, not centred.
- Not in the lab: climbing a cliff face is not in the simulation yet.

# Climbable back walls

`source/Meshy_AI_climbable_backwall_tileset.png`: 1536 x 864, transparent, clean alpha. Two climbable rock walls seen face-on, each as a panel, a top edge, a left edge and a right edge: very rough (big gilded ledges) in row 1, slightly rough (gilded cracks tracing a route) in row 2.

- **Keep.** Both panels repeat both ways with soft joins once the outline is trimmed. The two tiers are easy to tell apart.
- The gold is on the orange side of gold. Classy enough at game scale; noted in case it reads loud.
- The left and right edge pieces have a dark vertical groove painted down them, which looks like a seam. Use the panels and top edges; ignore the side edges or trim the groove.
- In the lab, the slightly rough panel (`wall_slightly_rough.png`) replaces the line-drawn brick wall. The very rough panel is cut (`wall_very_rough.png`) but nothing uses it yet.
