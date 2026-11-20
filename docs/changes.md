# Changes to the lab and the spec

Newest first. Each entry says what changed and what a port of the simulation has to do about it.

## 2026-11-19, later the same day

All 22 traces are unchanged tick for tick.

1. **The remaining bare numbers are in the tables.** `BODY` gained `runDrainAbove`, `crouchBelow`, `minUphillShare`, `pullSlideFrom`; `WORLD` gained `cliffProbe`, `cliffLevel`, `cliffTop`. What is left in the simulation code is arithmetic, two rounding guards (`1e-6` in `isSteep`, `1e-9` in landing), and drawing-only values.
2. **Surface order is now defined.** The lab checks ground first, then scaffolds, each in the order listed, which is the order a map file gives them. Rare exact ties go to the earlier surface.
3. **A third character, Ramorand,** with an atlas. He has no moving-draw frames, so frame choice must cope with an animation being absent.
4. **Each character carries their own gear.** `assets/sprites/items/items_atlas.json` says who uses which item.
5. **Decided by Brad:** sliding down a too-steep slope is not anchored; the dodge slide is not safe; a bottomless pit sends you back to the last save point (see `answers.md`).

## 2026-11-19, after the first simulation port (branch `sim-port`, built on commit 64b9da0)

All 15 traces that existed at that commit are unchanged tick for tick. Everything below is an addition.

1. **Map format.** `lines` is replaced by `ground` and `scaffolds`. Ground is solid; scaffolds are the jump-through lines the port already handles. See "Map format" in the spec.
2. **Cliff faces.** Solid ground gets a blocking face wherever it ends or steps down. They are derived from the ground lines by `buildCliffs` and applied by `hitCliffs`, called after each sideways move on the ground and in the air. `_world.json` lists the derived cliffs to check against.
3. **New number: `bodyWidth`** (0.6 tiles), used by `hitCliffs`.
4. **Dodge slide.** New numbers `duckTime`, `duckKeep`, `duckCost`, `duckHeight`. New state: time left in the slide, and whether down was held last tick. New trace column `dodgeSliding`.
5. **Fixed numbers are named.** The loose constants in the lab's simulation are gathered into `BODY` and `WORLD` and exported in `_world.json` under `fixed`. Behaviour is unchanged. Move the ported constants into data under these names.
6. **The test map grew** from 292 to 344 tiles wide, with a sixth section of cliffs, a pit, a scaffold over solid ground, and a ladder against a cliff.
7. **Traces: 22**, up from 15. New: `cliff-face-blocks`, `cliff-grab-and-pull-up`, `pit-and-respawn`, `scaffold-and-cliff-ladder`, `gap-with-walls`, `dodge-slide`, `dodge-slide-jump-and-edge`.
8. **A second character, Nina,** with a complete atlas. `movement.json` keyed by character is the right shape.
9. **Lab frame choice:** `pickFrame` now shows the slide frames during a dodge slide, and draw frames only for the arm nearer the viewer. This matters for step 2, not step 1.
