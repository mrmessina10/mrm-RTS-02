using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    private UnitController unitController;

    private void Awake()
    {
        // Busca el cerebro lógico en el objeto padre
        unitController = GetComponentInParent<UnitController>();
    }

    // Este es el método exacto que llamará la línea de tiempo de la animación
    public void OnAttackImpact()
    {
       // Debug.Log("Eslabón 1: ¡El Evento de Animación se disparó!"); // Si no sale esto, el evento está mal configurado.

        if (unitController != null)
        {
            unitController.TriggerAttackDamage();
        }
        else
        {
            Debug.LogError("Error: El Relay no encontró el UnitController en el objeto padre.");
        }
    }
}
