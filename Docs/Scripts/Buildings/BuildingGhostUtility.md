# BuildingGhostUtility

`Assets/_Scripts/Buildings/BuildingGhostUtility.cs`

Utilidad estática extraída de [BuildingPlacementController](BuildingPlacementController.md) al sumar [WallPlacementController](WallPlacementController.md), que necesitaba exactamente la misma lógica de ghost pero para varios segmentos en vez de uno solo.

- `StripFunctionalComponents(GameObject)`: destruye `NavMeshObstacle`, `Collider`, [BuildingPlacement](BuildingPlacement.md), [DropOffBuilding](DropOffBuilding.md), `Health` y `UnitSelectionHandler` de una instancia — deja solo la parte visual (mesh/renderer), para que un preview no talle el NavMesh, no bloquee físicamente, no se registre en ningún manager ni sea seleccionable/dañable.
- `Tint(GameObject, Color)`: aplica un color a todos los `MeshRenderer` del objeto (y sus hijos) — usado para el feedback verde/rojo de posición válida/inválida.
