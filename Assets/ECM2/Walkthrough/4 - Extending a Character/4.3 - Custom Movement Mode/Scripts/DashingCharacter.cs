using UnityEngine;

namespace ECM2.Walkthrough.Ex43
{
    /// <summary>
    /// This example shows how to extend a Character (through inheritance) adding a custom movement mode;
    /// in this case, a Dash mechanic.
    /// </summary>
    
    public class DashingCharacter : Character
    {
        #region ENUMS

        public enum ECustomMovementMode
        {
            None,
            Dashing
        }

        #endregion

        #region EDITOR EXPOSED FIELDS

        [Space(15.0f)]
        [Tooltip("Is the character able to Dash?")]
        public bool canEverDash = true;
        
        [Tooltip("Dash initial impulse.")]
        public float dashImpulse = 20.0f;
        
        [Tooltip("Dash duration in seconds.")]
        public float dashDuration = 0.15f;

        #endregion

        #region FIELDS

        protected float _dashingTime;
        protected bool _dashInputPressed;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Ture if dash input is pressed, false otherwise.
        /// </summary>

        protected bool dashInputPressed => _dashInputPressed;

        #endregion

        #region METHODS

        /// <summary>
        /// Is the character currently dashing?
        /// </summary>
        
        public bool IsDashing()
        {
            return movementMode == MovementMode.Custom && customMovementMode == (int)ECustomMovementMode.Dashing;
        }
        
        /// <summary>
        /// Request to perform a dash.
        /// The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Dash()
        {
            _dashInputPressed = true;
        }
        
        /// <summary>
        /// Request to stop dashing.
        /// The request is processed on the next simulation update.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopDashing()
        {
            _dashInputPressed = false;
        }
        
        /// <summary>
        /// Determines if the Character is able to dash in its current state.
        /// Defaults to Walking or Falling while NOT crouched.
        /// </summary>

        public bool IsDashAllowed()
        {
            if (IsCrouched())
                return false;
            
            return canEverDash && (IsWalking() || IsFalling());
        }
        
        /// <summary>
        /// Perform dash.
        /// </summary>

        protected virtual void DoDash()
        {
            // Apply dash impulse along input direction (if any) or along character's forward

            Vector3 dashDirection = GetMovementDirection();
            if (dashDirection.isZero())
                dashDirection = GetForwardVector();

            Vector3 dashDirection2D = dashDirection.onlyXZ().normalized;

            SetVelocity(dashDirection2D * dashImpulse);
            
            // Change to dashing movement mode
            
            SetMovementMode(MovementMode.Custom, (int)ECustomMovementMode.Dashing);
            
            // Lock rotation towards dashing direction

            if (rotationMode == RotationMode.OrientRotationToMovement)
                SetRotation(Quaternion.LookRotation(dashDirection2D));
        }
        
        /// <summary>
        /// Reset dashing state and exit dashing movement mode.
        /// </summary>

        protected virtual void ResetDashState()
        {
            // Reset dashing state
            
            _dashingTime = 0.0f;
            _dashInputPressed = false;
            
            // Clear dashing impulse
            
            SetVelocity(Vector3.zero);
            
            // Falling is auto-manged state so its safe to use as an exit state.
            
            SetMovementMode(MovementMode.Falling);
        }
        
        /// <summary>
        /// Update dashing movement mode.
        /// </summary>

        protected virtual void DashingMovementMode(float deltaTime)
        {
            // This prevents the character from rotate towards a movement direction
            
            SetMovementDirection(Vector3.zero);
            
            // Update dash timer...
                
            _dashingTime += deltaTime;
            if (_dashingTime >= dashDuration)
            {
                // If completed, exit dash state
                    
                ResetDashState();
            }
        }
        
        protected override void OnBeforeSimulationUpdate(float deltaTime)
        {
            // Call base method implementation
            
            base.OnBeforeSimulationUpdate(deltaTime);
            
            // Attempts to start a requested dash

            if (!IsDashing() && dashInputPressed && IsDashAllowed())
                DoDash();
        }

        protected override void CustomMovementMode(float deltaTime)
        {
            // Call base method implementation
            
            base.CustomMovementMode(deltaTime);
            
            // Update dashing movement mode

            if (customMovementMode == (int)ECustomMovementMode.Dashing)
                DashingMovementMode(deltaTime);
        }

        #endregion
    }
}