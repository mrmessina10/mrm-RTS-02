# PROJECT STATUS

Snapshot consolidado del proyecto: qué es, qué está construido, qué falta para el vertical slice y hacia dónde va después. Es un punto de entrada único — cada sección enlaza al documento que es la fuente de verdad de ese tema. Si algo acá contradice al doc enlazado, gana el doc enlazado.

**Fecha del snapshot:** 2026-09-18 · **Branch:** `economyNbuilding` · **Último commit:** `d59df86`

Leyenda: ✅ hecho · 🔶 parcial · ⬜ no empezado

---

## 1. Qué es el proyecto

Prototipo de RTS en Unity. El formato principal es **Tower Defense con componentes de Colony Sim**: el jugador arranca con una base chica al costado de un camino pregenerado, recolecta recursos para construir defensas y para expandir/habilitar su pueblo, y recibe por ese mismo camino dos tipos de tráfico — **oleadas de enemigos** y **caravanas** que llegan a comerciar en paz.

El eje de tensión central es la **Militarización**: una barra de dos polos (militar ↔ cultural) que pondera producción, qué caravana aparece y qué bonificación recibe el jugador. Una ciudad muy militarizada defiende mejor pero comercia peor; una muy cultural comercia mejor pero llega débil a cada oleada.

Diseño completo del gimmick: [Docs/Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md).

**Stack:** Unity `6000.5.6f1`, C#, NavMesh (`com.unity.ai.navigation`), Cinemachine, New Input System, URP.
⚠️ El [README.md](README.md) todavía declara Unity 6.3 LTS (`6000.3.9f1`) y describe el proyecto como RTS genérico sin mencionar el gimmick de Oleadas/Caravanas — quedó desactualizado respecto de este documento.

---

## 2. Estado del repositorio

Branch de trabajo `economyNbuilding` (rama principal: `master`). Commits recientes:

| Commit | Contenido |
|---|---|
| `d59df86` | Cableado final de animaciones del worker + ajustes de giro/interaction range |
| `786f283` | Fase 0 completa + mínimo funcional de Fase 1 (colocación y construcción de edificios) |
| `445d935` | Patrullaje de IA enemiga (`EnemyController`/`EnemyPatrolState`), prueba puntual |
| `9134fbb` | Recolección de recursos, drop-off points, FSM del worker |

**Sin commitear al momento del snapshot:** sistema de colocación de muros (`WallPlacementController`, `PlacementModeState`, `BuildingGhostUtility`), generador de prefabs defensivos, y tres documentos de diseño nuevos (`Design-EconomyBalance.md`, `Design-MapEditorAndProceduralGeneration.md`, más ediciones al roadmap y al GDD de pendientes).

**Convenciones de trabajo** (patrones de código, política de documentación, reglas de commits): [CLAUDE.md](CLAUDE.md).

---

## 3. Arquitectura implementada

51 scripts en `Assets/_Scripts/`, todos documentados individualmente — índice en [Docs/Scripts/README.md](Docs/Scripts/README.md).

### Patrones estructurales
- **FSM por unidad**: cada comportamiento es una clase `IState` con `Enter/Tick/Exit`, orquestada por `StateMachine`. Lógica nueva entra como estado nuevo, no como rama dentro de uno existente.
- **Contratos por interfaz**: `IInteractable` (+ `IHarvestable`, `IDropOffPoint`, `IConstructable`), `IDamageable`, `ISelectable`, `IState`. Managers y estados operan contra interfaces, nunca contra clases concretas.
- **Managers singleton** con `[DefaultExecutionOrder(-100)]`: `ResourceManager`, `BuildingManager`, `GlobalUnitManager`, `GameStateManager`.
- **Datos read-only en ScriptableObjects**: `BuildingDataSO`, `FactionDataSO`, event channels, `InputReader`. Regla general del proyecto: si no cambia en runtime, va en un SO.
- **Composición sobre herencia para edificios**: un edificio es un prefab que combina componentes chicos (`DropOffBuilding`, `BuildingPlacement`, `ConstructionSite`, `Health`), no una jerarquía de clases por tipo.

### Sistemas por área

| Área | Estado | Piezas |
|---|---|---|
| Cámara RTS | ✅ | `Player` (paneo, órbita, zoom, edge scrolling vía Cinemachine) |
| Input | ✅ | `InputReader` (SO + New Input System, eventos desacoplados) |
| Selección y comandos | ✅ | `SelectionManager`, `UnitSelectionHandler`, `GlobalUnitManager` — click, shift-click, doble click por tipo, box selection, grupos de control 1-9, movimiento en formación |
| Movimiento | ✅ | `UnitMovement` (wrapper de `NavMeshAgent`), `UnitMoveState` con anti-crowding |
| Combate | ✅ | `MeleeAttackState`, `RangedAttackState`, `Projectile`, `Health`/`IDamageable`, `DamageData`, daño sincronizado por Animation Event (`AnimationEventRelay`) |
| Economía — recolección | ✅ | `ResourceNode`/`IHarvestable`, `WorkerController`, FSM de 3 estados, `DropOffBuilding`, `BuildingManager`, `ResourceManager`. Verificado en play mode. Incluye force-drop e interrupción sin pérdida de carga (criterio AoE2) |
| Construcción | ✅ mínimo | `BuildingDataSO`, `BuildingPlacement` (grilla + `NavMeshObstacle`), `BuildingPlacementController` (ghost, validación, costo), `ConstructionSite` + `WorkerBuildState` (obra real por workers) |
| Muros | 🔶 | `WallPlacementController` (trazado Bresenham, elevación por celda, costo por celda válida, vida independiente por segmento). Falta costo/tiempo y prueba en escena |
| Animación de worker | ✅ | `WorkerUnitAnimator` con `IsMoving`/`IsCarrying`/`IsHarvestingWood`/`IsHarvestingFood`/`IsBuilding` |
| Tooling de editor | ✅ | 5 scripts en `Assets/_Scripts/Editor/` — generadores de prefabs (recursos, drop-offs, defensivos) y setup de managers en escena |
| IA enemiga | 🔶 | `EnemyController` + `EnemyPatrolState` — solo patrulla; no ataca ni avanza hacia el pueblo |
| Estado de partida | ⬜ | `GameStateManager`/`GameManager` existen como esqueleto; nada dispara victoria ni derrota |
| HUD / UI | ⬜ | No existe. Todo el feedback es por consola |

---

## 4. Objetivos del vertical slice

Los 6 requisitos trazados y su estado. Detalle y trazabilidad completa en [Docs/Roadmap-VerticalSlice.md](Docs/Roadmap-VerticalSlice.md).

1. **Unidades melee y de rango** — ✅ Sistema resuelto. Falta contenido (prefabs por tipo, balance).
2. **Workers que recolectan y depositan** — ✅ Loop cerrado de punta a punta, verificado en play mode con `Lumbermill`/`Farm` reales.
3. **Edificios de distinto tipo** — 🔶 Drop-offs listos; muros con colocación implementada; faltan Town Center, producción, torres, límite poblacional e indexado por categoría.
4. **Loop recolección → inversión** — 🔶 Inversión en edificios cierra; falta producción de unidades con costo.
5. **Creación, interacción y muerte de unidades** — 🔶 Muerte y combate resueltos; creación en runtime no existe.
6. **Core loop con victoria y derrota** — ⬜ No empezado. Sin oleadas, sin caravanas, sin condiciones de fin.

### Fases de implementación

| Fase | Estado | Contenido |
|---|---|---|
| 0 — Cerrar loop de economía | ✅ | Registro de drop-offs, depósito al inventario, gasto de recursos |
| 1 — Construcción de edificios | ✅ mínimo | Ghost, validación, costo, cimiento, obra por worker. Falta menú real (hoy hotkeys Numpad) y barra de progreso |
| 2 — Edificios funcionales por tipo | ⬜ | Town Center (rol triple), producción de workers y militares, límite poblacional, torre defensiva, indexado por categoría en `BuildingManager` |
| 3 — Enemigos y tower defense | ⬜ | `EnemyController` heredando de `UnitController`, `EnemyAttackState`, avance al HQ, spawner de rondas |
| 4 — Caravanas y recursos estratégicos | ⬜ | Enum separado de recursos estratégicos, ronda tipo caravana, un tipo funcional de punta a punta |
| 5 — Militarización | ⬜ | Manager del índice soldados vs trabajadores, ponderando producción y selección de caravana |
| 6 — Victoria y derrota | ⬜ | Derrota por Town Center destruido, victoria por N rondas, `Victory` en el enum `GameState` |
| 7 — Bootstrap de partida | ⬜ | Estado inicial (TC construido, workers, recursos), cámara centrada, mapa fijo a mano |
| 8 — HUD de partida | ⬜ | Minimapa, panel de unidades, recursos, menú de edificios, población, anuncio de ronda, índice de Militarización, pantalla de fin |

De la Fase 3 en adelante cada fase separa **Mínimo jugable** de **Extensión**, para poder cerrar el game loop completo con contenido mínimo antes de sumar profundidad.

**Fase 8 es transversal**: cada ítem del HUD depende de una fase distinta y se puede asignar por separado. Minimapa y panel de información de unidades no dependen de nada — se pueden empezar hoy.

---

## 5. Estado del diseño

Lo técnico y lo diseñado avanzan por carriles separados. Índice de lo que falta diseñar: [Docs/GDD-Pendientes.md](Docs/GDD-Pendientes.md).

### Decidido (con documento propio)
- ✅ **Oleadas y Caravanas** — [Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md): loop de rondas con prep-time, roster de enemigos (Raiders, Siege engines, Shock units) con targeting propio por tipo, caravanas como tienda temporal con precios dinámicos y bonificación de consuelo, recursos estratégicos en sistema paralelo, eje Militarización-Cultura, victoria/derrota.
- ✅ **Balance de economía (wave 1)** — [Design-EconomyBalance.md](Docs/Design-EconomyBalance.md): Lumbermill y Farm a 100 Wood / 25s, worker a 20 Food, Town Center con rol triple (drop-off universal + único productor de workers + HQ), y la tensión de Food entre workers y militar.
- ✅ **Editor de mapas y generación procedural** — [Design-MapEditorAndProceduralGeneration.md](Docs/Design-MapEditorAndProceduralGeneration.md): `MapData` como formato común, editor con pincel sobre Scene view, generador con A* perturbado y Poisson disk sampling. Fuera del vertical slice, diseñado para después.

### Parcialmente definido
- 🔶 **Roster de edificios y unidades** — económicos decididos; faltan militares, defensivos y culturales (Barracks, torres, talleres) con sus costos y tiempos. Bloqueado por la cola de producción de Fase 2.

### Sin empezar
- ⬜ Curva de progresión y dificultad (cuántas rondas, cómo escala cada oleada, boss encounters).
- ⬜ Estructura de meta-partida (run independiente vs progresión persistente).
- ⬜ UI/UX más allá del HUD de rondas (menú principal, pausa, paneles).
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
- Generación procedural del mapa y el editor de mapas que la precede (ya diseñados, ver sección 5).

### Direcciones futuras ya declaradas en la documentación
Decisiones de arquitectura tomadas y escritas, pendientes de implementación:
- `EnemyController` va a heredar de `UnitController` para reusar el combate genérico en vez de duplicarlo.
- `BuildingManager` va a indexar edificios por categoría (dropoff / económico / cultural / militar / defensivo), generalizando `GetNearestDropOff`, para que cada tipo de enemigo resuelva su objetivo prioritario.
- `ResourceType` queda acotado a recursos recolectables; los estratégicos usan enum y sistema propios.
- `FactionDataSO` existe sin uso — candidato natural para los valores de arranque de partida (Fase 7).

---

## 7. Pendientes técnicos conocidos

Cosas identificadas y anotadas en la documentación, no bloqueantes pero acumulables:

- Costo y tiempo de construcción de `Palisade`, `Gate` y `ArcherTower` sin definir; muros sin probar en escena todavía.
- Punto de entrada de construcción son hotkeys mock (Numpad 1/2/3), a reemplazar por hotkeys estilo AoE2 y menú real.
- Logs de consola que sustituyen feedback visual (carga del worker, depósitos) — a sacar cuando exista HUD.
- Pool de ghosts del muro se instancia/destruye cada frame, sin pooling persistente — primer candidato a optimizar si pesa.
- `FloatEventChannelSO`/`IntEventChannelSO` declarados con el tipo en el nombre pero sin el payload cableado; sin consumidores.
- El índice [Docs/Scripts/README.md](Docs/Scripts/README.md) no lista `EnemyController` ni `EnemyPatrolState`, aunque ambos tienen su `.md` escrito.

---

## 8. Mapa de documentación

| Documento | Qué contiene |
|---|---|
| [README.md](README.md) | Presentación pública del repo (desactualizado, ver sección 1) |
| [CLAUDE.md](CLAUDE.md) | Convenciones de trabajo, estilo de código, política de docs y commits |
| [Docs/Roadmap-VerticalSlice.md](Docs/Roadmap-VerticalSlice.md) | Scope y fases del vertical slice — fuente de verdad del estado de implementación |
| [Docs/GDD-Pendientes.md](Docs/GDD-Pendientes.md) | Índice de lo que falta diseñar para un GDD completo |
| [Docs/Design-WavesAndCaravans.md](Docs/Design-WavesAndCaravans.md) | Diseño del gimmick central |
| [Docs/Design-EconomyBalance.md](Docs/Design-EconomyBalance.md) | Números de costo/tiempo y su razonamiento |
| [Docs/Design-MapEditorAndProceduralGeneration.md](Docs/Design-MapEditorAndProceduralGeneration.md) | Diseño de las herramientas de mapa (post-slice) |
| [Docs/Scripts/](Docs/Scripts/README.md) | Un `.md` por script, espejando `Assets/_Scripts/` |
