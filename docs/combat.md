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
- **Decided 2026-11-20 (C13):** while a sub-menu is open, actions already queued keep starting, one after another, as in Final Fantasy VI. The rule is "bars stop, nothing else does". A monster whose bar was already full still gets its attack.

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

## Character stats in a fight

Brad set this list and the damage rule on 2026-11-20. It replaces the "each stat owns a bar" proposal (C14 to C16), which is withdrawn.

### The stats

**Decided 2026-11-20**, except the sources marked proposed.

| Stat | What it is | Set by |
|---|---|---|
| HP | Current and maximum. The character faints at zero. | Proposed (C18): Body |
| MP | Current and maximum. Pays spell costs. | Proposed (C18): Mind |
| Speed | Fills the ready bar. | Proposed (C18): Agility, on the stat curve |
| Attack | The damage a physical attack fundamentally does. | The weapon |
| Strength | The attacker's side of the physical comparison. | Proposed (C18): Body |
| Defense | The defender's side of the physical comparison. | Armour and Agility |
| Magic Attack | The damage a skill or spell fundamentally does. | The skill or spell |
| Magic Power | The attacker's side of the magical comparison. | Mind |
| Fortitude | A saving throw. | Body |
| Reflex | A saving throw. | Agility |
| Will | A saving throw. | Mind |
| Dodge % | Chance a dodgeable attack misses. | Agility |
| Block % | Chance a blockable attack misses. | The shield |

- **Decided 2026-11-20 (C14):** stamina is strictly a platformer-view stat. It plays no part in a fight. This also answers item 8 of the brief: stamina does not carry in.
- **Decided 2026-11-20 (C14):** skills cost nothing, or mana.
- **Decided 2026-11-20 (C14):** Agility also affects physical defence.

### How an attack resolves

**Decided 2026-11-20:**

1. An attack fundamentally does its **Attack** value (from the weapon) or its **Magic Attack** value (from the skill or spell).
2. That value is scaled by a comparison:
   - physical: the attacker's **Strength** against the defender's **Defense**;
   - magical: the attacker's **Magic Power** against one of the defender's saving throws, **Fortitude, Reflex or Will**, as the skill or spell defines.
3. The comparison makes the attack do **50% to 200%** of its value. It will tend to do **80% to 125%**.
4. Each attack is marked **Dodgeable?** and **Blockable?**. A dodgeable attack misses on the defender's Dodge %. A blockable attack misses on the defender's Block %. Dodge is rolled first, and the two combine multiplicatively: the chance of being hit is (1 − Dodge) × (1 − Block).

So every attack, skill and spell is described by: its base value (the weapon's Attack, or its own Magic Attack), which comparison it uses, whether it can be dodged, whether it can be blocked, and its cost (nothing, or mana).

### Numbers to try, all proposed

- **C19, the comparison:** scale = attacker's stat ÷ defender's stat, held between 50% and 200%. The 80% to 125% band is then two combatants within about one stat level of each other.

  | Attacker ÷ defender | 80 ÷ 200 | 80 ÷ 100 | 100 ÷ 100 | 120 ÷ 100 | 125 ÷ 100 | 200 ÷ 100 | 200 ÷ 80 |
  |---|---|---|---|---|---|---|---|
  | Scale | 50% (held) | 80% | 100% | 120% | 125% | 200% | 200% (held) |

- **C18, sources:** HP = Body. MP = Mind. Strength = Body. Magic Power = Mind. Fortitude = Body, Reflex = Agility, Will = Mind. Speed = the stat curve on Agility.
- **C20, Defense:** Defense = armour + Agility ÷ 2. Starting armour is worth 50, so a starting character has Defense 100.
- **C21, Dodge:** Dodge % = Agility ÷ 10. That is 8% at 80, 10% at 100, 20% at 200.
- **C22, Block:** 10%, 20% or 30% by the size of the shield, and only while the shield is in a hand.
- **C23, no damage spread:** damage has no random variation. Dodge and block are the only rolls in an ordinary attack.

### Points raised with these numbers

- **C21:** Agility now sets four things (Speed, part of Defense, Reflex, Dodge) against three each for Body and Mind, and two of its four reduce physical damage. The ninja would be the fastest and the hardest to hurt. Proposed answer: keep Agility's share of Defense and its Dodge small, as above, and give Body its own route to defence through armour weight, since Body already governs what a character can carry. That belongs with gear (item 9).
- **C22:** Attack comes from the weapon in hand and Block from the shield in hand. So most of what sheathed gear costs (item 6) follows from the stat list.
- **Dump stats:** each stat is also a saving throw, so a character who dumps one takes up to 200% from attacks aimed at it. That keeps Mind from being a free dump for soldiers.

### Still proposed from before

- **C15:** opening fill = Agility ÷ 4, as a percentage of the bar (20% at 80, 25% at 100, 50% at 200).
- **C17:** which stat unlocks which abilities. `design.md` gives Mind "which abilities you can equip". Proposed narrowing: Mind unlocks spells and perception abilities (the monster hint among them); Body and Agility thresholds unlock free skills. If accepted, this changes the Mind row of the vital statistics table in `design.md`.

### Not proposed yet

- Whether health recovers by itself outside a fight.
- Critical hits, if any.

## Waiting on Brad

- C15: opening fill from Agility.
- C17: each stat unlocks its own abilities; narrows a line in `design.md`.
- C18: what sets HP, MP, Speed and Strength.
- C19: the comparison is a plain ratio, held between 50% and 200%.
- C20: Defense = armour + Agility ÷ 2.
- C21: Dodge % = Agility ÷ 10; Body reaches defence through armour weight.
- C22: Block 10 / 20 / 30%, only with the shield in hand.
- C23: no random damage spread.

## Not started

In the order of `handoff-combat.md`: how stats rise (5); what sheathed gear costs (6), partly answered by C22; running away (7); gear and abilities (9); each monster (10). The battle scene's confirm and cancel controls are also to come.
