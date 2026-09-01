# Player (RTS Camera)

`Assets/_Scripts/RTSCamera/Scripts/Player.cs` (namespace `Assets.RTSCamera.Scripts`)

Controlador de la cámara RTS. Se suscribe a los eventos de [InputReader](../../_CORE%20Scripts/InputReader.md) (`OnEnable`/`OnDisable`) y traduce ese input en tres comportamientos independientes, todos actualizados en `LateUpdate` (para aplicarse después del resto del frame y reducir stuttering):

- **Movimiento** (`UpdateMovement`): paneo relativo a la orientación de la cámara, con aceleración/desaceleración e input combinado de teclado + edge scrolling (activado solo fuera del editor). La velocidad de movimiento se escala según el nivel de zoom actual (`moveSpeedZoomCurve`) para que se sienta más lenta cerca del suelo.
- **Órbita** (`UpdateOrbit`): rota la cámara alrededor del `cameraTarget` usando Cinemachine (`CinemachineOrbitalFollow`), solo mientras se mantiene presionado el botón de rotación.
- **Zoom** (`UpdateZoom`): ajusta el eje radial de Cinemachine con suavizado.

Depende del paquete Cinemachine (`CinemachineOrbitalFollow`) para exponer los ejes Horizontal/Vertical/Radial que este script manipula directamente.

Queda comentado un bloque grande de "Legacy controls" (versión anterior basada en `OnMove`/`OnLook` de Unity `PlayerInput` en vez de `InputReader`) — candidato a eliminar una vez confirmado que el flujo actual es estable.
