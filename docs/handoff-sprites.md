# Handoff: art intake for PRPG

You are taking over one job on Brad McArtor's 2D platformer RPG: **bringing new art into the project and making sure it fits.** That means judging each sheet Brad sends, cutting it into frames, marking the points the game needs, and getting it showing correctly in the feel lab. Everything you need is in the repository `bmcartor90/PRPG`; nothing lives only in the previous conversation.

## What is yours and what is not

**Yours:**
- Every character sprite sheet, item sheet, tile sheet and background Brad sends.
- `assets/` and its READMEs, `assets/sprites/characters.md`, `assets/sprites/prompt_template.md`.
- `tools/sprites/`.
- In `feel-lab/index.html`: the sprite data, `pickFrame`, and the drawing code after it. Also `feel-lab/*.png`.
- Writing Meshy prompts when a sheet is missing or needs redoing.

**Not yours. Leave these alone and tell Brad if they need changing:**
- The movement simulation: everything between `/*SIM-START*/` and `/*SIM-END*/` in the lab, the sliders, the level, `feel-lab/traces/` and `feel-lab/checks/`.
- `docs/build-spec.md`, `docs/design.md`, `docs/changes.md`, `docs/answers.md`. The design conversation maintains these.
- `sim/`, `game/`, `data/`. The programmer (Claude Opus, working on a branch) owns these.
- Design decisions. Brad makes them. Recommend, then ask.

Two other parties edit this repository, so **pull before you start and before every push**, commit small, and never force-push. The lab file is shared with the design conversation: keep your edits inside the parts listed above.

## How Brad wants to be worked with

- Blunt. Brief: he rates brevity 2 out of 5 as his target. Lead with the verdict.
- **Number every point of pushback or open decision** so he can answer by number. The design conversation is at 161; start yours at **S1** so the two series never collide.
- If his message has a likely typo, or a name you are unsure of, ask. Do not silently correct.
- Recommend the best approach. Do not do something only because he said so, and say when you disagree.
- End every reply with a "tier 5" word: an obscure real dictionary word and its definition.
- Do not touch files on his computer without his permission.
- He sends sheets with no message. Treat each one as "evaluate this, store it, and wire it in if it is usable."
- He told the previous agent it was doing too much in one turn. Do the art job and stop; do not wander into the simulation or the spec.

## What "fits" means

A sheet from Meshy is 1536 x 864, in a detailed pixel-art style that is **not** on a real pixel grid. It is scaled smoothly, never snapped.

Check every sheet for four things before doing anything else:

1. Every frame present, in the order asked for.
2. Clear space between frames, nothing cut off at an edge.
3. The figure is a consistent size, and matches that character's other sheets.
4. Nothing in the frame that was not asked for: no props, no items in the hands, no ground lines.

Then look for drift from the character's other sheets: colours, clothing details, bare hands versus gloves. Every character so far has drifted somewhere. Record it in that character's README, fix it when cutting if you can, and ask for a regeneration only if you cannot.

Meshy has consistent habits. They are written up in `assets/sprites/prompt_template.md`; read it before writing any prompt. The two that matter most: it will not animate the arm farther from the viewer however you ask, and it ignores requested pixel sizes.

## The data each frame carries

Read `assets/sprites/lucca/README.md` first. It defines everything; the other characters follow it. In short, each character has an atlas image and a JSON file listing animations by name, and each frame has:

- A **foot anchor**: the point under the feet. This is what pins the frame to the character's position.
- A **hand anchor**, on hanging, pull-up and rope frames only: where the gripping hand is. On a ledge or rope the frame is pinned by this point instead.
- **Back fist and front fist**, on ground, air and draw frames: where the two fists are, for pinning held items.
- An **item pose**, on draw frames only: where the item being drawn is and how it is turned.

Two rules that are easy to get backwards, both explained in the Lucca README:

- **Handedness.** The art faces right, turned slightly toward the viewer, so the back fist is the right hand. Mirrored to face left, the right hand is the front fist. A sword in the right hand therefore moves from the back fist to the front fist when the character turns around.
- **Draws are animated for the near arm only.** The far arm's item slides from hip or back into the fist with no arm animation. Stowed gear is never drawn.

Animation names and frame counts must match across characters, because the lab and the game pick frames by name. The full list is whatever `lucca_atlas.json` has. If a character lacks an animation, either reuse frames to fill the slots (Ramorand's jump) or leave it out and make sure the frame-picking code copes (Ramorand's moving draws).

## Where each character stands

| Character | Height | State |
|---|---|---|
| **Lucca** | 1.9 tiles | Complete and in the lab. His hanging and climbing frames have no fist positions yet, so items sit at a fixed spot on his body there. His first two sheets came on grey backgrounds and one on black. |
| **Nina** | 1.7 tiles | Complete and in the lab. Her ledge sheet's robe colour is corrected at cutting time; her boots on those frames are still slightly dark. No own gear; she uses Lucca's. |
| **Ramorand** (templar) | 1.9 tiles | In the lab. Missing the moving-draws sheet. His action sheet is short, so his jump and slide reuse frames. His fist positions were marked by eye and are rougher than the others'. Has his own sword and heater shield, plus a lightning sword that nothing uses yet. |

Each has a README under `assets/sprites/<name>/` with a per-sheet verdict and known flaws, and a block in `assets/sprites/characters.md` for re-briefing Meshy.

Items: `assets/sprites/items/`. Two gear sets in one image. All items are scaled to Lucca's pixels-per-tile and must stay the same size in the world whoever holds them.

Tiles and backgrounds, under `assets/tiles/` and `assets/backgrounds/`:

- **Scaffold** (wooden, for jump-through surfaces, with ladder and rope): good, piece positions recorded, not yet used in the lab.
- **Crystal platforms:** finished chunks, not a tileable set. A prompt for a proper ground set (surface, cliffs, fill) has been given to Brad; nothing has come back yet.
- **Far background:** good. Repeating versions exist, including a violet recolour made so Lucca's navy robe stands out. Brad has not said whether he prefers violet or the original blue.
- The lab still draws surfaces as plain lines. Putting tiles and the background into the lab has not been started.

`docs/mockups/` has still images of the art assembled at game scale. They are the quickest way to see what "fits" looks like.

## The tools

`tools/sprites/` holds the scripts that did all of the above, exactly as they were used. **They do not run from the repository yet**, and making them do so is your first job:

- They read sheets from upload paths like `/mnt/user-data/uploads/<id>-image.png`. The table below maps each id to the file in `assets/`.
- They read and write working files in a `sprites/` folder beside them. Those files are in `tools/sprites/work/`.
- They write the lab's images to `/home/claude/feel-lab/`. Point that at `feel-lab/`.

What each does:

| Script | Job |
|---|---|
| `char_boxes.py`, `nina_boxes.py`, `empty_boxes.py`, `slice3.py` | Find the frames on a sheet and group them into rows. Transparent sheets use alpha; opaque ones flood in from the border through the background colour. |
| `atlas.py` | Lucca's atlas. Older and more special-cased, because his sheets vary. |
| `atlas2.py` | Nina's and Ramorand's atlases, from a per-character table: which sheet and frame each animation uses, per-sheet size correction, colour fixes. **Add new characters here.** |
| `blobs.py` | Marks skin-coloured patches on each frame and writes a numbered contact sheet, for finding fists and hands. |
| `fists.py`, `fists_nina.py`, `fists_ramorand.py` | The labels: which patch is which fist, item poses for draw frames, grip points. |
| `items.py` | Cuts and scales item sheets. (Ramorand's set was added inline; fold it in.) |
| `repo_sync.py` | Writes each character's `atlas/` folder and copies the lab files into `feel-lab/`. |
| `mock.py`, `mock2.py` | The still mock-ups. |
| `smoke.js`, `shot16.py` | Load the lab without a browser to catch script errors; drive it in headless Chrome to capture every move of a character. |

The lab's sprite data is pasted into `feel-lab/index.html` by hand-run snippets, not by a script. Search that file for `const SPRITES`, `const FISTS`, `const DRAW_POSE` and `const ITEMS`. A script that regenerates those four blocks from the JSON would be a worthwhile early improvement.

**Sheet ids:**

| Id | File |
|---|---|
| `67ddc9c4` | `lucca/source/Meshy_AI_Pixel_Character_Sprite_Sheet.png` |
| `9aa0aed0` | `lucca/source/Meshy_AI_Character_Action_Sprite_Sheet.png` |
| `ebd959ba` | `lucca/source/Meshy_AI_Lucca_Ledge_and_Lunge_Sheet.png` |
| `31b2f36f` | `lucca/source/Meshy_AI_Lucca_Rope_and_Climb_Sheet.png` |
| `25cd7dc8` | `lucca/source/Meshy_AI_Lucca_Weapon_Draw_Sheet.png` |
| `51f22a66` | `lucca/source/Meshy_AI_Lucca_Invisible_Weapons_Sheet.png` |
| `442ad3cf` | `lucca/source/Meshy_AI_Lucca_Moving_Draws_Sheet.png` |
| `1dce5bb6` | `lucca/source/Meshy_AI_Lucca_Draw_Opposite_Hand_Sheet.png` |
| `f244808c` | `lucca/source/Meshy_AI_Lucca_Moving_Draws_Inverted_Sheet.png` |
| `b4e1e56c` | `items/source/Meshy_AI_Lucca_Weapons_Equipment_Sheet.png` |
| `65975ccc` | `nina/source/Meshy_AI_Nina_Walk_and_Run_Sheet.png` |
| `72dd2f7a` | `nina/source/Meshy_AI_Nina_Invisible_Combat_Sheet.png` |
| `0c517fb8` | `nina/source/Meshy_AI_Nina_Moving_Draws_Sheet.png` |
| `c67b52a8` | `nina/source/Meshy_AI_Nina_Moving_Draws_Inverted_Sheet.png` |
| `65ec7e6d` | `nina/source/Meshy_AI_Nina_Rope_and_Wall_Sheet.png` |
| `b04cdec0` | `nina/source/Meshy_AI_Nina_Ledge_and_Lunge_Sheet.png` |
| `4d7d789d` | `nina/source/Meshy_AI_Nina_Action_Acrobatics_Sheet.png` |
| `7997c641` | `ramorand/source/Meshy_AI_ramorand_walk_and_run_sheet.png` |
| `9bc3dcb6` | `ramorand/source/Meshy_AI_ramorand_action_acrobatics_sheet_1.png` |
| `f8aa2c67` | `ramorand/source/Meshy_AI_ramorand_ledge_bare_hands_sheet.png` |
| `229f252c` | `ramorand/source/Meshy_AI_ramorand_ledge_and_lunge_sheet.png` (superseded) |
| `b64f54a6` | `ramorand/source/Meshy_AI_ramorand_rope_and_climb_sheet.png` |
| `5e5e0121` | `ramorand/source/Meshy_AI_ramorand_invisible_combat_sheet.png` |
| `61b27d1a` | `items/source/Meshy_AI_ramorand_weapons_equipment_sheet.png` |
| `6dd62cb4` | `assets/tiles/crystal/source/Meshy_AI_crystal_platform_tileset.png` |
| `474ce6bb` | `assets/tiles/scaffold/source/Meshy_AI_miner_scaffold_tileset.png` |
| `41ccb3df`, `91d305e3` | `assets/backgrounds/crystal-cavern/` (first matte, far matte) |

Character paths are under `assets/sprites/`.

## Bringing in a new character sheet, step by step

1. Save the sheet under `assets/sprites/<name>/source/` with Brad's file name. Never edit a source file.
2. Run the four checks. Measure, do not eyeball: frame count per row, figure heights, background type.
3. Compare the standing height with that character's walk sheet and set a size correction for the sheet if it differs.
4. Add the sheet's frames to the character's table in `atlas2.py` and rebuild the atlas.
5. Run `blobs.py` on the new animations, look at the contact sheet, and record fists, grip points and item poses. Coordinates are in each frame's own pixels.
6. Update the lab's data blocks and, for a new character, the Character menu.
7. Run `smoke.js`, then capture the character doing the affected moves and **look at the result**. Hands on the ledge, feet on the ground, sword in the correct hand facing both ways.
8. Sync to the repository, update the character's README and block in `characters.md`, commit, push.
9. Republish the lab so Brad can play it.

## The feel lab

`feel-lab/index.html` in the repository is the source of truth. It is also published as an artifact titled "Platformer Feel Lab", which is how Brad plays it. Publish `index.html` together with every image it loads: `sprites.png` (Lucca), `nina.png`, `ramorand.png`, `items.png`. Find the artifact through the artifact tools in your own session; read it first to get its current version before republishing.

The Character menu swaps the art and sets the height slider to that character's height. It changes no other number.

## Work waiting, most useful first

1. Make `tools/sprites/` run from the repository.
2. Ramorand's moving-draws sheet, when Brad sends it. Skip any "inverted" sheet; it has failed for every character.
3. Fist positions on hanging and climbing frames, for all three characters.
4. The crystal ground set, when it comes back: cut it, check the pieces actually join, and report.
5. Tiles and the far background drawn in the lab in place of white lines. Ask Brad before starting; it is a visible change to a tool he uses daily.
6. A better action sheet for Ramorand, if Brad wants his jump and slide as smooth as the others'.
7. Decide with Brad what the lightning sword is for.

## Questions Brad has not answered that touch the art

- Violet or blue for the cavern background.
- Whether Nina's ledge sheet should be regenerated to fix her boots, or left.
- What the climbable wall should look like. There is no art for it.
- Whether Nina gets her own gear.
