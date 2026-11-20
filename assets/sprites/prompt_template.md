# Prompt template for sprite sheets

Paste the preamble, fill in the character, then list the rows of poses. Upload the character's reference picture with it (see `characters.md`).

## Preamble

```
Sprite sheet for a 2D side-scrolling platformer. One single image, 1536 x 864.
Detailed pixel-art style with a crisp dark outline. Transparent background.
Match the attached reference image exactly: same character, same palette,
same proportions, same size.

CHARACTER: [name, then the description from characters.md]

LAYOUT RULES:
- Side view, character facing right, full body visible in every frame.
- [N] rows. Each row is one animation, frames in order left to right.
- Equal-sized cells, evenly spaced, clear empty space between frames.
- The character is the same height in every standing frame, about 36% of
  the image height. Feet rest on the same invisible line across each row.
- No text, no labels, no frame numbers, no ground lines, no shadows.
- No scenery or props: no ladders, ropes, walls or ledges. Only the character.
- Hands are empty closed fists. No weapons or held items.

ROWS:
Row 1, [animation name], [N] frames: [describe each frame in order]
Row 2, ...
```

## Lessons from the sheets so far

- **Ask for a transparent background.** Black swallows his dark outline, hair and boots and makes the cut-out risky. Grey works but is second best.
- **Say "no props" explicitly.** The first ladder frames came back with a ladder painted in and could not be used.
- **Say "empty closed fists" explicitly.** Items painted into the hands cannot be reused across animations.
- **Describe every frame, not just the animation.** "Alternate arms" came back with the same arm raised twice. Write "frame 1 left arm up, frame 2 right arm up".
- **Do not ask for exact pixel sizes.** Meshy does not honour them. A request for a 32 x 48 box produced a figure 137 x 307.
- **Keep to 12 to 16 frames a sheet.** More than that and the frames shrink and drift.
- **Meshy only animates the arm nearer the viewer.** Three sheets asked for the far arm to act ("front arm", "rear arm", and an inverted version) and all three came back with the near arm doing it. Do not spend requests on far-arm actions; design around it.
- **Items go on their own sheet**, drawn flat and side-on, one view per item, nothing overlapping.

## Checking a sheet when it comes back

1. Every frame present, in the order asked for.
2. Clear space between frames, nothing touching or cut off at an edge.
3. The character is a consistent size, and matches earlier sheets.
4. Nothing in the frame that was not asked for.

A sheet that fails 1 or 2 gets regenerated. Small failures of 3 can be corrected when the frames are cut out.

## Monsters (agreed 2026-11-19)

Monster sheets use the same preamble, with these two rules added to LAYOUT RULES:

```
- No effects and no particles: no dust, dirt, sparks, shards, drool, motion lines,
  impact bursts, stars or glow outside the body. Only the creature itself.
- Frames must not touch one another, even at the tips of limbs or crystals.
```

Then ask for the effects on a second sheet, so they can be reused, turned off or timed separately:

```
Effects sheet for a 2D side-scrolling platformer. One single image, 1536 x 864.
Detailed pixel-art style with a crisp dark outline where it suits. Transparent background.
Match the style and colours of the attached monster sheet.

SUBJECT: the effects for this monster's attacks and reactions, drawn on their own,
with NO creature in any frame.

LAYOUT RULES:
- Each row is one effect, its frames in order left to right.
- Equal-sized cells, evenly spaced, clear empty space between frames.
- No text, no labels, no ground lines.

ROWS:
Row 1, [effect, e.g. dust puff on landing], [N] frames: [describe each frame]
Row 2, ...
```

The first four monsters (assassin vine, fat rabbit, cave mole, crystal golem) came before this rule and have their effects painted in. They are kept as they are.
