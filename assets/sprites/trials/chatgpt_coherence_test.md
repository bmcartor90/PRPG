# ChatGPT coherence test

The question: can ChatGPT draw about a dozen sheets per character and keep him the same? Three sheets from two chats answer it before the full set is ordered. Every sheet ends with a **control frame**: Lucca standing exactly as in the reference. That frame is measured against the reference (height, head size, robe colour) and is how a drifting sheet gets caught.

Rules that keep him coherent:

- Attach the same reference picture (`chatgpt/lucca_v2_anchor.png`) to every request. Never use a later sheet as the reference: copies of copies drift.
- 8 frames a sheet at most, the last one the control frame.
- A sheet whose control frame is off gets regenerated, not patched.

## Sheet A: far arm (same chat as test 1)

Paste Test 2 from `chatgpt_follow_up_tests.md`, and add this line at the end:

```
Add a fifth frame: Lucca standing exactly as in the last frame of your pull-up sheet.
```

## Sheets B and C: new chat, reference picture attached

```
Sprite sheet for a 2D side-scrolling platformer. The attached picture is my character,
Lucca. Match him exactly: same face, hair, robe, colours, outline and pixel style. Do not
redesign him.

CAMERA: fixed side-on, level with his waist, no perspective distortion. He faces right with
his chest turned very slightly toward the viewer, as in the attached picture. That angle
never changes. Facing right, his right arm is the near arm and his left arm is the far arm.

RULES:
- One landscape image, 1536 x 1024. Truly transparent background.
- Frames in equal-sized cells, left to right, clear space between them, nothing touching.
- The same scale in every frame: his head is the same size in every frame.
- Hands are empty closed fists.
- No text, labels, grid lines, ground lines, shadows, glows, motion lines, scenery or props.
- The LAST frame of the sheet is always Lucca standing exactly as in the attached picture.

FRAMES:
[paste one of the lists below]

After the image, say plainly where any frame departs from this.
```

Sheet B, walk cycle, 6 frames plus the control frame:

```
A walk cycle that loops, one row of 6, then the standing frame.
1. Right foot forward and flat on the ground, left foot back on its toe. Left arm forward,
   right arm back.
2. Weight over the right foot, left foot lifting and passing.
3. Left foot swinging forward, right heel lifting.
4. Left foot forward and flat, right foot back on its toe. Right arm forward, left arm back.
   The mirror of frame 1.
5. Weight over the left foot, right foot lifting and passing.
6. Right foot swinging forward, left heel lifting. Leads back into frame 1.
7. Standing, exactly as in the attached picture.
```

Sheet C, crawl cycle: the six frames of Test 3 in `chatgpt_follow_up_tests.md`, then `7. Standing, exactly as in the attached picture.`
