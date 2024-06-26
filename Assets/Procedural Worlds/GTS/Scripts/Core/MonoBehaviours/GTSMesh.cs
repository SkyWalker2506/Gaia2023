using UnityEngine;
namespace ProceduralWorlds.GTS
{
    /// <summary>
    /// Experimental: Use at your own risk!
    /// Attach the GTS Mesh script to a converted Terrain to update the Material with the Profile.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class GTSMesh : GTSComponent
    {
        public GTSAssetID<Material> m_defaultMaterialID = new GTSAssetID<Material>();
        private Material m_defaultMaterial;
        public Material defaultMaterial
        {
            get
            {
#if UNITY_EDITOR
                if (m_defaultMaterial == null)
                    m_defaultMaterial = m_defaultMaterialID?.LoadAsset();
#endif
                return m_defaultMaterial;
            }
            set
            {
#if UNITY_EDITOR
                if (m_defaultMaterial != value)
                    m_defaultMaterialID?.SaveAsset(value);
#endif
                m_defaultMaterial = value;
            }
        }
        private MeshRenderer m_meshRenderer;
        public MeshRenderer meshRenderer
        {
            get
            {
                if (m_meshRenderer == null)
                    m_meshRenderer = GetComponent<MeshRenderer>();
                return m_meshRenderer;
            }
        }
        private MeshFilter m_meshFilter;
        public MeshFilter meshFilter
        {
            get
            {
                if (m_meshFilter == null)
                    m_meshFilter = GetComponent<MeshFilter>();
                return m_meshFilter;
            }
        }
        public Mesh sharedMesh
        {
            get
            {
                if (meshFilter != null)
                    return meshFilter.sharedMesh;
                return null;
            }
        }
        public override Material material
        {
            get => meshRenderer.sharedMaterial;
            set => meshRenderer.sharedMaterial = value;
        }
        public override Vector2 position
        {
            get
            {
                Vector3 worldPos = transform.position;
                return new Vector2(worldPos.x, worldPos.z);
            }
        }
        public override Vector2 size
        {
            get => new Vector2(1024, 1024);
        }
        public override Vector3 worldSize
        {
            get => new Vector3(1024, 0f, 1024f);
        }
        public override int heightmapResolution
        {
            get => 1024;
        }
        public void GetDefaulTerrainMaterial()
        {
            if (defaultMaterial == null)
            {
                GTSUtility.GetPipelineShaders(out Shader gtsShader, out Shader unityShader);
                Material material = meshRenderer.sharedMaterial;
                if (material != null)
                    if (material.shader != gtsShader)
                        defaultMaterial = material;
#if UNITY_EDITOR
                    else
                        defaultMaterial = GTSDefaults.GetDefaultTerrainMaterial();
#endif
            }
        }
        public override void ApplyProfile()
        {
            if (profile == null)
                return;
            GetDefaulTerrainMaterial();
            SetTerrainToGTS();
        }
        public override void RemoveProfile()
        {
            if (defaultMaterial != null)
                material = defaultMaterial;
        }
    }
}