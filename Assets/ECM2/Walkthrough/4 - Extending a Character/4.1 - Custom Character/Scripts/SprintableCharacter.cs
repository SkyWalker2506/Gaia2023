using UnityEngine;

namespace ECM2.Walkthrough.Ex41
{
    /// <summary>
    /// This example shows how to extend a Character (through inheritance) to perform a sprint ability.
    /// This uses one of the new methods (introduced in v1.4) OnBeforeSimulationUpdate,
    /// to easily modify the character's state within Character's simulation loop. 
    /// </summary>
    
    public class SprintableCharacter : Character
    {
        [Space(15.0f)]
        public float maxSprintSpeed = 10.0f;
        
        private bool _isSprinting;
        private bool _sprintInputPressed;
        
        /// <summary>
        /// Request the character to start to sprint. 
        /// </summary>

        public void Sprint()
        {
            _sprintInputPressed = true;
        }
        
        /// <summary>
        /// Request the character to stop sprinting. 
        /// </summary>

        public void StopSprinting()
        {
            _sprintInputPressed = false;
        }

        public bool IsSprinting()
        {
            return _isSprinting;
        }
        
        /// <summary>
        /// Determines if the character is able to sprint in its current state.
        /// </summary>

        private bool CanSprint()
        {
            // A character can only sprint if:
            // A character is in its walking movement mode and not crouched
            
            return IsWalking() && !IsCrouched();
        }
        
        /// <summary>
        /// Start / stops a requested sprint.
        /// </summary>

        private void CheckSprintInput()
        {
            if (!_isSprinting && _sprintInputPressed && CanSprint())
            {
                _isSprinting = true;
            }
            else if (_isSprinting && (!_sprintInputPressed || !CanSprint()))
            {
                _isSprinting = false;
            }
        }
        
        /// <summary>
        /// Override GetMaxSpeed method to return maxSprintSpeed while sprinting.
        /// </summary>
        
        public override float GetMaxSpeed()
        {
            return _isSprinting ? maxSprintSpeed : base.GetMaxSpeed();
        }

        protected override void OnBeforeSimulationUpdate(float deltaTime)
        {
            // Call base method implementation
            
            base.OnBeforeSimulationUpdate(deltaTime);
            
            // Handle sprint
            
            CheckSprintInput();
        }
        
        private void Update()
        {
            // Movement input
            
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            Vector3 movementDirection =  Vector3.zero;

            movementDirection += Vector3.right * inputMove.x;
            movementDirection += Vector3.forward * inputMove.y;
            
            // If character has a camera assigned...
            
            if (camera)
            {
                // Make movement direction relative to its camera view direction
                
                movementDirection = movementDirection.relativeTo(cameraTransform);
            }

            SetMovementDirection(movementDirection);
            
            // Crouch input
            
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                UnCrouch();
            
            // Jump input
            
            if (Input.GetButtonDown("Jump"))
                Jump();
            else if (Input.GetButtonUp("Jump"))
                StopJumping();
            
            // SPRINT input
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
                Sprint();
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                StopSprinting();
        }
    }
}