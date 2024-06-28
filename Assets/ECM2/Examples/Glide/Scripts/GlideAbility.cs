using UnityEngine;

namespace ECM2.Examples.Glide
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) implementing a Glide mechanic. 
    /// </summary>
    
    public class GlideAbility : MonoBehaviour
    {
        public bool canEverGlide = true;
        public float maxFallSpeedGliding = 1.0f;
        
        private Character _character;
        
        protected bool _glideInputPressed;
        protected bool _isGliding;

        public bool glideInputPressed => _glideInputPressed;

        /// <summary>
        /// Is the Character gliding?
        /// </summary>
        
        public virtual bool IsGliding()
        {
            return _isGliding;
        }
        
        /// <summary>
        /// Request to start a glide.
        /// </summary>

        public virtual void Glide()
        {
            _glideInputPressed = true;
        }
        
        /// <summary>
        /// Request to stop gliding.
        /// </summary>

        public virtual void StopGliding()
        {
            _glideInputPressed = false;
        }
        
        /// <summary>
        /// Determines if the character is able to perform a glide in its current state. 
        /// </summary>
        
        protected virtual bool IsGlideAllowed()
        {
            return canEverGlide && _character.IsFalling();
        }
        
        /// <summary>
        /// Determines if the character can perform a requested glide.
        /// </summary>

        protected virtual bool CanGlide()
        {
            bool isGlideAllowed = IsGlideAllowed();
            
            if (isGlideAllowed)
            {
                Vector3 worldUp = -_character.GetGravityDirection();
                float verticalSpeed = Vector3.Dot(_character.GetVelocity(), worldUp);

                isGlideAllowed = verticalSpeed < 0.0f;
            }

            return isGlideAllowed;
        }
        
        /// <summary>
        /// Start / Stop a requested glide.
        /// </summary>
        
        protected virtual void CheckGlideInput()
        {
            if (!_isGliding && _glideInputPressed && CanGlide())
            {
                _isGliding = true;
                _character.maxFallSpeed = maxFallSpeedGliding;

            }
            else if (_isGliding && (!_glideInputPressed || !CanGlide()))
            {
                _isGliding = false;
                _character.maxFallSpeed = 40.0f;
            }
        }
        
        private void OnBeforeCharacterSimulationUpdated(float deltaTime)
        {
            CheckGlideInput();
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }
        
        private void OnEnable()
        {
            _character.BeforeSimulationUpdated += OnBeforeCharacterSimulationUpdated;
        }

        private void OnDisable()
        {
            _character.BeforeSimulationUpdated -= OnBeforeCharacterSimulationUpdated;
        }
    }
}