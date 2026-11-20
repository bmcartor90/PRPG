# Map editor: brief for version one

For the programmer. This says what the editor must let Brad do. How it is built is yours to decide. Build it after the tuning panel.

## What it is

An editor mode inside the game itself, in the same download Brad already runs. He draws a map, presses a key, and is playing it with the real movement. No separate tool, no Unity on his machine. It saves the same map files the game loads.

He edits **one map at a time**. Maps connect through doors and edges, described below, but the editor never needs two maps open.

## The map

- Up to **344 by 344 tiles**: about 14 screens wide and 25 tall. Maps are tall, and a lot of play is vertical climbing. The game camera must follow up and down as well as sideways.
- Positions snap to **sixths of a tile**. Store them so sixths stay exact; do not round them to decimals.
- The editor writes each slope's angle values into the file, so nobody types them (see `answers.md`, item 7).

## What Brad places

1. **Ground.** He clicks points along a surface, like tracing the outline of a hill.
   - An **open** trace is ground that fills all the way down to the bottom of the map.
   - A **closed** trace is a floating mass of rock with an underside.
   - The fill, the cliff faces and the undersides are worked out from the trace. He never draws them.
   - Undersides are a new movement rule (you bump your head). It is being added to the feel lab first; `changes.md` will say when it and its recordings are ready. Do not invent the rule.
2. **Scaffolds.** A line he can jump up through.
3. **Climbables.** Ladder, rope or wall: where it is, its bottom and its top.
4. **Doors.** Placed on a surface. Each has:
   - a name;
   - a width. You can go through from anywhere within it. The width comes from the door art and is not settled yet, so make it a number in the map file, not a constant;
   - the map and the door it leads to. That map usually will not exist yet, so the editor shows the door as unlinked and lets him carry on;
   - a locked mark, with a short note of what opens it. For now a locked door simply does not open; the plot logic comes later.
5. **Edge exits.** Any of the four edges of a map can be paired with the opposite edge of another map, the same way doors are paired. Walk off the right edge of one and you arrive at the left edge of the other.
6. **The start point.**

## Doors and edges in play

- Press **up** while standing on the ground at a door to go through it. You arrive standing at its partner.
- Doors are an escape: you can go through one even while a monster has noticed you.
- Going through a door or an edge counts as leaving the map.
- An edge with no partner: left and right are walls; the bottom is a pit, which sends you back to the last save point. *(The top is assumed closed. Brad has not said.)*

## Snapping and measuring

- Snap to sixths by default. Hold a key to place freely. Typing an exact number always works.
- Optional snap for slopes to the three clean grades: 1 in 2, 1 in 1, 2 in 1.
- Slopes too steep to stand on are drawn dashed, as in the lab.
- A ruler: pick two points and read the distance across and the height difference, in tiles.

## The reach overlay

The rule for every map is that it can be cleared at minimum abilities. So from any spot Brad picks, the editor shows where the character can get to:

- a standing jump and a running jump;
- the highest ledge he can catch, and how far a lunge reaches;
- for the character and numbers currently loaded.

Work it out by running the real simulation from that spot, not from a formula, so it can never disagree with the game. Later there will be a "minimum abilities" set of numbers to show instead; nothing defines it yet.

## Trying the map

- One key drops him into the map at the cursor, playing. The same key returns him to editing, where he was.
- A second way in: play from the start point.
- The character menu works here too.

## Getting around

A map is about 350 screens. He needs to pan, zoom out far enough to see all of it, and jump quickly to a place.

## Editing

- Select, move, delete, duplicate.
- Undo and redo without limit.
- Save often and automatically; never lose work to a crash.

## Warnings

Warn, do not block: ground traces that overlap; a door not standing on a surface; a door or edge with no partner; a climbable with nothing to step onto at its top; no start point.

## Not in version one

Monsters, areas, hazards and sleep sites. Hand-placed art; surfaces get their art automatically. A view of how maps join. What actually unlocks a locked door.

## Still open with Brad

- What the top edge of a map does when it has no partner.
- Whether an edge exit is the whole edge or a marked stretch of it.
- How wide a door is. Decided that it has a width; the number is waiting on the door art.
