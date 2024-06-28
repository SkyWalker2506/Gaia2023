using UnityEngine;

namespace ECM2.Examples.PlanetWalk
{
    /// <summary>
    /// Extends the third person controller created in the third person example,
    /// to allow an arbitrary up direction.
    /// </summary>
    
    public class ThirdPersonController : ThirdPerson.ThirdPersonController
    {
        // Current camera forward, perpendicular to target's up vector.
        
        private Vector3 _cameraForward = Vector3.forward;
        
        public override void AddControlYawInput(float value)
        {
            // Rotate our forward along follow target's up axis

            Vector3 targetUp = followTarget.transform.up;
            _cameraForward = Quaternion.Euler(targetUp * value) * _cameraForward;
        }

        protected override void UpdateCameraRotation()
        {
            // Make sure camera forward vector is perpendicular to Character's current up vector
            
            Vector3 targetUp = followTarget.transform.up;
            Vector3.OrthoNormalize(ref targetUp, ref _cameraForward);
            
            // Computes final Camera rotation from yaw and pitch
            
            Transform cameraTransform = _character.cameraTransform;

            cameraTransform.rotation =
                Quaternion.LookRotation(_cameraForward, targetUp) * Quaternion.Euler(_cameraPitch, 0.0f, 0.0f);
        }
    }
}
