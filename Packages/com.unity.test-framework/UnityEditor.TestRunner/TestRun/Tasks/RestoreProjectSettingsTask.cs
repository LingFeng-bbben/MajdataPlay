using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class RestoreProjectSettingsTask : TestTaskBase
    {
        public RestoreProjectSettingsTask()
        {
            RunOnError = ErrorRunMode.RunAlways;
            RunOnCancel = true;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.OriginalProjectSettings == null)
            {
                yield break;
            }
            ConsoleWindow.SetConsoleErrorPause(testJobData.OriginalProjectSettings.consoleErrorPaused);
            Application.runInBackground = testJobData.OriginalProjectSettings.runInBackgroundValue;
        }
    }
}
