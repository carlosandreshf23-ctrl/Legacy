# DevelopmentRoadmap.md — LEGADO: PERÚ

**Fase:** 0 — Arquitectura del Proyecto

Este documento es el roadmap técnico de referencia del proyecto, derivado directamente del prompt maestro. Se sigue estrictamente el principio de **vertical slices** (§16): cada fase produce algo jugable/verificable antes de ampliar alcance. No se avanza de fase sin instrucción explícita ("CONTINUAR FASE N"). El protocolo de trabajo por fase (objetivo, sistemas afectados, archivos, riesgos, criterios de aceptación al iniciar; implementado, pruebas, deuda técnica, próxima fase al cerrar) es el definido en §18 del prompt maestro y se aplica a partir de Fase 1.

Estado general al cierre de este documento: **Fase 0 en curso (este conjunto de documentos es su entregable). Fases 1 en adelante no iniciadas.**

---

## Fase 0 — Arquitectura del Proyecto
**Objetivo**: base técnica documentada sobre la que puede existir LEGADO: PERÚ, sin construir todavía mapa, personajes ni Paradoja.
**Entregables**: `Architecture.md`, `GameSystems.md`, `DataModel.md`, `SaveSystem.md`, `MultiplayerArchitecture.md`, `DevelopmentRoadmap.md` (este documento), esqueleto de carpetas (`game-client/`, `backend/`, `tools/`).
**Definition of Done**: la arquitectura permite razonar sobre años, personajes, familias, decisiones, consecuencias, mapas, regiones, reputación, patrimonio y multiplayer sin ambigüedad de diseño.
**Estado**: ▶ en curso.

## Fase 1 — Prototipo de movimiento
**Objetivo**: validar si es agradable controlar el personaje. Zona de prueba pequeña (concepto Paracas/Pisco), sin arte final.
**Sistemas afectados**: `Core` (bootstrap, EventBus), Character & Camera Controller, input móvil, `SaveSystem` (versión local-only), `DialogueSystem`/NPC básico, inventario básico, ciclo día/noche básico.
**Definition of Done**: entrar, moverse, interactuar, guardar, cerrar, volver, continuar.
**Nota**: aquí se crea el proyecto Unity real (`game-client/`) con el Editor, no antes.

## Fase 2 — Vertical Slice: Paracas 1820
**Objetivo**: demostrar que es divertido simplemente vivir en el Perú de 1820. Paracas/Pisco, septiembre de 1820, 30-60 min de contenido.
**Contenido mínimo**: Mateo Salazar, casa, familia, pueblo, costa, NPC, comercio, caballo, inventario, primeras rutas, rumores, primera información sobre el desembarco, autoridades correctas para el contexto, una ruta con riesgo, decisiones pequeñas con consecuencia inmediata y al menos una con consecuencia diferida almacenada.
**Contenido**: 1 misión principal, 3 secundarias, 1 decisión con consecuencia inmediata, 1 decisión con consecuencia futura.
**No incluye**: progreso hasta Junín.

## Fase 3 — Sistema Familiar
**Objetivo**: `FamilySystem` funcional: parentesco, personaje activo, edad, nacimiento, descendencia, muerte, herencia, cambio de protagonista. Interfaz de árbol familiar. Simulación interna de varias décadas como prueba.
**Definition of Done**: Personaje A envejece → muere → herencia → Personaje B se vuelve controlable sin destruir el estado del mundo.

## Fase 4 — Consequence Engine
**Objetivo**: `ConsequenceEngine` funcional con el esquema `DecisionID/CharacterID/FamilyID/Date/Region/Target/Consequences/Visibility/TriggerConditions` (`DataModel.md` §4). Consecuencias inmediatas, diferidas, generacionales, regionales y familiares.
**Mínimo**: 5 pruebas demostrables, incluyendo el caso de referencia (ayuda en 1820 → descendiente lo recuerda en 1840).

## Fase 5 — Patrimonio y Economía
**Objetivo**: `PatrimonySystem` + `EconomySystem`: dinero, inventario, precios, comercio, propiedad, negocios, deuda, patrimonio familiar, herencia, variación regional, oferta/demanda básica.
**Demostración requerida**: ruta insegura → menos comercio → precio aumenta.

## Fase 6 — Mapa y Exploración
**Objetivo**: arquitectura geográfica Costa/Sierra/Selva con un prototipo de una zona por bioma (no el mapa completo). `DiscoverySystem` con los 5 estados de conocimiento y conocimiento compartible entre familias.

## Fase 7 — Viajes
**Objetivo**: `TravelSystem` completo: distancia, terreno, transporte, clima, seguridad, tiempo, carga, costo. Los tres modos (`TravelComplete`, `TravelSummary`, `HistoricalFastTravel`). Probado con caballo, mula, caravana, embarcación. Extensible a ferrocarril/vapor/vehículos futuros.

## Fase 8 — Mundo Vivo
**Objetivo**: `WorldSimulationSystem`: actividad NPC, comercio, criminalidad, autoridad, rumores, seguridad, rutas, evolucionando sin intervención del jugador.
**Demostración requerida**: ruta A segura → aparece criminalidad → bajan comerciantes → suben precios → reacciona autoridad → la situación evoluciona, todo sin acción directa del jugador.
**Nota de infraestructura**: aquí se introduce por primera vez el backend real (`backend/`), ver `MultiplayerArchitecture.md` §2.

## Fase 9 — Multijugador
**Objetivo**: 2 → 3 → 4 jugadores, cada uno con su propia familia. Progresión de pruebas: ubicaciones distintas, encuentro, separación, comercio, transferencia de objetos, préstamo, reputación, decisiones compartidas, guardado multiplayer; luego robo, engaño, conflicto económico, ayuda. Evitar griefing irreversible sin mecanismo narrativo (`MultiplayerArchitecture.md` §5).
**Definition of Done**: 4 jugadores estables.

## Fase 10 — Cuatro Familias
**Objetivo**: Salazar, Quispe, Arrieta, Mendoza completas: introducción individual, ubicación inicial, contactos, habilidades contextuales (nunca objetivamente mejores — `DataModel.md` §2), reputación, historia inicial, y mecanismos de convergencia narrativa entre ellas.

## Fase 11 — Combate
**Objetivo**: `CombatSystem` contextual, no shooter arcade: melee, armas de época, resistencia, heridas, retirada, protección de compañeros, combate montado donde corresponda. El jugador no es superhumano. Probado con una escaramuza ficticia pequeña.

## Fase 12 — Acto I completo (1820-1824)
**Objetivo**: primera expansión histórica real: 1820-1824, con Independencia, Junín y Ayacucho como eventos centrales (fijos, `HistoricalStatus`/`OutcomeIsFixed` — `DataModel.md` §9), pero sin reducir el acto a "batalla → batalla → batalla": incluye familia, trabajo, viajes, relaciones, economía, secundarias, exploración.
**Cierre de fase**: QA completo del acto.

## Fase 13 — Generational Time Skip
**Objetivo**: validar técnicamente la transición 1824 → años posteriores: envejecimiento, nuevos hijos, cambios urbanos, negocios, muertes, relaciones, evolución del mapa (`RegionEraSnapshot` — `DataModel.md` §5). Solo se avanza a la Fase 14 tras validar esto.

## Fase 14+ — Expansión histórica progresiva
Un acto a la vez, con QA de cierre entre cada uno, nunca dos actos en desarrollo simultáneo sin necesidad técnica justificada:
- **Acto II** — 1825-1839: Nueva República, conflictos políticos e internacionales, Confederación. QA.
- **Acto III** — 1840-1866: prosperidad, economía, guerras civiles, guerra con España. QA.
- **Acto IV** — 1867-1878: periodo previo a la Guerra del Pacífico. QA.
- **Acto V** — 1879-1883: Guerra del Pacífico. QA.
- **Acto VI** — 1884-1895: Reconstrucción y guerra civil. QA.
- **Acto VII** — 1896-1920: modernización y nueva generación. QA.

## Fase Final del juego base — "Tu Legado"
**Objetivo**: generación automática de resumen de partida: árbol familiar, cronología, riqueza, propiedades, muertes, nacimientos, amigos, enemigos, relaciones entre jugadores, secretos, cartas, lugares descubiertos, acciones importantes. En multijugador, "Historia Compartida": narrativa de los principales acontecimientos creados por los jugadores.

## Solo después — LEGADO: PARADOJA
**No inicia hasta que LEGADO: PERÚ funcione completo.** Nueva rama de diseño: John Titor, Cronoscopio, viajes temporales, líneas temporales, paradojas, alteración histórica, tecnología entre épocas, nodos temporales hasta 2026, multiplayer temporal. Se apoya en los puntos de extensión ya dejados en `TimelineID`, `ConsequenceEngine` y la clasificación `HistoricalStatus` (ver `Architecture.md` §7 y `MultiplayerArchitecture.md` §7), pero no se diseña en detalle todavía.

---

## Reglas de progresión (aplican a todas las fases desde la 1)

1. No se avanza automáticamente de fase; se espera instrucción explícita "CONTINUAR FASE N" o correcciones.
2. Una fase debe funcionar (criterios de aceptación cumplidos) antes de ampliar alcance.
3. Ningún hecho histórico se inventa; todo dato histórico se clasifica `VERIFIED / NEEDS_RESEARCH / FICTIONAL`.
4. Código y datos históricos permanecen separados (ver `Architecture.md` §3.1, `DataModel.md`).
5. Los sistemas se diseñan generales y reutilizables, nunca nombrados por contenido específico (`Architecture.md` §3.2).
6. Optimización móvil es transversal desde Fase 1, no un pase de pulido al final.
7. El multijugador está considerado en la arquitectura desde Fase 0; no se construye single-player primero y se añade multiplayer al final.
8. Paradoja se considera arquitectónicamente pero no se desarrolla hasta que el juego base esté terminado.
9. No se eliminan sistemas existentes para resolver requisitos nuevos; se prefiere extensión por datos/composición.
10. Toda decisión técnica importante queda documentada (en `docs/`, ampliando estos documentos o añadiendo nuevos con el mismo estándar).

## Protocolo por fase (a partir de Fase 1)

**Al iniciar una fase:**
```
FASE X — [NOMBRE]
Objetivo
Sistemas afectados
Archivos que crearé/modificaré
Riesgos
Criterios de aceptación
```

**Al finalizar una fase:**
```
FASE X COMPLETADA
Implementado
Pruebas realizadas
Problemas conocidos
Deuda técnica
Archivos principales
Cómo probarlo
Próxima fase
```

Y detenerse a esperar "CONTINUAR" o instrucciones de modificación.

## Regla ante errores

Si una implementación falla: no se oculta. Se indica qué falló, por qué, qué parte sigue funcionando, y qué solución se implementará. Se corrige antes de construir nuevas funciones sobre una base rota (§19 del prompt maestro).
