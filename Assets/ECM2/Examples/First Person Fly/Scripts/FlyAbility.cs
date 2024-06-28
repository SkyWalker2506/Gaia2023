using UnityEngine;

namespace ECM2.Examples.FirstPersonFly
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) and use
    /// its Flying movement mode to implement a fly ability.
    ///
    /// Flying movement mode needs to be manually enabled / disabled as needed.
    /// </summary>
    
    public class FlyAbility : MonoBehaviour
    {
        public bool canEverFly = true;
        
        private Character _character;
        
        /// <summary>
        /// Determines if the Character is able to fly in its current state.
        /// </summary>

        private bool IsFlyAllowed()
        {
            return canEverFly && _character.IsFalling();
        }
        
        /// <summary>
        /// Determines if the character should enter flying movement mode.
        /// </summary>

        protected virtual bool CanFly()
        {
            bool isFlyAllowed = IsFlyAllowed();
            
            if (isFlyAllowed)
            {
                // If Fly is allowed, determine if is falling down otherwise its a jump!
                
                Vector3 worldUp = -_character.GetGravityDirection();
                float verticalSpeed = Vector3.Dot(_character.GetVelocity(), worldUp);

                isFlyAllowed = verticalSpeed < 0.0f;
            }

            return isFlyAllowed;
        }
        
        private void OnCollided(ref CollisionResult collisionResult)
        {
            // If flying and collided with walkable ground, exit flying state.
            // I.e: Change to Falling movement mode as this is managed based on grounding status.

            if (_character.IsFlying() && collisionResult.isWalkable)
                _character.SetMovementMode(Character.MovementMode.Falling);
        }
        
        private void OnBeforeSimulationUpdated(float deltaTime)
        {
            // Attempts to enter Flying movement mode
            
            bool isFlying = _character.IsFlying();
            bool wantsToFly = _character.jumpInputPressed;

            if (!isFlying && wantsToFly && CanFly())
                _character.SetMovementMode(Character.MovementMode.Flying);
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            _character.Collided += OnCollided;
            _character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;
        }
        
        private void OnDisable()
        {
            _character.Collided -= OnCollided;
            _character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;
        }
    }
}
