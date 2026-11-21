# Combat

The battle scene and what character stats do in it. Maintained by the combat conversation; its brief is `handoff-combat.md`. Everything outside the battle scene is in `design.md`.

Started 2026-11-20. Discussion points are numbered C1 onward.

Every statement is marked:

- **Decided:** Brad stated or agreed it. Dated.
- **Proposed:** recommended, not yet answered or not yet tried in play.
- **Open:** undecided, with no recommendation accepted.

Nothing here is ready to build. Battles are outside the first build.

## The shape of a turn

### The ready bar

- **Decided 2026-11-20 (design.md):** each character and monster has a ready bar and acts when it fills.
- **Decided 2026-11-20 (C5):** after acting, a bar resets to zero.
- **Decided 2026-11-20 (C5):** when more than one party member is ready, the one who was ready first gets the command menu. One button passes the menu to the next ready member.

### Active and Wait

- **Decided 2026-11-20 (C1):** the player can toggle between **Active** and **Wait**, as in Chrono Trigger.
- **Decided 2026-11-20 (C2):** the bars freeze **only while a sub-menu is open**: choosing a target for Fight, or inside the Skill, Magic or Item lists. They do not freeze on the top command menu, and they do not freeze while an action animates.
  - Active: nothing ever freezes the bars.
  - Wait: a sub-menu freezes them.
- **Decided 2026-11-20 (C2):** this is meant to be played with. When the player opens a sub-menu changes how the bars line up, to make a fight easier or to make it faster.
- **Proposed (C9):** in Wait, an open sub-menu stops the whole battle clock, including an action already in flight. One rule, no edge cases. Idle loops keep playing; they are cosmetic.
- **Proposed (C10):** the mode can be changed from the options menu at any time outside a sub-menu. A run records which mode it was played in, so shared-seed times can be compared like for like.

Superseded: C1 first recommended a single mode in which time always stops while choosing, and C2 recommended freezing the bars during animations too. Brad chose the Chrono Trigger behaviour instead.

### Actions take time

These follow from the bars running during animations.

- **Proposed (C8):** every action has a fixed length in frames, written in the combat data. Art is made to fit the length. A longer animation would otherwise hand everyone more bar.
- **Proposed (C8):** one action plays at a time. Anyone else who is ready queues in the order they became ready. A full bar gains nothing while it waits.

### How fast a bar fills

- **Decided 2026-11-20 (C3):** fill rate is not proportional to the stat behind it. A flat 0.8× to 2× would make bar speed the stat that beats all others.
- **Decided 2026-11-20 (C3), to try first:** fill rate = 50% + stat ÷ 2.

  | Stat | 80 | 100 | 120 | 140 | 160 | 180 | 200 |
  |---|---|---|---|---|---|---|---|
  | Fill rate | 0.90× | 1.00× | 1.10× | 1.20× | 1.30× | 1.40× | 1.50× |

- **Decided 2026-11-20 (C3), to try first:** at a rate of 1.00× a bar fills in 2.0 seconds, at the default battle speed.
- **Open (item 3):** which stat, or mix of stats, feeds the fill rate. Agility alone would give the ninja leader the most turns as well as the best movement.

### The opening

- **Decided 2026-11-20 (C6):** nobody starts a fight with a full bar. The clock skips straight to the first bar to fill, so a fight never opens on a wait.
- **Proposed (C6):** starting fill is the lever for sheathed gear (item 6) and for how the fight began (who touched whom). Numbers come with those items.

## What the player sees of monsters

- **Proposed (C4), to try in the prototype:** monsters' ready bars are visible. Brad's impulse is that this gets messy; it has to be drawn subtly, and it is dropped if it cannot be.
- **Proposed (C4):** a hint of what a monster will do next, as in Slay the Spire, possibly gated behind a skill. To be settled with abilities (item 9). It is a candidate for something Mind buys in a fight.
- **Proposed (C11):** a monster picks its next action the moment its bar resets, not when it fills. Any hint needs this, and it is cheaper to build in from the start than to add.

## Fight tiers and how long a fight lasts

- **Decided 2026-11-20 (C7):** fights come in three tiers: **mundane**, **featured** and **boss**. "Skippable" and "unskippable" are overworld terms; some mundane fights are unskippable only because of how the map is shaped.
- **Decided 2026-11-20 (C7), to start from:** the target lengths below are for a casual epic player. Runners will finish fights much faster, so a run holds more fights than these lengths suggest.

  | Tier | Casual length | Status |
  |---|---|---|
  | Mundane | 20 to 30 s, about 6 to 8 party actions | Decided, to start from |
  | Featured | 90 to 120 s | Proposed (C12): this number was first offered for "unskippable" fights |
  | Boss | 3 to 4 min | Proposed (C12) |

## Options

- **Decided 2026-11-20 (C3):** menu speed and battle speed are options in the menu.
- **Proposed (C10):** battle speed scales the whole battle clock evenly: bars and action lengths together. Turn order is then the same at every speed, and only the player's thinking time changes. Five settings: 0.5×, 0.75×, 1×, 1.5×, 2×.
- **Proposed (C10):** menu speed is cosmetic. A menu takes input on the frame it opens, at every setting.

## Waiting on Brad

- C8: fixed action lengths in data; one action at a time, the rest queue.
- C9: what a sub-menu freeze stops.
- C10: battle speed scales everything evenly; five settings; mode and speed recorded with the run.
- C11: monsters pick their next action at bar reset.
- C12: featured 90 to 120 s, boss 3 to 4 min.

## Not started

In the order of `handoff-combat.md`: what Body, Agility and Mind do in a fight (3); what one stat point is worth (4); how stats rise (5); what sheathed gear costs (6); running away (7); whether stamina carries in (8); gear and abilities (9); each monster (10). The battle scene's confirm and cancel controls are also to come.
