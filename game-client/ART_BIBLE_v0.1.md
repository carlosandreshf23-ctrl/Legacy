# ART_BIBLE_v0.1.md — LEGADO: PERÚ

**Dirección artística definitiva (Fase 2):** Historical Peruvian Pixel Art RPG.
**Estado:** v0.1 — validado con la prueba visual de Milestone 2.1 (mockups compuestos a partir de pixel art real, ver `Progress/Phase_02/Milestone_01/`). Pendiente de aprobación humana antes de producir contenido a escala.

Referencia de sensación (**no de copia**, prompt Fase 2 — leer antes de nada): RPG portátil de Game Boy Advance de inicios de los 2000. Se toma prestada exclusivamente **escala, legibilidad, perspectiva, densidad visual y sensación de exploración** — cero sprites, tiles, paletas, mapas o música derivados de ningún juego existente. Todo el arte de este documento es original, generado pixel a pixel para este proyecto (ver `tools/art-pipeline/generate_milestone_2_1_assets.py`).

---

## 1. Perspectiva y cámara

- **Perspectiva cenital 3/4** (top-down clásico de RPG portátil), cámara 2D fija.
- La cámara **no rota nunca** y **no cambia de perspectiva libremente** — decisión definitiva (prompt §16), salvo necesidad muy específica y justificada en el futuro.
- Sigue al jugador con suavizado (`PixelPerfectCamera2D`, `SmoothDamp`), sin salirse del grid de píxeles (ver §3).

## 2. Escala: decisión de tile size

**Decisión: tiles de 16×16 px.** Ver comparación visual en `Progress/Phase_02/Milestone_01/04_comparacion_escala_tiles.png`.

| Criterio | 16×16 | 32×32 |
|---|---|---|
| Lectura en móvil (más tiles visibles a la vez → mejor sentido de espacio en pantallas pequeñas) | ✅ Mejor | Requiere cámara más alejada o pierde contexto |
| Legibilidad del sprite a esa escala | ✅ Suficiente para silueta+color diferenciador (validado, ver §4 y las capturas del NPC vs. Mateo) | Mayor detalle posible, pero no es necesario para el objetivo de legibilidad |
| Arquitectura reconocible (casas, calles) en un HUB compacto | ✅ Permite más edificios en pantalla sin sensación de "mundo vacío" | Menos edificios visibles simultáneamente a resolución interna comparable |
| Rendimiento (relevante para móvil gama baja, ver `10_MOBILE_PERFORMANCE.md`) | ✅ Texturas más pequeñas, más tiles por atlas, menos memoria por escena | Mayor huella de memoria por la misma área de mundo |
| Producción sostenible (un tile menos "espacio" que rellenar de detalle por pieza) | ✅ Más rápido de producir de forma consistente | Cada pieza exige más decisiones de detalle |

**Personajes**: 16 px de ancho × 24 px de alto (1 tile de ancho × 1.5 tiles de alto, dentro del rango pedido por el prompt §2). Se probó también un personaje de 32×48 (upscale 2x de la misma silueta, ver comparación) únicamente para validar cobertura de pantalla, no como diseño independiente — si en el futuro se decide producir contenido a 32px, se rediseñaría con detalle propio, no solo escalando.

**Pixels Per Unit en Unity: 16.** 1 unidad de mundo = 1 tile = 16 px nativos.

## 3. Pixel perfect (prompt §17)

- Filtro de textura: **Point (no filtrado)** en todos los sprites/tiles (`Phase2ArtImportSetup.cs` fuerza esto en el importador).
- Sin compresión de textura, sin mipmaps (irrelevante en 2D cenital y perjudicial para la nitidez).
- Cámara ortográfica con `orthographicSize` calculado a partir de una resolución interna de referencia (ver §5) y PPU=16, nunca un valor arbitrario.
- La posición de cámara se redondea al grid de píxeles cada frame (`PixelPerfectCamera2D.LateUpdate`) para eliminar temblor/blur de subpíxel al seguir al jugador.

## 4. Personajes

### Proporciones y silueta
- Cabeza: ~8×7 px. Torso: ~10×8 px. Piernas: ~7×6 px (incluye calzado). Proporción "chibi" leve (cabeza relativamente grande) — estándar del género de referencia para legibilidad a esta escala, no busca proporciones realistas.
- **Contorno de 1px** alrededor de toda la silueta (técnica genérica, calculada automáticamente por adyacencia de píxeles no transparentes — no depende de la forma dibujada), color `#2E221E` (marrón oscuro casi negro, no negro puro: encaja mejor con la paleta cálida de costa que un contorno frío).

### Diferenciación visual (prompt §4)
Cada personaje se distingue por combinación de:
- **Silueta** (sombrero sí/no, delantal sí/no, complexión).
- **Paleta** (color de camisa/pantalón, tono de piel, color de cabello).
- Ejemplo ya construido: Mateo (joven, sin sombrero, cabello oscuro corto, camisa clara) vs. NPC pescador (mayor, sombrero de ala ancha, cabello canoso, camisa oxidada/marrón, delantal). Ver `Progress/Phase_02/Milestone_01/02_personaje_y_npc.png`.
- Regla dura: nunca dos NPC importantes comparten silueta+paleta exacta (evita "todos son recolores del mismo sprite").

### Animaciones (prompt §3)
Mínimo por personaje en esta fase: **Idle + Walk** en las 4 direcciones (Down/Left/Right/Up), 3 frames por dirección (idle, stepA, stepB). `Right` se genera reflejando `Left` (`SpriteRenderer.flipX`) salvo que una prenda asimétrica lo impida — no se duplica arte a mano. Frame rate de animación: 6 fps (`SpriteDirectionAnimator`), similar a la cadencia de paso de un RPG portátil clásico.

Fuera de alcance de esta fase (prompt §3): Run, Horse, Combat, Work, Sit, Sleep, Injured, SpecialAnimations.

### `CharacterVisualDefinition` (preparación futura, prompt §5, §60-61)
No implementado como ScriptableObject todavía (se hace cuando el envejecimiento/multiplayer visual lo requiera, Fase 3+/Fase 9+), pero el pipeline de generación ya está desacoplado de forma que admite esos campos sin romper nada:
- `AgeRange` (ej. Mateo_17 / Mateo_25 / Mateo_40 / Mateo_60 — el sufijo de edad ya es parte de la convención de nombre de archivo desde ahora: `Mateo_17_down_0.png`).
- `SpriteSet` (el conjunto de 12 frames direccionales actual es la unidad mínima de `SpriteSet`).
- `Portrait`, `Outfit`, `ProfessionVariant`, `HistoricalPeriod` — campos a añadir cuando se necesiten; el generador Python ya separa "spec" (paleta+prendas) de "layout" (dirección+frame), por lo que soportar variantes es extender specs, no rediseñar el sistema.
- Multiplayer visual (prompt §60-61): como cada jugador ya es una instancia de `PlayerCharacterController` + `SpriteDirectionAnimator` con su propio conjunto de frames, nada impide instanciar 4 personajes simultáneos con specs distintas (una por familia) — no existe un "PlayerCharacterSprite" único hardcodeado.

## 5. Resolución (prompt §18)

- **Resolución interna de referencia: 320×180 px** (16:9 exacto, múltiplo limpio de tiles de 16px: 20×11.25 tiles visibles — en la práctica `PixelPerfectCamera2D` usa `referenceHeightPx=180`, dando `orthographicSize = 5.625`).
- La cámara cubre siempre la misma altura en unidades de mundo; el **ancho visible varía con el aspect ratio real del dispositivo** (más ancho en 16:9, algo más recortado en 19.5:9/20:9) — esto es preferible a estirar o dejar barras negras, y es el enfoque estándar de cámaras pixel-perfect en móvil.
- Pendiente de Milestone 2.3 en adelante: diseñar el HUB de Pisco/Paracas con un margen de seguridad para que ningún elemento interactivo importante quede solo visible en el recorte de pantallas más anchas (20:9) — anotado como criterio de diseño de nivel, no resuelto todavía en el mockup de esta fase.
- La interfaz (UI) es independiente de la resolución del pixel art: usa `CanvasScaler` en modo "Scale With Screen Size", no hereda el filtro Point de los sprites de mundo.

## 6. Paleta

### Principio general (prompt §51)
**No hay filtro "sepia porque es antiguo".** 1820 era tan colorido para quien lo vivía como 2026. Los colores representan materiales, clima y vestimenta reales — no una fotografía envejecida. La paleta de costa usada en Milestone 2.1 (ver `tools/art-pipeline/generate_milestone_2_1_assets.py`, diccionario `PALETTE`) es cálida y saturada dentro de un rango terroso/oceánico, no desaturada.

### Por macroregión (prompt §50 — preparado conceptualmente, solo Costa implementada en 2.1)

| Región | Enfoque de paleta | Estado |
|---|---|---|
| **Costa** | Árida, ocres/arena, azules oceánicos, vegetación puntual (no densa) | ✅ Implementada (paleta base de este documento) |
| **Sierra** | Mayor contraste, roca, tierra, vegetación de altura, variación por altitud | 🔲 Pendiente — no es solo un recolor: tiles y vegetación deben rediseñarse (relieve, terrazas, distinta arquitectura) |
| **Selva** | Vegetación densa y variada, humedad, ríos | 🔲 Pendiente — mismo principio: geometría de tile distinta, no solo paleta |

Regla dura ya anotada para cuando se aborden Sierra/Selva: **no convertir una región en un simple recolor de Costa.** Deben cambiar también las siluetas de tile (vegetación, relieve, materiales de construcción).

### Por época (prompt §51)
No se aplica ningún filtro global por año. Los cambios entre 1821 y 1920 (Milestone futuro, ver `04_WORLD_ARCHITECTURE.md` — `TemporalWorldState`) se representan con **tiles y props distintos** (nueva infraestructura, otra vestimenta, otro transporte), nunca con un post-proceso de color.

## 7. Arquitectura (tiles de construcción)

- Paredes: adobe con junta horizontal marcada cada 4px (patrón determinista, no ruido aleatorio) — lectura clara a distancia sin parecer una textura plana.
- Techos: cubierta a dos aguas simplificada con bandas horizontales alternas (tono claro/oscuro) sugiriendo materia vegetal trenzada (quincha/caña + torta de barro, forma de techado costero histórico frecuente — marcado `NEEDS_HISTORICAL_VALIDATION`, ver `HISTORICAL_RESEARCH.md`).
- Puerta: madera oscura enmarcada en el mismo tile de pared, nunca un hueco vacío.
- Regla de producción: cada edificio se arma combinando estos tiles base + variantes de color/orientación, no re-dibujando la construcción entera por edificio (sostenible para 10-20 edificios del Milestone 2.3).

## 8. Vegetación

- Árbol: prop suelto (no tileable), 32×32 px (2×2 tiles) con copa redondeada de dos tonos de verde alternados por paridad de píxel (evita look plano sin necesitar ruido real) y tronco corto.
- Vegetación de costa: puntual, no densa (dispersa entre construcciones y caminos), consistente con la paleta árida de la región (§6).

## 9. UI

- Cuadro de diálogo: panel oscuro semitransparente con doble borde (exterior oscuro + interior claro cálido), etiqueta de nombre del hablante superpuesta en la esquina superior, texto de cuerpo + indicador "toca para continuar". Ver mockup `03_interfaz_dialogo.png`.
- Fuente: bitmap simple, alta legibilidad a tamaño pequeño (pendiente de sustituir el placeholder por una fuente pixel-art propia en un milestone posterior — ver §Deuda técnica).
- La UI vive en su propio Canvas (`Screen Space Overlay`), completamente desacoplada de la resolución/escala del mundo pixel-art (§5).
- Retratos (`Portrait` de NPC importantes, prompt §27): no implementados todavía en 2.1; el `DialogueUIController` de Fase 1 ya reserva la estructura (nombre + texto) para añadir un `Image` de retrato sin rediseñar el panel.

## 10. Outlines y sombreado — resumen de reglas

- Outline: 1px, color cálido oscuro (no negro puro), calculado por silueta (dilatación de 1px sobre la máscara alfa).
- Sombreado: bloques planos de 2 tonos por material/prenda (luz/sombra), sin degradados ni dithering en esta fase — coherente con la referencia de RPG portátil clásico y con presupuesto de producción sostenible.

## 11. Escala de objetos (referencia rápida)

| Elemento | Tamaño nativo |
|---|---|
| Tile de terreno (suelo/camino/arena/agua) | 16×16 px |
| Tile de construcción (pared/techo/puerta) | 16×16 px |
| Personaje (bounding box) | 16×24 px |
| Árbol | 32×32 px (prop, no tile) |
| Poste/cerca | 16×16 px (prop delgado) |

## 12. Criterios de aprobación del estilo (prompt §64)

A validar visualmente por el usuario contra `Progress/Phase_02/Milestone_01/`:

- [ ] ¿Se siente como un RPG portátil clásico?
- [ ] ¿Los sprites son legibles?
- [ ] ¿Se siente peruano? (arquitectura de adobe/quincha, paleta costera, vestimenta sencilla de familia comerciante)
- [ ] ¿Se distingue la costa peruana? (arena, mar, aridez)
- [ ] ¿Mateo parece un habitante y no un héroe fantástico?
- [ ] ¿La escala funciona pensando en móvil?
- [ ] ¿Da ganas de recorrer ese mapa?

Si alguna respuesta es negativa: iterar sobre este documento y el pipeline de generación **antes** de construir el pueblo completo (Milestone 2.2+), tal como exige el prompt §66.

## 13. Deuda técnica / pendientes conocidos de v0.1

- Tipografía UI es la fuente bitmap por defecto de Unity (`LegacyRuntime.ttf`), no una fuente pixel-art propia todavía.
- No hay Y-sorting dinámico todavía (personajes/props usan `sortingOrder` fijo); necesario antes de tener múltiples personajes cruzándose en profundidad — anotado para Milestone 2.3 (pueblo con más NPC).
- Sierra y Selva son solo principio de diseño, sin tiles propios todavía.
- Retratos de diálogo no implementados.
- Colisión de árboles/postes es solo visual en 2.1 (sin `Collider2D` en los props); a añadir cuando el pueblo tenga más densidad y el jugador pueda intentar atravesarlos.
