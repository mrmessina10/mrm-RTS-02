# Diseño — Balance de Economía (Wave 1)

Números concretos de costo/tiempo para los edificios y la producción de workers, y el razonamiento detrás — la parte que [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md) deja fuera a propósito ("Balance numérico... se resuelve en iteración de playtesting"). Este doc es esa iteración, arrancando por la primera oleada. Ver [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) para en qué Fase se implementa cada pieza.

## 1. Supuestos del diagrama de wave 1

- Prep-time: 90s antes de la primera oleada.
- Población máxima 20, arranque con 3 workers (ver Fase 7 del roadmap).
- Stats de recolección actuales (`WorkerController` en `testWorkerUnit`): `HarvestRate` 1s, `HarvestAmountPerCycle` 1 → 1 recurso/seg/worker en condiciones ideales (drop-off pegado al nodo, sin viaje).
- Los nodos grandes/principales están lejos de la seguridad de la ciudad — el jugador tiene que replegar a sus workers antes de que llegue la oleada, y mientras no haya un drop-off propio cerca del nodo, cada tanda de `MaxCarryCapacity` (10) implica un viaje de ida y vuelta.
- La ciudad arranca con depósitos chicos y seguros cerca del centro. Su función no es sustentar toda la run — es dejar que el jugador bootstree capital (Wood, sobre todo) con fricción ≈0 en los primeros segundos, para financiar la expansión hacia los nodos grandes. Una vez construido un Lumbermill/Farm junto al nodo grande, esa recolección pasa a ser tan eficiente como el bootstrap inicial.
- Con eso: ventana de cosecha real ≈75s (90s menos ~15s de buffer de repliegue), fricción de viaje redondo ≈5s cada 10 recursos en los nodos lejanos mientras no haya drop-off propio ahí → rendimiento efectivo ≈0.67 recursos/seg/worker en esa etapa transicional. El techo teórico sin ninguna fricción (todo bootstrap, sin construir nada) es 1/seg/worker.
- Con 5 workers en Wood / 5 en Food (de los ~10 workers que resultan de un 50% de la población en 20): **techo estimado ≈250 Wood / ≈250 Food** disponibles hacia la wave 1, en el escenario con fricción; hasta ≈450/450 en el caso ideal sin fricción alguna.

## 2. Costos decididos

| Edificio/Unidad | Costo | Tiempo | Notas |
|---|---|---|---|
| Lumbermill | 100 Wood | 25s (`ConstructionTime`) | Drop-off de Wood únicamente. Cuesta Wood (no Food) para no generar dependencia circular. |
| Farm | 100 Wood | 25s (`ConstructionTime`) | Drop-off de Food únicamente. Cuesta Wood, mismo criterio que Lumbermill (convención AoE2: los campamentos económicos cuestan Wood, no su propio recurso). |
| Worker | 20 Food | — (tiempo de producción todavía sin diseñar, depende de la cola de producción de Fase 2) | Costo bajo a propósito: expandir la fuerza de trabajo temprano debe tener fricción mínima. |

Ambos edificios cuestan Wood para que la decisión temprana sea "¿construyo Lumbermill o Farm primero?" sin que la falta de Food (todavía sin fuente antes de tener Farm) bloquee nada.

## 3. Town Center — rol triple

Único edificio con estas tres funciones combinadas (no hay otro building que las separe en este vertical slice):

1. **Drop-off universal**: acepta los cuatro `ResourceType`, a diferencia de Lumbermill/Farm que son de un único recurso.
2. **Único productor de workers**: cuesta 20 Food por worker (ver sección 2). Ningún otro edificio produce workers.
3. **HQ / objetivo principal de las oleadas**: los enemigos avanzan hacia el Town Center (ver [Design-WavesAndCaravans.md §2](Design-WavesAndCaravans.md#2-oleadas-enemigas)); su destrucción es la condición de derrota (ver [Design-WavesAndCaravans.md §6](Design-WavesAndCaravans.md#6-condición-de-victoria-y-derrota)).

## 4. La tensión Food: workers vs. militar

Hoy (sin Barracks implementado) el 100% del Food generado va a workers, porque no existe otro sink. Esto no es la tensión final — es el estado transitorio hasta que exista producción militar.

La apuesta de diseño: como el costo de worker es bajo (20 Food), el jugador **nunca va a estar bloqueado** para crecer su fuerza de trabajo por falta de Food. Una vez que las unidades militares también consuman Food (total o parcialmente, a definir cuando se diseñe el Barracks), cada tanda de Food que entra se vuelve una decisión activa y recurrente — "¿un worker más o un soldado?" — en vez de una barrera de acceso puntual. Tensión de asignación continua, no de accesibilidad.

Falta diseñar (bloqueado por Fase 2, cola de producción todavía sin existir): costo y tiempo de producción de unidades militares, y si consumen Food puro o una mezcla con Wood/recursos estratégicos.
