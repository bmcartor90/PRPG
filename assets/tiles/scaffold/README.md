# Miner's scaffold

Wooden scaffolding for every surface the player can jump up through, plus the ladder and rope. Anything solid is crystal ground instead, so the two read differently at a glance.

`source/Meshy_AI_miner_scaffold_tileset.png`: 1536 x 864, transparent background, 10 pieces. `scaffold_pieces.json` gives each piece's position.

## Pieces

| Piece | Use |
|---|---|
| `deckLeft`, `deckMid`, `deckRight` | The walkway. Their surfaces and undersides line up exactly, so they join with no step. Repeat `deckMid` for length. The end pieces carry a post stub and a brace. |
| `postTop`, `postMid`, `postFoot` | Supports. Repeat `postMid` for height; `postFoot` rests on the ground. |
| `brace` | A loose diagonal, for decoration. |
| `ladder` | One ladder section. Stack for height; overlap sections slightly so the rail caps hide. |
| `rope`, `ropeTop` | A rope tied to a beam. Neither is a clean repeating section: for length, repeat a slice from the middle of `rope` and finish with its knotted end. |

## Verdict

Usable as delivered. Flat-on view, consistent scale, and close to Lucca's resolution (about 150 pixels a tile against his 160).

- `deckMid` has rope lashings at both ends, so a long walkway shows a lashing every 3.5 tiles. It reads as joined sections, which suits scaffolding.
- There is no climbable wall surface yet.

`docs/mockups/scaffold-mockup-1080p.png` shows it assembled with Lucca.
