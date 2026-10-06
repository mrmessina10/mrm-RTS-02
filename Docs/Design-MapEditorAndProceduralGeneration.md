# Diseño — Editor de Mapas y Generación Procedural

Bases arquitectónicas para las dos herramientas que necesita el mapa/camino del juego: un editor de Unity para autoría manual y un generador procedural que produzca el mismo tipo de dato. Ver [Design-WavesAndCaravans.md §8](Design-WavesAndCaravans.md#8-mapa-y-camino) (declara la dependencia editor→generador) y [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) (ambas herramientas quedan fuera del vertical slice actual).

## Estado

🔶 Editor de mapas en implementación (2026-10-06): hay una primera versión en `Assets/_Scripts/Map/`, `Assets/_Scripts/Editor/MapEditor/` y los SO `MapDataSO`/`MapPaletteSO`, todavía sin commitear, documentada por script en `Docs/Scripts/Map/` y `Docs/Scripts/Editor/MapEditor/` (entrada: [MapEditorWindow.md](Scripts/Editor/MapEditor/MapEditorWindow.md)). Lo implementado se aparta del planteo original en tres puntos, marcados abajo como *Implementado*. 🔶 Generador procedural con una primera versión (2026-10-06) en `Assets/_Scripts/Map/Generation/`, con sus reglas en `MapGenerationRulesSO` y su panel dentro de la ventana del editor de mapas; detalle en la sección 4 (*Implementado*) y entrada por script en [MapGenerator.md](Scripts/Map/Generation/MapGenerator.md). Genera en el editor sobre el mapa activo; la generación en runtime todavía no está. La Fase 7 del roadmap usa un mapa fijo, que ahora se puede armar a mano o generar y retocar.

## 1. Orden de implementación

1. `MapData` (formato de datos común, sección 2).
2. Editor de mapas (autoría manual sobre `MapData`) — ya sirve por sí solo para armar el mapa fijo de la Fase 7.
3. Generador procedural, consumiendo/produciendo el mismo `MapData`.

## 2. Formato de datos común — MapData

`ScriptableObject` que guarda: lista de spawn points, punto(s) de llegada (HQ), waypoints del camino, nodos de recursos (posición + `ResourceType`), obstáculos (posición + tipo/footprint). Sigue el criterio ya establecido en el proyecto de que todo dato read-only en runtime vive en un SO. Es la superficie común entre editor manual y generador procedural: el mismo asset puede salir de cualquiera de los dos.

*Implementado* ([MapDataSO.md](Scripts/ScriptableObjects/MapDataSO.md)): además guarda tamaño/origen, referencia al `TerrainData`, nivel de agua y una máscara de celdas edificables. Los "obstáculos" quedaron generalizados como objetos colocados con categoría (`Resource`/`Nature`/`ManMade`) y prefab, tomados de una paleta ([MapPaletteSO.md](Scripts/ScriptableObjects/MapPaletteSO.md)).

## 3. Editor de mapas

- **APIs**: `EditorWindow` + `SceneView.duringSceneGui` + `Handles`/`Gizmos` para pintar de forma interactiva sobre la Scene view (Unity Manual: "Editor windows", "SceneView"). Extiende el mismo paradigma que ya usan los scripts de `Assets/_Scripts/Editor/`, hoy limitados a acciones one-shot vía `[MenuItem]`.
- **Grid**: lógico, no visual — reusa el mismo criterio de "1 unidad = 1 celda" que ya existe en `BuildingPlacement.GetFootprintOrigin`. Da snapping y determinismo sin que el terreno se vea cuadriculado.
- **Terreno**: `Terrain` nativo de Unity en vez de tiles modulares — heightmap + splatmap (capas = tipos de terreno) scripteables vía `TerrainData.SetHeights`/`SetAlphamaps` (Unity Manual: "Terrain scripting"), y bake directo a NavMesh. Menos trabajo que armar tiles a mano para un prototipo.
- **Modos de pincel**: elevación, tipo de terreno, nodo de recurso, obstáculo, spawn point, HQ point — cada uno escribe sobre el `MapData`, no directo en GameObjects sueltos de la escena.

*Implementado*:

- **Escena como copia de trabajo.** Objetos, marcadores y caminos son GameObjects bajo un `MapRoot` con contenedores por categoría (no sueltos), y se vuelcan al `MapData` al guardar la escena o a pedido; el `MapData` se puede volver a cargar en la escena. Relieve, texturas y máscara edificable sí se escriben directo sobre sus assets. Razón y detalle en [MapSceneSync.md](Scripts/Editor/MapEditor/MapSceneSync.md).
- **Agua con nivel global.** Un único plano de agua por mapa: hay agua donde el terreno queda por debajo de ese nivel (convención de RTS con nivel del mar). Consecuencia: una depresión más profunda que el nivel de agua se inunda; no conviven lagos a ras del suelo y barrancos secos más profundos en el mismo mapa sin pasar a una máscara de agua por celda.
- **Zona edificable como máscara pintada**, no solo como offset del camino: cubre tanto "zona designada del jugador" como "todo menos el camino". El offset del camino quedó como una acción de la herramienta de caminos que escribe sobre esa máscara.
- **Marcadores del loop**: spawn de enemigos (N), fin de oleada (0–1, extremo B del layout A→B) e inicio del jugador/HQ (1). Ver [MapMarker.md](Scripts/Map/MapMarker.md).
- Herramientas: elevación (incluye mesetas/barrancos por niveles y rampas), texturas, agua, objetos (individual, pincel, aldea), caminos, marcadores, zona edificable, más bake de NavMesh y validación del mapa desde la misma ventana.

## 4. Generación procedural

El camino es el núcleo del generador, no el terreno — es lo que determina el gameplay.

- **Modelo**: grafo de N spawn points que convergen a 1 o más HQ.
- **Las dos variantes de layout pedidas son el mismo generador, con distinta ubicación del HQ**:
  - Spawn(s) → HQ en la punta del camino (caso base, HQ como nodo terminal).
  - A → B con el jugador en el medio: mismo path A-B, HQ offseteado perpendicularmente a un tramo intermedio, con su propio acceso corto al camino principal.
- **Trazado del camino**: A* sobre el grid lógico con costo perturbado aleatoriamente entre celdas, en vez de una recta — da un camino orgánico y permite esquivar nodos/obstáculos ya colocados.
- **Ancho de camino / zona no edificable**: offset perpendicular a cada segmento del path, excluye esas celdas de `BuildingPlacement.IsAreaBuildable`.
- **Elevación**: Perlin/Simplex noise sobre el heightmap (`Mathf.PerlinNoise`, Unity Scripting API).
- **Distribución de nodos de recursos/obstáculos**: Poisson disk sampling con distancia mínima al path y al HQ (Bridson, "Fast Poisson Disk Sampling in Arbitrary Dimensions", 2007) — evita clusters o nodos pegados al HQ que rompan el balance.
- **NavMesh**: como el mapa se genera (no es estático de escena), el bake tiene que ser runtime vía `NavMeshSurface.BuildNavMesh()` del paquete AI Navigation (`com.unity.ai.navigation`, Unity Manual), no el bake de editor que se usa hoy.

*Implementado* (entrada: [MapGenerator.md](Scripts/Map/Generation/MapGenerator.md); uso: [MapGeneratorPanel.md](Scripts/Editor/MapEditor/MapGeneratorPanel.md)):

- **Semilla + reglas + tamaño = mapa.** El generador es determinista (RNG y ruido propios, sin `UnityEngine.Random` ni `Mathf.PerlinNoise`) y trabaja sobre datos planos; el editor solo vuelca el resultado al `Terrain` y al `MapData`. Por eso el mismo núcleo sirve para generar en runtime cuando haga falta.
- **Reglas como asset.** [MapGenerationRulesSO](Scripts/ScriptableObjects/MapGenerationRulesSO.md) guarda las dos cosas: las reglas de estructura que todo mapa tiene que cumplir y los rangos dentro de los que varía cada semilla. Un asset por arquetipo de mapa (una run roguelike puede usar reglas distintas por etapa).
- **Generar y verificar.** Cada intento se arma por construcción respetando las reglas y al final se verifica ([MapRuleChecker](Scripts/Map/Generation/MapRuleChecker.md)); si rompe una regla obligatoria se descarta y se prueba otro intento de la misma semilla (hasta `MaxAttempts`).
- **Orden de etapas**: layout → relieve y agua → rutas (A*) → nivelado del terreno bajo el camino → máscaras → objetos → reglas. El camino sigue siendo el núcleo, pero se traza sobre el relieve ya armado para poder rodear mesetas y agua, y después el terreno se adapta a él (rampas donde corta un acantilado, vado donde cruza agua).

Reglas de estructura (obligatorias, valores por defecto del asset):

| Regla | Qué garantiza |
|---|---|
| Inicio del jugador en suelo firme, plano y seco | El HQ siempre tiene dónde apoyarse: relieve y agua se anulan en un radio de 10 celdas. |
| Zona de inicio ≥ 60 % edificable | Lugar para la base inicial. |
| Todos los spawns (y el fin de oleada) conectados a pie con el HQ | Ninguna oleada ni caravana queda sin ruta. |
| Spawn a ≥ 0,4 del lado del mapa del HQ y ruta ≥ 0,4 | Tiempo de reacción y profundidad para defender. |
| Tramo final compartido ≥ 0,12 del lado del mapa | Con varios spawns, todo el tráfico pasa por un mismo tramo antes de la base: siempre hay un frente que defender. |
| Camino transitable de punta a punta | Sin agua, acantilados ni objetos sobre la traza; pendiente a lo largo ≤ 0,2. |
| Depósito inicial de cada recurso junto al HQ | El arranque de [Design-EconomyBalance.md §1](Design-EconomyBalance.md#1-supuestos-del-diagrama-de-wave-1): capital con fricción ≈ 0. |
| ≥ 2 depósitos grandes de cada recurso, alcanzables y con claro edificable al lado | La expansión con Lumbermill/Farm siempre es posible; el primero de cada recurso cae en el tercio más cercano. |
| Área jugable ≥ 30 % del mapa | Alcanzable desde el HQ y edificable. |

Variedad (sorteada por semilla dentro de los rangos del asset): layout (Spawn → HQ o A → B con el HQ al costado), borde de entrada, 1–2 spawns, sinuosidad y ancho del camino, cantidad/tamaño de mesetas de 1 o 2 niveles con 1–2 rampas, agua (nada, lagos, río con vados o costa), cantidad/tamaño/ubicación de depósitos, aldeas y ruinas, y capas de decoración en manchones.

Diferencias con el planteo de arriba:

- El **HQ del layout A → B** se decide antes de trazar el camino (punto de empalme cerca del centro + desplazamiento perpendicular) y el camino se obliga a pasar por ese empalme; así relieve y agua pueden reservar la zona de inicio antes de existir el camino.
- La **elevación** no es solo ruido: ruido suave de base más mesetas por niveles discretos con acantilado, la misma convención de la herramienta de Elevación.
- El **Poisson disk** se usa para la decoración; los recursos van en depósitos compactos ubicados por distancia al HQ, que es lo que pide el balance económico.
- El **NavMesh** se sigue horneando en el editor (el panel dispara el bake después de generar). El bake en runtime queda para cuando se genere en runtime.

Verificado fuera de Unity sobre 300 semillas por tamaño con las reglas por defecto: 64, 128 y 256 dan 300/300 mapas válidos (1,06–1,09 intentos promedio en 128/256; 2,2 en 64, donde las reglas por defecto quedan justas de espacio y casi no entran ríos). No está probado dentro de Unity: falta confirmar que la conectividad que calcula el generador coincide con la del NavMesh horneado (para eso sigue estando `Validar mapa`).

## 5. Compatibilidad con arquitectura existente

- El grid lógico de `BuildingPlacement` se reusa tal cual, no se duplica un sistema de grilla paralelo.
- `MapData` sigue el patrón SO de datos read-only ya establecido en el proyecto.
- `ResourceNode`/`ResourceType` no cambian — el generador solo decide dónde instanciarlos.
- El patrón de `Assets/_Scripts/Editor/` se extiende (ventana con modo pincel interactivo) en vez de reemplazarse.

## 6. Abierto / pendiente de afinar

- `Terrain` nativo vs tiles modulares: decisión tentativa: a confirmar si en algún momento el juego necesita mapas grandes o streaming.
- Costo de performance del bake de NavMesh en runtime para el tamaño de mapa del prototipo — sin medir todavía.
- Ubicación/cantidad de HQs si eventualmente hay multi-facción (hoy fuera de scope, ver notas de [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md)).
- Balance de la generación (frecuencia de curvas del camino, densidad de nodos/obstáculos): se resuelve en iteración de playtesting, no es parte de este doc. Los valores viven en el asset de reglas y la tanda de semillas del panel mide qué regla descarta más intentos.
- Generación en runtime: falta un aplicador que no dependa del editor (hoy `MapMarker`/`MapPath` se escriben por `SerializedObject`, y pintar texturas usa `TerrainBrushUtility`, que es de editor) y el bake de NavMesh en runtime.
- Stone: las reglas por defecto solo piden Wood y Food porque no hay prefab de nodo de piedra; al sumarlo a la paleta alcanza con agregar una regla de recurso.
- Reglas por tamaño de mapa: hoy un mismo asset sirve para los tres tamaños porque casi todo está expresado como fracción del lado; para mapas de 64 conviene un asset propio con menos depósitos.
- Capacidad por nodo: el generador decide cuántos nodos tiene un depósito, no cuánto rinde cada uno (`ResourceNode.maxCapacity` es del prefab).
