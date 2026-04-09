using UnityEngine;
using UnityEngine.AI;

public class WorkerController : UnitController
{
    [field: Header("Harvesting Stats")]
    // Estas propiedades son configurables desde el inspector, pero solo se pueden modificar dentro de esta clase
    [field: SerializeField] public int MaxCarryCapacity { get; private set; } = 10;
    [field: SerializeField] public float HarvestRate { get; private set; } = 1f;
    [field: SerializeField] public int HarvestAmountPerCycle { get; private set; } = 1;
    [field: SerializeField] public float AutoSeekRadius { get; private set; } = 15f;
    [field: SerializeField] public LayerMask ResourceMask { get; private set; }

    [HideInInspector] public IHarvestable currentResourceNode;
    [HideInInspector] public ResourceType currentCarriedType;
    [HideInInspector] public int currentCarriedAmount = 0;

    public override void SetTarget(IInteractable newTarget)
    {
        if (newTarget.Type == InteractionType.Harvest && newTarget is IHarvestable resourceNode)
        {
            // Regla de diseño: si se reasigna a otro tipo de recurso, se destruye lo que se llevaba
            if (currentCarriedAmount > 0 && currentCarriedType != resourceNode.ResourceType)
            {
                currentCarriedAmount = 0;
            }

            currentResourceNode = resourceNode;
            currentCarriedType = resourceNode.ResourceType;

            // transicion hacia estado de movimiento a un recurso
            ChangeState(new WorkerMoveToResourceState(this)); //descomentar con los estados implementados
        }
        else
        {
            // Si el nuevo objetivo no es un nodo de recurso, se limpia el estado de recolección actual y delega la logica a UnitController
            base.SetTarget(newTarget);
        }
    }

    public override void SetCommand(Vector3 destination)
    // Al hacer click derecho en el suelo, se cancela cualquier objetivo de recolección actual para evitar conflictos de estados
    {
        currentResourceNode = null;
        base.SetCommand(destination);
    }
    public bool TryFindNearbyResourceNode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, AutoSeekRadius, ResourceMask);
        float closestDistance = float.MaxValue;
        IHarvestable closestNode = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IHarvestable node) && node.ResourceType == currentCarriedType && !node.IsDepleted)
            {
                float distance = Vector3.Distance(transform.position, node.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNode = node;
                }
            }
        }
        if (closestNode != null)
        {
            currentResourceNode = closestNode;
            return true;
        }
        return false;
    }
}