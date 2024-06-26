using UnityEngine;

namespace ProceduralWorlds.GTS
{
    public class SetGTSRuntimeData : MonoBehaviour
    {
        public GTSRuntime runtimeData;

        public void Start()
        {
            SetGTSGlobalData();
        }

        public void SetGTSGlobalData()
        {
            // Set Vector Arrays
            Shader.SetGlobalVectorArray("_HeightData", runtimeData.HeightDataArray);
            Shader.SetGlobalVectorArray("_UVData", runtimeData.UVDataArray);
            Shader.SetGlobalVectorArray("_MaskMapRemapMinData", runtimeData.MaskMapRemapMinArray);
            Shader.SetGlobalVectorArray("_MaskMapRemapMaxData", runtimeData.MaskMapRemapMaxArray);
            Shader.SetGlobalVectorArray("_MaskMapRemapData", runtimeData.MaskMapRemapArray);
            Shader.SetGlobalVectorArray("_ColorData", runtimeData.ColorArray);
            Shader.SetGlobalVectorArray("_TriPlanarData", runtimeData.TriPlanarDataArray);
            Shader.SetGlobalVectorArray("_DisplacementData", runtimeData.DisplacementDataArray);
            Shader.SetGlobalVectorArray("_LayerDataA", runtimeData.LayerDataAArray);

            // Snow Settings
            Shader.SetGlobalVector("_PW_SnowDataA", runtimeData.SnowDataA);
            Shader.SetGlobalVector("_PW_SnowDataB", runtimeData.SnowDataB);
            Shader.SetGlobalVector("_PW_SnowDisplacementData", runtimeData.SnowDisplacementData);
            Shader.SetGlobalVector("_PW_SnowHeightData", runtimeData.SnowHeightData);
            Shader.SetGlobalVector("_PW_SnowColor", runtimeData.SnowColor);
            Shader.SetGlobalVector("_PW_SnowMaskRemapMin", runtimeData.SnowMaskRemapMin);
            Shader.SetGlobalVector("_PW_SnowMaskRemapMax", runtimeData.SnowMaskRemapMax);

            // Setting global textures is fine.
            Shader.SetGlobalTexture("_PW_SnowAlbedoMap", runtimeData.SnowAlbedoMap);
            Shader.SetGlobalTexture("_PW_SnowNormalMap", runtimeData.SnowNormalMap);
            Shader.SetGlobalTexture("_PW_SnowMaskMap", runtimeData.SnowMaskMap);

            // Set Legacy Snow Values
            Shader.SetGlobalFloat("_PW_Global_CoverLayer1Progress", runtimeData.GlobalSnowIntensity);
            Shader.SetGlobalFloat("_PW_Global_CoverLayer1FadeStart", runtimeData.GlobalCoverLayer1FadeStart);
            Shader.SetGlobalFloat("_PW_Global_CoverLayer1FadeDist", runtimeData.GlobalCoverLayer1FadeDist);

            Shader.SetGlobalVector("_PW_RainDataA", runtimeData.RainDataA);
            Shader.SetGlobalVector("_PW_RainDataB", runtimeData.RainDataB);
            Shader.SetGlobalTexture("_PW_RainMap", runtimeData.RainDataTexture);
        }
    }
}