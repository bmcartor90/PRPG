# Handoff: combat and character stats for PRPG

You are taking over one job on Brad McArtor's 2D platformer RPG: **designing the battle system and what character stats do in it.** Brad is the designer. You help him reach decisions, write them down, and later give the programmer something exact to build. Everything you need is in the repository `bmcartor90/PRPG`; nothing lives only in the previous conversation.

Written 2026-11-20 by the design conversation, which keeps everything outside the battle scene.

## What is yours and what is not

**Yours:**
- The battle scene: how a fight runs from its first frame to its last.
- What Body, Agility and Mind do in a fight, and every combat stat derived from them.
- What a character gains between stat levels (see "Stats" below).
- How stats rise at all: experience, levels, whatever it turns out to be.
- Monster stats and what each monster does in a fight.
- Weapons, armour, consumables and battle abilities, as far as they matter in a fight.
- The battle scene's own controls for confirm and cancel.
- `docs/combat.md`, which does not exist yet. Write everything there.

**Not yours. Leave these alone and tell Brad if they need changing:**
- `docs/design.md`, `docs/build-spec.md`, `docs/changes.md`, `docs/answers.md`. The design conversation maintains these. When a combat decision changes something written in `design.md`, say so to Brad by number and he will carry it across.
- Movement: the feel lab (`feel-lab/`), its recordings, and the six movement options on the sleep screen.
- Monsters while the player is exploring: detection zones, chasing, what hurts on contact. The design conversation is building the first one (the assassin vine) in the feel lab.
- Art: `assets/` belongs to a separate art conversation. If you need a battle pose or a hit frame, describe it to Brad and he will ask for it.
- `sim/`, `game/`, `data/`. The programmer (Claude Opus) owns these.
- Design decisions. Brad makes them. Recommend, then ask.

Three other parties push to this repository. **Pull before you start and before every push**, commit small, never force-push.

## How Brad wants to be worked with

- Blunt. Brief: his target is 2 out of 5 for length unless he asks for detail. Lead with the verdict.
- **Number every point of pushback or open decision** so he can answer by number. The design conversation is at 240 and the art conversation uses S1 onward; start yours at **C1** so the series never collide.
- Recommend the best approach. Do not do something only because he said so, and say when you disagree.
- He considers guessing balance numbers his strength. Give him numbers to react to, not questions about what a number should be.
- If his message has a likely typo, or a name you are unsure of, ask. Do not silently correct.
- Now and then, remind him that a brief answer on a big subject is brief.
- End every reply with a "tier 5" word: an obscure real dictionary word and its definition.
- Do not touch files on his computer without his permission.
- He told an earlier agent it was doing too much in one turn. Do the combat job and stop.

## The game in short

Read `docs/design.md` in full before proposing anything. The parts that bind combat:

- A side-on platformer first, an RPG second. You explore and move in the **platformer view**. All combat happens in a separate **battle scene**.
- The game is about 14 **blocks** of content. The **epic** plays all of them, about 15 hours. The **randomizer** plays 4 or 5 of them in 60 to 90 minutes and is what streamers will play.
- **Speedrunning matters.** Skipping fights by good movement is intended. Some fights cannot be skipped.
- A party of **3**, one of them the **leader**, who is the one on screen in the platformer view. Players are expected to build one ninja and two soldiers, and that is accepted.
- Three characters exist so far: **Lucca** (agile fighter and swordsman), **Nina** (archetype not decided), **Ramorand** (templar). Each has their own sword and shield, and all three have frames for drawing a staff. Their fighting roles and stats are not decided.
- Four monsters have art: fat rabbit, cave mole, crystal golem, assassin vine. See `assets/monsters/README.md`. None has stats or rules.

## What is already decided

Do not reopen these without a reason you can state.

**The battle system**
- **A ready bar.** Each character acts when their bar fills, as in Final Fantasy VI and Chrono Trigger. Brad chose this over a hex grid on 2026-11-20. Both of those games have well-loved randomizers, which is part of why.

**Stats**
- Three vital statistics: **Body** (consistent, sustained effort), **Agility** (bursts of speed), **Mind** (abilities). The goal is that all three matter in both scenes.
- **100 / 100 / 100 is a typical start, 80 is the minimum, about 200 is near the top.**
- For movement, stats count in steps of 20, shown to the player as **levels 1 to 7**. The player never sees the values behind movement.
- **Between levels, movement does not change.** Agility 107 moves exactly like Agility 100. **What a stat gains between levels is combat stats or discrete abilities.** That sentence is your brief: every point of a stat should be worth something in a fight.
- One exception already exists: the stamina bar equals Body exactly, point for point.
- Movement options are only changed when sleeping, so players get used to how their character feels. Nothing says combat stats must wait for sleep. That is open.

**Mana**
- It does not recover on its own; only consumables or sleep restore it.
- It is what you fight with. It also pays for **Imagine**, the leader's ability to try the next few seconds before doing them, so mana spent exploring is mana not available in a fight.
- Epic players will farm gold and potions until mana stops gating them. Randomizer players cannot afford the time.

**Hands**
- The game tracks what is in each hand. Free hands make climbing possible, so the leader often has a weapon put away.
- **If a fight starts while the leader's gear is sheathed, they are disadvantaged.** What "disadvantaged" means is yours to define.
- Changing other gear needs the Safe state (see `design.md`, "Safety states").

**Losing**
- A hazard in the platformer view hurts the whole party a little, and can finish a character off.
- If the leader faints, the player picks a new leader at the next anchor.
- **If the whole party faints,** the game goes back to the last sleep, with everything as it was then. In the randomizer that costs time and does not end the run.
- Revive consumables should exist.

**Monsters and farming**
- **Monster stats scale with where a block falls in the run, not with the block.** Block 12 rolled first uses block 1 monster stats.
- Cleared areas refill with an easier "returned" threat each time the player leaves the map and comes back. That is what makes gold and experience farmable, on purpose.
- Monsters come in types with different rules for noticing the player. That is a platformer-view matter, but a monster's type may also want to mean something in a fight.

**Randomness**
- There is a **run seed** and a **battle seed**, fixed and shareable.
- **Battle randomness also uses the frame counter,** which runs for the whole run and does not reset on reload. So a fight replays identically only if everything is identical to the frame, and a retried fight usually goes differently.
- Everything must be deterministic given the seeds, the frame counter and the inputs. No clocks, no unseeded random numbers.

## Where the two scenes meet

This is the whole boundary. Agree changes to it with Brad, and he will tell the design conversation.

**How a fight starts.** Touching a monster or its trigger zone makes a fight pending. It begins the next time the leader is **anchored**: somewhere gravity is not acting on them, which includes hanging from a ledge. The edge cases are deferred and belong to the design conversation.

**What the platformer view hands to a fight:**
- The three party members, each with stats, health, mana and equipped gear.
- For the leader, what is actually in each hand at that moment.
- Which monsters are in the fight. More than one may have been touched.
- The block's position in the run, for monster scaling.
- The battle seed and the current frame count.
- Not yet decided, and yours to ask for if you want it: the leader's stamina at that moment, and how the fight began (who touched whom, and whether the monster had noticed the player).

**What a fight hands back:**
- Won, or the whole party fainted. Whether running away exists is open.
- Each member's health and mana, and who has fainted.
- Consumables used, and experience and gold gained.
- Which monsters are gone, so their areas count as cleared.

After a win the leader is put back exactly where the fight began.

## What is open, in the order I would take it

1. **The shape of a turn.** Does time stop while the player picks a command? What fills the ready bar, and how fast? This decides how long a fight takes, which matters more here than in most RPGs: a 60 to 90 minute run cannot carry long fights.
2. **How long a fight should last,** for an ordinary monster and for one that cannot be skipped. Get a number from Brad early; most later numbers follow from it.
3. **What Body, Agility and Mind do in a fight.** The themes are sustained effort, bursts of speed, and abilities. The obvious first reading is health and defence, ready-bar speed, and mana and abilities, but that is my guess, not a decision.
4. **What one point of a stat is worth** between movement levels.
5. **How stats rise.** Experience exists and can be farmed. Nothing else is decided.
6. **What sheathed gear costs** at the start of a fight.
7. **Whether you can run away,** and what it costs.
8. **Does stamina carry into a fight?** A leader who arrives exhausted could start behind. This ties movement to combat, which Brad may like or may find punishing.
9. **Gear and abilities.** The randomizer shuffles the order they are unlocked in, so no fight can depend on the player having a particular one.
10. **Each monster's stats and behaviour,** starting with the four that have art.

Two things to keep in view throughout:

- **The ninja and two soldiers.** The leader is built for movement and may be the weakest fighter. The design should make that a real choice, not a mistake.
- **Mind must not be the dump stat or the only stat.** It gates abilities in both scenes and feeds mana. Check each proposal for what a low-Mind party and a high-Mind party actually do differently.

## How to work

- Movement was designed by building a small browser prototype (the feel lab), agreeing how it felt, then recording runs for the programmer to reproduce exactly. That worked. I would do the same for combat: a small battle prototype Brad can play, in its own folder, once items 1 to 3 are settled on paper. Do not build before then.
- Until there is a prototype, `docs/combat.md` is the deliverable. Mark every statement as decided, proposed or open, and date decisions.
- The programmer reads `CLAUDE.md` and `docs/build-spec.md`. Battles are outside the first build. When combat is ready to build, the design conversation will fold it into the spec; write `combat.md` exactly enough that this is a copying job.
