# 01_ARCHITECTURE.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto
**Reemplaza/amplía**: la versión anterior de `Architecture.md` (consolidada aquí y en los documentos 02-13 de esta misma fase).

## 1. Análisis del entorno de partida

El repositorio no contenía ningún proyecto previo (sin commits, sin motor instalado, sin escenas, sin sistemas, sin deuda técnica heredada). Por tanto esta fase no es una migración ni una auditoría de código existente: es una propuesta de stack completa, tratada como decisión técnica argumentada, no como "depende".

## 2. Decisión de motor: Unity (recomendación única y definitiva para esta fase)

| Criterio | Unity | Unreal Engine | Otras opciones (Godot, motor propio) |
|---|---|---|---|
| Rendimiento móvil (batería, memoria, build size) | URP diseñado específicamente para este rango; herramientas de profiling móvil maduras (Profiler, Frame Debugger, Memory Profiler) | Requiere ajuste manual intenso para bajar de gama alta; builds base más pesados | Godot es ligero pero su toolchain móvil y de mundo abierto es mucho menos probado en producción a esta escala |
| Multiplayer | Netcode for GameObjects + Relay/Lobby cubren tanto sesión en tiempo real como los servicios de emparejamiento sin operar infraestructura propia desde el día uno | Requiere backend propio igualmente; su networking nativo está más orientado a shooters que a estado persistente | Motor propio: coste de desarrollo de networking inasumible para el alcance del proyecto |
| Tamaño de mundo / streaming | Addressables + escenas aditivas, suficiente para regiones del tamaño planeado (no es un mundo tipo AAA de streaming continuo sin cortes) | World Partition es más potente pero pensado para equipos con roles dedicados a nivel de mundo | Godot carece de un equivalente maduro a este nivel |
| Tooling / velocidad de desarrollo con equipo reducido | C# accesible, iteración rápida, Editor extensible con herramientas custom (necesario para 12 — Herramientas Internas) | Blueprints + C++ compilación más lenta, curva de entrada mayor | Godot itera rápido pero el ecosistema de plugins/paquetes de terceros es más limitado |
| Soporte de assets (Asset Store, terreno, vegetación, animación) | Ecosistema más grande para el tipo de contenido histórico/ambiental necesario | Marketplace fuerte en assets fotorrealistas, menos en "realismo estilizado" ligero | Limitado |
| Escalabilidad a PC | Export directo, mismo proyecto | Export directo, mejor techo gráfico | Godot viable pero con más trabajo de pulido |
| Facilidad de contratación futura | Base de desarrolladores C#/Unity muy amplia, especialmente para mobile | Comunidad fuerte pero más orientada a AAA/consola | Comunidad Godot mucho más pequeña para contratación a escala de producción |
| Mantenimiento a largo plazo (proyecto de años) | LTS con soporte extendido, migraciones documentadas | LTS también disponible, pero el coste de mantener C++ a largo plazo con equipo pequeño es mayor | Riesgo de motor propio: todo el mantenimiento recae en el equipo del juego, no en un proveedor de motor |

**Decisión final: Unity (LTS vigente al iniciar producción real — actualmente serie 2022 LTS/6 LTS), C#, Universal Render Pipeline (URP).**

Es la opción con menor fricción para el conjunto específico de restricciones de este proyecto: equipo reducido, prioridad móvil, mundo semiabierto de escala media (no mundo abierto masivo tipo AAA), multijugador asíncrono más que shooter competitivo, y una vida útil de desarrollo de varios años que exige velocidad de iteración sostenida sobre potencia gráfica máxima.

### Paquetes/servicios base
- **Unity LTS + URP**
- **Input System** — abstrae touch móvil y input de PC/gamepad sin duplicar lógica de control
- **Addressables** — streaming de contenido bajo demanda (ver `04_WORLD_ARCHITECTURE.md`)
- **Netcode for GameObjects + Unity Relay/Lobby** — capa de sesión en tiempo real (ver `09_MULTIPLAYER_ARCHITECTURE.md`)
- **Cinemachine** — cámara de exploración/combate
- **Unity Localization** — separación de texto y código (§13 de este documento)
- **Unity Test Framework** — testing en Editor y modo headless (ver `12_DEVELOPMENT_ROADMAP.md`)

No se genera un proyecto Unity binario real en esta fase (no hay Editor disponible en este entorno de ejecución); esta decisión y su justificación son el entregable de Fase 0, la creación del proyecto ocurre al iniciar Fase 1.

## 3. Arquitectura en capas

Se adopta la división conceptual propuesta, con una precisión importante: son **capas de responsabilidad**, no necesariamente carpetas 1:1 ni namespaces aislados sin comunicación — algunas (Persistence, Multiplayer, UI, Audio, Debug) son transversales y no forman parte de la cadena de dependencia principal (ver `02_SYSTEMS_MAP.md` §2 sobre por qué esto evita ciclos).

| Capa | Responsabilidad | Tipo |
|---|---|---|
| **Core** | Bootstrap, ServiceLocator/inyección de dependencias ligera, EventBus, contratos base (`IPersistable`, `ITickable`), `GameCalendarSystem` | Base, sin dependencias |
| **World** | Definición y estado de regiones/subregiones/locations, `TemporalWorldState` (capas históricas + cambios dinámicos) | Depende de Core |
| **Characters** | `CharacterDefinition`/`CharacterInstance`, controlador de personaje/cámara/input | Depende de Core, World |
| **Families** | Genealogía relacional, `FamilySystem`, patrimonio familiar (enlace a Economy) | Depende de Core, Characters |
| **Narrative** | `DecisionSystem`, `ConsequenceEngine`, `QuestGraph` | Depende de Core, Families, World |
| **History** | `HistoricalTimelineSystem`, clasificación HARD/SOFT/FICTIONAL, sincronía con calendario | Depende de Core (calendario); es consumida por World/Narrative, no las consume |
| **Economy** | Precios regionales, oferta/demanda, `PatrimonySystem` | Depende de Core, World, WorldSimulation (seguridad de ruta) |
| **Travel** | `TravelSystem`, `TransportDefinition`, rutas | Depende de Core, World, Economy, WorldSimulation |
| **Simulation** | `WorldSimulationSystem` (mundo vivo), `CrimeAndWitnessSystem`, `InformationSystem` | Depende de Core, World, History |
| **Reputation** | `ReputationSystem`, `RelationshipSystem` | Depende de Narrative (consecuencias como origen), Families |
| **Authority** | `AuthoritySystem`, jurisdicciones, alcance territorial de delitos | Depende de Core, World, History (qué autoridad existe en qué época) |
| **Multiplayer** | Sesión en tiempo real + campaña compartida + autoridad de red | Transversal — envuelve Families/Narrative/Economy/Travel, no es una capa "debajo" de ellas |
| **Persistence** | `SaveSystem` (snapshots + event log) | Transversal — implementa `IPersistable` sobre todas las capas con estado |
| **UI** | Presentación, HUD, menús | Consumidor final, no tiene dependientes |
| **Audio** | Sonido ambiental, música, efectos | Consumidor final |
| **Content** | Datos configurables (históricos y de diseño) que alimentan World/History/Narrative/Economy/Travel | No es código: son assets que las capas anteriores leen (ver `03_DATA_MODEL.md`, `11_HISTORICAL_DATA_PIPELINE.md`) |

Esta reestructuración respecto a la propuesta original del prompt es mínima: se conservan los 16 nombres, y se explicita cuáles son "capas de dependencia" (Core→World→Characters→Families→Narrative, con History/Economy/Travel/Simulation/Reputation/Authority como ramas paralelas que cuelgan de Core+World) frente a cuáles son "capas transversales" (Multiplayer, Persistence, UI, Audio, Debug). Sin esta distinción, tratar las 16 como una pila estricta produciría dependencias circulares artificiales (ej. Persistence "debajo" de Families pero Families necesita guardarse a través de Persistence). El diagrama completo está en `02_SYSTEMS_MAP.md`.

## 4. Principios de arquitectura data-driven

1. **Separación estricta código/contenido histórico** (Regla 4 del proyecto). Personajes, lugares, años, acontecimientos, transportes, autoridades, regiones, precios, profesiones, misiones y relaciones se definen mediante datos (`Content`), nunca como literales en C#. El código en `Systems/` implementa mecanismos generales que interpretan esos datos.
2. **Sistemas generales, nunca sistemas de contenido** (Regla 5): nunca `SistemaBatallaAyacucho`, siempre un `CombatEncounterSystem`/`HistoricalTimelineSystem` parametrizado por datos.
3. **Identificadores persistentes globales para todo objeto narrativamente relevante** (ver §5).
4. **Todo objeto persistente lleva `TimelineID`** (default `"prime"`) y `SchemaVersion`, preparando el terreno para Paradoja sin construirlo (Regla 8).
5. **Autoridad de datos en backend para todo lo compartido**; el cliente es presentación + cola de intenciones (detalle en `09_MULTIPLAYER_ARCHITECTURE.md`).
6. **Ningún sistema conoce los detalles internos de otro**: comunicación vía interfaces públicas + `EventBus` de `Core`.

## 5. Identificadores persistentes

Todo objeto narrativamente importante recibe un GUID estable en el momento de su creación, que nunca se reutiliza ni se recicla, y que es independiente de cualquier referencia de escena/instancia de Unity:

`FamilyID`, `CharacterID`, `LocationID`, `DecisionID`, `HistoricalEventID`, `PropertyID`, `RelationshipID`, `QuestID`, `RouteID`, `ItemID`, `NPCID`, `WorldEventID`, `RegionID`, `TransportID`, `InformationID`, `CrimeEventID`, `CampaignID`, `SessionID`.

Regla dura: **ningún sistema puede usar una referencia de escena Unity (`GameObject`/`Transform`/índice de escena) como identidad narrativa**. Un `GameObject` en escena es, como máximo, una vista temporal de una entidad cuya identidad real es su GUID; si el `GameObject` se destruye (streaming descarga la región), el GUID y su estado persisten en los sistemas de datos. Esto es lo que permite que una decisión de 1824 sea consultable en 1890: la consulta se hace por `DecisionID`/`CharacterID` contra `ConsequenceEngine`/`SaveSystem`, nunca contra un objeto de escena que dejó de existir hace décadas de tiempo de juego.

## 6. Debug y observabilidad (transversal)

Se define un único `DebugInspectorService` (capa transversal, no una capa nueva) que expone en builds no-shipping, consultando las APIs públicas ya existentes de cada sistema (sin backdoors especiales a estado interno):

`CurrentGameDate`, `ActiveHistoricalEvents`, `FamilyState`, `RelationshipState`, `RegionalEconomy`, `WorldSecurity`, `PendingConsequences`.

Se construye incrementalmente desde Fase 1 (mínimo: fecha actual + estado de familia) y se amplía por fase a medida que cada sistema nuevo entra en producción. Es una herramienta de desarrollo, no una feature de jugador.

## 7. Testing (transversal)

Estrategia completa en `12_DEVELOPMENT_ROADMAP.md` §"Testing". Principio de diseño relevante aquí: toda la lógica de gameplay (`Systems/`) se escribe como C# puro sin dependencias del Editor de Unity siempre que sea posible, específicamente para poder correr **Unit Tests** e **Integration Tests** fuera del Editor, y para poder ejecutar una simulación acelerada 1820→1920 en segundos como prueba de regresión (requisito explícito de validación histórica/genealógica/económica antes de integrar contenido de cada Acto).

## 8. Localización (transversal)

Todo texto visible al jugador se referencia por `TextKey`, nunca como string literal embebido en código ni en datos narrativos/históricos directamente. `Content` (datos históricos y de diseño) almacena claves, no texto; una `LocalizationTable` (Unity Localization) resuelve la clave al idioma activo. Español es el locale fuente; inglés y otros se añaden como tablas adicionales sin tocar lógica de juego ni esquema de datos históricos. Esto también facilita que el mismo dato histórico (`HistoricalEventDefinition`) tenga descripciones distintas por idioma sin duplicar el registro completo.

## 9. Qué NO se hace en esta fase

No se construye Perú completo, ni Junín, ni Ayacucho, ni la Guerra del Pacífico, ni el árbol familiar completo, ni todas las ciudades, ni sistemas de conflicto armado moderno, ni contenido hasta 2026, ni John Titor, ni el Cronoscopio, ni Paradoja, ni cientos de NPC. Esta fase es exclusivamente arquitectura: decisiones que hacen posible construir todo lo anterior después sin reescritura estructural.
