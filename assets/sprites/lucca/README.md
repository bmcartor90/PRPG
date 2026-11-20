# Lucca sprites

Sprite sheets for Lucca, the test character, and the frames cut from them.

## Folders

- `source/` holds the sheets exactly as they came from Meshy. Do not edit these.
- `atlas/` holds the frames cut out of the sheets and packed into one image, with a JSON file saying where each frame is.

## Source sheets

All sheets are 1536 x 864. The art is a detailed pixel-art style but is **not** on a true pixel grid, so it is scaled smoothly, never snapped to whole pixels.

| File | Frames | Background | Used in the atlas |
|---|---|---|---|
| `Meshy_AI_Pixel_Character_Sprite_Sheet.png` | Stand and walk (6), run (6) | Grey | Yes |
| `Meshy_AI_Character_Action_Sprite_Sheet.png` | Jump (7), crouch (2), slide (3), ladder climb (4) | Grey | Jump, crouch and slide. The ladder frames have a ladder painted in and are not used. |
| `Meshy_AI_Lucca_Ledge_and_Lunge_Sheet.png` | Hang (2), pull-up (4), stand (1), one-handed hang (2), lunge (2), face down (1), get up (2) | Transparent | Yes |
| `Meshy_AI_Lucca_Rope_and_Climb_Sheet.png` | Rope climb (4), rope hang (1), one-handed rope (1), rope slide (1), ladder from behind (3), ladder rest (1), wall climb (2), wall hang (1) | Transparent | Yes |
| `Meshy_AI_Lucca_Weapon_Draw_Sheet.png` | Draw sword (4), shield from back (2), draw staff (4), sword and shield (2) | Transparent | No. The items are painted into him, so this is a reference for where items sit in his hands. |
| `Meshy_AI_Lucca_Invisible_Weapons_Sheet.png` | The same 12 poses with empty fists | Black | Not yet. These are the poses the items get pinned onto. |
| `Meshy_AI_Lucca_Weapons_Equipment_Sheet.png` | Items only: sword, sheath, shield (front, back, edge-on), staff | Transparent | Not yet. Each item is drawn flat so it can be rotated to fit his hand. They are drawn much larger than him and must be scaled down: the sword is 514 pixels long against his 307-pixel height. |

## Known flaws in the sheets

- Frame sizes vary within a sheet, so every frame is anchored individually when it is cut out.
- The ledge sheet draws him about 5% smaller than the others. The atlas corrects for it.
- The two one-handed hangs in the ledge sheet are the same pose.
- The ladder and wall "alternate arm" frames raise the same arm. The game mirrors the frame to get the other arm.
- The black background on the empty-handed sheet makes his dark outline hard to separate. Ask for a transparent or grey background.

## The atlas

`atlas/lucca_atlas.png` is all usable frames packed together, at the source resolution, with a thin pale outline added so he reads against a dark background.

`atlas/lucca_atlas.json` lists every animation and its frames:

- `x`, `y`, `w`, `h`: where the frame is in the image.
- `anchorX`, `anchorY`: the point under his feet, measured from the frame's top-left corner. Draw the frame so this point lands on his position in the world.
- `handX`, `handY`: present on hanging, pull-up and rope frames. The point where his gripping hand is. On a ledge or rope, pin this point to the ledge or rope instead of pinning his feet.
- `standingHeightPx`: how tall he is standing upright, in atlas pixels. Scale the art so this equals his height in the game.

He faces right in every side-view frame. Mirror the frame to face left.
