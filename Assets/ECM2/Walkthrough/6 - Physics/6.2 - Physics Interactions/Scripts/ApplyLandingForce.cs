using UnityEngine;

namespace ECM2.Walkthrough.Ex62
{
    /// <summary>
    /// This example extends a Character (through composition) to apply a landing force
    /// when it lands on a dynamic rigidbody.
    ///
    /// The landing force is calculated (characterMass * characterGravity * landingVelocity * landingForceMag)
    /// and is applied along gravity direction.
    /// </summary>
    
    public class ApplyLandingForce : MonoBehaviour
    {
        public float landingForceScale = 1.0f;
        
        private Character _character;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            _character.Landed += OnLanded;
        }

        private void OnDisable()
        {
            _character.Landed -= OnLanded;
        }

        private void OnLanded(Vector3 landingVelocity)
        {
            Rigidbody groundRigidbody = _character.characterMovement.groundRigidbody;
            if (!groundRigidbody)
                return;
            
            Vector3 force = _character.GetGravityVector() *
                            (_character.mass * landingVelocity.magnitude * landingForceScale);
                
            groundRigidbody.AddForceAtPosition(force, _character.position);
        }
    }
}