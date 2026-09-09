# DropOffBuildingPrefabGenerator

`Assets/_Scripts/Editor/DropOffBuildingPrefabGenerator.cs`

Script de Editor. Menú `Tools/RTS/Generate Drop-Off Building Prefabs` genera, en una sola corrida, los dos edificios de recolección por tipo de recurso: **Lumbermill** (Wood) y **Farm** (Food). Ambos con footprint 2x2 (2 unidades de Unity = 2 celdas de grilla, ver [BuildingPlacement](../Buildings/BuildingPlacement.md)).

Por cada edificio crea:
- Un [BuildingDataSO](../ScriptableObjects/BuildingDataSO.md) en `Assets/_Scripts/ScriptableObjects/AssetsFromSO/BuildingData_<Nombre>.asset` (mismo folder que ya usan `Channel_GameOver`/`InputReader`), con `BuildingType`, `DisplayName` y `Footprint = (2,2)` seteados. `ConstructionCost` queda vacío (lista serializada, no null) — sin costo hasta que se cargue a mano en el inspector, no se inventó ningún número de balance.
- Un prefab en `Assets/Prefabs/Buildings/Eco/<Nombre>.prefab` (carpeta ya existente en el proyecto, categoría "Eco" de la taxonomía de edificios).

Estructura del prefab:
- **Root** (layer `Buildings`, nuevo — ver más abajo): `BoxCollider` (2 x 1.5 x 2, mismo GameObject que `UnitSelectionHandler` para que la selección por raycast lo encuentre), [BuildingPlacement](../Buildings/BuildingPlacement.md) (agrega `NavMeshObstacle` automáticamente y lo referencia al `BuildingDataSO`), [DropOffBuilding](../Buildings/DropOffBuilding.md) (`acceptedResources` = `[Wood]` o `[Food]` según el edificio), [Health](../_CORE%20Scripts/Health.md) (100 HP default), `UnitSelectionHandler` (`unitType = Building`, sin `selectionRing` — campo null-safe).
- **Hijo "Visual"**: cubo placeholder escalado 2x1.5x2, sin collider propio (el collider vive en el root a propósito, ver arriba). Solo para ver el footprint en escena hasta que haya arte real.

**Por qué el collider no está en el mismo objeto que el visual escalado**: si el root tuviera `localScale` 2x2 en vez de 1x1x1, el tamaño del `NavMeshObstacle` que configura `BuildingPlacement` (calculado directamente en unidades de footprint, sin asumir escala) se multiplicaría por esa escala y el área tallada en el NavMesh terminaría siendo el doble de grande. Por eso el root queda siempre en escala (1,1,1) y el tamaño real (collider, obstáculo) se define explícitamente en unidades de mundo; el visual escalado es un hijo aparte, puramente cosmético.

**Layer `Buildings` (nuevo, índice 10)**: agregado en `ProjectSettings/TagManager.asset`, junto a `Resources` (mismo criterio: capa dedicada por categoría de entidad, sin pisar `Units`/`Interactables`). `SelectionManager.selectionMask` en la escena se actualizó para incluir `Buildings` además de `Units` (era `64`, pasó a `1088`) — si no, los edificios quedarían invisibles para el click de selección.

Si un prefab con ese nombre ya existe, lo saltea con warning. `TownCenter` y `Barracks` (producción, no drop-off puro) quedan para cuando se aborde esa parte del roadmap (Fase 2) — no los genera este script.
