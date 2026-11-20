# Questions and assumptions

One line each: the question, and what the code assumes until Brad answers.

## Step 1: simulation

1. **Trace count.** The spec says 16 traces; `feel-lab/traces/` has 15 runs plus `_world.json`. Assumed 15 is right; all 15 pass.
2. **"Grounded" in the lab vs the design.** The lab's `grounded` is true while sliding down a too-steep slope, and the traces record it that way. The code calls that `OnGround` and uses `Grounded` for the design term (standing on walkable ground). Assumed **sliding is not anchored**, since gravity is acting on him. Confirm.
3. **Fixed numbers that are not in `movement.json`.** The lab hard-codes these, so the port does too, unchanged: body radius 0.45, ground-snap distance 0.3, hang offsets 0.3 out / 0.4 in, ledge catch 0.8 outside / 0.6 under, ledge-end join 0.05, climbable reach 0.6, step-off tolerance 0.15, jump off a climbable at 0.8 × run speed, grab lock 0.3 s after letting go and 0.45 s after jumping off, hang ease 35% per tick, sliding down a rope or ladder at 3 × climb speed, winded until 25% stamina, pull-up moves sideways in its last 40%, respawn below y = -7. Should any of these become per-character numbers (the ledge catch distances look like they could be)?
4. **Respawn below y = -7** is a lab convenience. Ported as is. Keep it for the game, or replace it later with the faint/fall rules?
5. **`movement.json` shape** is `{"characters": {"Lucca": {...}}}` so each character has its own numbers. Assumed that is fine.
6. **Not ported:** the lab's jump trail and "last jump" readout. They only feed the lab's on-screen measurements and do not affect movement.
7. **Cross-machine exactness.** Slope angles use the platform's sin, cos and atan2 once, when a map loads. On Linux .NET the port matches the lab bit for bit. Windows or PlayStation could differ in the last bit on sloped lines. That does not affect how it feels; it would matter only if a recorded run must replay identically on another machine. Assumed not needed yet; fixable later by computing those values ourselves.
8. **Random number generator:** PCG32 with 64-bit seeds, shown as `RUN-` / `BTL-` plus 16 hex digits. The format is a placeholder and can change until someone shares a seed.
9. **Long stalls:** if the screen freezes, the simulation catches up at most 8 ticks per frame and then drops the rest of the lost time rather than racing. Assumed fine.
