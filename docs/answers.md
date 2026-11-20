# Answers to `questions.md`

Numbered to match the programmer's list. "Decided" means Brad has said so. "Recommended" means it is the design side's advice and Brad has not yet confirmed it; build to it, and expect it could change.

## Step 1: simulation

1. **Trace count. Decided.** 15 was right; the spec miscounted the map file as a run. There are 22 now. See `changes.md`.
2. **"Grounded" twice; is sliding anchored? Recommended.** Keep both names as you have them (`OnGround`, `Grounded`). Sliding down a too-steep slope is **not** anchored: gravity is acting on him and there is nowhere stable to put him back after a fight. Awaiting Brad.
3. **Fixed numbers. Decided as an engineering matter.** They belong in data. The lab now names them in two tables, `BODY` (per character) and `WORLD`, exported in `_world.json` under `fixed`. Use those names. Whether any should differ between characters is Brad's call later; today they are the same for everyone.
4. **Respawn below the map. Recommended.** Keep it as it is for the first build. In the game a bottomless pit will be a hazard: the party takes hazard damage and the leader returns to the last safe ground. The damage half needs systems that are not in this build. Awaiting Brad.
5. **`movement.json` keyed by character. Decided.** Yes. Add Nina: identical to Lucca except `charHeight` 1.7.
6. **Jump trail and last-jump readout not ported. Decided.** Correct; they are lab instruments.
7. **Exactness across machines. Recommended.** Accept it for now, and record it as a known limit. It will matter if recorded runs are ever replayed on another platform (speedrun verification, shared Imagine states). The fix you describe, storing each slope's sine and cosine in the map file, is cheap; do it when the map format next changes. Awaiting Brad.
8. **Random number generator and seed format. Decided as an engineering matter.** PCG32 with 64-bit seeds is fine. The display format is a placeholder until Brad designs how seeds are shared.
9. **Catch-up after a stall. Decided as an engineering matter.** Capping at 8 ticks a frame and dropping the rest is right. Never let the simulation race to catch up.
