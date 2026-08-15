using UnityEngine;

namespace Unity.VisualScripting
{
    public static class RuntimeVSUsageUtility
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoadBeforeSceneLoad()
        {
            SavedVariables.OnEnterPlayMode();

            ApplicationVariables.OnEnterPlayMode();

            ReferenceCollector.Initialize();
        }
    }
}
