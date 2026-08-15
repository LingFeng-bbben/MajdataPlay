using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events
{
    internal class RunFinishedInvocationEvent : TestTaskBase
    {
        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.TestResults == null)
            {
                // Temporary workaround to ensure that we do not loose the non serializable results due to a test leaking a domain reload.
                testJobData.TestResults = testJobData.editModeRunner.m_Runner.Result;
            }
            testJobData.editModeRunner.Dispose();

            testJobData.RunFinishedEvent.Invoke(testJobData.TestResults);
            yield break;
        }
    }
}
