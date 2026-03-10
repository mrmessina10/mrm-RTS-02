using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [Header("Architecture")]
    [SerializeField] private InputReader inputReader; // referencia al InputReader para suscribirse a eventos de entrada
    [SerializeField] private Camera mainCamera; // referencia a la cámara principal para realizar raycasts

    [Header("Settings")]
    [SerializeField] private LayerMask selectionMask; // capa para raycast de selección
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask interactablesMask;
    [SerializeField] private LayerMask enemiesMask;


    private Vector2 currentMousePosition;

    private List<ISelectable> selectedUnits = new List<ISelectable>();
    private void OnEnable()
    {
        if (inputReader == null) return;

        inputReader.PointerPositionEvent += HandlePointerPosition;
        inputReader.SelectEvent += HandleSelect;
        inputReader.CommandEvent += HandleMoveCommand;
    }

    private void OnDisable()
    {
        if (inputReader == null)
            return;

        inputReader.PointerPositionEvent -= HandlePointerPosition;
        inputReader.SelectEvent -= HandleSelect;
        inputReader.CommandEvent -= HandleMoveCommand;
    }

    private void HandlePointerPosition(Vector2 position) // actualizo la posicion del mouse constantemente
    {
        currentMousePosition = position;
    }

    private void HandleSelect() // Método llamado cuando se detecta un evento de selección (clic izquierdo)
    {
        bool isShiftHeld = inputReader.IsShiftHeld;

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
        {
            if (hit.collider.TryGetComponent<ISelectable>(out ISelectable newSelection))
            {
                if (isShiftHeld)
                {
                    // Si shift está apretado...
                    if (selectedUnits.Contains(newSelection))
                    {
                        newSelection.OnDeselect();
                        selectedUnits.Remove(newSelection); // ... y el objeto ya está seleccionado, lo deselecciono y lo saco de la lista
                    }
                    else
                    {
                        newSelection.OnSelect();
                        selectedUnits.Add(newSelection); // ... y el objeto no está seleccionado, lo selecciono y lo agrego a la lista
                    }
                }
                else
                {
                    //click sin apretar shift limpia la seleccion y selecciona solo el nuevo objeto
                    DeselectAll();
                    newSelection.OnSelect();
                    selectedUnits.Add(newSelection);
                }
                return;
            }
        }

        if (!isShiftHeld)
        {
            DeselectAll();
        }

    }

    private void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            unit.OnDeselect();
        }
        selectedUnits.Clear();
    }

    // legacy (sin soporte para multi-selección)

    //private void DeselectCurrent()
    //{
    //    if (currentSelection != null)
    //    {
    //        currentSelection.OnDeselect();
    //        currentSelection = null;
    //    }
    //}

    private void HandleMoveCommand()
    {
        // si no hay unidades seleccionadas, no hacemos nada
        if (selectedUnits.Count == 0) return;

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // PRIORIDAD 1: Combate
        if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemiesMask))
        {
            if (hitEnemy.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                // Usamos el método auxiliar para no repetir el foreach 3 veces
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitEnemy.point, Color.red, 1f);
                return;
            }
        }

        // PRIORIDAD 2: Interactuables
        if (Physics.Raycast(ray, out RaycastHit hitInteractable, Mathf.Infinity, interactablesMask))
        {
            if (hitInteractable.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitInteractable.point, Color.yellow, 1f);
                return;
            }
        }

        // PRIORIDAD 3: Movimiento
        if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundMask))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hitGround.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                ExecuteCommandOnSelected(controller => controller.SetCommand(navHit.position));
                Debug.DrawLine(mainCamera.transform.position, navHit.position, Color.green, 0.5f);
            }
        }
    }
    

        // --- MÉTODO AUXILIAR PARA COMANDOS EN GRUPO ---
        // Recorre la lista, filtra los que tengan UnitController y les pasa la orden.
    private void ExecuteCommandOnSelected(System.Action<UnitController> commandAction)
    {
        foreach (var selection in selectedUnits)
        {
            if (selection is MonoBehaviour monoSelection && monoSelection.TryGetComponent<UnitController>(out UnitController controller))
            {
                commandAction(controller);
            }
        }
    }
}
