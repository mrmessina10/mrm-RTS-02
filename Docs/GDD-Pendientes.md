# GDD — Pendientes

Índice de lo que falta diseñar para tener un GDD completo del juego (no solo del prototipo técnico actual). Se va marcando y linkeando a su documento propio a medida que cada punto se aborda y se cierra, igual que [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) hace con la implementación.

Leyenda: ✅ diseñado (con doc propio) · 🔶 parcialmente definido · ⬜ no empezado

## Cubierto

- ✅ **Oleadas y Caravanas** — loop de rondas, roster de enemigos, sistema de caravanas/recursos estratégicos, eje Militarización-Cultura, condiciones de victoria/derrota. Ver [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md).

- ✅ **Editor de mapas y generación procedural** — `MapData` como formato común, editor con pincel sobre la Scene view y generador del camino. Ver [Design-MapEditorAndProceduralGeneration.md](Design-MapEditorAndProceduralGeneration.md). El editor ya tiene una implementación inicial en curso y el generador una primera versión que corre en el editor con reglas configurables por asset; falta la generación en runtime.

## Parcialmente definido

- 🔶 **Roster de edificios y unidades**: costos y rol de Lumbermill, Farm, City Center (drop-off universal + único productor de workers + HQ) y el costo de producción del worker ya están decididos, con el razonamiento del diagrama de wave 1 detrás — ver [Design-EconomyBalance.md](Design-EconomyBalance.md). Falta: edificios militares/defensivos/culturales (Barracks, torres, talleres), el costo/tiempo de Empalizada y Puerta (ya colocables, hoy gratis e instantáneas) y el costo/tiempo de producción de unidades militares — bloqueado por la cola de producción de Fase 2 del roadmap.

- 🔶 **Recursos y economía del Oro**: los recolectables son Wood, Food y Stone; el Oro no se recolecta, solo se consigue comerciando con caravanas. Falta: por qué vía exacta lo entrega una caravana (ítem del catálogo, venta de excedentes, parte de la bonificación de consuelo), en qué se gasta, y cómo se relaciona con los recursos estratégicos. Ver [Design-WavesAndCaravans.md §3 y §4](Design-WavesAndCaravans.md#3-caravanas) y [Design-EconomyBalance.md §5](Design-EconomyBalance.md#5-oro).

- 🔶 **HUD de partida**: layout y componentes decididos sobre una maqueta interactiva — barra de recursos, anuncio de ronda, alertas, tarjeta de comandos, panel de selección, minimapa, Militarización como pop-up, tienda de caravana, pausa y pantalla de fin. Ver [Design-HUD.md](Design-HUD.md). Falta: comandos militares, contenido final del pop-up de Militarización y estilo visual.

## Pendiente

- ⬜ **Curva de progresión y dificultad**: cuántas rondas tiene una partida, cómo escala la composición de oleadas ronda a ronda, cómo y cuándo aparecen los boss encounters, cómo escala el costo de los ítems de caravana.

- ⬜ **Estructura de meta-partida**: si cada partida es una run independiente desde cero, o hay algo persistente entre partidas (desbloqueos, dificultad seleccionable, semillas de mapa reutilizables).

- ⬜ **UI/UX fuera de la partida**: menú principal, opciones y flujo entre pantallas. El HUD de partida ya tiene su propio doc (ver arriba).

- ⬜ **Dirección de arte y audio**: estética visual, tono, referencias, música y efectos de sonido.

- ⬜ **Contexto de mundo**: si el juego tiene lore/narrativa mínima o contexto de ambientación, más allá del pitch mecánico ya definido.
