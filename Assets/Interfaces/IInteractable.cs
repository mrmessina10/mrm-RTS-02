using Unity.VisualScripting;
using UnityEngine;

public enum InteractionType
{
    None,
    Attack,
    Gather,
    Build,
    Repair
}

public interface IInteractable
{
    InteractionType Type { get; }
    Transform GetTransform(); // obtengo la posicion del objeto interactuable
    void Interact(UnitController unit); // Método para realizar la interacción, recibe el controlador de la unidad que interactúa


}
