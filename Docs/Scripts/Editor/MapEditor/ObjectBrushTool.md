# ObjectBrushTool

`Assets/_Scripts/Editor/MapEditor/ObjectBrushTool.cs`

Herramienta **Objetos**. Coloca instancias de prefab de la [paleta](../../ScriptableObjects/MapPaletteSO.md) bajo el contenedor de su categoría en [MapRoot](../../Map/MapRoot.md) (Recursos / Naturaleza / Construcciones).

| Modo | Qué hace |
|---|---|
| Individual | Un objeto por click, en el punto exacto. |
| Pincel | Mientras se arrastra, tira `Intentos por segundo` posiciones al azar dentro del radio y coloca las que son válidas, eligiendo al azar entre las entradas seleccionadas. |
| Aldea | Un click reparte N construcciones en anillo, orientadas hacia el centro. |

`Shift` + arrastrar borra lo que haya dentro del radio (de la categoría activa, o de todas).

Una posición del pincel es válida si está dentro del mapa, sobre el nivel de agua, con pendiente menor a la máxima y a más de `Spacing` de cualquier otro objeto del mapa. Ese rechazo por distancia mínima (dart throwing) es lo que da bosques y campos de rocas con distribución orgánica en vez de amontonada, y hace que insistir sobre la misma zona la sature en vez de apilar objetos. El generador procedural va a usar la variante eficiente del mismo criterio (Poisson disk de Bridson, ver [doc de diseño](../../../Design-MapEditorAndProceduralGeneration.md#4-generación-procedural)).

Una entrada de paleta con variantes (hoy preparada para los árboles) se muestra como un solo ítem con la cantidad de prefabs entre paréntesis, y cada objeto colocado sortea su prefab entre el principal y las variantes.

Los objetos son instancias de prefab normales (`PrefabUtility.InstantiatePrefab`): se pueden seleccionar, mover y rotar a mano con la herramienta en "Ninguna", y [MapSceneSync](MapSceneSync.md) los captura igual.

Los nodos de recurso están en el layer `Resources`, que el bake del NavMesh ignora (mismo comportamiento que ya tenían: no tallan el NavMesh y la colocación de edificios los detecta por collider). Naturaleza y Construcciones con collider sí entran al bake como obstáculos.
