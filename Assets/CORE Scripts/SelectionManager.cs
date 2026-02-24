using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Architecture")]
    [SerializeField] private InputReader inputReader; // referencia al InputReader para suscribirse a eventos de entrada
    [SerializeField] private Camera mainCamera; // referencia a la cámara principal para realizar raycasts

    [Header("Settings")]
    [SerializeField] private LayerMask selectionMask; // capa para raycast de selección

    private Vector2 currentMousePosition;
    private ISelectable currentSelection;

    private void OnEnable()
    {
        if (inputReader == null)
            return;

        inputReader.PointerPositionEvent += HandlePointerPosition;
        inputReader.SelectEvent += HandleSelect;
    }

    private void OnDisable()
    {
        if (inputReader == null)
            return;

        inputReader.PointerPositionEvent -= HandlePointerPosition;
        inputReader.SelectEvent -= HandleSelect;
    }

    private void HandlePointerPosition(Vector2 position) // actualizo la posicion del mouse constantemente
    { 
        currentMousePosition = position;
    }

    private void HandleSelect() // Método llamado cuando se detecta un evento de selección (clic izquierdo)
    {
        // Realiza un raycast desde la posición del mouse para detectar objetos seleccionables
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // Si el raycast golpea un objeto en la capa de selección, intenta obtener el componente ISelectable
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
        {
            if (hit.collider.TryGetComponent<ISelectable>(out ISelectable newSelection)) // Si el objeto golpeado tiene un componente ISelectable, procede a seleccionar
            {
                if (currentSelection != null && currentSelection != newSelection) // Si ya hay una selección actual y es diferente a la nueva selección, deselecciona la anterior
                {
                    currentSelection.OnDeselect();
                }

                currentSelection = newSelection;
                currentSelection.OnSelect();
                return;
            }
        }

        DeselectCurrent(); //Si el rayo pega en nada seleccionable como el suelo o el vacio, limpio la seleccion
    }

    private void DeselectCurrent()
    {
        if (currentSelection != null)
        {
            currentSelection.OnDeselect();
            currentSelection = null;
        }
    }
}
