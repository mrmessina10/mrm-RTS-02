# Diseño — HUD de partida

Qué ve el jugador en pantalla durante una partida y dónde va cada cosa. Es la bajada a diseño de la Fase 8 de [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md#fase-8--hud-de-partida); los sistemas que alimentan cada componente están en [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md) y [Design-EconomyBalance.md](Design-EconomyBalance.md).

Leyenda: ✅ decidido · 🔶 decidido a medias / con detalle pendiente · ⬜ abierto

## Estado

🔶 Layout y componentes decididos sobre una maqueta HTML interactiva (2026-10-06): https://claude.ai/artifact/HxmKTSmzfbZsSyMNNQEvEA (artifact privado). Nada implementado en Unity todavía — hoy todo el feedback es por consola.

## 1. Layout

Vista del mundo en 3D con la cámara en perspectiva del juego; el HUD va superpuesto en los bordes.

- ✅ **Barra superior**, de izquierda a derecha: recursos recolectables, recursos de comercio, y a la derecha workers ociosos, población, Militarización, reloj y menú.
- ✅ **Debajo de la barra**: alertas a la izquierda, anuncio de ronda al centro.
- ✅ **Abajo a la izquierda**: tarjeta de comandos, con el tooltip de costo encima.
- ✅ **Abajo al centro**: panel de selección compacto, con las pestañas de grupos de control arriba.
- ✅ **Abajo a la derecha**: minimapa.

## 2. Componentes

| # | Componente | Qué muestra | Depende de |
|---|---|---|---|
| 1 | Recursos recolectables | Stock de Wood, Food y Stone | Fase 0 ✅ (`ResourceManager.OnResourceChanged`) |
| 2 | Recursos de comercio | Oro y recursos estratégicos, separados visualmente de los recolectables | Fase 4 |
| 3 | Población | Actual / máxima | Fase 2 |
| 4 | Workers ociosos | Cantidad; click recorre uno por uno | Sin dependencia |
| 5 | Reloj y menú | Tiempo de partida y acceso a pausa | Sin dependencia |
| 6 | Anuncio de ronda | Ronda actual / total, tipo de la próxima (oleada o caravana y cuál), cuenta regresiva y ventana de rondas futuras con las no reveladas | Fase 3 / 4 |
| 7 | Militarización | Barra compacta de dos polos junto a la población; el detalle va en un pop-up | Fase 5 |
| 8 | Alertas | Eventos recientes con severidad y salto de cámara al evento | Parcial hoy |
| 9 | Minimapa | Terreno, camino, recursos, edificios, unidades propias, enemigos, caravanas y el encuadre de la cámara; click mueve la cámara; filtros por capa | Sin dependencia |
| 10 | Grupos de control | Grupos 1 a 9 con su contenido | Sin dependencia |
| 11 | Panel de selección | Unidad: vida, estado, carga. Varias: grilla con vida por unidad. Edificio: vida y cola de producción. Obra: progreso. Recurso: cantidad restante | Sin dependencia, salvo cola (Fase 2) |
| 12 | Tarjeta de comandos | Acciones de lo seleccionado en grilla QWER / ASDF / ZXCV; con un worker es el menú de construcción | Fase 1 ✅; producción Fase 2 |
| 13 | Tooltip de costo | Nombre, función, costo y tiempo de obra | Fase 1 ✅ |
| 14 | Indicadores en el mundo | Barras de vida y de obra, ghost de colocación, caja de selección, punto de reunión, texto flotante de depósito | Sin dependencia |
| 15 | Tienda de caravana | Catálogo con stock limitado, costo, encarecimiento por compra, objetivo de la caravana y bonificación de consuelo | Fase 4 |
| 16 | Menú de pausa | Reanudar, opciones, reiniciar, salir | Sin dependencia |
| 17 | Pantalla de fin | Victoria o derrota con resumen de la partida | Fase 6 |

## 3. Decisiones

- ✅ **Comandos a la izquierda, minimapa a la derecha.**
- ✅ **Panel de selección compacto**: ocupa solo lo que su contenido necesita, no todo el ancho inferior, y no se muestra si no hay nada seleccionado.
- ✅ **Militarización como pop-up**: en la barra superior va solo la barra compacta junto a la población. El tier, los efectos sobre producción y caravanas y qué empuja hacia cada polo aparecen al pasar el mouse por encima, no en un panel fijo.
- ✅ **El Oro va con los recursos de comercio**, no con los recolectables: solo se consigue comerciando con caravanas (ver [Design-WavesAndCaravans.md §3](Design-WavesAndCaravans.md#3-caravanas)).
- ✅ **Tarjeta de comandos en grilla de teclado** (QWER / ASDF / ZXCV), reemplazando los hotkeys mock de Numpad.
- ✅ **Minimapa con el encuadre real de la cámara**: como la cámara es en perspectiva, lo que marca es un trapecio y no un rectángulo.

## 4. Abierto

- ⬜ Qué muestra la tarjeta de comandos sin nada seleccionado.
- ⬜ Comandos de unidades militares más allá de mover y atacar con click derecho (atacar-mover, mantener posición, patrullar): en la maqueta están como propuesta, no hay sistema detrás.
- ⬜ Contenido exacto del pop-up de Militarización: depende del nombre de los tiers y de los inputs definitivos del índice (ver [Design-WavesAndCaravans.md §11](Design-WavesAndCaravans.md#11-abierto--pendiente-de-afinar)).
- ⬜ Cómo se muestra el Oro en costos y en la tienda mientras no esté definido en qué se gasta.
- ⬜ Menú principal, opciones y resto de pantallas fuera de partida.
- ⬜ Estilo visual definitivo: la maqueta fija disposición y contenido, no dirección de arte.
