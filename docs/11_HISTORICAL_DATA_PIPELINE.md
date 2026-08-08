# 11_HISTORICAL_DATA_PIPELINE.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Flujo de trabajo

```
Research  →  Source  →  Verification  →  Game Data  →  Narrative
```

| Etapa | Qué ocurre | Responsable de datos |
|---|---|---|
| **Research** | Recolección de información histórica sobre un personaje/evento/ubicación/época | Fuera del código; documentado externamente |
| **Source** | Se registran las referencias concretas (`SourceReferences`) que respaldan el dato | Campo de datos, no comentario de código |
| **Verification** | Se asigna `HistoricalStatus` (`VERIFIED / NEEDS_RESEARCH / FICTIONAL`) y, si aplica, `Modifiability` (`HARD / SOFT / FICTIONAL` — `03_DATA_MODEL.md` §12) | Campo obligatorio del asset de datos |
| **Game Data** | El dato entra a `Content` como `HistoricalEventDefinition`/`CharacterDefinition`/`LocationDefinition`/`RegionEraSnapshot`, referenciable por ID | Asset de datos, separado del código (Regla 4) |
| **Narrative** | Diseño narrativo construye `QuestGraph`/diálogo/`NarrativeHooks` sobre el dato ya verificado, sin modificar el registro histórico base | Contenido narrativo, referencia el dato, no lo duplica |

Regla dura: **ningún dato histórico entra a `Content` sin pasar por Verification.** Un dato sin `HistoricalStatus` asignado no es válido — se fuerza mediante validación de datos en `tools/` (rechazo de build/integración si falta el campo).

## 2. Auditabilidad

Cada `HistoricalEventDefinition`/`CharacterDefinition` (cuando `IsHistorical = true`) expone: `HistoricalStatus`, `Modifiability`, `SourceReferences` (opcional en general, **obligatorio en la práctica para todo evento `HARD`** — un resultado histórico fijo debe estar respaldado por referencia, no por criterio de diseño no documentado). Esto permite auditar el contenido completo del juego por estado de verificación en cualquier momento (ej. "listar todo lo `NEEDS_RESEARCH` antes de cerrar un Acto" — ver `12_DEVELOPMENT_ROADMAP.md`, QA de cierre de Acto).

## 3. Tres tipos de NPC (recordatorio operativo, esquema completo en `03_DATA_MODEL.md` §7)

- **`HistoricalNPC`**: instanciado desde `CharacterDefinition` con `HistoricalStatus ∈ {VERIFIED, NEEDS_RESEARCH}`. Restricción dura: ningún sistema de simulación aleatoria (`WorldSimulationSystem`, generación de eventos de fondo) puede alterar su destino histórico central durante el juego base — su `DeathConstraints` y los `HARD` events que lo involucran son inmutables por diseño. Puede tener comportamiento y diálogo dinámico *alrededor* de esos hechos fijos, nunca en contradicción con ellos.
- **`NarrativeNPC`**: ficticio pero relevante (tiene `QuestGraph`/relaciones diseñadas). Editable por diseño; no sujeto a las restricciones de `HistoricalNPC`, pero tampoco generado aleatoriamente — su existencia y arco son contenido narrativo deliberado.
- **`PopulationNPC`**: generado y simulado por `WorldSimulationSystem`, sin identidad narrativa individual persistente, salvo que una interacción del jugador lo "promueva" a `NarrativeNPC` (mecanismo de promoción, no dos sistemas de identidad paralelos).

## 4. Eventos `HARD` vs `SOFT` vs `FICTIONAL` — ejemplo de referencia

- `BattleOfAyacucho`: `HistoricalStatus = VERIFIED`, `Modifiability = HARD`. Resultado macro no alterable; `WorldChanges` se aplican automáticamente al alcanzar la fecha (`05_TIME_SYSTEM.md` §3).
- `PlayerFamilyMissionAroundAyacucho`: `Modifiability = SOFT` o `FICTIONAL` según el caso — el contexto (que la batalla ocurre, que hay movilización, escasez, riesgo en rutas cercanas) es real y fijo; la participación específica de la familia jugadora (quién va, quién se queda, qué le ocurre a un personaje ficticio) es libre.

## 5. Clasificación de contenido sensible

El juego representa guerras, violencia política, delincuencia, conflictos sociales y periodos traumáticos reales. Se define, desde esta fase, una capa ligera de gobernanza de contenido (no un sistema de gameplay):

- **`ContentClassification`** por asset narrativo/histórico relevante: `IntensityTags` (violencia, trauma, conflicto político), `IntensityLevel` (`LOW/MEDIUM/HIGH`), `PresentationMode` (`IMPLIED` / `DEPICTED` / `DOCUMENTED`).
- **Principio de diseño**: la violencia no es el atractivo central ni la "recompensa" del gameplay (coherente con `CombatSystem` como sistema contextual, no de espectáculo — ver el prompt maestro original §11). El combate resuelve supervivencia y consecuencia, no puntuación.
- **Separación de registros**: documentación histórica (dato verificado, `HistoricalStatus`), representación narrativa (cómo se cuenta en juego) y gameplay (cómo se juega) se mantienen como capas distintas y auditables por separado — un evento traumático puede estar `VERIFIED` en lo documental sin que su representación en juego sea gráfica o explotadora.
- Esto no bloquea ni resuelve decisiones de contenido específicas ahora (fuera de alcance de Fase 0); fija el campo de datos y el principio para que el contenido de Fase 12+ lo aplique de forma consistente desde el principio, no como un pase de revisión al final.

## 6. Fuera de alcance de esta fase

No se investiga ni se carga contenido histórico real todavía (eso ocurre progresivamente desde Fase 2 en adelante, con profundidad completa en Fase 12+). Esta fase fija el pipeline y los campos obligatorios de clasificación, no el contenido.
