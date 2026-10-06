# MapRuleChecker

`Assets/_Scripts/Map/Generation/MapRuleChecker.cs`

Última etapa de [MapGenerator](MapGenerator.md): comprueba sobre el mapa ya armado las reglas de estructura y devuelve una lista de `MapRuleResult` (regla, pasó/no pasó, obligatoria o aviso, detalle con los números medidos). Una regla obligatoria rota descarta el intento.

| Regla | Tipo | Parámetro ([MapGenerationRulesSO](../../ScriptableObjects/MapGenerationRulesSO.md)) |
|---|---|---|
| Inicio del jugador en suelo firme | Obligatoria | — |
| Spawns conectados con el HQ | Obligatoria | — |
| Fin de oleada conectado (layout A → B) | Obligatoria | — |
| Distancia mínima spawn → HQ | Obligatoria | `Layout.MinSpawnDistance` |
| Largo mínimo de ruta hasta la zona de defensa | Obligatoria | `Layout.MinRouteLength` |
| Tramo final compartido (con más de un spawn) | Obligatoria | `Layout.MinSharedStretch` |
| Camino transitable de punta a punta | Obligatoria | — |
| Zona de inicio edificable | Obligatoria | `Start.ClearRadius`, `Start.MinBuildableRatio` |
| Depósito inicial de cada recurso | Obligatoria | `Resources[].StarterNodes` |
| Expansiones de cada recurso, alcanzables y con lugar para el drop-off | Obligatoria | `Resources[].ExpansionClusters` (mínimo), `ClusterSize` |
| Área jugable | Obligatoria | `MinPlayableArea` |
| Falta el prefab de un recurso en la paleta | Aviso | — |
| Entraron menos spawns de los pedidos | Aviso | — |
| Más del 35 % del mapa bajo el agua | Aviso | — |

"Zona de defensa" es el final del camino en el layout Spawn → HQ y el punto de empalme del acceso al HQ en el layout A → B.

"Edificable" acá es: no marcado en la máscara, caminable y sin obstáculo. "Alcanzable" es el relleno por inundación desde el HQ con los objetos ya colocados ([MapGenerationContext](MapGenerationContext.md)).

**Relación con [MapValidator](../../Editor/MapEditor/MapValidator.md).** Los dos chequean lo mismo en momentos distintos: este corre sin NavMesh, sobre los datos del generador, y decide si un intento sirve; `MapValidator` corre sobre la escena con el NavMesh horneado y es la verificación final, también para mapas hechos a mano. Que un mapa pase acá no reemplaza validar después del bake.

Una regla de recurso sin prefab en la paleta (hoy Stone) no falla: se informa como aviso y se ignora, para que el generador no quede sin poder producir ningún mapa.
