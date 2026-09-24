using UnityEngine;
using System.Collections.Generic;

public enum BuildingType
{
    TownCenter,
    Lumbermill,
    Farm,
    Barracks,
    ArcherTower,
    Palisade,
    Gate
}

// Catálogo de datos de solo lectura de un tipo de edificio: qué es, qué cuesta construirlo y qué prefab instanciar
[CreateAssetMenu(fileName = "New Building Data", menuName = "ScriptableObjects/BuildingDataSO")]
public class BuildingDataSO : ScriptableObject
{
    [field: SerializeField] public BuildingType BuildingType { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public GameObject BuildingPrefab { get; private set; }
    [field: SerializeField] public List<ResourceCost> ConstructionCost { get; private set; }

    [Tooltip("Segundos que tarda en construirse una vez confirmada la colocación")]
    [field: SerializeField] public float ConstructionTime { get; private set; }

    [Tooltip("Tamaño del landprint en celdas de grilla (ancho x profundidad) que ocupa el edificio sobre el terreno")]
    [field: SerializeField] public Vector2Int Footprint { get; private set; } = Vector2Int.one;
}
