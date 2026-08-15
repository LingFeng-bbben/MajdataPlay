using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class EnterPlayModeTask : TestTaskBase
    {
        public Func<bool> IsInPlayMode = () => Application.isPlaying;
        public Action EnterPlayMode = () => EditorApplication.isPlaying = true;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (IsInPlayMode())
            {
                yield break;
            }

            // Give the UI a change to update the progress bar, sa entering playmode freezes.
            yield return null;

            EnterPlayMode();

            while (!IsInPlayMode())
            {
                yield return null;
            }
        }
    }
}
