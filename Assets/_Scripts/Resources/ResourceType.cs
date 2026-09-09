public enum ResourceType
{
    Wood,
    Food,
    Gold,
    Stone
}

public interface IHarvestable : IInteractable
{
    ResourceType ResourceType { get; }
    int Harvest(int amount);
    bool IsDepleted { get; }
    UnityEngine.Vector3 Position { get; }
}

public interface IDropOffPoint : IInteractable
{
    bool AcceptsResource(ResourceType resourceType);
    void Deposit(ResourceType resourceType, int amount);
    UnityEngine.Vector3 Position { get; }
}

// Par recurso/cantidad reutilizable para cualquier costo (edificios, unidades, mejoras)
[System.Serializable]
public struct ResourceCost
{
    public ResourceType Type;
    public int Amount;
}
