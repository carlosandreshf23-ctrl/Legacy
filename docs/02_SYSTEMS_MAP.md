# 02_SYSTEMS_MAP.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Catálogo de sistemas

| Sistema | Capa (`01_ARCHITECTURE.md`) | Prioridad | Depende de | Expone a |
|---|---|---|---|---|
| `Core` (EventBus, ServiceLocator, `IPersistable`) | Core | P0 | — | todos |
| `GameCalendarSystem` | Core | P0 | Core | todos los que necesitan fecha/tiempo |
| `WorldState` (Region/Location, `TemporalWorldState`) | World | P0 | Core | Characters, Travel, Simulation, Authority |
| `CharacterDefinition`/`CharacterInstance` + controlador | Characters | P0 | Core, World | Families, UI |
| `FamilySystem` (genealogía relacional) | Families | P0 | Core, Characters | Narrative, Reputation, Persistence |
| `DecisionSystem` | Narrative | P1 | Core, Families, World | ConsequenceEngine |
| `ConsequenceEngine` | Narrative | P1 | DecisionSystem, GameCalendarSystem | Reputation, Relationship, Patrimony, Quest |
| `QuestGraph` | Narrative | P1 | ConsequenceEngine, Families | UI |
| `HistoricalTimelineSystem` | History | P1 (base P0 mínima) | GameCalendarSystem, Content | WorldState, Authority, NPC (HistoricalNPC) |
| `EconomySystem` | Economy | P1 | WorldState, Simulation (seguridad) | PatrimonySystem, Travel |
| `PatrimonySystem` | Economy/Families | P1 | EconomySystem, FamilySystem, ConsequenceEngine | Persistence, Multiplayer (transferencias) |
| `TravelSystem` | Travel | P1 | GameCalendarSystem, WorldState, EconomySystem, Simulation | Characters, Multiplayer (time sync) |
| `TransportDefinition` (datos) | Travel/Content | P1 | Content | TravelSystem |
| `WorldSimulationSystem` | Simulation | P1 | GameCalendarSystem, WorldState, History | Economy, Authority, Crime, NPC (PopulationNPC) |
| `InformationSystem` | Simulation | P1 | WorldSimulationSystem | DiscoverySystem (especialización geográfica), Economy, Crime |
| `CrimeAndWitnessSystem` | Simulation | P2 (mínimo P1 para vertical slice) | WorldSimulationSystem, InformationSystem, AuthoritySystem | Reputation, Authority |
| `ReputationSystem` | Reputation | P1 | ConsequenceEngine, FamilySystem | UI, Multiplayer |
| `RelationshipSystem` | Reputation | P1 | ConsequenceEngine, FamilySystem | UI (vista filtrada), Genealogy (parejas) |
| `AuthoritySystem` | Authority | P1 (mínimo), P2 (institucional completo) | GameCalendarSystem, WorldState, History | Crime, Travel (riesgo), Combat |
| `CombatSystem` | (consumidor de Simulation/Travel) | P2 | AuthoritySystem, Reputation, WorldSimulationSystem | ConsequenceEngine |
| `MultiplayerSystem` (Session + Campaign) | Multiplayer | P1 | Families, Narrative, Economy, Travel, Persistence | — (transversal, no expone hacia abajo) |
| `SaveSystem` | Persistence | P0 | `IPersistable` de todos los sistemas con estado | — (transversal) |
| `DebugInspectorService` | Debug (transversal) | P0/P1 incremental | APIs públicas de todos | herramientas de desarrollo |
| UI/Audio | UI/Audio | P0 mínimo, resto P1-P2 | todo lo anterior | jugador |
| `Content` (datos) | Content | P0 (esquema), contenido real P1-P3 según fase | — | World, History, Narrative, Economy, Travel |

## 2. Diagrama de dependencias

```
                              ┌─────────────────────────┐
                              │           Core           │
                              │ EventBus · ServiceLocator │
                              │ IPersistable · ITickable  │
                              └────────────┬─────────────┘
                                           │
                              ┌────────────▼─────────────┐
                              │    GameCalendarSystem     │
                              └───┬─────────────────┬─────┘
                                  │                 │
                  ┌───────────────▼───┐   ┌─────────▼─────────────┐
                  │ HistoricalTimeline │   │       WorldState        │
                  │ (HARD/SOFT/FICT.)  │──▶│ Region/Location/Temporal │
                  └───────────────────┘   └───┬──────────┬─────────┘
                                              │          │
                        ┌─────────────────────▼──┐   ┌───▼──────────────┐
                        │   CharacterDefinition/   │   │ WorldSimulation  │
                        │      CharacterInstance    │   │ (mundo vivo)      │
                        └───────────┬──────────────┘   └───┬───┬───┬─────┘
                                    │                        │   │   │
                        ┌───────────▼──────────┐   ┌─────────▼┐ │ ┌─▼──────────┐
                        │     FamilySystem       │   │ Economy  │ │ │ Authority  │
                        │ (genealogía relacional) │   │ System   │ │ │ System     │
                        └───┬──────────┬─────────┘   └───┬──────┘ │ └─┬──────────┘
                            │          │                  │        │   │
              ┌─────────────▼─┐   ┌────▼────────┐   ┌─────▼────┐ ┌▼───▼────────┐
              │ DecisionSystem │   │ Patrimony   │   │  Travel   │ │ Information │
              │       ↓         │   │ System      │   │  System   │ │  System /   │
              │ ConsequenceEngine│  └─────────────┘   │ Transport │ │ CrimeAndWit-│
              └───┬─────┬───────┘                      └───────────┘ │ nessSystem  │
                  │     │                                            └─────────────┘
        ┌─────────▼─┐ ┌─▼────────────┐
        │ Reputation │ │ QuestGraph   │
        │ Relationship│ └──────────────┘
        └────────────┘

  Transversales (no forman parte de la cadena anterior, la envuelven):
  ── Persistence (SaveSystem): implementa IPersistable sobre todo lo de arriba.
  ── Multiplayer (Session + Campaign): coordina autoridad de red sobre Families/Narrative/Economy/Travel.
  ── Debug/UI/Audio: consumidores finales, nada depende de ellos.
```

## 3. Verificación de ausencia de ciclos

- `HistoricalTimelineSystem` depende de `GameCalendarSystem` y de `Content`, nunca de `WorldState`; es `WorldState`/`Authority` quienes leen `HistoricalTimelineSystem`, no al revés. Esto evita el ciclo obvio "el mundo cambia por historia" ↔ "la historia depende del mundo".
- `ConsequenceEngine` depende de `DecisionSystem` y `GameCalendarSystem`, pero los sistemas que *reaccionan* a consecuencias (`Reputation`, `Patrimony`, `Quest`) no son dependencias de `ConsequenceEngine`: se suscriben a sus eventos vía `Core.EventBus`. Un sistema que reacciona a un evento nunca es una dependencia de compilación de quien lo emite.
- `Persistence` y `Multiplayer` se modelan como transversales precisamente para no forzar una relación "¿Persistence está antes o después de Families en la pila?" que no tiene una respuesta correcta — ambas preguntas son válidas (Families necesita guardarse; Persistence necesita saber qué es un Family) y solo son resolubles sin ciclo si `Persistence` no es una capa en la cadena principal, sino una implementación de una interfaz (`IPersistable`) que las capas de arriba exponen hacia abajo.
- `InformationSystem` es la base y `DiscoverySystem` (conocimiento geográfico) es una especialización de datos de `InformationSystem` (topic = ubicación), no un sistema paralelo con su propia dependencia — evita un segundo camino redundante hacia `WorldState`.

## 4. Clasificación de prioridades (P0-P3)

**P0 — Fundamental (sin esto no existe el juego)**
`Core`, `GameCalendarSystem`, `WorldState` (esquema Region/Location), `CharacterDefinition/Instance` + controlador, `FamilySystem` (básico), `SaveSystem` (local, snapshot+log mínimo), esquema de `Content`.

**P1 — Importante (necesario para vertical slice avanzado, Fases 2-9)**
`DecisionSystem`, `ConsequenceEngine`, `QuestGraph` (básico), `HistoricalTimelineSystem` (base), `EconomySystem`, `PatrimonySystem`, `TravelSystem`/`TransportDefinition`, `WorldSimulationSystem` (versión ligera), `InformationSystem`, `ReputationSystem`/`RelationshipSystem`, `AuthoritySystem` (básico), `MultiplayerSystem`.

**P2 — Expansión (puede agregarse después sin bloquear el slice)**
`CrimeAndWitnessSystem` completo, `AuthoritySystem` institucional detallado por época, `CombatSystem`, reputación por grupo (comerciantes/militares/autoridades/comunidades), herramientas internas avanzadas (World State Editor, Quest Editor), localización más allá de español.

**P3 — DLC/Futuro**
Todo lo relativo a LEGADO: PARADOJA (ramas de timeline navegables, John Titor, Cronoscopio, contenido hasta 2026).
