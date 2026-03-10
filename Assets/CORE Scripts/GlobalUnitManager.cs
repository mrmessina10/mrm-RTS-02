using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // Asegura que este manager se inicialice antes que otros scripts que dependan de él
public class GlobalUnitManager : MonoBehaviour
{
    public static GlobalUnitManager Instance { get; private set; }

    // Lista original para el Box Selection que agarra de todo
    public List<ISelectable> AllSelectables { get; private set; } = new List<ISelectable>();

    // Diccionario para búsquedas instantáneas por Tipo (O(1) lookup)
    public Dictionary<UnitType, List<UnitSelectionHandler>> UnitsByType { get; private set; } = new Dictionary<UnitType, List<UnitSelectionHandler>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(ISelectable selectable)
    {
        AllSelectables.Add(selectable);

        // Si el objeto que se registra es una unidad con tipo, lo catalogamos en el Diccionario
        if (selectable is UnitSelectionHandler handler)
        {
            if (!UnitsByType.ContainsKey(handler.Type))
            {
                UnitsByType[handler.Type] = new List<UnitSelectionHandler>();
            }
            UnitsByType[handler.Type].Add(handler);
        }
    }

    public void Unregister(ISelectable selectable)
    {
        AllSelectables.Remove(selectable);

        // Lo removemos del catálogo específico al morir
        if (selectable is UnitSelectionHandler handler && UnitsByType.ContainsKey(handler.Type))
        {
            UnitsByType[handler.Type].Remove(handler);
        }
    }
}