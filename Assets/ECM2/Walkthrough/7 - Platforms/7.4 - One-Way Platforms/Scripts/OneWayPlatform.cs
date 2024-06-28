using UnityEngine;

namespace ECM2.Walkthrough.EX74
{
    /// <summary>
    /// This example shows how to implement a one-way platform.
    ///
    /// This enables / disables the platform vs character collisions
    /// when a Character enter / exits the platform's trigger volume.
    /// 
    /// </summary>

    public class OneWayPlatform : MonoBehaviour
    {
        public Collider platformCollider;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            Character character = other.GetComponent<Character>();
            if (character)
                character.IgnoreCollision(platformCollider);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            
            Character character = other.GetComponent<Character>();
            if (character)
                character.IgnoreCollision(platformCollider, false);
        }
    }
}
