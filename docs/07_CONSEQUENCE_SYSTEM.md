# 07_CONSEQUENCE_SYSTEM.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. `DecisionSystem`

Registra cada decisión narrativamente relevante como un `DecisionRecord` (esquema completo en `03_DATA_MODEL.md` §4). El sistema en sí es delgado: valida el registro, lo persiste (vía `SaveSystem`), y lo publica al `ConsequenceEngine`. No decide qué significa la decisión — eso lo determinan los datos (`EffectDefinition` referenciados) y los sistemas que los interpretan.

## 2. `ConsequenceEngine` — ocho tipos de consecuencia

| Kind | Cuándo ocurre |
|---|---|
| `IMMEDIATE` | Al momento de la decisión |
| `DELAYED` | Fecha futura fija o relativa (`GameCalendarSystem.ScheduleRelative`, `05_TIME_SYSTEM.md` §1) |
| `CONDITIONAL` | Depende de que otra condición se cumpla, sin fecha fija — evaluado de forma reactiva cuando cambia el estado del que depende (no por polling) |
| `GENERATIONAL` | Afecta descendientes; se resuelve consultando `GenealogyGraph` (`06_FAMILY_SYSTEM.md` §3) desde el `CharacterId`/`FamilyId` origen |
| `REGIONAL` | Afecta a una `Region` (economía, seguridad, reputación) en vez de a un personaje específico |
| `RELATIONSHIP` | Modifica un `Relationship.SimulationValue` (`03_DATA_MODEL.md` §3) |
| `ECONOMIC` | Modifica `FamilyPatrimony`/`Asset`/`EconomySystem` |
| `HISTORICAL_CONTEXTUAL` | Modifica cómo la familia *experimenta* un evento histórico (diálogo, disponibilidad de misión, tono narrativo) sin alterar el `WorldChanges` fijo del evento — es el punto de integración con `HistoricalTimelineSystem` para eventos `SOFT`/`HARD` (`05_TIME_SYSTEM.md` §3) |

Motor de evaluación de condiciones: `TriggerCondition` es una expresión declarativa compuesta por predicados tipados combinables con `AND/OR/NOT` — no una condición de texto libre ni un script por decisión:

```
TriggerCondition ::=
    DateReached(WorldDate)
  | FlagSet(FlagId)
  | RelationshipThreshold(RelationshipId, Metric, Comparator, Value)
  | CharacterAlive(CharacterId) / CharacterDeceased(CharacterId)
  | RegionState(RegionId, StateKey, Comparator, Value)
  | DecisionTagPresent(Tag, Scope)   // Scope = Character/Family/Region
  | AND(TriggerCondition, TriggerCondition)
  | OR(TriggerCondition, TriggerCondition)
  | NOT(TriggerCondition)
```

Ejemplo del encargo, expresado en este esquema (dato, no código hardcodeado):
```
DecisionTagPresent("HelpedChild", Family)  AND  DateReached(1850-01-01)
  → EffectDefinition: UnlockNPCDoctorRelationship
```

## 3. Mecanismos anti-explosión combinatoria (requisito explícito de esta fase)

Con cien años de historia, hasta cuatro familias jugadoras y generaciones sucesivas, el número de `DecisionRecord` y de reglas de consecuencia potenciales crece rápido. Se controla con cinco mecanismos concretos, no con "cuidado" al escribir contenido:

1. **Efectos por tipo, no por decisión.** `EffectDefinition` es un catálogo finito de *tipos* de efecto (`RelationshipChange`, `PatrimonyChange`, `ReputationChange`, `UnlockContent`, `SpawnNPCEvent`, `ModifyKnowledgeState`, `RegionalStateChange`...) — del orden de una decena de ejecutores de efecto en código. Miles de `DecisionRecord` distintos se expresan combinando estos mismos tipos con datos distintos; la complejidad de código no crece con el número de decisiones de contenido.
2. **Reglas por etiqueta, no por instancia literal.** `DecisionRecord.Tags` (`03_DATA_MODEL.md` §4) permite que una regla de consecuencia haga match contra un patrón ("cualquier decisión etiquetada `HelpedStranger` en esta región") en vez de requerir una regla única por cada decisión literal posible — esto es lo que evita "una regla por combinación", que es la fuente real de explosión combinatoria si se hiciera al revés.
3. **Indexación por sujeto, no escaneo del log completo.** Las consultas de `ConsequenceEngine` (`QueryConsequencesFor(characterId/familyId/region)`) usan índices por `CharacterId`, `FamilyId`, `RegionId` y `WorldDate` de disparo — el coste de evaluar qué consecuencias aplican a "este personaje, ahora" es proporcional al subconjunto relevante, no al total de decisiones registradas en 100 años de partida.
4. **Evaluación reactiva, no polling.** Las condiciones `CONDITIONAL`/`RELATIONSHIP`/`REGIONAL` se reevalúan cuando el estado del que dependen cambia (vía `Core.EventBus`), no en un bucle que recorre todas las consecuencias pendientes en cada tick.
5. **Archivado del índice activo, log inmutable.** El `DecisionRecord`/`ConsequenceSpec` original nunca se borra (es la fuente de "Tu Legado" y de auditoría), pero una vez una consecuencia dispara (`HasFired = true`) y no tiene disparadores dependientes pendientes, se saca del índice "caliente" de reglas activas (puede archivarse/resumirse para consulta histórica de baja frecuencia). Esto mantiene acotado el conjunto de reglas que el motor evalúa activamente en cualquier momento, independientemente de cuántas décadas lleve la partida.

## 4. `QuestGraph` — reutiliza el motor, no crea uno nuevo

`QuestGraph` (esquema completo en `03_DATA_MODEL.md` §11) es nodos + transiciones + `ConsequenceHooks`. Cada resolución de nodo (entregar la carta, perderla, robarla, dársela a otro jugador, ignorarla) dispara `ConsequenceEngine` a través del mismo `EffectDefinition` que cualquier otra decisión — deliberadamente no existe un "motor de efectos de misión" separado (Regla 5: sistemas generales reutilizables). La única pieza específica de `QuestGraph` es la estructura de nodos/entradas/salidas múltiples y el soporte de `Deadline` (consultado contra `GameCalendarSystem`) y `Participants` multiplayer.

## 5. Visibilidad y secretos

`DecisionRecord`/`Relationship` llevan `Visibility` (`PRIVATE / LOCAL / REGIONAL / PUBLIC`). Esto es lo que separa "secretos de familia" (visibles solo internamente, consultables por `ConsequenceEngine` para efectos generacionales aunque nadie más los vea nunca en UI) de información que otros jugadores/NPC pueden llegar a conocer mediante `InformationSystem` (`03_DATA_MODEL.md` §9) — la propagación de un secreto a conocimiento público es, en sí misma, un tipo de consecuencia (`RELATIONSHIP`/`REGIONAL`) modelada con el mismo motor, no un sistema aparte.

## 6. Pruebas mínimas requeridas antes de considerar esta fase cerrada para este sistema

(A ejecutar en Fase 4, no ahora — se dejan enunciadas para que el diseño de esta fase quede verificable):

1. Decisión `IMMEDIATE` con efecto de relación aplicado en el acto.
2. Decisión `DELAYED` (ej. ayuda en 1820 → efecto en 1830 vía `ScheduleRelative`).
3. Decisión `GENERATIONAL` (ayuda en 1820 → descendiente la recuerda en 1840, atravesando `GenealogyGraph`).
4. Decisión `CONDITIONAL` con `AND` de dos predicados de tipos distintos (fecha + relación).
5. Decisión `ECONOMIC` con impacto medible en `FamilyPatrimony`, verificando que el índice por `FamilyId` recupera la consecuencia sin escanear el log completo.
