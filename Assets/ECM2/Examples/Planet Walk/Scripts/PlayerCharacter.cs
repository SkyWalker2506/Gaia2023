using UnityEngine;

namespace ECM2.Examples.PlanetWalk
{
    /// <summary>
    /// This example extends a Character (through inheritance) adjusting its gravity and orientation
    /// to follow a planet curvature similar to the Mario Galaxy game.
    /// </summary>
    
    public class PlayerCharacter : Character
    {
        [Space(15f)]
        public Transform planetTransform;
        
        protected override void UpdateRotation(float deltaTime)
        {
            // Call base method (i.e: rotate towards movement direction)

            base.UpdateRotation(deltaTime);
            
            // Adjust gravity direction (ie: a vector pointing from character position to planet's center)
            
            Vector3 toPlanet = planetTransform.position - GetPosition();
            SetGravityVector(toPlanet.normalized * GetGravityMagnitude());
            
            // Adjust Character's rotation following the new world-up (defined by gravity direction)
            
            Vector3 worldUp = GetGravityDirection() * -1.0f;
            Quaternion newRotation = Quaternion.FromToRotation(GetUpVector(), worldUp) * GetRotation();
            
            SetRotation(newRotation);
        }

        private void Update()
        {
            // Movement direction relative to camera's view direction
            
            Vector2 movementInput = new Vector2
                { x = Input.GetAxisRaw("Horizontal"), y = Input.GetAxisRaw("Vertical") };

            Vector3 movementDirection = Vector3.zero;

            movementDirection += Vector3.right * movementInput.x;
            movementDirection += Vector3.forward * movementInput.y;

            movementDirection = movementDirection.relativeTo(cameraTransform, GetUpVector());

            SetMovementDirection(movementDirection);

            // Jump

            if (Input.GetButton("Jump"))
                Jump();
            else if (Input.GetButtonUp("Jump"))
                StopJumping();
        }
    }
}
