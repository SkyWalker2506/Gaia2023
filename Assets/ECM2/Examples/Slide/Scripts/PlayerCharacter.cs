using UnityEngine;

namespace ECM2.Examples.Slide
{
    /// <summary>
    /// This example extends a Character (through inheritance) implementing a slide mechanic.
    /// </summary>
    
    public class PlayerCharacter : Character
    {
        [Space(15.0f)]
        public float slideImpulse = 20.0f;
        public float slideDownAcceleration = 20.0f;
        
        /// <summary>
        /// Our custom movement mode(s) id(s).
        /// </summary>
        
        enum ECustomMovementMode
        {
            Sliding = 1
        }
        
        /// <summary>
        /// If sliding, return max walk speed as our speed limit.
        /// </summary>

        public override float GetMaxSpeed()
        {
            return IsSliding() ? maxWalkSpeed : base.GetMaxSpeed();
        }
        
        /// <summary>
        /// If sliding, limit acceleration.
        /// </summary>

        public override float GetMaxAcceleration()
        {
            return IsSliding() ? maxAcceleration * 0.1f : base.GetMaxAcceleration();
        }

        /// <summary>
        /// Override IsWalking (aka: grounded movement mode) to add Sliding movement mode support,
        /// otherwise crouch, jump will fail while sliding due its conditions checking if IsWalking.
        /// </summary>
        
        public override bool IsWalking()
        {
            return IsSliding() || base.IsWalking();
        }
        
        /// <summary>
        /// Is the Character sliding?
        /// </summary>
        
        public bool IsSliding()
        {
            return movementMode == MovementMode.Custom && customMovementMode == (int)ECustomMovementMode.Sliding;
        }
        
        /// <summary>
        /// Determines if the character can slide in its current state.
        /// </summary>

        protected virtual bool CanSlide()
        {
            // Slide is tied to crouch, if not crouching cant slide!
            
            if (!IsGrounded())
                return false;
            
            // Check allowed slide speed threshold 
            
            float sqrSpeed = velocity.sqrMagnitude;
            float slideSpeedThreshold = maxWalkSpeedCrouched * maxWalkSpeedCrouched;

            return sqrSpeed >= slideSpeedThreshold * 1.02f;
        }
        
        /// <summary>
        /// Calculate sliding direction vector.
        /// </summary>
        
        protected virtual Vector3 CalcSlideDirection()
        {
            Vector3 slideDirection = GetMovementDirection();
            if (slideDirection.isZero())
                slideDirection = GetVelocity();
            else if (slideDirection.isZero())
                slideDirection = GetForwardVector();
            
            slideDirection = ConstrainInputVector(slideDirection);
            
            return slideDirection.normalized;
        }
        
        /// <summary>
        /// Attempts to perform a requested slide or
        /// stop it if requested or cant continue sliding.
        /// </summary>

        protected virtual void CheckSlideInput()
        {
            bool isSliding = IsSliding();
            bool wantsToSlide = crouchInputPressed;

            if (!isSliding && wantsToSlide && CanSlide())
            {
                SetMovementMode(MovementMode.Custom, (int)ECustomMovementMode.Sliding);
            }
            else if (isSliding && (!wantsToSlide || !CanSlide()))
            {
                SetMovementMode(MovementMode.Walking);
            }
        }
        
        /// <summary>
        /// Handle Sliding enter / exit.
        /// </summary>

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Call base method implementation
            
            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);
            
            // Enter sliding movement mode...

            if (IsSliding())
            {
                // Apply initial slide impulse
                
                Vector3 slideDirection = CalcSlideDirection();
                characterMovement.velocity += slideDirection * slideImpulse;
                
                // Disable Character rotation
                
                SetRotationMode(RotationMode.None);
            }
            
            // Exit sliding movement mode...

            bool wasSliding = prevMovementMode == MovementMode.Custom &&
                              prevCustomMode == (int)ECustomMovementMode.Sliding;
            
            if (wasSliding)
            {
                // Re-enable Character rotation
                
                SetRotationMode(RotationMode.OrientRotationToMovement);
                
                // If falling, make sure its velocity do not exceed maxWalkSpeed

                if (IsFalling())
                {
                    Vector3 worldUp = -GetGravityDirection();
                    Vector3 verticalVelocity = Vector3.Project(velocity, worldUp);
                    Vector3 lateralVelocity = Vector3.ClampMagnitude(velocity - verticalVelocity, maxWalkSpeed);

                    characterMovement.velocity = lateralVelocity + verticalVelocity;
                }
            }
        }

        protected override void OnBeforeSimulationUpdate(float deltaTime)
        {
            // Call base method implementation
            
            base.OnBeforeSimulationUpdate(deltaTime);
            
            // Attempts to do a requested slide

            CheckSlideInput();
        }
        
        /// <summary>
        /// Update Character's velocity while SLIDING on walkable surfaces.
        /// </summary>

        protected virtual void SlidingMovementMode(float deltaTime)
        {
            // Limit input to lateral movement only (strafing)

            Vector3 desiredVelocity = Vector3.Project(GetDesiredVelocity(), GetRightVector());
            
            // Calculate new velocity

            characterMovement.velocity =
                CalcVelocity(characterMovement.velocity, desiredVelocity, groundFriction * 0.2f, false, deltaTime);
            
            // Apply slide down acceleration
            
            Vector3 slideDownDirection =
                Vector3.ProjectOnPlane(GetGravityDirection(), characterMovement.groundNormal).normalized;

            characterMovement.velocity += slideDownAcceleration * deltaTime * slideDownDirection;
            
            // Apply downwards force

            if (applyStandingDownwardForce)
                ApplyDownwardsForce();
        }

        protected override void CustomMovementMode(float deltaTime)
        {
            // Call base method implementation
            
            base.CustomMovementMode(deltaTime);
            
            // Sliding custom movement mode

            if (customMovementMode == (int)ECustomMovementMode.Sliding)
                SlidingMovementMode(deltaTime);
        }
    }
}