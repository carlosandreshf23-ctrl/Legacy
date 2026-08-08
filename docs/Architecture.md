# Architecture.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto
**Estado del repositorio al iniciar:** vacío, sin commits, sin proyecto previo.
**Alcance de este documento:** arquitectura técnica base sobre la que se construirán todas las fases posteriores (Fase 1 en adelante). No define contenido de juego, mapas finales ni personajes históricos: define **cómo** se va a construir.

---

## 1. Resumen ejecutivo

LEGADO: PERÚ es un RPG narrativo, multigeneracional, multijugador (1-4), de mundo semiabierto, ambientado en Perú entre 1820-1920, con plataforma principal móvil y expansión posterior a PC. Requiere:

- Simulación persistente de mundo (economía, seguridad, autoridad, rumores) que continúa incluso sin jugadores presentes.
- Continuidad generacional (personajes que nacen, envejecen y mueren; el "protagonista" es la familia, no un personaje).
- Un motor de consecuencias que conecta decisiones de 1820 con eventos de 1880.
- Multijugador asíncrono: los jugadores no necesitan estar juntos ni conectados simultáneamente para que sus familias interactúen.
- Extensibilidad futura hacia LEGADO: PARADOJA (líneas temporales alternativas) sin reescribir la base.

Esto excluye, por diseño, cualquier arquitectura puramente "single-player con multiplayer añadido después" (Regla 7) o cualquier modelo de guardado local-only (el mundo debe sobrevivir a la sesión de un jugador individual).

La conclusión arquitectónica central de esta fase es que **LEGADO: PERÚ no es un juego multijugador en tiempo real con guardado; es un mundo persistente compartido (arquitectura tipo "servicio con estado") al que los clientes se conectan intermitentemente**, similar en filosofía a un MMO ligero o a un juego por turnos asíncrono, con una capa adicional de sesión en tiempo real para cuando 2-4 jugadores coinciden físicamente en la misma región y momento simulado.

---

## 2. Decisión de motor y stack tecnológico

No existe proyecto previo, por lo que esta fase debe fijar el motor. Se evaluaron Unity y Unreal Engine bajo los criterios del proyecto (móvil como plataforma principal, equipo de desarrollo reducido/agente único, necesidad de sistemas altamente data-driven, multijugador asíncrono + sesiones en tiempo real, expansión futura a PC).

**Decisión: Unity (LTS más reciente disponible al iniciar producción real, actualmente serie 2022 LTS/6 LTS), C#, render pipeline URP (Universal Render Pipeline).**

Justificación:

| Criterio | Unity | Unreal | Resultado |
|---|---|---|---|
| Rendimiento y herramientas en móvil (batería, memoria, build size) | Maduro, URP diseñado para esto | Requiere más ajuste, builds más pesados | Unity |
| Iteración rápida con equipo pequeño / agente de IA | C# más accesible para generación/mantenimiento de código, compilación incremental rápida | Blueprints + C++ compilación más lenta | Unity |
| Diseño data-driven (ScriptableObjects) | Soporte nativo de primer nivel para exactamente este patrón | Data Tables/DataAssets equivalentes pero más verboso | Unity |
| Streaming de mundo semiabierto por regiones | Addressables + escenas aditivas | World Partition (potente pero pensado para equipos AAA) | Empate, Unity más simple de operar solo |
| Multijugador | Netcode for GameObjects + Unity Gaming Services (Relay/Lobby/Cloud Save/Cloud Code) cubren tanto sesión en tiempo real como backend de servicio | Requiere backend propio igualmente | Unity (menor fricción inicial) |
| Camino a PC | Export directo, mismo proyecto | Export directo | Empate |
| Coste/licenciamiento a la escala de este proyecto | Aceptable | Aceptable | Empate |

Esta decisión es una **recomendación técnica documentada**, no irreversible: el modelo de datos y la separación de sistemas (Sección 3) se diseñan para que la lógica de juego (C# puro, sin dependencias de motor cuando sea posible) sea portable si en el futuro se justifica un cambio de motor. No se debe optimizar prematuramente para esa portabilidad a costa de velocidad de desarrollo; es una consecuencia del buen diseño en capas, no un objetivo en sí mismo.

### Paquetes/servicios base propuestos
- **Unity 2022 LTS (o LTS vigente), URP.**
- **Input System** (paquete oficial) — necesario para controles móviles + futuro soporte PC/gamepad sin duplicar lógica.
- **Addressables** — streaming de regiones, assets bajo demanda, clave para memoria en móvil.
- **Netcode for GameObjects + Unity Relay/Lobby** — capa de sesión en tiempo real (ver `MultiplayerArchitecture.md`).
- **Cinemachine** — cámara de exploración/combate contextual.
- **ScriptableObjects** como formato nativo de datos de diseño, con import/export a JSON para datos históricos versionables en texto plano (ver `DataModel.md`, Regla 4).

No se instala ni genera un proyecto Unity binario en este repositorio durante la Fase 0 (no hay Editor disponible en este entorno de ejecución y generar archivos `.meta`/`ProjectSettings` a mano sin el Editor real es contraproducente: se crearía un proyecto potencialmente corrupto). Fase 0 entrega la **estructura de carpetas** y la documentación; la creación del proyecto Unity real ocurre al iniciar Fase 1, donde sí hay validación jugable.

---

## 3. Principios de arquitectura

1. **Separación estricta código / datos históricos** (Regla 4). Ningún año, evento, personaje histórico o ubicación se hardcodea en C#. Todo vive en assets de datos (ScriptableObjects respaldados por JSON) cargados en tiempo de ejecución. El código implementa *mecanismos generales* (`ConsequenceEngine`, `HistoricalEventSystem`) que interpretan esos datos.
2. **Sistemas generales, no sistemas de contenido** (Regla 5). Nunca `SistemaBatallaAyacucho`; siempre `CombatEncounterSystem` parametrizado por datos que describen Ayacucho.
3. **Arquitectura en capas con bajo acoplamiento vía interfaces + bus de eventos**, no una jerarquía monolítica de `MonoBehaviour`. Cada sistema de gameplay (Sección de `GameSystems.md`) se implementa como un servicio C# plano (testable sin Editor) más una fachada delgada en `MonoBehaviour` para integrarlo con la escena.
4. **Todo lo persistente lleva `TimelineID`** desde el día uno (por defecto `"prime"`), aunque hoy solo exista una línea temporal. Esto es lo que permite que Paradoja se añada después sin migrar el modelo de datos (Regla 8). Ver `DataModel.md` §2.
5. **Todo lo persistente lleva also `WorldDate` (año/mes/día en el calendario del juego)**, no un timestamp real. El tiempo de juego es una entidad de primera clase (`TimeSystem`) de la que dependen viaje, envejecimiento, simulación de mundo y el motor de consecuencias.
6. **Autoridad de datos en el servidor/backend para todo lo que sea compartido** (familia, patrimonio, reputación, mapa descubierto, consecuencias). El cliente Unity es una vista + una cola de intenciones, nunca la fuente de verdad para datos que otros jugadores pueden observar o que deben persistir entre generaciones. Ver `MultiplayerArchitecture.md`.
7. **Fidelidad histórica como restricción de datos, no de motor**: el sistema no "sabe" qué es Ayacucho; sabe interpretar un `HistoricalEventDefinition` marcado `VERIFIED`, `NEEDS_RESEARCH` o `FICTIONAL` (Regla 3). Esta clasificación es un campo obligatorio del modelo de datos, no una convención de comentarios.
8. **Optimización móvil como restricción transversal desde Fase 1** (Regla 6): streaming por región (Addressables), LOD, presupuestos de memoria/CPU por sistema, red tolerante a desconexión (el juego debe ser jugable con conectividad intermitente, sincronizando cuando hay red).
9. **Ninguna fase elimina sistemas de fases previas para resolver un requisito nuevo** (Regla 9): las extensiones se hacen por composición de datos o nuevas implementaciones de interfaces existentes.

---

## 4. Estructura de carpetas del repositorio

```
Legacy/
├── docs/                          # Documentación de arquitectura y diseño (este documento y hermanos)
│   ├── Architecture.md
│   ├── GameSystems.md
│   ├── DataModel.md
│   ├── SaveSystem.md
│   ├── MultiplayerArchitecture.md
│   └── DevelopmentRoadmap.md
│
├── game-client/                   # Proyecto Unity (se crea en Fase 1 con el Editor real)
│   ├── Assets/
│   │   ├── _Project/
│   │   │   ├── Scripts/
│   │   │   │   ├── Core/          # Bootstrap, ServiceLocator/DI liviano, EventBus, TimeSystem
│   │   │   │   ├── Systems/       # Un subdirectorio por sistema de GameSystems.md
│   │   │   │   │   ├── Family/
│   │   │   │   │   ├── Consequence/
│   │   │   │   │   ├── Patrimony/
│   │   │   │   │   ├── Economy/
│   │   │   │   │   ├── Discovery/
│   │   │   │   │   ├── Travel/
│   │   │   │   │   ├── WorldSimulation/
│   │   │   │   │   ├── Authority/
│   │   │   │   │   ├── Reputation/
│   │   │   │   │   ├── Combat/
│   │   │   │   │   ├── Dialogue/
│   │   │   │   │   ├── HistoricalEvents/
│   │   │   │   │   ├── Save/
│   │   │   │   │   └── Multiplayer/
│   │   │   │   ├── Data/          # Definiciones de ScriptableObject (esquemas, no instancias)
│   │   │   │   ├── Characters/    # Controlador de personaje, cámara, input móvil
│   │   │   │   ├── UI/
│   │   │   │   └── Environment/   # Streaming de regiones, día/noche, clima
│   │   │   ├── Data/               # Instancias de datos (assets .asset / .json)
│   │   │   │   └── Historical/
│   │   │   │       ├── Characters/
│   │   │   │       ├── Events/
│   │   │   │       ├── Locations/
│   │   │   │       └── Eras/
│   │   │   ├── Scenes/
│   │   │   ├── Prefabs/
│   │   │   ├── Art/
│   │   │   └── Audio/
│   │   └── Plugins/
│   ├── Packages/
│   └── ProjectSettings/
│
├── backend/                        # Servicio de mundo persistente (se introduce a partir de Fase 8-9)
│   ├── src/
│   └── README.md
│
└── tools/                          # Validadores de datos, importadores de datos históricos, scripts de CI
    └── README.md
```

Notas:
- `game-client/`, `backend/` y `tools/` se crean como esqueleto con `README.md` explicando su propósito; el contenido funcional se llena en las fases donde corresponde (Fase 1 para `game-client`, Fase 8-9 para `backend`).
- La carpeta `Assets/_Project/Data/Historical` es intencionalmente datos, no código: es donde vive todo lo que Regla 3 obliga a marcar `VERIFIED / NEEDS_RESEARCH / FICTIONAL`.

---

## 5. Arquitectura de alto nivel (capas)

```
┌───────────────────────────────────────────────────────────────────┐
│  CLIENTE (Unity — Android/iOS, luego PC)                          │
│  ┌─────────────┐ ┌───────────────┐ ┌───────────────────────────┐  │
│  │ Presentación │ │ Sistemas de   │ │ Capa de red del cliente   │  │
│  │ (UI, cámara, │ │ gameplay      │ │ (Netcode for GameObjects  │  │
│  │  input móvil,│ │ (ver          │ │  + Relay para sesión en   │  │
│  │  render)     │ │ GameSystems.md)│ │  tiempo real; cliente REST │  │
│  │              │ │               │ │  para backend persistente)│  │
│  └─────────────┘ └───────────────┘ └───────────────────────────┘  │
│         Cache local (SQLite/JSON cifrado) + cola de intenciones    │
└───────────────────────────────────────────────────────────────────┘
                │ tiempo real (cuando hay      │ asíncrono (siempre)
                │ 2-4 jugadores co-presentes)  │
                ▼                              ▼
┌───────────────────────────┐      ┌──────────────────────────────────┐
│ CAPA DE SESIÓN EN TIEMPO   │      │ BACKEND DE MUNDO PERSISTENTE      │
│ REAL (Relay/Lobby)         │      │ - Fuente de verdad: familias,     │
│ - Posiciones, combate,     │      │   patrimonio, reputación, mapa    │
│   interacción física       │      │   descubierto, consequence log,   │
│   compartida                │      │   estado de simulación regional   │
│                             │◄────►│ - WorldSimulationSystem corre     │
│                             │      │   server-side (tick continuo,     │
│                             │      │   con o sin jugadores presentes)  │
└───────────────────────────┘      └──────────────────────────────────┘
```

El cliente nunca es dueño del estado compartido; es dueño de la presentación y de una cola de "intenciones" (acciones que el jugador quiere realizar) que se confirman contra el backend. Esto es lo que permite que dos familias jugadas por personas distintas, conectadas en momentos distintos, afecten la misma economía y el mismo motor de consecuencias sin condiciones de carrera de diseño.

Detalle de la capa de red y del backend en `MultiplayerArchitecture.md`; detalle de qué guarda cada capa en `SaveSystem.md`.

---

## 6. Streaming de mundo y rendimiento móvil

- El mapa (Costa/Sierra/Selva, `docs` §6 del prompt maestro) se divide en **regiones** cargadas/descargadas mediante Addressables + escenas aditivas, nunca como una única escena monolítica.
- Cada región lleva un **presupuesto de memoria y draw calls** definido en su asset de datos (`RegionDefinition`), validado por una herramienta en `tools/` antes de integrarse (no en runtime).
- LOD obligatorio para geometría y NPC fuera del radio de interacción; NPC lejanos se simulan de forma abstracta vía `WorldSimulationSystem` (estadística, no instancias con IA completa) — ver `GameSystems.md`.
- Red tolerante a intermitencia: toda escritura al backend pasa por la cola de intenciones local; si no hay red, el juego sigue siendo jugable localmente (exploración, decisiones locales) y sincroniza al reconectar. Se prioriza UX móvil sobre requerir conexión constante.
- Estos presupuestos concretos (MB de textura, número de NPC simultáneos, etc.) se definen con cifras reales en Fase 1, cuando exista un dispositivo de referencia contra el cual medir; en Fase 0 solo se fija el *mecanismo* (Addressables + LOD + simulación abstracta), no los números.

---

## 7. Extensibilidad hacia LEGADO: PARADOJA (Regla 8)

Sin desarrollar Paradoja, la arquitectura de Fase 0 ya deja los siguientes puntos de extensión:

- **`TimelineID`** en toda entidad persistente (personaje, familia, decisión, patrimonio, estado de región). Hoy siempre `"prime"`. Paradoja introducirá timelines adicionales sin requerir migración de esquema.
- **`ConsequenceEngine`** ya modela causa → memoria → consecuencia como datos versionados por fecha; Paradoja necesita exactamente esto para propagar alteraciones históricas, solo añade una dimensión de timeline a la consulta.
- **`HistoricalEventSystem`** separa "qué ocurrió" (dato) de "cómo se resuelve" (sistema). Paradoja necesita poder marcar eventos como alterables; el campo de clasificación (`VERIFIED/NEEDS_RESEARCH/FICTIONAL`) se extiende naturalmente con un cuarto estado futuro (`ALTERABLE_IN_PARADOX`) sin romper el modelo.
- **`WorldSimulationSystem`** corre server-side por región/tick; soportar múltiples timelines es, arquitectónicamente, correr la misma simulación parametrizada por `TimelineID` adicional — no un sistema nuevo.
- No se implementa nada de esto ahora. Se documenta para que ningún sistema de Fase 1-14 tome una decisión que lo bloquee (ej.: prohibido usar IDs de personaje que no sean estables/globales; prohibido asumir una única fecha "actual" global sin pasar por `TimeSystem`).

---

## 8. Riesgos técnicos principales identificados en Fase 0

| Riesgo | Impacto | Mitigación propuesta |
|---|---|---|
| Backend de mundo persistente es un componente nuevo grande, no trivial para un equipo pequeño | Alto — bloquea multiplayer real | Se pospone su construcción hasta Fase 8-9 (`WorldSimulationSystem`, `Multiplayer`); Fases 1-7 usan un backend simulado/local (mock) con la misma interfaz, para no bloquear progreso jugable temprano |
| Simulación de mundo continua ("vivo" sin jugadores) puede ser costosa de escalar | Medio | Simulación estadística/abstracta por región en vez de simulación de agentes individuales a escala completa; solo se "densifica" la simulación cerca de jugadores activos |
| Rendimiento móvil en mundo semiabierto 3D es exigente | Alto | Addressables + LOD + presupuestos por región desde el diseño, validados en Fase 1 con dispositivo real |
| Alcance narrativo/histórico muy amplio (100 años, 7 actos) sin research previo | Alto si se hardcodea | Regla 3 y 4: todo dato histórico pasa por clasificación y por asset de datos, revisable independientemente del código |
| Complejidad de mantener 1-4 jugadores asíncronos sin condiciones de carrera de diseño (ej. dos familias comprando el mismo bien) | Medio | Backend autoritativo único; cliente nunca resuelve conflictos, solo los backend (ver `MultiplayerArchitecture.md` §4) |
| Motor Unity elegido sin proyecto de referencia previo del equipo | Bajo | Fase 1 es explícitamente un prototipo de validación técnica antes de comprometer más sistemas |

---

## 9. Decisiones explícitamente pospuestas

- Elección final de proveedor de backend (Unity Gaming Services vs backend propio) — se decide con datos reales en Fase 8-9.
- Números concretos de presupuesto de rendimiento (MB, draw calls, NPC simultáneos) — Fase 1.
- Formato final de arte y pipeline de asset — no es parte de arquitectura de sistemas, se define cuando exista dirección de arte.
- Estructura interna del `backend/` (lenguaje, DB) — se propone una opción por defecto en `MultiplayerArchitecture.md` pero no se implementa aún.

---

## 10. Cómo verificar esta fase

Este documento, junto con `GameSystems.md`, `DataModel.md`, `SaveSystem.md`, `MultiplayerArchitecture.md` y `DevelopmentRoadmap.md`, debe permitir a cualquier desarrollador nuevo responder sin ambigüedad:

- ¿En qué motor se construye y por qué? → §2
- ¿Dónde vive cada sistema de juego y de qué depende? → `GameSystems.md`
- ¿Qué campos tiene un personaje/familia/decisión? → `DataModel.md`
- ¿Qué se guarda, dónde, y cómo sobrevive a generaciones y a multiplayer? → `SaveSystem.md`
- ¿Cómo interactúan 2-4 jugadores que no están juntos? → `MultiplayerArchitecture.md`
- ¿Qué se construye en qué orden y cuándo se considera "hecho"? → `DevelopmentRoadmap.md`

No se ha escrito código de gameplay en esta fase (por diseño). Fase 1 es la primera fase con un entregable jugable.
