using Unity.VisualScripting;
using UnityEngine;

public enum InteractionType
{
    None,
    Attack,
    Harvest,
    Build,
    Repair
}

public interface IInteractable
{
    InteractionType Type { get; }
    Transform GetTransform(); // obtengo la posicion del objeto interactuable
    void Interact(UnitController unit); // Método para realizar la interacción, recibe el controlador de la unidad que interactúa
}

public interface IConstructable : IInteractable
{
    bool IsComplete { get; }
    float BuildProgress { get; } // 0 a 1
    Vector3 Position { get; }
    void AddBuildProgress(float deltaTime);
}