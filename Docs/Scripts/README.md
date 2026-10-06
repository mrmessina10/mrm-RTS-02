# Documentación de Scripts

Índice de la documentación retroactiva de `Assets/_Scripts/`. Cada `.md` espeja la ubicación de su script correspondiente. Ver [CLAUDE.md](../../CLAUDE.md) para la política de documentación.

## Interfaces
- [IState](Interfaces/IState.md)
- [IInteractable](Interfaces/IInteractable.md)
- [IDamageable](Interfaces/IDamageable.md)
- [ISelectable](Interfaces/ISelectable.md)
- [IHeadquarters](Interfaces/IHeadquarters.md)
- [IUnitProducer](Interfaces/IUnitProducer.md)

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
- [UnitDataSO](ScriptableObjects/UnitDataSO.md)
- [MapDataSO](ScriptableObjects/MapDataSO.md)
- [MapPaletteSO](ScriptableObjects/MapPaletteSO.md)
- [MapGenerationRulesSO](ScriptableObjects/MapGenerationRulesSO.md)

## Map
- [MapRoot](Map/MapRoot.md)
- [MapMarker](Map/MapMarker.md)
- [MapPath](Map/MapPath.md)

## Map / Generation
- [MapGenerator](Map/Generation/MapGenerator.md)
- [MapGenerationRequest / MapGenerationCatalog](Map/Generation/MapGenerationRequest.md)
- [GeneratedMap](Map/Generation/GeneratedMap.md)
- [MapGenerationContext](Map/Generation/MapGenerationContext.md)
- [MapLayoutPlanner](Map/Generation/MapLayoutPlanner.md)
- [MapTerrainShaper](Map/Generation/MapTerrainShaper.md)
- [MapPathRouter](Map/Generation/MapPathRouter.md)
- [MapObjectScatterer](Map/Generation/MapObjectScatterer.md)
- [MapRuleChecker](Map/Generation/MapRuleChecker.md)
- [MapGenerationPreview](Map/Generation/MapGenerationPreview.md)
- [MapHeightField](Map/Generation/MapHeightField.md)
- [MapRandom](Map/Generation/MapRandom.md)
- [MapNoise](Map/Generation/MapNoise.md)

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
- [UnitProducer](Buildings/UnitProducer.md)
- [Headquarters](Buildings/Headquarters.md)
- [BuildingPlacement](Buildings/BuildingPlacement.md)
- [BuildingPlacementController](Buildings/BuildingPlacementController.md)
- [ConstructionSite](Buildings/ConstructionSite.md)
- [PlacementModeState](Buildings/PlacementModeState.md)
- [BuildingGhostUtility](Buildings/BuildingGhostUtility.md)
- [WallPlacementController](Buildings/WallPlacementController.md)

## Editor
- [ResourceNodePrefabGenerator](Editor/ResourceNodePrefabGenerator.md)
- [DropOffBuildingPrefabGenerator](Editor/DropOffBuildingPrefabGenerator.md)
- [CoreManagerSceneSetup](Editor/CoreManagerSceneSetup.md)
- [BuildingPlacementSceneSetup](Editor/BuildingPlacementSceneSetup.md)
- [DefensiveBuildingPrefabGenerator](Editor/DefensiveBuildingPrefabGenerator.md)
- [CityCenterPrefabGenerator](Editor/CityCenterPrefabGenerator.md)

## Editor / MapEditor
- [MapEditorWindow](Editor/MapEditor/MapEditorWindow.md)
- [MapEditorTool / MapEditorContext](Editor/MapEditor/MapEditorTool.md)
- [TerrainBrushUtility](Editor/MapEditor/TerrainBrushUtility.md)
- [TerrainSculptTool](Editor/MapEditor/TerrainSculptTool.md)
- [TerrainPaintTool](Editor/MapEditor/TerrainPaintTool.md)
- [WaterTool](Editor/MapEditor/WaterTool.md)
- [ObjectBrushTool](Editor/MapEditor/ObjectBrushTool.md)
- [PathTool](Editor/MapEditor/PathTool.md)
- [MarkerTool](Editor/MapEditor/MarkerTool.md)
- [BuildZoneTool](Editor/MapEditor/BuildZoneTool.md)
- [MapSceneSetup](Editor/MapEditor/MapSceneSetup.md)
- [MapCameraSetup](Editor/MapEditor/MapCameraSetup.md)
- [MapSceneSync](Editor/MapEditor/MapSceneSync.md)
- [MapValidator](Editor/MapEditor/MapValidator.md)
- [MapEditorAssetGenerator](Editor/MapEditor/MapEditorAssetGenerator.md)
- [MapGeneratorPanel](Editor/MapEditor/MapGeneratorPanel.md)
- [MapGenerationApplier](Editor/MapEditor/MapGenerationApplier.md)
