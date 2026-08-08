# PROGRESS.md — LEGADO: PERÚ

Actualizado al cierre de: **Milestone 2.1 — Look & Feel**.
Cálculo del porcentaje: 8 milestones definidos para la Fase 2 (2.1 a 2.8, prompt Fase 2 §13), cada uno vale 12.5 puntos. No es un número arbitrario — ver `DevelopmentRoadmap`/checklist de cada milestone para el detalle de qué cuenta como "completado".

```
LEGADO: PERÚ

FASE 2 — PRIMER VERTICAL SLICE VISUAL (Pisco/Paracas, sept. 1820)

██░░░░░░░░░░░░░░ 12.5 %
```

## COMPLETADO

✓ Dirección artística definitiva fijada (RPG 2D pixel-art cenital, Historical Peruvian Pixel Art RPG) — `ART_BIBLE_v0.1.md`
✓ Decisión de escala de tile (16×16, con comparación documentada frente a 32×32)
✓ Pixel art original: 8 tiles de terreno/construcción + 2 props + 2 personajes completos (Mateo 17 años, NPC pescador), 4 direcciones × 3 frames cada uno
✓ Cámara 2D cenital pixel-perfect (`PixelPerfectCamera2D`), sin rotación, siguiendo al jugador
✓ Movimiento libre sobre mapa de tiles (`CharacterMotor` reescrito sobre Rigidbody2D, sin grid forzado)
✓ Animador de sprites por dirección (`SpriteDirectionAnimator`), Right reflejando Left
✓ Escena jugable de prueba (Grid + Tilemap por capas: Ground/Water/Buildings, con colisión sólida en agua y construcciones)
✓ UI mínima (prompt de interacción) integrada en la escena 2D
✓ Mockups visuales compuestos a partir del pixel art real (vista general, personaje+NPC, diálogo, comparación de escala, variante nocturna) + 2 GIFs de animación
✓ Cámara 3D de Fase 1 retirada por completo (prompt §16)
✓ Iteración de fidelidad de personaje (feedback visual del usuario): sprite 1.5x más grande (24×36), dithering en costuras de sombra, tercer tono, faja + bolso, etapa "Niñez" (~7 años) como prueba del sistema de envejecimiento visual
✓ Hoja de diseño de personaje ("character design sheet") con ambas etapas, ciclos de caminata y detalle de items
✓ Mockup de pueblo ampliado (segundo edificio con paleta distinta, pozo, arbustos, 3 NPC diferenciados) — validación de variedad arquitectónica sin copiar Littleroot Town ni ningún otro mapa existente
✓ `ART_PROMPTS_EXTERNAL_AI.md`: vía de producción de arte de mayor fidelidad con generador de imágenes externo, para cuando se decida usarla

## EN DESARROLLO

→ (nada activo: Milestone 2.1 cerrado, a la espera de aprobación visual antes de continuar — prompt Fase 2 §66/§68)

## PENDIENTE

○ Milestone 2.2 — Casa Salazar (interior jugable, objetos interactivos, Diario de la Familia, familia inicial)
○ Milestone 2.3 — Pueblo (Pisco/Paracas Hub v0.1, 10-20 edificios)
○ Milestone 2.4 — Vida cotidiana (12-20 NPC con rutinas)
○ Milestone 2.5 — Economía simple (mercado, CurrencyPrototype)
○ Milestone 2.6 — Ruta exterior (exploración, niebla de conocimiento visual)
○ Milestone 2.7 — Rumores del desembarco (tensión narrativa gradual)
○ Milestone 2.8 — Pulido + vertical slice completo

---

## Historial de milestones

| Milestone | Estado | % Fase 2 |
|---|---|---|
| 2.1 — Look & Feel | ✅ Completado | 12.5% |
| 2.2 — Casa Salazar | 🔲 Pendiente | — |
| 2.3 — Pueblo | 🔲 Pendiente | — |
| 2.4 — Vida cotidiana | 🔲 Pendiente | — |
| 2.5 — Economía simple | 🔲 Pendiente | — |
| 2.6 — Ruta exterior | 🔲 Pendiente | — |
| 2.7 — Rumores del desembarco | 🔲 Pendiente | — |
| 2.8 — Pulido | 🔲 Pendiente | — |

## Evidencia visual disponible

`Progress/Phase_02/Milestone_01/`:
- `01_vista_general.png`, `02_personaje_y_npc.png`, `03_interfaz_dialogo.png`, `04_comparacion_escala_tiles.png`, `05_variante_nocturna.png`
- `06_character_design_sheet.png` (hoja de diseño Niñez/Juventud), `07_pueblo_ampliado.png` (mockup con 2 edificios + pozo + 3 NPC)
- `mateo_walk_cycle.gif`, `mateo_scene_walk.gif`

Nota de honestidad (ver `PHASE_01_IMPLEMENTATION.md` §0): estas imágenes son **mockups compuestos a partir del pixel art real** (generado por `tools/art-pipeline/generate_milestone_2_1_assets.py`), no capturas del motor Unity en ejecución — este entorno de desarrollo no tiene Unity Editor disponible. El código de la escena jugable (`Phase2LookAndFeelSceneBuilder.cs`) usa exactamente estos mismos assets con el mismo layout, así que lo que se vería al ejecutar el proyecto en Unity debería coincidir visualmente con estos mockups, pero eso está pendiente de confirmación real.
