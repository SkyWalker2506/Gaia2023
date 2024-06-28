using UnityEngine;

namespace ECM2.Walkthrough.Ex61
{
    /// <summary>
    /// This example shows how to simulate a directional wind using the Character's AddForce method.
    /// </summary>
    
    public class ForceZone : MonoBehaviour
    {
        public Vector3 windDirection = Vector3.up;
        public float windStrength = 20.0f;

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (!other.TryGetComponent(out Character character))
                return;
            
            Vector3 windForce = windDirection.normalized * windStrength;
            
            // Check to see if applied momentum is enough to overcome gravity,
            // if does, pause ground constraint to allow the character leave the ground
            
            Vector3 worldUp = -character.GetGravityDirection();
            float upWindForceMagnitude = Vector3.Dot(windForce, worldUp);
            
            if (upWindForceMagnitude > 0.0f)
            {
                if (character.IsWalking() && upWindForceMagnitude - character.GetGravityMagnitude() > 0.0f)
                    character.PauseGroundConstraint();
            }
            
            // Add force ignoring character's mass

            character.AddForce(windForce, ForceMode.Acceleration);
        }
    }
}