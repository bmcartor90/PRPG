# Monsters

One folder per monster, sheets as received in `source/`. None are cut or in the lab yet: the lab has no monsters.

Both sheets so far have a mostly transparent background with a semi-transparent grey or white haze in it. Cutting at alpha above 240 drops the haze and leaves clean figures.

## Assassin vine (`assassin-vine/`)

8 frames in one row, each hanging from the same chunk of crystal ceiling rock (about 175 pixels wide). In order: coiled, eye opening, uncoiling with jaws, fully extended (about 580 pixels long), jaws open close to the ceiling, lunging strike, whipping back, coiled again.

- **Keep.** Consistent ceiling chunk and colours; matches the crystal wall set.
- The strike and whip frames have flying crystal shards and motion streaks painted in. Fine for an attack, but they cannot be turned off.
- The ceiling chunk is part of every frame, so the vine has to sit under a ceiling whose underside matches it.

## Fat rabbit (`fat-rabbit/`)

12 frames, facing right, two rows. Row 1: crouch (2), leap (2), landing, landing with a dust puff. Row 2: snarl with drool, rearing up and roaring, lunging bite, hit (red impact sparks), knocked back, knocked out on its back with circling stars.

- **Keep.** Consistent size (crouching about 190 pixels tall, against Lucca's 307 standing) and colours.
- Effects are painted in: dust, drool, impact sparks, stars.

## Cave mole (`cave-mole/`)

12 frames, facing right, opaque dark grey background (a flood fill from the border cuts it). Row 1: dirt mound, claws breaking out, emerging, crawling (2), burrowing back in. Row 2: standing upright, claw raised, swipe, digging down, hit (dizzy stars), knocked on its back.

- **Keep.** Consistent size (upright about 235 to 270 pixels tall) and colours.
- Its fur is nearly as dark as the background; the cut works but check the edges closely.
- Effects painted in: flying dirt, motion arcs, stars.

## Crystal golem (`crystal-golem/`)

12 frames, facing right, opaque dark grey background. Row 1: idle (2), walk (3), roar with crystals flaring. Row 2: fist raised, ground slam with a crystal burst, shard punch, guarding behind crystals, crumbling, rubble pile.

- **Keep, with care when cutting.** Frames 8 and 9 (slam and punch) touch through their effects, and so do 11 and 12 (crumbling and rubble). They need splitting by hand.
- Standing about 280 pixels tall, a little shorter than Lucca on the sheet. A golem probably wants to be bigger than the heroes; it will be scaled up when cut, which softens it.
- Effects painted in: crystal bursts, flying shards, sparkles.
