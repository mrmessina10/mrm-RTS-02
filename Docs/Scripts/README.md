# Documentación de Scripts

Índice de la documentación retroactiva de `Assets/_Scripts/`. Cada `.md` espeja la ubicación de su script correspondiente. Ver [CLAUDE.md](../../CLAUDE.md) para la política de documentación.

## Interfaces
- [IState](Interfaces/IState.md)
- [IInteractable](Interfaces/IInteractable.md)
- [IDamageable](Interfaces/IDamageable.md)
- [ISelectable](Interfaces/ISelectable.md)

## Data
- [DamageData](Data/DamageData.md)

## _CORE Scripts
- [GameStateManager](_CORE%20Scripts/GameStateManager.md)
- [GameManager](_CORE%20Scripts/GameManager.md)
- [GlobalUnitManager](_CORE%20Scripts/GlobalUnitManager.md)
- [Health](_CORE%20Scripts/Health.md)
- [InputReader](_CORE%20Scripts/InputReader.md)
- [UnitSelectionHandler](_CORE%20Scripts/UnitSelectionHandler.md)
- [SelectionManager](_CORE%20Scripts/SelectionManager.md)
- [ResourceManager](_CORE%20Scripts/ResourceManager.md)
- [BuildingManager](_CORE%20Scripts/BuildingManager.md)

## ScriptableObjects
- [VoidEventChannelSO](ScriptableObjects/VoidEventChannelSO.md)
- [FloatEventChannelSO](ScriptableObjects/FloatEventChannelSO.md)
- [IntEventChannelSO](ScriptableObjects/IntEventChannelSO.md)
- [FactionDataSO](ScriptableObjects/FactionDataSO.md)
- [BuildingDataSO](ScriptableObjects/BuildingDataSO.md)

## RTSCamera
- [Player](RTSCamera/Scripts/Player.md)

## Unit Scripts
- [UnitController](Unit%20Scripts/UnitController.md)
- [WorkerController](Unit%20Scripts/WorkerController.md)
- [UnitMovement](Unit%20Scripts/UnitMovement.md)
- [AnimationEventRelay](Unit%20Scripts/AnimationEventRelay.md)
- [EnemyUnit](Unit%20Scripts/EnemyUnit.md)
- [Projectile](Unit%20Scripts/Projectile.md)

## StateMachines
- [StateMachine](StateMachines/StateMachine.md)
- [UnitIdleState](StateMachines/UnitIdleState.md)
- [UnitMoveState](StateMachines/UnitMoveState.md)
- [MeleeAttackState](StateMachines/MeleeAttackState.md)
- [RangedAttackState](StateMachines/RangedAttackState.md)
- [WorkerMoveToResourceState](StateMachines/WorkerMoveToResourceState.md)
- [WorkerHarvestResourceState](StateMachines/WorkerHarvestResourceState.md)
- [WorkerMoveToDropOffState](StateMachines/WorkerMoveToDropOffState.md)
- [WorkerBuildState](StateMachines/WorkerBuildState.md)

## Resources
- [ResourceType.cs (enums + interfaces)](Resources/ResourceType.md)
- [ResourceNode](Resources/ResourceNode.md)

## Buildings
- [DropOffBuilding](Buildings/DropOffBuilding.md)
- [BuildingPlacement](Buildings/BuildingPlacement.md)
- [BuildingPlacementController](Buildings/BuildingPlacementController.md)
- [ConstructionSite](Buildings/ConstructionSite.md)

## Editor
- [ResourceNodePrefabGenerator](Editor/ResourceNodePrefabGenerator.md)
- [DropOffBuildingPrefabGenerator](Editor/DropOffBuildingPrefabGenerator.md)
- [CoreManagerSceneSetup](Editor/CoreManagerSceneSetup.md)
- [BuildingPlacementSceneSetup](Editor/BuildingPlacementSceneSetup.md)
