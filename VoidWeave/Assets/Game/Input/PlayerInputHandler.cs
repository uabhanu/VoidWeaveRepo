namespace Game.Input
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerInputHandler : MonoBehaviour
    {
        #region Variables

        private Camera _mainCamera;
        private PlayerInputActions _playerInputActions;

        public Vector2 CursorWorldPosition{get; private set;}
        public bool DashPressed{get; private set;}
        public bool DeployPressed{get; private set;}
        public Vector2 MovementInput{get; private set;}

        public static PlayerInputHandler Instance{get; private set;}

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if(Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _mainCamera = Camera.main;
            _playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();

            _playerInputActions.Player.Attack.performed += OnDeployPerformed;
            _playerInputActions.Player.Jump.performed += OnDashPerformed;
            _playerInputActions.Player.Look.performed += OnLookPerformed;
            _playerInputActions.Player.Move.performed += OnMovePerformed;
            _playerInputActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Attack.performed -= OnDeployPerformed;
            _playerInputActions.Player.Jump.performed -= OnDashPerformed;
            _playerInputActions.Player.Look.performed -= OnLookPerformed;
            _playerInputActions.Player.Move.performed -= OnMovePerformed;
            _playerInputActions.Player.Move.canceled -= OnMoveCanceled;

            _playerInputActions.Player.Disable();
        }

        private void LateUpdate()
        {
            DashPressed = false;
            DeployPressed = false;
        }

        #endregion

        #region My Input Event Listeners

        private void OnDashPerformed(InputAction.CallbackContext context) { DashPressed = true; }

        private void OnDeployPerformed(InputAction.CallbackContext context) { DeployPressed = true; }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            Vector2 screenPosition = context.ReadValue<Vector2>();

            if(_mainCamera) { CursorWorldPosition = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x , screenPosition.y , _mainCamera.nearClipPlane)); }
        }

        private void OnMovePerformed(InputAction.CallbackContext context) { MovementInput = context.ReadValue<Vector2>(); }

        private void OnMoveCanceled(InputAction.CallbackContext context) { MovementInput = Vector2.zero; }

        #endregion
    }
}