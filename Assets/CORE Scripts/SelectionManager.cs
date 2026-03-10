using UnityEngine;

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
    private ISelectable currentSelection;

    private void OnEnable()
    {
        if (inputReader == null)
            return;

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

        Debug.Log($"1. Input Recibido en pos: {currentMousePosition}");

        // Realiza un raycast desde la posición del mouse para detectar objetos seleccionables
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);

        // Si el raycast golpea un objeto en la capa de selección, intenta obtener el componente ISelectable
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectionMask))
        {
            if (hit.collider.TryGetComponent<ISelectable>(out ISelectable newSelection)) // Si el objeto golpeado tiene un componente ISelectable, procede a seleccionar
            {
                Debug.Log("3. Componente ISelectable encontrado");
                if (currentSelection != null && currentSelection != newSelection) // Si ya hay una selección actual y es diferente a la nueva selección, deselecciona la anterior
                {
                    currentSelection.OnDeselect();
                }

                currentSelection = newSelection;
                currentSelection.OnSelect();
                return;
            }
            else Debug.Log("2. El Raycast no golpeó nada (Revisar Layers)");
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

    private void HandleMoveCommand()
    {
        // 1. Verificaciones de Seguridad
        if (currentSelection == null) return;

        // CORRECCIÓN: Primero verificamos si la selección es un objeto de Unity (MonoBehaviour)
        // y si tiene el componente UnitController.
        UnitController controller = null;

        if (currentSelection is MonoBehaviour monoSelection)
        {
            monoSelection.TryGetComponent<UnitController>(out controller);
        }

        // Si no encontramos el controlador salimos.
        if (controller == null) return;

        // --- A PARTIR DE AQUÍ 'controller' YA EXISTE Y ES SEGURO USARLO ---

        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // =================================================================================
        // PRIORIDAD 1: ENEMIGOS (Combate)
        // =================================================================================
        if (Physics.Raycast(ray, out RaycastHit hitEnemy, Mathf.Infinity, enemiesMask))
        {
            if (hitEnemy.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                Debug.Log($"<color=red>COMANDO DE ATAQUE:</color> {hitEnemy.collider.name}");
                controller.SetTarget(target);
                Debug.DrawLine(mainCamera.transform.position, hitEnemy.point, Color.red, 1f);
                return;
            }
        }

        // =================================================================================
        // PRIORIDAD 2: INTERACTUABLES (Recolección / Construcción)
        // =================================================================================
        if (Physics.Raycast(ray, out RaycastHit hitInteractable, Mathf.Infinity, interactablesMask))
        {
            if (hitInteractable.collider.TryGetComponent<IInteractable>(out IInteractable target))
            {
                Debug.Log($"<color=yellow>COMANDO DE INTERACCIÓN:</color> {hitInteractable.collider.name}");
                controller.SetTarget(target);
                Debug.DrawLine(mainCamera.transform.position, hitInteractable.point, Color.yellow, 1f);
                return;
            }
        }

        // =================================================================================
        // PRIORIDAD 3: MOVIMIENTO (Suelo)
        // =================================================================================
        if (Physics.Raycast(ray, out RaycastHit hitGround, Mathf.Infinity, groundMask))
        {
            if (UnityEngine.AI.NavMesh.SamplePosition(hitGround.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                controller.SetCommand(navHit.position);
                Debug.DrawLine(mainCamera.transform.position, navHit.position, Color.green, 0.5f);
            }
        }
    }
}
