using UnityEngine;

public class ResourceNode: MonoBehaviour //, IHarvestable
{
    [field: SerializeField] public ResourceType ResourceType { get; private set; }
    [SerializeField] private int currentAmount = 500;

    public bool IsDepleted => currentAmount <= 0;
    public Vector3 Position => transform.position;

    public int Harvest(int amount)
    {
        if (IsDepleted) return 0;

        int harvestedAmount = Mathf.Min(amount, currentAmount);
        currentAmount -= harvestedAmount;

        Debug.Log($"[ResourceNode] Harvested {harvestedAmount} {ResourceType}. Remaining: {currentAmount}");

        return harvestedAmount;
    }

    public void Interact(GameObject interactor)
    {
        // This method can be used for interactions like showing resource info or highlighting the node.
        Debug.Log($"[ResourceNode] Interacted with {ResourceType} node. Remaining: {currentAmount}");
    }
}
