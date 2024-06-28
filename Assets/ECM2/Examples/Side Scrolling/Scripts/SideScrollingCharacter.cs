using UnityEngine;

namespace ECM2.Examples.SideScrolling
{
    /// <summary>
    /// This example shows how to implement a typical side-scrolling movement with side to side rotation snap.
    /// </summary>
    
    public class SideScrollingCharacter : Character
    {
        protected override void Awake()
        {
            // Call base method implementation
            
            base.Awake();
            
            // Disable Character rotation, well handle it here (snap move direction)
            
            SetRotationMode(RotationMode.None);
        }

        private void Update()
        {
            // Add horizontal movement (in world space)

            float moveInput = Input.GetAxisRaw("Horizontal");
            SetMovementDirection(Vector3.right * moveInput);
            
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
            
            // Snap side to side rotation

            if (moveInput != 0.0f)
                SetYaw(moveInput * 90.0f);
        }
    }
}

