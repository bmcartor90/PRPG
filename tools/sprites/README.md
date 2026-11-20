# Sprite tools

Scripts that turn Meshy sheets in `assets/` into the atlases the lab and the game load. They run from any directory; `paths.py` holds every location.

- `paths.py`: repo locations and the sheet-id table (`SHEETS`). Scripts refer to source sheets by these ids. **Add every new sheet here.**
- `build.sh`: rebuilds all atlases, items, fist labels and `assets/sprites/*/atlas/`, then runs the lab smoke test. On unchanged inputs `git status` should show nothing changed. Run it after any change to check you broke nothing.
- `work/`: working files (frame boxes, metadata, fist labels). Scratch output (contact sheets, screenshots) is ignored by git.

Fist positions are stored as coordinates in `fists.py` (Lucca), `fists_nina.py`, `fists_ramorand.py`. To label new frames: `python3 blobs.py <tag> <anim,anim,...> [character]`, read the numbered contact sheet in `work/`, write the coordinates in.

Other scripts: `char_boxes.py` / `nina_boxes.py` / `empty_boxes.py` / `slice3.py` find frames on a sheet; `mock.py` / `mock2.py <background> <out.png>` make still mock-ups; `shot16.py` drives the lab in headless Chrome and writes `work/look17.png`.

`splice.js` is not an art tool: it pastes the movement simulation into the lab, which belongs to the design conversation.

The handoff (`docs/handoff-sprites.md`) describes the whole job.
