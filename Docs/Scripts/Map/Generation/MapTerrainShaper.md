# MapTerrainShaper

`Assets/_Scripts/Map/Generation/MapTerrainShaper.cs`

Etapas de relieve de [MapGenerator](MapGenerator.md). Opera sobre el [MapHeightField](MapHeightField.md) con las mismas convenciones que las herramientas del editor: suelo base en 0, mesetas por niveles discretos con acantilado, agua como plano global.

**`Shape`** (antes de trazar rutas)

1. *Relieve de base*: fBm de [MapNoise](MapNoise.md) con amplitud baja, multiplicado por la máscara de zonas reservadas y con un piso por encima del nivel de agua (el ruido solo nunca forma charcos).
2. *Mesetas*: manchas de borde irregular (radio perturbado con ruido) levantadas a `LevelHeight`, con un acantilado de ~1,2 unidades de ancho; algunas llevan un segundo nivel adentro. Equivale al modo Meseta de [TerrainSculptTool](../../Editor/MapEditor/TerrainSculptTool.md).
3. *Rampas*: por cada meseta busca el borde caminando desde afuera hacia el centro y aplica una rampa (`MapHeightField.ApplyRamp`) con la pendiente de las reglas. La primera apunta hacia el HQ. Si el pie de la rampa cae fuera del mapa o en agua, no se hace: una meseta sin rampa queda como obstáculo.
4. *Agua*, un modo por mapa:
   - **Lagos**: manchas cavadas con orilla suave, lejos de zonas reservadas y de mesetas.
   - **Río**: polilínea suavizada que cruza el mapa de lado a lado respecto del borde de entrada, cavada con ancho constante. Se le abren **vados**: una calzada plana apenas sobre el agua con una rampa hacia cada orilla. Sin las rampas el vado queda aislado, porque la barranca del río supera la pendiente caminable.
   - **Costa**: uno de los bordes sin spawns se hunde, con la línea de costa perturbada por ruido. El ancho se achica si se acerca a la zona de inicio.

   Si el modo sorteado no entra (por ejemplo un río en un mapa de 64) el mapa queda sin agua.

**`CarveRoutes`** (después de trazar rutas) — nivela el terreno bajo cada ruta, como "Nivelar terreno bajo el camino" de [PathTool](../../Editor/MapEditor/PathTool.md) pero con dos garantías extra:

- Donde la ruta cruza agua el perfil se levanta a altura de vado.
- El perfil se limita a `MaxGradient` con una pasada hacia adelante y otra hacia atrás, así que donde corta un acantilado queda una rampa tallada en vez de un escalón.

El volcado usa una grilla de distancia a la polilínea (`MapHeightField.StampPolyline`) en lugar de estampar tramo por tramo, para que los tramos superpuestos no se acumulen.

Referencias: Inigo Quilez, "fBM" — <https://iquilezles.org/articles/fbm/>.
