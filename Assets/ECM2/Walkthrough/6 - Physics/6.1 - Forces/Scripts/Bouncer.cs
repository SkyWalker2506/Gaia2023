using UnityEngine;

namespace ECM2.Walkthrough.Ex61
{
    /// <summary>
    /// This example shows how to implement a directional bouncer using the Character's LaunchCharacter function.
    /// </summary>

    public class Bouncer : MonoBehaviour
    {
        public float launchImpulse = 15.0f;

        public bool overrideVerticalVelocity;
        public bool overrideLateralVelocity;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (!other.TryGetComponent(out Character character))
                return;
            
            character.PauseGroundConstraint();
            character.LaunchCharacter(transform.up * launchImpulse, overrideVerticalVelocity, overrideLateralVelocity);
        }
    }
}
