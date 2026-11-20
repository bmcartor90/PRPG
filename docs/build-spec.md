# Build specification: first build

This is the brief for the programmer. Read it fully before writing code. `design.md` in this folder is the wider design; this file says what to build now.

## Who is who

- **Brad** is the designer and owns every design decision.
- **The programmer** (you) builds what this file describes.
- Design reasoning happens elsewhere and arrives as changes to `docs/`. If something here is unclear or looks wrong, do not guess: add it to `docs/questions.md` and carry on with the parts that are clear.

## What the first build is

One character running, jumping, climbing and handling his gear on one test map, with the foundations the rest of the game depends on built correctly underneath. It should feel the same as the feel lab.

**In:**
- All movement in the feel lab: walking, running, jumping, slopes and slides, stamina, ledge grab and pull-up, ladders, ropes and walls, the lunge, and drawing and sheathing each hand.
- The test map: the lab's level, rebuilt as a data file.
- Lucca drawn with his sprites and item overlays.
- Controller and keyboard input.
- Deterministic simulation, seeds, the frame counter, and save-and-restore of the whole simulation.

**Out. Do not build these, and do not leave stubs for them:**
- Battles, enemies, detection zones.
- The party, the leader swap, stats, levels, gear beyond the two hand slots.
- Imagine as a player feature. (Its foundation, save-and-restore, is in.)
- Hazards, towns, sleeping, saving to disk, menus, sound, more maps.

## Engine and layout

- **Unity, C#, 2D.** Use the current Unity LTS release and record the exact version in the top-level README.
- First target is Windows on Steam, played with a controller. Keyboard must work. PlayStation comes later; do nothing that blocks it.
- Proposed repository layout:
  - `sim/`: the movement simulation as a plain C# library with **no Unity references**, plus its tests. It must build and test with `dotnet test`, without Unity installed.
  - `game/`: the Unity project. It references the `sim/` source and only draws, plays sound and reads input.
  - `data/`: tuning numbers and maps as JSON.
  - `assets/`, `docs/`, `feel-lab/`: already here. Leave `feel-lab/` as it is.

## Rules that are not negotiable

1. **Fixed timestep.** The simulation advances exactly 60 times a second in steps of 1/60 s. It never reads frame time. Drawing runs at whatever rate the screen gives and blends between the last two simulation states.
2. **Simulation and drawing are separate.** The simulation knows nothing about sprites, cameras or Unity. Drawing never changes simulation state.
3. **No engine physics for the character.** No Rigidbody, no colliders for movement. The controller is our own arithmetic, ported from the lab.
4. **Deterministic.** The same starting state and the same inputs give the same result every time. Use `double` throughout the simulation. No `UnityEngine.Random`, no `System.Random`, no wall-clock time, no iteration over unordered collections.
5. **Input is sampled once per simulation step** into one small structure: direction (-1, 0, 1), run, jump held, jump pressed this step, up, down, grab, and which hand's draw key was pressed (none, left, right). The simulation sees only that structure.
6. **All tuning numbers live in data,** in `data/movement.json`, loaded at start. No tuning constants in code. The numbers belong to a character, not to the game, because other characters will have their own.
7. **Maps are data.** A map file lists platforms and climbables. See "Map format".
8. **The whole simulation state is one value that can be copied.** Copy it, run forward, put the copy back, and the game is exactly where it was.
9. **Do not change a number or a rule to make something easier to build.** Ask instead.

## The movement simulation

The lab is the reference. Port its behaviour, not its code style.

- **Where it is:** `feel-lab/index.html`, between `/*SIM-START*/` and `/*SIM-END*/`. About 450 lines. `step(player, params, input, lines, worldWidth, climbables)` advances one tick.
- **Units:** tiles and seconds. `y` is up. Speeds are tiles per second.
- **The numbers:** `feel-lab/traces/_world.json` has every parameter with the agreed starting values. These are what the game should feel like for a character with Body, Agility and Mind all at 100.
- **Surfaces** are line segments, flat or sloped. He lands on any of them from above. Slopes steeper than the walkable limit cannot be stood on; he slides. There are two kinds:
  - **Ground** is solid. Rock fills everything from the line down to the bottom of the map. Where ground ends or steps down there is a **cliff face** that blocks sideways movement for anyone whose feet are below its top. Nothing can be underneath ground.
  - **Scaffolds** are jump-through. He passes up through them and lands on top, and there is nothing underneath.
- **Cliff faces are derived, not authored.** `buildCliffs` in the lab works them out from the ground lines; port that rule. `_world.json` lists the result so the port can be checked. The top corner of a cliff is a ledge and can be grabbed and pulled up onto like any other.
- **Ceilings do not exist yet.** Ground always runs to the bottom of the map, so there is never anything to hit from below. Caves and overhangs will need them later; do not build them now.
- **States to preserve:** on the ground, in the air, hanging from a ledge, pulling up, climbing (ladder, rope or wall), prone after a missed lunge, exhausted. Plus what each hand holds and any draw or sheathe in progress.
- **Three terms from the design that the code should name explicitly,** because later systems depend on them:
  - *Anchored:* gravity is not acting on him (standing, hanging, climbing).
  - *Grounded:* anchored and standing on walkable ground.
  - *Dynamic action:* anything that is not grounded.

### How to know the port is right

`feel-lab/traces/` holds 20 recorded runs of the lab's simulation. Each file gives a starting position, the inputs for every tick, and the resulting state for every tick (position, velocity, stamina, mode, grounded, and what each hand holds). `_world.json` has the parameters and the test map they ran on.

- Write a test that replays each trace's inputs through the C# simulation and compares every tick.
- Positions, velocities and stamina must match to within 0.000001. Mode, grounded and hands must match exactly.
- If a trace drifts by a hair and then diverges at a decision point, that is a real difference in arithmetic order. Find it; do not widen the tolerance.
- To record more traces, add a scenario to `feel-lab/checks/make-traces.js` and run it with Node.

The other scripts in `feel-lab/checks/` print measurements for a person to read. They are useful for understanding intended behaviour but are not tests.

### Tests the simulation must also pass

- **Repeatable:** run any trace twice from the same start; every tick is identical.
- **Save and restore:** at an arbitrary tick, copy the state, run 300 more ticks, restore the copy, run the same 300 ticks of input again; the two runs are identical tick for tick.
- **Frame rate independence:** the result does not depend on how often the screen draws.

## Seeds and the frame counter

Nothing in the first build uses randomness, but these must exist in the simulation state from the start, because adding them later means touching everything:

- A **run seed** and a **battle seed**, both settable and both shown somewhere a player could copy them from.
- A **frame counter** that counts simulation ticks for the whole run and never resets.
- Our own small random number generator, written in `sim/`, whose output for a given seed is fixed forever. Its state is part of the simulation state, so save-and-restore covers it.

## Map format

A JSON file. The test map is `feel-lab/traces/_world.json` under `world`; copy it into `data/maps/` in this shape:

```json
{
  "width": 292,
  "ground": [[x1, y1, x2, y2], ...],
  "scaffolds": [[x1, y1, x2, y2], ...],
  "climbables": [{"kind": "Ladder", "x": 262, "y1": 0, "y2": 8}, ...]
}
```

- Coordinates are in tiles and may be fractional. A platform at height 1.66 is legal.
- `ground` lines are solid and filled beneath; `scaffolds` are jump-through.
- `kind` is `Ladder`, `Rope` or `Wall`. Each kind has a stamina cost multiplier in the parameters.
- The format will grow (areas, monsters, hazards). Keep the loader simple and tolerant of fields it does not know.

## Drawing

- **View:** 24 tiles wide by 13.5 tall on every display, scaled to fit, with the camera following Lucca.
- **Scaling:** smooth, never snapped to whole pixels. The art is a pixel style but is not on a pixel grid. He stands 1.9 tiles tall; `standingHeightPx` in the atlas JSON is his height in atlas pixels.
- **Sprites:** `assets/sprites/lucca/atlas/`. The README one folder up explains every field, including the foot anchor, the hand anchor for ledge and rope frames, and the fist positions.
- **Which frame when:** `pickFrame` in `feel-lab/index.html` is the reference. Port its choices.
- **Items:** `assets/sprites/items/`. The sword and shield are separate pictures pinned to his fists. Read "Which hand is which" and "Drawing and sheathing" in the Lucca README before touching this; the handedness rule is easy to get backwards. The lab's drawing code just after `pickFrame` is the reference.
- Surfaces can be plain white lines for now, as in the lab: dashed for too steep to stand on, ground filled in dark beneath with its cliff faces drawn, scaffolds with thin posts. Tile art comes later.
- **Background:** leave a hook for layers that scroll slower than the camera. The far layer will be a zoomed-out painting that moves at a small fraction of camera speed. No art for it yet; a flat colour is fine.

## Controls

Layouts are in `design.md` under "Controls". The controller layout is locked. Every binding must be rebindable later, so route input through named actions, not raw buttons.

## Order of work

1. **Simulation.** `sim/` in plain C#, the parameter and map loaders, and all the tests above passing. Nothing visual. This is done when `dotnet test` is green on every trace.
2. **On screen.** The Unity project: the test map, Lucca with his animations and items, the camera, controller and keyboard.
3. **Tuning panel.** A way for Brad to change the numbers while playing and write them back to `data/movement.json`. Simple is fine.

Stop after each step and say what was built, what was not, and anything that looked wrong.

## Working rules

- Small commits with plain messages. Tests pass before every commit.
- Keep `docs/questions.md` for anything you had to assume. One line each, with what you assumed.
- Do not edit `docs/design.md`, `feel-lab/` or `assets/`. If one of them is wrong, say so in `docs/questions.md`.
- When reporting to Brad, be brief and blunt, and lead with what works and what does not.
