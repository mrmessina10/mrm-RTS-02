using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("Architecture")]
    [SerializeField] private InputReader inputReader; // referencia al InputReader para suscribirse a eventos de entrada
    [SerializeField] private Camera mainCamera; // referencia a la cámara principal para realizar raycasts

    [Header("Settings")]
    [SerializeField] private LayerMask selectionMask; // capa para raycast de selección
    [SerializeField] private LayerMask groundMask;

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

        // DIBUJAR EL RAYO VISUALMENTE (Solo se ve en la ventana SCENE, no en Game)
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
                else Debug.LogWarning($"Golpeé {hit.collider.name} pero NO TIENE el script UnitSelectionHandler o ISelectable.");

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
        // Si no hay una selección actual, no se puede emitir un comando de movimiento
        if (currentSelection == null)
            return;

        // Realiza un raycast desde la posición del mouse para detectar el punto en el suelo donde el jugador hace click derecho
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            // verifico que el objeto seleccionado tenga el componente UnitMovement para emitir el comando de movimiento
            if (currentSelection is MonoBehaviour selectedObject && selectedObject.TryGetComponent<UnitMovement>(out UnitMovement movement))
            {
                // Verifico que el punto de destino esté en la NavMesh para evitar que las unidades intenten moverse a lugares no navegables
                if (UnityEngine.AI.NavMesh.SamplePosition(hit.point, out UnityEngine.AI.NavMeshHit navHit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    movement.MoveTo(hit.point);

                    Debug.DrawLine(mainCamera.transform.position, hit.point, Color.green, 2f); // donde hace click derecho el jugador

                }
                else
                {
                    Debug.DrawLine(mainCamera.transform.position, hit.point, Color.yellow, 2f); // punto de destino no navegable
                }
            }
        }
    }
}
