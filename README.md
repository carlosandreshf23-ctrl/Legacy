# LEGADO: PERÚ

Videojuego histórico narrativo, multijugador (1-4) y generacional ambientado en Perú entre 1820-1920. Plataforma principal: móvil, con expansión posterior a PC. DLC futuro planeado: **LEGADO: PARADOJA** (no en desarrollo todavía).

Este repositorio se desarrolla por **fases funcionales** (vertical slices): cada fase produce una versión jugable, verificable y ampliable antes de avanzar a la siguiente. Ver `docs/12_DEVELOPMENT_ROADMAP.md` para el detalle completo y el estado actual.

## Estado del proyecto

**Fase 0 — Arquitectura del Proyecto**: completa.
**Fase 1 — Fundación Jugable**: código completo (ver [`game-client/PHASE_01_IMPLEMENTATION.md`](game-client/PHASE_01_IMPLEMENTATION.md)), pendiente de compilar/ejecutar y validar en un entorno con Unity Editor real — este entorno de desarrollo no tiene Unity/.NET instalado, ver limitación documentada en ese archivo §0.

## Documentación

| Documento | Contenido |
|---|---|
| [`docs/01_ARCHITECTURE.md`](docs/01_ARCHITECTURE.md) | Motor/stack elegido, capas del sistema, principios data-driven, identificadores persistentes, debug/testing/localización |
| [`docs/02_SYSTEMS_MAP.md`](docs/02_SYSTEMS_MAP.md) | Catálogo de sistemas, mapa de dependencias completo, clasificación de prioridades P0-P3 |
| [`docs/03_DATA_MODEL.md`](docs/03_DATA_MODEL.md) | Entidades principales: personajes, genealogía relacional, decisiones, patrimonio, NPC, crimen, información, transporte, misiones |
| [`docs/04_WORLD_ARCHITECTURE.md`](docs/04_WORLD_ARCHITECTURE.md) | Regiones/subregiones/locations, streaming, `TemporalWorldState` por capas |
| [`docs/05_TIME_SYSTEM.md`](docs/05_TIME_SYSTEM.md) | `GameCalendarSystem`, sincronía con la historia, solución al problema de tiempo en multijugador |
| [`docs/06_FAMILY_SYSTEM.md`](docs/06_FAMILY_SYSTEM.md) | `FamilySystem`, `CharacterDefinition` vs `CharacterInstance`, genealogía relacional entre familias |
| [`docs/07_CONSEQUENCE_SYSTEM.md`](docs/07_CONSEQUENCE_SYSTEM.md) | `DecisionSystem`, `ConsequenceEngine`, `QuestGraph`, mecanismos anti explosión combinatoria |
| [`docs/08_SAVE_SYSTEM.md`](docs/08_SAVE_SYSTEM.md) | Persistencia: snapshots + event log, versionado y migraciones |
| [`docs/09_MULTIPLAYER_ARCHITECTURE.md`](docs/09_MULTIPLAYER_ARCHITECTURE.md) | Modelo de autoridad de red, jugadores en zonas distintas, propiedad de campaña |
| [`docs/10_MOBILE_PERFORMANCE.md`](docs/10_MOBILE_PERFORMANCE.md) | Presupuestos de rendimiento por tier de dispositivo (LOW/MID/HIGH) |
| [`docs/11_HISTORICAL_DATA_PIPELINE.md`](docs/11_HISTORICAL_DATA_PIPELINE.md) | Pipeline de datos históricos, tipos de NPC, clasificación de contenido sensible |
| [`docs/12_DEVELOPMENT_ROADMAP.md`](docs/12_DEVELOPMENT_ROADMAP.md) | Roadmap de fases, herramientas internas, estrategia de testing, especificación del vertical slice Paracas 1820 |
| [`docs/13_TECHNICAL_RISKS.md`](docs/13_TECHNICAL_RISKS.md) | Riesgos técnicos con probabilidad, impacto y mitigación |

## Estructura del repositorio

```
docs/            Documentación de arquitectura y diseño
game-client/     Proyecto Unity (cliente) — código de Fase 1, ver game-client/PHASE_01_IMPLEMENTATION.md
backend/         Servicio de mundo persistente — se introduce en Fase 8-9
tools/           Herramientas de soporte (validación de datos, CI)
```

## Principios no negociables del proyecto

- Ninguna fase se da por completa sin cumplir su *Definition of Done*.
- No se avanza de fase sin instrucción explícita.
- Ningún hecho histórico se inventa: todo dato histórico se clasifica `VERIFIED`, `NEEDS_RESEARCH` o `FICTIONAL`.
- Código y datos históricos están estrictamente separados.
- El multijugador y la extensibilidad hacia Paradoja se consideran desde la arquitectura base, no se añaden al final.

Detalle completo de reglas en `docs/12_DEVELOPMENT_ROADMAP.md`.
