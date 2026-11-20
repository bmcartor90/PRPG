#!/usr/bin/env bash
# Rebuild every character atlas, the items, the fist labels and assets/*/atlas/ from the source sheets. Runs from any directory.
# Afterwards run `git diff --stat`: on unchanged inputs it should change nothing.
set -euo pipefail
T="$(cd "$(dirname "$0")" && pwd)"; py() { python3 "$T/$@"; }
L='walk,run,jump,slide,crouch,drawSword,drawShield,drawStaff,guard,walkDrawSword,walkDrawShield,runDrawSword,runDrawShield,nearShield'
N='walk,run,jump,slide,crouch,drawSword,drawShield,walkDrawSword,walkDrawShield,runDrawSword,runDrawShield'
py atlas.py
py atlas2.py nina
py atlas2.py ramorand
py items.py
py fists.py                   > /dev/null
py fists_nina.py
py fists_ramorand.py
py repo_sync.py
node "$T/smoke.js"
