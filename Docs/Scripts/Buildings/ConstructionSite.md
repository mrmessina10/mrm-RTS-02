# ConstructionSite

`Assets/_Scripts/Buildings/ConstructionSite.cs`

Componente temporal que representa el estado "en construcción" de un edificio recién colocado por [BuildingPlacementController](BuildingPlacementController.md). Implementa [IConstructable](../Interfaces/IInteractable.md), así que un worker puede recibir la orden de construirlo igual que recolecta o deposita — mismo patrón de despacho por interfaz en [WorkerController.SetTarget](../Unit%20Scripts/WorkerController.md).

`Initialize(BuildingDataSO data)` se llama una sola vez, inmediatamente después de instanciar el prefab del edificio: guarda la referencia al SO (para leer `ConstructionTime`/`DisplayName`) y deja inerte al `DropOffBuilding` del mismo GameObject (`enabled = false`) — el edificio existe físicamente (bloquea NavMesh vía [BuildingPlacement](BuildingPlacement.md), que no se toca) pero no funciona todavía.

`AddBuildProgress(deltaTime)` acumula tiempo hasta `BuildingDataSO.ConstructionTime`. Como es simplemente sumar segundos por worker que esté trabajando ahí, varios workers construyendo el mismo sitio lo terminan proporcionalmente más rápido, sin lógica extra. Si `ConstructionTime` es `0` (default de `BuildingDataSO`, hoy el caso de Lumbermill/Farm), `IsComplete` es `true` desde `Initialize` — comportamiento "instantáneo" de compatibilidad hasta que se cargue un tiempo real.

Al completarse (`Complete()`): reactiva el `DropOffBuilding`, y **se destruye a sí mismo** (`Destroy(this)`, solo el componente) — el GameObject sigue siendo el edificio, ya sin rastro de que estuvo en construcción, y deja de matchear `IConstructable` para futuras órdenes.

**Extensión futura**: solo maneja `DropOffBuilding` porque es el único componente funcional que existe hoy. Cuando se agreguen edificios de producción/defensa (Fase 2), sus componentes se suman al mismo bloque de `Initialize`/`Complete`, no se crea un `ConstructionSite` por tipo.
