using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    private List<IDropOffPoint> activeDropOffs = new List<IDropOffPoint>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterDropOff(IDropOffPoint dropOff)
    {
        if (!activeDropOffs.Contains(dropOff))
            activeDropOffs.Add(dropOff);
    }

    public void UnregisterDropOff(IDropOffPoint dropOff)
    {
        activeDropOffs.Remove(dropOff);
    }

    // Encuentra el edificio más cercano que acepte el recurso específico
    public IDropOffPoint GetNearestDropOff(Vector3 workerPosition, ResourceType type)
    {
        IDropOffPoint nearest = null;
        float minDistance = float.MaxValue;

        foreach (var dropOff in activeDropOffs)
        {
            if (!dropOff.AcceptsResource(type)) continue;

            float distance = Vector3.Distance(workerPosition, dropOff.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = dropOff;
            }
        }

        return nearest;
    }
}
