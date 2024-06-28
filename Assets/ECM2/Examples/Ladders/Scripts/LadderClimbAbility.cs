using UnityEngine;

namespace ECM2.Examples.Ladders
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) adding a custom movement mode.
    /// Here we implement a ladder climbing ability using a Climbing custom movement mode.
    /// </summary>
    
    public class LadderClimbAbility : MonoBehaviour
    {
        public enum CustomMovementMode
        {
            Climbing = 1
        }
        
        public enum ClimbingState
        {
            None,
            Grabbing,
            Grabbed,
            Releasing
        }
        
        public float climbingSpeed = 5.0f;
        public float grabbingTime = 0.25f;

        public LayerMask ladderMask;
        
        private Character _character;
        
        private Ladder _activeLadder;
        private float _ladderPathPosition;

        private Vector3 _ladderStartPosition;
        private Vector3 _ladderTargetPosition;

        private Quaternion _ladderStartRotation;
        private Quaternion _ladderTargetRotation;

        private float _ladderTime;

        private ClimbingState _climbingState;
        
        private Character.RotationMode _previousRotationMode;
        
        /// <summary>
        /// True if the Character is in Climbing custom movement mode, false otherwise.
        /// </summary>
        
        public bool IsClimbing()
        {
            bool isClimbing = _character.movementMode == Character.MovementMode.Custom &&
                              _character.customMovementMode == (int)CustomMovementMode.Climbing;

            return isClimbing;
        }
        
        /// <summary>
        /// Determines if the Character is able to climb.
        /// </summary>

        private bool CanClimb()
        {
            // Do not allow to climb if crouching

            if (_character.IsCrouched())
                return false;

            // Attempt to find a ladder

            CharacterMovement characterMovement = _character.characterMovement;
            var overlappedColliders =
                characterMovement.OverlapTest(ladderMask, QueryTriggerInteraction.Collide, out int overlapCount);

            if (overlapCount == 0)
                return false;

            // Is a ladder ?

            if (!overlappedColliders[0].TryGetComponent(out Ladder ladder))
                return false;

            // Found a ladder, make it active ladder and return
            
            _activeLadder = ladder;

            return true;
        }
        
        /// <summary>
        /// Start a climb.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Climb()
        {
            if (IsClimbing() || !CanClimb())
                return;
            
            _character.SetMovementMode(Character.MovementMode.Custom, (int) CustomMovementMode.Climbing);

            _ladderStartPosition = _character.GetPosition();
            _ladderTargetPosition = _activeLadder.ClosestPointOnPath(_ladderStartPosition, out _ladderPathPosition);

            _ladderStartRotation = _character.GetRotation();
            _ladderTargetRotation = _activeLadder.transform.rotation;
        }
        
        /// <summary>
        /// Stop the Character from climbing.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopClimbing()
        {
            if (!IsClimbing() || _climbingState != ClimbingState.Grabbed)
                return;

            _climbingState = ClimbingState.Releasing;

            _ladderStartPosition = _character.GetPosition();
            _ladderStartRotation = _character.GetRotation();

            _ladderTargetPosition = _ladderStartPosition;
            _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
        }
        
        /// <summary>
        /// Perform climbing movement.
        /// </summary>

        private void ClimbingMovementMode(float deltaTime)
        {
            Vector3 velocity = Vector3.zero;

            switch (_climbingState)
            {
                case ClimbingState.Grabbing:
                case ClimbingState.Releasing:
                {
                    _ladderTime += deltaTime;

                    if (_ladderTime <= grabbingTime)
                    {
                        Vector3 interpolatedPosition = Vector3.Lerp(_ladderStartPosition, _ladderTargetPosition, _ladderTime / grabbingTime);
                    
                        velocity = (interpolatedPosition - transform.position) / deltaTime;
                    }
                    else
                    {
                        // If target has been reached, change ladder phase

                        _ladderTime = 0.0f;

                        if (_climbingState == ClimbingState.Grabbing )
                        {
                            // Switch to ladder climb phase

                            _climbingState = ClimbingState.Grabbed;
                        }
                        else if (_climbingState == ClimbingState.Releasing)
                        {
                            // Exit climbing state (change to falling movement mode)

                            _character.SetMovementMode(Character.MovementMode.Falling);
                        }
                    }

                    break;
                }

                case ClimbingState.Grabbed:
                {
                    // Get the path position from character's current position

                    _activeLadder.ClosestPointOnPath(_character.GetPosition(), out _ladderPathPosition);

                    if (Mathf.Abs(_ladderPathPosition) < 0.05f)
                    {
                        // Move the character along the ladder path

                        Vector3 movementInput = _character.GetMovementDirection();

                        velocity = _activeLadder.transform.up * (movementInput.z * climbingSpeed);
                    }
                    else
                    {
                        // If reached on of the ladder path extremes, change to releasing phase

                        _climbingState = ClimbingState.Releasing;

                        _ladderStartPosition = _character.GetPosition();
                        _ladderStartRotation = _character.GetRotation();

                        if (_ladderPathPosition > 0.0f)
                        {
                            // Above ladder path top point

                            _ladderTargetPosition = _activeLadder.TopPoint.position;
                            _ladderTargetRotation = _activeLadder.TopPoint.rotation;
                        }
                        else if (_ladderPathPosition < 0.0f)
                        {
                            // Below ladder path bottom point

                            _ladderTargetPosition = _activeLadder.BottomPoint.position;
                            _ladderTargetRotation = _activeLadder.BottomPoint.rotation;
                        }
                    }

                    break;
                }
            }

            // Update character's velocity

            _character.SetVelocity(velocity);
        }
        
        private void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMovementMode)
        {
            // Enter Climbing movement mode
            
            if (IsClimbing())
            {
                _climbingState = ClimbingState.Grabbing;
                
                _character.StopJumping();
                
                _character.EnableGroundConstraint(false);

                _previousRotationMode = _character.rotationMode;
                _character.SetRotationMode(Character.RotationMode.Custom);
            }
            
            // Exit Climbing movement mode

            bool wasClimbing = prevMovementMode == Character.MovementMode.Custom &&
                               prevCustomMovementMode == (int)CustomMovementMode.Climbing;

            if (wasClimbing)
            {
                _climbingState = ClimbingState.None;
                
                _character.EnableGroundConstraint(true);
                _character.SetRotationMode(_previousRotationMode);
            }
        }
        
        private void OnCustomMovementModeUpdated(float deltaTime)
        {
            if (IsClimbing())
                ClimbingMovementMode(deltaTime);
        }
        
        private void OnCustomRotationModeUpdated(float deltaTime)
        {
            if (IsClimbing() && (_climbingState == ClimbingState.Grabbing || _climbingState == ClimbingState.Releasing))
            {
                Quaternion rotation =
                    Quaternion.Slerp(_ladderStartRotation, _ladderTargetRotation, _ladderTime / grabbingTime);

                _character.SetRotation(rotation);
            }
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            _character.MovementModeChanged += OnMovementModeChanged;
            _character.CustomMovementModeUpdated += OnCustomMovementModeUpdated;
            _character.CustomRotationModeUpdated += OnCustomRotationModeUpdated;
        }
        
        private void OnDisable()
        {
            _character.MovementModeChanged -= OnMovementModeChanged;
            _character.CustomMovementModeUpdated -= OnCustomMovementModeUpdated;
            _character.CustomRotationModeUpdated -= OnCustomRotationModeUpdated;
        }
    }
}