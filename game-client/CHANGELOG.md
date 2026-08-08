# CHANGELOG.md — LEGADO: PERÚ

Formato libre, agrupado por build (prompt Fase 2 §46). Versionado: `MAJOR.PHASE.MILESTONE-dev` (ver §45 del prompt Fase 2 y `BuildInfo.cs`).

## BUILD 0.2.1-dev — Phase 2, Milestone 2.1 (Look & Feel)

### Added
- Dirección artística definitiva: RPG 2D pixel-art cenital (`ART_BIBLE_v0.1.md`).
- Pixel art original: 8 tiles (grass, dirt path, sand, water ×2 frames, adobe wall, thatch roof, door) + 2 props (árbol, poste) + 2 spritesheets de personaje completos (Mateo 17 años, NPC pescador — 4 direcciones × 3 frames).
- `PixelPerfectCamera2D`: cámara 2D cenital fija, pixel-perfect, sin rotación.
- `SpriteDirectionAnimator`: animación Idle/Walk por dirección, Right reflejando Left.
- `Phase2LookAndFeelSceneBuilder.cs` / `Phase2ArtImportSetup.cs`: generación procedural de la escena 2D de prueba y configuración de import pixel-perfect.
- `tools/art-pipeline/generate_milestone_2_1_assets.py`: pipeline de generación de pixel art original (reproducible, sin dependencias de assets externos).
- Documentación: `ART_BIBLE_v0.1.md`, `PROGRESS.md`, `HISTORICAL_RESEARCH.md`, `BuildInfo.cs`.

### Changed
- `CharacterMotor`: reescrito de `CharacterController` (3D) a `Rigidbody2D` (2D), movimiento libre sobre tiles.
- `PlayerCharacterController`: adaptado a 2D — `PlayerStateDto` reemplaza `rotY` por `facing` (string de dirección cardinal).
- `MovementConfig`: valores por defecto recalibrados para escala de tile 16px=1 unidad; eliminado `rotationSpeedDegPerSec` (ya no aplica: la dirección se comunica por sprite, no por rotación de transform).
- `InteractionSystem`: `Physics.OverlapSphere` (3D) → `Physics2D.OverlapCircleAll` (2D).
- `LocationTrigger`: `OnTriggerEnter(Collider)` → `OnTriggerEnter2D(Collider2D)`.
- `MapMVPController`: eje de profundidad de mundo `Z` (convención 3D) → `Y` (convención 2D cenital); campos renombrados `worldOriginXY`/`worldSizeXY`.

### Removed
- `ThirdPersonCameraController.cs` (cámara 3D en tercera persona de Fase 1 — prompt Fase 2 §16, retiro explícito).
- `Phase1SandboxSceneBuilder.cs` (generaba el blockout 3D de Fase 1; reemplazado por `Phase2LookAndFeelSceneBuilder.cs`).

### Fixed
- N/A (primer build de Fase 2; no hay regresiones reportadas todavía porque no se ha ejecutado en Unity real — ver limitación de entorno en `PHASE_01_IMPLEMENTATION.md` §0).

---

## BUILD 0.1.x-dev — Phase 1 (Fundación Jugable)

Ver `PHASE_01_IMPLEMENTATION.md` para el detalle completo de ese build. Resumen: personaje 3D controlable, cámara en tercera persona (retirada en 0.2.1-dev), interacción, inventario, diálogo, decisiones, calendario, guardado, UI base.
