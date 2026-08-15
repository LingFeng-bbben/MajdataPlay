using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class PlayModeRunTask : TestTaskBase
    {
        public PlayModeRunTask()
        {
            SupportsResumingEnumerator = true;
        }
        public override IEnumerator Execute(TestJobData testJobData)
        {
            yield return null; // Allow for setting the test job data after a resume.

            // Saving of the scene causes serialization of the runner, so the events needs to be resubscribed. This is temporary for now.
            // Wait for the active controller
            while (PlaymodeTestsController.ActiveController == null)
            {
                yield return null;
            }

            var controller = PlaymodeTestsController.ActiveController;

            if (controller.m_Runner != null && controller.m_Runner.IsTestComplete)
            {
                //Already finished, likely zero tests.
                testJobData.RunStartedEvent.Invoke(controller.m_Runner.LoadedTest);
                testJobData.RunFinishedEvent.Invoke(controller.m_Runner.Result);
                yield break;
            }
            
            controller.runStartedEvent.AddListener(testJobData.RunStartedEvent.Invoke);
            controller.testStartedEvent.AddListener(testJobData.TestStartedEvent.Invoke);
            controller.testFinishedEvent.AddListener(testJobData.TestFinishedEvent.Invoke);
            controller.runFinishedEvent.AddListener(testJobData.RunFinishedEvent.Invoke);

            controller.RunInfrastructureHasRegistered = true;

            var runDone = false;
            controller.runFinishedEvent.AddListener((_) =>
            {
                runDone = true;
            });

            while (!runDone)
            {
                if (controller.RaisedException != null)
                {
                    throw controller.RaisedException;
                }

                if (!Application.isPlaying)
                {
                    throw new Exception("Playmode tests were aborted because the player was stopped.");
                }
                
                yield return null;
            }

            yield return null;
        }
    }
}
