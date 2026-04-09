using UnityEngine;

public class ResourceNode: MonoBehaviour, IHarvestable
{
    [field: SerializeField] public ResourceType ResourceType { get; private set; } 
    //backing field para la propiedad ResourceType, con acceso público de solo lectura y asignable desde el inspector
    public bool IsDepleted => currentAmount <= 0;
    public Vector3 Position => transform.position;

    [SerializeField] private int maxCapacity = 500;
    private int currentAmount;

    public InteractionType Type => InteractionType.Harvest;

    private void Awake()
    {
        currentAmount = maxCapacity;
    }

    public int Harvest(int amountHarvested)
    {
        if (IsDepleted) return 0;

        // Min() evita que saquemos más recursos de los que quedan en el nodo
        int harvested = Mathf.Min(amountHarvested, currentAmount);
        currentAmount -= harvested;

        if (IsDepleted)
        {
            Debug.Log($"[ResourceNode] Nodo de {ResourceType} agotado.");
            Destroy(gameObject);
        }

        return harvested;
    }

    public Transform GetTransform() //de IInteractable
    {
        return transform;
    }
    public void Interact(UnitController unit)
    {
        // This method can be used for interactions like showing resource info or highlighting the node.
        Debug.Log($"[ResourceNode] Interacted with {ResourceType} node. Remaining: {currentAmount}");
    }
}
