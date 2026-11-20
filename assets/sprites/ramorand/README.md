# Ramorand sprites

Sheets for Ramorand, the third character. Six sheets so far, so he is not cut into an atlas and is not in the lab.

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

## Still needed

The moving draws (walking and running). Without them he can still be played; an item drawn while moving would slide into his hand instead of being animated. Skip the "inverted" moving draws sheet, which has never come back with the far arm.

## To watch for

- **The cape.** It hangs well behind his body. On ledges, ladders and ropes it needs somewhere to go, and it will hide a shield slung on his back.
- **His fists are bare and dark-skinned against silver armour.** Marking them for held items needs a different colour test from the one used for Lucca and Nina.
