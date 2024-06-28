using UnityEngine;

namespace ECM2.Examples.TwinStickMovement
{
    /// <summary>
    /// This example shows how to extend a Character (through inheritance), adding a custom rotation mode;
    /// in this case, implements a typical Mouse and Keyboard twin-stick shooter control.
    /// </summary>
    
    public class TwinStickCharacter : Character
    {
        private Vector3 _aimDirection;
        
        /// <summary>
        /// Returns the current aim direction vector.
        /// </summary>

        public virtual Vector3 GetAimDirection()
        {
            return _aimDirection;
        }
        
        /// <summary>
        /// Sets the desired aim direction vector (in world space).
        /// </summary>

        public virtual void SetAimDirection(Vector3 worldDirection)
        {
            _aimDirection = worldDirection;
        }
        
        /// <summary>
        /// Use a custom rotation mode to rotate towards aim direction (if shooting)
        /// or towards movement direction if not.
        /// </summary>
        /// <param name="deltaTime">The simulation delta time</param>

        protected override void CustomRotationMode(float deltaTime)
        {
            // Call base method implementation
            
            base.CustomRotationMode(deltaTime);
            
            // Update character rotation

            Vector3 targetDirection = 
                _aimDirection.isZero() ? GetMovementDirection() : GetAimDirection();

            RotateTowards(targetDirection, deltaTime);
        }

        private void Update()
        {
            // Movement input
            
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            // Movement direction in world space
            
            Vector3 movementDirection =  Vector3.zero;

            movementDirection += Vector3.right * inputMove.x;
            movementDirection += Vector3.forward * inputMove.y;
            
            // If character has a camera assigned...
            
            if (camera)
            {
                // Make movement direction relative to its camera view direction
                
                movementDirection = movementDirection.relativeTo(cameraTransform);
            }
            
            // Set Character's movement direction

            SetMovementDirection(movementDirection);
            
            // Calc aim direction

            Vector3 aimDirection = Vector3.zero;

            if (Input.GetMouseButton(0))
            {
                // Convert mouse screen position to world position
                
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitResult, Mathf.Infinity))
                {
                    // Compute aim direction vector (character direction -> mouse world position)
                    
                    Vector3 toHitPoint2D = (hitResult.point - GetPosition()).onlyXZ();
                    aimDirection = toHitPoint2D.normalized;
                }
            }
            
            // Set Character's aim direction
            
            SetAimDirection(aimDirection);
        }
    }
}
