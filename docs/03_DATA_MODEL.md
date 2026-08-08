# 03_DATA_MODEL.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

Reglas transversales (ver `01_ARCHITECTURE.md` §4-5): todo identificador es GUID estable y global; toda entidad persistente lleva `TimelineID` (default `"prime"`) y `SchemaVersion`; las fechas usan `WorldDate` (año/mes/día del calendario de juego, ver `05_TIME_SYSTEM.md`), nunca `DateTime` real; todo dato con contenido histórico lleva `HistoricalStatus` (`VERIFIED / NEEDS_RESEARCH / FICTIONAL`).

## 1. Personajes: `CharacterDefinition` vs `CharacterInstance`

Se separan explícitamente **datos de diseño** (plantilla) de **datos de partida** (estado real de esa partida), igual que el resto del modelo distingue Contenido de Guardado.

### `CharacterDefinition` (dato de diseño — usado para figuras históricas, vive en `Content`)

| Campo | Tipo | Notas |
|---|---|---|
| `DefinitionId` | GUID | |
| `FullName` | string | |
| `BirthDate` | WorldDate | puede ser exacta o rango, según investigación |
| `DeathConstraints` | dato | rango válido de fecha/causa de muerte si es un `HistoricalNPC` (ver §7) |
| `Origin` | RegionId | |
| `BaseTraits` | List\<TraitId\> | |
| `Profession` | string (dato) | |
| `KnownRelationships` | List\<RelationshipTemplate\> | relaciones históricamente documentadas, punto de partida |
| `HistoricalStatus` | enum | obligatorio si se usa para instanciar un `HistoricalNPC` |

No todo `CharacterInstance` tiene un `CharacterDefinition`: los personajes ficticios de familias jugadoras se generan directamente como `CharacterInstance` a partir de plantillas contextuales de su `Family` (rasgos de casa, no de individuo histórico).

### `CharacterInstance` (dato de partida — estado real, persistente, único por partida/`TimelineID`)

| Campo | Tipo | Notas |
|---|---|---|
| `CharacterId` | GUID | |
| `DefinitionId` | GUID? | nulo si es un personaje ficticio sin plantilla histórica |
| `FamilyId` | GUID | afiliación de casa primaria (económica/política, no genealógica — ver §2) |
| `TimelineID` | string | |
| `BirthDate` / `DeathDate` | WorldDate / WorldDate? | |
| `ParentIds` | List\<CharacterId\> (0-2) | genealogía real, independiente de `FamilyId` — ver §2 |
| `SpouseId` | CharacterId? | |
| `Age`, `Health`, `Experience` | valores de simulación | |
| `Inventory` | InventoryId | |
| `CurrentProfession` | string (dato) | puede diferir de `Profession` de la definición (puede cambiar en partida) |
| `CurrentRegionId` | RegionId | |
| `AcquiredTraits` | List\<TraitId\> | distintos de `BaseTraits`, ganados por decisiones |
| `RelationshipIds` | List\<RelationshipId\> | |
| `DecisionIds` | List\<DecisionId\> | decisiones tomadas por/sobre este personaje |
| `IsProtagonist` | bool | |
| `State` | enum | `ALIVE / DECEASED / MISSING` |

## 2. Genealogía relacional (no árboles copiados por familia)

Requisito explícito: si Salazar y Arrieta forman una rama mediante matrimonio, ambos linajes deben conservarse sin copiar personajes entre árboles.

**Decisión de diseño**: la genealogía **no** se deriva de `Family.MemberIds`. Se deriva exclusivamente del grafo `ParentIds`/`SpouseId` entre `CharacterInstance`. `FamilyId` es una etiqueta de **afiliación de casa** (para patrimonio, reputación regional, contexto de juego), completamente ortogonal a la genealogía biológica.

Consecuencia práctica: cuando un Salazar y una Arrieta se casan, su hijo es un `CharacterInstance` nuevo con `ParentIds = [padre_Salazar, madre_Arrieta]`. Ese hijo tiene un único `FamilyId` (la casa en la que es criado/hereda, decidido narrativamente), pero una consulta de ancestros (`GenealogyGraph.GetAncestors(characterId)`) atraviesa `ParentIds` sin que importe a qué `FamilyId` pertenece cada ancestro — el hijo sigue teniendo, correctamente, un abuelo Salazar y un abuelo Arrieta.

Servicio dedicado: **`GenealogyGraph`** (parte de `FamilySystem`, ver `06_FAMILY_SYSTEM.md`) — un grafo dirigido acíclico sobre `CharacterId`, independiente de la partición por `Family`. `Family.MemberIds` sigue existiendo como índice de conveniencia ("quiénes están afiliados a esta casa ahora"), pero nunca es la fuente de verdad genealógica.

## 3. Relación — `Relationship` (ampliado)

| Campo | Tipo | Notas |
|---|---|---|
| `RelationshipId` | GUID | |
| `TimelineID` | string | |
| `SourceId` / `TargetId` | GUID | `Character`, `Family` u `Organization` |
| `SimulationValue` | struct interno | `Trust`, `Respect`, `Rivalry`, `Debt`, `Fear`, `FamilialAffection`, `Hostility` — valores numéricos [-1,1] o decimal (deuda). **No se exponen directamente al jugador.** |
| `PlayerVisibleInformation` | dato derivado | etiquetas/tiers narrativos (ej. "Confía en tu familia", "Te teme") calculados desde `SimulationValue` por la capa UI/Content, nunca el número crudo — separación explícita requerida por el diseño |
| `KinshipType` | enum? | parentesco directo si aplica (complementario al grafo de `ParentIds`, para relaciones no biológicas: adopción narrativa futura, tutela) |
| `RegionalScope` | RegionId? | alcance territorial — reputación/relación no son globales |
| `LastUpdated` | WorldDate | |
| `OriginDecisionIds` | List\<DecisionId\> | trazabilidad |

## 4. Decisión y consecuencia

### `DecisionRecord`

| Campo | Tipo | Notas |
|---|---|---|
| `DecisionId` | GUID | |
| `Timestamp` (real, solo para auditoría técnica) | datetime | no confundir con `GameDate` |
| `GameDate` | WorldDate | |
| `PlayerId` | PlayerAccountId? | nulo si la decisión es de un NPC/simulación |
| `FamilyId`, `CharacterId` | GUID | |
| `LocationId` | GUID | |
| `Choice` | string (dato, referencia a `DecisionTypeDefinition`) | |
| `Participants` | List\<CharacterId\> | otros personajes/jugadores involucrados |
| `ImmediateEffects` | List\<EffectDefinitionId\> | aplicados al instante |
| `Tags` | List\<string\> | ver §5 (anti-explosión combinatoria) |

### `ConsequenceSpec` (ver `07_CONSEQUENCE_SYSTEM.md` para el motor completo)

| Campo | Tipo | Notas |
|---|---|---|
| `Kind` | enum | `IMMEDIATE / DELAYED / CONDITIONAL / GENERATIONAL / REGIONAL / RELATIONSHIP / ECONOMIC / HISTORICAL_CONTEXTUAL` |
| `TriggerConditions` | List\<TriggerCondition\> | expresión declarativa AND/OR/NOT (ver `07_CONSEQUENCE_SYSTEM.md` §2) |
| `EffectDefinitionId` | GUID | efecto delegado al sistema dueño de ese tipo de efecto |
| `HasFired` | bool | |

## 5. Reputación y relaciones — MVP

`FamilyReputation`: diccionario `RegionId → score` (no `GlobalReputation` único). Capa de grupo (comerciantes/militares/autoridades/comunidades/trabajadores) queda definida en el esquema (`GroupReputationRecord: RegionId, GroupTag, Score`) pero **no se puebla en el MVP** (P2, ver `02_SYSTEMS_MAP.md` §4) — se activa cuando el contenido lo requiera, sin cambio de esquema.

## 6. Patrimonio

| Entidad | Campos | Notas |
|---|---|---|
| `CharacterWealth` | `CharacterId`, `PersonalCash`, `PersonalItemIds` | bienes personales, no heredables directamente |
| `FamilyPatrimony` | `FamilyId`, `CashBalance`, `AssetIds`, `DebtIds` | heredable |
| `Asset` | `AssetId`, `AssetType` (dinero/propiedad/empresa/tierra/almacén/animal/embarcación/inversión/documento/objeto familiar), `RegionId`, `Value`, `OwnerType` (`CHARACTER/FAMILY`), `CoOwnerIds`, `AcquiredVia` (DecisionId?) | soporta copropiedad desde el esquema base |
| `DebtRecord` | `DebtId`, `CreditorId`, `DebtorId`, `Amount`, `DueDate` | |

Preparado desde el esquema (no requiere migración futura) para: herencia (vía `FamilyPatrimony` + `GenealogyGraph`), copropiedad (`CoOwnerIds`), pérdida/confiscación narrativa y daño (efectos de `ConsequenceEngine`/`HistoricalTimelineSystem` sobre `Asset.Value`/`State`), venta y transferencias multiplayer (operación transaccional contra el backend, ver `09_MULTIPLAYER_ARCHITECTURE.md` §4).

## 7. NPC: tres categorías

| Tipo | Definición | Restricciones |
|---|---|---|
| `HistoricalNPC` | Instanciado desde un `CharacterDefinition` con `HistoricalStatus = VERIFIED` o `NEEDS_RESEARCH` y ligado a un `HistoricalEventDefinition` real | **No** puede ser modificado arbitrariamente por sistemas aleatorios/simulación de fondo; su fecha de muerte y hechos centrales respetan `DeathConstraints`; ver `11_HISTORICAL_DATA_PIPELINE.md` |
| `NarrativeNPC` | Personaje ficticio pero importante (con `QuestGraph`/relaciones diseñadas) | Editable por diseño, no por simulación aleatoria de fondo |
| `PopulationNPC` | Personaje sistémico (población abstracta) | Generado/simulado por `WorldSimulationSystem`, sin identidad narrativa individual persistente salvo que sea "promovido" a `NarrativeNPC` por una interacción relevante del jugador |

## 8. Autoridad y crimen

### `JurisdictionDefinition` / `AuthorityRecord` (ya definidos en la iteración anterior, sin cambios de esquema)

### `CrimeEvent`

| Campo | Tipo | Notas |
|---|---|---|
| `CrimeEventId` | GUID | |
| `PerpetratorIds` | List\<CharacterId\> | |
| `WitnessIds` | List\<(CharacterId/NPCId, ObservationQuality)\> | calidad de observación, no todos los testigos son iguales |
| `RegionId` / `JurisdictionId` | GUID | |
| `EvidenceLevel` | float [0,1] | |
| `Date` | WorldDate | |
| `PropagationState` | referencia a `InformationRecord` | el conocimiento del delito se propaga como información, no como flag global (ver §9) |

## 9. Información (generaliza el conocimiento geográfico)

`InformationRecord` es el mecanismo general; `MapKnowledgeState` (conocimiento geográfico, `04_WORLD_ARCHITECTURE.md`) es una **especialización** de este mismo patrón (Topic = `LocationId`), no un sistema paralelo.

| Campo | Tipo | Notas |
|---|---|---|
| `InformationId` | GUID | |
| `HolderFamilyId` | GUID | el conocimiento es por familia |
| `Topic` | enum + ref | `ROUTE / PRICE / LOCATION / RUMOR / EVENT / CRIME`, con referencia al objeto (`RouteId`, `LocationId`, `CrimeEventId`, etc.) |
| `Source` | enum | viajero, comerciante, mapa, militar, autoridad, comunidad, explorador, ruta comercial, documento |
| `Reliability` | float [0,1] | |
| `DateReceived` | WorldDate | |
| `Region` | RegionId | |
| `Expiration` | WorldDate? | permite "última información: hace 12 días" |

## 10. Transporte y viaje

### `TransportDefinition`

| Campo | Tipo | Notas |
|---|---|---|
| `TransportId` | string (dato) | caminar, caballo, mula, carreta, embarcación, vapor, ferrocarril, (futuro: motorizado) |
| `Type` | enum | terrestre/fluvial/marítimo/ferroviario |
| `AvailableFrom` / `AvailableUntil` | WorldDate | disponibilidad histórica |
| `TerrainCompatibility` | List\<TerrainTag\> | |
| `Speed` | dato | |
| `Capacity` | int | pasajeros/carga |
| `Cost` | dato (referencia a `EconomySystem`) | |
| `RiskModifier` | float | modifica el riesgo base de la ruta |
| `InfrastructureRequirement` | List\<InfrastructureTag\> | debe cruzar con los tags de la región/ruta |

### `TravelSystem` — contrato de entrada/salida

**Input**: `Origin`, `Destination`, `Route`, `Transport`, `Date`, `Weather`, `Security` (leído de `WorldSimulationSystem`), `Party` (acompañantes), `Cargo`.
**Output**: `TravelTime`, `Cost`, `Risk`, `PossibleEvents`, `ArrivalDate`.
**Modos**: `TravelComplete` (exploración controlada), `TravelSummary` (rutas conocidas, interrumpible por eventos), `HistoricalFastTravel` (solo con infraestructura histórica correspondiente — estación/puerto). Detalle de integración con el calendario en `05_TIME_SYSTEM.md`.

## 11. Misiones — `QuestGraph`

No es un booleano de progreso; es un grafo:

| Campo | Tipo | Notas |
|---|---|---|
| `QuestId` | GUID | |
| `EntryPoints` | List\<QuestNodeId\> | múltiples formas de iniciar |
| `Nodes` | List\<QuestNode\> | cada nodo es un estado, no un paso lineal |
| `ResolutionNodes` | List\<QuestNodeId\> | múltiples soluciones válidas, incluyendo ignorar/perder/robar/transferir a otro jugador |
| `Deadline` | WorldDate? | opcional, evaluado por `GameCalendarSystem` |
| `Participants` | List\<CharacterId/PlayerAccountId\> | soporta participación multiplayer, personal o compartida |
| `ConsequenceHooks` | List\<(QuestNodeId, EffectDefinitionId)\> | cada resolución dispara `ConsequenceEngine`, reutilizando el motor existente en vez de un sistema de misiones con su propia lógica de efectos (Regla 5) |

## 12. Evento histórico — refinamiento del eje de clasificación

La versión anterior de este documento usaba un único booleano `OutcomeIsFixed`. Se reemplaza por un campo explícito y se aclara que son **dos ejes ortogonales**:

- `HistoricalStatus` (`VERIFIED / NEEDS_RESEARCH / FICTIONAL`) — eje de **rigor de investigación**: ¿qué tan fundamentado está el dato?
- `Modifiability` (`HARD / SOFT / FICTIONAL`) — eje de **libertad narrativa**: ¿puede el jugador alterar esto?
  - `HARD`: resultado macro fijo, no alterable (ej. Batalla de Ayacucho).
  - `SOFT`: el acontecimiento existe/es real, pero la participación y experiencia personal de la familia jugadora es libre.
  - `FICTIONAL`: contenido narrativo propio, sin restricción histórica.

Un evento puede ser `VERIFIED` + `HARD` (Ayacucho), `VERIFIED` + `SOFT` (contexto social documentado de una ciudad en una fecha, vivido libremente por el jugador), o `FICTIONAL` + `FICTIONAL` (una misión secundaria inventada). Ver `05_TIME_SYSTEM.md` §3 y `11_HISTORICAL_DATA_PIPELINE.md`.

| Campo adicional a lo ya definido | Tipo | Notas |
|---|---|---|
| `Modifiability` | enum | `HARD / SOFT / FICTIONAL` |
| `WorldChanges` | List\<EffectDefinitionId\> | efectos que `HistoricalTimelineSystem` aplica automáticamente al alcanzar la fecha (reutiliza el mismo `EffectDefinition` que `ConsequenceEngine`, no un mecanismo nuevo) |
| `NarrativeHooks` | List\<HookPointDefinition\> | dónde/cómo participa un personaje ficticio |
| `SourceReferences` | List\<string\>? | opcional, referencias de investigación — obligatorio en la práctica para todo evento `HARD` (ver `11_HISTORICAL_DATA_PIPELINE.md`) |

## 13. Persistencia vs. derivado (vigente, sin cambios de fondo)

Sin cambios respecto a la clasificación original: entidades de partida (`CharacterInstance`, `FamilyPatrimony`, `Asset`, `Relationship`, `DecisionRecord`, `MapKnowledgeState`/`InformationRecord`, `AuthorityRecord`, `CrimeEvent`) se guardan; datos de diseño (`CharacterDefinition`, `TransportDefinition`, `HistoricalEventDefinition`, `JurisdictionDefinition`) se cargan desde `Content` y no se serializan en el guardado; valores derivados (precios, seguridad de ruta, valor dinámico de `Asset`) se recalculan, nunca se guardan directamente. Detalle de mecánica de guardado en `08_SAVE_SYSTEM.md`.
