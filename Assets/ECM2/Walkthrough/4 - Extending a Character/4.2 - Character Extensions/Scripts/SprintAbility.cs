using UnityEngine;

namespace ECM2.Walkthrough.Ex42
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) to perform a sprint ability.
    /// This one use the new simulation OnBeforeSimulationUpdate event (introduced in v1.4),
    /// to easily modify the character's state within Character's simulation loop.
    /// </summary>
    
    public class SprintAbility : MonoBehaviour
    {
        [Space(15.0f)]
        public float maxSprintSpeed = 10.0f;
        
        private Character _character;

        private bool _isSprinting;
        private bool _sprintInputPressed;

        private float _cachedMaxWalkSpeed;
        
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
        
        /// <summary>
        /// Return true if the character is sprinting, false otherwise.
        /// </summary>

        public bool IsSprinting()
        {
            return _isSprinting;
        }
        
        /// <summary>
        /// Determines if the character, is able to sprint in its current state.
        /// </summary>

        private bool CanSprint()
        {
            return _character.IsWalking() && !_character.IsCrouched();
        }
        
        /// <summary>
        /// Handles sprint input and adjusts character speed accordingly.
        /// </summary>

        private void CheckSprintInput()
        {
            if (!_isSprinting && _sprintInputPressed && CanSprint())
            {
                _isSprinting = true;

                _cachedMaxWalkSpeed = _character.maxWalkSpeed;
                _character.maxWalkSpeed = maxSprintSpeed;

            }
            else if (_isSprinting && (!_sprintInputPressed || !CanSprint()))
            {
                _isSprinting = false;
                
                _character.maxWalkSpeed = _cachedMaxWalkSpeed;
            }
        }
        
        private void OnBeforeSimulationUpdated(float deltaTime)
        {
            // Handle sprinting
            
            CheckSprintInput();
        }

        private void Awake()
        {
            // Cache character
            
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            // Subscribe to Character BeforeSimulationUpdated event
            
            _character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;
        }
        
        private void OnDisable()
        {
            // Un-Subscribe from Character BeforeSimulationUpdated event
            
            _character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;
        }
    }
}