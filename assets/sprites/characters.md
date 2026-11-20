# Characters: art reference

The art tool (Meshy) remembers nothing between chats. This file is what keeps a character looking the same across sessions.

## How to use this in a new Meshy chat

1. Upload the character's reference picture from `references/`.
2. Paste that character's block from this file.
3. Paste the preamble from `prompt_template.md`, then the list of poses you want.
4. For gear, also upload `lucca/source/Meshy_AI_Lucca_Weapons_Equipment_Sheet.png`.

This gets close, not exact. Expect small drift in size and colour between sheets, and check every sheet before using it.

## Rules for every character

- Side view, facing right. The game mirrors him to face left.
- Empty closed fists unless the sheet is specifically about an item. Items are separate pictures laid over the hands, so one sword works with every animation.
- The art is a detailed pixel-art style but is not on a true pixel grid. Do not ask for exact pixel sizes; Meshy does not honour them. Give sizes relative to Lucca instead ("same height as Lucca", "15% taller").
- Sheets are 1536 x 864. A standing figure should be about 36% of the sheet's height, which is what Lucca is.

---

## Lucca

- **Reference picture:** `references/lucca_anchor.png` (first frame of the walk sheet)
- **Archetype:** agile fighter / swordsman (read from his reference picture; stats not yet decided)
- **Height in the game:** 1.9 tiles
- **Size on a sheet:** about 307 pixels tall and 140 wide standing, on a 1536 x 864 sheet
- **Hair:** shoulder-length, wavy, blue-black
- **Outfit:** long navy-teal wrap robe with wide sleeves, dark grey waist sash
- **Legs and boots:** dark grey fitted trousers, dark laced boots
- **Skin:** light
- **Style:** chunky pixel art, crisp dark outline
- **Sheets so far:** see `lucca/README.md`

---

## Nina

- **Reference picture:** `references/nina_anchor.png` (first frame of her walk sheet)
- **Archetype:** not yet decided
- **Height in the game:** 1.7 tiles
- **Size on a sheet:** about 321 pixels tall standing, hood up, on a 1536 x 864 sheet (Lucca is 307)
- **Hair:** long, blonde, worn loose under the hood
- **Outfit:** pale grey-white hooded wrap robe with wide sleeves, hood up; brown leather belt
- **Legs and boots:** dark charcoal trousers, brown wrapped boots
- **Skin:** light
- **Distinguishing features:** the hood stays up in every frame
- **Style:** chunky pixel art, crisp dark outline (same as Lucca)
- **Sheets so far:** see `nina/README.md`

---

## Ramorand

- **Reference picture:** `references/ramorand_anchor.png` (first frame of his walk sheet)
- **Archetype:** not yet decided
- **Height in the game:** not yet decided
- **Size on a sheet:** about 319 pixels tall standing, on a 1536 x 864 sheet, and much broader than Lucca
- **Hair:** bald, with a short dark beard
- **Outfit:** full silver plate armour with a high collar; long navy cape; navy tabard with silver trim hanging from the belt
- **Legs and boots:** dark trousers under plate greaves and plate boots
- **Skin:** dark brown
- **Hands:** bare fists, no gauntlets over the hands
- **Distinguishing features:** the cape trails behind him in every frame
- **Style:** chunky pixel art, crisp dark outline (same as Lucca)
- **Sheets so far:** see `ramorand/README.md`

---

## Template for a new character

Copy this block, fill it in, and add a reference picture to `references/` once the first sheet comes back.

```
## [Name]

- Reference picture: references/[name]_anchor.png
- Archetype: [for example: heavy knight, archer, mage]
- Height in the game: [tiles; Lucca is 1.9]
- Size compared with Lucca: [for example: same height but broader; 15% taller]
- Hair:
- Outfit:
- Legs and boots:
- Skin:
- Distinguishing features: [anything that must appear in every frame]
- Style: chunky pixel art, crisp dark outline (same as Lucca)
```

When making a second character, upload Lucca's reference picture alongside the description so the style and scale match.
