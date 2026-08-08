# DataModel.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

Este documento define las entidades de datos centrales del proyecto. Es el contrato entre todos los sistemas de `GameSystems.md`. Reglas transversales:

- Todo identificador (`*Id`) es un GUID estable, único globalmente, nunca reutilizado.
- Toda entidad persistente incluye `TimelineID` (string, por defecto `"prime"`) y `SchemaVersion` (int), aunque hoy solo exista una línea temporal — preparación para Paradoja (Regla 8) y para migraciones de guardado (`SaveSystem.md`).
- Las fechas de juego usan un tipo propio `WorldDate` (año, mes, día — calendario del mundo de juego), nunca `DateTime` real del sistema.
- Los datos históricos (personajes reales, eventos, ubicaciones documentadas) se separan físicamente de los datos de código (Regla 4): viven como assets de datos (`Assets/_Project/Data/Historical/...`), no como literales en C#. El modelo aquí descrito es el **esquema**; las instancias concretas (ej. "Mateo Salazar", "Batalla de Ayacucho") son datos, no clases nuevas.
- Todo dato con contenido histórico incluye el campo obligatorio `HistoricalStatus`: `VERIFIED | NEEDS_RESEARCH | FICTIONAL` (Regla 3).

---

## 1. Personaje — `Character`

| Campo | Tipo | Notas |
|---|---|---|
| `CharacterId` | GUID | |
| `FamilyId` | GUID | familia a la que pertenece |
| `TimelineID` | string | default `prime` |
| `FullName` | string | |
| `BirthDate` | WorldDate | |
| `DeathDate` | WorldDate? | nulo si vive |
| `IsProtagonist` | bool | si es el personaje controlable activo de su familia |
| `IsHistorical` | bool | true si corresponde a una figura histórica real |
| `HistoricalStatus` | enum | `VERIFIED / NEEDS_RESEARCH / FICTIONAL`; obligatorio si `IsHistorical == true`, opcional (default `FICTIONAL`) si no |
| `Attributes` | struct | rasgos/habilidades contextuales (no numérico-RPG genérico; contextual por familia/región, ver `GameSystems.md` §2) |
| `CurrentRegionId` | GUID | ubicación actual (nulo si histórico/fallecido) |
| `RelationshipIds` | List\<RelationshipId\> | ver `Relationship` |
| `KnownLocationIds` | List\<LocationId\> | referencia rápida, la fuente de verdad de conocimiento vive en `DiscoverySystem` |
| `InventoryId` | GUID | referencia a inventario personal |

## 2. Familia — `Family`

| Campo | Tipo | Notas |
|---|---|---|
| `FamilyId` | GUID | |
| `TimelineID` | string | |
| `Name` | string | ej. "Salazar" |
| `OriginRegionId` | GUID | ej. Pisco-Paracas |
| `ContextualAdvantages` | List\<AdvantageTag\> | comercio/transporte, arriería/agricultura, navegación, artesanía/administración — nunca un multiplicador global "mejor familia" (principio de diseño §2 del prompt maestro) |
| `ActiveProtagonistId` | CharacterId | quién controla el jugador ahora mismo |
| `MemberIds` | List\<CharacterId\> | vivos y fallecidos (árbol genealógico completo) |
| `PatrimonyId` | GUID | ver `PatrimonySystem` |
| `OwningPlayerAccountId` | GUID? | nulo si es familia NPC no jugadora |

## 3. Relación — `Relationship`

| Campo | Tipo | Notas |
|---|---|---|
| `RelationshipId` | GUID | |
| `TimelineID` | string | |
| `SourceId` | GUID | Character o Family |
| `TargetId` | GUID | Character o Family |
| `Trust` | float [-1,1] | confianza |
| `Respect` | float [-1,1] | respeto |
| `Fear` | float [-1,1] | temor |
| `Rivalry` | float [-1,1] | rivalidad |
| `DebtAmount` | decimal | deuda pendiente (puede ser negativa = acreedor) |
| `KinshipType` | enum? | parentesco directo, si aplica |
| `RegionalScope` | GUID? | jurisdicción/región donde aplica (reputación no es global — §12 del prompt maestro) |
| `LastUpdated` | WorldDate | |
| `OriginConsequenceIds` | List\<DecisionEventId\> | trazabilidad: qué decisiones construyeron esta relación |

## 4. Decisión / Evento de consecuencia — `DecisionEvent`

Núcleo del `ConsequenceEngine` (§3 de `GameSystems.md`), reflejando literalmente el esquema pedido en el prompt maestro:

| Campo | Tipo | Notas |
|---|---|---|
| `DecisionId` | GUID | |
| `TimelineID` | string | |
| `CharacterId` | GUID | quién decide |
| `FamilyId` | GUID | |
| `Date` | WorldDate | cuándo ocurrió |
| `Region` | GUID | dónde |
| `Target` | GUID? | personaje/familia/entidad afectada, si aplica |
| `DecisionType` | string (dato, no enum cerrado) | referencia a `DecisionTypeDefinition` en datos |
| `Consequences` | List\<ConsequenceSpec\> | ver abajo |
| `Visibility` | enum | `PRIVATE / LOCAL / REGIONAL / PUBLIC` — quién puede llegar a enterarse |
| `TriggerConditions` | List\<TriggerCondition\> | condiciones para que una consecuencia diferida se dispare |

### `ConsequenceSpec` (elemento de `Consequences`)

| Campo | Tipo | Notas |
|---|---|---|
| `Kind` | enum | `IMMEDIATE / DELAYED / GENERATIONAL / REGIONAL / FAMILIAL` |
| `DelayRange` | (WorldDate min, WorldDate max)? | para `DELAYED`/`GENERATIONAL` |
| `EffectDefinitionId` | GUID | referencia a dato que describe el efecto (cambio de relación, patrimonio, reputación, aparición de misión, etc.) — el motor no interpreta el efecto, lo delega al sistema dueño de ese tipo de efecto |
| `HasFired` | bool | |

## 5. Región / Ubicación — `Region` y `Location`

| Campo (`Region`) | Tipo | Notas |
|---|---|---|
| `RegionId` | GUID | |
| `Name` | string | |
| `Biome` | enum | `COSTA / SIERRA / SELVA` |
| `ParentRegionId` | GUID? | jerarquía (ej. Pisco dentro de Costa Sur) |
| `HistoricalStatus` | enum | (la geografía/infraestructura también se clasifica: fronteras y rutas deben basarse en documentación — §6 del prompt maestro) |
| `EraSnapshots` | List\<RegionEraSnapshot\> | el mismo lugar en distintos años (Lima 1821 vs 1900) — ver abajo |
| `JurisdictionId` | GUID | referencia a `AuthoritySystem` |

| Campo (`RegionEraSnapshot`) | Tipo | Notas |
|---|---|---|
| `ActOrYearRange` | (WorldDate, WorldDate) | vigencia de este snapshot |
| `LayoutAssetRef` | asset ref | escena/streaming de esa época |
| `PopulationEstimate` | int | usado por `WorldSimulationSystem` |
| `InfrastructureTags` | List\<string\> | ej. `HAS_RAILWAY`, `HAS_PORT` — condiciona `TravelSystem` |

| Campo (`Location`) | Tipo | Notas |
|---|---|---|
| `LocationId` | GUID | punto de interés específico dentro de una `Region` |
| `RegionId` | GUID | |
| `LocationType` | string (dato) | vivienda, mercado, puerto, hacienda, etc. |

## 6. Conocimiento geográfico — `MapKnowledgeState`

| Campo | Tipo | Notas |
|---|---|---|
| `FamilyId` | GUID | el conocimiento es por familia, no global |
| `LocationId` | GUID | |
| `State` | enum | `UNKNOWN / RUMOR / MAPPED / VISITED / MASTERED` |
| `SourceType` | enum | viajero, comerciante, mapa, militar, autoridad, comunidad, explorador, ruta comercial, documento |
| `AcquiredDate` | WorldDate | |
| `Inheritable` | bool | si se transmite a la siguiente generación |

## 7. Ruta y transporte — `Route`, `TravelMethodDefinition`

| Campo (`Route`) | Tipo | Notas |
|---|---|---|
| `RouteId` | GUID | |
| `FromRegionId` / `ToRegionId` | GUID | |
| `BaseDistance` | float | |
| `TerrainProfile` | enum/dato | condiciona métodos disponibles |
| `SecurityState` | referencia | leído de `WorldSimulationSystem`, no almacenado aquí (evitar duplicar fuente de verdad) |

| Campo (`TravelMethodDefinition`) | Tipo | Notas |
|---|---|---|
| `MethodId` | string (dato) | caminar, caballo, mula, caravana, carreta, embarcación, vapor, ferrocarril, fluvial, (futuro: motorizado) |
| `AvailableFrom`/`AvailableUntil` | WorldDate | disponibilidad histórica |
| `RequiredInfrastructureTags` | List\<string\> | debe cruzar con `InfrastructureTags` de la región |
| `SpeedProfile` | dato | usado por `TravelSystem` para calcular tiempo |
| `CostModel` | dato | usado junto a `EconomySystem` |

## 8. Patrimonio — `FamilyPatrimony`, `Asset`

| Campo (`FamilyPatrimony`) | Tipo | Notas |
|---|---|---|
| `PatrimonyId` | GUID | 1:1 con `Family` |
| `CashBalance` | decimal | |
| `AssetIds` | List\<AssetId\> | |
| `DebtIds` | List\<DebtId\> | |

| Campo (`Asset`) | Tipo | Notas |
|---|---|---|
| `AssetId` | GUID | |
| `AssetType` | string (dato) | casa, tierra, negocio, almacén, animal, embarcación, inversión, documento, conexión |
| `RegionId` | GUID | |
| `Value` | decimal | dinámico, recalculado por `EconomySystem` |
| `AcquiredVia` | DecisionEventId? | trazabilidad hacia el motor de consecuencias |

## 9. Evento histórico — `HistoricalEventDefinition`

| Campo | Tipo | Notas |
|---|---|---|
| `EventId` | GUID | |
| `Name` | string | ej. "Batalla de Ayacucho" |
| `Date` | WorldDate | |
| `RegionId` | GUID | |
| `HistoricalStatus` | enum | `VERIFIED / NEEDS_RESEARCH / FICTIONAL` — obligatorio |
| `OutcomeIsFixed` | bool | true para acontecimientos macro (Regla del prompt maestro §5): el resultado histórico no es alterable por el jugador |
| `PlayerHookPoints` | List\<HookPointDefinition\> | dónde y cómo un personaje ficticio puede participar sin alterar el resultado fijo |
| `ActId` | GUID | referencia al Acto (I-VII) al que pertenece |

## 10. Autoridad y jurisdicción — `JurisdictionDefinition`, `AuthorityRecord`

| Campo (`JurisdictionDefinition`) | Tipo | Notas |
|---|---|---|
| `JurisdictionId` | GUID | |
| `RegionId` | GUID | |
| `AuthorityTypeByEra` | List\<(WorldDate range, AuthorityTypeId)\> | qué tipo de autoridad existe en qué rango de fechas (nunca hardcodeado en código — Regla 4) |

| Campo (`AuthorityRecord`) | Tipo | Notas |
|---|---|---|
| `CharacterId` | GUID | |
| `JurisdictionId` | GUID | alcance territorial explícito — nunca "buscado en todo Perú" |
| `Status` | enum | `CLEAN / SUSPECTED / WANTED` |
| `OriginDecisionId` | GUID? | trazabilidad |

## 11. Cuenta de jugador y sesión — `PlayerAccount`, `GameSession`

| Campo (`PlayerAccount`) | Tipo | Notas |
|---|---|---|
| `PlayerAccountId` | GUID | |
| `DisplayName` | string | |
| `OwnedFamilyIds` | List\<FamilyId\> | histórico de familias jugadas a través de generaciones/timelines |

| Campo (`GameSession`) | Tipo | Notas |
|---|---|---|
| `SessionId` | GUID | sesión en tiempo real (ver `MultiplayerArchitecture.md`) |
| `ParticipantAccountIds` | List\<PlayerAccountId\> | 1 a 4 |
| `RegionId` | GUID | dónde coinciden |
| `StartedAt` | WorldDate | |

## 12. Acto / Era — `ActDefinition`

| Campo | Tipo | Notas |
|---|---|---|
| `ActId` | GUID | I a VII |
| `Name` | string | |
| `DateRange` | (WorldDate, WorldDate) | ej. 1820-1824 |
| `Summary` | string | dato de diseño, no lógica |

---

## Diagrama de relaciones (resumen textual)

```
PlayerAccount 1───N Family 1───N Character
                        │             │
                        │             └── N Relationship N ── Character/Family
                        │
                        ├── 1 FamilyPatrimony 1───N Asset
                        │
                        └── N MapKnowledgeState N── Location N── Region

Character/Family ── genera ──> N DecisionEvent 1───N ConsequenceSpec
                                        │
                                        └── referencia ──> EffectDefinition (dato, interpretado por el sistema dueño del efecto)

Region 1───N RegionEraSnapshot
Region N───N Route (Terrain/Infra) ── habilitado por ──> TravelMethodDefinition
Region 1───1 JurisdictionDefinition 1───N AuthorityRecord (por Character)

ActDefinition 1───N HistoricalEventDefinition (Region, Date, HistoricalStatus)
```

---

## Persistencia vs. derivado (importante para `SaveSystem.md`)

No todo lo anterior se guarda igual:

- **Fuente de verdad persistente** (se guarda tal cual): `Character`, `Family`, `Relationship`, `DecisionEvent`, `FamilyPatrimony`, `Asset`, `MapKnowledgeState`, `AuthorityRecord`, `PlayerAccount`.
- **Datos de diseño, no de partida** (se cargan desde assets, no se "guardan" — son iguales para todas las partidas salvo parche): `TravelMethodDefinition`, `HistoricalEventDefinition`, `JurisdictionDefinition.AuthorityTypeByEra`, `ActDefinition`, `RegionEraSnapshot` (la geometría), definiciones de `AssetType`/`DecisionType`/`EffectDefinition`.
- **Derivado/calculado, nunca guardado directamente** (se recalcula desde lo anterior): precios de `EconomySystem`, estado de seguridad de `Route` (viene de `WorldSimulationSystem`), valor dinámico de `Asset`.

Esta distinción es la que evita que el sistema de guardado intente serializar contenido de diseño (que vive en control de versiones como datos, no como partida) junto con el progreso real del jugador.
