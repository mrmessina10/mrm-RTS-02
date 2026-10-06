# GeneratedMap

`Assets/_Scripts/Map/Generation/GeneratedMap.cs`

Resultado de [MapGenerator](MapGenerator.md), en posiciones de mundo y sin referencias a la escena:

- `Heights` ([MapHeightField](MapHeightField.md)) y `WaterLevel`.
- `Objects`: índice de catálogo, posición, yaw y escala. `Markers`: tipo y posición. `Paths`: ancho y waypoints.
- `TexturePatches`: manchas de tierra (inicio, aldeas) y arena (vados) que el aplicador pinta sobre el auto-texturizado. Guardan un tipo de suelo (`MapGroundPatch`), no un nombre de capa: los nombres de `TerrainLayer` son del editor.
- `NotBuildableCells`: lo que va a la máscara del [MapDataSO](../../ScriptableObjects/MapDataSO.md).
- `WalkableCells`, `ReachableCells`, `PathDistance`: datos de análisis que usa [MapGenerationPreview](MapGenerationPreview.md).
- `Rules`: informe de [MapRuleChecker](MapRuleChecker.md). `IsValid` = ninguna regla obligatoria falló. `AttemptsUsed` y `DiscardedRules` cuentan cuántos intentos hicieron falta y por qué se descartaron los anteriores.
- Resumen de qué salió: `Layout`, `WaterMode`, `PlateauCount`, `Seed`, `Attempt`.

**Orden de `Paths`**: primero la ruta principal (spawn 1 → HQ, o spawn 1 → fin de oleada en el layout A → B), después una ruta completa por cada spawn extra (su tramo propio + el resto de la principal) y, solo en A → B, el acceso del camino al HQ al final. `MapPathEntry` no guarda nombre ni rol, así que el orden es la única identidad.
