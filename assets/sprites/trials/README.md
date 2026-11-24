# Trials of other ways to make character sprites

## 3D model route (Meshy rig, rendered in Blender), 2026-11-20 to 23: rejected

A stylised 3D Lucca was rigged in Meshy and photographed side-on at game size. The robe survived walking and running, perspective was consistent and both arms animated. Brad judged the painted sprites better looking, so the route was dropped. Wide sleeves stretched on overhead reaches, the library's sword clip was not a draw, and the rig had no finger bones. The model files (about 120 MB) are not stored here.

## ChatGPT image generation, started 2026-11-23

`chatgpt_brief.md` is pasted into ChatGPT with four Lucca images (walk and run sheet, a close-up, the action sheet, the ledge sheet as the example of what went wrong). It asks for a ledge pull-up with a fixed camera. `chatgpt_follow_up_tests.md` has a far-arm test and a crawl cycle. Results get the usual four checks.

### Test 1 result (ledge pull-up, 8 frames), 2026-11-23

`chatgpt/test1_ledge_pullup.png`, compared at game size in `chatgpt/test1_vs_painted.png`.

- **Structure: pass.** 8 frames in order, nothing touching, no props or effects. Real transparency (82% fully transparent, under 2% semi-transparent fringe; cuts cleanly at alpha above 200). The camera stays side-on from the hang to the crouch, which Meshy never managed, and the frames read as one motion. Face height is 45 to 50 pixels across frames 1 to 7, so the scale holds.
- **Look: fail.** A different drawing of the same character: shorter, bushier hair with a fringe over the eye, a rounder and younger face, narrower sleeves, a thicker outline and flatter shading. The robe is dark indigo (59, 64, 91) against the painted blue (57, 88, 118); that part can be recoloured when cutting.
- Not mixable with the Meshy sheets as it stands.

## Gemini image generation, 2026-11-23

Same brief. `gemini/test1_ledge_pullup.jpg`. Gemini said it had not looked at the reference images (they were given as a zip, which it could not open), so it worked from the text alone.

- **Fail.** It is not a pull-up: he stands with his feet on the ground in every frame, and frames 2 to 5 are a guard pose, two running poses and a raised knee. The camera is front three-quarter and changes direction (frames 6 and 7 face left).
- Labels and black grid lines are painted in; the brief forbade both. A lossy JPEG, 1264 x 848, on magenta with compression noise.
- The look is a smooth painted style with a purple robe (66, 75, 128), not the chunky pixel look.
- Not a fair test of matching the look, since it never saw the references. A fair retry attaches the four PNGs directly.

### Gemini retry with the four reference PNGs attached, 2026-11-23

`gemini/test1_retry_with_references.jpg`. All three side by side at game size: `test1_three_way.png`.

- **Look: the closest yet.** Robe (56, 85, 113) against the painted (57, 88, 118); wavy shoulder-length hair, wide sleeves, ankle-length robe, and the painted sheets' shading.
- **Structure: fail, the same way Meshy failed.** Frames 1, 2 and 6 face the viewer and the rest are side-on, so he turns during the move. Frame 5 is a forward punch, not a knee on the ledge. Frames 3 and 4 read as running.
- Black grid lines are still painted in (labels are gone). JPEG on magenta, leaving a magenta fringe that needs trimming when cut.
- Summary of the three: ChatGPT holds the camera and the motion but not the look; Gemini holds the look but not the camera. Next: give Gemini ChatGPT's sheet as the pose guide.

## Decision, 2026-11-23: ChatGPT draws the characters

Brad chose ChatGPT's version of Lucca over matching the Meshy look: a camera that holds matters more than keeping the old drawing, and the story can be written around any character. Gemini is dropped.

- `chatgpt/lucca_v2_anchor.png` is the standing frame from test 1, enlarged, as the provisional reference picture for new requests.
- Not yet proven: that ChatGPT keeps him the same from sheet to sheet and in a new chat, and that it can animate the far arm. Both get tested before the Meshy sheets are retired.
- The Meshy character sheets stay in the lab until a full replacement set has passed the checks.

### ChatGPT walk sheet, 2026-11-23

`chatgpt/sheetB_walk.png`; beside the painted walk in `chatgpt/sheetB_walk_vs_painted.gif`; the three standing Luccas in `chatgpt/three_luccas.png`.

- **Structure: pass.** 6 walk frames and a standing frame, side-on throughout, real transparency. All seven are 360 to 367 pixels tall, faces 50 to 53 pixels, and six of seven share the same foot line to the pixel.
- **Look: matches the painted Meshy Lucca, not ChatGPT's own test 1 Lucca.** Robe (57 to 62, 87 to 92, 118 to 124) against the painted (57, 88, 118); swept-back wavy hair, wide sleeves.
- So ChatGPT can draw the original Lucca. It also means its two sheets so far show two different Luccas; which reference each chat had decides what that says about coherence.
- The walk steps high in frames 2 and 5, a little like marching.

## One pack per chat, 2026-11-23

The earlier handoff asked Brad to send different subsets of one zip to different chats, which caused confusion about which reference each sheet had seen. Replaced by `chatgpt_packs/`: each pack is one new ChatGPT chat, with exactly one picture to attach (`lucca_reference.png`, the painted Lucca: standing close-up plus his walk and run rows) and one text to paste. Every pack ends with a freshly drawn standing control frame. The three packs (pull-up, far arm, crawl) in three separate chats are the coherence test. This goes back to the original Lucca look, since ChatGPT's walk sheet showed it can draw him.

Packs as of 2026-11-23: `Chat1_ledge_pullup`, `Chat3_crawl`, `Chat4_sword_draws`, `Chat5_shield_draws`. The two draw packs each show the move with the near arm and with the far arm, hands empty, and replace the earlier `Chat2_far_arm`. Brad wants sword and shield draws with either hand in the game but would cut them at the Meshy sheets' quality, so these two decide whether they stay.

### First run of the packs, 2026-11-23: no valid results

All five packs were given to ChatGPT as the zip file itself, with a typed line such as "process them into game ready assets". None tested what was intended:

- Pull-up and far-arm packs: ChatGPT unzipped them and cut the reference picture into idle, walk and run frames. Those are the original Meshy frames re-cut; nothing new was drawn.
- Crawl, sword and shield packs: the image generator was handed "game ready assets" and returned generic asset-pack pictures (a spider, village props).

Lesson: the reference must be attached as a loose picture and the prompt pasted as the message, with nothing else typed. A zip is treated as a file to process, and the picture inside never reaches the image generator as a visual reference. Packs are now delivered as one loose picture plus text pasted from the conversation, one test at a time.

### Pull-up from one reference picture in a new chat, 2026-11-23: pass

`chatgpt/pack1_pullup.png`; beside the painted pull-up in `chatgpt/pack1_vs_painted.png`; played against an imagined ledge in `chatgpt/pack1_on_ledge.gif`.

- 9 frames in order, real transparency (under 3% fringe), nothing painted in.
- **Camera holds side-on through the whole move, in the original Lucca look.** Robe (51 to 57, 84 to 90, 108 to 119) against the painted (57, 88, 118).
- Pinned by the fists for frames 1 to 5 and by the feet after, it plays as one pull-up onto a ledge.
- Control frame 372 pixels tall against 361 on the earlier walk sheet (3% apart, corrected when cutting); robe colour agrees between the two sheets.
- Flaws: frames 2 and 3 are nearly the same pose (the chin never clears the fists); his sleeves ride up to the elbow in frames 4 and 5; in frame 1 his raised arm hides his face.
- Method that worked: one loose reference picture attached, the prompt pasted as the message, nothing else typed.

### Sword draws, shield draws and crawl, each in its own new chat, 2026-11-23

`chatgpt/pack4_sword_draws.png`, `pack5_shield_draws.png`, `pack3_crawl.png`. Played back in `draws_either_hand.gif` and `crawl_cycle.gif`. The standing frame from all five ChatGPT sheets beside the painted one: `six_standing_luccas.png`.

**Coherence across five separate chats: pass.**

| Sheet | Control frame height | Robe colour |
|---|---|---|
| Painted (Meshy) | 307 | 57, 88, 118 |
| Walk | 361 | 57, 90, 123 |
| Pull-up | 372 | 54, 90, 119 |
| Sword draws | 391 | 61, 91, 119 |
| Shield draws | 439 | 60, 90, 121 |
| Crawl | 345 | 55, 86, 115 |

Each chat draws him at a different size, which the control frame corrects; scaled to the same height the five are the same character. Within the sword and shield sheets all nine frames are the same height to 2 pixels with feet on one line.

- **Far arm: pass, on both sheets.** The left arm animates while the right hangs still, and the reverse. This is what Meshy never did.
- Sword, near hand: the ready frame ends with the fist close to his belly, much like the first frame. The far-hand row reads better.
- **Crawl: redo.** He is 0.98 to 1.01 tiles tall, so he exactly fills a 1-tile gap with no clearance, and the legs barely alternate (frames 1, 4 and 6 are nearly the same).
- ChatGPT's Lucca has a slightly heavier outline and smoother shading than the Meshy frames, so the two should not be mixed within one character.
