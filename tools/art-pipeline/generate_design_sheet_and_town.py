#!/usr/bin/env python3
"""
LEGADO: PERU - Iteracion visual sobre Milestone 2.1
Genera (a) una hoja de diseno de personaje ("character design sheet") con
las etapas Ninez/Juventud de Mateo, ciclos de caminata y detalle de items, y
(b) un mockup de pueblo ampliado con mas variedad arquitectonica, en el
mismo espiritu que las referencias mostradas por el usuario pero 100%
pixel art ORIGINAL (nada derivado de Pokemon ni de ningun otro juego).

Reutiliza generate_milestone_2_1_assets.py como modulo (no duplica logica
de dibujo de tiles/personajes).
"""

import os
import sys
from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(__file__))
import generate_milestone_2_1_assets as base  # noqa: E402

REPO_ROOT = base.REPO_ROOT
PROGRESS_OUT = base.PROGRESS_OUT
TILES_OUT = base.TILES_OUT
PALETTE = base.PALETTE

# El font bitmap por defecto de PIL no tiene glifos acentuados (ñ, í, ó...);
# esta hoja de diseño SÍ necesita texto en español legible (a diferencia de
# los mockups de juego, que ya evitan tildes). Usamos una TTF del sistema.
_FONT_PATH = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
_FONT_BOLD_PATH = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
_font_cache = {}


def font(size, bold=False):
    key = (size, bold)
    if key in _font_cache:
        return _font_cache[key]
    path = _FONT_BOLD_PATH if bold else _FONT_PATH
    try:
        f = ImageFont.truetype(path, size)
    except OSError:
        f = ImageFont.load_default()
    _font_cache[key] = f
    return f


def save(img, path):
    img.save(path)
    print("  ->", os.path.relpath(path, REPO_ROOT))


# ---------------------------------------------------------------------------
# Hoja de diseño de personaje
# ---------------------------------------------------------------------------

BG = (240, 231, 210, 255)
INK = (58, 42, 34, 255)
INK_SOFT = (110, 92, 70, 255)
WARN = (176, 74, 46, 255)
RULE = (190, 172, 142, 255)


def build_character_design_sheet():
    W = 1040
    sheet = Image.new("RGBA", (W, 10), BG)
    y = 0

    def grow(extra_h, color=BG):
        nonlocal sheet
        new_sheet = Image.new("RGBA", (W, sheet.height + extra_h), color)
        new_sheet.paste(sheet, (0, 0))
        sheet = new_sheet

    def draw():
        return ImageDraw.Draw(sheet)

    # --- Header ---
    grow(92)
    d = draw()
    d.rectangle([0, 0, W - 1, 91], fill=(58, 42, 34, 255))
    d.text((20, 14), "MATEO SALAZAR — LIFE STAGES DESIGN SHEET (prototipo)", fill=BG, font=font(20, bold=True))
    d.text((20, 42), "Original Salazar · Pisco/Paracas, Perú · Inicia 1820 · Pixel art ORIGINAL (no Pokémon ni ningún otro juego)", fill=(200, 184, 160, 255), font=font(13))
    d.text((20, 64), "Estado: NEEDS_HISTORICAL_VALIDATION (vestimenta sin verificar aún) — ver HISTORICAL_RESEARCH.md", fill=(224, 150, 120, 255), font=font(13))
    y = 92

    # --- Stage rows ---
    def add_stage_row(title, subtitle, spec, draw_fn, char_w, char_h, scale=5):
        nonlocal y
        top_margin, header_h, bottom_margin = 14, 60, 18
        row_h = top_margin + header_h + char_h * scale + bottom_margin
        grow(row_h)
        dctx = draw()
        dctx.line([(0, y), (W, y)], fill=RULE, width=2)
        dctx.text((20, y + top_margin), title, fill=INK, font=font(17, bold=True))
        dctx.text((20, y + top_margin + 22), subtitle, fill=INK_SOFT, font=font(12))

        col_x = 230
        sprite_y = y + top_margin + header_h
        for label, direction in (("FRONT", "down"), ("SIDE (3/4)", "left")):
            dctx.text((col_x, sprite_y - 20), label, fill=INK, font=font(13, bold=True))
            for frame in range(3):
                frame_img = draw_fn(spec, direction, frame, char_w, char_h)
                big = base.upscale(frame_img, scale)
                sheet.paste(big, (col_x + frame * (char_w * scale + 8), sprite_y), big)
            col_x += 3 * (char_w * scale + 8) + 40

        y += row_h

    add_stage_row(
        "1. NIÑEZ (~7 años)", "Canvas 24×30 · proporción chibi acentuada · descalzo · sin accesorios",
        base.MATEO_CHILD_SPEC, base.draw_child, 24, 30, scale=5,
    )
    add_stage_row(
        "2. JUVENTUD (17 años) — etapa jugable del vertical slice", "Canvas 24×36 · faja + bolso de viaje · sombreado con dithering",
        base.MATEO_SPEC, base.draw_character, 24, 36, scale=5,
    )

    # --- Item detail callouts ---
    icon_scale = 4
    icon_h = 24 * icon_scale
    item_row_h = 20 + 20 + icon_h + 22 + 16
    grow(item_row_h)
    d = draw()
    d.line([(0, y), (W, y)], fill=RULE, width=2)
    d.text((20, y + 16), "ITEM DETAIL", fill=INK, font=font(17, bold=True))
    icons = [
        ("Satchel / bolso de viaje", base.icon_satchel()),
        ("Faja / sash", base.icon_sash()),
        ("Sello familiar (Diario de la Familia)", base.icon_family_seal()),
    ]
    x = 20
    icon_top = y + 46
    for label, icon in icons:
        big = base.upscale(icon, icon_scale)
        sheet.paste(big, (x, icon_top), big)
        d.text((x, icon_top + big.height + 6), label, fill=(80, 64, 50, 255), font=font(12))
        x += big.width + 70
    y += item_row_h

    # --- Palette swatch ---
    swatch_row_h = 20 + 34 + 20 + 16
    grow(swatch_row_h)
    d = draw()
    d.line([(0, y), (W, y)], fill=RULE, width=2)
    d.text((20, y + 16), "PALETA (Mateo, etapa Juventud)", fill=INK, font=font(17, bold=True))
    swatch_keys = [
        ("Piel", "skin_mateo"), ("Piel sombra", "skin_mateo_shadow"), ("Cabello", "hair_mateo"),
        ("Camisa", "shirt_mateo"), ("Camisa sombra", "shirt_mateo_shadow"), ("Pantalón", "trouser_mateo"),
        ("Faja", "sash_mateo"), ("Bolso", "satchel_bag"), ("Sello", "family_seal"),
    ]
    x = 20
    swatch_top = y + 44
    for label, key in swatch_keys:
        color = PALETTE[key]
        d.rectangle([x, swatch_top, x + 46, swatch_top + 34], fill=color, outline=(40, 30, 26, 255), width=2)
        d.text((x, swatch_top + 40), label, fill=(80, 64, 50, 255), font=font(11))
        x += 88
    y += swatch_row_h

    # --- Footer note ---
    grow(34)
    d = draw()
    d.text((20, y + 10), "Generado por tools/art-pipeline/generate_design_sheet_and_town.py — pixel art programático, sin IA generativa de imágenes.", fill=(120, 104, 84, 255), font=font(11))

    return sheet


# ---------------------------------------------------------------------------
# Pueblo ampliado (sigue siendo un MOCKUP de Milestone 2.1, no el HUB
# completo de Milestone 2.3 — mas variedad para validar "no todo es la
# misma casa recoloreada", inspirado en composicion/densidad, NUNCA en el
# diseño concreto de Littleroot Town).
# ---------------------------------------------------------------------------

def tile_wall_adobe_variant(size=16):
    """Segunda variante de pared (almacén): tono más frío/gris, junta distinta."""
    img = base.new_canvas(size, size)
    scale = size // 16
    base_color = (196, 186, 162, 255)
    shadow = (164, 152, 128, 255)
    base.rect(img, 0, 0, size - 1, size - 1, base_color)
    for y in range(0, size, 5 * scale):
        for x in range(0, size):
            base.px(img, x, min(y, size - 1), shadow)
    return img


def tile_roof_variant(size=16):
    img = base.new_canvas(size, size)
    scale = size // 16
    c1, c2 = (120, 96, 70, 255), (94, 72, 50, 255)
    for y in range(size):
        band = (y // (2 * scale)) % 2
        base.rect(img, 0, y, size - 1, y, c1 if band == 0 else c2)
    return img


def prop_bush(size=16):
    img = base.new_canvas(size, size)
    cx, cy, r = size // 2, size - size // 3, size // 2 - 1
    for y in range(size):
        for x in range(size):
            d2 = (x - cx) ** 2 + (y - cy) ** 2
            if d2 <= r * r:
                shade = PALETTE["leaf_dark"] if (x * 3 + y) % 5 == 0 else PALETTE["leaf_light"]
                base.px(img, x, y, shade)
    return base.outline_silhouette(img, PALETTE["outline"])


def prop_well(size=16):
    img = base.new_canvas(size, size)
    base.rect(img, 2, 6, size - 3, size - 2, PALETTE["adobe_wall_shadow"])
    base.rect(img, 3, 7, size - 4, size - 3, PALETTE["water_mid"])
    base.rect(img, 1, 2, size - 2, 4, PALETTE["trunk"])
    return base.outline_silhouette(img, PALETTE["outline"])


def build_extended_town_mockup():
    tile_size = 16
    cols, rows = base.COLS, base.ROWS
    layout = base.tile_grid_layout()

    tiles = {
        "grass": base.tile_grass(tile_size), "path": base.tile_dirt_path(tile_size),
        "sand": base.tile_sand(tile_size), "water": base.tile_water(tile_size, 0),
        "wall": base.tile_wall_adobe(tile_size), "roof": base.tile_roof_thatch(tile_size),
        "roof_dark": base.tile_roof_thatch(tile_size), "door": base.tile_door(tile_size),
    }

    canvas = Image.new("RGBA", (cols * tile_size, rows * tile_size), PALETTE["water_dark"])
    for (col, row), kind in layout.items():
        canvas.paste(tiles[kind], (col * tile_size, row * tile_size))

    # Segundo edificio: ALMACEN (bodega familiar), mas pequeno y con paleta fria distinta
    # (prompt §22: "Almacén" como lugar mínimo del futuro HUB) — se anticipa aquí solo
    # como prueba de variedad, no como el HUB completo de Milestone 2.3.
    wall_v = tile_wall_adobe_variant(tile_size)
    roof_v = tile_roof_variant(tile_size)
    warehouse_cols = range(14, 18)
    for col in warehouse_cols:
        canvas.paste(roof_v, (col * tile_size, 3 * tile_size))
        canvas.paste(wall_v, (col * tile_size, 4 * tile_size))
        canvas.paste(wall_v, (col * tile_size, 5 * tile_size))

    # Arbustos y pozo (variedad de vegetación/mobiliario urbano, no solo árboles repetidos)
    bush = prop_bush(tile_size)
    for col, row in [(9, 6), (13, 5), (5, 8)]:
        canvas.paste(bush, (col * tile_size, row * tile_size), bush)

    well = prop_well(tile_size)
    canvas.paste(well, (17 * tile_size, 6 * tile_size), well)

    # Vegetación/decoración ya existente
    tree = base.prop_tree(tile_size * 2)
    for col, row in [(2, 4), (18, 8), (3, 8)]:
        canvas.paste(tree, (col * tile_size, row * tile_size - tile_size), tree)

    # Personajes (mismo tamaño nativo 24x36 / 24x30 que la hoja de diseño; en el mundo
    # 2D real Unity los escala vía PPU=16, aquí replicamos esa proporción a mano:
    # 1.5 tiles de ancho x 2.25 tiles de alto para la etapa Juventud).
    char_w, char_h = int(tile_size * 1.5), int(tile_size * 2.25)
    mateo = base.draw_character(base.MATEO_SPEC, "down", 1, char_w, char_h)
    canvas.paste(mateo, (8 * tile_size - (char_w - tile_size) // 2, 8 * tile_size - (char_h - tile_size)), mateo)

    npc = base.draw_character(base.NPC_FISHERMAN_SPEC, "left", 0, char_w, char_h)
    canvas.paste(npc, (6 * tile_size - (char_w - tile_size) // 2, 9 * tile_size - (char_h - tile_size)), npc)

    trader_spec = dict(base.NPC_FISHERMAN_SPEC)
    trader_spec.update(
        shirt=(90, 74, 130, 255), shirt_shadow=(70, 56, 106, 255),
        hat=None, hair_style="short", hair=(40, 30, 24, 255),
    )
    trader = base.draw_character(trader_spec, "down", 0, char_w, char_h)
    canvas.paste(trader, (15 * tile_size - (char_w - tile_size) // 2, 6 * tile_size - (char_h - tile_size)), trader)

    upscaled = base.upscale(canvas, 3)
    labeled = base.add_label_bar(upscaled, [
        "PISCO / PARACAS - 1820 (concepto, compresion geografica - ver HISTORICAL_RESEARCH.md)",
        "Mockup ampliado: casa + almacen + pozo + arbustos + 3 personajes (no es el HUB completo de Milestone 2.3)",
    ])
    return labeled


def main():
    print("Generando hoja de diseño de personaje ...")
    sheet = build_character_design_sheet()
    save(sheet, os.path.join(PROGRESS_OUT, "06_character_design_sheet.png"))

    print("Generando mockup de pueblo ampliado ...")
    town = build_extended_town_mockup()
    save(town, os.path.join(PROGRESS_OUT, "07_pueblo_ampliado.png"))

    print("\nListo.")


if __name__ == "__main__":
    main()
