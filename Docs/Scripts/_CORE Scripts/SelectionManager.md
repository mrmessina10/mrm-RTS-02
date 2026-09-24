# SelectionManager

`Assets/_Scripts/_CORE Scripts/SelectionManager.cs`

El script más grande del proyecto: maneja toda la interacción de selección y comandos del jugador, escuchando los eventos de [InputReader](InputReader.md).

Responsabilidades:
- **Selección simple**: raycast bajo el mouse contra `selectionMask`, soporta shift-click (agregar/quitar de la selección) y doble click (seleccionar todas las unidades visibles del mismo `UnitType`, usando el diccionario O(1) de [GlobalUnitManager](GlobalUnitManager.md)).
- **Box selection**: arrastre del mouse por encima de `dragThreshold` activa un rectángulo UI; al soltar, selecciona todo lo que caiga dentro (recorre `AllSelectables` de `GlobalUnitManager`, proyectando posición mundo → pantalla).
- **Grupos de control (1-9)**: `AssignControlGroup`/`SelectControlGroup`, guardados en un diccionario local `int → List<ISelectable>`.
- **Comandos** (`HandleMoveCommand`, click derecho): tres raycasts en orden de prioridad — enemigos, interactuables (recursos/edificios), y suelo. Sobre suelo calcula posiciones de formación en grilla ([`CalculateFormationPositions`](#calculateformationpositions)) para que las unidades seleccionadas no se apilen en el mismo punto.

`HandleSelect`/`HandleMoveCommand` cortan temprano si [PlacementModeState.IsActive](../Buildings/PlacementModeState.md) es `true` — mientras haya un modo de colocación activo (edificio único o muro), el click lo consume ese controller (confirmar/cancelar), no la selección ni un comando de movimiento.

## CalculateFormationPositions
Genera una grilla cuadrada (columnas = `ceil(sqrt(n))`) centrada y orientada hacia la dirección de movimiento del grupo, y valida cada posición contra el NavMesh (`NavMesh.SamplePosition`), con fallback al punto central si no encuentra superficie válida.

## Relación con otros scripts
Ejecuta comandos vía la interfaz de [UnitController](../Unit%20Scripts/UnitController.md) (`SetTarget`/`SetCommand`), nunca conoce las clases concretas de unidad (Worker, Melee, Ranged).

## Fuente / patrón
Raycast prioritario en capas (enemigo > interactuable > suelo) es un patrón común de RTS de click-to-command. El cálculo de formación en grilla orientada es una implementación ad-hoc, no basada en una librería externa.
