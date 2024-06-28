using UnityEngine;

namespace ECM2.Walkthrough.Ex33
{
    /// <summary>
    /// This example shows how to extend a Character (through composition) to enable / disable (toggle)
    /// root motion as needed; in this case, enabled when walking.
    /// </summary>
    
    public class RootMotionToggle : MonoBehaviour
    {
        private Character _character;
        
        private void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMovementMode)
        {
            // Allow root motion only while walking
            
            _character.useRootMotion = _character.IsWalking();
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            // Subscribe to Character events
            
            _character.MovementModeChanged += OnMovementModeChanged;
        }

        private void OnDisable()
        {
            // Un-Subscribe from Character events
            
            _character.MovementModeChanged -= OnMovementModeChanged;
        }
    }
}
