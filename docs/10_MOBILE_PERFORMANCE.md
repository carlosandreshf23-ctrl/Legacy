# 10_MOBILE_PERFORMANCE.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

Los números de este documento son **objetivos iniciales de diseño**, no mediciones. Se validan y ajustan con dispositivos reales en Fase 1 (primer prototipo jugable) — fijarlos ahora permite tomar decisiones de arte/streaming/red con un presupuesto concreto en mente desde el principio (Regla 6 del proyecto: optimización móvil desde el comienzo), en vez de posponer toda cifra indefinidamente.

## 1. Niveles de dispositivo objetivo

| Tier | Perfil de referencia (2026) | Rol |
|---|---|---|
| **LOW** | Android gama de entrada, ~3-4 GB RAM, SoC de 2-3 años, almacenamiento limitado | Mínimo jugable — define el "piso" de todo presupuesto |
| **MID** | Android/iOS gama media, 4-6 GB RAM | Experiencia objetivo por defecto |
| **HIGH** | Gama alta móvil (8+ GB RAM) y, más adelante, PC | Fidelidad máxima, sin comprometer el resto |

## 2. Presupuestos por tier

| Métrica | LOW | MID | HIGH (móvil) | PC (futuro) |
|---|---|---|---|---|
| FPS objetivo | 30 | 30 (objetivo 45 si el dispositivo lo permite) | 60 | 60+ |
| RAM del juego (footprint total, no del SO) | ~700 MB | ~1.0-1.2 GB | ~1.8-2 GB | 3-4 GB+ |
| Tamaño de descarga inicial | ≤ 1.5 GB (resto vía streaming bajo demanda) | ≤ 1.5 GB | ≤ 1.5 GB | sin restricción dura |
| NPC de alta fidelidad simultáneos visibles | 8-10 | 15-20 | 30-40 | 50+ |
| Draw calls/frame | ≤ 300-400 | ≤ 600 | ≤ 900-1000 | 1500+ |
| Resolución de textura máxima | 1024px (atlas agresivo) | 2048px | 2048-4096px | 4096px+ |
| Distancia de render (mundo exterior) | ~150-200 m + niebla de corte | ~300 m | ~450-500 m | 800 m+ |
| LOD | Cambio agresivo (LOD1 desde ~15 m) | Estándar | Conservador | Mínimo |
| Red (tolerancia) | Debe funcionar con 300-500 ms de latencia y cortes intermitentes | 150-300 ms | < 150 ms | < 100 ms |

## 3. Principios de presupuesto (aplican a todos los tiers)

- **El LOW tier no es "el juego con menos cosas": es el mismo mundo con simulación de fondo más abstracta.** Se reduce la cantidad de NPC de alta fidelidad y distancia de render, no el alcance narrativo — el `WorldSimulationSystem` (`04_WORLD_ARCHITECTURE.md` §5) ya está diseñado para representar NPC lejanos de forma estadística en todos los tiers; en LOW simplemente el radio de "alta fidelidad" es más pequeño.
- **Streaming de región (Addressables) es obligatorio en todos los tiers**, no solo en LOW — la diferencia entre tiers es cuánto se mantiene cargado simultáneamente (regiones vecinas precargadas), no si hay streaming.
- **Presupuesto de memoria por región**: cada `RegionDefinition`/`RegionEraSnapshot` declara su huella estimada (texturas + geometría + audio) como metadato; una herramienta de validación en `tools/` (no en runtime) rechaza integrar una región que exceda el presupuesto del tier LOW antes de que llegue a build — evita descubrir el problema de rendimiento en QA tardío.
- **Red tolerante a intermitencia** (ya establecido en `08_SAVE_SYSTEM.md`/`09_MULTIPLAYER_ARCHITECTURE.md`): ninguna acción local del jugador debe bloquearse esperando al backend; la cola de intenciones asume el peor caso de red del tier LOW como caso normal, no como excepción.
- **Batería**: sincronización con backend en lotes (batching), no polling constante; simulación de `WorldSimulationSystem` ocurre server-side, el cliente nunca hace trabajo de simulación pesado en segundo plano solo por tener la app abierta.

## 4. Fuera de alcance de esta fase

Medición real en dispositivo, ajuste fino de LOD/atlas/compresión de textura por asset concreto, presupuesto de audio detallado. Estos números son el contrato inicial contra el que Fase 1 valida con hardware real; se espera revisión, no se espera que sean exactos desde el día uno.
