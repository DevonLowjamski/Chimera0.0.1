using UnityEngine;
using Cinemachine;
using ProjectChimera.Core;
using ProjectChimera.Scripts.Cultivation;
using ProjectChimera.Scripts.Facilities;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
// Explicit alias to resolve PostProcessingController ambiguity
using PostProcessingController = ProjectChimera.Data.Environment.PostProcessingController;

namespace ProjectChimera.Scripts.Camera
{
    /// <summary>
    /// Advanced camera system providing multiple camera views, smooth transitions,
    /// and contextual camera behavior for different gameplay scenarios.
    /// </summary>
    public class AdvancedCameraController : MonoBehaviour
    {
        [Header("Camera Configuration")]
        [SerializeField] private CinemachineVirtualCamera _overviewCamera;
        [SerializeField] private CinemachineVirtualCamera _plantInspectionCamera;
        [SerializeField] private CinemachineVirtualCamera _facilityCamera;
        [SerializeField] private CinemachineVirtualCamera _constructionCamera;
        [SerializeField] private CinemachineVirtualCamera _firstPersonCamera;
        [SerializeField] private CinemachineVirtualCamera _cinematicCamera;
        [SerializeField] private CinemachineFreeLook _freeLookCamera;
        
        [Header("Camera Settings")]
        [SerializeField] private float _transitionDuration = 1.5f;
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _panSpeed = 3f;
        [SerializeField] private bool _enableCameraShake = true;
        [SerializeField] private bool _enableDynamicFOV = true;
        
        [Header("Camera Bounds")]
        [SerializeField] private Vector3 _boundsMin = new Vector3(-50f, 2f, -50f);
        [SerializeField] private Vector3 _boundsMax = new Vector3(50f, 30f, 50f);
        [SerializeField] private float _minZoomDistance = 5f;
        [SerializeField] private float _maxZoomDistance = 50f;
        
        [Header("Auto-Focus Settings")]
        [SerializeField] private bool _enableAutoFocus = true;
        [SerializeField] private float _autoFocusDelay = 2f;
        [SerializeField] private LayerMask _focusLayers = -1;
        
        [Header("Cinematic Settings")]
        [SerializeField] private AnimationCurve _cinematicMovementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _cinematicDuration = 3f;
        [SerializeField] private bool _enableCinematicBars = true;
        
        // Camera State
        private CameraMode _currentMode = CameraMode.Overview;
        private Transform _focusTarget;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isTransitioning = false;
        private bool _userControlActive = true;
        
        // Input State
        private Vector2 _mouseInput;
        private float _scrollInput;
        private bool _isDragging = false;
        private Vector3 _lastMousePosition;
        
        // Auto-Focus System
        private Coroutine _autoFocusCoroutine;
        private Transform _lastFocusTarget;
        
        // Camera Effects
        private CinemachineImpulseSource _impulseSource;
        private PostProcessingController _postProcessing;
        
        // Camera Positions and Orientations
        private Dictionary<CameraMode, CameraState> _cameraStates = new Dictionary<CameraMode, CameraState>();
        
        // Events
        public System.Action<CameraMode, CameraMode> OnCameraModeChanged;
        public System.Action<Transform> OnFocusTargetChanged;
        public System.Action<bool> OnCinematicModeChanged;
        
        // Properties
        public CameraMode CurrentMode => _currentMode;
        public Transform FocusTarget => _focusTarget;
        public bool IsTransitioning => _isTransitioning;
        public bool UserControlActive => _userControlActive;
        
        private void Awake()
        {
            InitializeCameraSystem();
        }
        
        private void Start()
        {
            SetupCameraStates();
            SetCameraMode(CameraMode.Overview, true);
        }
        
        private void Update()
        {
            if (!_userControlActive) return;
            
            HandleInput();
            UpdateCamera();
            CheckAutoFocus();
        }
        
        private void LateUpdate()
        {
            if (_enableDynamicFOV)
                UpdateDynamicFOV();
        }
        
        #region Initialization
        
        private void InitializeCameraSystem()
        {
            // Get or create Cinemachine cameras
            if (_overviewCamera == null)
                _overviewCamera = CreateVirtualCamera("Overview Camera", CameraMode.Overview);
            if (_plantInspectionCamera == null)
                _plantInspectionCamera = CreateVirtualCamera("Plant Inspection Camera", CameraMode.PlantInspection);
            if (_facilityCamera == null)
                _facilityCamera = CreateVirtualCamera("Facility Camera", CameraMode.Facility);
            if (_constructionCamera == null)
                _constructionCamera = CreateVirtualCamera("Construction Camera", CameraMode.Construction);
            if (_firstPersonCamera == null)
                _firstPersonCamera = CreateVirtualCamera("First Person Camera", CameraMode.FirstPerson);
            if (_cinematicCamera == null)
                _cinematicCamera = CreateVirtualCamera("Cinematic Camera", CameraMode.Cinematic);
            
            // Get impulse source for camera shake
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            if (_impulseSource == null)
                _impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            
            // Get post-processing controller
            _postProcessing = GetComponent<PostProcessingController>();
        }
        
        private CinemachineVirtualCamera CreateVirtualCamera(string name, CameraMode mode)
        {
            GameObject cameraGO = new GameObject(name);
            cameraGO.transform.SetParent(transform);
            
            var virtualCamera = cameraGO.AddComponent<CinemachineVirtualCamera>();
            virtualCamera.Priority = mode == CameraMode.Overview ? 10 : 0;
            
            // Configure based on camera mode
            ConfigureVirtualCamera(virtualCamera, mode);
            
            return virtualCamera;
        }
        
        private void ConfigureVirtualCamera(CinemachineVirtualCamera camera, CameraMode mode)
        {
            var transposer = camera.AddCinemachineComponent<CinemachineTransposer>();
            var composer = camera.AddCinemachineComponent<CinemachineComposer>();
            
            switch (mode)
            {
                case CameraMode.Overview:
                    camera.transform.position = new Vector3(0f, 15f, -10f);
                    camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
                    camera.m_Lens.FieldOfView = 60f;
                    transposer.m_FollowOffset = new Vector3(0f, 15f, -10f);
                    break;
                    
                case CameraMode.PlantInspection:
                    camera.transform.position = new Vector3(0f, 2f, -3f);
                    camera.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
                    camera.m_Lens.FieldOfView = 40f;
                    transposer.m_FollowOffset = new Vector3(0f, 2f, -3f);
                    break;
                    
                case CameraMode.Facility:
                    camera.transform.position = new Vector3(0f, 8f, -8f);
                    camera.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
                    camera.m_Lens.FieldOfView = 50f;
                    transposer.m_FollowOffset = new Vector3(0f, 8f, -8f);
                    break;
                    
                case CameraMode.Construction:
                    camera.transform.position = new Vector3(0f, 5f, -5f);
                    camera.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
                    camera.m_Lens.FieldOfView = 55f;
                    transposer.m_FollowOffset = new Vector3(0f, 5f, -5f);
                    break;
                    
                case CameraMode.FirstPerson:
                    camera.transform.position = new Vector3(0f, 1.8f, 0f);
                    camera.transform.rotation = Quaternion.identity;
                    camera.m_Lens.FieldOfView = 75f;
                    transposer.m_FollowOffset = new Vector3(0f, 1.8f, 0f);
                    break;
                    
                case CameraMode.Cinematic:
                    camera.transform.position = new Vector3(0f, 12f, -12f);
                    camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
                    camera.m_Lens.FieldOfView = 35f;
                    transposer.m_FollowOffset = new Vector3(0f, 12f, -12f);
                    break;
            }
            
            // Configure composer
            composer.m_TrackedObjectOffset = Vector3.zero;
            composer.m_LookaheadTime = 0.5f;
            composer.m_LookaheadSmoothing = 10f;
            composer.m_DeadZoneWidth = 0.1f;
            composer.m_DeadZoneHeight = 0.1f;
            composer.m_SoftZoneWidth = 0.8f;
            composer.m_SoftZoneHeight = 0.8f;
        }
        
        private void SetupCameraStates()
        {
            _cameraStates[CameraMode.Overview] = new CameraState
            {
                Position = new Vector3(0f, 15f, -10f),
                Rotation = Quaternion.Euler(45f, 0f, 0f),
                FieldOfView = 60f,
                AllowUserControl = true,
                TransitionDuration = 1.5f
            };
            
            _cameraStates[CameraMode.PlantInspection] = new CameraState
            {
                Position = new Vector3(0f, 2f, -3f),
                Rotation = Quaternion.Euler(15f, 0f, 0f),
                FieldOfView = 40f,
                AllowUserControl = true,
                TransitionDuration = 1f
            };
            
            _cameraStates[CameraMode.Facility] = new CameraState
            {
                Position = new Vector3(0f, 8f, -8f),
                Rotation = Quaternion.Euler(30f, 0f, 0f),
                FieldOfView = 50f,
                AllowUserControl = true,
                TransitionDuration = 1.2f
            };
            
            _cameraStates[CameraMode.Construction] = new CameraState
            {
                Position = new Vector3(0f, 5f, -5f),
                Rotation = Quaternion.Euler(25f, 0f, 0f),
                FieldOfView = 55f,
                AllowUserControl = true,
                TransitionDuration = 1f
            };
            
            _cameraStates[CameraMode.FirstPerson] = new CameraState
            {
                Position = new Vector3(0f, 1.8f, 0f),
                Rotation = Quaternion.identity,
                FieldOfView = 75f,
                AllowUserControl = true,
                TransitionDuration = 0.8f
            };
            
            _cameraStates[CameraMode.Cinematic] = new CameraState
            {
                Position = new Vector3(0f, 12f, -12f),
                Rotation = Quaternion.Euler(35f, 0f, 0f),
                FieldOfView = 35f,
                AllowUserControl = false,
                TransitionDuration = 2f
            };
        }
        
        #endregion
        
        #region Camera Control
        
        /// <summary>
        /// Set camera mode with optional immediate transition
        /// </summary>
        public void SetCameraMode(CameraMode mode, bool immediate = false)
        {
            if (_currentMode == mode && !immediate) return;
            
            var previousMode = _currentMode;
            _currentMode = mode;
            
            // Disable all cameras
            SetAllCamerasPriority(0);
            
            // Enable target camera
            var targetCamera = GetCameraForMode(mode);
            if (targetCamera != null)
            {
                targetCamera.Priority = 10;
                
                if (!immediate && !_isTransitioning)
                {
                    StartCoroutine(TransitionToCamera(targetCamera, _cameraStates[mode]));
                }
            }
            
            // Update user control state
            _userControlActive = _cameraStates[mode].AllowUserControl;
            
            OnCameraModeChanged?.Invoke(previousMode, mode);
            Debug.Log($"Camera mode changed from {previousMode} to {mode}");
        }
        
        /// <summary>
        /// Focus camera on specific target
        /// </summary>
        public void FocusOnTarget(Transform target, CameraMode? preferredMode = null)
        {
            if (target == null) return;
            
            _focusTarget = target;
            
            // Set appropriate camera mode if specified
            if (preferredMode.HasValue)
            {
                SetCameraMode(preferredMode.Value);
            }
            
            // Update current camera's follow target
            var currentCamera = GetCameraForMode(_currentMode);
            if (currentCamera != null)
            {
                currentCamera.Follow = target;
                currentCamera.LookAt = target;
            }
            
            OnFocusTargetChanged?.Invoke(target);
            
            // Reset auto-focus timer
            if (_autoFocusCoroutine != null)
            {
                StopCoroutine(_autoFocusCoroutine);
                _autoFocusCoroutine = null;
            }
            
            Debug.Log($"Camera focused on {target.name}");
        }
        
        /// <summary>
        /// Clear camera focus and return to free camera
        /// </summary>
        public void ClearFocus()
        {
            _focusTarget = null;
            
            var currentCamera = GetCameraForMode(_currentMode);
            if (currentCamera != null)
            {
                currentCamera.Follow = null;
                currentCamera.LookAt = null;
            }
            
            OnFocusTargetChanged?.Invoke(null);
        }
        
        /// <summary>
        /// Perform camera shake effect
        /// </summary>
        public void ShakeCamera(float intensity = 1f, float duration = 0.5f)
        {
            if (!_enableCameraShake) return;
            
            _impulseSource.GenerateImpulse(Vector3.one * intensity);
            
            if (duration > 0f)
            {
                StartCoroutine(StopShakeAfterDuration(duration));
            }
        }
        
        /// <summary>
        /// Play cinematic camera sequence
        /// </summary>
        public void PlayCinematicSequence(CinematicSequence sequence)
        {
            StartCoroutine(PlayCinematicCoroutine(sequence));
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleInput()
        {
            // Mouse input
            _mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            _scrollInput = Input.GetAxis("Mouse ScrollWheel");
            
            // Mouse button states
            bool mouseDown = Input.GetMouseButtonDown(0);
            bool mouseUp = Input.GetMouseButtonUp(0);
            bool mouseHeld = Input.GetMouseButton(0);
            bool rightMouseDown = Input.GetMouseButtonDown(1);
            
            // Camera mode switching
            HandleCameraModeInput();
            
            // Camera movement based on current mode
            HandleCameraMovement(mouseDown, mouseUp, mouseHeld, rightMouseDown);
            
            // Focus target selection
            if (rightMouseDown)
            {
                HandleFocusSelection();
            }
        }
        
        private void HandleCameraModeInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetCameraMode(CameraMode.Overview);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetCameraMode(CameraMode.PlantInspection);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetCameraMode(CameraMode.Facility);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SetCameraMode(CameraMode.Construction);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                SetCameraMode(CameraMode.FirstPerson);
            else if (Input.GetKeyDown(KeyCode.C))
                SetCameraMode(CameraMode.Cinematic);
        }
        
        private void HandleCameraMovement(bool mouseDown, bool mouseUp, bool mouseHeld, bool rightMouseDown)
        {
            if (_isTransitioning || !_userControlActive) return;
            
            var currentCamera = GetCameraForMode(_currentMode);
            if (currentCamera == null) return;
            
            // Dragging control
            if (mouseDown)
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }
            else if (mouseUp)
            {
                _isDragging = false;
            }
            
            // Pan and rotate based on camera mode
            if (_isDragging && _focusTarget == null)
            {
                HandleFreeCameraMovement(currentCamera);
            }
            
            // Zoom control
            if (Mathf.Abs(_scrollInput) > 0.01f)
            {
                HandleZoomInput(currentCamera);
            }
            
            // WASD movement for first person
            if (_currentMode == CameraMode.FirstPerson)
            {
                HandleFirstPersonMovement(currentCamera);
            }
        }
        
        private void HandleFreeCameraMovement(CinemachineVirtualCamera camera)
        {
            var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null) return;
            
            Vector3 mouseDelta = Input.mousePosition - _lastMousePosition;
            _lastMousePosition = Input.mousePosition;
            
            switch (_currentMode)
            {
                case CameraMode.Overview:
                case CameraMode.Facility:
                    // Pan movement
                    Vector3 panMovement = new Vector3(-mouseDelta.x, 0f, -mouseDelta.y) * _panSpeed * Time.deltaTime;
                    panMovement = camera.transform.TransformDirection(panMovement);
                    panMovement.y = 0f; // Keep on horizontal plane
                    
                    Vector3 newOffset = transposer.m_FollowOffset + panMovement;
                    newOffset = ClampToBounds(newOffset);
                    transposer.m_FollowOffset = newOffset;
                    break;
                    
                case CameraMode.PlantInspection:
                case CameraMode.Construction:
                    // Orbit movement
                    float rotationX = -mouseDelta.y * _mouseSensitivity * Time.deltaTime;
                    float rotationY = mouseDelta.x * _mouseSensitivity * Time.deltaTime;
                    
                    camera.transform.RotateAround(Vector3.zero, Vector3.up, rotationY);
                    camera.transform.RotateAround(Vector3.zero, camera.transform.right, rotationX);
                    break;
            }
        }
        
        private void HandleZoomInput(CinemachineVirtualCamera camera)
        {
            var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null) return;
            
            float zoomDelta = _scrollInput * _zoomSpeed;
            
            switch (_currentMode)
            {
                case CameraMode.Overview:
                case CameraMode.Facility:
                case CameraMode.Construction:
                    // Zoom by moving camera closer/further
                    Vector3 zoomDirection = camera.transform.forward;
                    Vector3 newOffset = transposer.m_FollowOffset + zoomDirection * zoomDelta;
                    
                    float distance = newOffset.magnitude;
                    if (distance >= _minZoomDistance && distance <= _maxZoomDistance)
                    {
                        transposer.m_FollowOffset = newOffset;
                    }
                    break;
                    
                case CameraMode.PlantInspection:
                    // Zoom by changing FOV
                    float newFOV = camera.m_Lens.FieldOfView - zoomDelta * 2f;
                    camera.m_Lens.FieldOfView = Mathf.Clamp(newFOV, 20f, 80f);
                    break;
            }
        }
        
        private void HandleFirstPersonMovement(CinemachineVirtualCamera camera)
        {
            var transposer = camera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null) return;
            
            // WASD movement
            Vector3 movement = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) movement += camera.transform.forward;
            if (Input.GetKey(KeyCode.S)) movement -= camera.transform.forward;
            if (Input.GetKey(KeyCode.A)) movement -= camera.transform.right;
            if (Input.GetKey(KeyCode.D)) movement += camera.transform.right;
            
            movement.y = 0f; // Keep on ground level
            movement = movement.normalized * 5f * Time.deltaTime;
            
            Vector3 newOffset = transposer.m_FollowOffset + movement;
            newOffset = ClampToBounds(newOffset);
            transposer.m_FollowOffset = newOffset;
            
            // Mouse look
            if (_isDragging)
            {
                float rotationX = -_mouseInput.y * _mouseSensitivity;
                float rotationY = _mouseInput.x * _mouseSensitivity;
                
                camera.transform.Rotate(rotationX, rotationY, 0f);
                
                // Clamp vertical rotation
                Vector3 eulerAngles = camera.transform.eulerAngles;
                eulerAngles.x = ClampAngle(eulerAngles.x, -60f, 60f);
                eulerAngles.z = 0f; // No roll
                camera.transform.eulerAngles = eulerAngles;
            }
        }
        
        private void HandleFocusSelection()
        {
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _focusLayers))
            {
                Transform hitTransform = hit.transform;
                
                // Check if it's a valid focus target
                var plantComponent = hitTransform.GetComponent<PlantInstanceComponent>();
                var roomController = hitTransform.GetComponent<GrowRoomController>();
                
                if (plantComponent != null)
                {
                    FocusOnTarget(hitTransform, CameraMode.PlantInspection);
                }
                else if (roomController != null)
                {
                    FocusOnTarget(hitTransform, CameraMode.Facility);
                }
                else
                {
                    FocusOnTarget(hitTransform);
                }
            }
        }
        
        #endregion
        
        #region Camera Updates
        
        private void UpdateCamera()
        {
            // Update camera bounds
            EnforceCameraBounds();
            
            // Update camera effects
            UpdateCameraEffects();
        }
        
        private void UpdateDynamicFOV()
        {
            var currentCamera = GetCameraForMode(_currentMode);
            if (currentCamera == null) return;
            
            // Adjust FOV based on movement speed (for motion blur effect)
            if (_currentMode == CameraMode.FirstPerson)
            {
                float movementSpeed = _mouseInput.magnitude;
                float targetFOV = 75f + movementSpeed * 5f;
                float currentFOV = currentCamera.m_Lens.FieldOfView;
                currentCamera.m_Lens.FieldOfView = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * 2f);
            }
        }
        
        private void EnforceCameraBounds()
        {
            var currentCamera = GetCameraForMode(_currentMode);
            if (currentCamera == null) return;
            
            var transposer = currentCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer == null) return;
            
            Vector3 clampedOffset = ClampToBounds(transposer.m_FollowOffset);
            transposer.m_FollowOffset = clampedOffset;
        }
        
        private void UpdateCameraEffects()
        {
            // Update post-processing effects based on camera mode
            if (_postProcessing != null)
            {
                switch (_currentMode)
                {
                    case CameraMode.PlantInspection:
                        _postProcessing.SetDepthOfFieldEnabled(true);
                        _postProcessing.SetVignetteIntensity(0.2f);
                        break;
                        
                    case CameraMode.Cinematic:
                        _postProcessing.SetFilmGrainEnabled(true);
                        _postProcessing.SetColorGradingEnabled(true);
                        break;
                        
                    default:
                        _postProcessing.ResetToDefaults();
                        break;
                }
            }
        }
        
        #endregion
        
        #region Auto-Focus System
        
        private void CheckAutoFocus()
        {
            if (!_enableAutoFocus || _focusTarget != null || _isTransitioning) return;
            
            // Start auto-focus timer if not already running
            if (_autoFocusCoroutine == null)
            {
                _autoFocusCoroutine = StartCoroutine(AutoFocusCoroutine());
            }
        }
        
        private IEnumerator AutoFocusCoroutine()
        {
            yield return new WaitForSeconds(_autoFocusDelay);
            
            // Find nearest interesting object
            Transform nearestTarget = FindNearestFocusTarget();
            if (nearestTarget != null && nearestTarget != _lastFocusTarget)
            {
                FocusOnTarget(nearestTarget);
                _lastFocusTarget = nearestTarget;
            }
            
            _autoFocusCoroutine = null;
        }
        
        private Transform FindNearestFocusTarget()
        {
            Vector3 cameraPosition = GetCameraForMode(_currentMode).transform.position;
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            
            // Check plants
            var plants = UnityEngine.Object.FindObjectsByType<PlantInstanceComponent>(FindObjectsSortMode.None);
            foreach (var plant in plants)
            {
                float distance = Vector3.Distance(cameraPosition, plant.transform.position);
                if (distance < nearestDistance && distance < 20f) // Within reasonable range
                {
                    nearest = plant.transform;
                    nearestDistance = distance;
                }
            }
            
            // Check facilities
            var facilities = UnityEngine.Object.FindObjectsByType<GrowRoomController>(FindObjectsSortMode.None);
            foreach (var facility in facilities)
            {
                float distance = Vector3.Distance(cameraPosition, facility.transform.position);
                if (distance < nearestDistance && distance < 30f)
                {
                    nearest = facility.transform;
                    nearestDistance = distance;
                }
            }
            
            return nearest;
        }
        
        #endregion
        
        #region Cinematic System
        
        private IEnumerator PlayCinematicCoroutine(CinematicSequence sequence)
        {
            // Enable cinematic mode
            SetCameraMode(CameraMode.Cinematic);
            OnCinematicModeChanged?.Invoke(true);
            
            // Show cinematic bars
            if (_enableCinematicBars)
            {
                ShowCinematicBars(true);
            }
            
            var cinematicCamera = GetCameraForMode(CameraMode.Cinematic);
            
            foreach (var shot in sequence.Shots)
            {
                // Move camera to shot position
                yield return StartCoroutine(MoveCameraToPosition(cinematicCamera, shot.Position, shot.Rotation, shot.Duration));
                
                // Hold for shot duration
                yield return new WaitForSeconds(shot.HoldDuration);
            }
            
            // Hide cinematic bars
            if (_enableCinematicBars)
            {
                ShowCinematicBars(false);
            }
            
            // Return to previous camera mode
            SetCameraMode(CameraMode.Overview);
            OnCinematicModeChanged?.Invoke(false);
        }
        
        private IEnumerator MoveCameraToPosition(CinemachineVirtualCamera camera, Vector3 targetPosition, Quaternion targetRotation, float duration)
        {
            Vector3 startPosition = camera.transform.position;
            Quaternion startRotation = camera.transform.rotation;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = _cinematicMovementCurve.Evaluate(t);
                
                camera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                camera.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                
                yield return null;
            }
            
            camera.transform.position = targetPosition;
            camera.transform.rotation = targetRotation;
        }
        
        private void ShowCinematicBars(bool show)
        {
            // This would control UI cinematic bars
            // Implementation would depend on your UI system
            Debug.Log($"Cinematic bars: {(show ? "Show" : "Hide")}");
        }
        
        #endregion
        
        #region Transition System
        
        private IEnumerator TransitionToCamera(CinemachineVirtualCamera targetCamera, CameraState targetState)
        {
            _isTransitioning = true;
            
            float duration = targetState.TransitionDuration;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Smooth transition curve
                t = Mathf.SmoothStep(0f, 1f, t);
                
                yield return null;
            }
            
            _isTransitioning = false;
        }
        
        #endregion
        
        #region Utility Methods
        
        private CinemachineVirtualCamera GetCameraForMode(CameraMode mode)
        {
            return mode switch
            {
                CameraMode.Overview => _overviewCamera,
                CameraMode.PlantInspection => _plantInspectionCamera,
                CameraMode.Facility => _facilityCamera,
                CameraMode.Construction => _constructionCamera,
                CameraMode.FirstPerson => _firstPersonCamera,
                CameraMode.Cinematic => _cinematicCamera,
                _ => _overviewCamera
            };
        }
        
        private void SetAllCamerasPriority(int priority)
        {
            _overviewCamera.Priority = priority;
            _plantInspectionCamera.Priority = priority;
            _facilityCamera.Priority = priority;
            _constructionCamera.Priority = priority;
            _firstPersonCamera.Priority = priority;
            _cinematicCamera.Priority = priority;
        }
        
        private Vector3 ClampToBounds(Vector3 position)
        {
            return new Vector3(
                Mathf.Clamp(position.x, _boundsMin.x, _boundsMax.x),
                Mathf.Clamp(position.y, _boundsMin.y, _boundsMax.y),
                Mathf.Clamp(position.z, _boundsMin.z, _boundsMax.z)
            );
        }
        
        private float ClampAngle(float angle, float min, float max)
        {
            if (angle > 180f) angle -= 360f;
            return Mathf.Clamp(angle, min, max);
        }
        
        private IEnumerator StopShakeAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            // Shake will naturally fade out with Cinemachine Impulse
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Get current camera settings
        /// </summary>
        public CameraSettings GetCurrentSettings()
        {
            var currentCamera = GetCameraForMode(_currentMode);
            return new CameraSettings
            {
                Mode = _currentMode,
                Position = currentCamera.transform.position,
                Rotation = currentCamera.transform.rotation,
                FieldOfView = currentCamera.m_Lens.FieldOfView,
                FocusTarget = _focusTarget,
                IsTransitioning = _isTransitioning
            };
        }
        
        /// <summary>
        /// Enable or disable user camera control
        /// </summary>
        public void SetUserControlEnabled(bool enabled)
        {
            _userControlActive = enabled;
        }
        
        /// <summary>
        /// Get suggested camera mode for target type
        /// </summary>
        public CameraMode GetSuggestedModeForTarget(Transform target)
        {
            if (target.GetComponent<PlantInstanceComponent>())
                return CameraMode.PlantInspection;
            if (target.GetComponent<GrowRoomController>())
                return CameraMode.Facility;
            
            return CameraMode.Overview;
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public enum CameraMode
    {
        Overview,
        PlantInspection,
        Facility,
        Construction,
        FirstPerson,
        Cinematic
    }
    
    [System.Serializable]
    public class CameraState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
        public bool AllowUserControl;
        public float TransitionDuration;
    }
    
    [System.Serializable]
    public class CameraSettings
    {
        public CameraMode Mode;
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
        public Transform FocusTarget;
        public bool IsTransitioning;
    }
    
    [System.Serializable]
    public class CinematicSequence
    {
        public string Name;
        public List<CinematicShot> Shots;
    }
    
    [System.Serializable]
    public class CinematicShot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Duration;
        public float HoldDuration;
        public string Description;
    }
}