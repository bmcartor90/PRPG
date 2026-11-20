# Changes to the lab and the spec

Newest first. Each entry says what changed and what a port of the simulation has to do about it.

## 2026-11-19, late night: crouching, crawling and low rock

Brad reversed an earlier decision: he must be able to get under anything with at least 1 tile of room, by sliding or crawling.

**Three earlier traces changed.** Everything else is unchanged tick for tick in the columns it had.

- `dodge-slide` from tick 93 and `dodge-slide-jump-and-edge` from tick 255: down is still held when the slide ends, so he now drops to a crawl instead of walking.
- `under-high-rock` from tick 103: the same.

**Every trace has one more column**, `low`, at the end of each state.

1. **Low state.** `p.low`, set at the start of `step`. The rule is in the spec under "The movement simulation". `heightNow` now reads it, and the ground roof test and the bottom test in `hitCliffs` use `heightNow`.
2. **`crawlSpeed`** (2 tiles a second) is a new number. `duckHeight` is unchanged but now means his height whenever he is low, not only in a slide.
3. **No jumping under low rock.** See `cramped` in `step`.
4. **The rule "he always counts at full standing height against rock" is gone.**
5. **The test map is 400 tiles wide**, with an eighth section: a slab 8 tiles long with its underside 1 tile up.
6. **New traces:** `crawl-under-low-rock`, `slide-under-low-rock`, `low-rock-no-jump-no-stand`, `crouch-walk-in-the-open`.
7. **New check to read:** `feel-lab/checks/low-rock.js`.
8. **Frame choice (step 2):** crawling uses the hands-and-knees get-up frame and crouching the crouch frame, until there are crawl frames.

## 2026-11-19, night: climbable cliff faces

All earlier traces are unchanged tick for tick. There are six new ones, all named `face-...`.

1. **Renamed:** the climbable kind `Wall` is now `SmoothRock`, and `multWall` is `multSmoothRock`. Same numbers, same behaviour.
2. **New kind:** `RoughRock`, with `multRoughRock` (1.0). It needs one free hand.
3. **Climbables can run down a cliff face.** A climbable with `openSide` is on a face. The rules are in the spec. In the lab: `climbX` and `climbTop` are new; getting on checks the open side; the top of a rope or rock face hands over to the ledge hang; stepping off at either end uses his body position, not the face.
4. **Sliding down** is now limited to ladders and ropes by name, not "anything but a wall".
5. **`handsNeeded`:** `SmoothRock` 2, `Rope` and `RoughRock` 1, `Ladder` 0.
6. **Frame choice (step 2):** every climbable on a face uses the side-view rope frames for now, facing the rock, with his lower hand pinned to the face. Proper frames will come from the art side.

## 2026-11-19, evening: floating rock

All earlier traces are unchanged tick for tick. There are six new ones.

1. **Floating rock.** A closed outline of solid rock, with surfaces on top, faces at the sides and undersides beneath. The rules are in the spec under "The movement simulation". In the lab: `makeSlab` turns an outline into surfaces, faces and undersides; `roofOver` finds the lowest underside over his body; `hitCliffs` gained a bottom test; `step` takes the undersides as an eighth argument and checks them in three places (end of the ground move, end of the air move, and the rise).
2. **`buildCliffs` ignores the surfaces of floating rock**, both as a source of cliffs and as "ground beyond". Floating rock supplies its own faces.
3. **Hitting his head** sets upward speed to zero and ends the jump's rise (`jumping` false, `cutting` true). It does not touch sideways speed.
4. **Map format:** a new `floatingRock` list. `_world.json` lists the derived `undersides`, and its `cliffs` now include faces with a real bottom.
5. **The test map is 384 tiles wide**, with a seventh section.
6. **New traces:** `head-bump`, `head-bump-while-running`, `floating-rock-face`, `floating-rock-corner-grab`, `overhang-too-low`, `under-high-rock`.
7. **Map coordinates** are whole numbers of a unit named in the file (`unitsPerTile`): 6 for editor maps, 60 for the test map. See "Map format".
8. **Doors open with interact, not up.**

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
