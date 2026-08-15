using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PreparePlayModeRunTask : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            testJobData.OriginalProjectSettings = new TestJobData.SavedProjectSettings
            {
                consoleErrorPaused = ConsoleWindow.GetConsoleErrorPause(),
                runInBackgroundValue = Application.runInBackground
            };
            ConsoleWindow.SetConsoleErrorPause(false);
            Application.runInBackground = true;
            if (testJobData.InitTestScene.IsValid())
            {
                SceneManager.SetActiveScene(testJobData.InitTestScene);
            }

            yield break;
        }
    }
}
