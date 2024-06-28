using UnityEngine;

namespace ECM2.Examples.FirstPersonFly
{
    /// <summary>
    /// Regular First Person Character Input. Shows how to handle movement while flying.
    /// In this case, we allow to fly towards our view direction, allowing to freely move through the air. 
    /// </summary>
    
    public class CharacterInput : MonoBehaviour
    {
        private Character _character;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void Update()
        {
            Vector2 inputMove = new Vector2()
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };
            
            Vector3 movementDirection =  Vector3.zero;

            if (_character.IsFlying())
            {
                // Strafe
                
                movementDirection += _character.GetRightVector() * inputMove.x;
                
                // Forward, along camera view direction (if any) or along character's forward if camera not found 

                Vector3 forward =
                    _character.camera ? _character.cameraTransform.forward : _character.GetForwardVector();

                movementDirection += forward * inputMove.y;
                
                // Vertical movement

                if (_character.jumpInputPressed)
                    movementDirection += Vector3.up;
            }
            else
            {
                // Regular First Person movement relative to character's view direction
                
                movementDirection += _character.GetRightVector() * inputMove.x;
                movementDirection += _character.GetForwardVector() * inputMove.y;
            }

            _character.SetMovementDirection(movementDirection);
            
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
                _character.Crouch();
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
                _character.UnCrouch();
            
            if (Input.GetButtonDown("Jump"))
                _character.Jump();
            else if (Input.GetButtonUp("Jump"))
                _character.StopJumping();
        }
    }
}
