using System;
namespace ProceduralWorlds.GTS
{
    [Serializable]
    public class GTSMeshSettings : GTSProfileSettings
    {
        public SaveResolution saveResolution = SaveResolution.Full;
        public int lodCount = 4;
        public float[] lodQuality = { 100f, 50f, 25f, 12.5f };
        public int subTiles = 3;
        public override void Reset()
        {
            base.Reset();
            saveResolution = SaveResolution.Full;
            lodCount = 4;
            lodQuality = new[] { 100f, 50f, 25f, 12.5f };
            subTiles = 3;
        }
    }
}