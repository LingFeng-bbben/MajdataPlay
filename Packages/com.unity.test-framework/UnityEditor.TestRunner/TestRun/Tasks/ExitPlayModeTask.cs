using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class ExitPlayModeTask : TestTaskBase
    {
        public ExitPlayModeTask()
        {
            RunOnCancel = true;
            RunOnError = ErrorRunMode.RunAlways;
        }

        public Func<bool> IsInPlayMode = () => Application.isPlaying;
        public Action ExitPlayMode = () => EditorApplication.isPlaying = false;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (!IsInPlayMode())
            {
                yield break;
            }

            ExitPlayMode();

            while (IsInPlayMode())
            {
                yield return null;
            }
        }
    }
}
