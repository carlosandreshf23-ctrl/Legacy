# 08_SAVE_SYSTEM.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Qué debe guardarse (mínimo exigido)

Fecha del mundo, jugadores, familias, personajes, genealogía, patrimonio, reputación, relaciones, decisiones, consecuencias, misiones, inventarios, propiedades, ubicaciones, conocimiento (información), economía regional, eventos regionales. Ver el detalle de qué entidad concreta corresponde a cada uno en `03_DATA_MODEL.md` §13 ("Persistencia vs. derivado") — este documento no repite ese catálogo, define **cómo** se guarda de forma eficiente.

## 2. Arquitectura: Snapshots + Event Log

Se adopta explícitamente el patrón sugerido, equivalente a *event sourcing con snapshotting periódico*:

- **Event Log (fuente de verdad para "cómo llegamos aquí")**: el flujo append-only de `DecisionRecord` + `ConsequenceSpec` disparadas + eventos históricos aplicados (`HistoricalTimelineSystem`). Nunca se sobrescribe ni se edita en el sitio; es la misma estructura que ya usa `07_CONSEQUENCE_SYSTEM.md` para el motor de consecuencias — el guardado no duplica esto en un formato distinto, lo persiste tal cual.
- **Snapshot (estado materializado a una fecha)**: una foto completa y directamente cargable de `CharacterInstance`, `Family`, `FamilyPatrimony`, `Relationship`, `MapKnowledgeState`, `AuthorityRecord`, etc., válida a un `WorldDate` concreto.
- **Carga de partida = último snapshot + replay de solo los eventos posteriores a ese snapshot.** Nunca se recorre el log completo desde 1820 para cargar una partida de 1890 — se toma el snapshot más reciente anterior o igual a la fecha objetivo y se reproduce únicamente el tramo faltante.

### Ventajas

- Tiempo de carga acotado y predecible (no crece indefinidamente con la duración de la partida), crítico en móvil.
- El log completo se conserva íntegro para: auditoría, el motor de consecuencias generacionales (que necesita historia real, no solo el último estado), y la generación de "Tu Legado" (cronología completa al final del juego base).
- Las migraciones de esquema (§4) pueden aplicarse tanto a snapshots como, si es necesario, reproducirse sobre el log con el migrador correspondiente por versión de evento.
- Depurar un estado incorrecto es posible reproduciendo el log paso a paso desde el snapshot anterior, en vez de solo tener "el estado actual" sin explicación de cómo se llegó a él.

### Riesgos y mitigación

| Riesgo | Mitigación |
|---|---|
| Divergencia entre snapshot y log (el snapshot no refleja fielmente el resultado de reproducir el log hasta esa fecha) | El snapshot se genera *siempre* como resultado de un replay real, nunca escrito por una ruta alternativa; se valida periódicamente (herramienta de `tools/`, no en runtime) comparando snapshot vs. replay completo en builds de CI |
| Replay lento si se acumulan muchos eventos entre snapshots | Cadencia de snapshot regular (propuesta inicial: uno por año de juego por campaña, o cada N `DecisionRecord`, lo que ocurra primero — ajustable con datos reales) |
| Crecimiento del tamaño del log a lo largo de 100 años × 4 familias | El log es la responsabilidad del backend (`09_MULTIPLAYER_ARCHITECTURE.md` §2), no del dispositivo móvil; el cliente solo necesita el snapshot vigente + los eventos recientes relevantes a su sesión, no el log completo histórico |
| Corrupción de un snapshot concreto | Se puede reconstruir reproduciendo desde el snapshot anterior; ver §5 |

## 3. Dos ámbitos, sin cambios respecto a la decisión previa de Fase 0

- **Guardado de mundo (compartido, backend)**: snapshot + log tal como se describe arriba, para todo lo marcado "fuente de verdad persistente" en `03_DATA_MODEL.md` §13.
- **Guardado local (cliente)**: configuración, caché de assets, cola de intenciones pendientes (`PendingIntent`), progreso puramente de sesión. Formato JSON comprimido / SQLite ligero, no snapshot+log (no lo necesita: no tiene historia generacional que preservar).

Interfaz común `IPersistable<T>` (sin cambios respecto a la propuesta previa): cada sistema con estado expone `CaptureState()`/`RestoreState()`/`SchemaVersion`; `SaveSystem` orquesta cuándo y contra qué repositorio (local vs. backend) se aplica, sin conocer el contenido de cada sistema.

## 4. Versionado y migración (`SaveVersion`)

- Cada tipo de entidad serializada (snapshot de `CharacterInstance`, de `Family`, cada tipo de evento del log) lleva su propio `SchemaVersion`, no un único número global de "versión de guardado" — permite migrar solo lo que cambió en una actualización dada.
- Cadena de migradores `IStateMigrator`: `v1 → v2 → v3`, aplicados secuencialmente y de forma append-only (nunca se reescribe un migrador ya publicado; una corrección se agrega como un migrador nuevo). Esto es válido tanto para snapshots como para el formato de eventos del log, dado que el log debe seguir siendo reproducible con versiones de esquema antiguas mezcladas con nuevas a lo largo de los años de desarrollo.
- Dado que el proyecto se construye en fases a lo largo de mucho tiempo (`12_DEVELOPMENT_ROADMAP.md`), se asume que el esquema cambiará decenas de veces; el versionado por entidad es lo que hace esto sostenible sin invalidar partidas antiguas en cada actualización.

## 5. Recuperación ante corrupción

- Backend: el log es append-only, por lo que un snapshot corrupto es recuperable reproduciendo desde el snapshot íntegro anterior más cercano.
- Cliente: se mantienen las últimas N copias locales rotadas antes de sobrescribir; checksum en cada blob serializado; si falla la validación, se cae a la copia anterior y se notifica al jugador explícitamente en vez de fallar en silencio (Regla del proyecto sobre errores, §19 del prompt maestro original).

## 6. Cuándo se guarda (triggers) — sin cambios de fondo

Confirmación de decisión narrativa relevante (mundo/backend), transición entre regiones (checkpoint local + sync oportunista), cambio de protagonista por muerte (mundo/backend, crítico — no puede depender de autosave posterior), autosave periódico (local), guardado explícito (local + intento de sync), salida de sesión multijugador en tiempo real (mundo/backend, obligatorio antes de cerrar sesión).

## 7. Fuera de alcance de esta fase

Backend real, base de datos concreta, cadencia exacta de snapshot ajustada con datos de producción. Se fija el contrato (`IPersistable`, patrón snapshot+log, versionado por entidad); Fase 1 implementa una versión local-only detrás de este contrato; Fase 8-9 implementa el backend real detrás del mismo contrato.
