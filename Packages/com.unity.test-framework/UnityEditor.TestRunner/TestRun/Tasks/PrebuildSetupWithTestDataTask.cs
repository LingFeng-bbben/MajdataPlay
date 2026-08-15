using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestTools;
using TestMode = UnityEngine.TestTools.TestMode;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    class PrebuildSetupWithTestDataTask : BuildActionTaskBase<IPrebuildSetupWithTestData>
    {
        readonly TestMode m_TestModesIncluded;

        public PrebuildSetupWithTestDataTask(ExecutionSettings settings)
            : base(new PrebuildSetupWithTestDataAttributeFinder())
        {
            if (settings.EditModeIncluded())
                m_TestModesIncluded |= TestMode.EditMode;
            if (settings.PlayModeInEditorIncluded())
                m_TestModesIncluded |= TestMode.PlayMode;
            if (settings.PlayerIncluded())
                m_TestModesIncluded |= TestMode.Player;

        }

        protected override void Action(IPrebuildSetupWithTestData target, TestJobData testJobData)
        {
            target.Setup(new TestData(m_TestModesIncluded, testJobData.TargetRuntimePlatform, testJobData.filteredTests));
        }
    }
}
