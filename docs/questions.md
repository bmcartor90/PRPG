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

10. **Numbers still written into the lab's code.** Not in `BODY` or `WORLD`, so ported as constants, unchanged: cliff derivation looks 0.01 beyond a ground end and treats ground within 0.05 as continuing; a cliff only blocks feet more than 0.02 below its top; he counts as running above walk speed + 0.1 (the dodge slide uses `sprintShare` instead); crouching needs speed under 0.2; uphill top speed never drops below 10%; a pull-up starts moving him sideways after 60% of it. Should these join the tables?
11. **Line order.** The lab keeps one list of lines in the order the level code made them, mixing ground and scaffolds, and that order settles ties. The map file splits ground from scaffolds, so the code numbers ground first, then scaffolds. All 22 traces still match exactly. Assumed fine; a map with exact ties between a ground line and a scaffold could in principle behave differently from the lab.
12. **`feel-lab/README.md` says 20 traces;** there are 22. Not edited, since `feel-lab/` is off limits to me.
