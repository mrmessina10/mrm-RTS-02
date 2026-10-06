# MapObjectScatterer

`Assets/_Scripts/Map/Generation/MapObjectScatterer.cs`

Etapa de objetos de [MapGenerator](MapGenerator.md). Coloca en orden de prioridad, de lo que el juego necesita a lo decorativo:

1. **Depósitos iniciales** — un grupo chico de nodos de cada recurso a 9–13 celdas del HQ, fuera del camino y separados entre sí. Es el arranque sin fricción de [Design-EconomyBalance.md §1](../../../Design-EconomyBalance.md#1-supuestos-del-diagrama-de-wave-1).
2. **Depósitos de expansión** — grupos grandes en una corona de distancia al HQ. Se reparten por rondas (uno de cada recurso por vuelta) para que ningún recurso se quede sin lugar, y el primero de cada recurso cae en el tercio más cercano de la corona. Cada depósito reserva un **claro para el drop-off** del lado que mira al HQ: se marca como zona despejada y la decoración no lo puede ocupar.
3. **Conjuntos** — aldeas y ruinas en anillo con las piezas mirando al centro, igual que el modo Aldea de [ObjectBrushTool](../../Editor/MapEditor/ObjectBrushTool.md), más una mancha de tierra debajo.
4. **Capas de dispersión** — muestreo de disco de Poisson sobre todo el mapa, filtrado por ruido para formar manchones (`Clumping`), por cobertura y por distancia al camino y al HQ. No son solo decoración: la capa Bosque coloca `WoodNode`, así que sus árboles son madera recolectable igual que los de un depósito.

Cuando un recurso tiene varios prefabs (variantes de aspecto), cada nodo sortea el suyo, tanto en los depósitos como en las capas: un mismo bosque mezcla todos los aspectos de árbol. Con un solo prefab no se consume ningún número al azar extra, así que sumar la primera variante no altera la ubicación de los depósitos de una semilla.

**Validaciones de cada objeto** (`TryPlace`): dentro del margen del mapa, celda caminable y con pendiente menor a la permitida, fuera de las zonas despejadas (camino, inicio, spawns, rampas, vados, claros de drop-off) y a más del espaciado de cualquier otro objeto. El espaciado que vale es el mayor de los dos, así una casa no queda con un arbusto adentro. Las consultas de vecindad usan una grilla de buckets.

Los objetos cuyo prefab tiene collider marcan sus celdas como obstáculo; con eso [MapRuleChecker](MapRuleChecker.md) vuelve a calcular la conectividad con los objetos puestos.

La cantidad de nodos por depósito es la palanca del generador sobre la economía: cuánto rinde cada nodo lo define el prefab (`ResourceNode`). La otra palanca sobre la madera es la capa Bosque (`Coverage`, `Spacing`, `Clumping`): al ser árboles recolectables, cuánto bosque hay es cuánta madera total tiene el mapa.

Fuente: Robert Bridson, "Fast Poisson Disk Sampling in Arbitrary Dimensions" (SIGGRAPH 2007) — <https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf>.
