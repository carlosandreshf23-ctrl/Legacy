# 12_DEVELOPMENT_ROADMAP.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Roadmap de fases (resumen — protocolo y detalle completo por fase se formaliza al iniciar cada una)

Se mantiene íntegro el roadmap de vertical slices ya establecido para el proyecto: **Fase 0** (arquitectura, esta fase) → **Fase 1** (prototipo de movimiento) → **Fase 2** (vertical slice Paracas 1820, ver §4) → **Fase 3** (sistema familiar) → **Fase 4** (Consequence Engine) → **Fase 5** (patrimonio y economía) → **Fase 6** (mapa y exploración) → **Fase 7** (viajes) → **Fase 8** (mundo vivo + primer backend real) → **Fase 9** (multijugador 2→3→4) → **Fase 10** (cuatro familias) → **Fase 11** (combate) → **Fase 12** (Acto I completo 1820-1824, con QA de cierre) → **Fase 13** (salto generacional) → **Fase 14+** (Actos II-VII, uno a la vez, con QA entre cada uno) → **Fase Final** ("Tu Legado" / Historia Compartida) → **solo después, LEGADO: PARADOJA**.

No se avanza de fase sin instrucción explícita ("CONTINUAR FASE N"); protocolo de apertura/cierre de fase, reglas de progresión y regla ante errores: sin cambios respecto a lo ya fijado en la primera iteración de esta fase (documento previo `DevelopmentRoadmap.md`, ahora consolidado aquí).

## 2. Herramientas internas — priorización

No se construyen todas ahora; se priorizan por cuándo empiezan a ser indispensables:

| Herramienta | Prioridad | Necesaria a partir de |
|---|---|---|
| `DebugInspectorService` (consola de depuración) | P0 | Fase 1 (mínimo: fecha + estado de familia), crece por fase |
| Relationship Debugger | P1 | Fase 4 (Consequence Engine necesita poder inspeccionar `SimulationValue` real vs. lo que ve el jugador) |
| Economy Debugger | P1 | Fase 5 |
| Family Tree Editor / Genealogy Viewer | P1 | Fase 3 |
| Timeline Viewer | P1 | Antes de Fase 12 (necesaria para trabajar con contenido histórico real) |
| Historical Event Editor | P1 | Antes de Fase 12 |
| World State Editor | P2 | Fase 6/8 (cuando empieza el layering multi-época real) |
| Quest Editor | P2 | Cuando el volumen de `QuestGraph` lo justifique (aprox. Fase 10-12) |

No es obligatorio construir ninguna con UI dedicada antes de que la fase que la necesita comience; hasta entonces, `DebugInspectorService` cubre la necesidad mínima de inspección.

## 3. Estrategia de testing

| Tipo | Alcance | Cuándo corre |
|---|---|---|
| **Unit Tests** | Lógica C# pura sin dependencia del Editor: evaluador de `TriggerCondition`, aritmética de `GameCalendarSystem`, consultas de `GenealogyGraph` | Cada build, sin necesidad de Unity Editor |
| **Integration Tests** | Interacción entre sistemas (decisión → consecuencia → cambio de reputación) contra un repositorio en memoria (mock del backend) | CI, cada PR |
| **Gameplay Tests** | Unity Test Framework en modo Play, sobre el vertical slice activo | Antes de cerrar cada fase |
| **Save/Load Tests** | Round-trip de serialización + aplicación de cada migrador de `SchemaVersion` registrado | CI, cada PR que toque esquema |
| **Multiplayer Tests** | Arnés headless simulando 2-4 clientes contra el backend/mock | Desde Fase 9 |
| **Historical Validation Tests** | Validación automática de datos: todo evento `Modifiability = HARD` debe tener `SourceReferences` no vacío antes de permitir build; todo dato histórico debe tener `HistoricalStatus` asignado | CI, cada vez que se integra contenido de `Content/Historical` |
| **Simulación acelerada 1820→1920** | Arnés headless (sin render) que ejecuta el paso completo de calendario de 100 años de una campaña sintética, para detectar errores de genealogía (ciclos, huérfanos inválidos), crecimiento descontrolado del índice de consecuencias activas, divergencia económica o fallos de disparo de eventos históricos, antes de que ese contenido llegue a producción real | Antes de cerrar cada Acto (Fase 12, 14+) y cada vez que cambie el núcleo de `ConsequenceEngine`/`FamilySystem`/`GameCalendarSystem` |

## 4. Vertical Slice requerido: PARACAS/PISCO 1820 (Fase 2) — especificación de sistemas

Sin desarrollarlo todavía (eso es Fase 2), se fija ahora **qué sistemas estarán en qué estado** para ese slice, según lo pedido explícitamente en esta fase:

### Sistemas ACTIVOS (reales y funcionales, aunque a pequeña escala)
`Core`, `GameCalendarSystem`, `WorldState` limitado a la subregión Pisco-Paracas (un único `RegionEraSnapshot`), `CharacterDefinition/Instance` + controlador de personaje/cámara, `FamilySystem` (familia Salazar, una generación — el motor de genealogía existe y es real, aunque el árbol sea pequeño), `DecisionSystem` + `ConsequenceEngine` (funcional de verdad: es el sistema que Fase 2 debe demostrar que "vale la pena vivir en 1820"), `InformationSystem`/`DiscoverySystem` básico acotado a esa subregión, `TravelSystem` en modos `TravelComplete`/`TravelSummary` (sin `HistoricalFastTravel`: no hay infraestructura ferroviaria/portuaria de ese tipo todavía en 1820), `AuthoritySystem` básico (una única jurisdicción apropiada a 1820, sin instituciones republicanas posteriores), `PatrimonySystem`/`EconomySystem` a escala de un mercado local pequeño, `SaveSystem` local (snapshot+log aunque el backend real no exista aún — mismo contrato, implementación local-mock).

### Sistemas SIMULADOS (versión ligera/abstracta, no la implementación de producción completa)
`WorldSimulationSystem` reducido al alcance de la subregión (actividad de fondo simple, no la simulación regional completa de Fase 8), `CrimeAndWitnessSystem` mínimo (un único escenario de ruta con riesgo, no el sistema completo de propagación), `ReputationSystem` acotado a una sola región.

### Sistemas MOCKEADOS (interfaz real presente, implementación de producción ausente)
Backend de mundo persistente (`IWorldStateRepository` local-mock, tal como ya establece `08_SAVE_SYSTEM.md` §7), `MultiplayerSystem`/sesión en tiempo real (slice single-player; las interfaces existen pero no se ejercitan), `HistoricalTimelineSystem` (solo el dato puntual del rumor del desembarco de la expedición libertadora, no el pipeline histórico completo de Fase 12), `CombatSystem` (si la ruta de riesgo requiere resolución de un encuentro, se resuelve con una versión mínima/simplificada, no el sistema contextual completo de Fase 11), `QuestGraph` (1 misión principal + 3 secundarias usando la estructura general del sistema, sin explotar todavía su capacidad de resoluciones múltiples complejas).

Esto no es una lista de qué construir en Fase 2 — es la confirmación, desde la arquitectura, de que ningún sistema activo en el slice requiere que otro sistema todavía no construido exista realmente: todo lo "simulado"/"mockeado" tiene una interfaz ya definida en esta fase (`02_SYSTEMS_MAP.md`) que el slice puede satisfacer con una implementación mínima sin comprometer el diseño futuro.

## 5. Fuera de alcance de esta fase

No se construye ningún sistema todavía. Este documento es la referencia de secuenciación y de qué se espera exactamente en cada corte, no la implementación.
