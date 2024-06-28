using UnityEngine;

namespace ECM2.Examples.ThirdPerson
{
    /// <summary>
    /// This example shows how to implement a basic third person controller.
    /// This must be added to a Character.
    /// </summary>
    
    public class ThirdPersonController : MonoBehaviour
    {
        [Space(15.0f)]
        public GameObject followTarget;

        [Tooltip("The default distance behind the Follow target.")]
        [SerializeField]
        public float followDistance = 5.0f;

        [Tooltip("The minimum distance to Follow target.")]
        [SerializeField]
        public float followMinDistance;

        [Tooltip("The maximum distance to Follow target.")]
        [SerializeField]
        public float followMaxDistance = 10.0f;

        [Space(15.0f)]
        public bool invertLook = true;

        [Tooltip("Mouse look sensitivity")]
        public Vector2 mouseSensitivity = new Vector2(1.0f, 1.0f);

        [Space(15.0f)]
        [Tooltip("How far in degrees can you move the camera down.")]
        public float minPitch = -80.0f;

        [Tooltip("How far in degrees can you move the camera up.")]
        public float maxPitch = 80.0f;

        protected float _cameraYaw;
        protected float _cameraPitch;

        protected float _currentFollowDistance;
        protected float _followDistanceSmoothVelocity;

        protected Character _character;
        
        /// <summary>
        /// Add input (affecting Yaw).
        /// This is applied to the camera's rotation.
        /// </summary>

        public virtual void AddControlYawInput(float value)
        {
            _cameraYaw = MathLib.ClampAngle(_cameraYaw + value, -180.0f, 180.0f);
        }
        
        /// <summary>
        /// Add input (affecting Pitch).
        /// This is applied to the camera's rotation.
        /// </summary>

        public virtual void AddControlPitchInput(float value, float minValue = -80.0f, float maxValue = 80.0f)
        {
            _cameraPitch = MathLib.ClampAngle(_cameraPitch + value, minValue, maxValue);
        }
        
        /// <summary>
        /// Adds input (affecting follow distance).
        /// </summary>

        public virtual void AddControlZoomInput(float value)
        {
            followDistance = Mathf.Clamp(followDistance - value, followMinDistance, followMaxDistance);
        }
        
        /// <summary>
        /// Update camera's rotation applying current _cameraPitch and _cameraYaw values.
        /// </summary>

        protected virtual void UpdateCameraRotation()
        {
            Transform cameraTransform = _character.cameraTransform;
            cameraTransform.rotation = Quaternion.Euler(_cameraPitch, _cameraYaw, 0.0f);
        }
        
        /// <summary>
        /// Update camera's position maintaining _currentFollowDistance from target. 
        /// </summary>

        protected virtual void UpdateCameraPosition()
        {
            Transform cameraTransform = _character.cameraTransform;
            
            _currentFollowDistance =
                Mathf.SmoothDamp(_currentFollowDistance, followDistance, ref _followDistanceSmoothVelocity, 0.1f);

            cameraTransform.position =
                followTarget.transform.position - cameraTransform.forward * _currentFollowDistance;
        }
        
        /// <summary>
        /// Update camera's position and rotation.
        /// </summary>

        protected virtual void UpdateCamera()
        {
            UpdateCameraRotation();
            UpdateCameraPosition();
        }

        protected virtual void Awake()
        {
            _character = GetComponent<Character>();
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            Vector3 euler = _character.cameraTransform.eulerAngles;

            _cameraPitch = euler.x;
            _cameraYaw = euler.y;

            _currentFollowDistance = followDistance;
        }

        protected virtual void Update()
        {
            // Movement input
            
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            Vector3 movementDirection = Vector3.zero;

            movementDirection += Vector3.right * inputMove.x;
            movementDirection += Vector3.forward * inputMove.y;

            if (_character.cameraTransform)
                movementDirection = movementDirection.relativeTo(_character.cameraTransform, _character.GetUpVector());

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

            lookInput *= mouseSensitivity;

            AddControlYawInput(lookInput.x);
            AddControlPitchInput(invertLook ? -lookInput.y : lookInput.y, minPitch, maxPitch);
            
            // Zoom input

            float mouseScrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
            AddControlZoomInput(mouseScrollInput);
        }

        protected virtual void LateUpdate()
        {
            UpdateCamera();
        }
    }
}
