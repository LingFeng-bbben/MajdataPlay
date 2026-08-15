using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Logging;

namespace UnityEditor.TestTools.TestRunner
{
    internal abstract class TestLauncherBase
    {
        public abstract void Run();

        protected virtual RuntimePlatform TestTargetPlatform => Application.platform;

        protected bool ExecutePreBuildSetupMethods(ITest tests, ITestFilter testRunnerFilter)
        {
            using (new ProfilerMarker(nameof(ExecutePreBuildSetupMethods)).Auto()) {
                var logString = "Executing setup for: {0}";
                return ExecuteMethods<IPrebuildSetup>(tests, testRunnerFilter, new PrebuildSetupAttributeFinder(), logString, targetClass => targetClass.Setup(), TestTargetPlatform);
            }
        }

        protected bool ExecutePrebuildSetupWithTestDataMethods(ITest tests, ITestFilter testRunnerFilter)
        {
            using (new ProfilerMarker(nameof(ExecutePreBuildSetupMethods)).Auto()) {
                var logString = "Executing setup for: {0}";
                List<ITest> filteredTests = new List<ITest>();
                TestFiltering.GetMatchingTests(tests, testRunnerFilter, ref filteredTests, TestTargetPlatform);
                var testRun = new TestData(TestMode.Player, TestTargetPlatform, filteredTests);
                return ExecuteMethods<IPrebuildSetupWithTestData>(tests, testRunnerFilter, new PrebuildSetupWithTestDataAttributeFinder(), logString, targetClass => targetClass.Setup(testRun), TestTargetPlatform);
            }
        }

        public void ExecutePostBuildCleanupMethods(ITest tests, ITestFilter testRunnerFilter)
        {
            using (new ProfilerMarker(nameof(ExecutePostBuildCleanupMethods)).Auto())
            {
                ExecutePostBuildCleanupWithTestDataMethods(tests, testRunnerFilter, TestTargetPlatform);
                ExecutePostBuildCleanupMethods(tests, testRunnerFilter, TestTargetPlatform);
            }
        }

        static void ExecutePostBuildCleanupMethods(ITest tests, ITestFilter testRunnerFilter, RuntimePlatform testTargetPlatform)
        {
            using (new ProfilerMarker(nameof(ExecutePostBuildCleanupMethods)).Auto()) {
                var attributeFinder = new PostbuildCleanupAttributeFinder();
                var logString = "Executing cleanup for: {0}";
                ExecuteMethods<IPostBuildCleanup>(tests, testRunnerFilter, attributeFinder, logString, targetClass => targetClass.Cleanup(), testTargetPlatform);
            }
        }

        static void ExecutePostBuildCleanupWithTestDataMethods(ITest tests, ITestFilter testRunnerFilter, RuntimePlatform testTargetPlatform)
        {
            using (new ProfilerMarker(nameof(ExecutePostBuildCleanupMethods)).Auto()) {
                var attributeFinder = new PostbuildCleanupWithTestDataAttributeFinder();
                var logString = "Executing cleanup for: {0}";
                List<ITest> filteredTests = new List<ITest>();
                TestFiltering.GetMatchingTests(tests, testRunnerFilter, ref filteredTests, testTargetPlatform);
                var testRun = new TestData(TestMode.Player, testTargetPlatform, filteredTests);
                ExecuteMethods<IPostbuildCleanupWithTestData>(tests, testRunnerFilter, attributeFinder, logString, targetClass => targetClass.Cleanup(testRun), testTargetPlatform);
            }
        }

        private static bool ExecuteMethods<T>(ITest tests, ITestFilter testRunnerFilter, AttributeFinderBase attributeFinder, string logString, Action<T> action, RuntimePlatform testTargetPlatform)
        {
            var exceptionsThrown = false;
            foreach (var targetClassType in attributeFinder.Search(tests, testRunnerFilter, testTargetPlatform))
            {
                try
                {
                    var targetClass = (T)Activator.CreateInstance(targetClassType);

                    Debug.LogFormat(logString, targetClassType.FullName);

                    using (var logScope = new LogScope())
                    {
                        action(targetClass);
                        logScope.EvaluateLogScope(true);
                    }
                }
                catch (InvalidCastException) {}
                catch (Exception e)
                {
                    Debug.LogException(e);
                    exceptionsThrown = true;
                }
            }

            return exceptionsThrown;
        }
    }
}
