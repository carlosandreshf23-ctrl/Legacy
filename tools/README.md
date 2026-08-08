# tools

Herramientas de soporte al desarrollo de LEGADO: PERÚ (fuera del cliente Unity y del backend de juego).

**Estado actual**: vacío. Se puebla progresivamente según lo requiera cada fase. Uso previsto:

- Validadores de datos: comprobar que los assets de `game-client/Assets/_Project/Data/Historical/` cumplen el esquema de `docs/03_DATA_MODEL.md` (campos obligatorios como `HistoricalStatus`, `Modifiability`, `TimelineID`, `SourceReferences` en eventos `HARD`) antes de integrarse.
- Validadores de presupuesto de rendimiento por región (`docs/10_MOBILE_PERFORMANCE.md`), contra los objetivos LOW/MID/HIGH ya definidos.
- Scripts de importación/normalización de datos históricos de investigación hacia el formato de assets del proyecto.
- Scripts de CI (lint, tests de sistemas C# puros que no requieren el Editor de Unity).
