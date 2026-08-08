# MultiplayerArchitecture.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Naturaleza del multijugador de este proyecto

El requisito central (§3 del prompt maestro) es explícito: **los jugadores no tienen que permanecer juntos**. Pueden encontrarse, viajar por separado y reencontrarse; sus relaciones se conservan entre generaciones. Esto descarta, por diseño, cualquier modelo de "sesión multijugador = partida completa" (como un shooter o un co-op tradicional). El modelo correcto es más cercano a un **mundo persistente compartido con encuentros en tiempo real ocasionales**, es decir, dos capas de red con responsabilidades distintas y ciclos de vida distintos:

1. **Capa persistente (siempre activa, asíncrona)**: familias, patrimonio, reputación, mapa descubierto, consecuencias. Existe independientemente de si algún jugador está conectado.
2. **Capa de sesión en tiempo real (efímera, bajo demanda)**: cuando 2-4 jugadores están en la misma región y momento simulado, necesitan ver el movimiento del otro, comerciar en vivo, combatir, etc.

Esta separación es la razón por la que `Architecture.md` §5 dibuja dos rutas de red distintas desde el cliente.

## 2. Capa persistente — Backend de mundo

- **Responsabilidad**: única fuente de verdad para todo lo marcado "backend" en `GameSystems.md` y `DataModel.md` §"Persistencia vs. derivado". Expone una API (REST/RPC) que el cliente llama tanto conectado como para sincronizar su cola de intenciones (`SaveSystem.md` §6).
- **Ejecuta `WorldSimulationSystem` como proceso continuo** (no atado al ciclo de vida de ninguna sesión de cliente): esto es lo que hace que el mundo "siga vivo" sin jugadores, requisito central de la Fase 8.
- **Candidato tecnológico por defecto** (recomendación, no decisión cerrada — revisar con datos reales en Fase 8-9): **Unity Gaming Services (Cloud Save + Cloud Code + un servicio propio ligero para la simulación de mundo)** como punto de partida, con la opción de migrar a un backend propio (ej. Node.js/TypeScript o Go + PostgreSQL) si la simulación continua de `WorldSimulationSystem` excede lo que Cloud Code puede sostener de forma económica. La razón de proponer UGS primero es reducir superficie de infraestructura propia mientras el juego aún no tiene escala; el modelo de datos (`DataModel.md`) es agnóstico de esta elección precisamente para no bloquear un cambio posterior.
- Este backend **no se construye en Fase 0-7**. Esas fases usan una implementación local-mock del mismo contrato de repositorio (ver `SaveSystem.md` §1), permitiendo prototipos jugables sin bloquear en infraestructura de servidor.

## 3. Capa de sesión en tiempo real

- **Cuándo existe**: solo cuando 2-4 `PlayerAccount` están simultáneamente presentes en la misma región/instancia. Se crea un `GameSession` (`DataModel.md` §11) y se destruye al dispersarse o desconectarse.
- **Tecnología propuesta**: **Netcode for GameObjects + Unity Relay/Lobby**. Relay evita exigir IP pública/hosting dedicado (importante para jugadores móviles domésticos); Lobby resuelve el "encontrarse" (unirse a la sesión de otro jugador, o encontrarlo en el mundo si ambos están en la misma región simulada).
- **Autoridad dentro de la sesión**: modelo cliente-anfitrión autoritativo o servidor dedicado ligero (a decidir con datos de latencia reales, no en Fase 0) para posición, combate e interacción física; las transacciones que afectan estado compartido persistente (comercio, préstamo, transferencia de objetos) igual se confirman contra el backend de la capa persistente, no solo dentro de la sesión — una sesión en tiempo real que termina abruptamente (jugador pierde conexión) no debe dejar una transacción a medias en el mundo persistente.
- El combate (`CombatSystem`) y el diálogo compartido ocurren en esta capa; su resultado final se persiste vía la capa 1.

## 4. Resolución de conflictos y autoridad de datos

Regla general: **el backend de la capa persistente es la única autoridad para cualquier dato que otro jugador pueda observar o que deba sobrevivir a la sesión**. Ningún cliente decide unilateralmente el resultado de una acción compartida.

Ejemplos de conflicto y su resolución:

| Escenario | Resolución |
|---|---|
| Dos familias intentan comprar el último lote de un bien limitado casi simultáneamente, una offline momentos antes | El backend procesa intenciones en orden de llegada confirmada (no en orden de creación local); la segunda se rechaza o se reconcilia (ej. oferta parcial), y el cliente afectado recibe notificación clara, nunca un fallo silencioso |
| Un jugador pierde conexión a mitad de un intercambio de objetos en sesión en tiempo real | La transacción no se considera completa hasta que el backend la confirma; si no se confirma, se revierte para ambas partes |
| Dos jugadores generan decisiones que afectan la misma relación familiar el mismo día | `ConsequenceEngine` es append-only (`SaveSystem.md` §9): ambos eventos se registran con su propio `DecisionEventId`; la relación resultante es la suma/orden cronológico de ambos, no una sobrescritura |

## 5. Anti-griefing narrativo (Regla del prompt maestro §9, sección Multijugador)

El diseño evita griefing irreversible **por mecanismo, no por prohibición**:

- Robo, engaño y conflicto económico están permitidos como gameplay, pero se canalizan a través del `ConsequenceEngine`: un robo genera un `DecisionEvent` con consecuencias (reputación, posible intervención de `AuthoritySystem`, posible venganza generacional), nunca una simple resta de inventario sin rastro.
- No existe eliminación permanente de progreso ajeno sin una vía de resolución narrativa (negociación, restitución, venganza, intervención de autoridad) — esto es responsabilidad conjunta de `ReputationSystem` + `ConsequenceEngine`, ya definidos en `GameSystems.md`.
- Los detalles finos de balance (cuánto se puede robar, límites por sesión, etc.) se definen en Fase 9-10 con pruebas reales; Fase 0 solo fija que el mecanismo de resolución (no de prevención dura) es el principio rector.

## 6. Progresión de complejidad multijugador (alineado con Fase 9 del roadmap)

Se construye y valida incrementalmente, nunca "4 jugadores" de entrada:

1. 2 jugadores en la misma sesión en tiempo real (movimiento, verse mutuamente, comercio básico).
2. 3 jugadores.
3. 4 jugadores — techo de diseño actual.
4. Jugadores en ubicaciones distintas, sin sesión en tiempo real compartida, interactuando solo vía capa persistente (mensajería asíncrona, mercado compartido, reputación).
5. Robo/engaño/conflicto económico entre jugadores.

Cada paso se prueba antes de sumar el siguiente (principio de vertical slice, §16 del prompt maestro).

## 7. Preparación para LEGADO: PARADOJA (Regla 8)

- `GameSession` y todo el estado de la capa persistente llevan `TimelineID`. Una sesión en tiempo real futura de Paradoja simplemente ocurre dentro de un `TimelineID` distinto de `prime`; la capa de red no necesita rediseño, solo la lógica de qué timelines pueden "verse" entre sí (explícitamente fuera de alcance ahora).
- No se implementa nada de Paradoja en esta fase; se deja documentado el punto de extensión para que Fase 9-10 no tome decisiones de red que lo bloqueen (ej. prohibido asumir un único `WorldSimulationSystem` global sin partición por timeline).

## 8. Fuera de alcance de Fase 0

Selección final de proveedor de backend, topología exacta de servidores, costos de infraestructura, y balance fino de anti-griefing. Fase 0 fija el modelo (dos capas, autoridad en backend persistente, mecanismo narrativo sobre prohibición dura) y dos candidatos tecnológicos evaluables con datos reales cuando corresponda (Fase 8-9).
