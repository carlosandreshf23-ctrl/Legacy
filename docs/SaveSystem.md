# SaveSystem.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

## 1. Premisa de diseño

A diferencia de un RPG single-player clásico, en LEGADO: PERÚ **el "guardado" no puede vivir solo en el dispositivo del jugador**, porque:

- El mundo (economía, seguridad, reputación) es compartido entre hasta 4 familias jugadas por personas distintas, y sigue existiendo aunque un jugador esté desconectado (`WorldSimulationSystem`).
- El progreso debe sobrevivir a la muerte de un personaje (continuidad generacional — `FamilySystem`).
- Las consecuencias de una decisión pueden manifestarse décadas después, potencialmente cuando el jugador cambió de dispositivo.

Por lo tanto, el sistema de guardado se divide en **dos ámbitos con reglas distintas**, no en un único archivo de partida:

| Ámbito | Qué contiene | Dónde vive la fuente de verdad | Ejemplo |
|---|---|---|---|
| **Guardado de mundo (compartido)** | `Family`, `Character`, `Relationship`, `DecisionEvent`, `FamilyPatrimony`, `Asset`, `MapKnowledgeState`, `AuthorityRecord` | Backend (ver `MultiplayerArchitecture.md`) | El árbol genealógico de los Salazar |
| **Guardado local (por dispositivo/jugador)** | Configuración, caché de assets, preferencias, cola de intenciones pendientes de confirmar, progreso puramente de sesión (posición de cámara, tutorial visto) | Cliente (Unity, almacenamiento local) | "Mostrar controles en pantalla: sí/no" |

Esto también resuelve la Fase 1 (prototipo sin backend todavía): el prototipo usa **el mismo contrato de interfaz** (`IPersistable` / `IWorldStateRepository`) contra una implementación local-mock, de forma que migrar a backend real en Fase 8-9 no requiere reescribir los sistemas que ya guardan estado — solo cambiar la implementación del repositorio.

## 2. Interfaz común: `IPersistable`

Todo sistema con estado persistente (Sección "Persistencia vs. derivado" de `DataModel.md`) implementa:

```
interface IPersistable<T> {
    string GetPersistenceKey();
    T CaptureState();
    void RestoreState(T state);
    int SchemaVersion { get; }
}
```

`SaveSystem` no conoce el contenido de cada sistema; solo orquesta: en qué momento se captura/restaura, y contra qué repositorio (local vs backend) se serializa cada clave. Esto respeta el principio de sistemas desacoplados (`Architecture.md` §3.3).

## 3. Formato de serialización

- **Formato de intercambio**: JSON para todo estado de partida (legible, versionable, fácil de depurar y de migrar). No se usa binario propietario para el estado de mundo.
- **Formato de caché local**: JSON comprimido (gzip) o SQLite ligero para catálogos grandes (ej. caché completa del `MapKnowledgeState` de una familia), priorizando lectura rápida en dispositivos móviles de gama media.
- Los **datos de diseño** (definiciones de eventos históricos, métodos de viaje, etc. — ver `DataModel.md`) NO pasan por `SaveSystem`; se cargan desde `Assets/_Project/Data/` y se referencian por ID desde el estado guardado, nunca se duplican dentro del guardado.

## 4. Versionado y migración

- Todo objeto serializado incluye `SchemaVersion`.
- `SaveSystem` mantiene una cadena de migradores (`IStateMigrator`) por tipo y versión: `v1 → v2 → v3 ...`, aplicados secuencialmente al cargar un guardado antiguo. Nunca se edita en el sitio el parser de una versión vieja; se añade un migrador nuevo.
- Dado que el juego se desarrolla en fases a lo largo de meses/años (Fase 0 → Fase 14+), se asume que el esquema cambiará muchas veces; el versionado por entidad (no un único número global de "versión de guardado") permite migrar solo lo que cambió.

## 5. Cuándo se guarda (triggers)

| Trigger | Ámbito afectado | Justificación |
|---|---|---|
| Confirmación de una decisión narrativa relevante | Mundo (vía backend) | Las consecuencias deben registrarse de inmediato en `ConsequenceEngine`; no puede perderse por cierre inesperado de la app |
| Transición entre regiones / streaming de escena | Local (checkpoint) + sync oportunista a backend | Punto natural de bajo riesgo para persistir posición/estado de sesión |
| Cambio de protagonista (muerte de personaje) | Mundo (backend) — es un evento crítico de `FamilySystem` | Debe quedar firme antes de continuar, no puede depender de autosave posterior |
| Periódico (autosave local) | Local | Red intermitente típica de móvil; el autosave local no requiere red |
| Explícito (jugador guarda/sale) | Local + intento de sync a backend | UX estándar |
| Salida de sesión multijugador en tiempo real | Mundo (backend) — obligatorio antes de cerrar sesión | Evita estados de sesión "colgados" que otros jugadores no pueden observar correctamente |

## 6. Cliente móvil con conectividad intermitente

El cliente nunca bloquea al jugador esperando al backend para acciones locales (moverse, explorar, hablar con NPC). El patrón es:

1. La acción se aplica **optimistamente** en el cliente (UI responde de inmediato).
2. La acción se encola como **intención pendiente** (`PendingIntent`) en almacenamiento local.
3. Cuando hay red, `SaveSystem` sincroniza la cola contra el backend en orden, en lotes (batching para batería/datos).
4. El backend es la autoridad final: si una intención entra en conflicto con estado que cambió mientras el cliente estaba offline (ej. otro jugador ya compró el mismo lote de un bien limitado), el backend la resuelve según reglas de `MultiplayerArchitecture.md` §4 y el cliente reconcilia (se notifica al jugador si su intención no se pudo aplicar tal cual).
5. Ninguna intención pendiente se descarta silenciosamente: se resuelve, se reconcilia, o se marca visible como conflicto para el jugador.

## 7. Continuidad generacional

- El guardado de mundo de una familia **no se reinicia** cuando muere el protagonista. `FamilySystem.OnCharacterDied` dispara una transición de `ActiveProtagonistId` dentro del mismo `Family`, no una partida nueva.
- El árbol genealógico completo (vivos y fallecidos) se conserva siempre; nada se purga por "ya no es jugable".
- Saltos temporales generacionales (Fase 13, `1824 → años posteriores`) son una operación explícita del backend: avanza `TimeSystem`, ejecuta ticks acumulados de `WorldSimulationSystem`/`FamilySystem` (envejecimiento, nacimientos, muertes según reglas narrativas, evolución económica), y produce un nuevo estado consistente — no es una simple edición de fecha.

## 8. Guardado en contexto multijugador

- No existe "un archivo de guardado por partida multijugador" al estilo local co-op: cada `Family` tiene su propio estado persistente en el backend, independiente de con quién jugó esa sesión.
- Lo que sí se guarda como entidad de sesión es el `GameSession` (quién coincidió con quién, cuándo, dónde — ver `DataModel.md` §11), útil para reputación/relaciones pero no para "continuar la partida", porque la partida es, en realidad, el estado del mundo.
- Un jugador puede jugar solo, y más tarde otro jugador puede "encontrarse" con las consecuencias de sus decisiones (rumores, reputación, un NPC que menciona a su familia) sin haber compartido sesión en tiempo real — esto es posible precisamente porque el guardado de mundo no está atado a una sesión de juntos-en-vivo.

## 9. Recuperación ante corrupción / pérdida

- Backend: snapshots periódicos + write-ahead log de `DecisionEvent` (son append-only por diseño, nunca se sobrescriben, solo se marcan `HasFired`), lo que permite reconstruir estado desde el log si un snapshot se corrompe.
- Cliente: se mantienen las últimas N copias locales rotadas (no solo la última) antes de sobrescribir, para poder revertir un guardado local corrupto sin perder todo el progreso reciente.
- Checksums en cada blob serializado local; si falla la validación al cargar, se cae a la copia rotada anterior y se notifica al jugador en vez de fallar silenciosamente (Regla de errores del prompt maestro §19).

## 10. Fuera de alcance de Fase 0

No se implementa en esta fase: el backend real, la base de datos concreta, ni el protocolo de red. Fase 0 fija el **contrato** (`IPersistable`, `PendingIntent`, separación mundo/local, versionado). Fase 1 implementa una versión local-only detrás de este contrato (suficiente para el prototipo de movimiento con guardar/cargar). Fase 8-9 implementa el backend real detrás del mismo contrato.
