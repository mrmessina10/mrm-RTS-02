# ResourceNodePrefabGenerator

`Assets/_Scripts/Editor/ResourceNodePrefabGenerator.cs`

Script de Editor (vive en carpeta `Editor/`, no se compila al build). Menú `Tools/RTS/Generate Resource Node Prefabs` genera dos prefabs placeholder en `Assets/Prefabs/ResourceNodes/`:

- `WoodNode` (primitiva Cylinder) con `ResourceType.Wood`.
- `FoodNode` (primitiva Sphere) con `ResourceType.Food`.

Ambos con el componente [ResourceNode](../Resources/ResourceNode.md) (el mismo script sirve para cualquier tipo de recurso — solo cambia el `ResourceType` configurado) y layer `Resources` (índice 9, agregado en `ProjectSettings/TagManager.asset`), layer dedicado y separado de `Interactables` (que se mantiene libre para otros usos futuros: edificios interactuables, etc.). Para que el worker los encuentre, `WorkerController.ResourceMask` debe incluir `Resources`; para que el click-to-target del jugador los detecte, `SelectionManager.interactablesMask` (campo del inspector, en la escena) también debe tildar `Resources` además de lo que ya tenga.

`ResourceType` es una propiedad `[field: SerializeField]` con setter privado — no se puede asignar desde afuera de la clase por código normal, así que se escribe vía `SerializedObject.FindProperty("<ResourceType>k__BackingField")` (nombre que el compilador genera para el backing field de una auto-propiedad). `maxCapacity` queda en su valor default (500) del script; ajustarlo a mano en cada prefab si hace falta balance distinto por tipo.

Si un prefab con ese nombre ya existe en la carpeta, lo saltea con un warning en vez de sobreescribirlo — para no pisar ediciones manuales hechas después de una corrida anterior.

Es solo el placeholder inicial: falta reemplazar la malla/material por arte real, y decidir si `maxCapacity` debe diferir por tipo de recurso.
