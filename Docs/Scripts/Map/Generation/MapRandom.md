# MapRandom

`Assets/_Scripts/Map/Generation/MapRandom.cs`

Generador pseudoaleatorio del generador de mapas. Se usa en lugar de `UnityEngine.Random` por dos motivos:

- **Reproducibilidad**: la secuencia depende solo de (semilla, stream), no de un estado global que otro sistema pueda consumir entre medio, y es la misma en cualquier plataforma. Es lo que permite que una semilla identifique un mapa.
- **Streams independientes**: [MapGenerator](MapGenerator.md) crea uno por intento y por etapa, así ajustar una etapa no cambia lo que sortean las otras.

Algoritmo: SplitMix64 (estado de 64 bits, un incremento constante y una función de mezcla). Sobra para generación de contenido y es trivial de portar.

API: `Value` (0 ≤ x < 1), `Range` para `float`/`int` (el de enteros incluye los dos extremos, a diferencia de `UnityEngine.Random.Range`) y para rangos guardados como `Vector2`/`Vector2Int`, que es como los expone [MapGenerationRulesSO](../../ScriptableObjects/MapGenerationRulesSO.md); `Chance`, `Index`, `Direction`, `InsideUnitCircle` y `PickWeighted` para elegir entre opciones con peso (layout, modo de agua).

Fuente: Steele, Lea & Flood, "Fast Splittable Pseudorandom Number Generators" (OOPSLA 2014); implementación de referencia de Sebastiano Vigna — <https://prng.di.unimi.it/splitmix64.c>.
