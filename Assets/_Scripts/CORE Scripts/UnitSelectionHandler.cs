using Unity.VisualScripting;
using UnityEngine;
    public enum UnitType
    {
        None,
        Worker,
        Melee,
        Ranged,
        Siege,
        Building
    }

public class UnitSelectionHandler : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject selectionRing;

    [UnitHeaderInspectable("Unit Settings")]
    [SerializeField] private UnitType unitType = UnitType.None;

    public UnitType Type => unitType;

    private void Start()
    {
        if (selectionRing != null) selectionRing.SetActive(false);
    }

    // --- REGISTRO EN EL MANAGER ---

    // Usamos OnEnable y OnDisable en lugar de Start/OnDestroy porque es más robusto.
    // Si desactivas una unidad temporalmente dejará de ser seleccionable.

    private void OnEnable()
    {
        // Nos aseguramos de que el Manager exista antes de intentar registrarnos
        if (GlobalUnitManager.Instance != null)
        {
            GlobalUnitManager.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        // Al morir o desactivarse, la unidad se borra de la lista de seleccionables

        if (GlobalUnitManager.Instance != null)
        {
            GlobalUnitManager.Instance.Unregister(this);
        }
    }

    // --- INTERFAZ ISelectable ---
    public void OnSelect()
    {
        //Debug.Log($"Unidad {name} Seleccionada");
        if (selectionRing != null) selectionRing.SetActive(true);
    }

    public void OnDeselect()
    {
        //Debug.Log($"Unidad {name} Deseleccionada");
        if (selectionRing != null) selectionRing.SetActive(false);
    }
}
