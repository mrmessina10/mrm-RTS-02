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

    [Header("Control Groups")]
    private Dictionary<int, List<ISelectable>> controlGroups = new Dictionary<int, List<ISelectable>>(); // Diccionario para almacenar grupos de control (1-9)

    [Header("Formation Settings")]
    [SerializeField] private float formationSpacing = 1.5f; // Distancia entre unidades

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

        inputReader.AssignGroupEvent += AssignControlGroup;
        inputReader.SelectGroupEvent += SelectControlGroup;
    }

    private void OnDisable()
    {
        if (inputReader == null)
            return;

        inputReader.PointerPositionEvent -= HandlePointerPosition;
        inputReader.SelectEvent -= HandleSelect;
        inputReader.CommandEvent -= HandleMoveCommand;

        inputReader.AssignGroupEvent -= AssignControlGroup;
        inputReader.SelectGroupEvent -= SelectControlGroup;
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
        // Mientras haya un modo de colocación activo (edificio único, muro), el click lo consume ese controller, no la selección
        if (PlacementModeState.IsActive) return;

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
        // Mientras haya un modo de colocación activo (edificio único, muro), el click derecho lo consume ese controller, no un comando
        if (PlacementModeState.IsActive) return;

        if (selectedUnits.Count == 0) return;

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // 1. Raycast a Enemigos
        if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemiesMask))
        {
            if (hitEnemy.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitEnemy.point, Color.red, 1f);
                return;
            }
        }

        // 2. Raycast a Interactuables (Recursos, edificios, etc)
        if (Physics.Raycast(ray, out RaycastHit hitInteractable, Mathf.Infinity, interactablesMask))
        {
            if (hitInteractable.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                ExecuteCommandOnSelected(controller => controller.SetTarget(target));
                Debug.DrawLine(mainCamera.transform.position, hitInteractable.point, Color.yellow, 1f);
                return;
            }
        }

        // 3. Raycast al Suelo (Movimiento en Grilla con Rotación)
        if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundMask))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hitGround.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                List<UnitController> controllers = new List<UnitController>();
                Vector3 currentUnitsCenter = Vector3.zero;

                // Extraemos los controladores y sumamos sus posiciones para promediar
                foreach (var selection in selectedUnits)
                {
                    if (selection is MonoBehaviour mono && mono.TryGetComponent<UnitController>(out UnitController controller))
                    {
                        controllers.Add(controller);
                        currentUnitsCenter += controller.transform.position;
                    }
                }

                if (controllers.Count > 0)
                {
                    currentUnitsCenter /= controllers.Count; // Promedio = Centro de gravedad del grupo

                    List<Vector3> formationPositions = CalculateFormationPositions(navHit.position, currentUnitsCenter, controllers.Count, formationSpacing);

                    for (int i = 0; i < controllers.Count; i++)
                    {
                        controllers[i].SetCommand(formationPositions[i]);
                        Debug.DrawLine(mainCamera.transform.position, formationPositions[i], Color.green, 0.5f);
                    }
                }
            }
        }

        //if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemiesMask))
        //{
        //    if (hitEnemy.collider.TryGetComponent<IInteractable>(out IInteractable target))
        //    {
        //        ExecuteCommandOnSelected(controller => controller.SetTarget(target));
        //        Debug.DrawLine(mainCamera.transform.position, hitEnemy.point, Color.red, 1f);
        //        return;
        //    }
        //}

        //if (Physics.Raycast(ray, out RaycastHit hitInteractable, Mathf.Infinity, interactablesMask))
        //{
        //    if (hitInteractable.collider.TryGetComponent<IInteractable>(out IInteractable target))
        //    {
        //        ExecuteCommandOnSelected(controller => controller.SetTarget(target));
        //        Debug.DrawLine(mainCamera.transform.position, hitInteractable.point, Color.yellow, 1f);
        //        return;
        //    }
        //}

        //if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundMask))
        //{
        //    if (UnityEngine.AI.NavMesh.SamplePosition(hitGround.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
        //    {
        //        ExecuteCommandOnSelected(controller => controller.SetCommand(navHit.position));
        //        Debug.DrawLine(mainCamera.transform.position, navHit.position, Color.green, 0.5f);
        //    }
        //}
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

    private void AssignControlGroup(int groupIndex)
    {
        if (selectedUnits.Count == 0)
        {
            //     Debug.LogWarning($"[SelectionManager] Intento de guardar Grupo {groupIndex}, pero no hay unidades seleccionadas.");
            return;
        }

        selectedUnits.RemoveAll(unit => unit == null || (unit as MonoBehaviour) == null);
        controlGroups[groupIndex] = new List<ISelectable>(selectedUnits);

        // Debug.Log($"<color=green>[SelectionManager] Grupo {groupIndex} GUARDADO con {controlGroups[groupIndex].Count} unidades.</color>");
    }

    private void SelectControlGroup(int groupIndex)
    {
        if (controlGroups.TryGetValue(groupIndex, out List<ISelectable> group))
        {
            DeselectAll();
            group.RemoveAll(unit => unit == null || (unit as MonoBehaviour) == null);

            //Debug.Log($"<color=cyan>[SelectionManager] Grupo {groupIndex} CARGADO con {group.Count} unidades.</color>");

            foreach (var unit in group)
            {
                unit.OnSelect();
                selectedUnits.Add(unit);
            }
        }
        //else
        //{
        //    Debug.LogWarning($"[SelectionManager] El grupo {groupIndex} está vacío o no existe en el diccionario.");
        //}
    }

    private List<Vector3> CalculateFormationPositions(Vector3 targetCenter, Vector3 currentCenter, int unitCount, float spacing)
    {
        List<Vector3> positions = new List<Vector3>();
        if (unitCount == 0) return positions;

        // 1. Calculamos la dirección hacia la que van a caminar
        Vector3 moveDirection = (targetCenter - currentCenter).normalized;
        if (moveDirection == Vector3.zero) moveDirection = Vector3.forward; // Fallback

        // 2. Creamos una rotación basada en esa dirección
        Quaternion rotation = Quaternion.LookRotation(moveDirection);

        int columns = Mathf.CeilToInt(Mathf.Sqrt(unitCount));
        int rows = Mathf.CeilToInt((float)unitCount / columns);

        for (int i = 0; i < unitCount; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float xOffset = (col - (columns - 1) / 2f) * spacing;
            // Invertimos la lógica del Z para que la Fila 0 (row=0) tenga el Z positivo (Frente)
            float zOffset = ((rows - 1) / 2f - row) * spacing;

            Vector3 localOffset = new Vector3(xOffset, 0f, zOffset);

            // 3. Multiplicamos la rotación por el offset local para girar la grilla
            Vector3 worldOffset = rotation * localOffset;
            Vector3 gridPosition = targetCenter + worldOffset;

            if (UnityEngine.AI.NavMesh.SamplePosition(gridPosition, out UnityEngine.AI.NavMeshHit hit, spacing * 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                positions.Add(hit.position);
            }
            else
            {
                positions.Add(targetCenter);
            }
        }

        return positions;
    }
}