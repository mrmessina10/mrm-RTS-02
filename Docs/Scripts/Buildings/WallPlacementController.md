# WallPlacementController

`Assets/_Scripts/Buildings/WallPlacementController.cs`

Modo de colocación de la Empalizada (muro + puerta de madera). Arquitectura distinta a [BuildingPlacementController](BuildingPlacementController.md) porque la interacción es distinta: no es "un click, un edificio", es trazar una tira continua de segmentos de 1x1 entre dos puntos, y poder encadenar varios tramos con cambios de dirección en una sola sesión.

## Flujo de input
Consume [InputReader.WallBuildRequestEvent](../_CORE%20Scripts/InputReader.md) (hotkey mock Numpad 3, sin payload — solo hay un tipo de segmento de muro).

- **Primer click** (`SelectEvent`, filtrado a release igual que `BuildingPlacementController`): fija el punto inicial (`startCell`), no construye nada todavía.
- **Clicks siguientes**: cada uno confirma el tramo recto desde el punto anterior hasta la celda clickeada (`BuildRun`), y ese mismo punto final pasa a ser el nuevo punto inicial — así el jugador sigue trazando sin tener que volver a pedir el modo, generando cambios de dirección a mano.
- **Click derecho** (`CommandEvent`): cierra la sesión de trazado por completo.

## Trazado de la línea
`RasterizeLine` implementa el algoritmo de línea de Bresenham sobre la grilla 2D (X/Z, 1 unidad = 1 celda) para conectar dos celdas con una tira continua sin huecos, en cualquier ángulo — no restringido a horizontal/vertical/diagonal de 45°.

## Elevación de terreno
Cada celda del trazado samplea **su propia** altura de terreno con un raycast individual hacia abajo (`TryGetCellGroundPosition`) contra `groundMask` — no se asume una altura única para toda la tira ni se interpola entre el punto inicial y el final. El muro sigue el relieve real del terreno, funcione plano o con desniveles (el editor de mapas con elevación todavía no existe, pero este sampleo por celda ya funciona igual sea cual sea el terreno de abajo).

## Costo y validación
Por cada tramo confirmado: rasteriza la línea, samplea la altura y valida `BuildingPlacement.IsAreaBuildable` de cada celda (1x1), **descarta las celdas inválidas sin cancelar el resto** (se saltea el obstáculo, sigue construyendo del otro lado). El costo total del tramo es `BuildingDataSO.ConstructionCost` multiplicado por la cantidad de celdas válidas — todo o nada: si no alcanza para pagar el tramo completo, no se construye nada de ese tramo (no hay construcción parcial por falta de fondos).

Cada celda válida se instancia y recibe su propio [ConstructionSite](ConstructionSite.md) — cada segmento tiene su propia vida (`Health`) y su propia obra en curso, así el enemigo puede abrir una brecha puntual en un tramo poco defendido en vez de que todo el muro comparta una sola vida.

## Ghost
Mientras el mouse se mueve (con o sin punto inicial fijado), `UpdateGhosts` reconstruye un pool de ghosts (uno por celda del preview actual, usando [BuildingGhostUtility](BuildingGhostUtility.md)) — se instancian/destruyen cada frame según cambie el largo de la línea, sin pooling persistente (aceptable para un prototipo, primer punto a optimizar si hace falta). Sin punto inicial todavía, el preview es de una sola celda (dónde caería el primer click).

## Setup en escena
No se auto-instancia: `Tools/RTS/Ensure Wall Placement Controller In Scene` ([BuildingPlacementSceneSetup](../Editor/BuildingPlacementSceneSetup.md)) copia `inputReader`/`mainCamera`/`groundMask` desde `SelectionManager` y busca el `BuildingDataSO` de tipo `Palisade` para `wallSegmentData`.

## Pendiente
La Puerta (`BuildingType.Gate`) no usa este controller — es un edificio de footprint único que se coloca con `BuildingPlacementController` como cualquier otro, y se supone que "se funde" visualmente con los muros contiguos (responsabilidad de arte, no de este sistema). Sin mecánica de abrir/cerrar todavía — bloquea paso igual que un segmento de muro común.
