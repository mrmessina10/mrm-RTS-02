using UnityEngine;
using System.Collections.Generic;

// Catálogo de datos de solo lectura de un tipo de unidad producible: qué es, qué cuesta, cuánto tarda en salir y qué prefab instanciar
[CreateAssetMenu(fileName = "New Unit Data", menuName = "ScriptableObjects/UnitDataSO")]
public class UnitDataSO : ScriptableObject
{
    [field: SerializeField] public UnitType UnitType { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public GameObject UnitPrefab { get; private set; }
    [field: SerializeField] public List<ResourceCost> ProductionCost { get; private set; }

    [Tooltip("Segundos que tarda en producirse una vez que llega al frente de la cola")]
    [field: SerializeField] public float ProductionTime { get; private set; }
}
