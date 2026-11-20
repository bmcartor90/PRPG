# Answers to `questions.md`

Numbered to match the programmer's list. "Decided" means Brad has said so. "Recommended" means it is the design side's advice and Brad has not yet confirmed it; build to it, and expect it could change.

## Step 1: simulation

1. **Trace count. Decided.** 15 was right; the spec miscounted the map file as a run. There are 22 now. See `changes.md`.
2. **"Grounded" twice; is sliding anchored? Decided.** Keep both names as you have them (`OnGround`, `Grounded`). Sliding down a too-steep slope is **not** anchored. The dodge slide is anchored but **not safe**.
3. **Fixed numbers. Decided as an engineering matter.** They belong in data. The lab now names them in two tables, `BODY` (per character) and `WORLD`, exported in `_world.json` under `fixed`. Use those names. Whether any should differ between characters is Brad's call later; today they are the same for everyone.
4. **Respawn below the map. Decided.** In the game, falling into a bottomless pit sends you instantly back to the last save point, and maps are meant not to have such pits. The first build has no save points, so keep the lab's respawn for now.
5. **`movement.json` keyed by character. Decided.** Yes. Add Nina: identical to Lucca except `charHeight` 1.7.
6. **Jump trail and last-jump readout not ported. Decided.** Correct; they are lab instruments.
7. **Exactness across machines. Agreed with the programmer.** Accept it for now as a known limit. Store each slope's sine and cosine in the map file once the map editor writes those files, so nobody types them by hand.
8. **Random number generator and seed format. Decided as an engineering matter.** PCG32 with 64-bit seeds is fine. The display format is a placeholder until Brad designs how seeds are shared.
9. **Catch-up after a stall. Decided as an engineering matter.** Capping at 8 ticks a frame and dropping the rest is right. Never let the simulation race to catch up.

## Later questions

10. **Eight more numbers in the lab's code. Done.** They are in `BODY` and `WORLD` now; see `changes.md`.
11. **Order of surfaces. Done.** The lab now uses the same order as the map file: ground, then scaffolds.
12. **Map editor.** Built into the game as an editor mode, after the tuning panel. The brief is `map-editor-brief.md`.

## Items 15 to 21

15. **Coordinates as whole sixths. Decided.** In the spec's map format, with one addition: the file names its unit (`unitsPerTile`). Editor maps use 6. The lab's test map cannot be written in sixths (it has 331.6 and 5.75), so it uses 60.
16. **Floating rock. Done.** Cliff logic, undersides and six recordings are in the lab. See `changes.md`.
17. **Doors use interact. Decided.** The brief and `design.md` say so.
18. **Checking only nearby surfaces. Fine.** Ties must still go by file order.
19. **Top edge with no partner. Decided:** an invisible lid.
20. **Stat range. Decided:** 80 minimum, 100 typical, about 200 near the upper limit. In `design.md`.
21. **Stat curves. Not started.** Agreed that they are curves in data and that shape matters more than range. They will be drafted with Brad; until then every character uses the lab's numbers unchanged.
