# Nina sprites

Sheets for Nina, the second character, and the frames cut from them. She is playable in the feel lab (the Character menu).

All sheets are 1536 x 864 with transparent backgrounds and empty fists.

| File | Frames | Verdict |
|---|---|---|
| `Meshy_AI_Nina_Walk_and_Run_Sheet.png` | Stand and walk (6), run (6) | Keep. Her outfit differs slightly from the other sheets: a double belt and dark cuffs. |
| `Meshy_AI_Nina_Invisible_Combat_Sheet.png` | Sword draw (4), shield fetch (2), staff draw (4), guard (2) | Keep. A second copy arrived as `..._Sheet_1.png`; it is the same image and is not stored. |
| `Meshy_AI_Nina_Moving_Draws_Sheet.png` | Walking: sword draw (4), shield fetch (2). Running: the same six. | Keep. |
| `Meshy_AI_Nina_Moving_Draws_Inverted_Sheet.png` | The same twelve poses | Not needed. As with Lucca, the far arm was asked for and the near arm was drawn. |
| `Meshy_AI_Nina_Action_Acrobatics_Sheet.png` | Jump (7), crouch (2), slide (3), ladder climb from behind (4) | Keep. The ladder row is not used; the rope-and-wall sheet covers it. |
| `Meshy_AI_Nina_Ledge_and_Lunge_Sheet.png` | Hang (2), pull-up (4), stand, one-handed hang (2), lunge (2), face down, get up (2) | Keep, with the colour drift noted below. |
| `Meshy_AI_Nina_Rope_and_Wall_Sheet.png` | Rope climb and hang, side view (7); ladder and wall climb, from behind (7) | Keep. Frame 7 of the rope row has her hood down, unlike every other frame. |

## Size

She stands about 321 pixels tall on a sheet against Lucca's 307, most of it the hood. Scale her by her own standing height, not his: in the game she is 1.7 tiles tall and he is 1.9, so her art is drawn smaller than his even though it is larger on the sheet.

## The atlas

`atlas/nina_atlas.png` and `atlas/nina_atlas.json`, in the same format as Lucca's (see `../lucca/README.md`). Every animation Lucca has, she has, under the same names. Her `nearShield` frames are the same two as her `drawShield`.

## Flaws

- **Her colours drift between sheets.** The ledge-and-lunge sheet drew her robe cream and her boots black; everywhere else the robe is grey-white and the boots brown. It shows when she grabs a ledge.
- The walk-and-run sheet gives her a double belt and dark cuffs that the other sheets lack.
- Her hood is down in the rope-slide frame only.
