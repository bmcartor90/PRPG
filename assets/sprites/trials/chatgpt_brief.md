# Brief: sprite sheets for my platformer character, Lucca

I am making a 2D side-scrolling platformer. The four attached images show my lead character, Lucca, on sprite sheets made with another AI art tool. I like how he looks and I want new sheets that match him exactly.

That tool could not keep one camera angle through a complex move, and it could not animate the arm farther from the viewer. I want to find out whether you can. Please make **Test 1 only**, then stop so I can check it.

## What is attached

1. `01_lucca_walk_run__MATCH_THIS.png`: the look to match. Top row is his walk, bottom row his run.
2. `02_lucca_close_up.png`: his first walk frame enlarged, so you can see the detail.
3. `03_lucca_jump_crouch_slide.png`: more frames in the right style.
4. `04_lucca_ledge_pullup__HAS_THE_PROBLEMS.png`: the old attempt at the move in Test 1. Use it to see what went wrong, not as something to copy.

## The character

- **Lucca**, a young swordsman, about four and a half heads tall.
- Shoulder-length, wavy, blue-black hair. Light skin.
- A long navy-blue wrap robe that reaches his ankles, split at the front below the waist, with wide sleeves. A dark grey sash at the waist.
- Dark grey trousers and dark laced boots.
- Hands are empty closed fists.
- Style: chunky, detailed pixel art with a crisp dark outline. Do not smooth it, repaint it in another style, or redesign him.

## The camera, which is the part that matters most

- A fixed side-on camera, level with his waist, with no perspective distortion, as in a classic 2D platformer.
- He faces right, with his chest turned very slightly toward the viewer, exactly as in the walk sheet.
- **That angle never changes inside a sheet.** Not when he hangs, climbs, crouches or lies down. Only his limbs and spine move. We never see him from the front or from behind.
- Facing right, his **right arm is the near arm** (closer to the viewer) and his **left arm is the far arm**.

## What went wrong with the other tool

Image 4 shows most of these.

1. **The camera swings.** The hanging and pull-up frames face the viewer almost front-on, and the standing frame is side-on. Played in order, he spins round.
2. **The size drifts.** His lunging and lying-down frames are drawn smaller than his standing frames.
3. **The frames are separate poses, not one motion.** The pull-up jumps from hanging, to knees up, to kneeling, and nothing connects them.
4. **Only the near arm ever acts.** Three requests for the far arm to do something all came back with the near arm doing it.
5. **Frames get repeated.** "Alternate arms" came back with the same arm raised twice.
6. **He drifts between sheets:** the robe's shade, the sleeve width, the hair length, and how smooth the shading is.
7. **Extras get painted in:** ladders, glows, shadows and motion lines that were not asked for.

## Rules for every sheet

- One landscape image, 1536 x 1024 or larger.
- A truly transparent background. If you cannot produce real transparency, use flat solid magenta (#FF00FF) with no gradient. Never a checkerboard pattern.
- Frames in equal-sized cells, in order from left to right, with clear empty space between them. Nothing touching, nothing cut off at an edge.
- **The same scale in every frame.** Use his head as the ruler: it is the same size in every frame of every sheet.
- Centre each frame in its cell. I will line the frames up myself.
- No text, labels, frame numbers, grid lines, ground lines, shadows, glows or motion lines.
- No scenery or props: no ledge, rope, wall or weapon. Only Lucca.

## Test 1: ledge pull-up, 8 frames

Two rows of four. Row 1 is frames 1 to 4, row 2 is frames 5 to 8.

Imagine a ledge just in front of him, on his right, at the height his fists reach when he hangs at full stretch. Do not draw the ledge. His two fists stay side by side, gripping it, from frame 1 to frame 5.

1. **Dead hang.** Arms straight overhead, body hanging straight down, legs together. Seen from the side, the same angle as the walk sheet.
2. **Starting to pull.** Elbows bent to a right angle, the top of his head level with his fists, knees slightly bent.
3. **Chin over.** His chin is above his fists, elbows tucked down by his ribs, legs trailing below.
4. **Chest over.** His chest is above his fists and leaning forward over the ledge. His arms are pushing down, elbows pointing up and back. His near knee is coming up.
5. **Knee on.** His near knee is planted at the height of his fists. His arms are almost straight, pushing down. His far leg still hangs below.
6. **Crouched on top.** Both feet are on the ledge, he is crouched low, and both fists rest on the ground by his feet.
7. **Rising.** A half crouch, fists leaving the ground, back straightening.
8. **Standing.** Upright, exactly like the first frame of the walk sheet.

## When you answer

After the image, list frame by frame what you drew, and say plainly where any frame departs from this brief. Do not start another test until I ask.
