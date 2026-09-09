using UnityEngine;
using System.Collections.Generic;

public class DropOffBuilding : MonoBehaviour, IDropOffPoint
{
    [Header("Drop-Off Settings")]
    [Tooltip("Lista de recursos que este edificio acepta (ej.: town center todos, lumber camp solo madera)")]
    [SerializeField] private List<ResourceType> acceptedResources;

    // Implementación de IDropOffPoint
    public Vector3 Position => transform.position;
    public InteractionType Type => InteractionType.None; // No es interactuable directamente

    private void OnEnable()
    {
        // Registrar este edificio como punto de entrega para los recursos que acepta al construirlo o spawnearlo
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.RegisterDropOff(this);
        }
    }

    private void OnDisable()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterDropOff(this);
        }
    }

    public bool AcceptsResource(ResourceType resourceType)
    {
        return acceptedResources.Contains(resourceType);
    }

    public void Deposit(ResourceType resourceType, int amount)
    {
        ResourceManager.Instance.AddResource(resourceType, amount);
        Debug.Log($"[DropOffBuilding] Depositando {amount} de {resourceType} en {gameObject.name}");
    }

    public Transform GetTransform() => transform;
    public void Interact (UnitController unit)
    {
        // logica para cuando el jugador hace click derecho manualmente en el edificio para forzar la entrega de recursos
    }
}
