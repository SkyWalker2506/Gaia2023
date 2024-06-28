using UnityEngine;

namespace ECM2.Examples.OrientToGround
{
    /// <summary>
    /// This example extends a Character (through composition) to adjust a Character's rotation
    /// to follow a 'terrain' contour.
    /// </summary>
    
    public class CharacterOrientToGround : MonoBehaviour, IColliderFilter
    {
        public float maxSlopeAngle = 30.0f;
        public float alignRate = 10.0f;
        public float rayOffset = 0.1f;
        
        public LayerMask groundMask = 1;

        [Space(15f)]
        public bool drawRays = true;

        private readonly RaycastHit[] _hits = new RaycastHit[8];

        private Character _character;

        // Implement IColliderFilter.
        // Ignore character's capsule collider.

        public bool Filter(Collider otherCollider)
        {
            CharacterMovement characterMovement = _character.GetCharacterMovement();
            if (otherCollider == characterMovement.collider)
                return true;

            return false;
        }

        /// <summary>
        /// Computes the average normal sampling a 3x3 area, each ray is a rayOffset distance of other.
        /// </summary>        
        
        private Vector3 ComputeAverageNormal()
        {
            CharacterMovement characterMovement = _character.GetCharacterMovement();

            Vector3 worldUp = Vector3.up;            
            Vector3 castOrigin = _character.GetPosition() + worldUp * (characterMovement.height * 0.5f);

            Vector3 castDirection = -worldUp;
            float castDistance = characterMovement.height;
            LayerMask castLayerMask = groundMask;

            Vector3 avgNormal = Vector3.zero;
            
            float x = -rayOffset;
            float z = -rayOffset;

            int hitCount = 0;
            
            for (int i = 0; i < 3; i++)
            {
                z = -rayOffset;

                for (int j = 0; j < 3; j++)
                {
                    bool hit = CollisionDetection.Raycast(castOrigin + new Vector3(x, 0.0f, z), castDirection,
                        castDistance, castLayerMask, QueryTriggerInteraction.Ignore, out RaycastHit hitResult, _hits, this) > 0;
                    
                    if (hit)
                    {
                        float angle = Vector3.Angle(hitResult.normal, worldUp);
                        if (angle < maxSlopeAngle)
                        {
                            avgNormal += hitResult.normal;

                            if (drawRays)
                                Debug.DrawRay(hitResult.point, hitResult.normal, Color.yellow);

                            hitCount ++;
                        }    
                    }

                    z += rayOffset;
                }

                x += rayOffset;
            }

            if (hitCount > 0)
                avgNormal /= hitCount;
            else
                avgNormal = worldUp;

            if (drawRays)
                Debug.DrawRay(_character.GetPosition(), avgNormal * 2f, Color.green);
            
            return avgNormal;
        }
        
        private void OnAfterSimulationUpdated(float deltaTime)
        {
            Vector3 avgNormal = _character.IsWalking() ? ComputeAverageNormal() : Vector3.up;

            Quaternion characterRotation = _character.GetRotation();
            Vector3 characterUp = characterRotation * Vector3.up;

            Quaternion slopeRotation = Quaternion.FromToRotation(characterUp, avgNormal);
            characterRotation = Quaternion.Slerp(characterRotation, slopeRotation * characterRotation, alignRate * deltaTime);

            _character.SetRotation(characterRotation);
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            _character.AfterSimulationUpdated += OnAfterSimulationUpdated;
        }

        private void OnDisable()
        {
            _character.AfterSimulationUpdated -= OnAfterSimulationUpdated;
        }
    }
}