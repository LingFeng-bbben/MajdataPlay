using System.Collections;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class EnumerableMaxTimeCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private int maxTime;
        public EnumerableMaxTimeCommand(MaxTimeCommand commandToReplace) : base(commandToReplace.GetInnerCommand())
        {
            maxTime = (int)typeof(MaxTimeCommand)
                .GetField("maxTime", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(commandToReplace);
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new System.NotImplementedException("Use ExecuteEnumerable");
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            long timestamp = Stopwatch.GetTimestamp();
            
            if (innerCommand is IEnumerableTestMethodCommand)
            {
                var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
                foreach (var iterator in executeEnumerable)
                {
                    yield return iterator;
                }
            }
            else
            {
                context.CurrentResult = innerCommand.Execute(context);
            }
            
            var duration = (Stopwatch.GetTimestamp() - timestamp) / (double) Stopwatch.Frequency;
            var testResult = context.CurrentResult;
            testResult.Duration = duration;
            if (testResult.ResultState == ResultState.Success)
            {
                var durationInMilliseconds = duration * 1000.0;
                if (durationInMilliseconds > maxTime)
                    testResult.SetResult(ResultState.Failure, string.Format("Elapsed time of {0}ms exceeds maximum of {1}ms", (object) durationInMilliseconds, (object) this.maxTime));
            }
        }
    }
}