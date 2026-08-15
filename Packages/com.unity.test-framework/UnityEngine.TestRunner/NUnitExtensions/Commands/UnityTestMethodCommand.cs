using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal class UnityTestMethodCommand : TestMethodCommand
    {
        public UnityTestMethodCommand(TestMethod testMethod)
            : base(testMethod) {}

        public override TestResult Execute(ITestExecutionContext context)
        {
            using (new ProfilerMarker(Test.FullName).Auto())
            {
                var result = base.Execute(context);

                if (((UnityTestExecutionContext) context).HasTimedOut())
                {
                    context.CurrentResult.RecordException(new UnityTestTimeoutException(context.TestCaseTimeout));
                }
                
                return result;
            }
        }
    }
}
