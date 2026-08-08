# 05_TIME_SYSTEM.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. `GameCalendarSystem`

Fuente única de verdad sobre el tiempo de juego. Ningún otro sistema mantiene su propio reloj.

**Modelo de datos**: `WorldDate { Year, Month, Day, Hour }`. Se usa un calendario propio de juego (no `DateTime` del sistema real) para poder representar fechas históricas de 1820-1920 sin ambigüedad de calendario real/simulado, y para permitir avanzar el tiempo de juego de forma discontinua (saltos, aceleración, viajes) sin relación con el reloj del dispositivo.

**Responsabilidades**:
- Mantener `CurrentWorldDate` (por campaña/`CampaignID`, no global al servidor — cada campaña tiene su propio calendario, ver `09_MULTIPLAYER_ARCHITECTURE.md`).
- `AdvanceTime(duration)` — la única forma válida de mover el reloj; emite eventos `OnDateChanged`, `OnTimeOfDayChanged` consumidos por quien lo necesite (envejecimiento, ciclo día/noche, disparo de eventos programados).
- `ScheduleEvent(WorldDate targetDate, EventHandle)` — agenda callbacks para una fecha exacta ("¿qué ocurre el 6 de agosto de 1824?").
- `ScheduleRelative(WorldDate originDate, Duration offset, EventHandle)` — agenda callbacks relativos ("activa esto 14 años después de esta decisión"). Internamente se resuelve a una fecha absoluta en el momento de agendar (`originDate + offset`), no se recalcula en cada tick — evita recomputar miles de offsets relativos cada vez que avanza el calendario.
- Los eventos agendados (de `ConsequenceEngine`, `HistoricalTimelineSystem`, misiones con `Deadline`) se indexan por fecha en una estructura ordenada (cola de prioridad por `WorldDate`); en cada `AdvanceTime` solo se evalúan los eventos cuya fecha cae dentro del rango avanzado, nunca se escanea la lista completa — necesario para que avanzar décadas de golpe (Fase 13, saltos generacionales) siga siendo barato.

## 2. Fuentes de avance de tiempo

| Fuente | Mecanismo |
|---|---|
| Viaje | `TravelSystem` calcula `TravelTime` y llama `AdvanceTime` al confirmar/completar el viaje (`03_DATA_MODEL.md` §10) |
| Descanso/espera explícita | Acción directa del jugador, duración elegida dentro de límites narrativos |
| Cinemáticas/eventos con duración narrativa | El evento declara su propia duración en datos (`EventDefinition.NarrativeDuration`), aplicada igual que un viaje |
| Saltos generacionales | Operación especial (`GameCalendarSystem.AdvanceGenerational`), no un `AdvanceTime` simple — dispara ticks acumulados de `FamilySystem`/`WorldSimulationSystem`/`EconomySystem` en vez de un solo salto ciego (ver Fase 13 en `12_DEVELOPMENT_ROADMAP.md`) |
| Simulación de fondo (mundo vivo) | `WorldSimulationSystem` no avanza el calendario; reacciona a los ticks que el calendario ya emitió (dirección de dependencia: Calendar → Simulation, nunca al revés, ver `02_SYSTEMS_MAP.md`) |

## 3. Sincronía con `HistoricalTimelineSystem`

`HistoricalTimelineSystem` se suscribe a `OnDateChanged`. Cuando `CurrentWorldDate` alcanza el `Date`/`DateRange` de un `HistoricalEventDefinition`:

1. Si `Modifiability = HARD`: aplica automáticamente `WorldChanges` (lista de `EffectDefinitionId`, el mismo tipo de efecto que usa `ConsequenceEngine` — reutilización directa, no un segundo mecanismo de efectos) sobre `WorldState`/`AuthoritySystem`/`EconomySystem` según corresponda. El resultado macro no es opcional ni condicional a decisiones del jugador.
2. Si `Modifiability = SOFT`: se activa el contexto (el evento "existe" para propósitos de ambientación, diálogo, disponibilidad de misiones) pero no fuerza `WorldChanges` obligatorios sobre el estado de ninguna familia — la participación es narrativa y depende del jugador vía `NarrativeHooks`.
3. Si `Modifiability = FICTIONAL`: no interactúa con `HistoricalTimelineSystem` en absoluto; es contenido narrativo puro gestionado por `QuestGraph`/`ConsequenceEngine`.

Esto responde directamente a la pregunta de sincronización planteada: `GameCalendarSystem` es el reloj; `HistoricalTimelineSystem` es un oyente de ese reloj que aplica datos, nunca un reloj paralelo.

## 4. El problema crítico: tiempo en multijugador

Planteamiento del problema (explícito en el encargo): si el Jugador A viaja 8 días y el Jugador B permanece en una ciudad, A no puede esperar 8 días reales, pero tampoco puede el mundo compartido "no significar nada" mientras eso ocurre.

### Solución propuesta: Calendario Compartido con Techo + Offset Personal Acotado

Se definen dos conceptos distintos, ambos dentro de una misma `CampaignID` (ver `09_MULTIPLAYER_ARCHITECTURE.md` §1):

- **`SharedWorldDate`**: la fecha canónica de la campaña. Es la referencia para eventos históricos (`HARD`), disparo de Actos, y cualquier momento que requiera a todo el grupo activo en una ventana de fechas comparable.
- **`PersonalTimeOffset` por familia**: cuánto se ha adelantado esa familia respecto a `SharedWorldDate` debido a actividades en solitario (viaje, espera). Acotado por un límite configurable (propuesta MVP: máx. ~90 días de juego de adelanto, ajustable por diseño/balance en Fase 9).

**Mecanismo**:
1. Un viaje de 8 días del Jugador A se resuelve como una **`TravelInstance`** — una simulación local de ese personaje y las regiones que atraviesa, reutilizando exactamente el mismo mecanismo de simulación abstracta que ya existe para cuando ningún jugador está presente en una región (`04_WORLD_ARCHITECTURE.md` §5, Regla de sistemas reutilizables). No es un sistema nuevo: es `WorldSimulationSystem` aplicado al propio recorrido del jugador.
2. Mientras A viaja, B sigue jugando en tiempo real en `SharedWorldDate` sin bloquearse. El `PersonalTimeOffset` de A crece hasta 8 días.
3. Si el offset de A se acercara al límite acotado, el sistema deja de permitir que A siga adelantándose unilateralmente: A puede seguir jugando actividades que no requieran avanzar más su calendario personal (interacción local en su destino, comercio, diálogo) hasta que el resto del grupo "lo alcance" con su propia actividad natural.
4. `SharedWorldDate` mismo **no** lo mueve ningún jugador individual: lo mueve `WorldSimulationSystem`/`GameCalendarSystem` a partir de la actividad agregada del grupo activo de la campaña (mecanismo ya definido para el mundo vivo, reutilizado aquí también). Esto es lo que impide que un solo jugador arrastre el calendario compartido sin el resto.
5. Los disparadores de eventos históricos `HARD` y de Actos siempre se evalúan contra `SharedWorldDate`, nunca contra el offset personal de un jugador individual — garantiza coherencia narrativa del grupo aunque los personajes estén temporalmente desincronizados en detalle.
6. Saltos generacionales grandes (años, no días) son una operación de campaña, no de un jugador — ver `09_MULTIPLAYER_ARCHITECTURE.md` §5 para las reglas de consenso.

Esta solución evita tanto el extremo "todos esperan en tiempo real" como el extremo "cada jugador vive en su propia línea de tiempo inconexa": el mundo compartido avanza de forma agregada y acotada, y las divergencias cortas (días) se resuelven con la misma maquinaria de simulación abstracta que el proyecto ya necesita para el mundo vivo sin jugadores.

## 5. Envejecimiento

`FamilySystem` se suscribe a `OnDateChanged` (o específicamente a un tick anual derivado de él) para recalcular `CharacterInstance.Age` y evaluar transiciones de vida (posibilidad de tener hijos, deterioro de salud por edad, etc. — reglas concretas se diseñan en Fase 3, aquí solo se fija que el mecanismo de disparo es el calendario, no un contador independiente por personaje).

## 6. Fuera de alcance de esta fase

Los números concretos del límite de `PersonalTimeOffset`, la velocidad de compresión de tiempo dentro de una `TravelInstance`, y el balance fino de cuánta actividad de grupo se requiere para mover `SharedWorldDate` se ajustan con datos reales de playtesting en Fase 9. Esta fase fija el mecanismo, no los números.
