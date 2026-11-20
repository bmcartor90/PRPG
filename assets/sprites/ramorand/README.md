# Ramorand sprites

Sheets for Ramorand, the templar, and the frames cut from them. He is playable in the feel lab (the Character menu), 1.9 tiles tall, carrying his own sword and shield.

| File | Frames | Verdict |
|---|---|---|
| `Meshy_AI_ramorand_walk_and_run_sheet.png` | Stand and walk (6), run (6) | Keep. Transparent background, empty fists, consistent size. |
| `Meshy_AI_ramorand_action_acrobatics_sheet_1.png` | Jump (5), crouch (1), slide (1), ladder climb from behind (3) | Keep, but short: 10 frames where 16 were asked for (jump 7, crouch 2, slide 3, ladder 4). Enough to play him by reusing frames; the jump and slide will look choppier than the others'. |
| `Meshy_AI_ramorand_ledge_and_lunge_sheet.png` | Hang (2), pull-up (4), stand, one-handed hang (2), lunge (2), face down, get up (2) | Superseded by the bare-hands version below. His hands are in silver gauntlets here and bare everywhere else. |
| `Meshy_AI_ramorand_ledge_bare_hands_sheet.png` | The same 14 poses with bare fists | Keep; this is the one to cut. It has an opaque grey background where the others are transparent. The grey is far enough from his armour that a colour cut loses almost nothing, but it is a re-render, not a recolour: the armour shading differs slightly from the first version. |
| `Meshy_AI_ramorand_rope_and_climb_sheet.png` | Rope climb and hang, side view (7); ladder and wall climb, from behind (7) | Keep. All 14 frames, same order as the others'. Bare hands. From behind, the cape covers his whole back. |
| `Meshy_AI_ramorand_invisible_combat_sheet.png` | Sword draw (4), shield fetch (2), staff draw (4), guard (2) | Keep. All 12 frames, empty bare fists, consistent size. |

A second copy of the ledge sheet arrived as `..._sheet_1.png`; it is the same image and is not stored.

He stands about 319 pixels tall on the sheet and is much broader than Lucca or Nina, with a long cape trailing behind him.

## The atlas

`atlas/ramorand_atlas.png` and `atlas/ramorand_atlas.json`, in the same format as Lucca's. He has every animation name the others have except the four moving draws, so an item drawn while he walks or runs slides into his hand instead of being animated.

Because his action sheet is short, his `jump` reuses frames (7 slots from 5 drawings), and `crouch` and `slide` are one drawing each.

## Still needed

The moving draws (walking and running). Optional: a fuller action sheet, for a smoother jump and slide.

## To watch for

- **The cape.** It hangs well behind his body. On ledges, ladders and ropes it needs somewhere to go, and it will hide a shield slung on his back.
- **His near fist is often drawn gauntlet-grey**, even on sheets where the far fist is bare. His fist positions were marked by eye, not by colour, and are a few pixels less exact than the others'.
