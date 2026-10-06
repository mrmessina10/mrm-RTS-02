# MapHeightField

`Assets/_Scripts/Map/Generation/MapHeightField.cs`

Campo de alturas del generador: una grilla regular con 2 muestras por celda (la misma densidad que el heightmap que crea [MapSceneSetup](../../Editor/MapEditor/MapSceneSetup.md)), en coordenadas locales del mapa y alturas de mundo. Es el equivalente en datos planos de lo que [TerrainBrushUtility](../../Editor/MapEditor/TerrainBrushUtility.md) hace sobre un `TerrainData`, para que el generador no dependa de un `Terrain` ni del editor.

- `Sample(x, z)` — altura interpolada bilineal. `GetCellHeight` / `GetCellSlope` — altura del centro y pendiente máxima (en grados) de una celda de grilla, que es con lo que se decide la caminabilidad.
- `Modify(min, max, operation)` / `ModifyAll` — recorre las muestras de un rectángulo y aplica una operación `(x, z, alturaActual) → alturaNueva`, la misma forma que `TerrainBrushUtility.ModifyHeights`.
- `GetFalloff(distanciaNormalizada, dureza)` — el perfil de pincel del editor. La fórmula vive acá y `TerrainBrushUtility.GetFalloff` la llama, así pincel y generador comparten una sola definición.
- `ApplyRamp` — rampa lineal entre dos puntos con bordes fundidos, igual que la del editor pero con las alturas de los extremos explícitas.
- `StampPolyline` — para cada muestra de una grilla guarda la distancia a una polilínea y, opcionalmente, en qué punto de la polilínea está el más cercano. Es la base del río, del nivelado de rutas y de la grilla de distancia al camino: calcular la distancia una vez y aplicar el efecto en una sola pasada evita que tramos superpuestos se acumulen.

El aplicador remuestrea este campo a la resolución real del heightmap ([MapGenerationApplier](../../Editor/MapEditor/MapGenerationApplier.md)).
