using UnityEngine;

namespace ECM2.Examples.Jump
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) implementing a jump ability.
    /// This is the exact jump available in the Character class before v1.4.
    /// </summary>
    
    public class JumpAbility : MonoBehaviour
    {
        [Space(15f)]
        [Tooltip("Is the character able to jump ?")]
        [SerializeField]
        private bool _canEverJump;

        [Tooltip("Can jump while crouching ?")]
        [SerializeField]
        private bool _jumpWhileCrouching;

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
        private float _jumpPreGroundedTime;

        [Tooltip("How long after leaving the ground you can trigger a jump (in seconds).")]
        [SerializeField]
        private float _jumpPostGroundedTime;

        private Character _character;
        
        protected bool _jumpButtonPressed;
        protected float _jumpButtonHeldDownTime;
        protected float _jumpHoldTime;
        protected int _jumpCount;
        protected bool _isJumping;
        
        /// <summary>
        /// Is the character able to jump ?
        /// </summary>

        public bool canEverJump
        {
            get => _canEverJump;
            set => _canEverJump = value;
        }

        /// <summary>
        /// Can jump while crouching ?
        /// </summary>

        public bool jumpWhileCrouching
        {
            get => _jumpWhileCrouching;
            set => _jumpWhileCrouching = value;
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

        public float jumpPreGroundedTime
        {
            get => _jumpPreGroundedTime;
            set => _jumpPreGroundedTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// How long after leaving the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpPostGroundedTime
        {
            get => _jumpPostGroundedTime;
            set => _jumpPostGroundedTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// True is _jumpButtonPressed is true, false otherwise.
        /// </summary>

        public bool jumpButtonPressed => _jumpButtonPressed;

        /// <summary>
        /// This is the time (in seconds) that the player has held the jump button.
        /// </summary>

        public float jumpButtonHeldDownTime => _jumpButtonHeldDownTime;

        /// <summary>
        /// Tracks the current number of jumps performed.
        /// </summary>

        public int jumpCount => _jumpCount;

        /// <summary>
        /// This is the time (in seconds) that the player has been holding the jump.
        /// Eg: Variable height jump.
        /// </summary>

        public float jumpHoldTime => _jumpHoldTime;
        
         /// <summary>
        /// Is the Character jumping ?
        /// </summary>

        public virtual bool IsJumping()
        {
            return _isJumping;
        }

        /// <summary>
        /// Start a jump.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Jump()
        {
            _jumpButtonPressed = true;
        }

        /// <summary>
        /// Stop the Character from jumping.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopJumping()
        {
            // Input state

            _jumpButtonPressed = false;
            _jumpButtonHeldDownTime = 0.0f;

            // Jump holding state

            _isJumping = false;
            _jumpHoldTime = 0.0f;
        }

        /// <summary>
        /// Returns the current jump count.
        /// </summary>

        public virtual int GetJumpCount()
        {
            return _jumpCount;
        }

        /// <summary>
        /// Determines if the Character is able to perform the requested jump.
        /// </summary>

        public virtual bool CanJump()
        {
            // Is character even able to jump ?

            if (!canEverJump)
                return false;

            // Can jump while crouching ?

            if (_character.IsCrouched() && !jumpWhileCrouching)
                return false;

            // Cant jump if no jumps available

            if (jumpMaxCount == 0 || _jumpCount >= jumpMaxCount)
                return false;

            // Is fist jump ?

            if (_jumpCount == 0)
            {
                // On first jump,
                // can jump if is walking or is falling BUT withing post grounded time

                bool canJump = _character.IsWalking() ||
                               _character.IsFalling() && jumpPostGroundedTime > 0.0f && _character.fallingTime < jumpPostGroundedTime;

                // Missed post grounded time ?

                if (_character.IsFalling() && !canJump)
                {
                    // Missed post grounded time,
                    // can jump if have any 'in-air' jumps but the first jump counts as the in-air jump

                    canJump = jumpMaxCount > 1;
                    if (canJump)
                        _jumpCount++;
                }

                return canJump;
            }

            // In air jump conditions...

            return _character.IsFalling();
        }

        /// <summary>
        /// Determines the jump impulse vector.
        /// </summary>

        protected virtual Vector3 CalcJumpImpulse()
        {
            Vector3 worldUp = -_character.GetGravityDirection();

            float verticalSpeed = Vector3.Dot(_character.GetVelocity(), worldUp);
            float actualJumpImpulse = Mathf.Max(verticalSpeed, jumpImpulse);

            return worldUp * actualJumpImpulse;
        }

        /// <summary>
        /// Attempts to perform a requested jump.
        /// </summary>

        protected virtual void DoJump(float deltaTime)
        {
            // Update held down timer

            if (_jumpButtonPressed)
                _jumpButtonHeldDownTime += deltaTime;

            // Wants to jump and not already jumping..

            if (_jumpButtonPressed && !IsJumping())
            {
                // If jumpPreGroundedTime is enabled,
                // allow to jump only if held down time is less than tolerance

                if (jumpPreGroundedTime > 0.0f)
                {
                    bool canJump = _jumpButtonHeldDownTime <= jumpPreGroundedTime;
                    if (!canJump)
                        return;
                }

                // Can perform the requested jump ?

                if (CanJump())
                {
                    // Jump!

                    _character.SetMovementMode(Character.MovementMode.Falling);

                    _character.PauseGroundConstraint();
                    _character.LaunchCharacter(CalcJumpImpulse(), true);

                    _jumpCount++;
                    _isJumping = true;
                }
            }
        }

        /// <summary>
        /// Handle jumping state.
        /// Eg: check input, perform jump hold (jumpMaxHoldTime > 0), etc. 
        /// </summary>

        protected virtual void Jumping(float deltaTime)
        {
            // Is character allowed to jump ?

            if (!canEverJump)
            {
                // If not allowed but was jumping, stop jump

                if (IsJumping())
                    StopJumping();

                return;
            }

            // Check jump input state and attempts to do the requested jump

            DoJump(deltaTime);

            // Perform jump hold, applies an opposite gravity force proportional to _jumpHoldTime.

            if (IsJumping() && _jumpButtonPressed && jumpMaxHoldTime > 0.0f && _jumpHoldTime < jumpMaxHoldTime)
            {
                Vector3 actualGravity = _character.GetGravityVector();

                float actualGravityMagnitude = actualGravity.magnitude;
                Vector3 actualGravityDirection = actualGravityMagnitude > 0.0f
                    ? actualGravity / actualGravityMagnitude
                    : Vector3.zero;

                float jumpProgress = Mathf.InverseLerp(0.0f, jumpMaxHoldTime, _jumpHoldTime);
                float proportionalForce = Mathf.LerpUnclamped(actualGravityMagnitude, 0.0f, jumpProgress);

                Vector3 proportionalJumpForce = -actualGravityDirection * proportionalForce;
                _character.AddForce(proportionalJumpForce);

                _jumpHoldTime += deltaTime;
            }
        }

        protected virtual void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMode)
        {
            if (_character.IsWalking())
                _jumpCount = 0;
            else if (_character.IsFlying() || _character.IsSwimming())
                StopJumping();
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Reset()
        {
            _canEverJump = true;
            _jumpWhileCrouching = true;
            _jumpMaxCount = 1;
            _jumpImpulse = 5.0f;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>
        protected virtual void OnValidate()
        {
            jumpMaxCount = _jumpMaxCount;
            jumpImpulse = _jumpImpulse;
            jumpMaxHoldTime = _jumpMaxHoldTime;
            jumpPreGroundedTime = _jumpPreGroundedTime;
            jumpPostGroundedTime = _jumpPostGroundedTime;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>
        
        protected virtual void Awake()
        {
            _character = GetComponent<Character>();
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnEnable()
        {
            _character.MovementModeChanged += OnMovementModeChanged;
            _character.BeforeSimulationUpdated += Jumping;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>
        
        protected virtual void OnDisable()
        {
            _character.BeforeSimulationUpdated -= Jumping;
            _character.MovementModeChanged -= OnMovementModeChanged;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Start()
        {
            // Disable Character built-in jump
            
            _character.canEverJump = false;
        }
    }
}