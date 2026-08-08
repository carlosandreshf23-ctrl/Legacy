#!/usr/bin/env python3
"""
LEGADO: PERU - Milestone 2.1 (LOOK & FEEL)
Genera pixel art ORIGINAL (tiles, sprite de Mateo, un NPC, y una escena
compuesta) programaticamente, pixel a pixel, con Pillow.

No copia ni deriva de assets de ningun otro videojuego: toda la geometria
y paleta se define aqui mismo. Es arte de PROTOTIPO para validar escala,
legibilidad y sensacion general (Art Bible v0.1); no es arte final.

Salidas:
  - game-client/Assets/_Project/Art/Generated/Tiles/*.png       (assets nativos para Unity)
  - game-client/Assets/_Project/Art/Generated/Characters/*.png  (spritesheets nativos)
  - game-client/Progress/Phase_02/Milestone_01/*.png|*.gif      (capturas/mockups compuestos)
"""

import os
from PIL import Image

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
TILES_OUT = os.path.join(REPO_ROOT, "game-client/Assets/_Project/Art/Generated/Tiles")
CHARS_OUT = os.path.join(REPO_ROOT, "game-client/Assets/_Project/Art/Generated/Characters")
PROGRESS_OUT = os.path.join(REPO_ROOT, "game-client/Progress/Phase_02/Milestone_01")

for d in (TILES_OUT, CHARS_OUT, PROGRESS_OUT):
    os.makedirs(d, exist_ok=True)

TRANSPARENT = (0, 0, 0, 0)

# ---------------------------------------------------------------------------
# Paleta conceptual COSTA (ver ART_BIBLE_v0.1.md) - tonos aridos/ocres + azul
# oceanico, sin filtro "sepia" (Regla del prompt: 1820 no era una foto vieja).
# ---------------------------------------------------------------------------
PALETTE = {
    "grass_light": (168, 176, 92, 255),
    "grass_dark": (140, 148, 70, 255),
    "dirt_light": (196, 160, 108, 255),
    "dirt_dark": (162, 128, 82, 255),
    "sand_light": (230, 210, 160, 255),
    "sand_dark": (208, 184, 132, 255),
    "water_light": (94, 156, 176, 255),
    "water_mid": (62, 118, 148, 255),
    "water_dark": (38, 84, 114, 255),
    "adobe_wall": (214, 178, 132, 255),
    "adobe_wall_shadow": (182, 146, 104, 255),
    "adobe_trim": (150, 108, 72, 255),
    "roof_thatch": (168, 124, 62, 255),
    "roof_thatch_dark": (132, 92, 42, 255),
    "wood_door": (108, 70, 40, 255),
    "wood_door_dark": (78, 48, 26, 255),
    "outline": (46, 34, 30, 255),
    "leaf_light": (104, 132, 66, 255),
    "leaf_dark": (72, 100, 46, 255),
    "trunk": (96, 66, 44, 255),
    "skin_mateo": (206, 158, 118, 255),
    "skin_mateo_shadow": (178, 130, 92, 255),
    "hair_mateo": (54, 38, 28, 255),
    "shirt_mateo": (224, 214, 188, 255),
    "shirt_mateo_shadow": (196, 182, 150, 255),
    "trouser_mateo": (120, 92, 60, 255),
    "trouser_mateo_shadow": (96, 72, 46, 255),
    "sandal": (90, 62, 40, 255),
    "skin_npc": (188, 140, 100, 255),
    "skin_npc_shadow": (160, 114, 78, 255),
    "hair_npc": (196, 196, 196, 255),
    "shirt_npc": (150, 92, 58, 255),
    "shirt_npc_shadow": (120, 70, 42, 255),
    "trouser_npc": (76, 78, 84, 255),
    "trouser_npc_shadow": (56, 58, 64, 255),
    "hat_npc": (196, 168, 108, 255),
    "hat_npc_shadow": (162, 136, 84, 255),
}


def new_canvas(w, h):
    return Image.new("RGBA", (w, h), TRANSPARENT)


def px(img, x, y, color):
    if 0 <= x < img.width and 0 <= y < img.height:
        img.putpixel((x, y), color)


def rect(img, x0, y0, x1, y1, color):
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            px(img, x, y, color)


def outline_silhouette(img, color):
    """Anade un contorno de 1px alrededor de la silueta no transparente (tecnica generica,
    no depende de la forma dibujada) - practica estandar de sprites RPG de la epoca de referencia."""
    w, h = img.size
    src = img.load()
    out = img.copy()
    dst = out.load()
    for y in range(h):
        for x in range(w):
            if src[x, y][3] != 0:
                continue
            neighbors = [(x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)]
            if any(0 <= nx < w and 0 <= ny < h and src[nx, ny][3] != 0 for nx, ny in neighbors):
                dst[x, y] = color
    return out


def upscale(img, factor):
    return img.resize((img.width * factor, img.height * factor), Image.NEAREST)


def save(img, path):
    img.save(path)
    print("  ->", os.path.relpath(path, REPO_ROOT))


# ---------------------------------------------------------------------------
# Tiles de terreno (16x16). Cada uno usa 2 tonos + un patron de "ruido"
# determinista (no aleatorio real, para que el tile sea reproducible) que
# rompe la monotonia sin parecer un recolor plano.
# ---------------------------------------------------------------------------

def _speckle(img, base, dark, seed_bits, size=16):
    rect(img, 0, 0, size - 1, size - 1, base)
    for i, (x, y) in enumerate(seed_bits):
        px(img, x, y, dark)


def tile_grass(size=16):
    img = new_canvas(size, size)
    rect(img, 0, 0, size - 1, size - 1, PALETTE["grass_light"])
    spots = [(2, 2), (5, 9), (9, 4), (12, 12), (3, 13), (13, 3), (7, 7), (10, 9)]
    for x, y in spots:
        if size == 32:
            x, y = x * 2, y * 2
            rect(img, x, y, x + 1, y + 1, PALETTE["grass_dark"])
        else:
            px(img, x, y, PALETTE["grass_dark"])
    return img


def tile_dirt_path(size=16):
    img = new_canvas(size, size)
    rect(img, 0, 0, size - 1, size - 1, PALETTE["dirt_light"])
    spots = [(1, 1), (4, 6), (8, 2), (11, 10), (6, 12), (13, 13), (2, 9)]
    for x, y in spots:
        if size == 32:
            x, y = x * 2, y * 2
            rect(img, x, y, x + 1, y + 1, PALETTE["dirt_dark"])
        else:
            px(img, x, y, PALETTE["dirt_dark"])
    return img


def tile_sand(size=16):
    img = new_canvas(size, size)
    rect(img, 0, 0, size - 1, size - 1, PALETTE["sand_light"])
    spots = [(3, 3), (7, 8), (10, 2), (13, 11), (1, 12), (9, 14)]
    for x, y in spots:
        if size == 32:
            x, y = x * 2, y * 2
            rect(img, x, y, x + 1, y + 1, PALETTE["sand_dark"])
        else:
            px(img, x, y, PALETTE["sand_dark"])
    return img


def tile_water(size=16, frame=0):
    img = new_canvas(size, size)
    rect(img, 0, 0, size - 1, size - 1, PALETTE["water_mid"])
    scale = size // 16
    wave_rows = [3, 9] if frame == 0 else [6, 12]
    for row in wave_rows:
        for x in range(0, size, 2 * scale):
            rect(img, x, row * scale, min(x + scale - 1, size - 1), row * scale, PALETTE["water_light"])
    for x in range(0, size, 4 * scale):
        rect(img, x, size - 1, min(x + scale - 1, size - 1), size - 1, PALETTE["water_dark"])
    return img


def tile_wall_adobe(size=16):
    img = new_canvas(size, size)
    rect(img, 0, 0, size - 1, size - 1, PALETTE["adobe_wall"])
    scale = size // 16
    for row_i, y in enumerate(range(0, size, 4 * scale)):
        offset = (2 * scale) if row_i % 2 else 0
        for x in range(-2 * scale + offset, size, 8 * scale):
            for lx in range(max(x, 0), min(x + 8 * scale, size)):
                px(img, lx, min(y, size - 1), PALETTE["adobe_wall_shadow"])
    return img


def tile_roof_thatch(size=16):
    img = new_canvas(size, size)
    scale = size // 16
    for y in range(size):
        band = (y // (2 * scale)) % 2
        color = PALETTE["roof_thatch"] if band == 0 else PALETTE["roof_thatch_dark"]
        rect(img, 0, y, size - 1, y, color)
    return img


def tile_door(size=16):
    img = new_canvas(size, size)
    scale = size // 16
    rect(img, 0, 0, size - 1, size - 1, PALETTE["adobe_wall"])
    rect(img, 3 * scale, 1 * scale, size - 1 - 3 * scale, size - 1, PALETTE["wood_door"])
    rect(img, 3 * scale, 1 * scale, size - 1 - 3 * scale, 1 * scale, PALETTE["wood_door_dark"])
    px(img, size - 1 - 4 * scale, (size // 2), PALETTE["wood_door_dark"])
    return img


def prop_tree(size=32):
    """Arbol como PROP (no tile repetible): ocupa 2x2 tiles visualmente en 16px o 1x1 grande en 32px."""
    img = new_canvas(size, size)
    scale = size / 16.0
    trunk_w = max(2, int(2 * scale))
    trunk_x0 = size // 2 - trunk_w // 2
    rect(img, trunk_x0, int(11 * scale), trunk_x0 + trunk_w - 1, size - 1, PALETTE["trunk"])
    cx, cy, r = size // 2, int(7 * scale), int(7 * scale)
    for y in range(size):
        for x in range(size):
            d2 = (x - cx) ** 2 + (y - cy) ** 2
            if d2 <= r * r:
                shade = PALETTE["leaf_dark"] if (x + y) % 5 == 0 else PALETTE["leaf_light"]
                px(img, x, y, shade)
    return outline_silhouette(img, PALETTE["outline"])


def prop_fence_post(size=16):
    img = new_canvas(size, size)
    scale = size / 16.0
    w = max(2, int(3 * scale))
    x0 = size // 2 - w // 2
    rect(img, x0, int(2 * scale), x0 + w - 1, size - 1, PALETTE["trunk"])
    rect(img, x0, int(2 * scale), x0 + w - 1, int(3 * scale), PALETTE["adobe_trim"])
    return outline_silhouette(img, PALETTE["outline"])


# ---------------------------------------------------------------------------
# Personajes. Canvas base 16x24 (1 tile de ancho x 1.5 tiles de alto, ver
# ART_BIBLE_v0.1.md §2). draw_character() es parametrico para poder producir
# tanto a Mateo como a un NPC con siluetas/paletas claramente distintas
# (prompt §4: evitar que los NPC parezcan recolores del mismo sprite).
# ---------------------------------------------------------------------------

MATEO_SPEC = dict(
    skin=PALETTE["skin_mateo"], skin_shadow=PALETTE["skin_mateo_shadow"],
    hair=PALETTE["hair_mateo"],
    shirt=PALETTE["shirt_mateo"], shirt_shadow=PALETTE["shirt_mateo_shadow"],
    trouser=PALETTE["trouser_mateo"], trouser_shadow=PALETTE["trouser_mateo_shadow"],
    sandal=PALETTE["sandal"], hat=None, apron=None, hair_style="short",
)

NPC_FISHERMAN_SPEC = dict(
    skin=PALETTE["skin_npc"], skin_shadow=PALETTE["skin_npc_shadow"],
    hair=PALETTE["hair_npc"],
    shirt=PALETTE["shirt_npc"], shirt_shadow=PALETTE["shirt_npc_shadow"],
    trouser=PALETTE["trouser_npc"], trouser_shadow=PALETTE["trouser_npc_shadow"],
    sandal=PALETTE["sandal"],
    hat=(PALETTE["hat_npc"], PALETTE["hat_npc_shadow"]),
    apron=PALETTE["adobe_trim"], hair_style="grey_short",
)


def draw_character(spec, direction, frame, w=16, h=24):
    img = new_canvas(w, h)
    s = w / 16.0

    def R(x0, y0, x1, y1, color):
        rect(img, int(x0 * s), int(y0 * s), int(x1 * s + s - 1), int(y1 * s + s - 1), color)

    leg_shift = 0
    arm_shift = 0
    if frame == 1:
        leg_shift = 1
    elif frame == 2:
        leg_shift = -1
    stride = 1 if frame == 1 else (-1 if frame == 2 else 0)

    # --- Piernas (dibujadas primero, quedan detras del torso) ---
    if direction in ("down", "up"):
        R(5, 18 + max(leg_shift, 0), 7, 22, spec["trouser"])
        R(9, 18 + max(-leg_shift, 0), 11, 22, spec["trouser_shadow"])
        R(5, 22 + max(leg_shift, 0), 7, 23, spec["sandal"])
        R(9, 22 + max(-leg_shift, 0), 11, 23, spec["sandal"])
    else:  # left/right (perfil) -> stride horizontal
        back_x = 6 - stride
        front_x = 8 + stride
        R(back_x, 18, back_x + 2, 22, spec["trouser_shadow"])
        R(front_x, 18, front_x + 2, 22, spec["trouser"])
        R(back_x, 22, back_x + 2, 23, spec["sandal"])
        R(front_x, 22, front_x + 2, 23, spec["sandal"])

    # --- Torso ---
    torso_top, torso_bottom = 10, 18
    R(3, torso_top, 12, torso_bottom, spec["shirt"])
    R(3, torso_top, 5, torso_bottom, spec["shirt_shadow"])
    if spec.get("apron"):
        R(5, torso_top + 2, 10, torso_bottom, spec["apron"])

    # Brazos
    if direction == "down":
        R(1, 11 + arm_shift, 2, 16, spec["shirt_shadow"])
        R(13, 11 - arm_shift, 14, 16, spec["shirt"])
        R(1, 15 + arm_shift, 2, 16, spec["skin"])
        R(13, 15 - arm_shift, 14, 16, spec["skin"])
    elif direction == "up":
        R(1, 11, 2, 16, spec["shirt_shadow"])
        R(13, 11, 14, 16, spec["shirt"])
    else:
        arm_x = 11 if direction == "right" else 1
        R(arm_x, 11, arm_x + 1, 16, spec["shirt_shadow"])
        R(arm_x, 15, arm_x + 1, 16, spec["skin"])

    # --- Cabeza ---
    head_top, head_bottom = 1, 9
    R(4, head_top, 11, head_bottom, spec["skin"])
    R(4, head_top, 6, head_bottom, spec["skin_shadow"])

    hair_style = spec.get("hair_style", "short")
    if direction == "up":
        R(3, head_top - 1, 12, head_bottom - 2, spec["hair"])
    else:
        R(3, head_top - 1, 12, head_top + 1, spec["hair"])
        if hair_style in ("short", "grey_short"):
            R(3, head_top - 1, 4, head_bottom - 3, spec["hair"])
            R(11, head_top - 1, 12, head_bottom - 3, spec["hair"])
        if direction == "left":
            R(3, head_top - 1, 6, head_bottom - 3, spec["hair"])
        elif direction == "right":
            R(9, head_top - 1, 12, head_bottom - 3, spec["hair"])

    # Ojos (solo de frente)
    if direction == "down":
        R(5, 5, 5, 5, PALETTE["outline"])
        R(9, 5, 9, 5, PALETTE["outline"])
    elif direction in ("left", "right"):
        eye_x = 6 if direction == "left" else 9
        R(eye_x, 5, eye_x, 5, PALETTE["outline"])

    # Sombrero (si aplica)
    if spec.get("hat"):
        hat_color, hat_shadow = spec["hat"]
        R(2, head_top - 3, 13, head_top - 1, hat_color)
        R(2, head_top - 3, 13, head_top - 3, hat_shadow)
        R(5, head_top - 4, 10, head_top - 3, hat_color)

    return outline_silhouette(img, PALETTE["outline"])


def build_spritesheet(spec, w=16, h=24):
    """4 direcciones x 3 frames, layout de fila = direccion (down,left,right,up), columna = frame."""
    directions = ["down", "left", "right", "up"]
    sheet = new_canvas(w * 3, h * len(directions))
    for row, direction in enumerate(directions):
        for col, frame in enumerate(range(3)):
            frame_img = draw_character(spec, direction, frame, w, h)
            sheet.paste(frame_img, (col * w, row * h), frame_img)
    return sheet


def save_individual_frames(spec, name, out_dir, w=16, h=24):
    """Exporta cada frame como PNG independiente (Mateo_17_down_0.png, ...) para que Unity
    los importe como un Sprite cada uno, sin depender de slicing de sprite-sheet en el
    importador (evita una fuente de error no verificable en este entorno sin Editor)."""
    directions = ["down", "left", "right", "up"]
    char_dir = os.path.join(out_dir, name)
    os.makedirs(char_dir, exist_ok=True)
    for direction in directions:
        for frame in range(3):
            img = draw_character(spec, direction, frame, w, h)
            save(img, os.path.join(char_dir, f"{name}_{direction}_{frame}.png"))


# ---------------------------------------------------------------------------
# Composicion de escena (prompt Milestone 2.1: Mateo + suelo + camino + casa
# + vegetacion + NPC + oceano/costa + UI minima). Grid conceptual 20x14.
# ---------------------------------------------------------------------------

COLS, ROWS = 20, 14
HOUSE_COLS = range(5, 12)   # 7 tiles de ancho
PATH_COLS = range(7, 10)    # 3 tiles de ancho


def tile_grid_layout():
    """Devuelve dict {(col,row): tile_kind} para la escena de prueba."""
    layout = {}
    for row in range(ROWS):
        for col in range(COLS):
            if row <= 1:
                layout[(col, row)] = "grass"
            elif row == 2 and col in HOUSE_COLS:
                layout[(col, row)] = "roof"
            elif row == 3 and col in HOUSE_COLS:
                layout[(col, row)] = "roof_dark"
            elif row in (4, 5) and col in HOUSE_COLS:
                layout[(col, row)] = "door" if (row == 5 and col == 8) else "wall"
            elif row <= 6:
                layout[(col, row)] = "grass"
            elif row in (7, 8, 9) and col in PATH_COLS:
                layout[(col, row)] = "path"
            elif row in (7, 8, 9):
                layout[(col, row)] = "grass"
            elif row in (10, 11):
                layout[(col, row)] = "sand"
            else:
                layout[(col, row)] = "water"
    return layout


def compose_scene(tile_size=16, with_ui=False, night=False, label=None):
    scale_factor = tile_size // 16
    tiles = {
        "grass": tile_grass(tile_size),
        "path": tile_dirt_path(tile_size),
        "sand": tile_sand(tile_size),
        "water": tile_water(tile_size, frame=0),
        "wall": tile_wall_adobe(tile_size),
        "roof": tile_roof_thatch(tile_size),
        "roof_dark": tile_roof_thatch(tile_size),
        "door": tile_door(tile_size),
    }

    canvas = Image.new("RGBA", (COLS * tile_size, ROWS * tile_size), PALETTE["water_dark"])
    layout = tile_grid_layout()
    for (col, row), kind in layout.items():
        canvas.paste(tiles[kind], (col * tile_size, row * tile_size))

    tree = prop_tree(tile_size * 2)
    for col, row in [(2, 4), (17, 3), (3, 8), (16, 7)]:
        canvas.paste(tree, (col * tile_size, row * tile_size - tile_size), tree)

    for col, row in [(4, 6), (12, 6)]:
        post = prop_fence_post(tile_size)
        canvas.paste(post, (col * tile_size, row * tile_size), post)

    char_w, char_h = tile_size, int(tile_size * 1.5)
    mateo_frame = draw_character(MATEO_SPEC, "up", 1, char_w, char_h)
    mateo_pos = (8 * tile_size, 8 * tile_size - (char_h - tile_size))
    canvas.paste(mateo_frame, mateo_pos, mateo_frame)

    npc_frame = draw_character(NPC_FISHERMAN_SPEC, "down", 0, char_w, char_h)
    npc_pos = (6 * tile_size, 9 * tile_size - (char_h - tile_size))
    canvas.paste(npc_frame, npc_pos, npc_frame)

    if night:
        overlay = Image.new("RGBA", canvas.size, (14, 16, 46, 190))
        canvas = Image.alpha_composite(canvas.convert("RGBA"), overlay)
        # Resplandor calido junto a la puerta (ventana/lampara encendida), da sensacion de vida nocturna.
        glow_x = 8 * tile_size - tile_size
        glow_y = 5 * tile_size - tile_size // 2
        glow = Image.new("RGBA", (tile_size * 2, tile_size), (255, 200, 120, 90))
        canvas.paste(glow, (glow_x, glow_y), glow)

    if with_ui:
        canvas = add_dialogue_ui(canvas, tile_size)

    if label:
        canvas = add_label_bar(canvas, label, tile_size)

    return canvas


def add_dialogue_ui(canvas, tile_size, speaker="Anciano Pescador", line="Buen dia, joven Salazar. El mar esta en calma hoy."):
    from PIL import ImageDraw
    canvas = canvas.copy()
    w, h = canvas.size
    box_h = int(h * 0.24)
    box = Image.new("RGBA", (w, box_h), (24, 20, 18, 235))
    draw_border(box, PALETTE["outline"], (230, 214, 180, 255))
    canvas.paste(box, (0, h - box_h), box)

    name_tag_w = min(int(w * 0.32), 150)
    name_tag_h = int(box_h * 0.28)
    name_tag = Image.new("RGBA", (name_tag_w, name_tag_h), (150, 92, 58, 255))
    draw_border(name_tag, PALETTE["outline"], (230, 214, 180, 255))
    name_tag_pos = (int(tile_size * 0.5), h - box_h - int(box_h * 0.22))
    d = ImageDraw.Draw(name_tag)
    d.text((6, name_tag_h // 2 - 5), speaker, fill=(255, 244, 224, 255))
    canvas.paste(name_tag, name_tag_pos, name_tag)

    d2 = ImageDraw.Draw(canvas)
    d2.text((16, h - box_h + 10), line, fill=(240, 232, 216, 255))
    d2.text((16, h - box_h + 26), "[toca para continuar]", fill=(170, 160, 140, 255))
    return canvas


def draw_border(img, outer, inner):
    w, h = img.size
    d_rect(img, 0, 0, w - 1, h - 1, outer)
    d_rect(img, 1, 1, w - 2, h - 2, inner)


def d_rect(img, x0, y0, x1, y1, color):
    for x in range(x0, x1 + 1):
        px(img, x, y0, color)
        px(img, x, y1, color)
    for y in range(y0, y1 + 1):
        px(img, x0, y, color)
        px(img, x1, y, color)


def add_label_bar(canvas, text_lines, tile_size=16):
    """Barra superior simple con texto (para distinguir mockups en la galeria de progreso)."""
    from PIL import ImageDraw
    canvas = canvas.copy()
    w = canvas.width
    bar_h = 14 * len(text_lines) + 8
    bar = Image.new("RGBA", (w, bar_h), (10, 10, 14, 235))
    d = ImageDraw.Draw(bar)
    for i, line in enumerate(text_lines):
        d.text((6, 4 + i * 14), line, fill=(255, 255, 255, 255))
    out = Image.new("RGBA", (w, canvas.height + bar_h), (0, 0, 0, 255))
    out.paste(bar, (0, 0))
    out.paste(canvas, (0, bar_h))
    return out


# ---------------------------------------------------------------------------
# GIFs (animacion real compuesta a partir de los sprites; no es una captura
# del motor, ver PHASE_02 docs).
# ---------------------------------------------------------------------------

def make_walk_cycle_gif(spec, path, preview_scale=8):
    frames = []
    for direction in ["down", "left", "up", "right"]:
        for frame in [0, 1, 0, 2]:
            img = draw_character(spec, direction, frame, 16, 24)
            canvas = Image.new("RGBA", (16, 24), (86, 140, 160, 255))
            canvas.paste(img, (0, 0), img)
            frames.append(upscale(canvas, preview_scale).convert("P", palette=Image.ADAPTIVE))
    frames[0].save(
        path, save_all=True, append_images=frames[1:], duration=180, loop=0, disposal=2
    )
    print("  ->", os.path.relpath(path, REPO_ROOT))


def make_scene_walk_gif(path, tile_size=16, steps=10):
    base_layout = tile_grid_layout()
    tiles = {
        "grass": tile_grass(tile_size), "path": tile_dirt_path(tile_size),
        "sand": tile_sand(tile_size), "water": tile_water(tile_size, 0),
        "wall": tile_wall_adobe(tile_size), "roof": tile_roof_thatch(tile_size),
        "roof_dark": tile_roof_thatch(tile_size), "door": tile_door(tile_size),
    }
    static_bg = Image.new("RGBA", (COLS * tile_size, ROWS * tile_size), PALETTE["water_dark"])
    for (col, row), kind in base_layout.items():
        static_bg.paste(tiles[kind], (col * tile_size, row * tile_size))
    tree = prop_tree(tile_size * 2)
    for col, row in [(2, 4), (17, 3), (3, 8), (16, 7)]:
        static_bg.paste(tree, (col * tile_size, row * tile_size - tile_size), tree)

    char_w, char_h = tile_size, int(tile_size * 1.5)
    start_row_px, end_row_px = 5 * tile_size, 9 * tile_size
    col_px = 8 * tile_size

    crop_box = (2 * tile_size, 2 * tile_size, 14 * tile_size, 12 * tile_size)

    frames = []
    for i in range(steps):
        t = i / (steps - 1)
        y = int(start_row_px + t * (end_row_px - start_row_px)) - (char_h - tile_size)
        frame_idx = 1 if i % 2 == 0 else 2
        mateo = draw_character(MATEO_SPEC, "down", frame_idx, char_w, char_h)
        frame_canvas = static_bg.copy()
        frame_canvas.paste(mateo, (col_px, y), mateo)
        cropped = frame_canvas.crop(crop_box)
        frames.append(upscale(cropped, 4).convert("P", palette=Image.ADAPTIVE))

    frames[0].save(
        path, save_all=True, append_images=frames[1:], duration=220, loop=0, disposal=2
    )
    print("  ->", os.path.relpath(path, REPO_ROOT))


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    print("Generando tiles nativos (16x16) para Assets/_Project/Art/Generated/Tiles ...")
    save(tile_grass(16), os.path.join(TILES_OUT, "tile_grass.png"))
    save(tile_dirt_path(16), os.path.join(TILES_OUT, "tile_dirt_path.png"))
    save(tile_sand(16), os.path.join(TILES_OUT, "tile_sand.png"))
    save(tile_water(16, 0), os.path.join(TILES_OUT, "tile_water_a.png"))
    save(tile_water(16, 1), os.path.join(TILES_OUT, "tile_water_b.png"))
    save(tile_wall_adobe(16), os.path.join(TILES_OUT, "tile_wall_adobe.png"))
    save(tile_roof_thatch(16), os.path.join(TILES_OUT, "tile_roof_thatch.png"))
    save(tile_door(16), os.path.join(TILES_OUT, "tile_door.png"))
    save(prop_tree(32), os.path.join(TILES_OUT, "prop_tree.png"))
    save(prop_fence_post(16), os.path.join(TILES_OUT, "prop_fence_post.png"))

    print("Generando spritesheets nativos (16x24, 4 direcciones x 3 frames) ...")
    save(build_spritesheet(MATEO_SPEC), os.path.join(CHARS_OUT, "Mateo_17_spritesheet.png"))
    save(build_spritesheet(NPC_FISHERMAN_SPEC), os.path.join(CHARS_OUT, "NPC_Fisherman_spritesheet.png"))

    print("Generando frames individuales (import directo en Unity, sin slicing) ...")
    save_individual_frames(MATEO_SPEC, "Mateo_17", CHARS_OUT)
    save_individual_frames(NPC_FISHERMAN_SPEC, "NPC_Fisherman", CHARS_OUT)

    print("Componiendo mockups de escena (Progress/Phase_02/Milestone_01) ...")
    save(
        add_label_bar(upscale(compose_scene(16), 3), ["LEGADO: PERU - Mockup compuesto (no captura de motor)", "01 - Vista general - Pisco/Paracas 1820 (prototipo)"]),
        os.path.join(PROGRESS_OUT, "01_vista_general.png"),
    )

    general_16 = compose_scene(16)
    crop = general_16.crop((5 * 16, 6 * 16, 11 * 16, 10 * 16))
    save(
        add_label_bar(upscale(crop, 8), ["02 - Mateo y NPC (detalle)"]),
        os.path.join(PROGRESS_OUT, "02_personaje_y_npc.png"),
    )

    save(
        add_label_bar(upscale(compose_scene(16, with_ui=True), 3), ["03 - Interfaz de dialogo (mockup)"]),
        os.path.join(PROGRESS_OUT, "03_interfaz_dialogo.png"),
    )

    save(
        add_label_bar(upscale(compose_scene(16, night=True), 3), ["05 - Variante nocturna (concepto)"]),
        os.path.join(PROGRESS_OUT, "05_variante_nocturna.png"),
    )

    print("Comparacion de escala de tile (16px vs 32px) ...")
    scene_16 = compose_scene(16).crop((4 * 16, 1 * 16, 13 * 16, 10 * 16))
    scene_32 = compose_scene(32).crop((4 * 32, 1 * 32, 13 * 32, 10 * 32))
    scene_16_up = upscale(scene_16, 4)
    scene_32_up = upscale(scene_32, 2)
    comparison = Image.new("RGBA", (scene_16_up.width + scene_32_up.width + 20, max(scene_16_up.height, scene_32_up.height) + 30), (10, 10, 14, 255))
    from PIL import ImageDraw
    d = ImageDraw.Draw(comparison)
    d.text((6, 6), "OPCION A: tiles 16x16 (elegida, ver ART_BIBLE_v0.1.md)", fill=(255, 255, 255, 255))
    comparison.paste(scene_16_up, (0, 26), scene_16_up)
    d.text((scene_16_up.width + 12, 6), "OPCION B: tiles 32x32 (comparacion)", fill=(255, 255, 255, 255))
    comparison.paste(scene_32_up, (scene_16_up.width + 12, 26), scene_32_up)
    save(comparison, os.path.join(PROGRESS_OUT, "04_comparacion_escala_tiles.png"))

    print("Generando GIFs (animacion compuesta, no captura de motor) ...")
    make_walk_cycle_gif(MATEO_SPEC, os.path.join(PROGRESS_OUT, "mateo_walk_cycle.gif"))
    make_scene_walk_gif(os.path.join(PROGRESS_OUT, "mateo_scene_walk.gif"))

    print("\nListo.")


if __name__ == "__main__":
    main()
