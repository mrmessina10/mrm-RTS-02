using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// Capacidad composable de producir unidades: cola FIFO que cobra el costo al encolar, avanza el tiempo solo del
// pedido que está al frente y al completarlo instancia la unidad en el punto de salida del edificio.
public class UnitProducer : MonoBehaviour, IUnitProducer
{
    private const float SpawnSampleRadius = 3f;

    [SerializeField] private BuildingDataSO buildingData;
    [SerializeField] private Transform spawnPoint;

    private readonly Queue<UnitDataSO> productionQueue = new Queue<UnitDataSO>();
    private float elapsedProductionTime;

    public int QueuedCount => productionQueue.Count;
    public float CurrentProgress
    {
        get
        {
            if (productionQueue.Count == 0) return 0f;

            float productionTime = productionQueue.Peek().ProductionTime;
            return productionTime <= 0f ? 1f : Mathf.Clamp01(elapsedProductionTime / productionTime);
        }
    }

    private void Update()
    {
        if (productionQueue.Count == 0) return;

        elapsedProductionTime += Time.deltaTime;
        if (elapsedProductionTime < productionQueue.Peek().ProductionTime) return;

        SpawnUnit(productionQueue.Dequeue());
        elapsedProductionTime = 0f;
    }

    public bool TryEnqueueUnit(UnitType unitType)
    {
        if (!isActiveAndEnabled) return false;

        UnitDataSO unitData = FindProducibleUnit(unitType);
        if (unitData == null) return false;

        if (unitData.UnitPrefab == null)
        {
            Debug.LogWarning($"[UnitProducer] {unitData.DisplayName} no tiene UnitPrefab asignado.");
            return false;
        }

        if (productionQueue.Count >= buildingData.ProductionQueueCapacity)
        {
            Debug.Log($"[UnitProducer] Cola de {name} llena ({productionQueue.Count}/{buildingData.ProductionQueueCapacity}).");
            return false;
        }

        if (ResourceManager.Instance == null || !ResourceManager.Instance.HasEnoughResources(unitData.ProductionCost))
        {
            Debug.Log($"[UnitProducer] Recursos insuficientes para {unitData.DisplayName}.");
            return false;
        }

        foreach (var cost in unitData.ProductionCost)
        {
            ResourceManager.Instance.TrySpend(cost.Type, cost.Amount);
        }

        productionQueue.Enqueue(unitData);
        Debug.Log($"[UnitProducer] {unitData.DisplayName} en cola de {name} ({productionQueue.Count}/{buildingData.ProductionQueueCapacity}).");
        return true;
    }

    private UnitDataSO FindProducibleUnit(UnitType unitType)
    {
        foreach (var unitData in buildingData.ProducibleUnits)
        {
            if (unitData != null && unitData.UnitType == unitType) return unitData;
        }
        return null;
    }

    private void SpawnUnit(UnitDataSO unitData)
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, SpawnSampleRadius, NavMesh.AllAreas))
        {
            spawnPosition = hit.position;
        }

        Instantiate(unitData.UnitPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"[UnitProducer] {unitData.DisplayName} producido en {name}.");
    }
}
