using System;
using System.Collections;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class ClearDeveloperConsoleTask : TestTaskBase
    {
        internal Action ClearDeveloperConsole = Debug.ClearDeveloperConsole;

        public override IEnumerator Execute(TestJobData testJobData)
        {
            ClearDeveloperConsole();
            yield break;
        }
    }
}
