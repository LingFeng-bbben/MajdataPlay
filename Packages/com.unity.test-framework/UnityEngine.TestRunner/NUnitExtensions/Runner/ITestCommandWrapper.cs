using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    /// <summary>
    /// Implement this interface to wrap test commands with custom behavior.
    /// </summary>
    public interface ITestCommandWrapper
    {
        /// <summary>
        /// The order in which this wrapper is applied relative to other wrappers.
        /// Lower values are applied first (closer to the test command), higher values are applied last (outer wrapper).
        /// Outer wrappers execute their pre-test logic first and post-test logic last.
        /// </summary>
        int Order { get; }
        /// <summary>
        /// Determines whether the wrapper should be applied to the given test method.
        /// </summary>
        /// <param name="test">The test method to evaluate.</param>
        /// <returns>True if the wrapper should wrap this test; otherwise, false.</returns>
        bool ShouldWrap(TestMethod test);
        /// <summary>
        /// Wraps the given test command with custom behavior.
        /// </summary>
        /// <param name="command">The test command to wrap.</param>
        /// <returns>A new test command that wraps the original command.</returns>
        TestCommand Wrap(TestCommand command);
    }
}
