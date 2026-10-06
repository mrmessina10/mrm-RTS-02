# MapGenerationRequest / MapGenerationCatalog

`Assets/_Scripts/Map/Generation/MapGenerationRequest.cs`

Frontera entre los assets de Unity y el generador.

**MapGenerationRequest** — todo lo que necesita una corrida: tamaño y origen del mapa, semilla, `MaxAttempts`, `MinPlayableArea`, los grupos de reglas y el catálogo. `Create(rules, palette, mapData, seed)` lo arma desde un [MapGenerationRulesSO](../../ScriptableObjects/MapGenerationRulesSO.md), la [paleta](../../ScriptableObjects/MapPaletteSO.md) y el [MapDataSO](../../ScriptableObjects/MapDataSO.md) activo (de ahí salen tamaño y origen). Los grupos de reglas se pasan por referencia; el generador solo los lee.

**MapGenerationCatalog** — lo que el generador puede colocar, resuelto a datos planos. `Build` recorre la paleta y las reglas:

- Cada prefab (principal o variante) de una entrada de categoría `Resource` que implementa `IHarvestable` pasa a ser un nodo de ese `ResourceType`, con una entrada de catálogo propia. No hace falta configurar qué prefab es "el de madera": sale de la interfaz. Lo mismo vale para un prefab recolectable referenciado desde una capa o un conjunto.
- Si una regla referencia el prefab principal de una entrada de paleta, entran también sus variantes. Por eso la capa Bosque lista solo `WoodNode` y aun así mezcla todos los aspectos de árbol que tenga la paleta.
- Cada prefab de una capa de decoración o de un conjunto toma su espaciado, escala, offset vertical y yaw de su entrada de paleta (o valores por defecto si no está en la paleta).
- `BlocksMovement` = el prefab tiene algún `Collider`. Con eso el generador sabe qué objetos cuentan como obstáculo al chequear conectividad.

Las etapas trabajan con índices de catálogo y nunca tocan un `GameObject`; el prefab solo lo usa quien aplica el resultado ([MapGenerationApplier](../../Editor/MapEditor/MapGenerationApplier.md)). Esa separación es la que permite probar el generador fuera de Unity con un catálogo armado a mano.
