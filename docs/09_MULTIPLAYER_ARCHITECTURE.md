# 09_MULTIPLAYER_ARCHITECTURE.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Network Authority Model — decisión

Se evalúan las tres opciones pedidas explícitamente:

| Modelo | Costos | Trampas | Guardado | Host migration | Desconexiones/offline | Sincronización temporal |
|---|---|---|---|---|---|---|
| **Host-authoritative puro** | Bajo (sin servidores propios) | Alto riesgo — el host puede manipular su propio cliente al ser autoridad | El progreso "vive" en el dispositivo del host: inaceptable para una narrativa que debe sobrevivir a que el host se desinstale el juego | Requiere renegociar autoridad completa si el host se va — complejo y con ventana de inconsistencia | Si el host se desconecta, la sesión completa muere | No resuelve por sí mismo el problema de A viaja/B se queda (§28) |
| **Servidor dedicado puro** | Alto (infraestructura corriendo 24/7 incluso sin jugadores) | Bajo — autoridad centralizada real | Ideal en teoría, pero sobredimensionado para el volumen de sesiones en tiempo real reales (2-4 jugadores, encuentros ocasionales) | No aplica (no hay host) | Requiere alta disponibilidad constante | Resuelve bien pero a un costo de operación no justificado para la capa efímera |
| **Híbrido (elegido)** | Medio — servidor dedicado ligero solo para el estado persistente/compartido; sesión en tiempo real usa host-autoritativo con Relay | Mitigado — nada que un jugador pueda "hacer trampa" en su sesión en tiempo real (posición, combate) tiene efecto permanente hasta ser confirmado por el backend persistente, que es autoridad real | El guardado real vive siempre en el backend (nunca en el dispositivo de ningún jugador) | Solo afecta a la sesión en tiempo real efímera; el mundo persistente no se ve afectado por absoluto | El mundo sigue existiendo y simulándose aunque todos los clientes estén offline (`WorldSimulationSystem`, backend) | Es la base que permite la solución de `05_TIME_SYSTEM.md` §4 |

**Decisión: modelo híbrido — servidor/backend autoritativo para todo el estado persistente y compartido (patrimonio, reputación, decisiones, mapa, calendario), combinado con host-autoritativo + Relay para la sesión en tiempo real efímera cuando 2-4 jugadores coinciden.** Ya estaba implícito en la iteración anterior de esta fase; aquí se fija como respuesta directa y única (no "depende") a la pregunta planteada.

Justificación adicional específica para este proyecto: un servidor dedicado puro sería coherente si el juego fuera una sesión continua tipo MMO con jugadores simultáneos constantes; no lo es — el diseño central es asíncrono (jugadores que no tienen que estar juntos), así que pagar por infraestructura de tiempo real 24/7 no está justificado. Un host-autoritativo puro sería coherente para una sesión co-op efímera sin persistencia real; no lo es tampoco — el requisito de continuidad generacional de 100 años exige que el progreso nunca dependa de que un dispositivo concreto siga existiendo.

## 2. Dos capas (recordatorio operativo, detalle en `01_ARCHITECTURE.md` §3 y versión previa de esta fase)

1. **Backend de mundo persistente** — autoridad única para `Family`, `CharacterInstance`, `Relationship`, `DecisionRecord`/log de consecuencias, `FamilyPatrimony`/`Asset`, `MapKnowledgeState`/`InformationRecord`, `AuthorityRecord`, `PlayerAccount`, `SharedWorldDate`. Ejecuta `WorldSimulationSystem` como proceso continuo, independiente de si hay clientes conectados.
2. **Sesión en tiempo real** — Netcode for GameObjects + Unity Relay/Lobby, existe solo mientras 2-4 jugadores están co-presentes en la misma región/momento simulado; termina al dispersarse. Cualquier resultado con efecto permanente (transacción, combate, robo) se confirma contra la capa 1 antes de considerarse definitivo — una sesión que muere abruptamente no deja transacciones a medias en el mundo persistente.

## 3. Jugadores en zonas distintas simultáneamente

Escenario explícito: P1 en Lima, P2 en Ayacucho, P3 en Callao, P4 en Arequipa, sin requerir estar cargados en la misma escena.

- Cada cliente carga únicamente las regiones (`04_WORLD_ARCHITECTURE.md` §2) relevantes a su propia posición vía Addressables — no existe una "escena compartida" única que los cuatro deban compartir.
- No se crea `GameSession`/sesión en tiempo real entre ellos mientras no coincidan en región — no hay tráfico de Netcode entre P1 y P2 en este escenario, solo llamadas asíncronas de cada cliente contra el backend (consultar estado, confirmar acciones).
- La comunicación entre ellos, si la hay, es exclusivamente vía el backend de mundo persistente: mensajería asíncrona, mercado compartido, reputación, efectos de `ConsequenceEngine` que uno provoca y otro puede llegar a observar más tarde (por ejemplo, un rumor que se propaga vía `InformationSystem`, `03_DATA_MODEL.md` §9). No requiere que ambos estén conectados al mismo tiempo.
- `SharedWorldDate` de la campaña sigue existiendo y avanzando (impulsada por la actividad agregada del grupo, `05_TIME_SYSTEM.md` §4), independientemente de que ninguno de los cuatro esté nunca en la misma escena.
- Si dos de ellos viajan hacia la misma región y coinciden en el tiempo, en ese momento (y solo en ese momento) se instancia una `GameSession` en tiempo real entre ellos.

## 4. Resolución de conflictos (sin cambios de fondo respecto a la iteración previa, resumen)

El backend procesa intenciones en orden de llegada confirmada; conflictos (ej. dos familias comprando el último lote de un bien) se resuelven en el backend, nunca en el cliente; transacciones de sesión en tiempo real no confirmadas por el backend se revierten para ambas partes; decisiones simultáneas sobre la misma relación se registran como eventos independientes append-only, nunca se sobrescriben.

## 5. Propiedad de la campaña — regla de diseño

Pregunta explícita: ¿a quién pertenece la campaña — host, grupo o servidor?

**Decisión: la campaña es una entidad propia (`CampaignID`), propiedad del grupo, con autoridad de datos en el servidor/backend.** No pertenece a ningún jugador individual como "host" con poder unilateral — esto evita que un solo jugador controle, congele o descontinúe la partida de los demás. El backend es donde vive el estado (igual que el resto del mundo persistente); las decisiones administrativas del grupo (invitar/expulsar, aprobar avances grandes de tiempo) se resuelven por reglas de consenso configurables del grupo, no por privilegio de host.

### Evitar que un jugador "corra" diez años sin los demás

Regla de diseño (mecanismo ya sentado por `05_TIME_SYSTEM.md` §4, aplicado aquí a la escala de campaña):

- **Actividades menores** (viajes cortos, decisiones locales, comercio) generan `PersonalTimeOffset` acotado por familia, sin requerir aprobación de nadie — esto es lo que permite jugar en solitario sin bloquear a los demás.
- **Avances grandes de tiempo** (saltos generacionales, Fase 13 — años, no días) son una operación de `CampaignID`, no de una familia individual: requieren que `SharedWorldDate` avance, y `SharedWorldDate` solo lo mueve la actividad agregada del grupo o un consenso explícito de "avance de campaña" entre los miembros activos.
- Si el resto del grupo está inactivo por un periodo prolongado (offline real, no solo desincronizado en `PersonalTimeOffset`), se permite que la familia activa continúe con actividades menores (no se bloquea a un jugador solitario indefinidamente porque los demás no juegan), pero el salto generacional grande sigue reservado a una operación de campaña explícita, nunca implícita por inactividad ajena — evita que la ausencia de otros jugadores se traduzca accidentalmente en una década de historia compartida decidida unilateralmente.

Esta es la regla propuesta para esta fase; el prompt explícitamente permite diseñar una mejor que la sugerida como ejemplo — esta versión (offset acotado + avance grande como operación de campaña con consenso) es la elegida, por ser la que ya se deriva naturalmente del mecanismo de `05_TIME_SYSTEM.md` sin introducir un sistema adicional.

## 6. Anti-griefing narrativo (sin cambios de fondo)

Robo/engaño/conflicto económico se canalizan por `ConsequenceEngine` (generan `DecisionRecord` con consecuencias, nunca una simple resta de inventario sin rastro); no existe eliminación permanente de progreso ajeno sin vía de resolución narrativa. Balance fino en Fase 9.

## 7. Progresión de complejidad (sin cambios): 2 → 3 → 4 jugadores, luego zonas distintas sin sesión compartida, luego robo/engaño/conflicto económico.

## 8. Preparación para Paradoja

`CampaignID`, `SharedWorldDate` y todo el estado de la capa persistente llevan `TimelineID`. Una futura sesión de Paradoja ocurre en un `TimelineID` distinto de `prime` sin rediseño de la capa de red — solo queda pendiente (deliberadamente, fuera de esta fase) la lógica de qué timelines pueden observarse entre sí.

## 9. Fuera de alcance de esta fase

Proveedor final de backend, topología exacta de servidores, costos de infraestructura, valores numéricos exactos de `PersonalTimeOffset` y reglas de consenso de campaña. Ver `13_TECHNICAL_RISKS.md` para los riesgos asociados a estas decisiones pendientes.
