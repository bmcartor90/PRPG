# Combat

The battle scene and what character stats do in it. Maintained by the combat conversation; its brief is `handoff-combat.md`. Everything outside the battle scene is in `design.md`.

Started 2026-11-20. Discussion points are numbered C1 onward.

Every statement is marked:

- **Decided:** Brad stated or agreed it. Dated.
- **To try first:** a starting number or rule Brad accepted, or let stand, as a place to begin. Expected to move in play.
- **Proposed:** recommended, not yet answered.
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
- **Decided 2026-11-20 (C9):** a sub-menu freezes bar filling only. An action already animating carries on and lands.
- **Decided 2026-11-20 (C2, C9):** this is meant to be played with. When the player opens a sub-menu changes how the bars line up, to make a fight easier or to make it faster. It is a source of skill expression.
- **Proposed (C13):** while a sub-menu is open, actions already queued keep starting, one after another, as in Final Fantasy VI. The rule is then "bars stop, nothing else does". A monster whose bar was already full still gets its attack.

Superseded: C1 first recommended a single mode in which time always stops while choosing; C2 recommended freezing the bars during animations too; C9 recommended that a sub-menu stop the whole battle clock. Brad chose otherwise each time.

### Actions take time

- **Decided 2026-11-20 (C8):** Final Fantasy VI logic, not Chrono Trigger logic. Every action has an animation of fixed length. The animation does not need to cover ground: a character swings their sword in the general direction of the target from where they stand.
- **Decided 2026-11-20 (C8):** one action plays at a time. Anyone else who is ready queues in the order they became ready.
- **To try first (C8):** each action's length is a number of frames written in the combat data, and art is made to fit it.

### How fast a bar fills

- **Decided 2026-11-20 (C3):** fill rate is not proportional to the stat behind it. A flat 0.8× to 2× would make bar speed the stat that beats all others.
- **To try first (C3):** fill rate = 50% + stat ÷ 2. This is **the stat curve**; the proposals under "What the stats do" reuse it.

  | Stat | 80 | 100 | 120 | 140 | 160 | 180 | 200 |
  |---|---|---|---|---|---|---|---|
  | Multiplier | 0.90× | 1.00× | 1.10× | 1.20× | 1.30× | 1.40× | 1.50× |

- **To try first (C3):** at 1.00× a bar fills in 2.0 seconds, at battle speed 1×.

### The opening

- **Decided 2026-11-20 (C6):** nobody starts a fight with a full bar. The clock skips straight to the first bar to fill, so a fight never opens on a wait.
- **Proposed (C6):** starting fill is the lever for sheathed gear (item 6) and for how the fight began (who touched whom). Numbers come with those items.

## What the player sees of monsters

- **To try in the prototype (C4):** monsters' ready bars are visible. Brad's impulse is that this gets messy; it has to be drawn subtly, and it is dropped if it cannot be.
- **Proposed (C4):** a hint of what a monster will do next, as in Slay the Spire, possibly gated behind a skill. To be settled with abilities (item 9). It is a candidate for something Mind buys in a fight.
- **Decided 2026-11-20 (C11):** a monster picks its next action the moment its bar resets, not when it fills. Any hint depends on this.

## Fight tiers and how long a fight lasts

- **Decided 2026-11-20 (C7):** fights come in three tiers: **mundane**, **featured** and **boss**. "Skippable" and "unskippable" are overworld terms; some mundane fights are unskippable only because of how the map is shaped.
- **Decided 2026-11-20 (C7, C12), to start from:** target lengths for a casual epic player. Runners will finish fights much faster, so a run holds more fights than these lengths suggest.

  | Tier | Casual length |
  |---|---|
  | Mundane | 20 to 30 s, about 6 to 8 party actions |
  | Featured | 90 to 120 s |
  | Boss | 3 to 4 min |

## Options

- **Decided 2026-11-20 (C3):** battle speed is an option in the menu.
- **Decided 2026-11-20 (C10):** a fight is harder at a higher speed. Racers will tend to play at the highest and pay something for it. It only affects fights, which are the smaller share of a run, so leaderboards hold up.
- **To try first (C10):** battle speed scales the whole battle clock evenly: bars and action lengths together. Turn order is the same at every speed; only the player's time to think and to work the menus changes. Five settings: 0.5×, 0.75×, 1×, 1.5×, 2×.
- **Proposed (C10):** a run records the mode (Active or Wait) and battle speed it was played at.
- **Not combat (C10):** menu speed is an overworld option, about how quickly text can be skipped. It belongs in `design.md`.

## What the stats do in a fight

All of this section is **Proposed** (C14 to C17), awaiting Brad.

**Each stat owns a bar.** Body owns health and stamina, Agility owns the ready bar, Mind owns mana. Each stat also drives one multiplier, and all three use the stat curve: 50% + stat ÷ 2.

| | Body | Agility | Mind |
|---|---|---|---|
| Theme | Sustained effort | Bursts of speed | Abilities |
| Bar | Health = Body. Stamina = Body. | Ready bar | Mana = Mind |
| Multiplier (stat curve) | Fight and Skill damage | Ready bar fill rate | Magic power |
| Also | | Opening fill = Agility ÷ 4, as a percentage | |
| Pays for | Skills, in stamina | | Magic, in mana |
| Per point | +1 health, +1 stamina, +0.5% damage | +0.5% fill rate, +0.25% opening fill | +1 mana, +0.5% magic power |

### Body (C14)

- Health equals Body, point for point. 100 at a typical start.
- Stamina comes into the fight. It equals Body, as it already does in the platformer view.
- Skills are paid for in stamina. Stamina recovers steadily during a fight, and quickly while defending, as crouching does outside one.
- Fight and Skill damage are multiplied by the stat curve on Body.
- Cost: a fourth bar on each character (health, stamina, mana, ready).

### Agility (C15)

- Fill rate, by the stat curve, as already being tried.
- Opening fill = Agility ÷ 4, as a percentage of the bar.

  | Agility | 80 | 100 | 120 | 140 | 160 | 180 | 200 |
  |---|---|---|---|---|---|---|---|
  | Opening fill | 20% | 25% | 30% | 35% | 40% | 45% | 50% |

- Evasion and critical hits are not proposed yet.

### Mind (C16)

- Mana equals Mind, point for point.
- Magic is paid for in mana and multiplied by the stat curve on Mind.
- Magic is the strong, fast way to end a fight, and mana does not come back. So mana is speed: spent on Magic it shortens fights, spent on Imagine it shortens the map, and the same pool pays for both.

### Which stat unlocks which abilities (C17)

- `design.md` gives Mind "which abilities you can equip". Proposed narrowing: Mind unlocks Magic and perception abilities (the monster hint among them). Body and Agility thresholds unlock Skills.
- Reason: if Mind unlocks everything, Mind is the only stat that gives a character buttons to press.
- If accepted, this changes the Mind row of the vital statistics table in `design.md`.

### The two checks from the brief

- **The ninja.** A high-Agility leader acts first and most often, and hits at ordinary strength. What the leader pays in a fight comes from the situation, not the stats: hands that may be sheathed (item 6) and stamina that may be spent (item 8).
- **Mind.** A low-Mind party fights for free and slowly, on Fight and Skills, and has a short Imagine. A high-Mind party ends fights quickly on a budget that does not refill.

### Not proposed yet

- Defence. For now it is assumed to come from gear alone.
- Hit, miss and critical chances.
- Whether health recovers by itself outside a fight.

## Waiting on Brad

- C13: with a sub-menu open, queued actions keep starting.
- C14: Body gives health, stamina in battle, and Fight and Skill damage.
- C15: Agility gives fill rate and opening fill.
- C16: Mind gives mana and Magic power.
- C17: each stat unlocks its own abilities; narrows a line in `design.md`.

## Not started

In the order of `handoff-combat.md`: how stats rise (5); what sheathed gear costs (6); running away (7); gear and abilities (9); each monster (10). What one stat point is worth (4) and whether stamina carries in (8) are partly answered by the proposals above. The battle scene's confirm and cancel controls are also to come.
