using UnityEngine;

namespace ECM2.Walkthrough.Ex91
{
    /// <summary>
    /// This example shows how to implement a Cinemachine-based first person controller,
    /// using Cinemachine Virtual Camera’s 3rd Person Follow.
    ///
    /// Additionally, shows how to implement a Crouch / UnCrouch animation.
    ///
    /// Must be added to a Character.
    /// </summary>
    
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow.")]
        public GameObject cameraTarget;
        [Tooltip("How far in degrees can you move the camera up.")]
        public float maxPitch = 80.0f;
        [Tooltip("How far in degrees can you move the camera down.")]
        public float minPitch = -80.0f;
        
        [Space(15.0f)]
        [Tooltip("Cinemachine Virtual Camera positioned at desired crouched height.")]
        public GameObject crouchedCamera;
        [Tooltip("Cinemachine Virtual Camera positioned at desired un-crouched height.")]
        public GameObject unCrouchedCamera;
        
        [Space(15.0f)]
        [Tooltip("Mouse look sensitivity")]
        public Vector2 lookSensitivity = new Vector2(1.5f, 1.25f);
        
        // Cached Character
        
        private Character _character;
        
        // Current camera target pitch
        
        private float _cameraTargetPitch;
        
        /// <summary>
        /// Add input (affecting Yaw).
        /// This is applied to the Character's rotation.
        /// </summary>
        
        public void AddControlYawInput(float value)
        {
            _character.AddYawInput(value);
        }
        
        /// <summary>
        /// Add input (affecting Pitch).
        /// This is applied to the cameraTarget's local rotation.
        /// </summary>
        
        public void AddControlPitchInput(float value, float minValue = -80.0f, float maxValue = 80.0f)
        {
            if (value == 0.0f)
                return;
            
            _cameraTargetPitch = MathLib.ClampAngle(_cameraTargetPitch + value, minValue, maxValue);
            cameraTarget.transform.localRotation = Quaternion.Euler(-_cameraTargetPitch, 0.0f, 0.0f);
        }
        
        /// <summary>
        /// When character crouches, toggle Crouched / UnCrouched cameras.
        /// </summary>
        
        private void OnCrouched()
        {
            crouchedCamera.SetActive(true);
            unCrouchedCamera.SetActive(false);
        }
        
        /// <summary>
        /// When character un-crouches, toggle Crouched / UnCrouched cameras.
        /// </summary>
        
        private void OnUnCrouched()
        {
            crouchedCamera.SetActive(false);
            unCrouchedCamera.SetActive(true);
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            
            // Disable Character's rotation mode, we'll handle it here
            
            _character.SetRotationMode(Character.RotationMode.None);
        }
        
        private void OnEnable()
        {
            // Subscribe to Character events
            
            _character.Crouched += OnCrouched;
            _character.UnCrouched += OnUnCrouched;
        }
        
        private void OnDisable()
        {
            // Unsubscribe to Character events
            
            _character.Crouched -= OnCrouched;
            _character.UnCrouched -= OnUnCrouched;
        }

        private void Update()
        {
            // Movement input
            
            Vector2 moveInput = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            // Movement direction relative to Character's forward

            Vector3 movementDirection = Vector3.zero;

            movementDirection += _character.GetRightVector() * moveInput.x;
            movementDirection += _character.GetForwardVector() * moveInput.y;
            
            // Set Character movement direction

            _character.SetMovementDirection(movementDirection);
            
            // Look input

            Vector2 lookInput = new Vector2
            {
                x = Input.GetAxisRaw("Mouse X"),
                y = Input.GetAxisRaw("Mouse Y")
            };
            
            // Add yaw input, this update character's yaw rotation

            AddControlYawInput(lookInput.x * lookSensitivity.x);
            
            // Add pitch input (look up / look down), this update cameraTarget's local rotation
            
            AddControlPitchInput(lookInput.y * lookSensitivity.y, minPitch, maxPitch);
            
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
        }
    }
}