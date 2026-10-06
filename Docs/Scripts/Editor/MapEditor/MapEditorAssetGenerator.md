# MapEditorAssetGenerator

`Assets/_Scripts/Editor/MapEditor/MapEditorAssetGenerator.cs`

Script de Editor. Menú `Tools/RTS/Generate Map Editor Assets`; también lo invoca [MapSceneSetup](MapSceneSetup.md) al crear un mapa. Mismo criterio que los otros generadores de `Assets/_Scripts/Editor/`: placeholders con primitivas, sin sobreescribir lo que ya exista.

Genera:

- **Capas de terreno** en `Assets/Materials/Terrain/`: Grass, Dirt, Rock, Sand, Road. Cada una con una textura PNG procedural de 128 px (color base + ruido Perlin + moteado), hecha tileable mezclando cuatro muestras desplazadas un período.
- **Material de agua** `Assets/Materials/Water.mat` (URP Lit transparente).
- **Prefabs** en `Assets/Prefabs/MapFeatures/Nature` (roca chica/grande, matorral) y `.../ManMade` (casa chica/grande, pozo, cerca, parva, ruina), con materiales de color en `Assets/Materials/MapFeatures/`. Las piezas que deben bloquear el paso conservan su collider; las decorativas (copas, techos, matorral) no.
- **Paleta** `MapPalette_Default.asset` ([MapPaletteSO](../../ScriptableObjects/MapPaletteSO.md)) con esos prefabs más los nodos de recurso existentes (`WoodNode`, `FoodNode`). No se genera ningún árbol: el único prefab de árbol es `WoodNode` y sus futuras variantes de aspecto se agregan a mano en la entrada "Árbol (madera)" (ver [MapPaletteSO](../../ScriptableObjects/MapPaletteSO.md#variantes)).

- **Reglas de generación** `MapGenerationRules_Default.asset` ([MapGenerationRulesSO](../../ScriptableObjects/MapGenerationRulesSO.md)): los valores por defecto del SO más tres capas de dispersión (Bosque con `WoodNode`, Rocas, Matorrales) y dos conjuntos (Aldea, Ruinas) armados con los prefabs de arriba. También lo crea el botón "Crear" de [MapGeneratorPanel](MapGeneratorPanel.md).

Si la paleta o las reglas ya existen no se regeneran: para reemplazar placeholders por arte real o ajustar la generación se edita el asset a mano.

No se genera nodo de Stone: es recolectable por diseño pero todavía no tiene prefab, drop-off ni animación de recolección; cuando exista se suma a la paleta como una entrada más. El Oro no lleva nodo: solo entra por comercio con caravanas.

Referencia del ruido tileable: Paul Bourke, "Tiling textures on the plane" — <https://paulbourke.net/texture_colour/tiling/>.
