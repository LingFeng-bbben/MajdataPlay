using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PostbuildCleanupTask : BuildActionTaskBase<IPostBuildCleanup>
    {
        public PostbuildCleanupTask() : base(new PostbuildCleanupAttributeFinder())
        {
            RunOnError = ErrorRunMode.RunAlways;
        }

        protected override void Action(IPostBuildCleanup target, TestJobData testJobData)
        {
            target.Cleanup();
        }
    }
}
