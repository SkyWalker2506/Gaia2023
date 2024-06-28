using UnityEngine;
using UnityEngine.AI;

namespace ECM2
{
    /// <summary>
    /// This component extends a Character (through composition) adding navigation capabilities using a NavMeshAgent.
    /// This replaces the previous AgentCharacter class.
    /// </summary>
    
    [RequireComponent(typeof(Character)), RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshCharacter : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Should the agent brake automatically to avoid overshooting the destination point? \n" +
                 "If true, the agent will brake automatically as it nears the destination.")]
        [SerializeField]
        private bool _autoBraking;

        [Tooltip("Distance from target position to start braking.")]
        [SerializeField]
        private float _brakingDistance;

        [Tooltip("Stop within this distance from the target position.")]
        [SerializeField]
        private float _stoppingDistance;

        #endregion

        #region FIELDS

        private NavMeshAgent _agent;
        private Character _character;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Cached NavMeshAgent component.
        /// </summary>

        public NavMeshAgent agent => _agent;
        
        /// <summary>
        /// Cached Character component.
        /// </summary>

        public Character character => _character;
        
        /// <summary>
        /// Should the agent brake automatically to avoid overshooting the destination point?
        /// If this property is set to true, the agent will brake automatically as it nears the destination.
        /// </summary>

        public bool autoBraking
        {
            get => _autoBraking;
            set
            {
                _autoBraking = value;
                agent.autoBraking = _autoBraking;
            }
        }

        /// <summary>
        /// Distance from target position to start braking.
        /// </summary>

        public float brakingDistance
        {
            get => _brakingDistance;
            set => _brakingDistance = Mathf.Max(0.0001f, value);
        }

        /// <summary>
        /// The ratio (0 - 1 range) of the agent's remaining distance and the braking distance.
        /// 1 If no auto braking or if agent's remaining distance is greater than brakingDistance.
        /// less than 1, if agent's remaining distance is less than brakingDistance.
        /// </summary>

        public float brakingRatio
        {
            get
            {
                if (!autoBraking)
                    return 1f;

                return agent.hasPath ? Mathf.InverseLerp(0.0f, brakingDistance, agent.remainingDistance) : 1.0f;
            }
        }

        /// <summary>
        /// Stop within this distance from the target position.
        /// </summary>

        public float stoppingDistance
        {
            get => _stoppingDistance;
            set
            {
                _stoppingDistance = Mathf.Max(0.0f, value);
                agent.stoppingDistance = _stoppingDistance;
            }
        }

        #endregion

        #region EVENTS
        
        public delegate void DestinationReachedEventHandler();

        /// <summary>
        /// Event triggered when agent reaches its destination.
        /// </summary>

        public event DestinationReachedEventHandler DestinationReached;

        /// <summary>
        /// Trigger DestinationReached event.
        /// Called when agent reaches its destination.
        /// </summary>

        public virtual void OnDestinationReached()
        {
            DestinationReached?.Invoke();
        }
        
        #endregion

        #region METHODS
        
        /// <summary>
        /// Cache used components.
        /// </summary>
        
        protected virtual void CacheComponents()
        {
            _agent = GetComponent<NavMeshAgent>();
            _character = GetComponent<Character>();
        }
        
        /// <summary>
        /// Does the Agent currently has a path?
        /// </summary>
        
        public virtual bool HasPath()
        {
            return agent.hasPath;
        }

        /// <summary>
        /// True if Agent is following a path, false otherwise.
        /// </summary>
        
        public virtual bool IsPathFollowing()
        {
            return agent.hasPath && !agent.isStopped;
        }
        
        /// <summary>
        /// Returns the destination set for this agent.
        /// If a destination is set but the path is not yet processed,
        /// the returned position will be valid navmesh position that's closest to the previously set position.
        /// If the agent has no path or requested path - returns the agents position on the navmesh.
        /// If the agent is not mapped to the navmesh (e.g. Scene has no navmesh) - returns a position at infinity.
        /// </summary>
        
        public virtual Vector3 GetDestination()
        {
            return agent.destination;
        }
        
        /// <summary>
        /// Requests the character to move to the valid navmesh position that's closest to the requested destination.
        /// </summary>

        public virtual void MoveToDestination(Vector3 destination)
        {
            Vector3 worldUp = -character.GetGravityDirection();
            Vector3 toDestination2D = Vector3.ProjectOnPlane(destination - character.position, worldUp);
            
            if (toDestination2D.sqrMagnitude >= MathLib.Square(stoppingDistance))
                agent.SetDestination(destination);
        }
        
        /// <summary>
        /// Pause / Resume Character path following movement.
        /// If set to True, the character's movement will be stopped along its current path.
        /// If set to False after the character has stopped, it will resume moving along its current path.
        /// </summary>
        
        public virtual void PauseMovement(bool pause)
        {
            agent.isStopped = pause;
            character.SetMovementDirection(Vector3.zero);
        }
        
        /// <summary>
        /// Halts Character's current path following movement.
        /// This will clear agent's current path.
        /// </summary>
        
        public virtual void StopMovement()
        {
            agent.ResetPath();
            character.SetMovementDirection(Vector3.zero);
        }
        
        /// <summary>
        /// Computes the analog input modifier (0.0f to 1.0f) based on Character's max speed and given desired velocity.
        /// </summary>
        
        protected virtual float ComputeAnalogInputModifier(Vector3 desiredVelocity)
        {
            float maxSpeed = _character.GetMaxSpeed();
            if (desiredVelocity.sqrMagnitude > 0.0f && maxSpeed > 0.00000001f)
                return Mathf.Clamp01(desiredVelocity.magnitude / maxSpeed);

            return 0.0f;
        }
        
        /// <summary>
        /// Calculates Character movement direction from a given desired velocity factoring (if enabled) auto braking.
        /// </summary>

        protected virtual Vector3 CalcMovementDirection(Vector3 desiredVelocity)
        {
            Vector3 worldUp = -character.GetGravityDirection();
            Vector3 desiredVelocity2D = Vector3.ProjectOnPlane(desiredVelocity, worldUp);
            
            Vector3 scaledDesiredVelocity2D = desiredVelocity2D * brakingRatio;

            float minAnalogSpeed = _character.GetMinAnalogSpeed();
            if (scaledDesiredVelocity2D.sqrMagnitude < MathLib.Square(minAnalogSpeed))
                scaledDesiredVelocity2D = scaledDesiredVelocity2D.normalized * minAnalogSpeed;

            return Vector3.ClampMagnitude(scaledDesiredVelocity2D, ComputeAnalogInputModifier(scaledDesiredVelocity2D));
        }
        
        /// <summary>
        /// Makes the character's follow Agent's path (if any).
        /// Eg: Keep updating Character's movement direction vector to steer towards Agent's destination until reached.
        /// </summary>

        protected virtual void DoPathFollowing()
        {
            if (!IsPathFollowing())
                return;
            
            // Is destination reached ?

            if (agent.remainingDistance <= stoppingDistance)
            {
                // Destination is reached, stop movement

                StopMovement();

                // Trigger event

                OnDestinationReached();
            }
            else
            {
                // If destination not reached, request a Character to move towards agent's desired velocity direction

                Vector3 movementDirection = CalcMovementDirection(agent.desiredVelocity);
                character.SetMovementDirection(movementDirection);
            }
        }
        
        /// <summary>
        /// Synchronize the NavMeshAgent with Character (eg: speed, acceleration, velocity, etc) as we moves the Agent.
        /// </summary>

        protected virtual void SyncNavMeshAgent()
        {
            agent.angularSpeed = _character.rotationRate;

            agent.speed = _character.GetMaxSpeed();
            agent.acceleration = _character.GetMaxAcceleration();
            
            agent.velocity = _character.GetVelocity();
            agent.nextPosition = _character.GetPosition();

            agent.radius = _character.radius;
            agent.height = _character.height;
        }
        
        /// <summary>
        /// On MovementMode change, stop agent movement if character is not walking or falling.
        /// </summary>
        
        protected virtual void OnMovementModeChanged(Character.MovementMode prevMovementMode, int prevCustomMovementMode)
        {
            if (!character.IsWalking() || !character.IsFalling())
            {
                StopMovement();
            }
        }
        
        /// <summary>
        /// While Character has a valid path, do path following. 
        /// </summary>

        protected virtual void OnBeforeSimulationUpdated(float deltaTime)
        {
            DoPathFollowing();
        }
        
        #endregion

        #region MONOBEHAVIOUR
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        private void Reset()
        {
            _autoBraking = true;

            _brakingDistance = 2.0f;
            _stoppingDistance = 1.0f;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        private void OnValidate()
        {
            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();
            
            brakingDistance = _brakingDistance;
            stoppingDistance = _stoppingDistance;
        }

        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void Awake()
        {
            // Cache used components
            
            CacheComponents();
            
            // Initialize NavMeshAgent

            agent.autoBraking = autoBraking;
            agent.stoppingDistance = stoppingDistance;

            // Turn-off NavMeshAgent auto-control,
            // we control it (see SyncNavMeshAgent method)
            
            agent.updatePosition = false;
            agent.updateRotation = false;

            agent.updateUpAxis = false;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void OnEnable()
        {
            // Subscribe to Character events
            
            character.MovementModeChanged += OnMovementModeChanged;
            character.BeforeSimulationUpdated += OnBeforeSimulationUpdated;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>
        
        protected virtual void OnDisable()
        {
            // Un-Subscribe to Character events
            
            character.MovementModeChanged -= OnMovementModeChanged;
            character.BeforeSimulationUpdated -= OnBeforeSimulationUpdated;
        }
        
        /// <summary>
        /// If overriden, base method MUST be called.
        /// </summary>

        protected virtual void LateUpdate()
        {
            SyncNavMeshAgent();
        }

        #endregion
    }
}