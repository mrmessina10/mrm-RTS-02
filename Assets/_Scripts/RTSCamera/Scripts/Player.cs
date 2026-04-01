using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.Rendering;


namespace Assets.RTSCamera.Scripts
{
    public class Player : MonoBehaviour
    {
        [Header("Architecture")]
        [SerializeField] private InputReader inputReader;

        [Header("Movement Settings")]
        [SerializeField] float moveSpeed = 20f;
        [SerializeField] AnimationCurve moveSpeedZoomCurve = AnimationCurve.Linear(0f, 0.5f, 1f, 1f);
        //cambio la velocidad de movimiento segun el nivel de zoom, para que la camara se mueva mas lento cuando esta mas cerca del objetivo y mas rapido cuando esta mas lejos

        [SerializeField] float acceleration = 10f;
        [SerializeField] float deceleration = 20f;

        [Space(10)]
        [SerializeField] float edgeScrollMargin = 12f;

        Vector2 edgeScrollInput;

        Vector3 velocity = Vector3.zero;

        [Header("Orbit Settings")]
        [SerializeField] float orbitSensitivity = 0.5f;
        [SerializeField] float orbitSmoothing = 5f;

        [Header("Zoom Settings")]
        [SerializeField] float zoomSpeed = 0.3f;
        [SerializeField] float zoomSmoothing = 5f;

        float currentZoomSpeed = 0f;

        public float ZoomLevel //valor entre 0 y 1, donde 0 es zoom in y 1 zoom out
        {
            get
            {
                InputAxis axis = orbitalFollow.RadialAxis;

                return Mathf.InverseLerp(axis.Range.x, axis.Range.y, axis.Value);
            }
        }


        [Header("Components")]
        [SerializeField] Transform cameraTarget;
        [SerializeField] CinemachineOrbitalFollow orbitalFollow;

        Vector2 moveInput;
        Vector2 lookInput;
        float zoomInput;
        bool isRotatingCamera = false;
        Vector2 currentMousePosition;

        #region Event Subscription

        private void OnEnable() //subscribo a los canales de InputReader
        {
            if (inputReader == null)
            {
                Debug.LogError("InputReader reference is missing on Player script.");
                return;
            }

            inputReader.MoveEvent += HandleMove;
            inputReader.LookEvent += HandleLook;
            inputReader.CameraZoomEvent += HandleZoom;
            inputReader.RotateCameraEvent += HandleRotateCamera;
            inputReader.PointerPositionEvent += HandlePointerPosition;
        }

        private void OnDisable() //desubscribo para evitar memory leaks
        {
            if (inputReader == null)
                return;

            inputReader.MoveEvent -= HandleMove;
            inputReader.LookEvent -= HandleLook;
            inputReader.CameraZoomEvent -= HandleZoom;
            inputReader.RotateCameraEvent -= HandleRotateCamera;
            inputReader.PointerPositionEvent -= HandlePointerPosition;
        }

        #endregion

        #region Input Handlers
        private void HandleMove(Vector2 newMoveInput) => moveInput = newMoveInput;

        private void HandleLook(Vector2 newLookInput) => lookInput = newLookInput;

        private void HandleZoom(float newZoomInput) => zoomInput = newZoomInput;

        private void HandleRotateCamera(bool isPressed) => isRotatingCamera = isPressed;

        private void HandlePointerPosition(Vector2 position) => currentMousePosition = position;
        #endregion

        //========================= Legacy controls ==============================
        //#region Input

        //Vector2 moveInput;
        //Vector2 lookInput;
        //Vector2 scrollInput;
        //bool middleClickInput = false;

        //void OnMove(InputValue value)
        //{
        //    moveInput = value.Get<Vector2>();
        //}

        //void OnLook(InputValue value)
        //{
        //    lookInput = value.Get<Vector2>();
        //}
        //void OnScrollWheel(InputValue value)
        //{
        //    scrollInput = value.Get<Vector2>();
        //}

        //void OnMiddleClick(InputValue value)
        //{
        //    middleClickInput = value.isPressed;
        //}

        //#endregion

        #region Unity Methods

        private void LateUpdate() //uso LateUpdate para asegurarme de que el movimiento de la cámara se aplique después que todo lo demas y asi reducir stuttering
        {
            float deltaTime = Time.unscaledDeltaTime;

            if (!Application.isEditor)
            {
                UpdateEdgeScrolling();
            }

            UpdateMovement(deltaTime);
            UpdateOrbit(deltaTime);
            UpdateZoom(deltaTime);
        }
        #endregion

        #region Control Methods

        void UpdateMovement(float deltaTime)
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = Camera.main.transform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 inputVector = new Vector3(moveInput.x + edgeScrollInput.x, 0, moveInput.y + edgeScrollInput.y);
            inputVector.Normalize();

            float zoomMultiplier = moveSpeedZoomCurve.Evaluate(ZoomLevel);

            Vector3 targetVelocity = moveSpeed * zoomMultiplier * inputVector;

            if (inputVector.sqrMagnitude > 0.01f)
            {
                velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * deltaTime);
            }
            else
            {
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * deltaTime);
            }

            Vector3 motion = deltaTime * velocity;

            cameraTarget.position += forward * motion.z + right * motion.x;

        }

        void UpdateOrbit(float deltaTime)
        {
            Vector2 orbitInput = lookInput * (isRotatingCamera ? 1f : 0f);
            // Vector2 orbitInput = lookInput * (middleClickInput ? 1f : 0f); // legacy

            orbitInput *= orbitSensitivity;

            InputAxis horizontalAxis = orbitalFollow.HorizontalAxis;
            InputAxis verticalAxis = orbitalFollow.VerticalAxis;

            horizontalAxis.Value = Mathf.Lerp(horizontalAxis.Value, horizontalAxis.Value + orbitInput.x, orbitSmoothing * deltaTime);
            verticalAxis.Value = Mathf.Lerp(verticalAxis.Value, verticalAxis.Value - orbitInput.y, orbitSmoothing * deltaTime); //eje Y invertido para que el movimiento sea más intuitivo

            //horizontalAxis.Value = Mathf.Clamp(horizontalAxis.Value, horizontalAxis.Range.x, horizontalAxis.Range.y);
            verticalAxis.Value = Mathf.Clamp(verticalAxis.Value, verticalAxis.Range.x, verticalAxis.Range.y);

            orbitalFollow.HorizontalAxis = horizontalAxis;
            orbitalFollow.VerticalAxis = verticalAxis;
        }

        void UpdateZoom(float deltaTime)
        {
            InputAxis axis = orbitalFollow.RadialAxis;

            float targetZoomSpeed = 0f;

            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetZoomSpeed = zoomSpeed * Mathf.Clamp(zoomInput, -1f, 1f);
            }

            // == legacy ==
            //if (Mathf.Abs(scrollInput.y) > 0.01f)
            //{
            //    targetZoomSpeed = zoomSpeed * scrollInput.y;
            //}

            currentZoomSpeed = Mathf.Lerp(currentZoomSpeed, targetZoomSpeed, zoomSmoothing * deltaTime);

            axis.Value -= currentZoomSpeed;
            axis.Value = Mathf.Clamp(axis.Value, axis.Range.x, axis.Range.y);

            orbitalFollow.RadialAxis = axis;
        }

        void UpdateEdgeScrolling()
        {
            Vector2 mousePosition = currentMousePosition;

            // == legacy ==
            //Vector2 mousePosition = Mouse.current.position.ReadValue(); //metodo del paquete de inputs obtiene la posicion del mouse en pantalla

            edgeScrollInput = Vector2.zero;

            if (mousePosition.x < edgeScrollMargin)
            {
                edgeScrollInput.x = -1f;
            }
            else if (mousePosition.x > Screen.width - edgeScrollMargin)
            {
                edgeScrollInput.x = 1f;
            }
            else
            {
                edgeScrollInput.x = 0f;

            }

            if (mousePosition.y < edgeScrollMargin)
            {
                edgeScrollInput.y = -1f;
            }
            else if (mousePosition.y > Screen.height - edgeScrollMargin)
            {
                edgeScrollInput.y = 1f;
            }
            else
            {
                edgeScrollInput.y = 0f;
            }
        }
        #endregion
    }
}
