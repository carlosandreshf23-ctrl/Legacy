# game-client

Proyecto Unity de LEGADO: PERÚ (cliente móvil/PC).

**Estado actual**: no inicializado. Se crea con el Editor de Unity real al comenzar la **Fase 1** (`docs/DevelopmentRoadmap.md`), no antes — generar `ProjectSettings/` y archivos `.meta` sin el Editor produce un proyecto inválido.

**Motor/stack decidido en Fase 0** (ver `docs/Architecture.md` §2): Unity LTS, C#, URP, Input System, Addressables, Netcode for GameObjects + Unity Relay/Lobby, Cinemachine.

**Estructura de carpetas objetivo** (se puebla progresivamente por fase, ver `docs/Architecture.md` §4):

```
Assets/
  _Project/
    Scripts/
      Core/            Bootstrap, ServiceLocator/EventBus, TimeSystem
      Systems/          Un subdirectorio por sistema (ver docs/GameSystems.md)
      Data/             Definiciones de ScriptableObject (esquemas)
      Characters/       Controlador de personaje, cámara, input móvil
      UI/
      Environment/      Streaming de regiones, día/noche, clima
    Data/
      Historical/       Instancias de datos históricos (Characters/Events/Locations/Eras)
    Scenes/
    Prefabs/
    Art/
    Audio/
  Plugins/
Packages/
ProjectSettings/
```

No colocar datos históricos (personajes reales, eventos, ubicaciones) como literales en scripts C#: deben vivir en `Assets/_Project/Data/Historical/` como assets de datos (Regla 4 del proyecto, ver `docs/DataModel.md`).
