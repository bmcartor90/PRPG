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
| `Meshy_AI_Lucca_Invisible_Weapons_Sheet.png` | The same 12 poses with empty fists | Black | Yes. The sword and shield draws are used in the lab; the staff and guard frames are cut but not used yet. |
| `Meshy_AI_Lucca_Moving_Draws_Sheet.png` | Walking: sword draw (4), shield fetch (2). Running: the same six. Empty fists. | Transparent | Yes |
| `Meshy_AI_Lucca_Draw_Opposite_Hand_Sheet.png` | Standing sword draw (4), shield fetch and carry (2), the same two again (2) | Transparent | Frames 5 and 6 only. It was meant to show the far arm doing the work and shows the near arm instead. |
| `Meshy_AI_Lucca_Moving_Draws_Inverted_Sheet.png` | Same as the moving draws sheet | Transparent | No. It was meant to swap arms and did not, so it duplicates the other sheet. |
| `../items/source/Meshy_AI_Lucca_Weapons_Equipment_Sheet.png` | Items only: sword, sheath, shield (front, back, edge-on), staff | Transparent | Cut out and scaled to his size in `../items/`. On the sheet they are drawn much larger than him: the sword is 514 pixels long against his 307-pixel height. |

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

- `backFist`, `frontFist`: on every frame. Where his two fists are, for pinning held items. On frames seen from behind (ladder, wall), where he has no facing, `backFist` is simply his right hand: the one on the right of the unmirrored picture.
- One-handed hangs (`hang1`, `rope1`) grip with a fixed hand: `hang1` with his right, `rope1` with his left (the same for every character). Until frames gripping with the other hand exist, put a held item in the free fist even if it belongs to the gripping hand.
- `itemPose`: on draw frames. Where the item being drawn is and how it is turned.

He faces right in every side-view frame. Mirror the frame to face left.

## Which hand is which

The art faces right and is turned slightly towards the viewer, so the fist on the side he faces away from (the back fist) is his right hand. When the frame is mirrored to face left, that same fist is anatomically his left. So an item in his right hand is pinned to the back fist when he faces right and to the front fist when he faces left.

## Drawing and sheathing

Meshy only ever animates the arm nearer the viewer, however the request is worded. Three sheets asked for the far arm and none delivered it. So the rule is:

- **Near arm** (his right facing right, his left facing left): the draw is animated. Facing right that is the sword; facing left it is the shield, using the mirrored `nearShield`, `walkDrawShield` and `runDrawShield` frames.
- **Far arm:** no arm animation. The item slides from where it is stowed (sword at his hip, shield on his back) to that fist over the draw time, passing behind his body for the first part.

Sheathing is the draw played backwards. `drawShield`, `drawStaff` and `guard` are cut into the atlas but not used yet.
