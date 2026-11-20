# PRPG

A 2D sidescroller platforming RPG. Working title.

## What is here

- `docs/build-spec.md`: what to build first, and the rules for building it. **Programmers start here.**
- `docs/design.md`: the wider design.
- `docs/map-editor-brief.md`: what the map editor must do.
- `docs/handoff-sprites.md`: the brief for whoever brings new art into the project.
- `docs/changes.md`, `docs/answers.md`: what has changed for the programmer, and answers to his questions.
- `tools/sprites/`: the scripts that cut sprite sheets into frames.
- `game/`: the Unity project (Unity **6000.6.5f1**). It draws, reads input and runs the simulation; it holds no movement rules.
- `build.bat`: builds `build/PRPG.exe` with the installed Unity, no Unity window needed. Logs go to `build-logs/`.
- `data/characters.json`: which art each character uses.
- `sim/`: the movement simulation in plain C# (no Unity), and its tests. Run `dotnet test` in `sim/` (needs the .NET 8 SDK).
- `data/movement.json`: each character's movement numbers (Lucca, Nina, Ramorand). `data/world.json`: numbers that belong to the game. `data/maps/test.json`: the test map.
- `docs/questions.md`: open questions and what the code assumes until they are answered.
- `feel-lab/`: the browser prototype used to tune movement, and scripts that measure it.
- `assets/sprites/characters.md`: what each character looks like, for briefing the art tool in a new chat.
- `assets/sprites/prompt_template.md`: the prompt format for requesting a sprite sheet, and what has gone wrong before.
- `assets/sprites/references/`: one reference picture per character.
- `assets/sprites/<name>/`: each character's sprite sheets and the frames cut from them (Lucca, Nina, Ramorand).
- `assets/sprites/items/`: swords, shields and other gear, cut out and scaled.
- `assets/tiles/`, `assets/backgrounds/`: platform pieces and background paintings.

## Building the game (Windows)

1. Pull the latest code.
2. Double-click `build.bat`. The first run takes several minutes while Unity imports the project; later runs are quicker.
3. It opens the `build` folder. Run `PRPG.exe`.

In the game: arrows move, Space jumps, D sprints, F grabs, Q and W draw or sheathe the left and right hand. A controller works too (layout in `docs/design.md`). Number keys 1 to 7 jump to the test map's sections, T restarts the section, Tab changes character, F5 copies the seeds, F1 hides the help, Esc quits.
