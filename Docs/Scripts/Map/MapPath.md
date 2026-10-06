# MapPath

`Assets/_Scripts/Map/MapPath.cs`

Camino del mapa: lista ordenada de waypoints en espacio de mundo y un `Width`. Es el camino compartido por oleadas y caravanas ([Design-WavesAndCaravans.md §1](../../Design-WavesAndCaravans.md#1-loop-de-ronda)). Puede haber varios (N spawns que convergen).

`GetSmoothedPoints(stepLength)` devuelve la polilínea suavizada con un spline Catmull-Rom uniforme, que pasa exactamente por cada waypoint. La usan [PathTool](../Editor/MapEditor/PathTool.md) para pintar/nivelar/bloquear el camino sobre el terreno y el gizmo del propio componente; queda disponible para que el gameplay recorra el camino. La versión estática (`GetSmoothedPoints(waypoints, stepLength)`) hace lo mismo sobre cualquier lista de puntos: la usa el generador procedural ([MapPathRouter](Generation/MapPathRouter.md), [MapTerrainShaper](Generation/MapTerrainShaper.md)) para nivelar el terreno exactamente sobre la curva que después va a tener el `MapPath`.

Los waypoints se editan desde `PathTool` vía `SerializedObject` (el campo es privado, sin setter público).

Fuente del spline: Catmull & Rom, "A class of local interpolating splines" (1974); forma matricial usada: <https://en.wikipedia.org/wiki/Cubic_Hermite_spline#Catmull%E2%80%93Rom_spline>.
