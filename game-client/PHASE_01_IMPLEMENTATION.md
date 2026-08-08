# PHASE_01_IMPLEMENTATION.md — LEGADO: PERÚ

**Fase:** 1 — Fundación Jugable
**Estado:** código completo, **no compilado ni ejecutado por el agente** (ver §0, limitación crítica del entorno).

> **Nota de Fase 2 (pivote de dirección artística):** a partir de Fase 2 el proyecto es un
> RPG 2D cenital en pixel art, no 3D. La cámara en tercera persona y el `CharacterMotor`
> basado en `CharacterController` descritos en este documento fueron **retirados y
> reemplazados** por `PixelPerfectCamera2D` y un `CharacterMotor` basado en `Rigidbody2D`.
> `Phase1SandboxSceneBuilder.cs` (escena de bloques 3D) fue eliminado y sustituido por
> `Phase2LookAndFeelSceneBuilder.cs`. El resto de sistemas descritos aquí (guardado,
> inventario, diálogo, decisiones, calendario, UI) siguen vigentes sin cambios de fondo.
> Ver `game-client/ART_BIBLE_v0.1.md` y `game-client/PROGRESS.md` para el detalle vigente.

---

## 0. Limitación crítica del entorno de desarrollo (leer primero)

Esta implementación se escribió en un contenedor sin Unity Editor, sin `dotnet`/`mono`/`msbuild` y sin GPU. Se verificó explícitamente al iniciar la fase (`which unity dotnet mono msbuild` → nada instalado). Esto significa:

- **Todo el código de este documento fue escrito, no compilado ni ejecutado.** No hay garantía de que compile sin errores a la primera.
- **Ningún sistema fue jugado.** No puedo confirmar que "la base del juego se sienta correcta" (la pregunta central de esta fase, prompt §1) — eso solo puede responderlo una persona jugando en un Editor real.
- **La prueba manual crítica (prompt §42) y la demostración jugable (prompt §53) no se ejecutaron.** Se documentan como checklist (§7 de este documento) para que se realicen en un entorno con Unity real.
- **Rendimiento, FPS, draw calls y tamaño de build no se midieron.** No hay dispositivo ni Editor disponible aquí.

Esta fase se completó bajo el modelo acordado explícitamente con el usuario: **código completo + checklist QA**, no "jugable verificado por el agente". El resto de este documento asume que un humano con Unity Editor instalado ejecutará el checklist de la sección 7 y reportará resultados.

---

## 1. Qué se construyó

Un proyecto Unity completo en código fuente (`game-client/Assets/_Project/`), organizado según la arquitectura de Fase 0, más un conjunto de **scripts de Editor** que generan las dos escenas (`MainMenu`, `Sandbox_1820_Prototype`) y los datos de contenido (ítem, 3 ubicaciones, diálogo de NPC) de forma **procedural dentro del Editor**, en lugar de archivos `.unity`/`.asset` escritos a mano.

### Por qué escenas generadas por código y no archivos `.unity` hechos a mano

Los archivos de escena y de assets de Unity (`.unity`, `.asset`) son YAML con referencias por GUID que Unity asigna al importar. Sin Editor para generarlos y validarlos, escribirlos a mano a ciegas habría sido probable causa de un proyecto roto (GUID mal formados, componentes "Missing Script", etc.) sin ninguna forma de detectarlo antes de entregarlo. En su lugar, se escribieron **scripts de Editor** (`Assets/_Project/Editor/Phase1*.cs`) que construyen la escena/los datos usando la API de Unity — es Unity mismo quien genera la serialización correcta la primera vez que el usuario los ejecuta. Es la decisión técnica más importante de esta fase; ver §19 (Decisiones técnicas) para el detalle.

---

## 2. Cómo poner el proyecto en marcha (primera vez)

1. Instalar **Unity 2022.3 LTS** (o una LTS posterior compatible) vía Unity Hub.
2. Crear un proyecto nuevo con la plantilla **3D (URP)**. Esto genera un `ProjectSettings/`/`Packages/` válidos que este repositorio no incluye (ver §0 — no se generaron a mano por el mismo motivo que las escenas).
3. Copiar dentro de ese proyecto nuevo, sustituyendo/fusionando lo que corresponda:
   - `game-client/Assets/_Project/` → `<TuProyecto>/Assets/_Project/`
   - `game-client/Assets/Tests/` → `<TuProyecto>/Assets/Tests/`
   - Revisar `game-client/Packages/manifest.json` de este repositorio y añadir a tu `Packages/manifest.json` cualquier dependencia que falte (Unity Test Framework normalmente ya viene en la plantilla 3D URP).
4. Abrir el proyecto en Unity. Dejar que termine de importar (puede tardar unos minutos la primera vez).
5. En la barra de menú: **Legado → Fase 1 → 0. Build Everything (Content + Scenes + Build Settings)**.
   - Esto ejecuta, en orden: sembrado de contenido → construcción de `Sandbox_1820_Prototype.unity` → construcción de `MainMenu.unity` → registro de ambas en Build Settings.
   - Si prefieres ejecutarlo paso a paso (más fácil de depurar si algo falla), usa los ítems `1.` a `4.` del mismo menú en orden.
6. Abrir la escena `Assets/_Project/Scenes/MainMenu.unity` y presionar **Play**.

Si algún paso del menú "Legado" lanza un error de compilación o de API: ver §8 (limitaciones conocidas) y §9 (tabla de construcción manual alternativa) antes de reportarlo como bloqueante — es el punto más probable de necesitar un ajuste menor específico de la versión de Unity usada.

---

## 3. Arquitectura real (mapeo a Fase 0)

| Capa (`01_ARCHITECTURE.md`) | Dónde vive en código | Assembly |
|---|---|---|
| Core | `Scripts/Core/` (ServiceLocator, EventBus, GameBootstrap, GameEvents) | `LegadoPeru.Runtime` |
| Calendar (parte de Core/History en Fase 0) | `Scripts/Calendar/` | `LegadoPeru.Runtime` |
| Characters | `Scripts/Characters/` | `LegadoPeru.Runtime` |
| InputSystem | `Scripts/InputSystem/` | `LegadoPeru.Runtime` |
| Interaction | `Scripts/Interaction/` | `LegadoPeru.Runtime` |
| Inventory (parte de Economy en Fase 0) | `Scripts/Inventory/` | `LegadoPeru.Runtime` |
| Dialogue (parte de Narrative en Fase 0) | `Scripts/Dialogue/` | `LegadoPeru.Runtime` |
| Narrative | `Scripts/Narrative/` | `LegadoPeru.Runtime` |
| World | `Scripts/World/` | `LegadoPeru.Runtime` |
| Persistence | `Scripts/Persistence/` | `LegadoPeru.Runtime` |
| UI | `Scripts/UI/` | `LegadoPeru.Runtime` |
| Audio | `Scripts/Audio/` | `LegadoPeru.Runtime` |
| Settings (parte de Content/UI en Fase 0) | `Scripts/Settings/` | `LegadoPeru.Runtime` |
| Debug | `Scripts/DebugTools/` | `LegadoPeru.Runtime` |
| — (herramientas de construcción) | `Assets/_Project/Editor/` | `LegadoPeru.Editor` |
| — (tests) | `Assets/Tests/EditMode/` | `LegadoPeru.Tests.EditMode` |

Tres assemblies (`.asmdef`), tal como pedía el prompt §3 ("assemblies/módulos"): `LegadoPeru.Runtime` (gameplay, compila en cliente y editor), `LegadoPeru.Editor` (solo Editor, referencia a Runtime), `LegadoPeru.Tests.EditMode` (solo Editor, referencia a Runtime + NUnit).

### Principio de diseño aplicado consistentemente

Todo sistema con estado (`GameCalendarSystem`, `InventoryModel`, `DecisionService`, `DiscoverySystem`, `LocationSystem`, `WorldObjectRegistry`) es una **clase C# plana**, no un `MonoBehaviour` — se registra en `ServiceLocator` desde `GameBootstrap` y no depende del Editor ni de una escena para funcionar. Esto es lo que permite que los tests EditMode (§6) prueben la lógica de guardado/decisiones/descubrimiento sin necesitar Play Mode.

### `GameBootstrap` — orden de inicialización

`GameBootstrap` se autoinstala vía `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`, antes de que cargue la primera escena (sea `MainMenu` o `Sandbox`, útil para iterar directamente en la escena de pruebas). Registra: `GameCalendarSystem`, `DiscoverySystem`, `LocationSystem`, `DecisionService`, `WorldObjectRegistry`, `DialogueRunner`, `InventoryModel` (con `ItemDatabase` cargado desde `Resources`), `SettingsService`, `SaveSystem`, `AudioService`.

---

## 4. Controles

| Acción | Teclado/mouse | Táctil |
|---|---|---|
| Mover | WASD / flechas (ejes "Horizontal"/"Vertical" por defecto de Unity) | Joystick virtual inferior izquierdo |
| Mirar/cámara | Mouse | Arrastrar en la zona derecha de la pantalla |
| Correr | Mantener Shift izquierdo | Botón "Correr" (mantener presionado) |
| Interactuar | E | Botón "Interactuar" |
| Inventario | Tab (o botón HUD) | Botón "Inventario" en HUD |
| Pausa | Esc (o botón HUD) | Botón "Pausa" en HUD |
| Panel de debug | F1 | F1 (no hay atajo táctil dedicado en esta fase) |

La capa `GameInput`/`IInputProvider` (prompt §12) abstrae ambos esquemas; el resto del gameplay solo consulta `MoveAxis`, `LookDelta`, `InteractPressed`, `RunHeld`, etc. — nunca teclas directamente. `GameInput` decide en su propio `Awake()` (tiempo de ejecución, no de Editor) si usar el proveedor táctil o el de teclado/mouse, según `Application.isMobilePlatform`.

**Decisión técnica**: se usa el Input Manager clásico de Unity (`UnityEngine.Input`) en vez del paquete Input System nuevo, para no depender de un asset `.inputactions` generado por Editor (mismo motivo que evitar escenas hechas a mano). Migrar al paquete nuevo en el futuro solo requeriría reemplazar `KeyboardMouseInputProvider`, sin tocar ningún otro sistema.

---

## 5. Sistemas por área

### Interacción (prompt §13-15)
`InteractionSystem` (en el jugador) hace `Physics.OverlapSphere` cada frame, encuentra el `IInteractable` más cercano disponible y publica el prompt vía `EventBus`. `SupplyBoxInteractable` y `NPCController` implementan `IInteractable`; ninguna lógica específica de objeto vive en `InteractionSystem` ni en `PlayerCharacterController`.

### Inventario (prompt §16-17)
`InventoryModel` (lista de `ItemStack { itemId, quantity }`) + `ItemDefinition`/`ItemDatabase` como `ScriptableObject` data-driven. Un único ítem sembrado en esta fase: `ITEM_PROVISIONS`.

### NPC y diálogo (prompt §18-22)
`NPCController` patrulla 2 puntos, mira al jugador si está cerca, y abre `DialogueRunner` al interactuar. `DialogueDefinition` (data-driven, `ScriptableObject`) contiene el diálogo de prueba completo: saludo con elección Sí/No → pregunta de la bolsa perdida (decisión `TestDecision_Honesty`, valores `TRUE`/`FALSE`) → cierre. La decisión se registra en `DecisionService` exactamente como pide el prompt §21-22.

### Tiempo (prompt §23-25)
`GameCalendarSystem` (fecha inicial 1 sep 1820, 08:00) avanza a razón de `MinutesPerRealSecond = 4` en el Sandbox (configurable). `DayNightController` rota la luz direccional y ajusta color/intensidad según la hora. `DebugPanelController` (F1) muestra `GameDate`, FPS, posición y ubicación actual.

### Mundo y descubrimiento (prompt §26-28)
`LocationSystem` + `LocationTrigger` + 3 `LocationDefinition` (`Prototype_Coast`, `Prototype_Path`, `Prototype_House`). `DiscoverySystem` implementa el enum completo de 5 estados pero Fase 1 solo transiciona `Unknown → Visited`, tal como pide el prompt §27. `MapMVPController` muestra un mapa esquemático con 3 marcadores + posición aproximada del jugador.

### Guardado (prompt §29-34)
`SaveSystem` construye un único DTO (`SaveGameDataV1`) por slot, serializado con `JsonUtility` a `Application.persistentDataPath/Saves/<slot>.json`, con copia `.bak` antes de cada sobrescritura. `SaveVersion = 1` presente desde el primer save (prompt §31). `PersistentWorldObject`/`WorldObjectRegistry` guardan el estado de la caja de suministros y del NPC por `PersistentId` (`SupplyBox_Prototype_001`, `NPC_Test_01`), nunca por referencia de escena. `AutosaveController` guarda en cambios de ubicación y al terminar un diálogo, con un mínimo de 30s entre autosaves.

**Nota importante sobre el orden de carga**: `SandboxSceneController.Start()` aplica el save pendiente (si lo hay) *después* de que todos los `PersistentWorldObject` de la escena ya ejecutaron su propio `Awake()` (Unity garantiza que todos los `Awake` de una escena ocurren antes que cualquier `Start`), de modo que `WorldObjectRegistry.RestoreAll` siempre encuentra los objetos ya registrados.

---

## 6. Tests automatizados (prompt §41)

En `Assets/Tests/EditMode/`, ejecutables vía **Window → General → Test Runner → EditMode** una vez el proyecto esté abierto en Unity:

| Archivo | Cubre |
|---|---|
| `PlayerCharacterControllerTests.cs` | Test 1: posición/rotación persiste |
| `InventoryModelTests.cs` | Test 2: inventario persiste (+ stacking, remove insuficiente) |
| `WorldObjectRegistryTests.cs` | Test 3: objeto recogido permanece recogido |
| `DecisionServiceTests.cs` | Test 4: decisión persiste (+ caso `TestDecision_Honesty`) |
| `GameCalendarSystemTests.cs` | Test 5: GameDate persiste (+ aritmética de `WorldDate`, eventos) |
| `DiscoverySystemTests.cs` | Test 6: zona visitada persiste |
| `SaveGameDataSerializationTests.cs` | Round-trip completo de `SaveGameDataV1` vía `JsonUtility` |

Todos prueban las clases de servicio C# puras directamente (sin depender de Play Mode), salvo `PlayerCharacterControllerTests` y `WorldObjectRegistryTests`, que instancian el `MonoBehaviour` mínimo necesario pero llaman a sus métodos públicos directamente (`CaptureState`/`RestoreState`/`Register`) sin depender de si `Awake()` se ejecuta o no en el momento de creación del `GameObject` (comportamiento no garantizado fuera de Play Mode) — diseño deliberado para que los tests sean fiables independientemente de esa ambigüedad.

**Estos tests no se han ejecutado** (no hay Unity aquí). Ejecutarlos es el primer paso recomendado del checklist de la sección 7.

---

## 7. Checklist de QA manual (a ejecutar en Unity real)

Mapea directamente la prueba crítica del prompt §42 y los criterios de aceptación del prompt §52. Marcar cada casilla al validarla.

### 7.1 Compilación y arranque
- [ ] El proyecto compila sin errores tras seguir §2.
- [ ] `Window → General → Test Runner → EditMode → Run All` — todos los tests de §6 pasan.
- [ ] `Legado → Fase 1 → 0. Build Everything` se ejecuta sin excepciones en la consola.
- [ ] `MainMenu.unity` abre y el botón "Nueva partida" carga `Sandbox_1820_Prototype`.

### 7.2 Personaje y cámara (prompt §52 "PLAYER")
- [ ] El personaje camina con WASD/joystick.
- [ ] Corre al mantener Shift/botón Correr (velocidad visiblemente mayor).
- [ ] La stamina baja al correr y se recupera al parar (barra en HUD).
- [ ] La cámara orbita con mouse/arrastre táctil, con límites verticales razonables.
- [ ] La cámara no atraviesa groseramente la geometría del bloqueo (se acerca al chocar).
- [ ] Controles táctiles visibles y usables si se fuerza `forceTouchInEditor = true` en el `GameInput` de la escena.

### 7.3 Interacción (prompt §52 "INTERACTION")
- [ ] Al acercarse a la caja de suministros aparece el prompt "Recoger provisiones".
- [ ] Interactuar añade 1x Provisiones al inventario y la caja cambia de aspecto (overlay "vacía").
- [ ] Al acercarse al NPC aparece el prompt "Hablar".
- [ ] Interactuar abre el panel de diálogo con el saludo inicial.

### 7.4 Diálogo y decisión (prompt §52 "NARRATIVE")
- [ ] El diálogo avanza correctamente con las elecciones Sí/No del saludo.
- [ ] Llega a la pregunta de la bolsa perdida y ambas respuestas están disponibles.
- [ ] Elegir una respuesta cierra el diálogo en el nodo de cierre correspondiente.
- [ ] (Debug) Verificar en el panel F1 o con un breakpoint que `DecisionService.Records` contiene `TestDecision_Honesty = TRUE` o `FALSE` según la elección.

### 7.5 Inventario (prompt §52 "INVENTORY")
- [ ] El botón/panel de inventario muestra "Provisiones x1" tras recogerlo.
- [ ] El panel de inventario se puede cerrar y reabrir sin perder el dato.

### 7.6 Mundo y descubrimiento (prompt §52 "WORLD")
- [ ] Existen al menos 3 subzonas reconocibles (costa, camino, casa) con geometría de bloqueo distinta.
- [ ] Cruzar cada zona por primera vez la marca como visitada (verificable en el panel de mapa: el marcador cambia de color).
- [ ] El panel de mapa muestra un marcador aproximado de la posición del jugador que se mueve al caminar.

### 7.7 Tiempo (prompt §52 "TIME")
- [ ] El reloj del HUD/panel de debug avanza solo con el tiempo real (sin input del jugador).
- [ ] La iluminación cambia visiblemente entre un intervalo de tiempo suficiente (unos minutos reales, dado `MinutesPerRealSecond = 4`).

### 7.8 Guardado (prompt §52 "SAVE", y prueba crítica §42 completa)
Ejecutar la secuencia exacta del prompt §42:
1. [ ] Nueva partida.
2. [ ] Caminar hacia el NPC.
3. [ ] Hablar.
4. [ ] Tomar la decisión (elegir una respuesta a la pregunta de la bolsa).
5. [ ] Recoger las provisiones.
6. [ ] Visitar una nueva zona.
7. [ ] Esperar a que cambie visiblemente la hora/iluminación.
8. [ ] Guardar (botón "Guardar partida" del menú de pausa, o esperar al autosave tras un cambio de zona).
9. [ ] Cerrar el juego (detener Play Mode o cerrar la build).
10. [ ] Cargar ("Continuar" o "Cargar partida" desde el menú principal).
11. [ ] Confirmar:
    - [ ] Misma posición (aproximada, dentro de la misma zona).
    - [ ] Misma hora de juego (o posterior, nunca reiniciada a las 08:00 salvo que así se haya guardado).
    - [ ] Inventario correcto (Provisiones x1 presente).
    - [ ] La caja de suministros sigue vacía (no reaparecen las provisiones al re-interactuar).
    - [ ] La decisión sigue registrada (mismo valor TRUE/FALSE que se eligió).
    - [ ] La zona visitada sigue marcada como tal en el mapa.

Si cualquier punto de 7.8 falla: **Fase 1 no está completa**, tal como especifica el prompt §42.

### 7.9 Móvil (prompt §52 "MOBILE")
- [ ] `Legado → Fase 1` no requiere ningún paso adicional para generar los 3 niveles de calidad (Low/Medium/High vía `QualityLevelController`); confirmar que el dropdown de Configuración cambia `QualitySettings.SetQualityLevel` sin error.
- [ ] Medir FPS aproximado en el panel de debug (F1) — no hay objetivo numérico verificado por el agente, solo confirmar que el contador funciona.

---

## 8. Limitaciones conocidas / deuda técnica

| Punto | Detalle |
|---|---|
| **No compilado ni jugado por el agente** | Ver §0. Es la limitación principal de esta entrega. |
| Sin arte real | Todo el Sandbox usa primitivas de Unity (planos, cubos, cápsulas) coloreadas, tal como pedía el prompt §46 (prioridad a gameplay, no a arte). |
| Sin audio real | `AudioService` existe y expone `PlayFootstep/PlayInteraction/PlayUI/PlayAmbient`, pero no hay ningún `AudioClip` asignado (no hay assets de audio disponibles en este entorno de desarrollo). Los métodos son no-op sin clips. |
| `WorldDate` no considera años bisiestos | Limitación documentada en el propio código (`WorldDate.cs`); sin impacto narrativo en el rango de fechas de este slice de pruebas. |
| `PendingLoadRequest` es estado estático global | Puente simple MainMenu → Sandbox; aceptable para una sola escena de juego en esta fase, a revisar si se añaden más escenas de gameplay. |
| Sin NavMesh | El NPC patrulla por `Vector3.MoveTowards` entre 2 puntos fijos, no usa navegación — suficiente para "rutina extremadamente sencilla" (prompt §18). |
| `SaveSystem` no es aún snapshot+event log | Fase 1 implementa un DTO único por slot (MVP), detrás del mismo contrato conceptual (`CaptureState`/`RestoreState`) que la arquitectura completa de `08_SAVE_SYSTEM.md` (snapshot+log) usará desde Fase 8-9. No es una regresión: es el nivel de complejidad correcto para esta fase, según lo acordado en Fase 0. |
| Sin backend/multiplayer | Fuera de alcance explícito de esta fase (prompt §43); `PlayerId/CharacterId/FamilyId` ya existen como datos (`LocalPlayer01` implícito, `SAL_MATEO_001`, `FAM_SALAZAR`) para no bloquear su introducción futura. |
| Editor scripts no probados | `Phase1SandboxSceneBuilder.cs` en particular es el archivo de mayor riesgo (uso extenso de la API de `UnityEditor`/`UnityEngine.UI` sin poder compilar). Ver §9 para un plan de contingencia si falla. |

---

## 9. Si el script de construcción de escena falla: tabla de referencia manual

Si `Legado → Fase 1 → 2. Build Sandbox Scene` lanza un error de compilación o de API específico de tu versión de Unity, esta tabla resume exactamente qué construye el script, para reproducirlo a mano o corregir el script puntualmente sin tener que rediseñar nada:

| Elemento | Tipo/Componentes | Notas |
|---|---|---|
| `Sun` | `Light` (Directional) | Rotación inicial (50,170,0) |
| `Ground_Blockout` | Primitive Plane, escala (45,1,45) | ~450×450m |
| `Path_Blockout`, `Obstacle_Rock_01`, `Slope_Blockout`, `House_Blockout` | Primitive Cube | Geometría de bloqueo, ver prompt §6 |
| `Location_Coast/Path/House` | `BoxCollider` (isTrigger) + `LocationTrigger` | Referencian los 3 `LocationDefinition` sembrados |
| `Player` | `CharacterController` + `CharacterMotor` + `PlayerCharacterController` + `FootstepPlayer` + `InteractionSystem`, tag `Player` | Hijo visual: Capsule sin collider propio |
| `MainCamera` | `Camera` + `AudioListener` + `ThirdPersonCameraController`, tag `MainCamera` | Target = `Player` |
| `_Systems` | `GameCalendarDriver` + `DayNightController` (sun = `Sun`) | |
| `NPC_Test_01` | `NPCController`, PersistentId=`NPC_Test_01`, dialogue = asset sembrado, 2 patrol points | |
| `SupplyBox_Prototype_001` | `SupplyBoxInteractable`, PersistentId=`SupplyBox_Prototype_001`, item=`ITEM_PROVISIONS` x1 | Hijo `Visual_Empty_Overlay` inactivo por defecto |
| `Canvas` (+ `EventSystem`) | `ScreenSpaceOverlay`, `CanvasScaler` 1920×1080 | Contiene HUD, InventoryPanel, DialoguePanel, PausePanel, SettingsPanel, DebugPanel, MapPanel, TouchControls_Root, GameInput |
| `_SandboxSceneController` | `SandboxSceneController` + `AutosaveController` | |

Los datos sembrados (`Legado → Fase 1 → 1. Seed Content Data`) crean: `Assets/_Project/Data/Resources/ItemDatabase.asset` (+`ITEM_PROVISIONS.asset`), `Locations/Prototype_{Coast,Path,House}.asset`, `Dialogue/NPC_Test_01.asset`.

---

## 10. Archivos principales

```
game-client/
  Assets/_Project/Scripts/          ~40 scripts (Core, Calendar, InputSystem, Characters,
                                     Interaction, Inventory, Dialogue, Narrative, World,
                                     Persistence, UI, Audio, Settings, DebugTools)
  Assets/_Project/Editor/           Phase1ContentSeeder.cs, Phase1SandboxSceneBuilder.cs,
                                     Phase1MainMenuSceneBuilder.cs, Phase1BuildSetup.cs
  Assets/Tests/EditMode/            7 archivos de test (ver §6)
  Packages/manifest.json            Dependencias de referencia (fusionar con la del proyecto real)
  PHASE_01_IMPLEMENTATION.md        Este documento
```

---

## 11. Cómo probarlo (resumen)

1. Seguir §2 para poner el proyecto en marcha.
2. Ejecutar los tests EditMode (§6).
3. Ejecutar el checklist completo de §7, especialmente 7.8 (prueba crítica).
4. Reportar de vuelta: qué falló, en qué paso, y el mensaje de error exacto si lo hay — para poder corregirlo en una iteración siguiente sin tener que re-derivar todo el diseño.

## 12. Próxima fase

**Fase 2 — Vertical Slice Histórico (Paracas/Pisco, septiembre de 1820)**, según el roadmap ya aprobado en Fase 0 y descrito en el prompt de esta fase §56. No debe iniciarse hasta que el checklist de §7 se haya ejecutado en un entorno real y Fase 1 se declare completa sin errores críticos conocidos (prompt §49).
