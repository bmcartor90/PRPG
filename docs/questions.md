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

15. **Sixths in the map file.** Proposal, approved by Brad: store every coordinate as a whole number of sixths (7 means 7/6 of a tile); the game converts on load. Please record it in the spec's map format.
16. **Floating rock.** Closed ground traces break today's rule that nothing is under ground. The lab needs new cliff logic as well as the underside (ceiling) rule, with recordings for both. Waiting on them.
17. **Doors use interact, not up.** Decided by Brad. Up also mounts ladders, so a door near a ladder would be ambiguous. Please update the brief and `design.md`.
18. **Big maps.** Engineering, no design change: the simulation will only check surfaces near him. Ties still go by file order; the traces will prove nothing changed.
19. **Top edge with no partner.** Until ceilings exist, it stops upward speed like an invisible lid. Brad says ceilings are coming soon; then it is just a ceiling.

## Stats

20. **Upper limit 200.** Brad: Body, Agility and Mind start at 80 minimum, 100 typical, with about 200 near the upper limit. `design.md` has 80 and 100 but not 200.
21. **How stats and gear change movement numbers.** Each movement number needs a rule (a curve) from Body / Agility / Mind and gear. Proposal: the curves live in data so Brad tunes them without code. With an 80–200 spread, the curve shape matters more than the range (a straight line would take jump height from 4 to 8). Needs drafting with Brad.
