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
- **Role in the game:** test character. Role and stats not yet decided.
- **Height in the game:** 1.9 tiles
- **Size on a sheet:** about 307 pixels tall and 140 wide standing, on a 1536 x 864 sheet
- **Hair:** shoulder-length, wavy, blue-black
- **Outfit:** long navy-teal wrap robe with wide sleeves, dark grey waist sash
- **Legs and boots:** dark grey fitted trousers, dark laced boots
- **Skin:** light
- **Style:** chunky pixel art, crisp dark outline
- **Sheets so far:** see `lucca/README.md`

---

## Template for a new character

Copy this block, fill it in, and add a reference picture to `references/` once the first sheet comes back.

```
## [Name]

- Reference picture: references/[name]_anchor.png
- Role in the game: [for example: heavy fighter, archer, mage]
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
