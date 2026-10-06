# WaterTool

`Assets/_Scripts/Editor/MapEditor/WaterTool.cs`

Herramienta **Agua**. Modelo: el agua es un único plano a una altura global (`MapDataSO.WaterLevel`); hay un cuerpo de agua en cualquier lugar donde el terreno quede por debajo de ese nivel. No hay objetos "lago" o "río": se cavan.

- Arrastrar cava hacia `WaterLevel − Profundidad`, a velocidad proporcional a la profundidad; la orilla sale con la pendiente del falloff del pincel. Opcionalmente pinta la capa de arena.
- `Shift` + arrastrar rellena hasta el suelo base (Y = 0).
- El slider de nivel mueve el plano y actualiza el `MapDataSO` (`MapSceneSetup.ApplyWaterLevel`).

**Navegación.** El objeto `Water` lleva un `NavMeshModifierVolume` con área *Not Walkable* cuyo techo queda 0.3 por debajo de la superficie: todo lo más profundo que esa profundidad de vadeo se excluye del NavMesh en el siguiente bake, y la orilla sigue siendo caminable. Como el volumen es parte de `Water`, acompaña cualquier cambio de nivel. El plano no tiene collider: no entra al bake ni bloquea el raycast del pincel contra el fondo.

Referencia: paquete AI Navigation, "NavMesh Modifier Volume component reference".
