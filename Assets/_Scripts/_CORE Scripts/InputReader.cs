using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "ScriptableObjects/InputReader", order = 1)]
public class InputReader : ScriptableObject, GameInput.IPlayerActions
{
    //camara
    public event UnityAction<Vector2> MoveEvent; //WASD
    public event UnityAction<Vector2> LookEvent; // mouse delta
    public event UnityAction<bool> RotateCameraEvent; // middle mouse button hold for camera rotation
    public event UnityAction<float> CameraZoomEvent; // mouse scroll wheel

    //RTS core
    public event UnityAction<Vector2> PointerPositionEvent; // mouse position in world space
    public event UnityAction SelectEvent; // left click
    public event UnityAction SelectCanceledEvent; // left click released for box selection
    public event UnityAction CommandEvent; // right click
    public event UnityAction MenuPauseEvent; // Pause key
    //public event UnityAction ShiftKeyEvent; // Shift key for multi-selection

    [Header("Modifiers")]
    public bool IsCtrlHeld { get; private set; } // Propiedad para verificar si Ctrl está presionado
    public bool IsShiftHeld { get; private set; } // Propiedad para verificar si Shift está presionado
    public bool IsLeftClickHeld { get; set; } // Propiedad para verificar si el clic izquierdo está presionado (para selección con caja)

    private GameInput gameInput;

    private void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new GameInput();

            gameInput.Player.SetCallbacks(this); //conexion de los eventos del input system con los métodos de esta clase
            gameInput.Player.Enable();
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.Player.Disable();
        }

    }

    public event System.Action<int> AssignGroupEvent;

    public event System.Action<int> SelectGroupEvent;

    // Mock de hotkeys de construcción (Numpad, uno por edificio) — a reemplazar por un sistema de hotkeys tipo AoE2
    public event System.Action<BuildingType> BuildRequestEvent;

    // Entra en modo "trazar muro" (WallPlacementController) — sin payload, solo hay un tipo de segmento de muro
    public event System.Action WallBuildRequestEvent;

    //=======================================================
    //  Implementacion de interfaz GameInput.IPlayerActions
    //=======================================================

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        IsLeftClickHeld = context.ReadValueAsButton(); // Actualiza el estado del clic izquierdo cada vez que se presiona o suelta

        if (context.started || context.canceled)
        {
            SelectEvent?.Invoke();
        }
    }

    public void OnPointerPosition(InputAction.CallbackContext context)
    {
        PointerPositionEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnCommand(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            CommandEvent?.Invoke();
    }

    public void OnMenuPause(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            MenuPauseEvent?.Invoke();
    }

    public void OnRotateCamera(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            RotateCameraEvent?.Invoke(true);
        else if (context.phase == InputActionPhase.Canceled)
            RotateCameraEvent?.Invoke(false);
    }

    public void OnCameraZoom(InputAction.CallbackContext context)
    {
        CameraZoomEvent?.Invoke(context.ReadValue<float>());
    }

    public void OnShiftKey(InputAction.CallbackContext context)
    {
        IsShiftHeld = context.ReadValueAsButton(); // Actualiza el estado de Shift cada vez que se presiona o suelta
    }

    public void OnCtrlKey(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsCtrlHeld = true;
        }
        else if (context.canceled)
        {
            IsCtrlHeld = false;
        }

        Debug.Log($"[InputReader] Estado del Ctrl: {IsCtrlHeld} | Fase: {context.phase}");
    }

    public void OnNumberKey(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Limpiamos el string por si acaso el jugador presiona el numpad
            string cleanKeyName = context.control.name.Replace("numpad", "");

            if (int.TryParse(cleanKeyName, out int groupNumber))
            {
                // ===============================================================================================
                // MODO TESTEO EN EDITOR (Usa SHIFT para evitar conflictos con atajos nativos del editor de Unity)
                // ===============================================================================================
                if (IsShiftHeld)
                {
                    AssignGroupEvent?.Invoke(groupNumber);
                }
                else
                {
                    SelectGroupEvent?.Invoke(groupNumber);
                }

                /*
                // ================================================================================================
                // MODO BUILD FINAL (Usa CTRL)
                // Descomentar esto y comentar el de arriba antes de buildear
                // ================================================================================================
                if (IsCtrlHeld)
                {
                    AssignGroupEvent?.Invoke(groupNumber);
                }
                else
                {
                    SelectGroupEvent?.Invoke(groupNumber);
                }
                */
            }
        }
    }

    public void OnBuildLumbermill(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        Debug.Log($"[InputReader] Build request: {BuildingType.Lumbermill}");
        BuildRequestEvent?.Invoke(BuildingType.Lumbermill);
    }

    public void OnBuildFarm(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        Debug.Log($"[InputReader] Build request: {BuildingType.Farm}");
        BuildRequestEvent?.Invoke(BuildingType.Farm);
    }

    public void OnBuildPalisade(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        Debug.Log("[InputReader] Wall build request: Palisade");
        WallBuildRequestEvent?.Invoke();
    }

    public void OnBuildGate(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        Debug.Log($"[InputReader] Build request: {BuildingType.Gate}");
        BuildRequestEvent?.Invoke(BuildingType.Gate);
    }
}
