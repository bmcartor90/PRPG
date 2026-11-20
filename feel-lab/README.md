# Feel lab

A one-page browser prototype for tuning how movement feels. Open `index.html` in a browser; it needs `sprites.png` and `items.png` beside it.

It is a test bench, not the game. Its levels are there to measure things, and its code is not meant to be carried into the real build. What should carry over is the behaviour and the numbers.

## Where the physics is

Inside `index.html`, everything between `/*SIM-START*/` and `/*SIM-END*/` is the movement simulation. It runs at a fixed 60 steps a second and does not touch the screen, so it can be run on its own.

## Traces

`traces/` holds 20 recorded runs of the simulation: the inputs for every tick and the resulting position, velocity, stamina and state. `traces/_world.json` has the parameters and the map they ran on. A port of the simulation is correct when it reproduces these. `checks/make-traces.js` regenerates them and is where to add scenarios.

## Checks

`checks/` holds scripts that pull the simulation out of the page and run scenarios through it. Run one with `node checks/slopes.js`.

They print measurements (how far a jump carries, how much stamina a climb costs). They do not pass or fail on their own; someone has to read the numbers. Turning them into real pass/fail tests is a job for the first build.

- `slopes.js`: sliding, running onto and off slopes, launching from a ramp.
- `feet-and-gaps.js`: foot width, walking off edges, widest gap cleared.
- `stamina-and-climbing.js`: stamina, ledge grabs, pull-ups, ladders, ropes, walls, lunge, hands.
- `cliffs.js`: solid ground, cliff faces, the pit, a scaffold, and a ladder against a cliff.
- `page.js`: loads the whole page with a stand-in browser and checks it starts and picks sprite frames.
