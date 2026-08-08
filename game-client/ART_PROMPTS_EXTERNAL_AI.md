# ART_PROMPTS_EXTERNAL_AI.md — LEGADO: PERÚ

Prompts de referencia para producir arte de mayor fidelidad con una herramienta de generación de imágenes (Midjourney, Stable Diffusion, DALL·E, etc.) que el usuario ejecute externamente. **Yo no tengo acceso a ningún generador de imágenes en este entorno** — este documento existe para que tú (u otra persona con esa herramienta) puedas producir arte de referencia/mayor calidad, que luego yo integro al proyecto (import a Unity, ajuste de paleta/tamaño, slicing si aplica).

## Cómo usar este documento

1. Copia el prompt de la sección que necesites en tu herramienta de generación de imágenes.
2. Genera varias variantes y elige la que mejor encaje.
3. Pásame la imagen resultante (o dime dónde la dejaste) — la integro al pipeline (recorte, paleta, importación a Unity) y actualizo `ART_BIBLE_v0.1.md`/`HISTORICAL_RESEARCH.md` según corresponda.
4. Todo lo generado con estos prompts sigue sujeto a la Regla de la Fase 2: **nada de estilo Pokémon ni de ningún juego existente**, identidad visual peruana, sin filtro "sepia".

## Reglas para cualquier prompt que uses (inclúyelas siempre)

- `original character design, NOT Pokemon, NOT based on any existing game or franchise`
- `Peruvian coastal identity (Pisco/Paracas region, 1820), historically-inspired but not photorealistic`
- `flat/limited color palette suitable for a 2D top-down RPG, no sepia/vintage photo filter — colors are vivid and period-accurate, not faded`
- Si generas sprites de personaje: `top-down 3/4 perspective RPG sprite, NOT front-facing portrait` (los modelos de imagen tienden a dar retratos frontales por defecto; hay que insistir en la perspectiva cenital).

---

## 1. Mateo Salazar — Juventud (17 años, protagonista del vertical slice)

```
Original pixel-art RPG character sprite sheet, top-down 3/4 perspective (Game Boy Advance
era RPG style — Golden Sun/early 2000s handheld RPG aesthetic), NOT Pokemon, NOT based on
any existing game.

Character: Mateo Salazar, 17 years old, young man from a coastal Peruvian merchant/
transport family, Pisco/Paracas region, September 1820. NOT a fantasy hero — an ordinary
working-class young man. Short dark brown hair, warm olive-tan skin, cream/off-white linen
shirt, rolled brown trousers, leather sandals, a rust-red woven sash (faja) at the waist,
a small brown leather satchel bag crossed over one shoulder. No hat.

Sprite sheet layout: 4 directions (down/front, up/back, left, right), 3 frames per
direction (idle, walk step A, walk step B). Flat 2D pixel art, 1px dark brown outline
(not pure black), 3-tone cel shading with a light dithered transition between shadow and
base tones (no smooth gradients). Character proportions: slightly large head (mild chibi
ratio), body about 1.5 tiles wide x 2.25 tiles tall relative to a 16px tile grid.

Peruvian coastal identity, historically-inspired clothing (NEEDS validation against real
sources), vivid arid/ocher coastal color palette — no sepia or vintage filter.
```

## 2. Mateo Salazar — Niñez (~7 años)

```
Same original character (Mateo Salazar) as a ~7 year old child, same pixel-art RPG sprite
style (top-down 3/4, GBA-era handheld RPG aesthetic, NOT Pokemon). Exaggerated chibi
proportions (larger head, shorter torso and legs than the adult version), barefoot, simple
cream shirt, no sash, no satchel, no accessories yet. Same dark brown hair and warm skin
tone as the adult version for visual continuity across life stages. 4-direction sprite
sheet (down/up/left/right), 3 frames each (idle + 2 walk frames), same flat shading +
light dithering style as the reference adult sprite.
```

## 3. NPC — Anciano Pescador (Pisco/Paracas, ambientación)

```
Original pixel-art RPG NPC sprite, top-down 3/4 perspective, GBA-era handheld RPG
aesthetic, NOT Pokemon, NOT based on any existing game.

Character: elderly Peruvian fisherman, coastal Pisco/Paracas 1820. Grey short hair, wide-
brim straw/palm hat, weathered warm-brown skin, rust/ochre work shirt, dark grey rolled
trousers, simple apron. Distinct silhouette from a younger character (stockier build, hat,
apron) — must NOT read as a recolor of a young character. 4-direction sprite sheet
(down/up/left/right), 3 frames each. Same flat cel-shading + dithered transitions style,
1px dark brown outline. Vivid coastal color palette, no sepia filter.
```

## 4. NPC — Comerciante (mercado, referencia de variedad)

```
Original pixel-art RPG NPC sprite, top-down 3/4 perspective, GBA-era handheld RPG
aesthetic, NOT Pokemon.

Character: Peruvian coastal trader/shopkeeper, coastal Pisco/Paracas 1820. Short dark
hair, no hat, deep indigo/purple-dyed shirt (indicates a merchant who can afford dyed
cloth, contrast with working-class characters), dark grey trousers. Confident stance.
Silhouette and palette must be clearly distinct from both Mateo and the fisherman NPC —
no recolors. 4-direction sprite sheet, 3 frames each, same shading/outline style as the
other sprites for consistency.
```

## 5. Casa Salazar (exterior)

```
Original pixel-art RPG building exterior, top-down 3/4 perspective, GBA-era handheld RPG
tileset style, NOT Pokemon, NOT based on any existing game's specific building design.

A modest adobe (mud-brick) house with a thatched/quincha roof (cane and mud construction,
period-appropriate for Peruvian coast, 1820 — NEEDS_HISTORICAL_VALIDATION), single wooden
door, small windows. Warm ocher/tan adobe walls, brown woven-look roof texture. Building
footprint approximately 7x5 tiles on a 16px tile grid. Belongs to a modest merchant/
transport family, not wealthy, not a hovel — comfortable working-class home. Vivid arid
coastal palette, no sepia filter.
```

## 6. Pueblo / HUB de Pisco-Paracas (vista general, referencia de composición)

```
Original pixel-art RPG top-down map/town composition, GBA-era handheld RPG aesthetic
(compact, memorable, walkable town layout), NOT Pokemon, NOT based on Littleroot Town or
any other existing game's specific map design — inspiration is ONLY in density/perspective/
scale, not in any specific building shapes or palette from an existing game.

A small compact Peruvian coastal fishing/trading settlement, Pisco/Paracas, 1820. Mix of
modest adobe houses with thatched roofs, a small warehouse (almacén) with a slightly
different cooler-toned wall, a small market area, a well, scattered trees and low arid
coastal vegetation (not dense jungle — arid/ocher/scrub vegetation), dirt paths connecting
buildings, transitioning to a sandy beach and ocean at one edge. Populated with 2-3 small
NPC sprites doing everyday activities. Geographic compression is intentional (compact,
not 1:1 real scale) while keeping a believable sense of orientation. Vivid warm coastal
color palette (ochre, sand, adobe tan, ocean blue) — no sepia/vintage filter, no fantasy
elements.
```

## 7. Paleta de referencia (para mantener consistencia si generas variantes)

Si tu herramienta admite fijar una paleta o quieres pedir "usa estos colores", estos son los tonos ya validados en el pipeline programático (`tools/art-pipeline/generate_milestone_2_1_assets.py`):

| Uso | Hex |
|---|---|
| Piel (Mateo) | `#CE9E76` |
| Piel sombra | `#B2825C` |
| Cabello (Mateo) | `#362618` |
| Camisa (Mateo) | `#E0D6BC` |
| Camisa sombra | `#C4B696` |
| Pantalón | `#785C3C` |
| Faja/sash | `#A84E2E` |
| Bolso/satchel | `#704C2E` |
| Césped | `#A8B05C` |
| Camino de tierra | `#C4A06C` |
| Arena | `#E6D2A0` |
| Agua | `#3E7694` |
| Pared adobe | `#D6B284` |
| Techo (thatch) | `#A87C3E` |

## 8. Qué hago yo con el resultado

Una vez me pases las imágenes generadas:
1. Verifico que cumplen la Regla de originalidad (no debe ser reconociblemente derivado de un juego existente) antes de integrarlas.
2. Las recorto/ajusto a los tamaños de canvas del proyecto (24×36 personaje adulto, 24×30 niño, 16×16 tiles) si no vienen ya en esa escala.
3. Las importo a `Assets/_Project/Art/Generated/` (o una carpeta `Assets/_Project/Art/External/` separada, para distinguir origen) con la configuración pixel-perfect correspondiente.
4. Actualizo `ART_BIBLE_v0.1.md` marcando qué assets son "programáticos" (Pillow) vs. "generados externamente" (IA), y `HISTORICAL_RESEARCH.md` si el diseño pretende representar algo históricamente específico.
