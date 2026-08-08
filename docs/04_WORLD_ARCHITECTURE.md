# 04_WORLD_ARCHITECTURE.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Jerarquía Regiones + Subregiones + Locations

```
Macroregión (Costa / Sierra / Selva)
   └── Región (ej. Costa Sur, Sierra Central)
         └── Subregión (ej. Pisco-Paracas, Valle del Mantaro)
               └── Location (punto de interés: vivienda, mercado, puerto, hacienda)
```

- `Region.ParentRegionId` permite esta jerarquía sin tipos separados por nivel (Macroregión, Región, Subregión son todas instancias de `Region` con distinta profundidad — un único esquema, no tres).
- `Region.Biome ∈ {COSTA, SIERRA, SELVA}` se fija en la macroregión y se hereda hacia abajo (no se redefine en cada subregión).
- `Location` es siempre hoja: no tiene hijos, pertenece a exactamente una `Region`.

No se construye el mapa completo de Perú en Fase 0-6 (§7 del prompt original). La jerarquía se puebla progresivamente: Fase 2 solo necesita una subregión (Pisco-Paracas) con un puñado de `Location`; Fase 6 añade un prototipo por bioma; el resto se construye durante los Actos (Fase 12+).

## 2. Streaming, carga y descarga

Decisión: **Addressables + escenas aditivas por Subregión**, no world streaming continuo tipo World Partition. Justificación: el mundo de LEGADO: PERÚ es semiabierto por regiones discretas conectadas por viaje (que ya consume tiempo de juego, §23 del prompt/`05_TIME_SYSTEM.md`), no un mundo abierto continuo sin cortes — el "corte" narrativo de viajar ya existe por diseño, así que no hace falta ocultar las transiciones de streaming con técnicas de mundo continuo.

- Cada `Region`/`Subregión` es una **escena Addressable** cargada aditivamente cuando el jugador viaja hacia ella o se aproxima a su borde; se descarga cuando ya no hay jugadores ni actividad de alta fidelidad activa en ella.
- `RegionEraSnapshot` (ver §3) referencia el asset de escena correspondiente a la época activa — cargar una región implica resolver primero qué snapshot temporal corresponde a la fecha actual del mundo.
- **Persistencia de estado al descargar**: el estado narrativo/económico de una región (precios, seguridad, NPC de alta fidelidad con tareas en curso) no vive en la escena: vive en los sistemas de datos (`EconomySystem`, `WorldSimulationSystem`, `FamilySystem`). Descargar la escena solo libera geometría/render; el estado lógico sigue existiendo y simulándose de forma abstracta (§5).
- **Transición entre regiones**: mientras el jugador viaja, la región de origen se descarga tras un margen razonable, la región de destino se precarga cuando el viaje está próximo a completarse (usando el tiempo de viaje real como ventana de precarga, no un loading screen brusco salvo en el modo `TravelComplete` donde el propio viaje ya es la transición jugable).

## 3. `TemporalWorldState` — un lugar, múltiples épocas sin duplicar todo

Un mismo `LocationId`/`RegionId` puede lucir radicalmente distinto en 1821 que en 1900 (Lima como ejemplo canónico). Se modela por **composición en capas**, no por assets independientes completos por año:

```
WorldStateActual  =  BaseLocation  +  HistoricalLayer(fecha activa)  +  DynamicPlayerChanges
```

- **`BaseLocation`**: geometría y layout que no cambia entre épocas (topografía, trazado de calles principales estables, geografía natural). Se modela una sola vez.
- **`HistoricalLayer`**: el `RegionEraSnapshot` correspondiente al rango de fechas activo (`ActOrYearRange`) — añade/oculta edificios, rutas, infraestructura (`InfrastructureTags`: `HAS_RAILWAY`, `HAS_PORT`, etc.), cambia densidad de población estimada, y referencia variantes de material/prop de época (ver `33` del prompt — pipeline de assets reutilizables: casa base + variantes regionales/históricas). Varias décadas pueden compartir el mismo `HistoricalLayer` si no hubo cambios estructurales relevantes (no se crea un snapshot por año, solo cuando hay un cambio real documentado).
- **`DynamicPlayerChanges`**: alteraciones producidas por decisiones de jugadores dentro de esa época (una propiedad comprada/destruida, un negocio fundado, daño de un evento de guerra con participación del jugador) — se guarda como delta sobre la capa histórica, nunca reescribiendo `HistoricalLayer` (que es contenido de diseño, compartido por todas las partidas).

Esto responde directamente a "evitar duplicar completamente todos los assets": solo el `HistoricalLayer` cambia entre épocas, y dentro de él solo se modelan las diferencias reales (edificios nuevos, rutas nuevas, cambios de frontera/autoridad), reutilizando `BaseLocation` y el catálogo de props/materiales (§33 del prompt) siempre que sea posible.

### Qué controla cada capa

| Cambio | Capa responsable |
|---|---|
| Edificios que aparecen/desaparecen | `HistoricalLayer.LayoutAssetRef` (composición aditiva de piezas, no reemplazo total de escena cuando el cambio es local) |
| Rutas, ferrocarriles | `HistoricalLayer.InfrastructureTags` + `Route`/`TransportDefinition` (`03_DATA_MODEL.md` §10) |
| Daños de guerra | `DynamicPlayerChanges` si derivan de participación del jugador; `HistoricalLayer` si es un hecho histórico fijo (`HARD`) que ocurre para todas las partidas |
| Autoridades | `AuthoritySystem` vía `JurisdictionDefinition.AuthorityTypeByEra`, no geometría — es un dato consultado, no algo que se "ve" en el mapa necesariamente |
| Comercio | `EconomySystem`/`WorldSimulationSystem`, estado lógico, no geometría |
| Fronteras | `Region` cambia de `ParentRegionId`/metadatos jurisdiccionales por época si corresponde, sin mover geometría |
| Eventos de guerra | Combinación: `HistoricalTimelineSystem` aplica `WorldChanges` (fijo) + `ConsequenceEngine` aplica efectos específicos de la partida (variable) |

## 4. Conocimiento geográfico (`MapKnowledgeState`)

Ya definido como especialización de `InformationSystem` (`03_DATA_MODEL.md` §9). Estados: `UNKNOWN → RUMOR → MAPPED → VISITED → MASTERED`, por familia, transmisible entre generaciones y comerciable entre familias jugadoras.

## 5. Simulación de fondo vs. alta fidelidad (resumen; detalle completo del mecanismo en `WorldSimulationSystem` es parte de Fase 8, no de esta fase)

- **Alta fidelidad**: regiones con jugadores presentes o actividad reciente relevante — NPC con movimiento, rutinas e interacción real.
- **Simulación abstracta**: regiones sin jugadores — se representan por estado agregado (nivel de seguridad, actividad comercial, rumores generados) sin instanciar geometría ni agentes individuales. Al entrar un jugador, el estado abstracto se "traduce" a instancias visibles consistentes con ese estado (ej. si la simulación abstracta determinó actividad criminal alta, aparecen encuentros de riesgo acordes al entrar, no un mundo "reseteado").
- Esta arquitectura de dos niveles es la que hace viable un Perú amplio en dispositivos móviles sin destruir rendimiento: nunca se simulan con IA completa NPC fuera del radio de interacción del jugador.

## 6. Alcance de esta fase

No se construye el mapa completo, ni Junín/Ayacucho, ni todas las ciudades. Se fija el mecanismo (jerarquía Region/Subregion/Location, streaming por Addressables, composición en capas `TemporalWorldState`) para que Fase 6 (prototipo de una zona por bioma) y Fase 12+ (Actos históricos) lo pueblen con datos sin requerir cambios estructurales.
