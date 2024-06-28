using UnityEngine;

namespace ECM2.Walkthrough.Ex71
{
    /// <summary>
    /// This example shows how to implement a rotating platform.
    /// This will freely rotate at the given rotation speed along its defined rotationAxis.
    /// </summary>
    
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicRotate : MonoBehaviour
    {
        #region FIELDS

        [SerializeField]
        private float _rotationSpeed = 30.0f;
        
        public Vector3 rotationAxis = Vector3.up;

        #endregion

        #region PRIVATE FIELDS

        private Rigidbody _rigidbody;

        private float _angle;

        #endregion

        #region PROPERTIES

        public float rotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        public float angle
        {
            get => _angle;
            set => _angle = MathLib.ClampAngle(value, 0.0f, 360.0f);
        }

        #endregion

        #region MONOBEHAVIOUR

        public void OnValidate()
        {
            rotationSpeed = _rotationSpeed;
            rotationAxis = rotationAxis.normalized;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
        }

        public void FixedUpdate()
        {
            angle += rotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, rotationAxis.normalized);
            _rigidbody.MoveRotation(_rigidbody.rotation * rotation);
        }

        #endregion
    }
}
