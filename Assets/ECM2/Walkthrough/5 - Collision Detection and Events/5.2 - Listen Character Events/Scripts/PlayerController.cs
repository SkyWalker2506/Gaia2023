using UnityEngine;

namespace ECM2.Walkthrough.Ex52
{
    /// <summary>
    /// This example shows how to listen to Character events when extending a Character through composition.
    /// </summary>
    
    public class PlayerController : MonoBehaviour
    {
        // The controlled Character
        
        private Character _character;
        
        protected void OnCollided(ref CollisionResult collisionResult)
        {
            Debug.Log($"Collided with {collisionResult.collider.name}");
        }

        protected void OnFoundGround(ref FindGroundResult foundGround)
        {
            Debug.Log($"Found {foundGround.collider.name} ground");
        }

        protected void OnLanded(Vector3 landingVelocity)
        {
            Debug.Log($"Landed with {landingVelocity:F4} landing velocity.");
        }

        protected void OnCrouched()
        {
            Debug.Log("Crouched");
        }

        protected void OnUnCrouched()
        {
            Debug.Log("UnCrouched");
        }

        protected void OnJumped()
        {
            Debug.Log("Jumped!");
            
            // Enable apex notification event
            
            _character.notifyJumpApex = true;
        }

        protected void OnReachedJumpApex()
        {
            Debug.Log($"Apex reached {_character.GetVelocity():F4}");
        }

        private void Awake()
        {
            // Cache controlled character
            
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            // Subscribe to Character events
            
            _character.Collided += OnCollided;
            _character.FoundGround += OnFoundGround;
            _character.Landed += OnLanded;
            _character.Crouched += OnCrouched;
            _character.UnCrouched += OnUnCrouched;
            _character.Jumped += OnJumped;
            _character.ReachedJumpApex += OnReachedJumpApex;
        }

        private void OnDisable()
        {
            // Un-subscribe from Character events
            
            _character.Collided -= OnCollided;
            _character.FoundGround -= OnFoundGround;
            _character.Landed -= OnLanded;
            _character.Crouched -= OnCrouched;
            _character.UnCrouched -= OnUnCrouched;
            _character.Jumped -= OnJumped;
            _character.ReachedJumpApex -= OnReachedJumpApex;
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
        }
    }
}