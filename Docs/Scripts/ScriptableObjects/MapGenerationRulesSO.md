# MapGenerationRulesSO

`Assets/_Scripts/ScriptableObjects/MapGenerationRulesSO.cs`

Reglas de generación procedural de mapas. Guarda dos cosas en un mismo asset: la **estructura** que todo mapa tiene que cumplir y los **rangos** dentro de los que puede variar cada semilla. El generador ([MapGenerator](../Map/Generation/MapGenerator.md)) solo lo lee; sigue la convención del proyecto de que todo dato de solo lectura en runtime vive en un SO.

Un asset = un arquetipo de mapa. El de por defecto (`MapGenerationRules_Default.asset`) lo crea [MapEditorAssetGenerator](../Editor/MapEditor/MapEditorAssetGenerator.md); se pueden crear más con `Create → ScriptableObjects → MapGenerationRulesSO` para tener, por ejemplo, un arquetipo de islas, uno de mesetas o uno más difícil con tres spawns.

Los rangos son `Vector2` / `Vector2Int` con (mínimo, máximo). Las distancias que tienen que escalar con el mapa están expresadas como **fracción del lado menor**; las que dependen del tamaño de un edificio o una unidad están en celdas. Cada campo tiene su tooltip.

| Grupo | Qué define |
|---|---|
| `Layout` | Peso de cada layout, cantidad de spawns, margen del borde, dónde cae el HQ, y las reglas de distancia: spawn → HQ, largo de ruta, tramo final compartido. |
| `Start` | Radio de la zona de inicio y su mínimo edificable, dónde termina el camino, radio despejado de los spawns. |
| `Terrain` | Relieve de base, cantidad/tamaño/altura de mesetas, segundo nivel, rampas, pendiente caminable. |
| `Water` | Nivel de agua, peso de cada modo (nada, lagos, río, costa) y los parámetros de cada uno, incluidos los vados. |
| `Path` | Ancho, sinuosidad, cuánto cuesta cruzar acantilado o agua, pendiente máxima, margen no edificable, separación de waypoints. |
| `Resources` | Una regla por recurso recolectable: depósito inicial y depósitos de expansión (cantidad, tamaño, distancia). El mínimo de `ExpansionClusters` es obligatorio. |
| `Scatter` | Capas de dispersión (bosque, rocas, matorrales): prefabs, espaciado, cobertura, manchones, distancias al camino y al HQ. |
| `Landmarks` | Conjuntos en anillo (aldeas, ruinas): prefabs, cantidad, piezas, radio. |
| `MaxAttempts`, `MinPlayableArea` | Intentos por semilla y fracción mínima del mapa alcanzable y edificable. |

Qué reglas son obligatorias y cómo se miden: [MapRuleChecker](../Map/Generation/MapRuleChecker.md).

Las capas de decoración y los conjuntos referencian **prefabs**; los parámetros de pincel de cada uno (espaciado, escala, yaw) se toman de su entrada en la [paleta](MapPaletteSO.md). Los nodos de recurso de los depósitos no se referencian: el generador los encuentra en la paleta por su `ResourceType` (`IHarvestable`). La capa Bosque sí referencia `WoodNode`: no hay árbol decorativo, todo árbol es recolectable. Referenciar el prefab principal de una entrada de paleta incluye sus variantes.

Las clases de grupo (`MapLayoutRules`, `MapTerrainRules`, etc.) son `[Serializable]` con campos públicos, igual que `MapPaletteEntry`: así el generador puede recibirlas sin depender del `ScriptableObject`.

Por defecto hay reglas de Wood y Food. Stone no tiene regla porque todavía no existe su prefab de nodo; el Oro no lleva porque no se recolecta.
