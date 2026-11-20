# Platformer RPG: design concepts

Working notes for Brad's 2D sidescroller platforming RPG. First recorded 2026-11-19; updated the same day through discussion items 1 to 43.

Everything here is a concept as Brad stated or agreed it. Items marked **Open** are explicitly undecided. Movement feel numbers (gravity, jump, run, slopes) are tuned separately in the Platformer Feel Lab and are not recorded here.

## The goal and the two modes

The game is built from **blocks** of content. Imagine about **14 blocks** in total.

- **The epic (flagship mode):** runs you through all the content, every block.
- **The randomizer:** a run of about **1 hour, maybe 90 minutes**, that randomly includes **4 or 5** of those blocks. It is not the flagship. It exists because it is what will get streamers playing the game.

**Scale:** the epic is expected to be roughly a 15-hour RPG on a first playthrough. That is the intended size, and it counts as lofty.

### What the randomizer randomizes

- The **order** in which you encounter blocks.
- **Where monsters are** within those blocks.
- The **order in which you unlock gear and abilities**.

What that asks of the player:

- You need to know which routes are possible with a high jump and with a low jump, because you don't know when you will get it.
- You need to know how to clear each puzzle when it holds a Type A monster or a Type B monster, which have different threat detection rules.

### Monster placement rules

- Placement is not "anything anywhere." Each spot needs **valid type** rules for which monsters can appear there.
- It is fine for a puzzle that is skippable in the epic to become unskippable in the randomizer because of the monster rolled there. Not all fights should be skippable.

### Scaling by position

- Monster stats scale with the position a block appears in, not with the block itself. If the randomizer rolls block 12 as the first block, it is the block 12 map with block 1 monster stats.

What holds for both:

- **Speedrunning matters.** Skipping encounters through agility, teleportation and similar means is **intentional design**.
- There will be **unskippable encounters**, so a player cannot fully ignore combat.
- **Seeds:** the run seed and the battle seed are shareable and fixed, from day one.

Superseded: an earlier version of this section named the 1-hour randomizer as the ultimate goal and said the game was not about epic storytelling. Brad has lofty dreams for the game that do not fit in a 1-hour run, so the epic is now the flagship.

## Structure: blocks, maps, areas

- **Blocks** are made of **maps**.
- **Maps** are made of **areas**.
- An area might have a monster assigned to it.

## Wild and returned

- Each area has what it looks like **wild**, and what it looks like once it has been cleared and **returned** to. New monsters move into the empty space, but not immediately.
- An area, or the monster assigned to it, might have a "returned" condition.
- This is not gated on time. The two states, wild and returned, are enough.
- A returned area should typically be easier and more sparse.

The rule:

- While you are in a map, an area you clear **stays cleared**.
- If you leave that map and come back, you face every **wild** threat you left uncleared, and every **returned** threat for the areas you had cleared.
- Going to town and going to a sleep site **always count as leaving the map**.
- A returned threat **comes back again** after you clear it. Every time you leave and re-enter, the areas you have cleared hold their returned threat again. This is what makes gold and experience farmable.

## Failing a jump

- **The default is falling to a lower route.** A missed jump drops you onto a slower way through, not into a pit.
- A failed jump should often mean you **hit the monster and now must fight it**.
- There could also be things that "merely" damage you if you hit them.
- **Bottomless pits:** falling into one sends you instantly back to the last save point. The intent is that maps should not have them.

## Hazards

- A damaging hazard hurts **everybody in the party**, by a relatively small amount.
- Wading through spikes is a valid strategy if you are willing to pay the cost.
- A hazard **can finish off a character**, and therefore a whole party.
- **If your leader faints,** you must immediately select a new leader. "Immediately" follows the same rule as starting a fight: at your next anchor, not during a dynamic action. (This is a forced change, so it does not wait for "no threat.")

## Towns and going home

- Each block has a **starting town**.
- "Between blocks" basically means at the town. That is where buying happens.
- **Go Home** should typically be an option. It returns you to the current block's starting town.
- Going home is a real cost: afterwards you return to the **beginning of the dungeon**, not to where you left.
- Going home requires "no threat."

## When the whole party faints

- **Where you resume, and what you lose:** it works like resetting to a save point. You go back to the last time you slept, with the game state as it was then. Towns and campsites are both Big Safe, so a mid-block sleep at a campsite counts.
- **Tentative:** it may be the last time you were *allowed* to sleep, whether or not you did.
- Superseded: an earlier answer had you resume at the block's starting town, with an unlockable ability moving that to the campsite.
- **In the randomizer:** it costs time; it does not end the run. Players can govern their own hardcore mode.
- **Revival:** revive consumables should exist.

## Controls

### Controller (locked)

| Input | Use |
|---|---|
| Left stick or d-pad | Move, climb, crouch |
| Bottom face button | Jump |
| Left face button | Run (hold) |
| Right face button | Talk and interact |
| Top face button | Open the menu |
| Right trigger | Grab (hold) |
| Left shoulder | Left hand: draw or sheathe |
| Right shoulder | Right hand: draw or sheathe |
| Left trigger | Imagine |
| Right stick, stick clicks | Free |
| Start, Select | Pause, map |

Rules that go with it:

- **Interact is its own button, not grab,** because grab is held constantly while platforming.
- **Physical interactions need a free hand** (opening a chest, pulling a lever, picking something up). Talking never does.
- **The battle scene has its own layout** for confirm and cancel, to be decided with the battle system.

### Keyboard

Arrow keys on the right hand, actions under the left.

| Key | Use |
|---|---|
| Left and right arrows | Move |
| Up arrow | Climb |
| Down arrow | Crouch |
| Space | Jump |
| D | Sprint (hold) |
| F | Grab (hold) |
| A | Talk and interact |
| S | Imagine |
| Q | Left hand: draw or sheathe |
| W | Right hand: draw or sheathe |
| C | Open the menu |

- The hand keys are Q and W, not E and R, so that drawing never means lifting a finger off sprint (D) or grab (F).
- Z is free. Pause and map have no keys assigned yet.
- **To check before this is final:** arrow keys held with Space is a combination some keyboards drop. Test a running jump and a lunge (an arrow, D, F and Space together) on each keyboard in use.
- The shipped game should offer full rebinding and a second preset.

## Art and view

- **Art approach (decided):** keep the current sprite style and drop the strict pixel grid. The sprite sheets are not true indexed pixel art to begin with, which is why they came out at a different size than the 32 × 48 box that was requested. They are treated as high-resolution illustrations in a pixel style and scaled smoothly to each screen.
- **View (decided):** 24 × 13.5 tiles on every display, with art at 160 pixels per tile on 4K and 80 on 1080p. The character stands about 1.9 tiles tall, so the existing sheets show at full size on 4K and half size on 1080p.

## Movement baseline for the first build

The starting numbers are the Platformer Feel Lab's defaults, not an earlier tuned set:

> jump height 4.0 tiles, time to apex 0.40 s, fall gravity ×1.60, jump cut ×3.0, terminal velocity 20 tiles/s, walk speed 5.5 tiles/s, run speed 10.0 tiles/s, speed-up time 0.45 s, stop time 0.30 s, turn time 0.35 s, air control ×0.65, walkable limit 50°, uphill cost 20%, downhill boost 15%, slide friction ×0.75, slide steering ×0.30, foot width 0.60 tiles, coyote time 80 ms, jump buffer 100 ms, slide jump kick away, gravity lock on

Stamina, ledge grab, climbing, lunge and hand numbers also start from the lab's defaults; the full list is in `feel-lab/traces/_world.json`.

**Scope of the first build (decided):** movement, climbing, stamina and hands on one test map, in Unity, with the simulation written as plain C# that runs without Unity. Imagine is out as a feature; save-and-restore, the seeds and the frame counter are in as foundations. No battles, party, stats or enemies. The details are in `build-spec.md`.

## Day-one engine requirements

These have to be built in from the start, because they are painful to add later:

- **Seeds:** a shareable, fixed run seed and battle seed.
- **Battle randomness also uses the frame counter.** A fight plays out identically only if *everything* is identical, down to the frame. In practice something will usually differ, so a retried fight may go differently.
- **The frame counter keeps running for the whole run.** It does not reset when you reload after fainting.
- **Frame-based randomness is for battle only.** An overworld threat that behaves based on the frame counter could be created as a specific exception; there is no plan to do so yet.
- **Constant save state:** the whole overworld can be saved, simulated forward and restored exactly. Imagine depends on it.

## Maps

- Maps are **artisan made**, not generated.
- Every map always has a **"slow" way** that the player is known to be capable of.
- **Every map is clearable at minimum abilities.** Stats and abilities open faster routes; they are never required to get through.

### Ground, scaffolds and cliffs

- **Ground is solid.** It is filled in from its surface down to the bottom of the map, and where it ends or steps down there is a cliff face you cannot walk or jump through. The top corner of a cliff is a ledge: it can be grabbed and pulled up onto.
- **Scaffolds are jump-through.** Anything you can jump up through looks built, like wooden scaffolding, so the two read differently at a glance.
- **Negative space is filled.** Under the lowest ground the rock continues off the bottom of the screen.
- Ceilings (caves, overhangs) are not designed yet.

### Map size, doors and edges (decided 2026-11-19)

- **A map is up to 344 by 344 tiles:** about 14 screens wide and 25 tall. There will be a lot of vertical climbing.
- **Ground can have an underside.** A floating mass of rock is solid all the way round, and you bump your head on it from below: your rise stops dead and your sideways speed carries on. Rock lower than you are tall cannot be got under, not even with a dodge slide. That is intended: the slide is for dodging monsters. Ground that is not floating still fills down to the bottom of the map. (This replaces "ceilings are not designed yet" above.)
- **Positions snap to sixths of a tile** in the map editor.
- **Doors.** Press **interact** while standing at a door to go through it to another map. (Not up: up also mounts ladders, so a door near a ladder would be ambiguous.) Doors come in pairs. A door is 1.25 tiles wide, and you can enter from anywhere within it. They are an escape: you can use one while a monster has noticed you. A door can be marked locked, and then a plot element is needed to open it.
- **Edges.** Walking off the edge of a map can lead to the opposite edge of another map. Edges are paired the way doors are.
- **The map editor** is built into the game as an editor mode, one map at a time. See `map-editor-brief.md`.

### Background

The background is a matte painting zoomed way out. As the foreground scrolls quickly, the background scrolls slowly behind it.

## Current focus

Movement and map building come first. Battle-start edge rules and similar details are deferred until they can be felt in play.

## Two scenes

The game has two separate views:

- **Platformer view:** where you explore and move. No combat happens here.
- **Battle scene:** where all combat happens.

## Entering a battle

- Touching an enemy model, or touching its trigger zone, starts a battle.
- The battle does not begin mid-movement. It begins the next time you are **anchored**: somewhere gravity is not presently affecting you. That includes standing on ground and holding onto something. It does not require standing still.
- The purpose: the game needs a position it can return the player to when the fight ends without a wonky transition.
- **Deferred:** the exact definitions and edge rules. One proposal is on the table for when this is picked up: a touched enemy makes a battle pending; it starts at your next anchor; if you hit a hazard or pit first, or stay unanchored too long, it starts at your last anchor; every enemy touched while one is pending joins the fight.

## Enemy awareness

- Monsters may have a **notice** range and an **engage** range.
- Some monsters will chase you to start a fight.
- Monsters come in types (for example Type A and Type B) with different threat detection rules.
- **Detection zone:** each monster has a detection zone based on its position. If it detects you and chases you, the zone comes with it, so you stay inside it. If you escape the detection zone, you are no longer Active.
- **Following through areas:** monsters should follow you from one area into another. The area it left stays "wild."
- **Tracked:** the game has a maximum number of viewable pixels. The **tracked** region is that maximum view, **centred on the leader**. It replaces what was earlier called "on screen," so behaviour does not depend on the player's actual display or on where the camera is. (An earlier answer made it a square; the maximum view was chosen instead so nothing is tracked that could never be seen.)
- If a "chaser drops down on you" moment is wanted, it can be built with a mechanic that has no sprite of its own and spawns a sprite on a timer.
- The overworld will very rarely use the global clock. Timers there normally count from when something becomes tracked.
- There is no need to track sprites outside the tracked region. If you get a chasing monster out of it, the monster goes back to its original area.
- **Overworld timing:** animations and movements in the overworld should generally be based on when they become tracked.
- Some puzzles require running, so whether you can outrun or outlast a chase can decide whether a puzzle is done with or without a fight.

## Battle system

**Open.** Two candidates, to be decided later:

- A traditional JRPG battle, where each character acts when their ready bar fills.
- A hex-grid battle in the style of Heroes of Might and Magic.

Noted in favour of the ready-bar option: Final Fantasy VI and Chrono Trigger both use it, and both have great randomizers.

## Party and leader

- You have a party of 3 characters at a time.
- There will be a lore reason the party is capped at 3. **Open:** what that reason is.
- You designate one character as the leader. The leader is the one who runs through the platformer view.
- You can change who the leader is whenever there is "no threat."
- Expected and accepted consequence: players will likely build one character as a ninja (the leader) and the other two as soldiers.

## Vital statistics

Each character has three vital statistics: **Body**, **Agility** and **Mind**.

Design goal: all three are useful both in the overworld and in the battle scene.

### What each does in the overworld

| Statistic | Theme | Overworld role |
|---|---|---|
| Body | Consistent, sustained effort | Amount of stamina. Climb speed. Ability to pull up onto ledges. Weight you can carry without encumbrance. |
| Agility | Bursts of speed | Move speed. Acceleration. Jump height. Walkable limit (steepest slope you can stand on). Air control. |
| Mind | Abilities | Which abilities you can equip. Examples: perceiving monsters' threat zones, receiving helpful hints, possibly fantastical abilities such as a double jump. |

Notes on Mind:

- There is a lot of room to decide what Mind unlocks. The intent is that Mind assists movement in the overworld, not that it undermines the other two.
- How strong something like a double jump is depends entirely on how it is statted.
- A teleportation wizard build being possible would be very cool. Teleporting past encounters is intended, within the limit that some encounters are unskippable.
- **Imagine (a definite feature):** Mind lets you "imagine" some number of seconds into the future, so you can try a maneuver first before doing it for real. The number of seconds scales with the Mind stat.
  - Monsters react during the imagined seconds, so Imagine also works as a scouting tool. That is how it implicitly helps platforming.
  - Imagining costs **mana**, and it can be made relatively cheap.
  - **The leader's own Mind and mana pay for it.** Other party members cannot power it for the leader.
  - Why: it gives you a reason not to "just" build a ninja. Whoever has the mana is who you will want to imagine with, and leading with a different character means facing different challenges.

### Mana

- Mana does not recover on its own. You recover it through consumables or by sleeping.
- Mana is also what you fight with, so spending it on Imagine in the overworld makes combat more difficult.
- Epic players will farm some gold and buy some potions, at which point mana stops being a major gate for them. That takes time, so randomizer players will be more careful with it.
- Experience can be farmed as well.

### What each does in battle

**Open.** Not yet defined; depends on the battle system.

## Stamina and climbing

- Stamina comes from Body.
- **Pulling up onto a ledge** is worth nearly 2 extra tiles of jump height, which is a huge boon.
- **Every ledge therefore has two keys:** enough jump height to reach it outright (Agility), or a lower jump plus a pull-up (Body, with free hands).
- A pull-up consumes a lot of stamina. The cost is derived from your climb speed and your weight.
- If you get all the way up before your stamina bar empties, you succeed.
- **When stamina hits zero,** you momentarily crouch or lie down. If you are climbing at the time, that makes you fall.
- **Each climbable has a difficulty, and difficulty is a stamina cost multiplier,** not a pass/fail threshold. One rule covers pull-ups, ladders, ropes and walls: a race between the distance to climb and your stamina. Ladders are easy. Ropes are relatively easy. Brick walls are nearly impossible, and a high enough Body can eventually beat them.
- **Stairs are slopes, not climbables.** They fall under walkable limit (Agility).

### Refinements from the lab prototype (2026-11-19)

- **Grabbing is a button.** It is not automatic.
- **A pull-up takes an amount of time based on your Body.** It costs stamina continuously for that whole time (stated as an amount per 0.1 seconds, or whatever fits the game's ticks). You fail the pull-up if you run out of stamina during it. This replaces the earlier wording that the cost is derived from climb speed.
- **Running costs stamina, but only barely more than you regenerate,** so it does not become frustrating.
- **Jumping costs more:** about 30% of your stamina.
- **Stamina regenerates while you walk,** and regenerates very swiftly while you crouch and do not move.
- **Character height can be a stat.** Tiles can be fractional, so a character's height does not have to be a whole number of tiles, and height affects what they can grab.
- **Tops of inclines can be grabbed.** The crest of a slope too steep to stand on counts as a ledge.
- **Lunge:** in the air, a move that lets the character dive forward so their height helps them reach a surface ahead. Being prototyped: hold grab and a direction, press jump; it costs stamina, removes air control, and a miss lands you prone.
- **Climbing differs by pace and by free hands.** Climbing (a rope, for example) should be different depending on whether you are sprinting and on whether you have a free hand.
- **Tone check: we are still heroes, and this is an action adventure.** One-handed rope climbing is slower and costlier, but not "very" slow; penalties should not make heroic play feel impossible.
- **You can jump from a ladder or rope.**
- **You can climb down a rope from the platform at its top,** the same way you can a ladder.
- **You can rest on a ladder** and regenerate stamina there, if your hands are empty.

### Dodge slide

- Press down while sprinting and he drops into a slide for a moment. It is for dodging things.
- He is low during it (0.9 tiles against his standing 1.9), cannot steer, and loses speed. It ends by itself, or with a jump.
- Lab starting numbers: 0.35 seconds, 70% of speed kept at the end (about 3.2 tiles covered from a full sprint), 5 stamina.
- **Its purpose is dodging monster hit boxes.** His low height during the slide is tested against monsters, not against rock. It does not let him under rock lower than he stands, and no crawl is planned.

### Stat scale

- The stamina and movement costs in the lab are what the game should feel like at **100 / 100 / 100** (Body / Agility / Mind).
- **100 / 100 / 100 is the typical starting condition. 80 / 80 / 80 is the minimum. About 200 is near the upper limit.**

### Stats and movement (in progress, 2026-11-19)

**Decided:**

- **Stats count in steps of 20:** 80, 100, 120, 140, 160, 180, 200. That is seven levels.
- **The player sees levels 1 to 7, never the values behind them.** Level 1 is a stat of 80, level 2 is 100, and so on up to level 7 at 200.
- **Each movement number belongs to one stat, or to none.**
- **Agility's options:** Jump, Speed, Acceleration, Air control.
- **Acceleration is one option that moves three numbers together:** how fast he speeds up, stops and turns.
- **Body's options:** Pull-up, Climb.
- **Body also works automatically, not as an option:** a higher Body gives a bigger stamina bar (always the full bar), shortens exhaustion, and raises the share of climb speed kept when climbing one-handed.
- **On no stat:** walkable limit, draw and sheathe time, walking recovery, crouched recovery, walk speed, every stamina cost, gravity, jump cut, coyote time and jump buffer. Height and reach belong to the character.
- **Walkable limit may become an ability that Mind unlocks.**
- **Between levels, movement does not change.** A stat that sits between two levels (Agility 107, say) plays as the lower level. What a stat gains between levels is combat stats or discrete abilities.
- **One exception: the stamina bar equals Body exactly,** point for point. Body 107 is 107 stamina.

**Proposed values, not yet tuned by feel:**

| Level | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
|---|---|---|---|---|---|---|---|
| Stat | 80 | 100 | 120 | 140 | 160 | 180 | 200 |
| **Jump:** height (tiles) | 3.6 | 4.0 | 4.4 | 4.7 | 5.0 | 5.3 | 5.5 |
| **Speed:** run speed (tiles/s) | 9.0 | 10.0 | 10.8 | 11.5 | 12.1 | 12.6 | 13.0 |
| **Acceleration:** speed-up time (s) | 0.50 | 0.45 | 0.41 | 0.38 | 0.35 | 0.32 | 0.30 |
| **Acceleration:** stop time (s) | 0.33 | 0.30 | 0.28 | 0.26 | 0.24 | 0.23 | 0.22 |
| **Acceleration:** turn time (s) | 0.39 | 0.35 | 0.32 | 0.30 | 0.28 | 0.26 | 0.25 |
| **Air control** | 0.60 | 0.65 | 0.70 | 0.74 | 0.78 | 0.82 | 0.85 |
| **Pull-up:** time (s) | 1.25 | 1.00 | 0.85 | 0.73 | 0.64 | 0.56 | 0.50 |
| **Climb:** speed (tiles/s) | 1.7 | 2.0 | 2.4 | 2.7 | 3.0 | 3.3 | 3.5 |
| Stamina bar, automatic (equals Body exactly, including between levels) | 80 | 100 | 120 | 140 | 160 | 180 | 200 |
| Exhaustion (s), automatic | 1.4 | 1.2 | 1.05 | 0.9 | 0.8 | 0.7 | 0.6 |
| One-handed share of climb speed, automatic | 60% | 65% | 70% | 74% | 78% | 82% | 85% |

**Not yet decided:** whether each of the six options is set separately at sleep, to any level up to what its stat allows.
- At these numbers stamina feels costly, and it is not the free-jumping Mario dream. That is accepted: it is the starting point, not the ceiling.

### Draw and sheathe

- Changing gear in general requires being Safe. **Hands are the exception:** there is a draw / sheathe hotkey that brings up a small menu for the left and right hand, usable even when you are not Safe.
- It still unequips that gear, for the leader only. If you sheathe and then end up in a fight, you are disadvantaged.
- **It should be timely.** The best players should be able to weave it into their ninja moves, but it should not be free.
- **Earlier concern, now answered by the refinements below:** "bursts versus sustained" implies sprinting costs stamina, and running dry during plain traversal with nothing at stake is a known way stamina systems annoy players.
- **Stowed gear is invisible.** A sheathed sword or a put-away shield is not drawn on the character. Items only appear in a hand.

## When stat changes take effect

- Movement details such as jump height, speed and acceleration do **not** change the moment Body, Agility or Mind changes. The reason: players should get used to how their character feels.
- Whenever you sleep, you can change these details about your character.
- Body, Agility and Mind gate which options are available for each detail.

### How often you sleep

- You are likely to sleep **between each block**.
- You are likely encouraged to sleep **once in the middle of each block**.
- Additional sleeps only happen when you **"fail"** and need to go back.

## Hands and gear

- The game tracks what each character is holding in their hands.
- Free hands give you more opportunities to grab ledges and climb.

## Safety states

The player is always in one of three states:

| State | When | What it allows |
|---|---|---|
| **Big Safe** | In a town or at a campsite. | Sleeping, changing movement details, saving. Also everything Safe allows. |
| **Safe** | Grounded, not in a hazard, and outside every monster's detection zone. | Changing less vital statistics such as held gear. Changing who the leader is. Going home. |
| **Active** | In a dynamic action, in a hazard, or inside a monster's detection zone. | None of the above. |

Other sections of this document say "no threat." That means Safe (or Big Safe).

### The terms underneath

Three nested conditions, each a subset of the one before:

- **Anchored:** gravity is not acting on you. Standing, hanging from a wall, on a ladder or rope. Fights and forced leader changes can start here, because there is somewhere to put you afterwards.
- **Grounded:** anchored *and* standing on walkable ground on your own feet. Hanging and climbing do not count.
- **Safe:** grounded, not in a hazard, and outside every detection zone.

A **dynamic action** is anything that is not grounded. So hanging from a wall is anchored but still a dynamic action: a fight can start there, but you cannot change your gear.

Decided on 2026-11-19:

- **Sliding down a too-steep slope is not anchored.** Gravity is acting on him, so a battle cannot start mid-slide.
- **The dodge slide is not safe.** He is on walkable ground, but it counts as a dynamic action: no gear changes mid-slide.

## Open questions

1. Which battle system: ready-bar JRPG or hex grid?
2. What is the lore reason for a party of 3?
3. What do Body, Agility and Mind do in battle?
4. Does reaching a Big Safe without sleeping count as a save point, or only actually sleeping?
5. The battle-start edge rules (hazards, pits, several enemies touched before anchoring). Deferred until it can be felt.
6. Which abilities Mind unlocks, and how each is statted (double jump, teleportation, a steeper walkable limit and others).
7. How sprinting and stamina interact during ordinary traversal.
8. Climbable difficulty in numbers: the cost multiplier for each surface.
