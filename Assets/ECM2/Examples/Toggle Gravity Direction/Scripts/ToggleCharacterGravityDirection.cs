using UnityEngine;

namespace ECM2.Examples.ToggleGravityDirection
{
    /// <summary>
    /// Extends a Character (through composition) to modify its gravity direction (toggle),
    /// and orient it along the world-up defined by the gravity direction.
    /// </summary>
    
    public class ToggleCharacterGravityDirection : MonoBehaviour
    {
        private Character _character;

        private void RotateCharacterToGravity(float deltaTime)
        {
            // Calculate target character's rotation
            
            Vector3 worldUp = -1.0f * _character.GetGravityDirection();
            Vector3 characterUp = _character.GetUpVector();
            
            Quaternion characterRotation = _character.GetRotation();
            Quaternion targetRotation = Quaternion.FromToRotation(characterUp, worldUp) * characterRotation;
            
            // Smoothly adjust character's rotation to follow new world-up direction

            characterRotation =
                Quaternion.RotateTowards(characterRotation, targetRotation, _character.rotationRate * deltaTime);

            _character.SetRotation(characterRotation);
        }
        
        /// <summary>
        /// This event is called after the Character's movement mode and rotation has been updated,
        /// so we use this to append the gravity direction rotation. 
        /// </summary>
        
        private void OnAfterSimulationUpdated(float deltaTime)
        {
            RotateCharacterToGravity(deltaTime);
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            // Subscribe to Character events
            
            _character.AfterSimulationUpdated += OnAfterSimulationUpdated;
        }

        private void OnDisable()
        {
            // Subscribe from Character events
            
            _character.AfterSimulationUpdated -= OnAfterSimulationUpdated;
        }

        private void Update()
        {
            // If requested, toggle gravity direction if character is Falling
            
            if (_character.IsFalling() && Input.GetKeyDown(KeyCode.E))
                _character.gravityScale *= -1.0f;
        }
    }
}