using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class UtpMessageReporter : IUtpMessageReporter
    {
        public ITestRunnerApiMapper TestRunnerApiMapper;
        public IUtpLogger Logger;

        public UtpMessageReporter(IUtpLogger utpLogger, string projectRepoPath, ISpanIdStateStore spanIdStateStore = null)
        {
            TestRunnerApiMapper = new TestRunnerApiMapper(projectRepoPath, spanIdStateStore);
            Logger = utpLogger;
        }

        public void ReportTestRunStarted(ITestAdaptor testsToRun)
        {
            var testPlanMessage = TestRunnerApiMapper.MapTestToTestPlanMessage(testsToRun);
            Logger.Log(testPlanMessage);

            // Emit the single run-level root suite here, not from the per-node tree walk. RunStarted
            // fires exactly once and is not re-fired after a domain reload, so this root is stable;
            // emitting it from the tree walk instead would re-emit the NUnit synthetic root after a
            // reload under a different (productName-derived) name, producing a second, mismatched root.
            Logger.Log(TestRunnerApiMapper.MapToRunRootStartedMessage());

            Logger.Log(UtpMessageBuilder.BuildScreenSettings());
            Logger.Log(UtpMessageBuilder.BuildPlayerSettings());
            Logger.Log(UtpMessageBuilder.BuildBuildSettings());
            Logger.Log(UtpMessageBuilder.BuildPlayerSystemInfo());
            Logger.Log(UtpMessageBuilder.BuildQualitySettings());
        }

        public void ReportTestRunFinished(ITestResultAdaptor testResults)
        {
            // Close the run-level root opened in ReportTestRunStarted. RunFinished is terminal (fires
            // once, after any domain reloads), so the End matches the single Begin by spanId.
            Logger.Log(TestRunnerApiMapper.MapToRunRootFinishedMessage(testResults));
        }

        public void ReportTestStarted(ITestAdaptor test)
        {
            if (test.IsSuite)
            {
                // The run container is emitted once as the stable run-level root from Run Started/Finished;
                // skip its per-node emission (re-fires post-reload with an unstable productName → second
                // root). A meaningful parentless top (e.g. a scene group) is not the container and is emitted.
                if (TestTree.IsDisposableRunContainer(test))
                    return;

                var suiteMsg = TestRunnerApiMapper.MapSuiteToSuiteStartedMessage(test);
                Logger.Log(suiteMsg);
            }
            else
            {
                var msg = TestRunnerApiMapper.MapTestToTestStartedMessage(test);
                Logger.Log(msg);
            }
        }

        public void ReportTestFinished(ITestResultAdaptor result)
        {
            if (result.Test.IsSuite)
            {
                // Pass the result (not result.Test): remote finish callbacks leave result.Test.Children
                // null, so the container must be classified from the result tree, which is populated.
                if (TestTree.IsDisposableRunContainer(result))
                    return;

                var suiteMsg = TestRunnerApiMapper.TestResultToSuiteFinishedMessage(result);
                Logger.Log(suiteMsg);
            }
            else
            {
                var msg = TestRunnerApiMapper.TestResultToTestFinishedMessage(result);
                Logger.Log(msg);
            }
        }
    }
}
