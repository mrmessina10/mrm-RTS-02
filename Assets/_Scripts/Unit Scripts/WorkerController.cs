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

    // Buffer reutilizado entre llamadas para evitar el alloc de Physics.OverlapSphere
    private static readonly Collider[] overlapBuffer = new Collider[32];

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
        else if (newTarget is IDropOffPoint dropOffPoint)
        {
            // Force-drop: orden manual de depositar en un edificio puntual, sin esperar a llenar la capacidad de carga.
            // Cada campamento es de un único propósito/recurso: si no acepta lo que el worker lleva encima, la orden
            // se ignora directamente (no camina hacia el edificio ni interactúa, aunque terminara depositando 0).
            if (dropOffPoint.AcceptsResource(currentCarriedType))
            {
                ChangeState(new WorkerMoveToDropOffState(this, dropOffPoint));
            }
            else
            {
                Debug.Log($"[WorkerController] {name}: ese edificio no acepta {currentCarriedType}, se ignora la orden.");
            }
        }
        else if (newTarget is IConstructable constructionSite)
        {
            // Orden de construir: el worker se acerca al sitio y le dedica tiempo hasta terminarlo (WorkerBuildState).
            // Si ya está completo (IConstructable se auto-elimina al terminar) esto no debería dispararse nunca.
            ChangeState(new WorkerBuildState(this, constructionSite));
        }
        else
        {
            // Cualquier otro objetivo (ej. un enemigo) interrumpe la recolección y delega la lógica a UnitController.
            // La carga actual (currentCarriedAmount/currentCarriedType) no se toca: el worker la conserva hasta volver a
            // trabajar ese mismo recurso o recibir la orden de force-drop de arriba.
            base.SetTarget(newTarget);
        }
    }

    public override void SetCommand(Vector3 destination)
    // Al hacer click derecho en el suelo, se cancela cualquier objetivo de recolección actual para evitar conflictos de estados
    {
        currentResourceNode = null;
        base.SetCommand(destination);
    }
    // Refleja en el Animator si el worker lleva recursos encima, independiente de si está quieto o caminando
    public void UpdateCarryAnimation()
    {
        if (UnitAnimator != null)
        {
            UnitAnimator.SetBool("IsCarrying", currentCarriedAmount > 0);
        }
    }

    public bool TryFindNearbyResourceNode()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, AutoSeekRadius, overlapBuffer, ResourceMask);
        float closestDistance = float.MaxValue;
        IHarvestable closestNode = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapBuffer[i];
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