using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject selectionRing; // Referencia al GameObject del anillo de selección

    private void Start()
    {
        if (selectionRing != null) selectionRing.SetActive(false);
    }

    public void OnSelect()
    {
        Debug.Log($"Unidad {name} Seleccionada");
        if (selectionRing != null) selectionRing.SetActive(true);
    }

    public void OnDeselect()
    {
        Debug.Log($"Unidad {name} Deseleccionada");
        if (selectionRing != null) selectionRing.SetActive(false);
    }
}
