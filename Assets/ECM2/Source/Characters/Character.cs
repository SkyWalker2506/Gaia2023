using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECM2
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        #region ENUMS

        public enum MovementMode
        {
            /// <summary>
            /// Disables movement clearing velocity and any pending forces / impulsed on Character.
            /// </summary>
            
            None,
            
            /// <summary>
            /// Walking on a surface, under the effects of friction, and able to "step up" barriers. Vertical velocity is zero.
            /// </summary>
            
            Walking,
            
            /// <summary>
            /// Falling under the effects of gravity, after jumping or walking off the edge of a surface.
            /// </summary>
            
            Falling,
            
            /// <summary>
            /// Flying, ignoring the effects of gravity.
            /// </summary>
            
            Flying,
            
            /// <summary>
            /// Swimming through a fluid volume, under the effects of gravity and buoyancy.
            /// </summary>
            
            Swimming,
            
            /// <summary>
            /// User-defined custom movement mode, including many possible sub-modes.
            /// </summary>
            
            Custom
        }

        public enum RotationMode
        {
            /// <summary>
            /// Disable Character's rotation.
            /// </summary>
            
            None,
            
            /// <summary>
            /// Smoothly rotate the Character toward the direction of acceleration, using rotationRate as the rate of rotation change.
            /// </summary>
            
            OrientRotationToMovement,
            
            /// <summary>
            /// Smoothly rotate the Character toward camera's view direction, using rotationRate as the rate of rotation change.
            /// </summary>
            
            OrientRotationToViewDirection,
            
            /// <summary>
            /// Let root motion handle Character rotation.
            /// </summary>
            
            OrientWithRootMotion,
            
            /// <summary>
            /// User-defined custom rotation mode.
            /// </summary>
            
            Custom
        }

        #endregion

        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("The Character's current rotation mode.")]
        [SerializeField]
        private RotationMode _rotationMode;

        [Tooltip("Change in rotation per second (Deg / s).\n" +
                 "Used when rotation mode is OrientRotationToMovement or OrientRotationToViewDirection.")]
        [SerializeField]
        private float _rotationRate;
        
        [Space(15f)]
        [Tooltip("The Character's default movement mode. Used at player startup.")]
        [SerializeField]
        private MovementMode _startingMovementMode;
        
        [Space(15f)]
        [Tooltip("The maximum ground speed when walking.\n" +
                 "Also determines maximum lateral speed when falling.")]
        [SerializeField]
        private float _maxWalkSpeed;

        [Tooltip("The ground speed that we should accelerate up to when walking at minimum analog stick tilt.")]
        [SerializeField]
        private float _minAnalogWalkSpeed;

        [Tooltip("Max Acceleration (rate of change of velocity).")]
        [SerializeField]
        private float _maxAcceleration;

        [Tooltip("Deceleration when walking and not applying acceleration.\n" +
                 "This is a constant opposing force that directly lowers velocity by a constant value.")]
        [SerializeField]
        private float _brakingDecelerationWalking;

        [Tooltip("Setting that affects movement control.\n" +
                 "Higher values allow faster changes in direction.\n" +
                 "If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).")]
        [SerializeField]
        private float _groundFriction;
        
        [Space(15.0f)]
        [Tooltip("Is the character able to crouch ?")]
        [SerializeField]
        private bool _canEverCrouch;

        [Tooltip("If canEverCrouch == true, determines the character height when crouched.")]
        [SerializeField]
        private float _crouchedHeight;
        
        [Tooltip("If canEverCrouch == true, determines the character height when un crouched.")]
        [SerializeField]
        private float _unCrouchedHeight;

        [Tooltip("The maximum ground speed while crouched.")]
        [SerializeField]
        private float _maxWalkSpeedCrouched;
        
        [Space(15f)]
        [Tooltip("The maximum vertical velocity a Character can reach when falling. Eg: Terminal velocity.")]
        [SerializeField]
        private float _maxFallSpeed;

        [Tooltip("Lateral deceleration when falling and not applying acceleration.")]
        [SerializeField]
        private float _brakingDecelerationFalling;

        [Tooltip("Friction to apply to lateral movement when falling. \n" +
                 "If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).")]
        [SerializeField]
        private float _fallingLateralFriction;

        [Range(0.0f, 1.0f)]
        [Tooltip("When falling, amount of lateral movement control available to the Character.\n" +
                 "0 = no control, 1 = full control at max acceleration.")]
        [SerializeField]
        private float _airControl;
        
        [Space(15.0f)]
        [Tooltip("Is the character able to jump ?")]
        [SerializeField]
        private bool _canEverJump;

        [Tooltip("Can jump while crouching ?")]
        [SerializeField]
        private bool _canJumpWhileCrouching;

        [Tooltip("The max number of jumps the Character can perform.")]
        [SerializeField]
        private int _jumpMaxCount;

        [Tooltip("Initial velocity (instantaneous vertical velocity) when jumping.")]
        [SerializeField]
        private float _jumpImpulse;
        
        [Tooltip("The maximum time (in seconds) to hold the jump. eg: Variable height jump.")]
        [SerializeField]
        private float _jumpMaxHoldTime;
        
        [Tooltip("How early before hitting the ground you can trigger a jump (in seconds).")]
        [SerializeField]
        private float _jumpMaxPreGroundedTime;

        [Tooltip("How long after leaving the ground you can trigger a jump (in seconds).")]
        [SerializeField]
        private float _jumpMaxPostGroundedTime;
        
        [Space(15f)]
        [Tooltip("The maximum flying speed.")]
        [SerializeField]
        private float _maxFlySpeed;

        [Tooltip("Deceleration when flying and not applying acceleration.")]
        [SerializeField]
        private float _brakingDecelerationFlying;

        [Tooltip("Friction to apply to movement when flying.")]
        [SerializeField]
        private float _flyingFriction;
        
        [Space(15f)]
        [Tooltip("The maximum swimming speed.")]
        [SerializeField]
        private float _maxSwimSpeed;

        [Tooltip("Deceleration when swimming and not applying acceleration.")]
        [SerializeField]
        private float _brakingDecelerationSwimming;

        [Tooltip("Friction to apply to movement when swimming.")]
        [SerializeField]
        private float _swimmingFriction;

        [Tooltip("Water buoyancy ratio. 1 = Neutral Buoyancy, 0 = No Buoyancy.")]
        [SerializeField]
        private float _buoyancy;
        
        [Tooltip("This Character's gravity.")]
        [Space(15f)]
        [SerializeField]
        private Vector3 _gravity;

        [Tooltip("The degree to which this object is affected by gravity.\n" +
                 "Can be negative allowing to change gravity direction.")]
        [SerializeField]
        private float _gravityScale;
        
        [Space(15f)]
        [Tooltip("Should animation determines the Character's movement ?")]
        [SerializeField]
        private bool _useRootMotion;

        [Space(15f)]
        [Tooltip("Whether the Character moves with the moving platform it is standing on.")]
        [SerializeField]
        private bool _impartPlatformMovement;

        [Tooltip("Whether the Character receives the changes in rotation of the platform it is standing on.")]
        [SerializeField]
        private bool _impartPlatformRotation;

        [Tooltip("If true, impart the platform's velocity when jumping or falling off it.")]
        [SerializeField]
        private bool _impartPlatformVelocity;

        [Space(15f)]
        [Tooltip("If enabled, the player will interact with dynamic rigidbodies when walking into them.")]
        [SerializeField]
        private bool _enablePhysicsInteraction;

        [Tooltip("Should apply push force to characters when walking into them ?")]
        [SerializeField]
        private bool _applyPushForceToCharacters;

        [Tooltip("Should apply a downward force to rigidbodies we stand on ?")]
        [SerializeField]
        private bool _applyStandingDownwardForce;

        [Space(15.0f)]
        [Tooltip("This Character's mass (in Kg)." +
                 "Determines how the character interact against other characters or dynamic rigidbodies if enablePhysicsInteraction == true.")]
        [SerializeField]
        private float _mass;

        [Tooltip("Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.")]
        [SerializeField]
        private float _pushForceScale;

        [Tooltip("Force applied to rigidbodies we stand on (due to mass and gravity) is scaled by this amount.")]
        [SerializeField]
        private float _standingDownwardForceScale;

        [Space(15f)]
        [Tooltip("Reference to the Player's Camera.\n" +
                 "If assigned, the Character's movement will be relative to this camera, otherwise movement will be relative to world axis.")]
        [SerializeField]
        private Camera _camera;

        #endregion

        #region FIELDS

        protected readonly List<PhysicsVolume> _physicsVolumes = new List<PhysicsVolume>();

        private Coroutine _lateFixedUpdateCoroutine;
        private bool _enableAutoSimulation = true;
        
        private Transform _transform;
        private CharacterMovement _characterMovement;
        private Animator _animator;
        private RootMotionController _rootMotionController;
        private Transform _cameraTransform;
        
        /// <summary>
        /// The Character's current movement mode.
        /// </summary>
        
        private MovementMode _movementMode = MovementMode.None;

        /// <summary>
        /// Character's User-defined custom movement mode (sub-mode).
        /// Only applicable if _movementMode == Custom.
        /// </summary>
        
        private int _customMovementMode;
        
        private bool _useSeparateBrakingFriction;
        private float _brakingFriction;
        
        private bool _useSeparateBrakingDeceleration;
        private float _brakingDeceleration;
        
        private Vector3 _movementDirection = Vector3.zero;
        private Vector3 _rotationInput = Vector3.zero;

        private Vector3 _desiredVelocity = Vector3.zero;
        
        protected bool _isCrouched;
        
        protected bool _isJumping;
        private float _jumpInputHoldTime;
        private float _jumpForceTimeRemaining;
        private int _jumpCurrentCount;

        protected float _fallingTime;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// This Character's camera transform.
        /// If assigned, the Character's movement will be relative to this, otherwise movement will be relative to world.
        /// </summary>

        public new Camera camera
        {
            get => _camera;
            set => _camera = value;
        }

        /// <summary>
        /// Cached camera transform (if any).
        /// </summary>

        public Transform cameraTransform
        {
            get
            {
                if (_camera != null)
                    _cameraTransform = _camera.transform;

                return _cameraTransform;
            }
        }
        
        /// <summary>
        /// Cached Character transform.
        /// </summary>

        public new Transform transform => _transform;
        
        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>
        
        public CharacterMovement characterMovement => _characterMovement;
        
        /// <summary>
        /// Cached Animator component. Can be null.
        /// </summary>

        public Animator animator => _animator;

        /// <summary>
        /// Cached Character's RootMotionController component. Can be null.
        /// </summary>

        public RootMotionController rootMotionController => _rootMotionController;
        
        /// <summary>
        /// Change in rotation per second, used when orientRotationToMovement or orientRotationToViewDirection are true.
        /// </summary>
        
        public float rotationRate
        {
            get => _rotationRate;
            set => _rotationRate = value;
        }
        
        /// <summary>
        /// The Character's current rotation mode.
        /// </summary>

        public RotationMode rotationMode
        {
            get => _rotationMode;
            set => _rotationMode = value;
        }
        
        /// <summary>
        /// The maximum ground speed when walking. Also determines maximum lateral speed when falling.
        /// </summary>

        public float maxWalkSpeed
        {
            get => _maxWalkSpeed;
            set => _maxWalkSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The ground speed that we should accelerate up to when walking at minimum analog stick tilt.
        /// </summary>

        public float minAnalogWalkSpeed
        {
            get => _minAnalogWalkSpeed;
            set => _minAnalogWalkSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Max Acceleration (rate of change of velocity).
        /// </summary>

        public float maxAcceleration
        {
            get => _maxAcceleration;
            set => _maxAcceleration = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when walking and not applying acceleration.
        /// This is a constant opposing force that directly lowers velocity by a constant value.
        /// </summary>

        public float brakingDecelerationWalking
        {
            get => _brakingDecelerationWalking;
            set => _brakingDecelerationWalking = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Setting that affects movement control.
        /// Higher values allow faster changes in direction.
        /// If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).
        /// </summary>

        public float groundFriction
        {
            get => _groundFriction;
            set => _groundFriction = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Is the character able to crouch ?
        /// </summary>
        
        public bool canEverCrouch
        {
            get => _canEverCrouch;
            set => _canEverCrouch = value;
        }
        
        /// <summary>
        /// If canEverCrouch == true, determines the character height when crouched.
        /// </summary>

        public float crouchedHeight
        {
            get => _crouchedHeight;
            set => _crouchedHeight = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// If canEverCrouch == true, determines the character height when un crouched.
        /// </summary>

        public float unCrouchedHeight
        {
            get => _unCrouchedHeight;
            set => _unCrouchedHeight = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// The maximum ground speed while crouched.
        /// </summary>
        
        public float maxWalkSpeedCrouched
        {
            get => _maxWalkSpeedCrouched;
            set => _maxWalkSpeedCrouched = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Is the crouch input pressed?
        /// </summary>

        public bool crouchInputPressed { get; protected set; }
        
        /// <summary>
        /// The maximum vertical velocity (in m/s) a Character can reach when falling.
        /// Eg: Terminal velocity.
        /// </summary>

        public float maxFallSpeed
        {
            get => _maxFallSpeed;
            set => _maxFallSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Lateral deceleration when falling and not applying acceleration.
        /// </summary>

        public float brakingDecelerationFalling
        {
            get => _brakingDecelerationFalling;
            set => _brakingDecelerationFalling = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to lateral air movement when falling.
        /// </summary>

        public float fallingLateralFriction
        {
            get => _fallingLateralFriction;
            set => _fallingLateralFriction = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// The Character's time in falling movement mode.
        /// </summary>

        public float fallingTime => _fallingTime;
        
        /// <summary>
        /// When falling, amount of lateral movement control available to the Character.
        /// 0 = no control, 1 = full control at max acceleration.
        /// </summary>

        public float airControl
        {
            get => _airControl;
            set => _airControl = Mathf.Clamp01(value);
        }
        
        /// <summary>
        /// Is the character able to jump ?
        /// </summary>
        
        public bool canEverJump
        {
            get => _canEverJump;
            set => _canEverJump = value;
        }
        
        /// <summary>
        /// Is allowed to jump while crouching?
        /// </summary>

        public bool canJumpWhileCrouching
        {
            get => _canJumpWhileCrouching;
            set => _canJumpWhileCrouching = value;
        }
        
        /// <summary>
        /// The max number of jumps the Character can perform.
        /// </summary>

        public int jumpMaxCount
        {
            get => _jumpMaxCount;
            set => _jumpMaxCount = Mathf.Max(1, value);
        }
        
        /// <summary>
        /// Initial velocity (instantaneous vertical velocity) when jumping.
        /// </summary>

        public float jumpImpulse
        {
            get => _jumpImpulse;
            set => _jumpImpulse = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// The maximum time (in seconds) to hold the jump. eg: Variable height jump.
        /// </summary>

        public float jumpMaxHoldTime
        {
            get => _jumpMaxHoldTime;
            set => _jumpMaxHoldTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// How early before hitting the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpMaxPreGroundedTime
        {
            get => _jumpMaxPreGroundedTime;
            set => _jumpMaxPreGroundedTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// How long after leaving the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpMaxPostGroundedTime
        {
            get => _jumpMaxPostGroundedTime;
            set => _jumpMaxPostGroundedTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// This is the time (in seconds) that the player has held the jump input.
        /// </summary>

        public float jumpInputHoldTime
        {
            get => _jumpInputHoldTime;
            protected set => _jumpInputHoldTime = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Amount of jump force time remaining, if jumpMaxHoldTime > 0.
        /// </summary>

        public float jumpForceTimeRemaining
        {
            get => _jumpForceTimeRemaining;
            protected set => _jumpForceTimeRemaining = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Tracks the current number of jumps performed.
        /// </summary>

        public int jumpCurrentCount
        {
            get => _jumpCurrentCount;
            protected set => _jumpCurrentCount = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Should notify a jump apex ?
        /// Set to true to receive OnReachedJumpApex event.
        /// </summary>

        public bool notifyJumpApex { get; set; }
        
        /// <summary>
        /// Is the jump input pressed?
        /// </summary>

        public bool jumpInputPressed { get; protected set; }
        
        /// <summary>
        /// The maximum flying speed.
        /// </summary>

        public float maxFlySpeed
        {
            get => _maxFlySpeed;
            set => _maxFlySpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when flying and not applying acceleration.
        /// </summary>

        public float brakingDecelerationFlying
        {
            get => _brakingDecelerationFlying;
            set => _brakingDecelerationFlying = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to movement when flying.
        /// </summary>

        public float flyingFriction
        {
            get => _flyingFriction;
            set => _flyingFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The maximum swimming speed.
        /// </summary>

        public float maxSwimSpeed
        {
            get => _maxSwimSpeed;
            set => _maxSwimSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when swimming and not applying acceleration.
        /// </summary>

        public float brakingDecelerationSwimming
        {
            get => _brakingDecelerationSwimming;
            set => _brakingDecelerationSwimming = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to movement when swimming.
        /// </summary>

        public float swimmingFriction
        {
            get => _swimmingFriction;
            set => _swimmingFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Water buoyancy ratio. 1 = Neutral Buoyancy, 0 = No Buoyancy.
        /// </summary>

        public float buoyancy
        {
            get => _buoyancy;
            set => _buoyancy = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Should use a separate braking friction ?
        /// </summary>

        public bool useSeparateBrakingFriction
        {
            get => _useSeparateBrakingFriction;
            set => _useSeparateBrakingFriction = value;
        }

        /// <summary>
        /// Friction (drag) coefficient applied when braking (whenever Acceleration = 0, or if Character is exceeding max speed).
        /// This is the value, used in all movement modes IF useSeparateBrakingFriction is true.
        /// </summary>

        public float brakingFriction
        {
            get => _brakingFriction;
            set => _brakingFriction = Mathf.Max(0.0f, value);
        }
        
        /// <summary>
        /// Should use a separate braking deceleration ?
        /// </summary>

        public bool useSeparateBrakingDeceleration
        {
            get => _useSeparateBrakingDeceleration;
            set => _useSeparateBrakingDeceleration = value;
        }
        
        /// <summary>
        /// Deceleration when not applying acceleration.
        /// This is a constant opposing force that directly lowers velocity by a constant value.
        /// This is the value, used in all movement modes IF useSeparateBrakingDeceleration is true.
        /// </summary>

        public float brakingDeceleration
        {
            get => _brakingDeceleration;
            set => _brakingDeceleration = value;
        }
        
        /// <summary>
        /// The Character's gravity (modified by gravityScale). Defaults to Physics.gravity.
        /// </summary>

        public Vector3 gravity
        {
            get => _gravity * _gravityScale;
            set => _gravity = value;
        }

        /// <summary>
        /// The degree to which this object is affected by gravity.
        /// Can be negative allowing to change gravity direction.
        /// </summary>

        public float gravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }
        
        /// <summary>
        /// Should animation determines the Character' movement ?
        /// </summary>

        public bool useRootMotion
        {
            get => _useRootMotion;
            set => _useRootMotion = value;
        }
        
        /// <summary>
        /// If enabled, the player will interact with dynamic rigidbodies when walking into them.
        /// </summary>

        public bool enablePhysicsInteraction
        {
            get => _enablePhysicsInteraction;
            set
            {
                _enablePhysicsInteraction = value;

                if (_characterMovement)
                    _characterMovement.enablePhysicsInteraction = _enablePhysicsInteraction;
            }
        }

        /// <summary>
        /// Should apply push force to other characters when walking into them ?
        /// </summary>

        public bool applyPushForceToCharacters
        {
            get => _applyPushForceToCharacters;
            set
            {
                _applyPushForceToCharacters = value;

                if (_characterMovement)
                    _characterMovement.physicsInteractionAffectsCharacters = _applyPushForceToCharacters;
            }
        }

        /// <summary>
        /// Should apply a downward force to rigidbodies we stand on ?
        /// </summary>

        public bool applyStandingDownwardForce
        {
            get => _applyStandingDownwardForce;
            set => _applyStandingDownwardForce = value;
        }

        /// <summary>
        /// This Character's mass (in Kg).
        /// </summary>

        public float mass
        {
            get => _mass;
            set
            {
                _mass = Mathf.Max(1e-07f, value);

                if (_characterMovement && _characterMovement.rigidbody)
                    _characterMovement.rigidbody.mass = _mass;
            }
        }

        /// <summary>
        /// Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.
        /// </summary>

        public float pushForceScale
        {
            get => _pushForceScale;
            set
            {
                _pushForceScale = Mathf.Max(0.0f, value);

                if (_characterMovement)
                    _characterMovement.pushForceScale = _pushForceScale;
            }
        }

        /// <summary>
        /// Force applied to rigidbodies we stand on (due to mass and gravity) is scaled by this amount.
        /// </summary>

        public float standingDownwardForceScale
        {
            get => _standingDownwardForceScale;
            set => _standingDownwardForceScale = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// If true, impart the platform's velocity when jumping or falling off it.
        /// </summary>

        public bool impartPlatformVelocity
        {
            get => _impartPlatformVelocity;
            set
            {
                _impartPlatformVelocity = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformVelocity = _impartPlatformVelocity;
            }
        }

        /// <summary>
        /// Whether the Character moves with the moving platform it is standing on.
        /// If true, the Character moves with the moving platform.
        /// </summary>

        public bool impartPlatformMovement
        {
            get => _impartPlatformMovement;
            set
            {
                _impartPlatformMovement = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformMovement = _impartPlatformMovement;
            }
        }

        /// <summary>
        /// Whether the Character receives the changes in rotation of the platform it is standing on.
        /// If true, the Character rotates with the moving platform.
        /// </summary>

        public bool impartPlatformRotation
        {
            get => _impartPlatformRotation;
            set
            {
                _impartPlatformRotation = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformRotation = _impartPlatformRotation;
            }
        }
        
        /// <summary>
        /// The character's current position (read only)
        /// Use SetPosition method to modify it. 
        /// </summary>
        
        public Vector3 position => characterMovement.position;
        
        /// <summary>
        /// The character's current position (read only).
        /// Use SetRotation method to modify it. 
        /// </summary>

        public Quaternion rotation => characterMovement.rotation;
        
        /// <summary>
        /// The character's current velocity (read only).
        /// Use SetVelocity method to modify it. 
        /// </summary>

        public Vector3 velocity => characterMovement.velocity;

        /// <summary>
        /// The Character's current speed.
        /// </summary>

        public float speed => characterMovement.velocity.magnitude;
        
        /// <summary>
        /// The character's current radius (read only).
        /// Use CharacterMovement SetDimensions method to modify it. 
        /// </summary>

        public float radius => characterMovement.radius;
        
        /// <summary>
        /// The character's current height (read only).
        /// Use CharacterMovement SetDimensions method to modify it. 
        /// </summary>

        public float height => characterMovement.height;

        /// <summary>
        /// The Character's current movement mode (read only.
        /// Use SetMovementMode method to modify it.
        /// </summary>
        
        public MovementMode movementMode => _movementMode;

        /// <summary>
        /// Character's User-defined custom movement mode (sub-mode).
        /// Only applicable if _movementMode == Custom (read only).
        /// Use SetMovementMode method to modify it.
        /// </summary>
        
        public int customMovementMode => _customMovementMode;
        
        /// <summary>
        /// PhysicsVolume overlapping this component. NULL if none.
        /// </summary>

        public PhysicsVolume physicsVolume { get; protected set; }
        
        /// <summary>
        /// If true, enables a LateFixedUpdate Coroutine to simulate this character.
        /// If false, Simulate method must be called in order to simulate this character.
        /// Enabled by default.
        /// </summary>

        public bool enableAutoSimulation
        {
            get => _enableAutoSimulation;
            set
            {
                _enableAutoSimulation = value;
                EnableAutoSimulationCoroutine(_enableAutoSimulation);
            }
        }
        
        // Is the Character paused?

        public bool isPaused { get; private set; }

        #endregion
        
        #region EVENTS
        
        public delegate void PhysicsVolumeChangedEventHandler(PhysicsVolume newPhysicsVolume);

        public delegate void MovementModeChangedEventHandler(MovementMode prevMovementMode, int prevCustomMode);
        public delegate void CustomMovementModeUpdateEventHandler(float deltaTime);

        public delegate void CustomRotationModeUpdateEventHandler(float deltaTime);
        
        public delegate void BeforeSimulationUpdateEventHandler(float deltaTime);
        public delegate void AfterSimulationUpdateEventHandler(float deltaTime);
        public delegate void CharacterMovementUpdateEventHandler(float deltaTime);
        
        public delegate void CollidedEventHandler(ref CollisionResult collisionResult);
        public delegate void FoundGroundEventHandler(ref FindGroundResult foundGround);
        public delegate void LandedEventHandled(Vector3 landingVelocity);
        
        public delegate void CrouchedEventHandler();
        public delegate void UnCrouchedEventHandler();
        
        public delegate void JumpedEventHandler();
        public delegate void ReachedJumpApexEventHandler();
        
        /// <summary>
        /// Event triggered when a character enter or leaves a PhysicsVolume.
        /// </summary>

        public event PhysicsVolumeChangedEventHandler PhysicsVolumeChanged;
        
        /// <summary>
        /// Event triggered when a MovementMode change.
        /// </summary>

        public event MovementModeChangedEventHandler MovementModeChanged;
        
        /// <summary>
        /// Event for implementing custom character movement mode.
        /// Called if MovementMode is set to Custom.
        /// </summary>

        public event CustomMovementModeUpdateEventHandler CustomMovementModeUpdated;
        
        /// <summary>
        /// Event for implementing custom character rotation mode.
        /// Called when RotationMode is set to Custom.
        /// </summary>

        public event CustomRotationModeUpdateEventHandler CustomRotationModeUpdated;
        
        /// <summary>
        /// Event called before character simulation updates.
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>

        public event BeforeSimulationUpdateEventHandler BeforeSimulationUpdated;
        
        /// <summary>
        /// Event called after character simulation updates.
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>
        
        public event AfterSimulationUpdateEventHandler AfterSimulationUpdated;
        
        /// <summary>
        /// Event called when CharacterMovement component is updated (ie. Move call).
        /// At this point the character movement has completed and its state is current. 
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>

        public event CharacterMovementUpdateEventHandler CharacterMovementUpdated;
        
        /// <summary>
        /// Event triggered when characters collides with other during a Move.
        /// Can be called multiple times.
        /// </summary>

        public event CollidedEventHandler Collided;

        /// <summary>
        /// Event triggered when a character finds ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        public event FoundGroundEventHandler FoundGround;
        
        /// <summary>
        /// Event triggered when a character is falling and finds walkable ground as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        public event LandedEventHandled Landed;
        
        /// <summary>
        /// Event triggered when Character enters crouching state.
        /// </summary>

        public event CrouchedEventHandler Crouched;

        /// <summary>
        /// Event triggered when character exits crouching state.
        /// </summary>

        public event UnCrouchedEventHandler UnCrouched;
        
        /// <summary>
        /// Event triggered when character jumps.
        /// </summary>

        public event JumpedEventHandler Jumped;

        /// <summary>
        /// Triggered when Character reaches jump apex (eg: change in vertical speed from positive to negative).
        /// Only triggered if notifyJumpApex == true.
        /// </summary>

        public event ReachedJumpApexEventHandler ReachedJumpApex;
        
        /// <summary>
        /// Event for implementing custom character movement mode.
        /// Called if MovementMode is set to Custom.
        /// Derived Character classes should override CustomMovementMode method instead. 
        /// </summary>
        
        protected virtual void OnCustomMovementMode(float deltaTime)
        {
            // Trigger event
            
            CustomMovementModeUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Event for implementing custom character rotation mode.
        /// Called if RotationMode is set to Custom.
        /// Derived Character classes should override CustomRotationMode method instead. 
        /// </summary>
        
        protected virtual void OnCustomRotationMode(float deltaTime)
        {
            CustomRotationModeUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Called at the beginning of the Character Simulation, before current movement mode update.
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>
        
        protected virtual void OnBeforeSimulationUpdate(float deltaTime)
        {
            BeforeSimulationUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Called after current movement mode update.
        /// This 'hook' lets you externally update the character 'state'. 
        /// </summary>

        protected virtual void OnAfterSimulationUpdate(float deltaTime)
        {
            AfterSimulationUpdated?.Invoke(deltaTime);
        }
        
        /// <summary>
        /// Event called when CharacterMovement component is updated (ie. Move call).
        /// At this point the character movement has been applied and its state is current. 
        /// This 'hook' lets you externally update the character 'state'.
        /// </summary>

        protected virtual void OnCharacterMovementUpdated(float deltaTime)
        {
            CharacterMovementUpdated?.Invoke(deltaTime);
        }

        /// <summary>
        /// Event triggered when characters collides with other during a CharacterMovement Move call.
        /// Can be called multiple times.
        /// </summary>

        protected virtual void OnCollided(ref CollisionResult collisionResult)
        {
            Collided?.Invoke(ref collisionResult);
        }

        /// <summary>
        /// Event triggered when a character find ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        protected virtual void OnFoundGround(ref FindGroundResult foundGround)
        {
            FoundGround?.Invoke(ref foundGround);
        }

        /// <summary>
        /// Event triggered when character enter Walking movement mode (ie: isOnWalkableGround AND isConstrainedToGround).
        /// </summary>

        protected virtual void OnLanded(Vector3 landingVelocity)
        {
            Landed?.Invoke(landingVelocity);
        }
        
        /// <summary>
        /// Called when character crouches.
        /// </summary>
        
        protected virtual void OnCrouched()
        {
            Crouched?.Invoke();
        }
        
        /// <summary>
        /// Called when character un crouches.
        /// </summary>

        protected virtual void OnUnCrouched()
        {
            UnCrouched?.Invoke();
        }
        
        /// <summary>
        /// Called when a jump has been successfully triggered.
        /// </summary>
        
        protected virtual void OnJumped()
        {
            Jumped?.Invoke();
        }
        
        /// <summary>
        /// Called when Character reaches jump apex (eg: change in vertical speed from positive to negative).
        /// Only triggered if notifyJumpApex == true.
        /// </summary>

        protected virtual void OnReachedJumpApex()
        {
            ReachedJumpApex?.Invoke();
        }

        #endregion

        #region METHODS
        
        /// <summary>
        /// Returns the Character's gravity vector modified by gravityScale.
        /// </summary>

        public Vector3 GetGravityVector()
        {
            return gravity;
        }

        /// <summary>
        /// Returns the gravity direction (normalized).
        /// </summary>

        public Vector3 GetGravityDirection()
        {
            return gravity.normalized;
        }
        
        /// <summary>
        /// Returns the current gravity magnitude factoring current gravity scale.
        /// </summary>

        public float GetGravityMagnitude()
        {
            return gravity.magnitude;
        }

        /// <summary>
        /// Sets the Character's gravity vector
        /// </summary>

        public void SetGravityVector(Vector3 newGravityVector)
        {
            _gravity = newGravityVector;
        }
        
        /// <summary>
        /// Start / Stops Auto-simulation coroutine (ie: LateFixedUpdate).
        /// </summary>

        private void EnableAutoSimulationCoroutine(bool enable)
        {
            if (enable)
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);

                _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
            }
            else
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);
            }
        }
        
        /// <summary>
        /// Cache used components.
        /// </summary>

        protected virtual void CacheComponents()
        {
            _transform = GetComponent<Transform>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponentInChildren<Animator>();
            _rootMotionController = GetComponentInChildren<RootMotionController>();

            {
                characterMovement.impartPlatformMovement = _impartPlatformMovement;
                characterMovement.impartPlatformRotation = _impartPlatformRotation;
                characterMovement.impartPlatformVelocity = _impartPlatformVelocity;

                characterMovement.enablePhysicsInteraction = _enablePhysicsInteraction;
                characterMovement.physicsInteractionAffectsCharacters = _applyPushForceToCharacters;
                characterMovement.pushForceScale = _pushForceScale;
                
                mass = _mass;
            }
        }
        
        /// <summary>
        /// Sets the given new volume as our current Physics Volume.
        /// Trigger PhysicsVolumeChanged event.
        /// </summary>

        protected virtual void SetPhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            // Do nothing if nothing is changing
            
            if (newPhysicsVolume == physicsVolume)
                return;

            // Trigger PhysicsVolumeChanged event

            OnPhysicsVolumeChanged(newPhysicsVolume);

            // Updates current physics volume

            physicsVolume = newPhysicsVolume;
        }
        
        /// <summary>
        /// Called when this Character's PhysicsVolume has been changed.
        /// </summary>

        protected virtual void OnPhysicsVolumeChanged(PhysicsVolume newPhysicsVolume)
        {
            if (newPhysicsVolume && newPhysicsVolume.waterVolume)
            {
                // Entering a water volume
            
                SetMovementMode(MovementMode.Swimming);
            }
            else if (IsInWaterPhysicsVolume() && newPhysicsVolume == null)
            {
                // Left a water volume
            
                // If Swimming, change to Falling mode

                if (IsSwimming())
                {
                    SetMovementMode(MovementMode.Falling);
                }
            }
            
            // Trigger PhysicsVolumeChanged event

            PhysicsVolumeChanged?.Invoke(newPhysicsVolume);
        }

        /// <summary>
        /// Update character's current physics volume.
        /// </summary>

        protected virtual void UpdatePhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            // Check if Character is inside or outside a PhysicsVolume,
            // It uses the Character's center as reference point

            Vector3 characterCenter = characterMovement.worldCenter;

            if (newPhysicsVolume && newPhysicsVolume.boxCollider.ClosestPoint(characterCenter) == characterCenter)
            {
                // Entering physics volume

                SetPhysicsVolume(newPhysicsVolume);
            }
            else
            {
                // Leaving physics volume

                SetPhysicsVolume(null);
            }
        }

        /// <summary>
        /// Attempts to add a new physics volume to our volumes list.
        /// </summary>

        protected virtual void AddPhysicsVolume(Collider other)
        {
            if (other.TryGetComponent(out PhysicsVolume volume) && !_physicsVolumes.Contains(volume))
                _physicsVolumes.Insert(0, volume);
        }

        /// <summary>
        /// Attempts to remove a physics volume from our volumes list.
        /// </summary>

        protected virtual void RemovePhysicsVolume(Collider other)
        {
            if (other.TryGetComponent(out PhysicsVolume volume) && _physicsVolumes.Contains(volume))
                _physicsVolumes.Remove(volume);
        }

        /// <summary>
        /// Sets as current physics volume the one with higher priority.
        /// </summary>

        protected virtual void UpdatePhysicsVolumes()
        {
            // Find volume with higher priority

            PhysicsVolume volume = null;
            int maxPriority = int.MinValue;

            for (int i = 0, c = _physicsVolumes.Count; i < c; i++)
            {
                PhysicsVolume vol = _physicsVolumes[i];
                if (vol.priority <= maxPriority)
                    continue;

                maxPriority = vol.priority;
                volume = vol;
            }

            // Update character's current volume

            UpdatePhysicsVolume(volume);
        }
        
        /// <summary>
        /// Is the character in a water physics volume ?
        /// </summary>

        public virtual bool IsInWaterPhysicsVolume()
        {
            return physicsVolume && physicsVolume.waterVolume;
        }
        
        /// <summary>
        /// Adds a force to the Character.
        /// This forces will be accumulated and applied during Move method call.
        /// </summary>

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            characterMovement.AddForce(force, forceMode);
        }

        /// <summary>
        /// Applies a force to a rigidbody that simulates explosion effects.
        /// The explosion is modeled as a sphere with a certain centre position and radius in world space;
        /// normally, anything outside the sphere is not affected by the explosion and the force decreases in proportion to distance from the centre.
        /// However, if a value of zero is passed for the radius then the full force will be applied regardless of how far the centre is from the rigidbody.
        /// </summary>

        public void AddExplosionForce(float forceMagnitude, Vector3 origin, float explosionRadius,
            ForceMode forceMode = ForceMode.Force)
        {
            characterMovement.AddExplosionForce(forceMagnitude, origin, explosionRadius, forceMode);
        }

        /// <summary>
        /// Set a pending launch velocity on the Character. This velocity will be processed next Move call.
        /// If overrideVerticalVelocity is true replace the vertical component of the Character's velocity instead of adding to it.
        /// If overrideLateralVelocity is true replace the XY part of the Character's velocity instead of adding to it.
        /// </summary>

        public void LaunchCharacter(Vector3 launchVelocity, bool overrideVerticalVelocity = false,
            bool overrideLateralVelocity = false)
        {
            characterMovement.LaunchCharacter(launchVelocity, overrideVerticalVelocity, overrideLateralVelocity);
        }

        /// <summary>
        /// Should collision detection be enabled ?
        /// </summary>

        public void DetectCollisions(bool detectCollisions)
        {
            characterMovement.detectCollisions = detectCollisions;
        }

        /// <summary>
        /// Makes the character to ignore all collisions vs otherCollider.
        /// </summary>

        public void IgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            characterMovement.IgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Makes the character to ignore collisions vs all colliders attached to the otherRigidbody.
        /// </summary>

        public void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
        {
            characterMovement.IgnoreCollision(otherRigidbody, ignore);
        }

        /// <summary>
        /// Makes the character's collider (eg: CapsuleCollider) to ignore all collisions vs otherCollider.
        /// NOTE: The character can still collide with other during a Move call if otherCollider is in CollisionLayers mask.
        /// </summary>

        public void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            characterMovement.CapsuleIgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Jump, etc.
        /// </summary>

        public void PauseGroundConstraint(float seconds = 0.1f)
        {
            characterMovement.PauseGroundConstraint(seconds);
        }
        
        /// <summary>
        /// Should movement be constrained to ground when on walkable ground ?
        /// When enabled, character will be constrained to ground ignoring vertical velocity.  
        /// </summary>

        public void EnableGroundConstraint(bool enable)
        {
            characterMovement.constrainToGround = enable;
        }
        
        /// <summary>
        /// Was the character on ground last Move call ?
        /// </summary>

        public bool WasOnGround()
        {
            return characterMovement.wasOnGround;
        }

        /// <summary>
        /// Is the character on ground ?
        /// </summary>

        public bool IsOnGround()
        {
            return characterMovement.isOnGround;
        }

        /// <summary>
        /// Was the character on walkable ground last Move call ?
        /// </summary>

        public bool WasOnWalkableGround()
        {
            return characterMovement.wasOnWalkableGround;
        }

        /// <summary>
        /// Is the character on walkable ground ?
        /// </summary>

        public bool IsOnWalkableGround()
        {
            return characterMovement.isOnWalkableGround;
        }

        /// <summary>
        /// Was the character on walkable ground AND constrained to ground last Move call ?
        /// </summary>

        public bool WasGrounded()
        {
            return characterMovement.wasGrounded;
        }

        /// <summary>
        /// Is the character on walkable ground AND constrained to ground.
        /// </summary>

        public bool IsGrounded()
        {
            return characterMovement.isGrounded;
        }
        
        /// <summary>
        /// Return the CharacterMovement component. This is guaranteed to be not null.
        /// </summary>

        public CharacterMovement GetCharacterMovement()
        {
            return characterMovement;
        }

        /// <summary>
        /// Return the Animator component or null if not found.
        /// </summary>

        public Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Return the RootMotionController or null is not found.
        /// </summary>

        public RootMotionController GetRootMotionController()
        {
            return rootMotionController;
        }

        /// <summary>
        /// Return the Character's current PhysicsVolume, null if none.
        /// </summary>

        public PhysicsVolume GetPhysicsVolume()
        {
            return physicsVolume;
        }
        
        /// <summary>
        /// The Character's current position.
        /// </summary>

        public Vector3 GetPosition()
        {
            return characterMovement.position;
        }

        /// <summary>
        /// Sets the Character's position.
        /// This complies with the interpolation resulting in a smooth transition between the two positions in any intermediate frames rendered.
        /// </summary>

        public void SetPosition(Vector3 position, bool updateGround = false)
        {
            characterMovement.SetPosition(position, updateGround);
        }

        /// <summary>
        /// Instantly modify the character's position.
        /// Unlike SetPosition this disables rigidbody interpolation (interpolating == true) before updating the character's position resulting in an instant movement.
        /// If interpolating == true it will re-enable rigidbody interpolation after teleportation.
        /// </summary>

        public void TeleportPosition(Vector3 newPosition, bool interpolating = true, bool updateGround = false)
        {
            if (interpolating)
            {
                characterMovement.interpolation = RigidbodyInterpolation.None;
            }

            characterMovement.SetPosition(newPosition, updateGround);

            if (interpolating)
            {
                characterMovement.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        
        /// <summary>
        /// The Character's current rotation.
        /// </summary>

        public Quaternion GetRotation()
        {
            return characterMovement.rotation;
        }

        /// <summary>
        /// Sets the Character's current rotation.
        /// </summary>

        public void SetRotation(Quaternion newRotation)
        {
            characterMovement.rotation = newRotation;
        }
        
        /// <summary>
        /// Instantly modify the character's rotation.
        /// Unlike SetRotation this disables rigidbody interpolation (interpolating == true) before updating the character's rotation resulting in an instant rotation.
        /// If interpolating == true it will re-enable rigidbody interpolation after teleportation.
        /// </summary>

        public void TeleportRotation(Quaternion newRotation, bool interpolating = true)
        {
            if (interpolating)
            {
                characterMovement.interpolation = RigidbodyInterpolation.None;
            }

            characterMovement.SetRotation(newRotation);

            if (interpolating)
            {
                characterMovement.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
        
        /// <summary>
        /// The Character's current up vector.
        /// </summary>

        public virtual Vector3 GetUpVector()
        {
            return transform.up;
        }

        /// <summary>
        /// The Character's current right vector.
        /// </summary>

        public virtual Vector3 GetRightVector()
        {
            return transform.right;
        }

        /// <summary>
        /// The Character's current forward vector.
        /// </summary>

        public virtual Vector3 GetForwardVector()
        {
            return transform.forward;
        }
        
        /// <summary>
        /// Orient the character's towards the given direction (in world space) using rotationRate as the rate of rotation change.
        /// If updateYawOnly is true, rotation will affect character's yaw axis only (defined by its up-axis).
        /// </summary>
        
        public virtual void RotateTowards(Vector3 worldDirection, float deltaTime, bool updateYawOnly = true)
        {
            Vector3 characterUp = GetUpVector();

            if (updateYawOnly)
                worldDirection = Vector3.ProjectOnPlane(worldDirection, characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);
            characterMovement.rotation = Quaternion.RotateTowards(rotation, targetRotation, rotationRate * deltaTime);
        }
        
        /// <summary>
        /// Append root motion rotation to Character's rotation.
        /// </summary>

        protected virtual void RotateWithRootMotion()
        {
            if (useRootMotion && rootMotionController)
                characterMovement.rotation = rootMotionController.ConsumeRootMotionRotation() * characterMovement.rotation;
        }
        
        /// <summary>
        /// The current relative velocity of the Character.
        /// The velocity is relative because it won't track movements to the transform that happen outside of this,
        /// e.g. character parented under another moving Transform, such as a moving vehicle.
        /// </summary>

        public Vector3 GetVelocity()
        {
            return characterMovement.velocity;
        }

        /// <summary>
        /// Sets the character's velocity.
        /// </summary>

        public void SetVelocity(Vector3 newVelocity)
        {
            characterMovement.velocity = newVelocity;
        }
        
        /// <summary>
        /// The Character's current speed.
        /// </summary>

        public float GetSpeed()
        {
            return characterMovement.velocity.magnitude;
        }
        
        /// <summary>
        /// The character's radius
        /// </summary>

        public float GetRadius()
        {
            return characterMovement.radius;
        }
        
        /// <summary>
        /// The character's current height.
        /// </summary>

        public float GetHeight()
        {
            return characterMovement.height;
        }
        
        /// <summary>
        /// The current movement direction (in world space), eg: the movement direction used to move this Character.
        /// </summary>

        public Vector3 GetMovementDirection()
        {
            return _movementDirection;
        }

        /// <summary>
        /// Assigns the Character's movement direction (in world space), eg: our desired movement direction vector.
        /// </summary>

        public void SetMovementDirection(Vector3 movementDirection)
        {
            _movementDirection = movementDirection;
        }
        
        /// <summary>
        /// Sets the yaw value.
        /// This will reset current pitch and roll values.
        /// </summary>

        public virtual void SetYaw(float value)
        {
            characterMovement.rotation = Quaternion.Euler(0.0f, value, 0.0f);
        }
        
        /// <summary>
        /// Amount to add to Yaw (up axis).
        /// </summary>

        public virtual void AddYawInput(float value)
        {
            _rotationInput.y += value;
        }

        /// <summary>
        /// Amount to add to Pitch (right axis).
        /// </summary>

        public virtual void AddPitchInput(float value)
        {
            _rotationInput.x += value;
        }

        /// <summary>
        /// Amount to add to Roll (forward axis).
        /// </summary>

        public virtual void AddRollInput(float value)
        {
            _rotationInput.z += value;
        }

        /// <summary>
        /// Append input rotation (eg: AddPitchInput, AddYawInput, AddRollInput) to character rotation.
        /// </summary>

        protected virtual void ConsumeRotationInput()
        {
            // Apply rotation input (if any)

            if (_rotationInput != Vector3.zero)
            {
                // Consumes rotation input (e.g. apply and clear it)

                characterMovement.rotation *= Quaternion.Euler(_rotationInput);

                _rotationInput = Vector3.zero;
            }
        }

        /// <summary>
        /// The Character's current movement mode.
        /// </summary>

        public MovementMode GetMovementMode()
        {
            return _movementMode;
        }
        
        /// <summary>
        /// Character's User-defined custom movement mode (sub-mode).
        /// Only applicable if _movementMode == Custom.
        /// </summary>

        public int GetCustomMovementMode()
        {
            return _customMovementMode;
        }
        
        /// <summary>
        /// Change movement mode.
        /// The new custom sub-mode (newCustomMode), is only applicable if newMovementMode == Custom.
        ///
        /// Trigger OnMovementModeChanged event.
        /// </summary>

        public void SetMovementMode(MovementMode newMovementMode, int newCustomMode = 0)
        {
            // Do nothing if nothing is changing

            if (newMovementMode == _movementMode)
            {
                // Allow changes in custom sub-modes

                if (newMovementMode != MovementMode.Custom || newCustomMode == _customMovementMode)
                    return;
            }

            // Performs movement mode change

            MovementMode prevMovementMode = _movementMode;
            int prevCustomMode = _customMovementMode;

            _movementMode = newMovementMode;
            _customMovementMode = newCustomMode;

            OnMovementModeChanged(prevMovementMode, prevCustomMode);
        }
        
        /// <summary>
        /// Called after MovementMode has changed.
        /// Does special handling for starting certain modes, eg: enable / disable ground constraint, etc.
        /// If overridden, base method MUST be called.
        /// </summary>

        protected virtual void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Perform additional tasks on mode change

            switch (movementMode)
            {
                case MovementMode.None:
                    
                    // Entering None mode...

                    // Disable Character's movement and clear any pending forces
                    
                    characterMovement.velocity = Vector3.zero;
                    characterMovement.ClearAccumulatedForces();
                    
                    break;
                
                case MovementMode.Walking:
                    
                    // Entering Walking mode...
                    
                    // Reset jump
                    
                    ResetJumpState();
                    
                    // If was flying or swimming, enable ground constraint
            
                    if (prevMovementMode == MovementMode.Flying || prevMovementMode == MovementMode.Swimming)
                        characterMovement.constrainToGround = true;
                    
                    // Trigger Landed event
                    
                    OnLanded(characterMovement.landedVelocity);
                    
                    break;
                
                case MovementMode.Falling:
                    
                    // Entering Falling mode...
                    
                    // If was flying or swimming, enable ground constraint as it could lands on walkable ground
                    
                    if (prevMovementMode == MovementMode.Flying || prevMovementMode == MovementMode.Swimming)
                        characterMovement.constrainToGround = true;
                    
                    break;
                
                case MovementMode.Flying:
                case MovementMode.Swimming:
                    
                    // Entering Flying or Swimming mode...
                    
                    // Reset jump

                    ResetJumpState();
                    
                    // Disable ground constraint

                    characterMovement.constrainToGround = false;
                    
                    break;
            }
            
            // Left Falling mode, reset falling timer

            if (!IsFalling())
                _fallingTime = 0.0f;
            
            // Trigger movement mode changed event
            
            MovementModeChanged?.Invoke(prevMovementMode, prevCustomMode);
        }
        
        /// <summary>
        /// Returns true if the Character is in the Walking movement mode (eg: on walkable ground).
        /// </summary>

        public virtual bool IsWalking()
        {
            return _movementMode == MovementMode.Walking;
        }

        /// <summary>
        /// Returns true if currently falling, eg: on air (not flying) or in not walkable ground.
        /// </summary>

        public virtual bool IsFalling()
        {
            return _movementMode == MovementMode.Falling;
        }

        /// <summary>
        /// Returns true if currently flying (moving through a non-water volume without resting on the ground).
        /// </summary>

        public virtual bool IsFlying()
        {
            return _movementMode == MovementMode.Flying;
        }

        /// <summary>
        /// Returns true if currently swimming (moving through a water volume).
        /// </summary>

        public virtual bool IsSwimming()
        {
            return _movementMode == MovementMode.Swimming;
        }

        /// <summary>
        /// The maximum speed for current movement mode (accounting crouching state).
        /// </summary>

        public virtual float GetMaxSpeed()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                    return IsCrouched() ? maxWalkSpeedCrouched : maxWalkSpeed;

                case MovementMode.Falling:
                    return maxWalkSpeed;

                case MovementMode.Swimming:
                    return maxSwimSpeed;

                case MovementMode.Flying:
                    return maxFlySpeed;

                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// The ground speed that we should accelerate up to when walking at minimum analog stick tilt.
        /// </summary>

        public virtual float GetMinAnalogSpeed()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                case MovementMode.Falling:
                    return minAnalogWalkSpeed;

                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// The acceleration for current movement mode.
        /// </summary>

        public virtual float GetMaxAcceleration()
        {
            if (IsFalling())
                return maxAcceleration * airControl;

            return maxAcceleration;
        }

        /// <summary>
        /// The braking deceleration for current movement mode.
        /// </summary>

        public virtual float GetMaxBrakingDeceleration()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                    return brakingDecelerationWalking;

                case MovementMode.Falling:
                {
                    // Falling,
                    // BUT ON non-walkable ground, bypass braking deceleration to force slide off

                    return characterMovement.isOnGround ? 0.0f : brakingDecelerationFalling;
                }

                case MovementMode.Swimming:
                    return brakingDecelerationSwimming;

                case MovementMode.Flying:
                    return brakingDecelerationFlying;

                default:
                    return 0.0f;
            }
        }
        
        /// <summary>
        /// Computes the analog input modifier (0.0f to 1.0f) based on current input vector and desired velocity.
        /// </summary>
        
        protected virtual float ComputeAnalogInputModifier(Vector3 desiredVelocity)
        {
            float maxSpeed = GetMaxSpeed();
            
            if (desiredVelocity.sqrMagnitude > 0.0f && maxSpeed > 0.00000001f)
            {
                return Mathf.Clamp01(desiredVelocity.magnitude / maxSpeed);
            }

            return 0.0f;
        }
        
        /// <summary>
        /// Apply friction and braking deceleration to given velocity.
        /// Returns modified input velocity.
        /// </summary>
        
        public virtual Vector3 ApplyVelocityBraking(Vector3 velocity, float friction, float maxBrakingDeceleration, float deltaTime)
        {
            const float kMinTickTime = 0.000001f;
            if (velocity.isZero() || deltaTime < kMinTickTime)
                return velocity;
            
            bool isZeroFriction = friction == 0.0f;
            bool isZeroBraking = maxBrakingDeceleration == 0.0f;
            if (isZeroFriction && isZeroBraking)
                return velocity;
            
            // Decelerate to brake to a stop
            
            Vector3 oldVel = velocity;
            Vector3 revAccel = isZeroBraking ? Vector3.zero : -maxBrakingDeceleration * velocity.normalized;
            
            // Subdivide braking to get reasonably consistent results at lower frame rates
            
            const float kMaxTimeStep = 1.0f / 33.0f;
            
            float remainingTime = deltaTime;
            while (remainingTime >= kMinTickTime)
            {
                // Zero friction uses constant deceleration, so no need for iteration

                float dt = remainingTime > kMaxTimeStep && !isZeroFriction
                    ? Mathf.Min(kMaxTimeStep, remainingTime * 0.5f)
                    : remainingTime;
                
                remainingTime -= dt;
                
                // Apply friction and braking
                
                velocity += (-friction * velocity + revAccel) * dt;
                
                // Don't reverse direction
                
                if (Vector3.Dot(velocity, oldVel) <= 0.0f)
                    return Vector3.zero;
            }
            
            // Clamp to zero if nearly zero, or if below min threshold and braking
            
            float sqrSpeed = velocity.sqrMagnitude;
            if (sqrSpeed <= 0.00001f || (!isZeroBraking && sqrSpeed <= 0.1f))
                return Vector3.zero;

            return velocity;
        }

        /// <summary>
        /// Calculates a new velocity for the given state, applying the effects of friction or
        /// braking friction and acceleration or deceleration.
        /// </summary>

        public virtual Vector3 CalcVelocity(Vector3 velocity, Vector3 desiredVelocity, float friction, bool isFluid, float deltaTime)
        {
            const float kMinTickTime = 0.000001f;
            if (deltaTime < kMinTickTime)
                return velocity;
            
            // Compute requested move direction

            float desiredSpeed = desiredVelocity.magnitude;
            Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

            // Requested acceleration (factoring analog input)

            float analogInputModifier = ComputeAnalogInputModifier(desiredVelocity);
            Vector3 inputAcceleration = GetMaxAcceleration() * analogInputModifier * desiredMoveDirection;

            // Actual max speed (factoring analog input)

            float actualMaxSpeed = Mathf.Max(GetMinAnalogSpeed(), GetMaxSpeed() * analogInputModifier);
            
            // Apply braking or deceleration
            
            bool isZeroAcceleration = inputAcceleration.isZero();
            bool isVelocityOverMax = velocity.isExceeding(actualMaxSpeed);
            
            // Only apply braking if there is no acceleration, or we are over our max speed and need to slow down to it.

            if (isZeroAcceleration || isVelocityOverMax)
            {
                Vector3 oldVelocity = velocity;
                
                // Apply friction and braking

                float actualBrakingFriction = useSeparateBrakingFriction ? brakingFriction : friction;
                float actualBrakingAcceleration =
                    useSeparateBrakingDeceleration ? brakingDeceleration : GetMaxBrakingDeceleration();

                velocity = ApplyVelocityBraking(velocity, actualBrakingFriction, actualBrakingAcceleration, deltaTime);
                
                // Don't allow braking to lower us below max speed if we started above it.
                
                if (isVelocityOverMax && velocity.sqrMagnitude < actualMaxSpeed.square() && Vector3.Dot(inputAcceleration, oldVelocity) > 0.0f)
                    velocity = oldVelocity.normalized * actualMaxSpeed;
            }
            else
            {
                // Friction, this affects our ability to change direction
                
                Vector3 accelDir = inputAcceleration.normalized;
                float velMag = velocity.magnitude;

                velocity -= (velocity - accelDir * velMag) * Mathf.Min(friction * deltaTime, 1.0f);
            }
            
            // Apply fluid friction
            
            if (isFluid)
                velocity *= 1.0f - Mathf.Min(friction * deltaTime, 1.0f);
            
            // Apply input acceleration

            if (!isZeroAcceleration)
            {
                float newMaxSpeed = velocity.isExceeding(actualMaxSpeed) ? velocity.magnitude : actualMaxSpeed;

                velocity += inputAcceleration * deltaTime;
                velocity = velocity.clampedTo(newMaxSpeed);
            }

            return velocity;
        }
        
        /// <summary>
        /// Enforce constraints on input vector given current movement mode.
        /// Return constrained input vector.
        /// </summary>
        
        public virtual Vector3 ConstrainInputVector(Vector3 inputVector)
        {
            Vector3 worldUp = -GetGravityDirection();
            
            float inputVectorDotWorldUp = Vector3.Dot(inputVector, worldUp);
            if (!Mathf.Approximately(inputVectorDotWorldUp, 0.0f) && (IsWalking() || IsFalling()))
                inputVector = Vector3.ProjectOnPlane(inputVector, worldUp);

            return characterMovement.ConstrainVectorToPlane(inputVector);
        }
        
        /// <summary>
        /// Calculate the desired velocity for current movement mode.
        /// </summary>

        protected virtual void CalcDesiredVelocity(float deltaTime)
        {
            // Current movement direction

            Vector3 movementDirection = Vector3.ClampMagnitude(GetMovementDirection(), 1.0f);

            // The desired velocity from animation (if using root motion) or from input movement vector

            Vector3 desiredVelocity = useRootMotion && rootMotionController
                ? rootMotionController.ConsumeRootMotionVelocity(deltaTime)
                : movementDirection * GetMaxSpeed();
            
            // Return constrained desired velocity

            _desiredVelocity = ConstrainInputVector(desiredVelocity);
        }
        
        /// <summary>
        /// Calculated desired velocity for current movement mode.
        /// </summary>

        public virtual Vector3 GetDesiredVelocity()
        {
            return _desiredVelocity;
        }
        
        /// <summary>
        /// Calculates the signed slope angle in degrees for current movement direction.
        /// Positive if moving up-slope, negative if moving down-slope or 0 if Character
        /// is not on ground or not moving (ie: movementDirection == Vector3.zero).
        /// </summary>
        
        public float GetSignedSlopeAngle()
        {
            Vector3 movementDirection = GetMovementDirection();
            if (movementDirection.isZero() || !IsOnGround())
                return 0.0f;

            Vector3 projMovementDirection =
                Vector3.ProjectOnPlane(movementDirection, characterMovement.groundNormal).normalized;

            return Mathf.Asin(Vector3.Dot(projMovementDirection, -GetGravityDirection())) * Mathf.Rad2Deg;
        }
        
        /// <summary>
        /// Apply a downward force when standing on top of non-kinematic physics objects (if applyStandingDownwardForce == true).
        /// The force applied is: mass * gravity * standingDownwardForceScale
        /// </summary>

        protected virtual void ApplyDownwardsForce()
        {
            Rigidbody groundRigidbody = characterMovement.groundRigidbody;
            if (!groundRigidbody || groundRigidbody.isKinematic)
                return;

            Vector3 downwardForce = mass * GetGravityVector();
            groundRigidbody.AddForceAtPosition(downwardForce * standingDownwardForceScale, GetPosition());
        }

        /// <summary>
        /// Update Character's velocity while moving on walkable surfaces.
        /// </summary>
        
        protected virtual void WalkingMovementMode(float deltaTime)
        {
            // If using root motion, use animation velocity

            if (useRootMotion && rootMotionController)
                characterMovement.velocity = GetDesiredVelocity();
            else
            {
                // Calculate new velocity

                characterMovement.velocity =
                    CalcVelocity(characterMovement.velocity, GetDesiredVelocity(), groundFriction, false, deltaTime);
            }
            
            // Apply downwards force

            if (applyStandingDownwardForce)
                ApplyDownwardsForce();
        }
        
        /// <summary>
        /// True if the character is currently crouched, false otherwise.
        /// </summary>

        public virtual bool IsCrouched()
        {
            return _isCrouched;
        }
        
        /// <summary>
        /// Request the Character to crouch.
        /// The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public virtual void Crouch()
        {
            crouchInputPressed = true;
        }
        
        /// <summary>
        /// Request the Character to stop crouching.
        /// The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public virtual void UnCrouch()
        {
            crouchInputPressed = false;
        }
        
        /// <summary>
        /// Determines if the Character is able to crouch in its current state.
        /// Defaults to Walking mode only.
        /// </summary>

        protected virtual bool IsCrouchAllowed()
        {
            return canEverCrouch && IsWalking();
        }
        
        /// <summary>
        /// Determines if the Character is able to un crouch.
        /// Eg. Check if there's room to expand capsule, etc.
        /// </summary>
        
        protected virtual bool CanUnCrouch()
        {
            bool overlapped = characterMovement.CheckHeight(_unCrouchedHeight);
            return !overlapped;
        }
        
        /// <summary>
        /// Check crouch input and attempts to perform the requested crouch.
        /// </summary>
        
        protected virtual void CheckCrouchInput()
        {
            if (!_isCrouched && crouchInputPressed && IsCrouchAllowed())
            {
                _isCrouched = true;
                characterMovement.SetHeight(_crouchedHeight);

                OnCrouched();
            }
            else if (_isCrouched && (!crouchInputPressed || !IsCrouchAllowed()))
            {
                if (!CanUnCrouch())
                    return;
                
                _isCrouched = false;
                characterMovement.SetHeight(_unCrouchedHeight);

                OnUnCrouched();
            }
        }
        
        /// <summary>
        /// Update Character's velocity while falling.
        /// Applies gravity and make sure it don't exceed terminal velocity.
        /// </summary>

        protected virtual void FallingMovementMode(float deltaTime)
        {
            // Current target velocity

            Vector3 desiredVelocity = GetDesiredVelocity();
            
            // World-up defined by gravity direction

            Vector3 worldUp = -GetGravityDirection();
            
            // On not walkable ground...

            if (IsOnGround() && !IsOnWalkableGround())
            {
                // If moving into the 'wall', limit contribution

                Vector3 groundNormal = characterMovement.groundNormal;

                if (Vector3.Dot(desiredVelocity, groundNormal) < 0.0f)
                {
                    // Allow movement parallel to the wall, but not into it because that may push us up

                    Vector3 groundNormal2D = Vector3.ProjectOnPlane(groundNormal, worldUp).normalized;
                    desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, groundNormal2D);
                }

                // Make velocity calculations planar by projecting the up vector into non-walkable surface

                worldUp = Vector3.ProjectOnPlane(worldUp, groundNormal).normalized;
            }

            // Separate velocity into its components

            Vector3 verticalVelocity = Vector3.Project(characterMovement.velocity, worldUp);
            Vector3 lateralVelocity = characterMovement.velocity - verticalVelocity;

            // Update lateral velocity

            lateralVelocity = CalcVelocity(lateralVelocity, desiredVelocity, fallingLateralFriction, false, deltaTime);

            // Update vertical velocity

            verticalVelocity += gravity * deltaTime;

            // Don't exceed terminal velocity.

            float actualFallSpeed = maxFallSpeed;
            if (physicsVolume)
                actualFallSpeed = physicsVolume.maxFallSpeed;

            if (Vector3.Dot(verticalVelocity, worldUp) < -actualFallSpeed)
                verticalVelocity = Vector3.ClampMagnitude(verticalVelocity, actualFallSpeed);

            // Apply new velocity

            characterMovement.velocity = lateralVelocity + verticalVelocity;

            // Update falling timer

            _fallingTime += deltaTime;
        }

        /// <summary>
        /// True if the character is jumping, false otherwise.
        /// </summary>

        public virtual bool IsJumping()
        {
            return _isJumping;
        }
        
        /// <summary>
        /// Request the Character to jump. The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>
        
        public virtual void Jump()
        {
            jumpInputPressed = true;
        }
        
        /// <summary>
        /// Request the Character to end a jump. The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public virtual void StopJumping()
        {
            jumpInputPressed = false;
            jumpInputHoldTime = 0.0f;
            
            ResetJumpState();
        }
        
        /// <summary>
        /// Reset jump related vars.
        /// </summary>
        
        protected virtual void ResetJumpState()
        {
            if (!IsFalling())
                jumpCurrentCount = 0;

            jumpForceTimeRemaining = 0.0f;
            
            _isJumping = false;
        }
        
        /// <summary>
        /// True if jump is actively providing a force, such as when the jump input is held
        /// and the time it has been held is less than jumpMaxHoldTime.
        /// </summary>
        
        public virtual bool IsJumpProvidingForce()
        {
            return jumpForceTimeRemaining > 0.0f;
        }
        
        /// <summary>
        /// Compute the max jump height based on the jumpImpulse velocity and gravity.
        /// This does not take into account the jumpMaxHoldTime. 
        /// </summary>
        
        public virtual float GetMaxJumpHeight()
        {
            float gravityMagnitude = GetGravityMagnitude();
            if (gravityMagnitude > 0.0001f)
            {
                return jumpImpulse * jumpImpulse / (2.0f * gravityMagnitude);
            }
            
            return 0.0f;
        }
        
        /// <summary>
        /// Compute the max jump height based on the jumpImpulse velocity and gravity.
        /// This does take into account the jumpMaxHoldTime. 
        /// </summary>
        
        public virtual float GetMaxJumpHeightWithJumpTime()
        {
            float maxJumpHeight = GetMaxJumpHeight();
            return maxJumpHeight + jumpImpulse * jumpMaxHoldTime;
        }
        
        /// <summary>
        /// Determines if the Character is able to jump in its current state.
        /// </summary>
        
        protected virtual bool IsJumpAllowed()
        {
            if (!canJumpWhileCrouching && IsCrouched())
                return false;

            return canEverJump && (IsWalking() || IsFalling());
        }
        
        /// <summary>
        /// Determines if the Character is able to perform the requested jump.
        /// </summary>
        
        protected virtual bool CanJump()
        {
            // Ensure that the Character state is valid

            bool isJumpAllowed = IsJumpAllowed();
            if (isJumpAllowed)
            {
                // Ensure jumpCurrentCount and jumpInputHoldTime are valid
                
                if (!_isJumping || jumpMaxHoldTime <= 0.0f)
                {
                    if (jumpCurrentCount == 0)
                    {
                        // On first jump, jumpInputHoldTime MUST be within jumpMaxPreGroundedTime grace period
                        
                        isJumpAllowed = jumpInputHoldTime <= jumpMaxPreGroundedTime;
                        
                        // If is a valid jump, reset jumpInputHoldTime,
                        // otherwise jump hold will be inaccurate due jumpInputHoldTime not starting at zero
                        
                        if (isJumpAllowed)
                            jumpInputHoldTime = 0.0f;
                    }
                    else
                    {
                        // Consecutive jump, must be enough jumps and a new press (ie: jumpInputHoldTime == 0.0f)
                        
                        isJumpAllowed = jumpCurrentCount < jumpMaxCount && jumpInputHoldTime == 0.0f;
                    }
                }
                else
                {
                    // Only consider JumpInputHoldTime as long as:
                    // A) The jump limit hasn't been met OR
                    // B) The jump limit has been met AND we were already jumping

                    bool jumpInputHeld = jumpInputPressed && jumpInputHoldTime < jumpMaxHoldTime;
                
                    isJumpAllowed = jumpInputHeld && (jumpCurrentCount < jumpMaxCount || (_isJumping && jumpCurrentCount == jumpMaxCount));
                }
            }

            return isJumpAllowed;
        }
        
        /// <summary>
        /// Perform the jump applying jumpImpulse.
        /// This can be called multiple times in case jump is providing force (ie: variable height jump).
        /// </summary>
        
        protected virtual bool DoJump()
        {
            // World up, determined by gravity direction
            
            Vector3 worldUp = -GetGravityDirection();
            
            // Don't jump if we can't move up/down.
            
            if (characterMovement.isConstrainedToPlane && 
                Mathf.Approximately(Vector3.Dot(characterMovement.GetPlaneConstraintNormal(), worldUp), 1.0f))
            {
                return false;
            }
            
            // Apply jump impulse along world up defined by gravity direction
            
            float verticalSpeed = Mathf.Max(Vector3.Dot(characterMovement.velocity, worldUp), jumpImpulse);

            characterMovement.velocity =
                Vector3.ProjectOnPlane(characterMovement.velocity, worldUp) + worldUp * verticalSpeed;
            
            return true;
        }
        
        /// <summary>
        /// Check jump input and attempts to perform the requested jump.
        /// </summary>
        
        protected virtual void CheckJumpInput()
        {
            if (!jumpInputPressed)
                return;
            
            // If this is the first jump and we're already falling, then increment the JumpCount to compensate,
            // ONLY if missed post grounded time tolerance

            if (jumpCurrentCount == 0 && IsFalling() && fallingTime > jumpMaxPostGroundedTime)
                jumpCurrentCount++;

            bool didJump = CanJump() && DoJump();
            if (didJump)
            {
                // Transition from not (actively) jumping to jumping
                    
                if (!_isJumping)
                {
                    jumpCurrentCount++;
                    jumpForceTimeRemaining = jumpMaxHoldTime;
                    
                    characterMovement.PauseGroundConstraint();
                    SetMovementMode(MovementMode.Falling);
                    
                    OnJumped();
                }
            }

            _isJumping = didJump;
        }
        
        /// <summary>
        /// Update jump related timers
        /// </summary>
        
        protected virtual void UpdateJumpTimers(float deltaTime)
        {
            if (jumpInputPressed)
                jumpInputHoldTime += deltaTime;
            
            if (jumpForceTimeRemaining > 0.0f)
            {
                jumpForceTimeRemaining -= deltaTime;
                if (jumpForceTimeRemaining <= 0.0f)
                    ResetJumpState();
            }
        }
        
        /// <summary>
        /// If notifyJumpApex is true, track vertical velocity change to trigger ReachedJumpApex event.
        /// </summary>
        
        protected virtual void NotifyJumpApex()
        {
            if (!notifyJumpApex)
                return;

            float verticalSpeed = Vector3.Dot(GetVelocity(), -GetGravityDirection());
            if (verticalSpeed >= 0.0f)
                return;

            notifyJumpApex = false;
            OnReachedJumpApex();
        }
        
        /// <summary>
        /// Determines the Character's movement when 'flying'. Affected by the current physics volume's friction (if any).
        /// Ground-Unconstrained movement with full desiredVelocity (lateral AND vertical) and gravity-less.
        /// </summary>

        protected virtual void FlyingMovementMode(float deltaTime)
        {
            if (useRootMotion && rootMotionController)
                characterMovement.velocity = GetDesiredVelocity();
            else
            {
                float actualFriction = IsInWaterPhysicsVolume() ? physicsVolume.friction : flyingFriction;

                characterMovement.velocity 
                    = CalcVelocity(characterMovement.velocity, GetDesiredVelocity(), actualFriction, true, deltaTime);
            }
        }
        
        /// <summary>
        /// How deep in water the character is immersed.
        /// Returns a float in range 0.0 = not in water, 1.0 = fully immersed.
        /// </summary>

        public virtual float CalcImmersionDepth()
        {
            float depth = 0.0f;

            if (IsInWaterPhysicsVolume())
            {
                float height = characterMovement.height;
                if (height == 0.0f || buoyancy == 0.0f)
                    depth = 1.0f;
                else
                {
                    Vector3 worldUp = -GetGravityDirection();
                    
                    Vector3 rayOrigin = GetPosition() + worldUp * height;
                    Vector3 rayDirection = -worldUp;

                    BoxCollider waterVolumeCollider = physicsVolume.boxCollider;
                    depth = !waterVolumeCollider.Raycast(new Ray(rayOrigin, rayDirection), out RaycastHit hitInfo, height)
                        ? 1.0f
                        : 1.0f - Mathf.InverseLerp(0.0f, height, hitInfo.distance);
                }
            }
            
            return depth;
        }
        
        /// <summary>
        /// Determines the Character's movement when Swimming through a fluid volume, under the effects of gravity and buoyancy.
        /// Ground-Unconstrained movement with full desiredVelocity (lateral AND vertical) applies gravity but scaled by (1.0f - buoyancy).
        /// </summary>

        protected virtual void SwimmingMovementMode(float deltaTime)
        {
            // Compute actual buoyancy factoring current immersion depth
            
            float depth = CalcImmersionDepth();
            float actualBuoyancy = buoyancy * depth;
            
            // Calculate new velocity

            Vector3 desiredVelocity = GetDesiredVelocity();
            Vector3 newVelocity = characterMovement.velocity;
            
            Vector3 worldUp = -GetGravityDirection();
            float verticalSpeed = Vector3.Dot(newVelocity, worldUp);
            
            if (verticalSpeed > maxSwimSpeed * 0.33f && actualBuoyancy > 0.0f)
            {
                // Damp positive vertical speed (out of water)

                verticalSpeed = Mathf.Max(maxSwimSpeed * 0.33f, verticalSpeed * depth * depth);
                newVelocity = Vector3.ProjectOnPlane(newVelocity, worldUp) + worldUp * verticalSpeed;
            }
            else if (depth < 0.65f)
            {
                // Damp positive vertical desired speed

                float verticalDesiredSpeed = Vector3.Dot(desiredVelocity, worldUp);
                
                desiredVelocity =
                    Vector3.ProjectOnPlane(desiredVelocity, worldUp) + worldUp * Mathf.Min(0.1f, verticalDesiredSpeed);
            }
            
            // Using root motion...

            if (useRootMotion && rootMotionController)
            {
                // Preserve current vertical velocity as we want to keep the effect of gravity

                Vector3 verticalVelocity = Vector3.Project(newVelocity, worldUp);

                // Updates new velocity

                newVelocity = Vector3.ProjectOnPlane(desiredVelocity, worldUp) + verticalVelocity;
            }
            else
            {
                // Actual friction

                float actualFriction = IsInWaterPhysicsVolume()
                    ? physicsVolume.friction * depth
                    : swimmingFriction * depth;

                newVelocity = CalcVelocity(newVelocity, desiredVelocity, actualFriction, true, deltaTime);
            }

            // If swimming freely, apply gravity acceleration scaled by (1.0f - actualBuoyancy)

            newVelocity += gravity * ((1.0f - actualBuoyancy) * deltaTime);

            // Update velocity

            characterMovement.velocity = newVelocity;
        }
        
        /// <summary>
        /// User-defined custom movement mode, including many possible sub-modes.
        /// Called if MovementMode is set to Custom.
        /// </summary>

        protected virtual void CustomMovementMode(float deltaTime)
        {
            // Trigger CustomMovementModeUpdate event
            
            OnCustomMovementMode(deltaTime);
        }
        
        /// <summary>
        /// Returns the Character's current rotation mode.
        /// </summary>

        public RotationMode GetRotationMode()
        {
            return _rotationMode;
        }

        /// <summary>
        /// Sets the Character's current rotation mode:
        ///     -None:                          Disable rotation.
        ///     -OrientRotationToMovement:      Rotate the Character toward the direction of acceleration, using rotationRate as the rate of rotation change.
        ///     -OrientRotationToViewDirection: Smoothly rotate the Character toward camera's view direction, using rotationRate as the rate of rotation change.
        ///     -OrientWithRootMotion:          Let root motion handle Character rotation.
        ///     -Custom:                        User-defined custom rotation mode.
        /// </summary>

        public void SetRotationMode(RotationMode rotationMode)
        {
            _rotationMode = rotationMode;
        }
        
        /// <summary>
        /// Updates the Character's rotation based on its current RotationMode.
        /// </summary>

        protected virtual void UpdateRotation(float deltaTime)
        {
            if (_rotationMode == RotationMode.None)
            {
                // Do nothing
            }
            else if (_rotationMode == RotationMode.OrientRotationToMovement)
            {
                // Determines if rotation should modify character's yaw only
            
                bool shouldRemainVertical = IsWalking() || IsFalling();
                
                // Smoothly rotate the Character toward the movement direction, using rotationRate as the rate of rotation change
                
                RotateTowards(_movementDirection, deltaTime, shouldRemainVertical);
            }
            else if (_rotationMode == RotationMode.OrientRotationToViewDirection && camera != null)
            {
                // Determines if rotation should modify character's yaw only
            
                bool shouldRemainVertical = IsWalking() || IsFalling();
                
                // Smoothly rotate the Character toward camera's view direction, using rotationRate as the rate of rotation change
                
                RotateTowards(cameraTransform.forward, deltaTime, shouldRemainVertical);
            }
            else if (_rotationMode == RotationMode.OrientWithRootMotion)
            {
                // Let root motion handle Character rotation
                
                RotateWithRootMotion();
            }
            else if (_rotationMode == RotationMode.Custom)
            {
                CustomRotationMode(deltaTime);
            }
        }
        
        /// <summary>
        /// User-defined custom rotation mode.
        /// Called if RotationMode is set to Custom.
        /// </summary>
        
        protected virtual void CustomRotationMode(float deltaTime)
        {
            // Trigger CustomRotationModeUpdated event
            
            OnCustomRotationMode(deltaTime);
        }
        
        private void BeforeSimulationUpdate(float deltaTime)
        {
            // Toggle walking / falling mode based on CharacterMovement ground status
            
            if (IsWalking() && !IsGrounded())
                SetMovementMode(MovementMode.Falling);
            
            if (IsFalling() && IsGrounded())
                SetMovementMode(MovementMode.Walking);
            
            // Update active physics volume
            
            UpdatePhysicsVolumes();
            
            // Handle crouch / un-crouch

            CheckCrouchInput();
            
            // Handle jump
            
            CheckJumpInput();
            UpdateJumpTimers(deltaTime);
            
            // Trigger BeforeSimulationUpdated event
            
            OnBeforeSimulationUpdate(deltaTime);
        }

        private void SimulationUpdate(float deltaTime)
        {
            // Calculate desired velocity for current movement mode

            CalcDesiredVelocity(deltaTime);
            
            // Update current movement mode
            
            switch (_movementMode)
            {
                case MovementMode.None:
                    break;

                case MovementMode.Walking:
                    WalkingMovementMode(deltaTime);
                    break;

                case MovementMode.Falling:
                    FallingMovementMode(deltaTime);
                    break;

                case MovementMode.Flying:
                    FlyingMovementMode(deltaTime);
                    break;

                case MovementMode.Swimming:
                    SwimmingMovementMode(deltaTime);
                    break;

                case MovementMode.Custom:
                    CustomMovementMode(deltaTime);
                    break;
            }
            
            // Update rotation

            UpdateRotation(deltaTime);
            
            // Append input rotation (eg: AddYawInput, etc)

            ConsumeRotationInput();
        }
        
        private void AfterSimulationUpdate(float deltaTime)
        {
            // If requested, check apex reached and trigger corresponding event
            
            NotifyJumpApex();
            
            // Trigger AfterSimulationUpdated event
            
            OnAfterSimulationUpdate(deltaTime);
        }

        private void CharacterMovementUpdate(float deltaTime)
        {
            // Perform movement
            
            characterMovement.Move(deltaTime);
            
            // Trigger CharacterMovementUpdated event
            
            OnCharacterMovementUpdated(deltaTime);
            
            // If not using root motion, flush root motion accumulated deltas.
            // This prevents accumulation while character is toggling root motion.

            if (!useRootMotion && rootMotionController)
                rootMotionController.FlushAccumulatedDeltas();
        }
        
        /// <summary>
        /// Perform this character simulation, ie: update velocity, position, rotation, etc.
        /// Automatically called when enableAutoSimulation is true.
        /// </summary>
        
        public void Simulate(float deltaTime)
        {
            if (isPaused)
                return;
            
            BeforeSimulationUpdate(deltaTime);
            SimulationUpdate(deltaTime);
            AfterSimulationUpdate(deltaTime);
            CharacterMovementUpdate(deltaTime);
        }
        
        /// <summary>
        /// If enableAutoSimulation is true, perform this character simulation.
        /// </summary>
        
        private void OnLateFixedUpdate()
        {
            // Simulate this character
            
            Simulate(Time.deltaTime);
        }
        
        /// <summary>
        /// Is the Character currently paused?
        /// </summary>

        public bool IsPaused()
        {
            return isPaused;
        }
        
        /// <summary>
        /// Pause / Resume Character.
        /// When paused, a character prevents any interaction (no movement, no rotation, no collisions, etc.)
        ///  If clearState is true, will clear any pending movement, forces and rotations.
        /// </summary>
        
        public void Pause(bool pause, bool clearState = true)
        {
            isPaused = pause;
            characterMovement.collider.enabled = !isPaused;
            
            if (clearState)
            {
                _movementDirection = Vector3.zero;
                _rotationInput = Vector3.zero;
                
                characterMovement.velocity = Vector3.zero;
                characterMovement.ClearAccumulatedForces();
            }
        }
        
        #endregion

        #region MONOBEHAVIOUR
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Reset()
        {
            _rotationMode = RotationMode.OrientRotationToMovement;
            _rotationRate = 540.0f;

            _startingMovementMode = MovementMode.Walking;

            _maxWalkSpeed = 5.0f;
            _minAnalogWalkSpeed = 0.0f;
            _maxAcceleration = 20.0f;
            _brakingDecelerationWalking = 20.0f;
            _groundFriction = 8.0f;
            
            _canEverCrouch = true;
            _crouchedHeight = 1.25f;
            _unCrouchedHeight = 2.0f;
            _maxWalkSpeedCrouched = 3.0f;

            _maxFallSpeed = 40.0f;
            _brakingDecelerationFalling = 0.0f;
            _fallingLateralFriction = 0.3f;
            _airControl = 0.3f;
            
            _canEverJump = true;
            _canJumpWhileCrouching = true;
            _jumpMaxCount = 1;
            _jumpImpulse = 5.0f;
            _jumpMaxHoldTime = 0.0f;
            _jumpMaxPreGroundedTime = 0.0f;
            _jumpMaxPostGroundedTime = 0.0f;

            _maxFlySpeed = 10.0f;
            _brakingDecelerationFlying = 0.0f;
            _flyingFriction = 1.0f;

            _maxSwimSpeed = 3.0f;
            _brakingDecelerationSwimming = 0.0f;
            _swimmingFriction = 0.0f;
            _buoyancy = 1.0f;

            _gravity = new Vector3(0.0f, -9.81f, 0.0f);
            _gravityScale = 1.0f;
            
            _useRootMotion = false;
            
            _impartPlatformVelocity = false;
            _impartPlatformMovement = false;
            _impartPlatformRotation = false;

            _enablePhysicsInteraction = false;
            _applyPushForceToCharacters = false;
            _applyStandingDownwardForce = false;
            
            _mass = 1.0f;
            _pushForceScale = 1.0f;
            _standingDownwardForceScale = 1.0f;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnValidate()
        {
            rotationRate = _rotationRate;
            
            maxWalkSpeed = _maxWalkSpeed;
            minAnalogWalkSpeed = _minAnalogWalkSpeed;
            maxAcceleration = _maxAcceleration;
            brakingDecelerationWalking = _brakingDecelerationWalking;
            groundFriction = _groundFriction;
            
            crouchedHeight = _crouchedHeight;
            unCrouchedHeight = _unCrouchedHeight;
            maxWalkSpeedCrouched = _maxWalkSpeedCrouched;

            maxFallSpeed = _maxFallSpeed;
            brakingDecelerationFalling = _brakingDecelerationFalling;
            fallingLateralFriction = _fallingLateralFriction;
            airControl = _airControl;
            
            jumpMaxCount = _jumpMaxCount;
            jumpImpulse = _jumpImpulse;
            jumpMaxHoldTime = _jumpMaxHoldTime;
            jumpMaxPreGroundedTime = _jumpMaxPreGroundedTime;
            jumpMaxPostGroundedTime = _jumpMaxPostGroundedTime;

            maxFlySpeed = _maxFlySpeed;
            brakingDecelerationFlying = _brakingDecelerationFlying;
            flyingFriction = _flyingFriction;

            maxSwimSpeed = _maxSwimSpeed;
            brakingDecelerationSwimming = _brakingDecelerationSwimming;
            swimmingFriction = _swimmingFriction;
            buoyancy = _buoyancy;

            gravityScale = _gravityScale;
            
            useRootMotion = _useRootMotion;

            if (_characterMovement == null)
                _characterMovement = GetComponent<CharacterMovement>();
            
            impartPlatformVelocity = _impartPlatformVelocity;
            impartPlatformMovement = _impartPlatformMovement;
            impartPlatformRotation = _impartPlatformRotation;

            enablePhysicsInteraction = _enablePhysicsInteraction;
            applyPushForceToCharacters = _applyPushForceToCharacters;
            applyPushForceToCharacters = _applyPushForceToCharacters;
            
            mass = _mass;
            pushForceScale = _pushForceScale;
            standingDownwardForceScale = _standingDownwardForceScale;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Awake()
        {
            // Cache components

            CacheComponents();
            
            // Set starting movement mode

            SetMovementMode(_startingMovementMode);
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnEnable()
        {
            // Subscribe to CharacterMovement events

            characterMovement.Collided += OnCollided;
            characterMovement.FoundGround += OnFoundGround;
            
            // If enabled, start LateFixedUpdate coroutine to perform auto simulation

            if (_enableAutoSimulation)
                EnableAutoSimulationCoroutine(true);
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnDisable()
        {
            // Unsubscribe from CharacterMovement events
            
            characterMovement.Collided -= OnCollided;
            characterMovement.FoundGround -= OnFoundGround;
            
            // If enabled, stops LateFixedUpdate coroutine to disable auto simulation

            if (_enableAutoSimulation)
                EnableAutoSimulationCoroutine(false);
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Start()
        {
            // Force a ground check to update CM ground state,
            // Otherwise character will change to falling, due characterMovement updating its ground state until next Move call.
            
            if (_startingMovementMode == MovementMode.Walking)
            {
                characterMovement.SetPosition(transform.position, true);
            }
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnTriggerEnter(Collider other)
        {
            AddPhysicsVolume(other);
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnTriggerExit(Collider other)
        {
            RemovePhysicsVolume(other);
        }
        
        /// <summary>
        /// If enableAutoSimulation is true, this coroutine is used to perform character simulation.
        /// </summary>
        
        private IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return waitTime;

                OnLateFixedUpdate();
            }
        }
        
        #endregion
    }
}