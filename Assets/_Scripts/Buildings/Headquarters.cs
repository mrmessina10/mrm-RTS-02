using UnityEngine;

// Marca al edificio como HQ del jugador: se registra en BuildingManager para que la IA de oleadas lo resuelva
// como objetivo principal. La derrota al destruirse la dispara el Health del mismo GameObject (onDeathChannel).
public class Headquarters : MonoBehaviour, IHeadquarters
{
    public Vector3 Position => transform.position;
    public InteractionType Type => InteractionType.None;

    private void OnEnable()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.RegisterHeadquarters(this);
        }
    }

    private void OnDisable()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterHeadquarters(this);
        }
    }

    public Transform GetTransform() => transform;
    public void Interact(UnitController unit) { }
}
