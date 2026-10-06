# PROJECT STATUS

Snapshot consolidado del proyecto: qué es, qué está construido, qué falta para el vertical slice y hacia dónde va después. Es un punto de entrada único — cada sección enlaza al documento que es la fuente de verdad de ese tema. Si algo acá contradice al doc enlazado, gana el doc enlazado.

**Fecha del snapshot:** 2026-10-06 · **Branch:** `economyNbuilding` · **Último commit:** `4038b71` · con trabajo sin commitear (ver sección 2)

Leyenda: ✅ hecho · 🔶 parcial · ⬜ no empezado

---

## 1. Qué es el proyecto

Prototipo de RTS en Unity. El formato principal es **Tower Defense con componentes de Colony Sim**: el jugador arranca con una base chica al costado de un camino pregenerado, recolecta recursos para construir defensas y para expandir/habilitar su pueblo, y recibe por ese mismo camino dos tipos de tráfico — **oleadas de enemigos** y **caravanas** que llegan a comerciar en paz.

El eje de tensión central es la **Militarización**: una barra de dos polos (militar ↔ cultural) que pondera producción, qué caravana aparece y qué bonificación recibe el jugador. Una ciudad muy militarizada defiende mejor pero comercia peor; una muy cultural comercia mejor pero llega débil a cada oleada.

Diseño completo del gimmick: [Docs/Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md).

Recursos: se recolectan Wood, Food y Stone. El Oro no se recolecta — solo se consigue comerciando con caravanas.

**Stack:** Unity 6.5 (`6000.5.6f1`), 3D sobre URP 17.5.0, C#, NavMesh (`com.unity.ai.navigation` 2.0.14), Cinemachine 3.1.7, New Input System 1.20.0.
**Entorno de desarrollo:** MCP de Unity instalado (`com.coplaydev.unity-mcp` v10.3.0, servidor HTTP en `localhost:8080`, configurado en `.mcp.json`) — permite operar el editor de Unity desde Claude Code.
---

## 2. Estado del repositorio

Branch de trabajo `economyNbuilding` (rama principal: `master`). Commits recientes:

| Commit | Contenido |
|---|---|
| `4038b71` | Validación de ocupación física en la colocación + prefabs y datos de Empalizada/Puerta, prefabs reorganizados por categoría |
| `4ec3eee` | Instalación del MCP de Unity para Claude Code |
| `d0dd82a` | Sistema de colocación de muros (Empalizada) + docs de balance económico y de editor/generación de mapas + `PROJECT_STATUS.md` |
| `d59df86` | Cableado final de animaciones del worker + ajustes de giro/interaction range |
| `786f283` | Fase 0 completa + mínimo funcional de Fase 1 (colocación y construcción de edificios) |
| `445d935` | Patrullaje de IA enemiga (`EnemyController`/`EnemyPatrolState`), prueba puntual |
| `9134fbb` | Recolección de recursos, drop-off points, FSM del worker |

**Sin commitear al momento del snapshot:** primera versión del editor de mapas (`Assets/_Scripts/Map/`, `Assets/_Scripts/Editor/MapEditor/`, `MapDataSO`, `MapPaletteSO`, y el chequeo de zona edificable en `BuildingPlacement`), más la actualización de README y documentos de diseño (Oro solo por comercio, [Design-HUD.md](Docs/Design-HUD.md)).

**Convenciones de trabajo** (patrones de código, política de documentación, reglas de commits): [CLAUDE.md](CLAUDE.md).

---

## 3. Arquitectura implementada

51 scripts commiteados en `Assets/_Scripts/`, todos documentados individualmente — índice en [Docs/Scripts/README.md](Docs/Scripts/README.md). Los 19 scripts nuevos del editor de mapas (sin commitear) también tienen su `.md`, en `Docs/Scripts/Map/`, `Docs/Scripts/Editor/MapEditor/` y `Docs/Scripts/ScriptableObjects/`.

### Patrones estructurales
- **FSM por unidad**: cada comportamiento es una clase `IState` con `Enter/Tick/Exit`, orquestada por `StateMachine`. Lógica nueva entra como estado nuevo, no como rama dentro de uno existente.
- **Contratos por interfaz**: `IInteractable` (+ `IHarvestable`, `IDropOffPoint`, `IConstructable`), `IDamageable`, `ISelectable`, `IState`. Managers y estados operan contra interfaces, nunca contra clases concretas.
- **Managers singleton** con `[DefaultExecutionOrder(-100)]`: `ResourceManager`, `BuildingManager`, `GlobalUnitManager`, `GameStateManager`.
- **Datos read-only en ScriptableObjects**: `BuildingDataSO`, `FactionDataSO`, event channels, `InputReader`. Regla general del proyecto: si no cambia en runtime, va en un SO.
- **Composición sobre herencia para edificios**: un edificio es un prefab que combina componentes chicos (`DropOffBuilding`, `UnitProducer`, `Headquarters`, `BuildingPlacement`, `ConstructionSite`, `Health`), no una jerarquía de clases por tipo.

### Sistemas por área

| Área | Estado | Piezas |
|---|---|---|
| Cámara RTS | ✅ | `Player` (paneo, órbita, zoom, edge scrolling vía Cinemachine) |
| Input | ✅ | `InputReader` (SO + New Input System, eventos desacoplados) |
| Selección y comandos | ✅ | `SelectionManager`, `UnitSelectionHandler`, `GlobalUnitManager` — click, shift-click, doble click por tipo, box selection, grupos de control 1-9, movimiento en formación |
| Movimiento | ✅ | `UnitMovement` (wrapper de `NavMeshAgent`), `UnitMoveState` con anti-crowding |
| Combate | ✅ | `MeleeAttackState`, `RangedAttackState`, `Projectile`, `Health`/`IDamageable`, `DamageData`, daño sincronizado por Animation Event (`AnimationEventRelay`) |
| Economía — recolección | ✅ | `ResourceNode`/`IHarvestable`, `WorkerController`, FSM de 3 estados, `DropOffBuilding`, `BuildingManager`, `ResourceManager`. Verificado en play mode. Incluye force-drop e interrupción sin pérdida de carga (criterio AoE2) |
| Construcción | ✅ mínimo | `BuildingDataSO`, `BuildingPlacement` (grilla + `NavMeshObstacle` + validación de celda libre por `Physics.CheckBox` contra layers `Resources`/`Buildings`), `BuildingPlacementController` (ghost, validación, costo), `ConstructionSite` + `WorkerBuildState` (obra real por workers) |
| City Center y producción | 🔶 | Fase 2 en curso, sin commitear: `UnitProducer` + `UnitDataSO` (cola FIFO, costo al encolar, tiempo por unidad), `Headquarters`/`IHeadquarters` registrado en `BuildingManager.Headquarters`, hotkey mock Numpad 5 para producir un worker con el City Center seleccionado. Compila; falta generar el prefab (`Tools/RTS/Generate City Center Prefab`) y probar en play mode |
| Muros | 🔶 | `WallPlacementController` (trazado Bresenham, elevación por celda, costo por celda válida, vida independiente por segmento), con prefabs reales `PalisadeSegment`/`Gate` y probado en escena. Falta cargar costo y tiempo de construcción |
| Animación de worker | ✅ | `WorkerUnitAnimator` con `IsMoving`/`IsCarrying`/`IsHarvestingWood`/`IsHarvestingFood`/`IsBuilding` |
| Tooling de editor | ✅ | 6 scripts en `Assets/_Scripts/Editor/` — generadores de prefabs (recursos, drop-offs, defensivos, City Center) y setup de managers en escena |
| Contenido de edificios | 🔶 | `Assets/Prefabs/Buildings/` organizado por categoría: `Eco/` (Lumbermill, Farm), `Defense/` (PalisadeSegment, Gate), `Pop/` (destino de `CityCenter`, todavía sin generar) y `Units/` vacía — reservada para el Barracks |
| IA enemiga | 🔶 | `EnemyController` + `EnemyPatrolState` — solo patrulla; no ataca ni avanza hacia el pueblo |
| Estado de partida | ⬜ | `GameStateManager`/`GameManager` existen como esqueleto; nada dispara victoria ni derrota |
| Editor de mapas | 🔶 | Primera versión sin commitear: `MapRoot`/`MapPath`/`MapMarker` en runtime, `MapDataSO`/`MapPaletteSO` como datos, y ventana `Tools/RTS/Map Editor` con herramientas de relieve, pintura de terreno, agua, objetos, camino, marcadores y zona edificable. `BuildingPlacement` ya consulta la zona edificable del mapa. Probado por script en una escena temporal (cada herramienta, bake de NavMesh, validación, ida y vuelta a `MapDataSO`); falta probar el input interactivo sobre la Scene view y crear el mapa en `SampleScene`. Los marcadores (spawn, fin de oleada, inicio del jugador) todavía no los consume ningún sistema de gameplay |
| HUD / UI | ⬜ | No existe en el juego: todo el feedback es por consola. Layout y componentes ya diseñados sobre una maqueta — [Design-HUD.md](Docs/Design-HUD.md) |

---

## 4. Objetivos del vertical slice

Los 6 requisitos trazados y su estado. Detalle y trazabilidad completa en [Docs/Roadmap-VerticalSlice.md](Docs/Roadmap-VerticalSlice.md).

1. **Unidades melee y de rango** — ✅ Sistema resuelto. Falta contenido (prefabs por tipo, balance).
2. **Workers que recolectan y depositan** — ✅ Loop cerrado de punta a punta, verificado en play mode con `Lumbermill`/`Farm` reales.
3. **Edificios de distinto tipo** — 🔶 Drop-offs listos; muros y puerta con colocación implementada y prefabs reales (sin costo/tiempo todavía); City Center implementado (falta generar el prefab y probarlo); faltan Barracks, torres, límite poblacional e indexado por categoría.
4. **Loop recolección → inversión** — 🔶 Inversión en edificios cierra; producción de workers con costo implementada, falta la militar.
5. **Creación, interacción y muerte de unidades** — 🔶 Muerte y combate resueltos; creación en runtime implementada para workers, sin probar en play mode.
6. **Core loop con victoria y derrota** — ⬜ No empezado. Sin oleadas, sin caravanas, sin condiciones de fin.

### Fases de implementación

| Fase | Estado | Contenido |
|---|---|---|
| 0 — Cerrar loop de economía | ✅ | Registro de drop-offs, depósito al inventario, gasto de recursos |
| 1 — Construcción de edificios | ✅ mínimo | Ghost, validación, costo, cimiento, obra por worker. Falta menú real (hoy hotkeys Numpad) y barra de progreso |
| 2 — Edificios funcionales por tipo | 🔶 | City Center (rol triple) y producción de workers implementados, sin probar en play mode. Faltan producción militar (Barracks), límite poblacional, torre defensiva e indexado por categoría en `BuildingManager` |
| 3 — Enemigos y tower defense | ⬜ | `EnemyController` heredando de `UnitController`, `EnemyAttackState`, avance al HQ, spawner de rondas |
| 4 — Caravanas y recursos estratégicos | ⬜ | Enum separado de recursos estratégicos, ronda tipo caravana, un tipo funcional de punta a punta |
| 5 — Militarización | ⬜ | Manager del índice soldados vs trabajadores, ponderando producción y selección de caravana |
| 6 — Victoria y derrota | ⬜ | Derrota por City Center destruido, victoria por N rondas, `Victory` en el enum `GameState` |
| 7 — Bootstrap de partida | ⬜ | Estado inicial (City Center construido, workers, recursos), cámara centrada, mapa fijo a mano |
| 8 — HUD de partida | ⬜ | Minimapa, panel de unidades, recursos, menú de edificios, población, anuncio de ronda, índice de Militarización, pantalla de fin |

De la Fase 3 en adelante cada fase separa **Mínimo jugable** de **Extensión**, para poder cerrar el game loop completo con contenido mínimo antes de sumar profundidad.

**Fase 8 es transversal**: cada ítem del HUD depende de una fase distinta y se puede asignar por separado. Minimapa y panel de información de unidades no dependen de nada — se pueden empezar hoy.

---

## 5. Estado del diseño

Lo técnico y lo diseñado avanzan por carriles separados. Índice de lo que falta diseñar: [Docs/GDD-Pendientes.md](Docs/GDD-Pendientes.md).

### Decidido (con documento propio)
- ✅ **Oleadas y Caravanas** — [Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md): loop de rondas con prep-time, roster de enemigos (Raiders, Siege engines, Shock units) con targeting propio por tipo, caravanas como tienda temporal con precios dinámicos y bonificación de consuelo, recursos estratégicos en sistema paralelo, eje Militarización-Cultura, victoria/derrota.
- ✅ **Balance de economía (wave 1)** — [Design-EconomyBalance.md](Docs/Design-EconomyBalance.md): Lumbermill y Farm a 100 Wood / 25s, worker a 20 Food, City Center con rol triple (drop-off universal + único productor de workers + HQ), y la tensión de Food entre workers y militar.
- ✅ **Editor de mapas y generación procedural** — [Design-MapEditorAndProceduralGeneration.md](Docs/Design-MapEditorAndProceduralGeneration.md): `MapData` como formato común, editor con pincel sobre Scene view, generador con A* perturbado y Poisson disk sampling. Fuera del vertical slice, diseñado para después.

### Parcialmente definido
- 🔶 **Roster de edificios y unidades** — económicos decididos; faltan militares, defensivos y culturales (Barracks, torres, talleres) con sus costos y tiempos. Bloqueado por la cola de producción de Fase 2.
- 🔶 **Recursos y economía del Oro** — recolectables acotados a Wood, Food y Stone; el Oro solo entra por caravanas. Falta por qué vía lo entrega una caravana y en qué se gasta.
- 🔶 **HUD de partida** — [Design-HUD.md](Docs/Design-HUD.md): layout decidido (comandos abajo a la izquierda, selección compacta al centro, minimapa a la derecha, Militarización como pop-up junto a la población) e inventario de 17 componentes con su dependencia por fase. Faltan comandos militares y estilo visual.

### Sin empezar
- ⬜ Curva de progresión y dificultad (cuántas rondas, cómo escala cada oleada, boss encounters).
- ⬜ Estructura de meta-partida (run independiente vs progresión persistente).
- ⬜ UI/UX fuera de la partida (menú principal, opciones).
- ⬜ Dirección de arte y audio.
- ⬜ Contexto de mundo / lore.

---

## 6. Scope proyectado

### Dentro del vertical slice
Todo lo listado en las Fases 0 a 8: una partida jugable de punta a punta — arrancar con base y workers, recolectar, construir, producir unidades, sobrevivir oleadas, comerciar con al menos un tipo de caravana, y ganar o perder — con contenido mínimo pero sin huecos en el loop.

### Fuera del vertical slice (decidido explícitamente)
- Múltiples facciones o jugadores.
- Árbol de mejoras y tecnología.
- Más de un tipo de unidad melee/ranged.
- Guardado de partida.
- Generación procedural del mapa (ya diseñada, ver sección 5). El editor de mapas que la precede también estaba afuera, pero se empezó a implementar para armar el mapa fijo de la Fase 7; el generador tiene una primera versión de editor (2026-10-06) con reglas en `MapGenerationRulesSO`, sin generación en runtime todavía.

### Direcciones futuras ya declaradas en la documentación
Decisiones de arquitectura tomadas y escritas, pendientes de implementación:
- `EnemyController` va a heredar de `UnitController` para reusar el combate genérico en vez de duplicarlo.
- `BuildingManager` va a indexar edificios por categoría (dropoff / económico / cultural / militar / defensivo), generalizando `GetNearestDropOff`, para que cada tipo de enemigo resuelva su objetivo prioritario.
- `ResourceType` queda acotado a recursos recolectables; los estratégicos usan enum y sistema propios. Pendiente: el enum todavía incluye `Gold`, que dejó de ser recolectable — decidir si se queda como recurso de inventario o pasa al sistema de comercio (también toca `FactionDataSO.startingGold`).
- `FactionDataSO` existe sin uso — candidato natural para los valores de arranque de partida (Fase 7).

---

## 7. Pendientes técnicos conocidos

Cosas identificadas y anotadas en la documentación, no bloqueantes pero acumulables:

- Los SO `BuildingData_PalisadeSegment` y `BuildingData_Gate` existen pero con `ConstructionCost` vacío y `ConstructionTime` en 0 — se construyen gratis e instantáneo. `ArcherTower` todavía no tiene ni SO ni prefab. El balance de los defensivos no está diseñado ([Design-EconomyBalance.md](Docs/Design-EconomyBalance.md) solo cubre Lumbermill/Farm/worker).
- La validación de celda ocupada depende de los nombres de layer `Resources` y `Buildings` vía `LayerMask.GetMask` (cacheado) — si se renombran en `TagManager`, hay que actualizar `BuildingPlacement`. Los `Units` quedan fuera a propósito: no bloquean colocación.
- Punto de entrada de construcción y de producción son hotkeys mock (Numpad 1-4 para construir, Numpad 5 para producir un worker), a reemplazar por hotkeys estilo AoE2 y menú real.
- `UnitController.SetTarget` ya solo ataca objetivos `InteractionType.Attack` (el resto es una orden de acercarse), así que el jugador no puede dañar sus propios edificios. Queda para la Fase 3: la IA enemiga tiene que atacar edificios (`Type = None`) vía `GetAttackState` directo, no por `SetTarget` — ver [UnitController.md](Docs/Scripts/Unit%20Scripts/UnitController.md).
- Logs de consola que sustituyen feedback visual (carga del worker, depósitos) — a sacar cuando exista HUD.
- Pool de ghosts del muro se instancia/destruye cada frame, sin pooling persistente — primer candidato a optimizar si pesa.
- `FloatEventChannelSO`/`IntEventChannelSO` declarados con el tipo en el nombre pero sin el payload cableado; sin consumidores.
- El índice [Docs/Scripts/README.md](Docs/Scripts/README.md) no lista `EnemyController` ni `EnemyPatrolState`, aunque ambos tienen su `.md` escrito.

---

## 8. Mapa de documentación

| Documento | Qué contiene |
|---|---|
| [README.md](README.md) | Presentación pública del repo |
| [CLAUDE.md](CLAUDE.md) | Convenciones de trabajo, estilo de código, política de docs y commits |
| [Docs/Roadmap-VerticalSlice.md](Docs/Roadmap-VerticalSlice.md) | Scope y fases del vertical slice — fuente de verdad del estado de implementación |
| [Docs/GDD-Pendientes.md](Docs/GDD-Pendientes.md) | Índice de lo que falta diseñar para un GDD completo |
| [Docs/Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md) | Diseño del gimmick central |
| [Docs/Design-EconomyBalance.md](Docs/Design-EconomyBalance.md) | Números de costo/tiempo y su razonamiento |
| [Docs/Design-HUD.md](Docs/Design-HUD.md) | Layout y componentes del HUD de partida, con link a la maqueta |
| [Docs/Design-MapEditorAndProceduralGeneration.md](Docs/Design-MapEditorAndProceduralGeneration.md) | Diseño de las herramientas de mapa (post-slice) |
| [Docs/Scripts/](Docs/Scripts/README.md) | Un `.md` por script, espejando `Assets/_Scripts/` |
