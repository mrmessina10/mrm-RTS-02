# MapNoise

`Assets/_Scripts/Map/Generation/MapNoise.cs`

Ruido 2D del generador de mapas, sembrado desde un [MapRandom](MapRandom.md). Reemplaza a `Mathf.PerlinNoise`, que no acepta semilla (solo desplazar las coordenadas) y es una llamada nativa.

- `Sample(x, y)` — ruido de valor: un valor pseudoaleatorio por vértice de una grilla entera, interpolado con la curva de quinto grado `6t⁵ − 15t⁴ + 10t³`. Devuelve 0–1.
- `Fractal(x, y, octaves)` — fBm: suma de octavas, cada una con el doble de frecuencia y la mitad de amplitud. Devuelve 0–1, concentrado alrededor de 0,5 (en la práctica entre 0,25 y 0,75); los umbrales que lo usan están calibrados para esa distribución.

Usos: relieve de base, forma irregular de mesetas y lagos, línea de costa ([MapTerrainShaper](MapTerrainShaper.md)), sinuosidad del camino ([MapPathRouter](MapPathRouter.md)) y manchones de decoración ([MapObjectScatterer](MapObjectScatterer.md)). Cada uso crea su propia instancia, con su propia semilla.

Fuentes: Ken Perlin, "Improving Noise" (SIGGRAPH 2002) para la curva de interpolación; Inigo Quilez, "fBM" — <https://iquilezles.org/articles/fbm/>. El hash entero usa las constantes primas de xxHash32 (Yann Collet).
