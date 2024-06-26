#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    public class BakeLightmapsTask : GaiaTaskBase
    {
        public override void SetTaskTimer(float value)
        {
            TaskWaitTime = value;
        }

        public override void DoTask()
        {
#if UNITY_EDITOR
#if !UNITY_2023_2_OR_NEWER
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#endif
            Lightmapping.BakeAsync();
#endif
            FinishTask();
        }

        public override void FinishTask()
        {
            TaskFinished = true;
        }
    }
}