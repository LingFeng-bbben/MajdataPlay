using System.Collections;
using System.Reflection;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class ResetInteractionModeTask : TestTaskBase
    {
        private const string ApplicationIdleTimeKey = "ApplicationIdleTime";
        private const string InteractionModeKey = "InteractionMode";

        public ResetInteractionModeTask()
        {
            RunOnError = ErrorRunMode.RunAlways;
            RunOnCancel = true;
        }
        public override IEnumerator Execute(TestJobData testJobData)
        {
            SetInteractionModeToUserSetting(testJobData);
            EditorApplication.UpdateInteractionModeSettings();
            yield break;
        }
        private void SetInteractionModeToUserSetting(TestJobData testJobData)
        {
            if (testJobData.UserApplicationIdleTime != -1)
            {
                EditorPrefs.SetInt(ApplicationIdleTimeKey, testJobData.UserApplicationIdleTime);
            }

            if (testJobData.UserInteractionMode != -1)
            {
                EditorPrefs.SetInt(InteractionModeKey, testJobData.UserInteractionMode);
            }
        }
    }
}
