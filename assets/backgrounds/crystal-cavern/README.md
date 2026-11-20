# Crystal cavern background

## Far layer (use this one)

`Meshy_AI_crystal_cavern_far_matte.png`: 1536 x 864, as received. A cavern seen from far away: small, hazy, nothing at the scale of a character. This is the slow-scrolling far layer.

- It did not repeat cleanly as delivered: the left and right edges differed by about four times the normal column-to-column change.
- `far_matte_blue_repeating.png` and `far_matte_violet_repeating.png` are the fixed versions, 1344 x 864. The last 192 columns are cross-faded into the first 192, so copies placed side by side show no join.
- The violet one has its blues moved to a dark muted violet so a navy character stands out. It came back blue although violet was asked for.

Mock-ups: `docs/mockups/far-matte-mockup-1080p.png` and `far-matte-mockup-violet-1080p.png`.

## First attempt (not suitable as delivered)

`Meshy_AI_crystal_cavern_bg_matte.png`: 1536 x 864, one screen, opaque.

`crystal_cavern_bg_violet.png`: the same image recoloured. Its blues are moved to a dark, muted violet so a navy character stands out against it; the warm torch colours are untouched. Made by a hue shift, not by Meshy. Compare the two mock-ups in `docs/mockups/`.

## Limits

- **It is one screen wide and does not repeat.** Maps are many screens wide. As it stands it can only be a fixed backdrop or a very slow-scrolling far layer.
- **It contains things that read as platforms.** The torch-lit ledge at top right, the stone bridge and the stepped ledges all look standable. A player will try to jump to them.
- **The torches are the brightest thing on screen** and pull the eye away from the play area.
- Filling a 4K screen means enlarging it 2.5 times. Softness is acceptable for a far layer, not for anything near the play area.

## What a usable set looks like

Three separate layers (far, middle, near), each repeating seamlessly left to right, with no ledges, bridges or floors in the band where play happens, and with the bright accents kept small and dim.
