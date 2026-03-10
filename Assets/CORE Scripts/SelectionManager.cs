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

    [Header("Box Selection UI")]
    [SerializeField] private RectTransform selectionBoxUI; // referencia al UI element que muestra el área de selección
    [SerializeField] private float dragThreshold = 6f; // distancia mínima para considerar que se está arrastrando

    [Header("Multi-Selection Settings")]
    [SerializeField] private float doubleClickThreshold = 0.3f; // tiempo máximo entre clicks para considerar un doble click
    private float lastClickTime = 0f;

    private Vector2 currentMousePosition;
    private Vector2 startMousePosition;

    private List<ISelectable> selectedUnits = new List<ISelectable>();

    private bool isBoxSelecting = false;

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

    private void Update()
    {
        if (inputReader.IsLeftClickHeld)
        {
            if (!isBoxSelecting && Vector2.Distance(startMousePosition, currentMousePosition) > dragThreshold)
            {
                isBoxSelecting = true;
                if (selectionBoxUI != null) selectionBoxUI.gameObject.SetActive(true);
            }

            if (isBoxSelecting)
            {
                UpdateSelectionBoxUI();
            }
        }
    }

    private void UpdateSelectionBoxUI()
    {
        if (selectionBoxUI == null) return;

        float width = currentMousePosition.x - startMousePosition.x;
        float height = currentMousePosition.y - startMousePosition.y;

        selectionBoxUI.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBoxUI.anchoredPosition = startMousePosition + new Vector2(width / 2, height / 2);
    }

    private void ReleaseBoxSelection(bool isShiftHeld)
    {
        if (!isShiftHeld) DeselectAll();

        // VALIDACIÓN DE SEGURIDAD: Evita el NullReferenceException si el Manager no existe
        if (GlobalUnitManager.Instance == null)
        {
            Debug.LogError("GlobalUnitManager.Instance is null! Asegúrate de que el script GlobalUnitManager esté en un GameObject en la escena.");
            return;
        }

        Vector2 min = Vector2.Min(startMousePosition, currentMousePosition);
        Vector2 max = Vector2.Max(startMousePosition, currentMousePosition);
        Rect selectionRect = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);

        foreach (ISelectable selectable in GlobalUnitManager.Instance.AllSelectables)
        {
            if (selectable is MonoBehaviour monoSelectable)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(monoSelectable.transform.position);

                if (selectionRect.Contains(screenPos))
                {
                    if (!selectedUnits.Contains(selectable))
                    {
                        selectable.OnSelect();
                        selectedUnits.Add(selectable);
                    }
                }
            }
        }
    }

    private void SingleClickSelection(bool isShiftHeld)
    {
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
        {
            if (hit.collider.TryGetComponent<ISelectable>(out ISelectable newSelection))
            {
                bool isDoubleClick = (Time.time - lastClickTime) <= doubleClickThreshold; // Verifico si el tiempo entre clicks es menor al umbral para considerar un doble click
                lastClickTime = Time.time; // Actualizo el tiempo del último click para detectar futuros dobles clicks

                // Si es un doble clic, miramos el tipo de la unidad y ejecutamos el barrido
                if (isDoubleClick && hit.collider.TryGetComponent<UnitSelectionHandler>(out UnitSelectionHandler clickedUnit))
                {
                    SelectAllVisibleOfType(clickedUnit.Type, isShiftHeld);
                    return; // Cortamos aquí para no ejecutar la selección individual
                }

                if (isShiftHeld)
                {
                    if (selectedUnits.Contains(newSelection))
                    {
                        newSelection.OnDeselect();
                        selectedUnits.Remove(newSelection);
                    }
                    else
                    {
                        newSelection.OnSelect();
                        selectedUnits.Add(newSelection);
                    }
                }
                else
                {
                    DeselectAll();
                    newSelection.OnSelect();
                    selectedUnits.Add(newSelection);
                }
                return;
            }
        }

        if (!isShiftHeld) DeselectAll();
    }

    private void SelectAllVisibleOfType(UnitType targetType, bool isShiftHeld)
    {
        if (!isShiftHeld) DeselectAll();

        // 1. Buscamos instantáneamente en el diccionario si hay unidades de este tipo
        if (GlobalUnitManager.Instance.UnitsByType.TryGetValue(targetType, out List<UnitSelectionHandler> unitsOfSameType))
        {
            // 2. Iteramos SOLO sobre esa lista reducida (Ej: iteramos 20 guerreros en lugar de 500 objetos totales)
            foreach (UnitSelectionHandler unit in unitsOfSameType)
            {
                // Como UnitSelectionHandler hereda de MonoBehaviour, ya tenemos acceso directo a transform
                Vector3 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);

                if (screenPos.z > 0 &&
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height)
                {
                    if (!selectedUnits.Contains(unit))
                    {
                        unit.OnSelect();
                        selectedUnits.Add(unit);
                    }
                }
            }
        }
    }

    private void HandleSelect()
    {
        bool isShiftHeld = inputReader.IsShiftHeld;

        if (inputReader.IsLeftClickHeld)
        {
            startMousePosition = currentMousePosition;
        }
        else
        {
            if (isBoxSelecting)
            {
                ReleaseBoxSelection(isShiftHeld);
            }
            else
            {
                SingleClickSelection(isShiftHeld);
            }

            isBoxSelecting = false;
            if (selectionBoxUI != null) selectionBoxUI.gameObject.SetActive(false);
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

    private void HandleMoveCommand()
    {
        if (selectedUnits.Count == 0) return;

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemiesMask))
        {
            if (hitEnemy.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitEnemy.point, Color.red, 1f);
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hitInteractable, Mathf.Infinity, interactablesMask))
        {
            if (hitInteractable.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitInteractable.point, Color.yellow, 1f);
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundMask))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hitGround.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                ExecuteCommandOnSelected(controller => controller.SetCommand(navHit.position));
                Debug.DrawLine(mainCamera.transform.position, navHit.position, Color.green, 0.5f);
            }
        }
    }

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