using System;
using System.Collections;
using UnityEditor.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene
{
    internal class StoreSceneSetupTask : TestTaskBase
    {
        internal Func<SceneSetup[]> GetSceneManagerSetup = EditorSceneManager.GetSceneManagerSetup;
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.SceneSetup = GetSceneManagerSetup();
            yield break;
        }
    }
}
