using System.Collections;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System.Linq;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class ParametrizedIgnoreCommand : DelegatingTestCommand, IEnumerableTestMethodCommand
    {
        private readonly TestCommand m_Command;

        public object[] Arguments { get; }
        public string Reason { get; }

        public ParametrizedIgnoreCommand(TestCommand command, object[] arguments, string reason = "") : base(command)
        {
            m_Command = command;
            Arguments = arguments;
            Reason = reason;
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            if (context.CurrentTest is TestMethod testMethod)
            {
                var isIgnored = testMethod.parms.Arguments.SequenceEqual(Arguments);
                if (isIgnored)
                {
                    context.CurrentResult.SetResult(ResultState.Ignored, Reason);
                    return context.CurrentResult;
                }
            }

            return m_Command.Execute(context);
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        { 
            if (context.CurrentTest is TestMethod testMethod)
            {
                var isIgnored = testMethod.parms.Arguments.SequenceEqual(Arguments);
                if (isIgnored)
                {
                    context.CurrentResult.SetResult(ResultState.Ignored, Reason);
                    yield break;
                }
            }
            
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
        }
    }
}
