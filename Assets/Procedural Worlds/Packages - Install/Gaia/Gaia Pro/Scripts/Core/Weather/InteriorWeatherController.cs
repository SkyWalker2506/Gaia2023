using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public enum InteriorWeatherControllerMode { Collision, DisableVFX }
    public enum InteriorWeatherTriggerMode { Trigger, Bounds }

    public class InteriorWeatherController : MonoBehaviour
    {
        public CollisionDetectionType TriggerType
        {
            get { return m_triggerType; }
            set
            {
                if (m_triggerType != value)
                {
                    m_triggerType = value;
                    UpdateColliderType();
                }
            }
        }
        public Vector3 TriggerSize
        {
            get { return m_triggerSize; }
            set
            {
                if (m_triggerSize != value)
                {
                    m_triggerSize = value;
                    UpdateColliderType();
                }
            }
        }
        public float TriggerRadius
        {
            get { return m_triggerRadius; }
            set
            {
                if (m_triggerRadius != value)
                {
                    m_triggerRadius = value;
                    UpdateColliderType();
                }
            }
        }

        public AudioReverbPreset m_interiorAudioRevertPreset = AudioReverbPreset.Room;
        public AudioReverbPreset m_exteriorAudioRevertPreset = AudioReverbPreset.Forest;
        public ParticleSystemCollisionQuality m_colliderQuality = ParticleSystemCollisionQuality.High;
        public LayerMask m_collideLayers = 1;
        public InteriorWeatherControllerMode m_controllerMode = InteriorWeatherControllerMode.DisableVFX;
        public InteriorWeatherTriggerMode m_triggerMode = InteriorWeatherTriggerMode.Bounds;
        public string m_playerTag = "Player";
        public Transform m_playerTransform;

        [SerializeField]
        private CollisionDetectionType m_triggerType = CollisionDetectionType.Box;
        [SerializeField]
        private Vector3 m_triggerSize = new Vector3(30f, 30f, 30f);
        [SerializeField]
        private float m_triggerRadius = 30f;
        [SerializeField]
        private AudioReverbFilter m_reverbFilter;
        [SerializeField]
        private Camera m_playerCamera;
        [SerializeField]
        private ParticleSystem m_rainParticleSystem;
        [SerializeField] 
        private MeshRenderer m_rainParticlesMeshRenderer;
        [SerializeField]
        private ParticleSystem m_snowParticleSystem;
        [SerializeField]
        private ParticleSystem m_snowParticleSystemExtra;
        [SerializeField]
        private ProceduralWorldsGlobalWeather m_weatherSystem;
        [SerializeField]
        private Bounds m_triggerBounds;
        private bool m_triggerStateChanged = false;
        private const string m_createVolumeMenuItem = "GameObject/Procedural Worlds/Gaia/Interior Weather Volume";

        #region Unity Functions

        /// <summary>
        /// Draw gizmo when object is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.color = new Color(1f, 0.137112f, 0f, 0.4f);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, boxCollider.size);
            }

            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.color = new Color(1f, 0.137112f, 0f, 0.4f);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, sphereCollider.radius);
            }
        }
        /// <summary>
        /// Load on enable
        /// </summary>
        private void OnEnable()
        {
            Setup();
            UpdateColliderType();
        }
        /// <summary>
        /// Sets interior reverb on enter
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (m_triggerMode != InteriorWeatherTriggerMode.Trigger)
            {
                return;
            }

            if (other.CompareTag(m_playerTag))
            {
                UpdateParticleColliders(false);
            }
        }
        /// <summary>
        /// Sets exterior reverb on exit
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (m_triggerMode != InteriorWeatherTriggerMode.Trigger)
            {
                return;
            }

            if (other.CompareTag(m_playerTag))
            {
                UpdateParticleColliders(true);
            }
        }

        private void LateUpdate()
        {
            if (m_triggerMode == InteriorWeatherTriggerMode.Bounds && m_playerTransform != null)
            {
                bool triggerState = m_triggerBounds.Contains(m_playerTransform.position);
                if (m_triggerStateChanged != triggerState)
                {
                    m_triggerStateChanged = triggerState;
                    if (triggerState)
                    {
                        UpdateParticleColliders(false);
                    }
                    else
                    {
                        UpdateParticleColliders(true);
                    }
                }
            }
        }

        #endregion
        #region Utils

        /// <summary>
        /// Used to update the particles collision conditions
        /// </summary>
        /// <param name="enabled"></param>
        public void UpdateParticleColliders(bool enabled)
        {
            if (CheckWeatherSystem())
            {
                SetAudioReverbState(enabled);
                switch (m_controllerMode)
                {
                    case InteriorWeatherControllerMode.Collision:
                    {
                        if (m_weatherSystem.IsRaining)
                        {
                            SetParticleCollisionState(m_rainParticleSystem, enabled, m_rainParticlesMeshRenderer);
                        }
                        else if (m_weatherSystem.IsSnowing)
                        {
                            SetParticleCollisionState(m_snowParticleSystem, enabled);
                            SetParticleCollisionState(m_snowParticleSystemExtra, enabled);
                        }
                        break;
                    }
                    case InteriorWeatherControllerMode.DisableVFX:
                    {
                        if (m_weatherSystem.IsRaining)
                        {
                            SetParticleVFXState(m_rainParticleSystem, enabled, m_rainParticlesMeshRenderer);
                        }
                        else if (m_weatherSystem.IsSnowing)
                        {
                            SetParticleVFXState(m_snowParticleSystem, enabled);
                            SetParticleVFXState(m_snowParticleSystemExtra, enabled);
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Sets up the player and the audio reverb
        /// </summary>
        public void Setup()
        {
            if (CheckWeatherSystem())
            {
                if (m_playerCamera == null)
                {
                    Transform player = ProceduralWorldsGlobalWeather.GetPlayer();
                    if (player != null)
                    {
                        m_playerCamera =  player.GetComponent<Camera>();
                    }
                }

                if (m_playerCamera != null)
                {
                    m_reverbFilter = m_playerCamera.gameObject.GetComponent<AudioReverbFilter>();
                    if (m_reverbFilter == null)
                    {
                        m_reverbFilter = m_playerCamera.gameObject.AddComponent<AudioReverbFilter>();
                        if (m_reverbFilter != null)
                        {
                            m_reverbFilter.reverbPreset = AudioReverbPreset.Forest;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Sets the player transform to 'player' value
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayerTransform(Transform player)
        {
            m_playerTransform = player;
        }
        /// <summary>
        /// Checks all the components and return true if the weather system is found
        /// </summary>
        /// <returns></returns>
        private bool CheckWeatherSystem()
        {
            if (m_weatherSystem == null)
            {
                m_weatherSystem = ProceduralWorldsGlobalWeather.Instance;
            }
            if (m_weatherSystem != null)
            {
                if (m_rainParticleSystem == null)
                {
                    m_rainParticleSystem = m_weatherSystem.m_rainVFX;
                }
                if (m_rainParticlesMeshRenderer == null)
                {
                    m_rainParticlesMeshRenderer = m_weatherSystem.m_rainParticles;
                }
                if (m_snowParticleSystem == null)
                {
                    m_snowParticleSystem = m_weatherSystem.m_snowVFX;
                }
                if (m_snowParticleSystemExtra == null)
                {
                    m_snowParticleSystemExtra = m_weatherSystem.m_snowParticles;
                }
            }

            return m_weatherSystem != null;
        }
        /// <summary>
        /// Sets the audio reverb state
        /// </summary>
        /// <param name="state"></param>
        private void SetAudioReverbState(bool state)
        {
            if (m_reverbFilter != null)
            {
                if (m_weatherSystem.IsRaining || m_weatherSystem.IsSnowing)
                {
                    if (enabled)
                    {
                        m_reverbFilter.reverbPreset = m_exteriorAudioRevertPreset;
                    }
                    else
                    {
                        m_reverbFilter.reverbPreset = m_interiorAudioRevertPreset;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the particle vfx state, this will stop the particle system and disable the vfx
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <param name="state"></param>
        /// <param name="rainMeshRenderer"></param>
        private void SetParticleVFXState(ParticleSystem particleSystem, bool state, MeshRenderer rainMeshRenderer = null)
        {
            if (particleSystem == null)
            {
                return;
            }

            if (state)
            {
                particleSystem.gameObject.SetActive(true);
                particleSystem.Play();
                if (rainMeshRenderer != null)
                {
                    rainMeshRenderer.enabled = true;
                }
            }
            else
            {
                particleSystem.gameObject.SetActive(false);
                particleSystem.Stop();
                if (rainMeshRenderer != null)
                {
                    rainMeshRenderer.enabled = false;
                }
            }
        }
        /// <summary>
        /// Sets the particle collision based on the state
        /// Also disables the rain mesh renderer
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <param name="state"></param>
        /// <param name="rainMeshRenderer"></param>
        private void SetParticleCollisionState(ParticleSystem particleSystem, bool state, MeshRenderer rainMeshRenderer = null)
        {
            if (particleSystem == null)
            {
                return;
            }

            ParticleSystem.CollisionModule collision = particleSystem.collision;
            if (state)
            {
                collision.enabled = false;
                collision.type = ParticleSystemCollisionType.World;
                collision.quality = m_colliderQuality;
                collision.collidesWith = m_collideLayers;
                if (rainMeshRenderer != null)
                {
                    rainMeshRenderer.enabled = true;
                }
            }
            else
            {
                collision.enabled = true;
                collision.type = ParticleSystemCollisionType.World;
                if (rainMeshRenderer != null)
                {
                    rainMeshRenderer.enabled = false;
                }
            }
        }
        /// <summary>
        /// Updates the collider settings
        /// </summary>
        private void UpdateColliderType()
        {
            switch (TriggerType)
            {
                case CollisionDetectionType.Box:
                    AddBoxCollider();
                    break;
                case CollisionDetectionType.Spherical:
                    AddSphericalCollider();
                    break;
            }
        }
        /// <summary>
        /// Adds and configures the box collider
        /// </summary>
        private void AddBoxCollider()
        {
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }

            boxCollider.isTrigger = true;
            boxCollider.size = TriggerSize;

            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(sphereCollider);
                }
                else
                {
                    DestroyImmediate(sphereCollider);
                }
            }

            m_triggerBounds = new Bounds(transform.position, TriggerSize);
        }
        /// <summary>
        /// Adds and configures the spherical collider
        /// </summary>
        private void AddSphericalCollider()
        {
            SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }

            sphereCollider.isTrigger = true;
            sphereCollider.radius = TriggerRadius;

            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(boxCollider);
                }
                else
                {
                    DestroyImmediate(boxCollider);
                }
            }

            m_triggerBounds = new Bounds(transform.position, new Vector3(TriggerRadius, TriggerRadius, TriggerRadius));
        }

        #endregion
        #region Public Static Functions

#if UNITY_EDITOR
        /// <summary>
        /// Function used to create a new controller volume
        /// </summary>
        [MenuItem(m_createVolumeMenuItem)]
        public static void CreateInteriorWeatherControllerVolume()
        {
            GameObject newVolumeObject = new GameObject("New Gaia Interior Weather Volume")
            {
                transform =
                {
                    position = SceneView.lastActiveSceneView.camera.transform.position
                }
            };

            InteriorWeatherController interiorController = newVolumeObject.AddComponent<InteriorWeatherController>();
            interiorController.Setup();

            BoxCollider boxCollider = newVolumeObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(30f, 30f, 30f);
            boxCollider.isTrigger = true;

            Selection.activeObject = newVolumeObject;
            EditorGUIUtility.PingObject(newVolumeObject);
            SceneView.lastActiveSceneView.FrameSelected();
        }
#endif

        #endregion
    }
}