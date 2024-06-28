using UnityEngine;

namespace ECM2.Walkthrough.Ex31
{
    /// <summary>
    /// This example shows how to animate a Character,
    /// using the Character data (movement direction, velocity, is jumping, etc) to feed your Animator.
    /// </summary>
    
    public class AnimationController : MonoBehaviour
    {
        // Cache Animator parameters
        
        private static readonly int Forward = Animator.StringToHash("Forward");
        private static readonly int Turn = Animator.StringToHash("Turn");
        private static readonly int Ground = Animator.StringToHash("OnGround");
        private static readonly int Crouch = Animator.StringToHash("Crouch");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int JumpLeg = Animator.StringToHash("JumpLeg");
        
        // Cached Character
        
        private Character _character;

        private void Awake()
        {
            // Cache our Character
            
            _character = GetComponentInParent<Character>();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            // Get Character animator

            Animator animator = _character.GetAnimator();

            // Compute input move vector in local space

            Vector3 move = transform.InverseTransformDirection(_character.GetMovementDirection());

            // Update the animator parameters

            float forwardAmount = _character.useRootMotion && _character.GetRootMotionController()
                ? move.z
                : Mathf.InverseLerp(0.0f, _character.GetMaxSpeed(), _character.GetSpeed());

            animator.SetFloat(Forward, forwardAmount, 0.1f, deltaTime);
            animator.SetFloat(Turn, Mathf.Atan2(move.x, move.z), 0.1f, deltaTime);

            animator.SetBool(Ground, _character.IsGrounded());
            animator.SetBool(Crouch, _character.IsCrouched());

            if (_character.IsFalling())
                animator.SetFloat(Jump, _character.GetVelocity().y, 0.1f, deltaTime);

            // Calculate which leg is behind, so as to leave that leg trailing in the jump animation
            // (This code is reliant on the specific run cycle offset in our animations,
            // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)

            float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + 0.2f, 1.0f);
            float jumpLeg = (runCycle < 0.5f ? 1.0f : -1.0f) * forwardAmount;

            if (_character.IsGrounded())
                animator.SetFloat(JumpLeg, jumpLeg);
        }
    }
}