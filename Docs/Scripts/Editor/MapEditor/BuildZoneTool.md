# BuildZoneTool

`Assets/_Scripts/Editor/MapEditor/BuildZoneTool.cs`

Herramienta **Zona edificable**. Pinta por celda la máscara del [MapDataSO](../../ScriptableObjects/MapDataSO.md) que [BuildingPlacement](../../Buildings/BuildingPlacement.md)`.IsAreaBuildable` consulta en runtime.

La máscara cubre los dos usos con una sola herramienta:

- **Zona designada** (lista blanca): "Nada edificable" y después pintar la zona del jugador.
- **Solo exclusiones** (lista negra): dejar todo edificable y bloquear lo que haga falta, por ejemplo los caminos desde [PathTool](PathTool.md).

No hace falta bloquear a mano agua, acantilados ni obstáculos: `IsAreaBuildable` ya exige NavMesh navegable en cada celda.

**Overlay.** Las celdas bloqueadas se dibujan en rojo con un mesh generado (un quad por celda bloqueada, apoyado en la altura del terreno en sus cuatro esquinas) y `Graphics.DrawMeshNow` con el shader `Hidden/Internal-Colored`. Se regenera solo cuando cambia la máscara, no en cada repaint.
