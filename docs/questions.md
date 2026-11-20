# Questions and assumptions

One line each: the question, and what the code assumes until Brad answers. Answers live in `answers.md`.

## Step 1: simulation

1. **Trace count.** Answered: 22. All 22 pass.
2. **"Grounded" twice.** Answered: `OnGround` (lab) and `Grounded` (design). Sliding is not anchored (recommended; awaiting Brad). Built that way.
3. **Fixed numbers.** Answered. The lab's `BODY` numbers are in `data/movement.json` with each character's other numbers; `WORLD` numbers are in `data/world.json`.
4. **Respawn below the map.** Kept as is (recommended; awaiting Brad).
5. **`movement.json` keyed by character.** Answered. Nina added: Lucca's numbers with `charHeight` 1.7.
6. **Jump trail and readout not ported.** Answered.
7. **Exactness across machines.** Still open. The spec's map format changed this round, which `answers.md` named as the moment to store each slope's sine and cosine in the map file. Not done yet: it would mean hand-written maps carry computed numbers that must agree with the coordinates. Proposal: do it when a map editor or exporter writes the map files, so no person types them. Until then it is a known limit: on another platform, sloped lines may differ in the last bit.
8. **Seed format.** Answered: placeholder.
9. **Catch-up after a stall.** Answered.

## Step 1, second round (cliffs, scaffolds, dodge slide)

10. **Remaining numbers in code.** Answered: all in data now. Only arithmetic and two rounding guards are left, as in the lab.
11. **Line order.** Answered: ground, then scaffolds, as in the file.
12. **Trace count in the lab README.** Fixed by the design side.

## Step 1, third round

13. **Ramorand's movement numbers.** Nothing gives them, so `movement.json` has him identical to Lucca (the lab shows him at 1.9 tiles). Confirm or give his numbers.
14. **"Safe".** The dodge slide is anchored but not safe. There is no safety state in the simulation yet (it needs threats), so nothing marks this beyond `InDynamicAction` being true during a slide. Will add `Safe` when monsters exist.

## Map editor brief (`map-editor-brief.md`)

15. **Sixths in the map file.** Done: `unitsPerTile` in the file (60 for the test map, 6 for editor maps); the loader converts.
16. **Floating rock.** Done: ported with undersides and faces; all 28 traces match bit for bit.
17. **Doors use interact, not up.** Decided and recorded.
18. **Big maps.** Engineering, no design change: the simulation will only check surfaces near him. Ties still go by file order; the traces will prove nothing changed.
19. **Top edge with no partner.** Until ceilings exist, it stops upward speed like an invisible lid. Brad says ceilings are coming soon; then it is just a ceiling.

## Stats

20. **Upper limit 200.** Brad: Body, Agility and Mind start at 80 minimum, 100 typical, with about 200 near the upper limit. `design.md` has 80 and 100 but not 200.
21. **How stats and gear change movement numbers.** Each movement number needs a rule (a curve) from Body / Agility / Mind and gear. Proposal: the curves live in data so Brad tunes them without code. With an 80–200 spread, the curve shape matters more than the range (a straight line would take jump height from 4 to 8). Needs drafting with Brad.

## Step 2: on screen

22. **Unity version.** The spec asks for the current LTS. Brad has 6000.6.5f1 installed, so the project uses that to avoid a second install. Whether 6.6 is an LTS release I have not confirmed. Fine to move to an LTS later; the project is small.
23. **Builds run on Brad's PC, not GitHub.** Unity's free licence is tied to one machine and cannot be used on a cloud build machine. `build.bat` builds with the installed Unity and no window.
24. **Development keys.** 1 to 7 (sections), T (retry), Tab (character), F1, F5 and Esc are not in the design's layout. They are for testing and come out when menus exist.
25. **The character is a setting,** remembered between runs and changed with Tab. Changing character keeps the position and swaps movement numbers and art.
26. **Seeds** are random at start-up and shown in the help line (F5 copies them). There is no way to type one in yet.

## Map editor, version one

27. **First map size.** Brad asked for 1.5 screens tall and 3 wide: 72 by 20.25 tiles. New maps start at 72 by 20, because 20.25 tiles is 121.5 sixths, not a whole number. The panel resizes in steps of 12 tiles wide and 6 tall.
28. **Not in version one:** doors and edge exits (Brad: not needed yet), typing exact numbers, slope angle values in the file, the top-edge lid, and a "minimum abilities" set for the reach overlay.
29. **Map height** is a new field in the map file (`height`), and the start point is `start: [x, y]`. The game's camera stops at the top of a map that has a height.
30. **Ground traces** are stored in the file too (`groundTraces`) so the editor gets back exactly what was drawn; the game reads only `ground`.
31. **Reach overlay** shows standing and running jumps both ways and a straight-up jump, all with grab held, so they stop on the first ledge or climbable they reach. The running jump starts at full run speed, as if after a run-up. The lunge is not drawn yet.
