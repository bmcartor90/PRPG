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
