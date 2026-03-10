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
    public event UnityAction ShiftKeyEvent; // Shift key for multi-selection

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
}
