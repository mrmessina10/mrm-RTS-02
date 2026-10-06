# MapLayoutPlanner

`Assets/_Scripts/Map/Generation/MapLayoutPlanner.cs`

Etapa 1 de [MapGenerator](MapGenerator.md): decide el esqueleto del mapa antes de que exista relieve. Produce un `MapLayout` y registra las zonas reservadas en el [contexto](MapGenerationContext.md).

Todo se mide desde el **borde de entrada** (sorteado entre los cuatro): `GetPoint(lateral, depth)` da un punto con `lateral` recorriendo ese borde y `depth` alejándose de él, los dos de 0 a 1. Así las reglas se escriben una sola vez y sirven para cualquier orientación.

**Layouts** (los dos de [Design-MapEditorAndProceduralGeneration.md §4](../../../Design-MapEditorAndProceduralGeneration.md#4-generación-procedural)):

- `PathToStart` — spawn sobre el borde de entrada, HQ en la punta del camino, a una profundidad de 0,62–0,8 del mapa.
- `PassThrough` — spawn en un borde, fin de oleada en el opuesto, un punto de **empalme** cerca del centro por donde el camino tiene que pasar, y el HQ desplazado en perpendicular a 16–24 celdas de ese empalme (del lado que entre en el mapa).

**Spawns extra**: sobre el borde de entrada o sobre el primer 40 % de los bordes laterales, nunca del lado del HQ. Se aceptan solo si respetan la distancia mínima al HQ y la separación entre spawns; si no entra, el mapa queda con menos spawns de los pedidos y [MapRuleChecker](MapRuleChecker.md) lo informa como aviso.

Los spawns y el fin de oleada se apoyan sobre la línea de `EdgeMargin`, no sobre el borde mismo, para que haya terreno caminable alrededor.
