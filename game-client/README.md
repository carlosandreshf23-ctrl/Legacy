# game-client

Proyecto Unity de LEGADO: PERÚ (cliente móvil/PC).

**Estado actual**: código fuente de **Fase 1 — Fundación Jugable** completo (`Assets/_Project/Scripts`, `Assets/_Project/Editor`, `Assets/Tests`), escrito sin acceso a Unity Editor en este entorno de desarrollo — no compilado ni ejecutado todavía por el agente. Ver [`PHASE_01_IMPLEMENTATION.md`](PHASE_01_IMPLEMENTATION.md) para el detalle completo, instrucciones de puesta en marcha y el checklist de QA manual pendiente de ejecutar en un entorno con Unity real.

Este repositorio **no incluye** `ProjectSettings/`/`Library/` de Unity: hay que crear un proyecto 3D (URP) nuevo desde Unity Hub y copiar `Assets/_Project`, `Assets/Tests` dentro (motivo detallado en `PHASE_01_IMPLEMENTATION.md` §0 y §2 — generar esos archivos a mano sin el Editor real habría producido un proyecto inválido).

**Motor/stack decidido en Fase 0** (ver `docs/01_ARCHITECTURE.md` §2): Unity 2022.3 LTS, C#, URP. El paquete Input System, Addressables, Netcode for GameObjects y Cinemachine están previstos en la arquitectura pero **no se usan todavía en Fase 1** (se usó el Input Manager clásico + streaming de una única escena pequeña + cámara scripteada a mano, decisiones documentadas en `PHASE_01_IMPLEMENTATION.md` §19/§4).

**Estructura real de `Assets/_Project/`** (ver `docs/01_ARCHITECTURE.md` §3 para el objetivo completo a largo plazo):

```
Assets/
  _Project/
    Scripts/
      Core/            ServiceLocator, EventBus, GameBootstrap, eventos
      Calendar/         GameCalendarSystem, WorldDate, ciclo día/noche
      InputSystem/       Abstracción de input (teclado/mouse + táctil)
      Characters/        PlayerCharacterController, cámara, NPC
      Interaction/       IInteractable, InteractionSystem, caja de suministros
      Inventory/         ItemDefinition/ItemDatabase, InventoryModel
      Dialogue/          DialogueDefinition data-driven, DialogueRunner
      Narrative/         DecisionRecord, DecisionService
      World/             Locations, Discovery, PersistentWorldObject
      Persistence/       SaveSystem, autosave
      UI/                HUD, menús, paneles
      Audio/             AudioService
      Settings/          Configuración, niveles de calidad
      DebugTools/        Logging categorizado
    Data/Resources/      Datos sembrados por el Editor (ItemDatabase, Locations, Dialogue)
    Scenes/              MainMenu.unity, Sandbox_1820_Prototype.unity (generadas por script)
  Editor/                Scripts que generan escenas/datos (menú "Legado > Fase 1")
Tests/EditMode/          Tests de los sistemas de guardado/decisiones/descubrimiento
```

No hay todavía datos históricos reales (eso empieza en Fase 2); el único contenido narrativo de Fase 1 es de prueba y está marcado `PROTOTYPE / NOT VALIDATED` en el propio código (Regla 3 del proyecto).
