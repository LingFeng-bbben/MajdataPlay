using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;
using TestMode = UnityEngine.TestTools.TestMode;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    class PostbuildCleanupWithTestDataTask : BuildActionTaskBase<IPostbuildCleanupWithTestData>
    {
        readonly TestMode m_TestModesIncluded;

        public PostbuildCleanupWithTestDataTask(ExecutionSettings settings) : base(new PostbuildCleanupWithTestDataAttributeFinder())
        {
            if (settings.EditModeIncluded())
                m_TestModesIncluded |= TestMode.EditMode;
            if (settings.PlayModeInEditorIncluded())
                m_TestModesIncluded |= TestMode.PlayMode;
            if (settings.PlayerIncluded())
                m_TestModesIncluded |= TestMode.Player;
            RunOnError = ErrorRunMode.RunAlways;
        }

        protected override void Action(IPostbuildCleanupWithTestData target, TestJobData testJobData)
        {
            target.Cleanup(new TestData(m_TestModesIncluded, testJobData.TargetRuntimePlatform, testJobData.filteredTests));
        }
    }
}
