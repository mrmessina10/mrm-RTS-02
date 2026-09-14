# Diseño — Oleadas y Caravanas

Gimmick principal del juego: el jugador arranca con una base chica, unos pocos workers y soldados, al costado de un camino generado de antemano. Debe organizar su pueblo para defenderlo (construcción de defensas, unidades, edificios económicos) mientras recibe dos tipos de tráfico por ese mismo camino: oleadas de enemigos (tower defense clásico) y caravanas que llegan a comerciar en paz. Ver [README.md](../README.md) para la visión general del proyecto y [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) para el estado de la base técnica sobre la que se construye este diseño.

Leyenda: ✅ decidido · 🔶 decidido a medias / con detalle pendiente · ⬜ abierto

## 1. Loop de ronda

Secuencia única de rondas con prep-time fijo entre cada una. Por defecto, toda ronda es una oleada enemiga. Cada cierto número de rondas —frecuencia dinámica y balanceable, no fija— una ronda se reemplaza por una **Caravana** en vez de un ataque. El HUD anuncia con anticipación la naturaleza de la próxima ronda (oleada o caravana) y, si es caravana, revela también **qué tipo** es.

El camino es **compartido** entre oleadas y caravanas: las mismas defensas que el jugador construye contra los enemigos también condicionan (para bien o para mal) el paso de las caravanas. No hay dos rutas separadas.

## 2. Oleadas enemigas

Tower defense clásico: los enemigos entran por el camino y avanzan hacia el Town Center (HQ, objetivo principal, condición de derrota si es destruido). La composición de cada oleada se diseña manualmente en tiempo y forma (balance artesanal, no generación puramente aleatoria).

Roster inicial de tipos de enemigo, cada uno con su propia lógica de targeting:

- **Raiders**: atacan edificios, priorizando económicos y culturales. Evitan trabarse con torres/defensas salvo que estas se interpongan en su camino hacia el HQ.
- **Siege engines**: priorizan edificios militares y defensas, en última instancia el HQ. Ignoran soldados y edificios comerciales. Van escoltados por Raiders.
- **Shock units**: rápidas, priorizan atacar soldados y también workers desarmados que estén recolectando lejos de las defensas. No pueden destruir edificios ni provocan la derrota del jugador por sí solas, pero son una amenaza seria para el cálculo de guarnición y de logística de recolección (sobre todo de recursos estratégicos, ver sección 4).

## 3. Caravanas

No son una oleada alternativa de combate: son una **tienda temporal**. Cada tipo de caravana trae un catálogo fijo de ventajas (recursos, bonificaciones, unidades exclusivas no conseguibles de otra forma), con inventario limitado en cantidad y variedad.

- Cada ítem del catálogo tiene un costo fijo en **recursos estratégicos** (ver sección 4) — como una receta. El costo es fijo por definición, pero balanceable y escalable a lo largo de la partida.
- Requisito híbrido para comprar: el edificio productor del recurso estratégico correspondiente tiene que estar construido y activo, **y** el jugador tiene que tener stock mínimo acumulado de ese recurso antes de que la caravana llegue.
- Se puede comprar tantos ítems como el jugador pueda pagar en una sola visita; el límite real es el stock propio y el inventario limitado de la caravana.
- **Precios dinámicos**: cada compra encarece la siguiente compra del mismo tipo de ítem. Los precios se pueden resetear o reducir cumpliendo objetivos que la propia caravana propone.
- **Consuelo sin compra**: si el jugador no puede pagar nada del catálogo, igual recibe una bonificación estándar de recursos. El tamaño de esa bonificación escala con qué tan volcada a la cultura está la ciudad (ver sección 5) — una ciudad más culta es más abierta para comerciar y recibe más.
- **Selección de tipo de caravana**: aleatoriedad ponderada según los recursos/edificios disponibles del jugador, la cantidad de soldados, y el índice de Militarización.

## 4. Recursos estratégicos ("strategic trading resources")

Categoría de recursos separada de los recolectables (`ResourceType`, ver [ResourceType.md](Scripts/Resources/ResourceType.md)), producida únicamente por edificios económicos y talleres específicos. Corren por un sistema paralelo independiente del de recolección/inventario de recursos básicos — no se mezclan enums ni flujos. Cada tipo de caravana pide uno o más recursos estratégicos concretos.

## 5. Militarización — el eje central de tensión

Índice compuesto, continuo por dentro, mostrado al jugador en tiers (nombre del stat 🔶 a confirmar). Es una sola barra de dos polos, simétrica: cada punto que sube de un lado se le resta al otro.

Sube hacia el polo militar con: soldados vivos, ratio de edificios militares/defensivos vs económicos/culturales, bonificaciones militares ganadas en caravanas anteriores, objetivos cumplidos de corte militar.

Sube hacia el polo cultural con: trabajadores vivos, stock de recursos de talleres/edificios culturales, bonificaciones y objetivos cumplidos de corte cultural/comercial.

Efectos: pondera la producción (más polo militar = mejor rendimiento de cuarteles/torres y peor de talleres/mercados, y viceversa, de forma proporcional y simétrica), pondera qué tipo de caravana tiene más chance de aparecer, y determina el tamaño de la bonificación de consuelo cuando no se puede pagar nada del catálogo.

Esto cierra el loop de tensión: una ciudad muy militarizada defiende mejor pero empobrece su relación comercial; una ciudad muy cultural comercia mejor pero llega más débil a cada oleada.

## 6. Condición de victoria y derrota

- **Derrota**: Town Center destruido (usa el canal `Channel_GameOver` ya existente, ver [GameStateManager.md](Scripts/_CORE%20Scripts/GameStateManager.md)).
- **Victoria**: sobrevivir un número preestablecido de rondas. Puede haber boss encounters seedeados en puntos específicos de la secuencia de rondas, con mayor desafío que una oleada normal.

## 7. UI

El HUD muestra la ronda inmediatamente próxima y, dependiendo de mejoras/bonificaciones conseguidas durante la run actual, también algunas rondas siguientes (ventana de visión anticipada ampliable 🔶 — falta definir si lo único que escala es cuántas rondas a futuro se ven, o también el nivel de detalle mostrado de cada una).

## 8. Mapa y camino

Generado proceduralmente. Dependencia de secuencia: hacen falta herramientas de editor de Unity (siguiendo el patrón ya existente en `Assets/_Scripts/Editor/`) diseñadas *antes* de poder construir el generador procedural en sí.

## 9. Los cuatro ejes de balance del jugador

En cada prep-time el jugador administra, en simultáneo: unidades y defensa, nivel de Militarización de su ciudad, recursos básicos (para unidades/defensas), y recursos estratégicos (para interactuar con caravanas).

## 10. Compatibilidad con la arquitectura existente

Este diseño no contradice la base de código actual — completa exactamente el hueco que [Roadmap-VerticalSlice.md](Roadmap-VerticalSlice.md) ya tenía reservado (Fase 2 en adelante). Decisiones de arquitectura ya tomadas y declaradas en su documentación correspondiente:

- ✅ `EnemyController` va a heredar de `UnitController` (para reusar el combate genérico Melee/Ranged) — todavía no implementado por testing temprano del patrullaje en aislamiento. Declarado en [EnemyController.md](Scripts/Unit%20Scripts/EnemyController.md).
- ✅ `BuildingManager` va a indexar edificios por múltiples filtros de categoría (dropoff, económico, cultural, militar, defensivo), no solo `IDropOffPoint` — necesario para que Raiders/Siege engines resuelvan su objetivo prioritario por categoría. Declarado en [BuildingManager.md](Scripts/_CORE%20Scripts/BuildingManager.md), pendiente de los edificios en sí (todavía no diseñados por completo).
- ✅ `ResourceType` queda acotado a recursos recolectables únicamente; los recursos estratégicos (sección 4) usan un enum separado y un sistema paralelo independiente. Declarado en [ResourceType.md](Scripts/Resources/ResourceType.md).

Dependencias de orden pendientes (no son incompatibilidades, son huecos de fases previas del roadmap que este diseño necesita resueltas primero):

- ⬜ Población y producción de unidades (Fase 2 del roadmap) — la Militarización necesita "soldados vivos" como input real.
- ⬜ `GameState` (enum en `GameStateManager`) solo contempla `GameOver` — falta agregar `Victory` para la condición de la sección 6.
- ⬜ `FactionDataSO` existe sin uso — candidato natural a valores iniciales si en algún momento hay multi-facción, no bloqueante ahora.

## 11. Abierto / pendiente de afinar

- Nombre definitivo del stat de Militarización.
- Detalle de qué mejora exactamente la ventana de visión anticipada de la UI (sección 7).
- Qué determina exactamente los "objetivos" que la caravana propone para resetear/reducir precios.
- Catálogo y recurso estratégico concreto de cada tipo de caravana (más allá de Raiders/Siege/Shock del lado enemigo, todavía no hay tipos de caravana nombrados).
- Balance numérico de todo lo anterior: no es parte de este documento de diseño, se resuelve en iteración de playtesting.
