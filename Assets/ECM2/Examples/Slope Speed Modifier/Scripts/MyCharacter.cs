using UnityEngine;

namespace ECM2.Examples.SlopeSpeedModifier
{
    /// <summary>
    /// This example shows how to extend a Character (through inheritance) to modify its speed
    /// based on current slope angle.
    /// </summary>
    
    public class MyCharacter : Character
    {
        /// <summary>
        /// Returns the Character's current speed modified by slope angle. 
        /// </summary>
        
        public override float GetMaxSpeed()
        {
            float maxSpeed = base.GetMaxSpeed();

            float slopeAngle = GetSignedSlopeAngle();
            float speedModifier = slopeAngle > 0.0f
                ? 1.0f - Mathf.InverseLerp(0.0f, 90.0f, +slopeAngle)    // Decrease speed when moving up-slope
                : 1.0f + Mathf.InverseLerp(0.0f, 90.0f, -slopeAngle);   // Increase speed when moving down-slope
            
            return maxSpeed * speedModifier;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 400, 20),
                $"Slope angle: {GetSignedSlopeAngle():F2} maxSpeed: {GetMaxSpeed():F2} ");
        }
    }
}
