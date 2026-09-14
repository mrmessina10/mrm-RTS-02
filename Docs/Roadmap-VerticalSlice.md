# Roadmap — Vertical Slice

Scope del vertical slice jugable de esta etapa. Formato: Tower Defense con componentes de Colony Sim — se recolectan recursos para construir defensas y para expandir/habilitar el pueblo, con el gimmick central de Oleadas y Caravanas (ver [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md) para el diseño completo). Ver [README.md](../README.md) para la visión general del proyecto y [Docs/Scripts/README.md](Scripts/README.md) para la documentación técnica de cada script.

Leyenda: ✅ hecho · 🔶 parcial / a medio conectar · ⬜ no empezado

## Los 6 requisitos del vertical slice

### 1. Unidades con ataque melee y de rango
✅ Hecho. [MeleeAttackState](Scripts/StateMachines/MeleeAttackState.md) y [RangedAttackState](Scripts/StateMachines/RangedAttackState.md) sobre [UnitController](Scripts/Unit%20Scripts/UnitController.md), con [Projectile](Scripts/Unit%20Scripts/Projectile.md) para el disparo a distancia. Falta contenido (prefabs de unidad de cada tipo, balance), pero el sistema está resuelto.

### 2. Workers que recolectan y depositan
✅ Hecho y verificado en play mode. El ciclo recolectar → cargar → moverse a depósito → depositar cierra de punta a punta ([WorkerController](Scripts/Unit%20Scripts/WorkerController.md) + `WorkerMoveToResourceState` → `WorkerHarvestResourceState` → `WorkerMoveToDropOffState` → [DropOffBuilding](Scripts/Buildings/DropOffBuilding.md) → [BuildingManager](Scripts/_CORE%20Scripts/BuildingManager.md) → [ResourceManager](Scripts/_CORE%20Scripts/ResourceManager.md)), probado con `Lumbermill`/`Farm` reales en escena. Además: interrupción de la recolección con cualquier otra orden (conserva la carga sin auto-depositar) y force-drop manual respetando el tipo de recurso de cada campamento — comportamiento estilo Age of Empires 2. Pendiente, fuera del loop en sí: animaciones y HUD (hoy sustituidos por logs de consola).

### 3. Edificios de distinto tipo
🔶 Parcial. Existe el modelo de datos ([BuildingDataSO](Scripts/ScriptableObjects/BuildingDataSO.md): tipo, costo, prefab, footprint) y la validación de terreno/obstáculo NavMesh ([BuildingPlacement](Scripts/Buildings/BuildingPlacement.md)). Falta la lógica *funcional* de cada tipo:
- **Drop-off point**: ✅ [DropOffBuilding](Scripts/Buildings/DropOffBuilding.md) conectado (ver punto 2), con prefabs reales — `Lumbermill`/`Farm` (footprint 2x2, [DropOffBuildingPrefabGenerator](Scripts/Editor/DropOffBuildingPrefabGenerator.md)) — y construcción real por workers vía [ConstructionSite](Scripts/Buildings/ConstructionSite.md) (ver Fase 1). Falta `TownCenter` (acepta todos los recursos + producción de workers).
- **Producción de unidades** (Barracks): ⬜ no existe — ningún edificio instancia unidades.
- **Defensa** (torres): ⬜ no existe — ningún edificio ataca enemigos.
- **Límite poblacional**: ⬜ no existe — no hay concepto de población ni de cap en el proyecto.
- **Indexado por categoría** (dropoff/económico/cultural/militar/defensivo): ⬜ no existe — `BuildingManager` solo indexa `IDropOffPoint`. Necesario para que la IA de oleadas resuelva su objetivo prioritario (ver Fase 3).

### 4. Loop completo de recolección e inversión en construcción/creación
🔶 Parcial. El lado de "recolección" ya cierra (ver punto 2) y la "inversión" en edificios también: [BuildingPlacementController](Scripts/Buildings/BuildingPlacementController.md) resuelve ghost + validación + costo + instanciación (Fase 1, con hotkeys mock). Falta cola de producción de *unidades* con costo (Fase 2) para cerrar el otro lado de la inversión.

### 5. Loop completo de creación, interacción y muerte de unidades
🔶 Parcial. Muerte e interacción (combate) están resueltas ([Health](Scripts/_CORE%20Scripts/Health.md), `IDamageable`, ambos attack states). *Creación* solo existe manual (unidades ya puestas en la escena/prefabs); falta creación en runtime desde un edificio de producción.

### 6. Core loop con condiciones de victoria y derrota
⬜ No empezado. [GameStateManager](Scripts/_CORE%20Scripts/GameStateManager.md) tiene el enum de estados y el evento de cambio, pero nada dispara `GameOver` ni una condición de victoria; el enum tampoco contempla `Victory` todavía. Tampoco hay sistema de oleadas ni de caravanas: lo único que existe del lado enemigo es [EnemyController](Scripts/Unit%20Scripts/EnemyController.md) con [EnemyPatrolState](Scripts/StateMachines/EnemyPatrolState.md) — patrulla, no ataca ni avanza hacia el pueblo. El diseño completo de este loop (rondas, roster de enemigos, caravanas, Militarización) está en [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md); las fases 3 a 6 de abajo son su bajada a implementación.

---

## Fases propuestas

Orden por dependencia funcional, no por tiempo calendario. A partir de la Fase 3, cada fase separa **Mínimo jugable** (lo indispensable para poder jugar una partida completa del concepto de una punta a la otra, aunque sea con contenido mínimo) de **Extensión** (profundidad adicional del diseño completo en [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md), no bloqueante para tener el game loop en pie).

### Fase 0 — Cerrar el loop de economía existente ✅
Conectar lo que ya está construido antes de sumar sistemas nuevos.
- ✅ `DropOffBuilding.OnEnable/OnDisable` → registrar/desregistrar en `BuildingManager`.
- ✅ `DropOffBuilding.Deposit()` → `ResourceManager.AddResource()`.
- ✅ `ResourceManager.TrySpend(type, amount)` / `HasEnoughResources(List<ResourceCost>)`.

### Fase 1 — Construcción de edificios (inversión de recursos) ✅ mínimo funcional
- ✅ Punto de entrada mock: hotkeys Numpad 1/2 (`InputReader.BuildRequestEvent`) — a reemplazar por el sistema de hotkeys estilo AoE2 más adelante.
- ✅ Flujo de colocación interactiva: [BuildingPlacementController](Scripts/Buildings/BuildingPlacementController.md) — ghost siguiendo el mouse, validado en vivo con `BuildingPlacement.IsAreaBuildable`, confirmar/cancelar con click.
- ✅ Al confirmar: chequea costo (`BuildingDataSO.ConstructionCost`) contra `ResourceManager`, descuenta, instancia un cimiento — no el edificio terminado.
- ✅ Construcción real por workers: [ConstructionSite](Scripts/Buildings/ConstructionSite.md) + [WorkerBuildState](Scripts/StateMachines/WorkerBuildState.md) — seleccionar worker, click derecho sobre el cimiento, acumula `BuildingDataSO.ConstructionTime` hasta habilitar la funcionalidad real del edificio.
- Pendiente: UI real para elegir edificio (hoy son hotkeys fijos, no un menú), feedback más allá de logs/tinte del ghost, barra de progreso de construcción visible.

### Fase 2 — Edificios funcionales por tipo

**Mínimo jugable**
- **Producción** (Barracks): cola simple de creación de unidades con costo en recursos y tiempo de fabricación.
- **Límite poblacional**: contador de población actual/máxima; edificios "de vivienda" (o el mismo Town Center) aportan cap; producción bloqueada si se alcanza el límite. Es el input real de "soldados vivos"/"trabajadores vivos" que necesita Militarización (Fase 5).
- **Defensa** (torre): estructura estática con `IDamageable` + un estado de ataque a enemigos en rango — puede reutilizar la lógica de `RangedAttackState`/`Projectile` en vez de duplicarla.
- **Indexado por categoría en `BuildingManager`**: extender el registro más allá de `IDropOffPoint` para clasificar cada edificio como económico, cultural, militar o defensivo (ya declarado como dirección futura en [BuildingManager.md](Scripts/_CORE%20Scripts/BuildingManager.md)). Es requisito duro de la Fase 3: sin esto, Raiders y Siege engines no tienen cómo resolver su objetivo prioritario.

**Extensión**
- Talleres/edificios culturales productores de recursos estratégicos — se puede resolver junto con la Fase 4 si conviene, no necesita esperar al roster completo de edificios militares.

### Fase 3 — Enemigos y loop de tower defense

**Mínimo jugable**
- `EnemyController` pasa a heredar de `UnitController` (ya declarado como intención en [EnemyController.md](Scripts/Unit%20Scripts/EnemyController.md)) para reusar `GetAttackState`/`MeleeAttackState`/`RangedAttackState` en vez de reimplementar combate del lado enemigo.
- `EnemyAttackState` + comportamiento de avance hacia el Town Center (objetivo = HQ) en lugar de patrullaje puro.
- Al menos **un tipo de enemigo funcional** con targeting básico — Raiders es el candidato más simple (prioriza edificios económicos/culturales, evita defensas salvo que bloqueen el camino al HQ) para validar el loop de punta a punta.
- Spawner de oleadas con estructura de rondas: secuencia de rondas, prep-time, timer visible.

**Extensión**
- Roster completo de los tres tipos de [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md#2-oleadas-enemigas): sumar Siege engines (con la relación de escolta con Raiders) y Shock units (targeting de soldados/workers desarmados).
- Boss encounters seedeados en rondas específicas.
- Composición de oleada balanceada a mano ronda por ronda (contenido, no sistema).

### Fase 4 — Caravanas y recursos estratégicos (nueva)

**Mínimo jugable**
- Nuevo enum de recursos estratégicos, separado de `ResourceType` (ya declarado en [ResourceType.md](Scripts/Resources/ResourceType.md)), con al menos un edificio productor.
- Una ronda puede resolverse como "Caravana" en vez de oleada (frecuencia fija al principio, la versión dinámica/balanceable es refinamiento posterior).
- Un tipo de caravana funcional de punta a punta: llega por el camino, presenta un catálogo mínimo, resuelve la compra (o la bonificación de consuelo si no se puede pagar), se va.

**Extensión**
- Múltiples tipos de caravana con catálogo y recurso estratégico propios.
- Precios dinámicos (encarecen con cada compra) y objetivos de caravana para resetearlos.
- Selección de tipo de caravana por aleatoriedad ponderada (arranca como aleatorio simple/uniforme).

### Fase 5 — Militarización (nueva)

**Mínimo jugable**
- Manager nuevo (mismo patrón singleton que el resto) que calcule un índice simplificado a partir de soldados vivos vs trabajadores vivos (los dos inputs que ya existen gracias a la Fase 2).
- El índice pondera, aunque sea de forma lineal simple, la producción económica/militar y la selección de tipo de caravana (Fase 4).

**Extensión**
- Sumar el resto de los inputs de [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md#5-militarización--el-eje-central-de-tensión) (ratio de edificios por categoría, bonificaciones/objetivos de caravanas anteriores).
- Mostrar el índice en tiers en vez del valor crudo.
- Definir el nombre final del stat.

### Fase 6 — Condiciones de victoria y derrota
- Derrota: Town Center destruido → dispara `GameOver` vía `GameStateManager`/`GameManager` (el canal `Channel_GameOver` ya existe).
- Victoria: sobrevivir N rondas — requiere agregar `Victory` al enum `GameState` de `GameStateManager` (hoy solo tiene `GameOver`).
- Al menos un log/evento claro de fin de partida; UI de pantalla de victoria/derrota puede quedar como placeholder visual.

### Fase 7 — Bootstrap de partida
- Estado inicial clásico de RTS: Town Center ya construido, un puñado de workers y unidades de combate, recursos iniciales — reutilizar [FactionDataSO](Scripts/ScriptableObjects/FactionDataSO.md) (hoy sin uso) para los valores de arranque.
- Cámara centrada en el área inicial al comenzar la partida.
- Mapa/camino fijo, armado a mano — la generación procedural (y las herramientas de editor que necesita antes) queda fuera del vertical slice, ver notas de scope.

### Fase 8 — HUD de partida
No es parte de los 6 requisitos explícitos, pero es necesaria para que el vertical slice se pueda jugar sin mirar la consola. Cada ítem depende de datos de una fase distinta — se pueden asignar y ejecutar por separado, sin esperar a que el roadmap completo esté terminado:

- **Minimapa** — sin dependencia, se puede empezar ya. Necesita posiciones de unidades/edificios ([GlobalUnitManager](Scripts/_CORE%20Scripts/GlobalUnitManager.md)) y del terreno.
- **Información en tiempo real de unidades** (panel de selección) — sin dependencia, se puede empezar ya. La selección ya existe (`SelectionManager`/`ISelectable`/`UnitSelectionHandler`).
- **HUD de recursos** — depende de Fase 0. `ResourceManager.OnResourceChanged` ya está listo para escucharse. Extender para recursos estratégicos depende de Fase 4.
- **Edificios disponibles** (menú de construcción) — depende de Fase 1 (flujo de colocación/costo).
- **Población actual/máxima** — depende de Fase 2 (límite poblacional).
- **Anuncio de próxima ronda** (oleada vs caravana, y tipo) — depende de Fase 3/4 (spawner con estructura de rondas). La ventana de visión anticipada ampliable es extensión, no mínimo.
- **Índice de Militarización** — depende de Fase 5.
- **Pantalla de victoria/derrota** — depende de Fase 6.

---

## Notas de scope

- No incluido en este vertical slice (fuera de alcance salvo que se decida sumarlo después): múltiples facciones/jugadores, mejoras/tecnología, más de un tipo de unidad melee/ranged, guardado de partida, generación procedural del mapa/camino (y las herramientas de editor que esa generación necesita diseñar primero).
- `FloatEventChannelSO`/`IntEventChannelSO` (payload no cableado) están declarados pero sin uso — quedan disponibles si alguna fase los necesita, no requieren trabajo previo. (`WorkerState`, enum redundante con la FSM de clases, se eliminó.)
- El diseño completo del gimmick (todos los tipos de enemigo, caravana y todos los inputs de Militarización) vive en [Design-WavesAndCaravans.md](Design-WavesAndCaravans.md) — este roadmap solo prioriza qué parte de ese diseño es indispensable para tener el game loop jugable de punta a punta versus qué es profundidad a sumar después.
