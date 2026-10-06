# TerrainSculptTool

`Assets/_Scripts/Editor/MapEditor/TerrainSculptTool.cs`

Herramienta **Elevación**. Modos:

| Modo | Qué hace |
|---|---|
| Subir / Bajar | Suma o resta altura a `Fuerza` unidades por segundo, ponderado por el pincel. `Shift` invierte. |
| Suavizar | Promedia con los vecinos. |
| Aplanar | Lleva el terreno a la altura del punto donde empezó el trazo. |
| Meseta / Barranco | Lleva el terreno a `Nivel × Altura por nivel` (0 = suelo base). Niveles discretos al estilo de los editores de RTS clásicos: el desnivel queda como acantilado, que el NavMesh descarta por pendiente. Niveles negativos cavan barrancos. |
| Rampa | Click en el inicio, arrastrar, soltar en el final: interpola la altura entre ambos puntos con el ancho indicado. Es la forma de dar acceso caminable a una meseta. |
| Ruido | Suma ruido Perlin (`Mathf.PerlinNoise`) para romper superficies planas. |

Un barranco cuya altura objetivo queda por debajo del nivel de agua del mapa se inunda (el agua es un plano global, ver [WaterTool](WaterTool.md)); la herramienta lo avisa. Para barrancos secos hay que bajar el nivel de agua del mapa.

Toda la matemática está en [TerrainBrushUtility](TerrainBrushUtility.md); esta clase solo elige la operación según el modo.

El suelo base está en Y = 0 de mundo y el terreno admite bajar hasta Y = −10 y subir hasta Y = +30 (ver [MapSceneSetup](MapSceneSetup.md)).
