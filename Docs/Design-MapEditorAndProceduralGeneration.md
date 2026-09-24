# Diseño — Editor de Mapas y Generación Procedural

Bases arquitectónicas para las dos herramientas que necesita el mapa/camino del juego: un editor de Unity para autoría manual y un generador procedural que produzca el mismo tipo de dato. Ver [Design-WavesAndCaravans.md §8](Design-WavesAndCaravans.md#8-mapa-y-camino) (declara la dependencia editor→generador) y [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) (ambas herramientas quedan fuera del vertical slice actual).

## Estado

⬜ No empezado. Este doc fija el diseño para cuando se aborde en una fase posterior — no es trabajo activo. La Fase 7 del roadmap usa mapa fijo armado a mano mientras tanto.

## 1. Orden de implementación

1. `MapData` (formato de datos común, sección 2).
2. Editor de mapas (autoría manual sobre `MapData`) — ya sirve por sí solo para armar el mapa fijo de la Fase 7.
3. Generador procedural, consumiendo/produciendo el mismo `MapData`.

## 2. Formato de datos común — MapData

`ScriptableObject` que guarda: lista de spawn points, punto(s) de llegada (HQ), waypoints del camino, nodos de recursos (posición + `ResourceType`), obstáculos (posición + tipo/footprint). Sigue el criterio ya establecido en el proyecto de que todo dato read-only en runtime vive en un SO. Es la superficie común entre editor manual y generador procedural: el mismo asset puede salir de cualquiera de los dos.

## 3. Editor de mapas

- **APIs**: `EditorWindow` + `SceneView.duringSceneGui` + `Handles`/`Gizmos` para pintar de forma interactiva sobre la Scene view (Unity Manual: "Editor windows", "SceneView"). Extiende el mismo paradigma que ya usan los scripts de `Assets/_Scripts/Editor/`, hoy limitados a acciones one-shot vía `[MenuItem]`.
- **Grid**: lógico, no visual — reusa el mismo criterio de "1 unidad = 1 celda" que ya existe en `BuildingPlacement.GetFootprintOrigin`. Da snapping y determinismo sin que el terreno se vea cuadriculado.
- **Terreno**: `Terrain` nativo de Unity en vez de tiles modulares — heightmap + splatmap (capas = tipos de terreno) scripteables vía `TerrainData.SetHeights`/`SetAlphamaps` (Unity Manual: "Terrain scripting"), y bake directo a NavMesh. Menos trabajo que armar tiles a mano para un prototipo.
- **Modos de pincel**: elevación, tipo de terreno, nodo de recurso, obstáculo, spawn point, HQ point — cada uno escribe sobre el `MapData`, no directo en GameObjects sueltos de la escena.

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

## 5. Compatibilidad con arquitectura existente

- El grid lógico de `BuildingPlacement` se reusa tal cual, no se duplica un sistema de grilla paralelo.
- `MapData` sigue el patrón SO de datos read-only ya establecido en el proyecto.
- `ResourceNode`/`ResourceType` no cambian — el generador solo decide dónde instanciarlos.
- El patrón de `Assets/_Scripts/Editor/` se extiende (ventana con modo pincel interactivo) en vez de reemplazarse.

## 6. Abierto / pendiente de afinar

- `Terrain` nativo vs tiles modulares: decisión tentativa: a confirmar si en algún momento el juego necesita mapas grandes o streaming.
- Costo de performance del bake de NavMesh en runtime para el tamaño de mapa del prototipo — sin medir todavía.
- Ubicación/cantidad de HQs si eventualmente hay multi-facción (hoy fuera de scope, ver notas de [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md)).
- Balance de la generación (frecuencia de curvas del camino, densidad de nodos/obstáculos): se resuelve en iteración de playtesting, no es parte de este doc.
