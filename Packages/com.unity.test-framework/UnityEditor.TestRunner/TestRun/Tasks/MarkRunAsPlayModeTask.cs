using System.Collections;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class MarkRunAsPlayModeTask : TestTaskBase
    {
        public MarkRunAsPlayModeTask()
        {
            RerunAfterResume = true;
        }
        public override IEnumerator Execute(TestJobData testJobData)
        {
            // This is a workaround to raise the signal that Playmode Launcher is running.
            // It is used by the graphics test framework, as there is no api to provide that information yet.
            PlaymodeLauncher.IsRunning = true;
            yield break;
        }
    }
}
