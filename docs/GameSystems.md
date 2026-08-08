# GameSystems.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

Catálogo de sistemas de juego identificados a partir de la visión del proyecto. Cada sistema se implementa como un **servicio C# desacoplado del motor** (lógica pura, testable) más una fachada delgada que lo integra a Unity. Ningún sistema conoce los detalles internos de otro: se comunican mediante interfaces públicas y un **EventBus** central (`Core`), nunca mediante referencias directas cruzadas.

Convención de esta tabla por sistema:
- **Responsabilidad**: qué decide y qué NO decide.
- **Depende de**: otros sistemas que consume (vía interfaz).
- **Expone**: qué otros sistemas pueden consumir de él.
- **Autoridad de datos**: dónde vive el estado real (cliente / backend — ver `MultiplayerArchitecture.md`).
- **Fase de introducción**: cuándo se construye por primera vez (no significa que se complete ahí).
- **Nota Paradoja**: cómo queda preparado para timelines múltiples sin implementarlo ahora (Regla 8).

---

## 1. `TimeSystem`
- **Responsabilidad**: es la única fuente de verdad sobre "qué fecha es" en el mundo de juego (año/mes/día), avance del tiempo por viaje/espera/acciones, ciclo día-noche. Ningún otro sistema mantiene su propio reloj.
- **Depende de**: nada (sistema base).
- **Expone**: `GetCurrentWorldDate()`, `AdvanceTime(duration)`, eventos `OnDateChanged`, `OnTimeOfDayChanged`.
- **Autoridad de datos**: backend (el tiempo de mundo es compartido entre jugadores); el cliente predice localmente y corrige por servidor.
- **Fase de introducción**: Fase 1 (versión mínima local), autoridad de backend en Fase 8-9.
- **Nota Paradoja**: toda consulta de fecha ya requiere `TimelineID` implícito (por defecto `prime`) para que timelines paralelos con fechas divergentes sean solo un parámetro adicional.

## 2. `FamilySystem`
- **Responsabilidad**: parentesco, personaje activo por familia, nacimiento, envejecimiento, muerte, cambio de protagonista, herencia de rasgos/relaciones (no de patrimonio, eso es `PatrimonySystem`).
- **Depende de**: `TimeSystem` (envejecimiento), `ConsequenceEngine` (eventos de vida generan/consumen memoria).
- **Expone**: árbol genealógico, personaje activo, eventos `OnCharacterBorn`, `OnCharacterDied`, `OnProtagonistChanged`.
- **Autoridad de datos**: backend (compartido: otras familias pueden observar/recordar miembros de esta familia).
- **Fase de introducción**: Fase 3.
- **Nota Paradoja**: el árbol genealógico es un grafo con nodos fechados; una rama alterada en Paradoja es una vista alternativa del mismo grafo filtrada por `TimelineID`, no una estructura nueva.

## 3. `ConsequenceEngine`
- **Responsabilidad**: registrar decisiones como eventos (`DecisionID`, `CharacterID`, `FamilyID`, `Date`, `Region`, `Target`, `Consequences`, `Visibility`, `TriggerConditions` — ver `DataModel.md`), evaluar condiciones de disparo (inmediatas, diferidas, generacionales, regionales, familiares) y notificar a los sistemas interesados. No decide *qué* hacer con una consecuencia (eso lo hacen los sistemas que la consumen: `ReputationSystem`, `WorldSimulationSystem`, diálogo, etc.), solo que *debe* ocurrir y cuándo.
- **Depende de**: `TimeSystem` (evaluar condiciones diferidas), `FamilySystem` (alcance generacional).
- **Expone**: `RecordDecision(...)`, `QueryConsequencesFor(characterId/familyId/region)`, evento `OnConsequenceTriggered`.
- **Autoridad de datos**: backend (es el "libro de memoria" del mundo, debe sobrevivir a cualquier sesión de cliente).
- **Fase de introducción**: Fase 4.
- **Nota Paradoja**: es, junto con `TimelineID`, el sistema más directamente reutilizado por Paradoja (alterar historia = insertar una decisión en un punto pasado y dejar que el motor de consecuencias, ya existente, la propague hacia adelante en una rama de timeline nueva).

## 4. `PatrimonySystem` (incluye economía familiar)
- **Responsabilidad**: dinero, propiedades, negocios, animales, embarcaciones, inversiones, deuda; herencia de patrimonio al cambiar protagonista; pérdida de patrimonio por guerra/decisión/evento histórico.
- **Depende de**: `EconomySystem` (precios regionales), `FamilySystem` (herencia), `ConsequenceEngine` (una ruina económica puede ser consecuencia diferida).
- **Expone**: balance familiar, listado de propiedades, eventos `OnAssetGained/Lost`, `OnDebtChanged`.
- **Autoridad de datos**: backend.
- **Fase de introducción**: Fase 5.
- **Nota Paradoja**: sin cambios estructurales necesarios; el patrimonio de una rama alterada es simplemente otro `TimelineID`.

## 5. `EconomySystem`
- **Responsabilidad**: precios regionales, oferta/demanda simulada, efecto de inseguridad/rutas sobre precios. Es un motor de simulación numérica, no dueño de "quién tiene qué" (eso es `PatrimonySystem`).
- **Depende de**: `WorldSimulationSystem` (inseguridad de rutas, actividad comercial), `DiscoverySystem`/`TravelSystem` indirectamente vía rutas activas.
- **Expone**: `GetPrice(good, region, date)`, eventos `OnPriceShift`.
- **Autoridad de datos**: backend.
- **Fase de introducción**: Fase 5.
- **Nota Paradoja**: N/A directa; podría divergir por timeline igual que el resto de estado regional.

## 6. `DiscoverySystem` (conocimiento geográfico)
- **Responsabilidad**: estado de conocimiento por jugador/familia y por locación (`UNKNOWN → RUMOR → MAPPED → VISITED → MASTERED`), transmisión de conocimiento entre generaciones y entre familias (comercio de mapas/información).
- **Depende de**: `FamilySystem` (herencia de conocimiento), `TravelSystem` (visitar actualiza estado).
- **Expone**: `GetKnowledgeState(familyId, locationId)`, `ShareKnowledge(fromFamily, toFamily, locationId)`.
- **Autoridad de datos**: backend, con caché local por jugador (es información privada por familia, a diferencia del mapa físico que es público).
- **Fase de introducción**: Fase 6.
- **Nota Paradoja**: sin cambios; el conocimiento geográfico de una línea alternativa es independiente por `TimelineID`.

## 7. `TravelSystem`
- **Responsabilidad**: calcular viajes (distancia, terreno, transporte disponible según año/región/economía/infraestructura, clima, seguridad, tiempo, carga, costo); las tres modalidades de viaje (Completo, Resumido, Rápido históricamente justificado); consumir `TimeSystem` de forma real (el mundo avanza mientras se viaja).
- **Depende de**: `TimeSystem`, `DiscoverySystem` (rutas conocidas habilitan viaje resumido/rápido), `WorldSimulationSystem` (seguridad de ruta), `EconomySystem` (costo).
- **Expone**: `PlanTravel(...)`, `TravelComplete`, `TravelSummary`, `HistoricalFastTravel`.
- **Autoridad de datos**: backend valida y confirma (evita que el cliente "salte" tiempo/espacio de forma inválida); cliente ejecuta la presentación del viaje.
- **Fase de introducción**: Fase 7.
- **Nota Paradoja**: los medios de transporte son datos (`TravelMethodDefinition`), extensibles a tecnología futura del DLC sin tocar el sistema.

## 8. `WorldSimulationSystem`
- **Responsabilidad**: simular actividad de NPC, comercio, criminalidad, autoridad, rumores, seguridad de rutas a nivel regional, con o sin jugadores presentes ("mundo vivo"). Simulación abstracta/estadística lejos de jugadores, más detallada cerca de ellos.
- **Depende de**: `TimeSystem` (tick de simulación), `AuthoritySystem`, `EconomySystem`, `DiscoverySystem` (rumores como forma de conocimiento).
- **Expone**: estado de seguridad/actividad por región, eventos `OnRegionEventTriggered` (ej. aparición de bandolerismo).
- **Autoridad de datos**: backend (corre server-side de forma continua, es el corazón de "el mundo sigue existiendo sin mí").
- **Fase de introducción**: Fase 8.
- **Nota Paradoja**: parametrizable por `TimelineID` para correr simulaciones divergentes en paralelo.

## 9. `AuthoritySystem`
- **Responsabilidad**: modelar qué tipo de autoridad existe según año/región/jurisdicción/situación política (Regla: nunca policía moderna en 1820), y el alcance territorial de delitos/búsqueda (nunca "buscado en todo Perú" automático).
- **Depende de**: `TimeSystem` (qué autoridad existe en qué año), datos de `HistoricalEventSystem` para contexto político.
- **Expone**: `GetAuthorityFor(region, date)`, `GetJurisdictionalStatus(characterId, jurisdiction)`.
- **Autoridad de datos**: backend.
- **Fase de introducción**: Fase 6-8 (soporte básico), profundizado en Fase 11-12.
- **Nota Paradoja**: el DLC introduce nuevos tipos de autoridad/riesgo; `AuthoritySystem` ya está diseñado para resolver "qué autoridad aplica" por datos, no por código específico de época.

## 10. `ReputationSystem`
- **Responsabilidad**: confianza, respeto, deuda, rivalría, temor, reputación — por relación (familia-familia, familia-NPC, familia-región/facción), con alcance territorial, no global.
- **Depende de**: `ConsequenceEngine` (la reputación es en gran parte una vista agregada de consecuencias registradas), `AuthoritySystem` (reputación ante autoridades).
- **Expone**: `GetRelationship(familyA, familyB)`, `GetReputation(familyId, region)`.
- **Autoridad de datos**: backend.
- **Fase de introducción**: Fase 2 (versión mínima), formalizado en Fase 4 junto a `ConsequenceEngine`.
- **Nota Paradoja**: relaciones también quedan indexadas por `TimelineID`.

## 11. `CombatSystem`
- **Responsabilidad**: combate contextual (no shooter arcade): melee, armas de época, resistencia, heridas, retirada, combate montado cuando corresponde. El jugador no es superhumano.
- **Depende de**: `AuthoritySystem`/`ReputationSystem` (el combate es una entre varias formas de resolver un encuentro, no la default), `WorldSimulationSystem` (encuentros surgen de la simulación regional).
- **Expone**: resolución de encuentro, eventos `OnCombatResolved` (consumidos por `ConsequenceEngine`).
- **Autoridad de datos**: sesión en tiempo real (cliente(s) co-presentes) valida servidor de sesión; resultado final se persiste en backend.
- **Fase de introducción**: Fase 11.
- **Nota Paradoja**: N/A directa.

## 12. `DialogueSystem` / `NPCSystem`
- **Responsabilidad**: interacción con NPC, resolución no-combate de encuentros (conversación, documentos, contactos, negociación), comportamiento básico de NPC cercanos al jugador.
- **Depende de**: `ReputationSystem`, `AuthoritySystem`, `ConsequenceEngine` (diálogo puede disparar o consultar consecuencias).
- **Expone**: árbol/resolución de diálogo, eventos `OnDialogueOutcome`.
- **Autoridad de datos**: cliente ejecuta, backend valida outcomes que afectan estado compartido.
- **Fase de introducción**: Fase 1-2 (básico), ampliado progresivamente.
- **Nota Paradoja**: N/A directa.

## 13. `HistoricalEventSystem`
- **Responsabilidad**: define qué acontecimientos históricos son fijos (no alterables) dentro de LEGADO: PERÚ, y expone los puntos de enganche donde personajes ficticios participan sin alterar el resultado macro. Aplica la clasificación `VERIFIED / NEEDS_RESEARCH / FICTIONAL` (Regla 3).
- **Depende de**: `TimeSystem`.
- **Expone**: `GetHistoricalEventsFor(date, region)`, banderas de "resultado fijo" para que otros sistemas (misiones, `ConsequenceEngine`) no permitan alterarlo.
- **Autoridad de datos**: backend (dato compartido y fijo por diseño para todos los jugadores en LEGADO: PERÚ).
- **Fase de introducción**: Fase 12 en profundidad (Acto I), estructura base antes.
- **Nota Paradoja**: el campo de clasificación se extiende con un futuro estado `ALTERABLE_IN_PARADOX` sin romper compatibilidad — es la puerta de entrada al DLC.

## 14. `SaveSystem`
- **Responsabilidad**: persistencia local (perfil, caché, configuración) y coordinación con el backend para el estado compartido. Ver `SaveSystem.md` para el detalle completo.
- **Depende de**: todos los sistemas con estado persistente (los consume vía una interfaz `ISaveable`/`IPersistable` común, no acopla contenido).
- **Expone**: `SaveLocal()`, `SyncWithBackend()`, `LoadGame()`.
- **Autoridad de datos**: mixto (ver `SaveSystem.md`).
- **Fase de introducción**: Fase 1 (mínimo local), ampliado en Fase 9 (multiplayer).

## 15. `MultiplayerSystem` / `SessionSystem`
- **Responsabilidad**: gestión de sesión en tiempo real (2-4 jugadores co-presentes), matchmaking/encuentro, transferencia de objetos, préstamos, comercio entre jugadores, guardado multiplayer. Ver `MultiplayerArchitecture.md`.
- **Depende de**: `PatrimonySystem`, `ReputationSystem`, `FamilySystem`, `SaveSystem`.
- **Expone**: eventos de sesión, API de transacciones entre jugadores.
- **Autoridad de datos**: backend + capa de sesión en tiempo real.
- **Fase de introducción**: Fase 9.

## 16. `Core` (Bootstrap / ServiceLocator / EventBus)
- **Responsabilidad**: arranque de la aplicación, resolución de dependencias entre servicios (sin acoplar sistemas entre sí directamente), bus de eventos central, gestión de ciclo de vida de escenas/regiones (streaming).
- **Depende de**: nada (es la base).
- **Expone**: registro/resolución de servicios, publicación/suscripción de eventos.
- **Fase de introducción**: Fase 1.

## 17. `Character & Camera Controller` (capa de presentación)
- **Responsabilidad**: control de personaje, cámara, input móvil (touch), animación, interacción con el mundo. Es la única capa que "sabe" que corre en Unity/móvil; todo lo demás es lógica pura.
- **Depende de**: `Core` (input), `WorldSimulationSystem`/`DiscoverySystem` para contexto de interacción.
- **Fase de introducción**: Fase 1.

---

## Mapa de dependencias (resumen)

```
Core (EventBus/ServiceLocator)
 └── TimeSystem
      ├── FamilySystem ── ConsequenceEngine ── ReputationSystem
      │                                     └── HistoricalEventSystem
      ├── PatrimonySystem ── EconomySystem
      ├── DiscoverySystem ── TravelSystem
      ├── WorldSimulationSystem ── AuthoritySystem
      ├── CombatSystem / DialogueSystem (consumidores de encuentro)
      └── SaveSystem / MultiplayerSystem (transversales, tocan a todos los anteriores)
```

Ningún sistema de la mitad inferior del árbol depende de `Character & Camera Controller`: la presentación consume sistemas, nunca al revés. Esto es lo que permite que la lógica de juego sea testable sin el Editor y, a largo plazo, portable.
