# Roadmap — Vertical Slice

Scope del vertical slice jugable de esta etapa. Formato: Tower Defense con componentes de Colony Sim — se recolectan recursos para construir defensas y para expandir/habilitar el pueblo. Ver [README.md](../README.md) para la visión general del proyecto y [Docs/Scripts/README.md](Scripts/README.md) para la documentación técnica de cada script.

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

### 4. Loop completo de recolección e inversión en construcción/creación
🔶 Parcial. El lado de "recolección" ya cierra (ver punto 2) y la "inversión" en edificios también: [BuildingPlacementController](Scripts/Buildings/BuildingPlacementController.md) resuelve ghost + validación + costo + instanciación (Fase 1, con hotkeys mock). Falta cola de producción de *unidades* con costo (Fase 2) para cerrar el otro lado de la inversión.

### 5. Loop completo de creación, interacción y muerte de unidades
🔶 Parcial. Muerte e interacción (combate) están resueltas ([Health](Scripts/_CORE%20Scripts/Health.md), `IDamageable`, ambos attack states). *Creación* solo existe manual (unidades ya puestas en la escena/prefabs); falta creación en runtime desde un edificio de producción.

### 6. Core loop con condiciones de victoria y derrota
⬜ No empezado. [GameStateManager](Scripts/_CORE%20Scripts/GameStateManager.md) tiene el enum de estados y el evento de cambio, pero nada dispara `GameOver` ni una condición de victoria. Tampoco hay sistema de oleadas de enemigos: lo único que existe es [EnemyController](Scripts/Unit%20Scripts/EnemyController.md) con [EnemyPatrolState](Scripts/StateMachines/EnemyPatrolState.md) — patrulla, no ataca ni avanza hacia el pueblo.

---

## Fases propuestas

Orden por dependencia funcional, no por tiempo calendario.

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
- **Producción** (Barracks): cola simple de creación de unidades con costo en recursos y tiempo de fabricación.
- **Límite poblacional**: contador de población actual/máxima; edificios "de vivienda" (o el mismo Town Center) aportan cap; producción bloqueada si se alcanza el límite.
- **Defensa** (torre): estructura estática con `IDamageable` + un estado de ataque a enemigos en rango — puede reutilizar la lógica de `RangedAttackState`/`Projectile` en vez de duplicarla.

### Fase 3 — Enemigos y loop de tower defense
- `EnemyAttackState`: en vez de solo patrullar, el enemigo detecta y ataca unidades/edificios del jugador en rango.
- Comportamiento de "avance hacia el pueblo" (objetivo = Town Center u otro edificio clave) en lugar de patrullaje puro.
- Spawner de oleadas: puntos de spawn, cantidad y timing por oleada.

### Fase 4 — Condiciones de victoria y derrota
- Derrota: Town Center destruido (o población en 0) → dispara `GameOver` vía `GameStateManager`/`GameManager`.
- Victoria: sobrevivir N oleadas.
- Al menos un log/evento claro de fin de partida; UI de pantalla de victoria/derrota puede quedar como placeholder visual.

### Fase 5 — Bootstrap de partida
- Estado inicial clásico de RTS: Town Center ya construido, un puñado de workers y unidades de combate, recursos iniciales — reutilizar [FactionDataSO](Scripts/ScriptableObjects/FactionDataSO.md) (hoy sin uso) para los valores de arranque.
- Cámara centrada en el área inicial al comenzar la partida.

### Fase 6 — HUD de partida
No es parte de los 6 requisitos explícitos, pero es necesaria para que el vertical slice se pueda jugar sin mirar la consola. Cada ítem depende de datos de una fase distinta — se pueden asignar y ejecutar por separado, sin esperar a que el roadmap completo esté terminado:

- **Minimapa** — sin dependencia, se puede empezar ya. Necesita posiciones de unidades/edificios ([GlobalUnitManager](Scripts/_CORE%20Scripts/GlobalUnitManager.md)) y del terreno.
- **Información en tiempo real de unidades** (panel de selección) — sin dependencia, se puede empezar ya. La selección ya existe (`SelectionManager`/`ISelectable`/`UnitSelectionHandler`).
- **HUD de recursos** — depende de Fase 0. `ResourceManager.OnResourceChanged` ya está listo para escucharse.
- **Edificios disponibles** (menú de construcción) — depende de Fase 1 (flujo de colocación/costo).
- **Población actual/máxima** — depende de Fase 2 (límite poblacional).
- **Tiempo de partida** (cronómetro/oleada actual) — depende de Fase 3/4 (spawner de oleadas, condiciones de victoria/derrota).
- **Pantalla de victoria/derrota** — depende de Fase 4.

---

## Notas de scope

- No incluido en este vertical slice (fuera de alcance salvo que se decida sumarlo después): múltiples facciones/jugadores, mejoras/tecnología, más de un tipo de unidad melee/ranged, guardado de partida.
- `FloatEventChannelSO`/`IntEventChannelSO` (payload no cableado) están declarados pero sin uso — quedan disponibles si alguna fase los necesita, no requieren trabajo previo. (`WorkerState`, enum redundante con la FSM de clases, se eliminó.)
