# PRPG

A 2D sidescroller platforming RPG, platformer first. Working title.

Before writing any code, read these in order:

1. `docs/build-spec.md`: what to build now, the rules that are not negotiable, and the order of work.
2. `docs/design.md`: the wider design. Most of it is not in the first build.
3. `assets/sprites/lucca/README.md`: how the sprite data works.

## Standing rules

These rules are for the programmer building the game. `docs/`, `feel-lab/` and `assets/` are maintained separately, at Brad's direction, by the design work that produces them.

- Brad is the designer. Do not make design decisions or change tuning numbers. Put questions and assumptions in `docs/questions.md`.
- The simulation in `sim/` is plain C# with no Unity references, deterministic, and runs at a fixed 60 steps a second. Tests run with `dotnet test`.
- `feel-lab/` is the reference for behaviour. Do not edit it. `feel-lab/traces/` are the recordings the simulation must reproduce.
- Do not edit `docs/design.md` or `assets/`.
- Report briefly and bluntly: what works, what does not, what looked wrong.
