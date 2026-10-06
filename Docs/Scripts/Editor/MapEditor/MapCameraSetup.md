# MapCameraSetup

`Assets/_Scripts/Editor/MapEditor/MapCameraSetup.cs`

Deja la escena de un mapa con la cámara RTS armada igual que en `SampleScene`. Lo llama [MapSceneSetup](MapSceneSetup.md)`.CreateMap` al crear un mapa, y [MapEditorWindow](MapEditorWindow.md) lo ofrece con un botón cuando la escena tiene mapa pero no cámara.

**Qué es "el estándar de SampleScene".** La cámara de `SampleScene` es una instancia del prefab `Assets/Prefabs/CameraSystem.prefab` (Main Camera con `CinemachineBrain`, `RTScamera` con `CinemachineOrbitalFollow`, `RTScameraTarget` y [Player](../../RTSCamera/Scripts/Player.md)) con tres overrides que el prefab solo no trae, y que `EnsureCameraRig` replica sobre la instancia nueva:

| Override en SampleScene | Por qué importa |
|---|---|
| `Player.inputReader` = asset `InputReader` | El prefab lo tiene vacío; sin eso `Player` loguea error y la cámara no responde. |
| Componente `PlayerInput` removido de `Player` | Es el input legacy, reemplazado por `InputReader`. |
| Rig a Y = 14.2 | El `cameraTarget` solo se mueve en XZ, así que esa altura es parte del encuadre: con radio orbital 25, define a qué distancia efectiva del suelo queda la cámara y hasta dónde llega el zoom. |

La posición XZ no se copia (en `SampleScene` apunta a la base de prueba): el rig se centra sobre el Inicio del jugador si el mapa ya tiene ese marcador, o sobre el centro del mapa. `FocusCameraRig` repite ese centrado a pedido.

**Otras cámaras.** Antes de instanciar el rig se desactivan (no se borran) los GameObjects con una `Camera` activa con tag `MainCamera` — típicamente la "Main Camera" por defecto de una escena nueva — porque `Player` usa `Camera.main` y dos `AudioListener` activos generan warnings.

Si la escena ya tiene un `Player` de cámara, no hace nada.

El prefab `CameraSystem` no se modifica. Si en algún momento se le aplican esos overrides al prefab, este script sigue funcionando: solo asigna el `InputReader` cuando falta y solo remueve `PlayerInput` cuando existe.
