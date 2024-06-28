using UnityEngine;

namespace ECM2.Examples.Glide
{
    public class PlayerController : MonoBehaviour
    {
        private Character _character;
        private GlideAbility _glideAbility;

        private void Awake()
        {
            _character = GetComponent<Character>();
            _glideAbility = GetComponent<GlideAbility>();
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
            
            if (_character.camera)
            {
                // Make movement direction relative to its camera view direction
                
                movementDirection = movementDirection.relativeTo(_character.cameraTransform);
            }

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
            
            // Glide input
            
            if (Input.GetButtonDown("Jump"))
                _glideAbility.Glide();
            else if (Input.GetButtonUp("Jump"))
                _glideAbility.StopGliding();
        }
    }
}