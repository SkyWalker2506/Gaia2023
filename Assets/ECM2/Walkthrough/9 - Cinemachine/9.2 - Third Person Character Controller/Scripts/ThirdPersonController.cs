using Cinemachine;
using UnityEngine;

namespace ECM2.Walkthrough.Ex92
{
    /// <summary>
    /// This example shows how to implement a Cinemachine-based third person controller,
    /// using Cinemachine Virtual Camera’s 3rd Person Follow.
    ///
    /// Must be added to a Character.
    /// </summary>
    
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Cinemachine")]
        [Tooltip("The CM virtual Camera following the target.")]
        public CinemachineVirtualCamera followCamera;

        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
        public GameObject followTarget;

        [Tooltip("The default distance behind the Follow target.")]
        [SerializeField]
        public float followDistance = 5.0f;

        [Tooltip("The minimum distance to Follow target.")]
        [SerializeField]
        public float followMinDistance = 2.0f;

        [Tooltip("The maximum distance to Follow target.")]
        [SerializeField]
        public float followMaxDistance = 10.0f;

        [Tooltip("How far in degrees can you move the camera up.")]
        public float maxPitch = 80.0f;

        [Tooltip("How far in degrees can you move the camera down.")]
        public float minPitch = -80.0f;

        [Space(15.0f)]
        public bool invertLook = true;
        
        [Tooltip("Mouse look sensitivity")]
        public Vector2 lookSensitivity = new Vector2(1.5f, 1.25f);
        
        // Cached Character
        
        private Character _character;
        
        // Current followTarget yaw and pitch angles

        private float _cameraTargetYaw;
        private float _cameraTargetPitch;
        
        // Current follow distance
        
        private Cinemachine3rdPersonFollow _cmThirdPersonFollow;
        protected float _followDistanceSmoothVelocity;
        
        /// <summary>
        /// Add input (affecting Yaw). 
        /// This is applied to the followTarget's yaw rotation.
        /// </summary>
        
        public void AddControlYawInput(float value, float minValue = -180.0f, float maxValue = 180.0f)
        {
            if (value != 0.0f) _cameraTargetYaw = MathLib.ClampAngle(_cameraTargetYaw + value, minValue, maxValue);
        }
        
        /// <summary>
        /// Add input (affecting Pitch). 
        /// This is applied to the followTarget's pitch rotation.
        /// </summary>
        
        public void AddControlPitchInput(float value, float minValue = -80.0f, float maxValue = 80.0f)
        {
            if (value == 0.0f)
                return;
            
            if (invertLook)
                value = -value;
            
            _cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
        }
        
        /// <summary>
        /// Adds input (affecting follow distance).
        /// </summary>

        public virtual void AddControlZoomInput(float value)
        {
            followDistance = Mathf.Clamp(followDistance - value, followMinDistance, followMaxDistance);
        }
        
        /// <summary>
        /// Update followTarget rotation using _cameraTargetYaw and _cameraTargetPitch values and its follow distance.
        /// </summary>

        private void UpdateCamera()
        {
            followTarget.transform.rotation = Quaternion.Euler(_cameraTargetPitch, _cameraTargetYaw, 0.0f);
            
            _cmThirdPersonFollow.CameraDistance = 
                Mathf.SmoothDamp(_cmThirdPersonFollow.CameraDistance, followDistance, ref _followDistanceSmoothVelocity, 0.1f);
        }

        private void Awake()
        {
            // Cache our controlled character
            
            _character = GetComponent<Character>();
            
            // Cache and init Cinemachine3rdPersonFollow component

            _cmThirdPersonFollow = followCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (_cmThirdPersonFollow)
                _cmThirdPersonFollow.CameraDistance = followDistance;
        }

        private void Start()
        {
            // Lock mouse cursor
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            // Movement input
            
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            // Set Movement direction in world space
            
            Vector3 movementDirection =  Vector3.zero;

            movementDirection += Vector3.right * inputMove.x;
            movementDirection += Vector3.forward * inputMove.y;
            
            // If character has a camera assigned...
            
            if (_character.camera)
            {
                // Make movement direction relative to its camera view direction
                
                movementDirection = movementDirection.relativeTo(_character.cameraTransform);
            }
            
            // Set Character movement direction

            _character.SetMovementDirection(movementDirection);
            
            // Crouch input
            
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                _character.Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                _character.UnCrouch();
            
            // Jump input
            
            if (Input.GetButtonDown("Jump"))
                _character.Jump();
            else if (Input.GetButtonUp("Jump"))
                _character.StopJumping();
            
            // Look input

            Vector2 lookInput = new Vector2
            {
                x = Input.GetAxisRaw("Mouse X"),
                y = Input.GetAxisRaw("Mouse Y")
            };
            
            AddControlYawInput(lookInput.x * lookSensitivity.x);
            AddControlPitchInput(lookInput.y * lookSensitivity.y, minPitch, maxPitch);
            
            // Zoom (in / out) input

            float mouseScrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
            AddControlZoomInput(mouseScrollInput);
        }

        private void LateUpdate()
        {
            // Update cameraTarget rotation using our yaw and pitch values
            
            UpdateCamera();
        }
    }
}