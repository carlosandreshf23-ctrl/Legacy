# LEGADO: PERÚ

Videojuego histórico narrativo, multijugador (1-4) y generacional ambientado en Perú entre 1820-1920. Plataforma principal: móvil, con expansión posterior a PC. DLC futuro planeado: **LEGADO: PARADOJA** (no en desarrollo todavía).

Este repositorio se desarrolla por **fases funcionales** (vertical slices): cada fase produce una versión jugable, verificable y ampliable antes de avanzar a la siguiente. Ver `docs/DevelopmentRoadmap.md` para el detalle completo y el estado actual.

## Estado del proyecto

**Fase 0 — Arquitectura del Proyecto** (en curso). Aún no existe cliente jugable; esta fase define la base técnica.

## Documentación

| Documento | Contenido |
|---|---|
| [`docs/Architecture.md`](docs/Architecture.md) | Motor/stack elegido, principios de arquitectura, estructura de carpetas, capas del sistema |
| [`docs/GameSystems.md`](docs/GameSystems.md) | Catálogo de sistemas de juego, responsabilidades y dependencias |
| [`docs/DataModel.md`](docs/DataModel.md) | Modelo de datos de las entidades centrales del juego |
| [`docs/SaveSystem.md`](docs/SaveSystem.md) | Diseño del sistema de guardado (local + mundo persistente compartido) |
| [`docs/MultiplayerArchitecture.md`](docs/MultiplayerArchitecture.md) | Modelo multijugador asíncrono + sesión en tiempo real |
| [`docs/DevelopmentRoadmap.md`](docs/DevelopmentRoadmap.md) | Roadmap completo de fases, protocolo de trabajo y reglas de progresión |

## Estructura del repositorio

```
docs/            Documentación de arquitectura y diseño
game-client/     Proyecto Unity (cliente) — se inicializa en Fase 1
backend/         Servicio de mundo persistente — se introduce en Fase 8-9
tools/           Herramientas de soporte (validación de datos, CI)
```

## Principios no negociables del proyecto

- Ninguna fase se da por completa sin cumplir su *Definition of Done*.
- No se avanza de fase sin instrucción explícita.
- Ningún hecho histórico se inventa: todo dato histórico se clasifica `VERIFIED`, `NEEDS_RESEARCH` o `FICTIONAL`.
- Código y datos históricos están estrictamente separados.
- El multijugador y la extensibilidad hacia Paradoja se consideran desde la arquitectura base, no se añaden al final.

Detalle completo de reglas en `docs/DevelopmentRoadmap.md`.
