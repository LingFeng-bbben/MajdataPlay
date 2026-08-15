using System;
using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class DeleteBootstrapSceneTask : TestTaskBase
    {
        public DeleteBootstrapSceneTask()
        {
            RunOnError = ErrorRunMode.RunAlways;
            RunOnCancel = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (string.IsNullOrEmpty(testJobData.InitTestScenePath))
            {
                yield break;
            }

            AssetDatabase.DeleteAsset(testJobData.InitTestScenePath);
        }
    }
}
