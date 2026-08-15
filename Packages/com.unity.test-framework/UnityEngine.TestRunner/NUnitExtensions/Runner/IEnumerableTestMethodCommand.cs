using System;
using System.Collections;
using NUnit.Framework.Internal;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    /// <summary>
    /// Implement this interface on a test command to support coroutine-style test execution.
    /// </summary>
    public interface IEnumerableTestMethodCommand
    {
        /// <summary>
        /// Executes the test command as an enumerable, allowing for frame-by-frame execution.
        /// </summary>
        /// <param name="context">The test execution context.</param>
        /// <returns>An enumerable that yields control back to the test runner between steps.</returns>
        IEnumerable ExecuteEnumerable(ITestExecutionContext context);
    }
}
