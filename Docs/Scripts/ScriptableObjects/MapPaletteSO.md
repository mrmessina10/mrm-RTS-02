# MapPaletteSO

`Assets/_Scripts/ScriptableObjects/MapPaletteSO.cs`

Catálogo de prefabs que el editor de mapas puede colocar. Cada `MapPaletteEntry` tiene nombre, prefab principal, variantes, categoría (`MapObjectCategory`) y sus parámetros de pincel: `Spacing` (distancia mínima a cualquier otro objeto del mapa), `ScaleRange` (escala aleatoria sobre la del prefab), `VerticalOffset` (para prefabs con pivot en el centro, como los nodos de recurso hechos con una primitiva), `RandomYaw` y `SnapToGrid`.

La paleta por defecto (`Assets/_Scripts/ScriptableObjects/AssetsFromSO/MapPalette_Default.asset`) la genera [MapEditorAssetGenerator](../Editor/MapEditor/MapEditorAssetGenerator.md) con placeholders. Para sumar contenido real (árboles con modelo, casas, props) alcanza con agregar entradas desde el inspector: la consume [ObjectBrushTool](../Editor/MapEditor/ObjectBrushTool.md), que no conoce ningún prefab concreto.

## Variantes

`Variants` es la lista de prefabs alternativos de una entrada: **funcionalmente iguales** al principal, con otro aspecto. Una entrada con variantes sigue siendo un solo ítem de la paleta (el principal da la miniatura y el nombre) y comparte sus parámetros de pincel; cada vez que se coloca un objeto se sortea uno entre el principal y las variantes (`GetPrefabs()`).

Es el mecanismo para la variedad de árboles. Todos los árboles del juego son recolectables: el único prefab de árbol es `WoodNode` (principal de la entrada "Árbol (madera)") y no existe un árbol decorativo. Para sumar otro aspecto de árbol:

1. Crear el prefab nuevo como **Prefab Variant** de `WoodNode` (click derecho sobre `WoodNode` → Create → Prefab Variant) y cambiarle solo el hijo `Model`. Así hereda `ResourceNode`, collider y layer, y un cambio funcional en `WoodNode` llega a todas las variantes.
2. Agregarlo a `Variants` de la entrada "Árbol (madera)" en `MapPalette_Default.asset`.

Con eso lo usan sin más cambios el pincel de [ObjectBrushTool](../Editor/MapEditor/ObjectBrushTool.md), los depósitos de madera del generador y la capa Bosque (ver [MapGenerationRequest](../Map/Generation/MapGenerationRequest.md)). `FindEntry(prefab)` devuelve la entrada a la que pertenece un prefab, sea el principal o una variante.

Referencia: Unity Manual, "Prefab Variants".
