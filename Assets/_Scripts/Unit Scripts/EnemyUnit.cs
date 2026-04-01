using System;
using UnityEngine;

public class EnemyUnit : MonoBehaviour, IInteractable
{
    public InteractionType Type => InteractionType.Attack;

    public Transform GetTransform()
    {
        return transform;
    }

    public void Interact (UnitController controller)
    {
        Debug.Log($"Enemy {name} is being attacked by {controller.name}");
    }
}
