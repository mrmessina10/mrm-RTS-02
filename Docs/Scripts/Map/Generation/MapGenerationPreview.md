# MapGenerationPreview

`Assets/_Scripts/Map/Generation/MapGenerationPreview.cs`

Dibuja un [GeneratedMap](GeneratedMap.md) como imagen cenital para revisar una semilla sin volcarla a la escena. `Render` devuelve `Color32[]` con la fila 0 en el borde sur (el orden de `Texture2D.SetPixels32`); [MapGeneratorPanel](../../Editor/MapEditor/MapGeneratorPanel.md) lo carga en una textura.

Qué se ve:

- Relieve con sombreado por pendiente (luz desde el noroeste) y pasto más claro en altura.
- Agua en azul, más oscura con la profundidad.
- Acantilados (celdas no caminables por pendiente) en gris.
- Camino en ocre.
- Suelo caminable pero **inalcanzable desde el HQ** teñido de violeta: una meseta sin rampa, una orilla sin vado. Es la forma rápida de ver si un accidente partió el mapa.
- Objetos como puntos: madera verde oscuro, comida rosa, piedra gris claro, naturaleza verde apagado, construcciones marrón.
- Marcadores con el mismo color que su gizmo (`MapMarker.GetColor`): spawn rojo, fin de oleada naranja, HQ azul.

La imagen apunta a ~512 px de lado: 8 px por celda en mapas de 64, 4 en 128 y 2 en 256.
