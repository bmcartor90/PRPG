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
- **To try first (C15):** opening fill = Agility ÷ 4, as a percentage of the bar: 20% at 80, 25% at 100, 50% at 200.
- **Proposed (C6):** starting fill is also the lever for sheathed gear (item 6) and for how the fight began (who touched whom). Numbers come with those items.

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

### Where a stat's value comes from

- **Decided 2026-11-20:** each character has **base stats** of their own, before the vital statistics are counted.
- **Decided 2026-11-20:** **character level** raises all of these values.
- **Decided 2026-11-20:** Body, Agility and Mind are therefore one part of each value, not the whole of it. The spread between characters is narrower than the 80 to 200 range of the vital statistics suggests.

### The stats

**Decided 2026-11-20**, except where marked.

| Stat | What it is | Made from |
|---|---|---|
| HP | Current and maximum. The character faints at zero. | Includes Body. Body governs it; the scaling is open. |
| MP | Current and maximum. Pays spell costs. | Includes Mind. |
| Speed | Fills the ready bar. | To try first: the stat curve on Agility. |
| Attack | The damage a physical attack fundamentally does. | The weapon. |
| Strength | The attacker's side of the physical comparison. | Character Strength + Body ÷ 2. |
| Defense | The defender's side of the physical comparison. | Character Defense + armour Defense + Agility ÷ 4. |
| Magic Attack | The damage a skill or spell fundamentally does. | The skill or spell. |
| Magic Power | The attacker's side of the magical comparison. | Character Magic + Mind ÷ 2. |
| Fortitude | A saving throw. | Character Fortitude + Body ÷ 2. |
| Reflex | A saving throw. | Character Reflex + Agility ÷ 2. |
| Will | A saving throw. | Character Will + Mind ÷ 2. |
| Dodge % | Chance a dodgeable attack misses. | Agility ÷ 20: 4% at 80, 5% at 100, 10% at 200. |
| Block % | Chance a blockable attack misses. | The shield. |
| Critical % | Agreed in outline (C26), settled with gear. | The weapon. |

- **Decided 2026-11-20:** Defense runs from **80 to 120 early** in the game to **300 to 420 late**. Agility is about 25% of Defense early and about 11% late.
- **Decided 2026-11-20 (C24):** Strength and Defense have to scale commensurately with each other, and so do Magic Power and the saving throws. The two pairs do not have to be on the same scale as each other.
- **Decided 2026-11-20 (C25):** Agility's share of Defense fades as levels rise, and that is intended. Gear should be an important part of Defense, and the ninja should not be naturally high in Defense.
- **Decided 2026-11-20 (C14):** stamina is strictly a platformer-view stat. It plays no part in a fight. This also answers item 8 of the brief: stamina does not carry in.
- **Decided 2026-11-20 (C14):** skills cost nothing, or mana.
- **Decided 2026-11-20 (C21):** Body's part in staying alive is HP. (The idea of reaching Defense through armour weight was not taken up.)

### How an attack resolves

**Decided 2026-11-20:**

1. An attack fundamentally does its **Attack** value (from the weapon) or its **Magic Attack** value (from the skill or spell).
2. That value is scaled by a comparison:
   - physical: the attacker's **Strength** against the defender's **Defense**;
   - magical: the attacker's **Magic Power** against one of the defender's saving throws, **Fortitude, Reflex or Will**, as the skill or spell defines.
3. The comparison makes the attack do **50% to 200%** of its value. It will tend to do **80% to 125%**.
4. Each attack is marked **Dodgeable?** and **Blockable?**. A dodgeable attack misses on the defender's Dodge %. A blockable attack misses on the defender's Block %. Dodge is rolled first, and the two combine multiplicatively: the chance of being hit is (1 − Dodge) × (1 − Block).

So every attack, skill and spell is described by: its base value (the weapon's Attack, or its own Magic Attack), which comparison it uses, whether it can be dodged, whether it can be blocked, and its cost (nothing, or mana).

### The comparison formula

**Open.** Brad offered a formula on 2026-11-20 and withdrew it a minute later as "all wrong": AQ (Attack Quotient) = attacker ÷ defender, damage = (Attack ÷ 2) + (Attack ÷ (2 × AQ)). As written it makes a stronger attacker do less. Nothing here is decided until he gives his replacement.

- **Proposed (C29), one candidate:** keep AQ as the attacker's stat ÷ the defender's stat (Strength ÷ Defense, or Magic Power ÷ the saving throw), and multiply where the withdrawn formula divided:

  **Damage = (Attack ÷ 2) + (Attack × AQ ÷ 2)**, which is Attack × (1 + AQ) ÷ 2, and never more than 200%.

  | AQ | 0.25 | 0.5 | 0.67 | 0.8 | 1 | 1.25 | 1.5 | 2 | 3 or more |
  |---|---|---|---|---|---|---|---|---|---|
  | Damage | 63% | 75% | 83% | 90% | 100% | 113% | 125% | 150% | 200% |

  - The floor of 50% is built in: it is approached as the attacker's stat shrinks beside the defender's, and never reached.
  - 200% is reached at AQ 3 and held there.
  - With Strength and Defense both between 80 and 120, AQ runs from 0.67 to 1.5, which is 83% to 125%: the "tends to do 80% to 125%" band.
- **Raised (C30), about that candidate:** it is gentle. Doubling Defense from an even match takes damage from 100% to 75%. The whole early spread of Defense (80 to 120, against Strength 100) is 113% down to 92% of damage taken, about a fifth. Late (300 to 420 against 360) it is 110% down to 93%. A plain Damage = Attack × AQ, held between 50% and 200%, makes the same early spread 125% down to 83%.

### Shields and hands

- **Decided 2026-11-20 (C22):** there will be a wide range of gear, and shields can do many things. Each shield is defined on its own, not by a size table.
- **Decided 2026-11-20 (C22):** shields should be pretty good. A two-handed weapon against sword and shield is always a trade-off, and this game adds a third pull: wanting an open hand in the platformer view.
- **To try first (C22):** Block % runs from 6% to 30% across shields.
- **Proposed (C22):** Attack and Block come from what is actually in the hands, so they are most of what sheathed gear costs (item 6).

### Skills and Tech Points

- **Decided 2026-11-20 (C17):** Body and Agility do not unlock skills.
- **Decided 2026-11-20 (C17):** Mind × experience gives a character **Tech Points**, which are spent on skills, like Job Points in Final Fantasy Tactics.
- **To try first (C27):** Tech Points gained = experience gained × Mind ÷ 100, for each character on their own Mind. Experience is not used up by this. The scale can end up anywhere; ability prices are balanced to it.
- **To try first (C27):** a skill has to be unlocked in the world before it can be bought. The randomizer shuffles the unlocks; Tech Points decide who learns what.
- **Decided 2026-11-20 (C28):** the platformer-view abilities (double jump, teleport, perceiving threat zones and the rest) are bought with the same Tech Points.

### Critical hits

- **Agreed in outline 2026-11-20 (C26), punted until gear:** no random spread on damage. Critical hits instead, as a weapon stat. First numbers: 5% to 25% by weapon, ×1.5, applied after the comparison, only on attacks that use the weapon's Attack.

### Waiting on Brad

- **C29:** the comparison formula. Brad withdrew his; his replacement, or the candidate above.
- **C30:** how steep it should be, so gear matters as much as intended.
- **C31:** what comes next: a playable battle prototype on placeholder numbers, or more of the list on paper first.

### For `design.md` (the design conversation maintains it)

- Character level exists and raises combat values.
- The Mind row of the vital statistics table: abilities, in both scenes, are bought with Tech Points, earned as Mind × experience. This answers part of open question 6.
- Menu speed is an overworld option (C10).

### Not proposed yet

- How HP and MP scale with Body, Mind and level.
- Whether health recovers by itself outside a fight.

## Not started

In the order of `handoff-combat.md`: how stats rise (5), now partly fixed by character level and Tech Points; what sheathed gear costs (6), partly answered by C22; running away (7); gear and abilities (9); each monster (10). The battle scene's confirm and cancel controls are also to come.
