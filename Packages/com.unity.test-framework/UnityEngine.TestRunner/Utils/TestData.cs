using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Represents a test run configuration. This record holds information about the test run environment and the tests to be executed.
    /// </summary>
    /// <param name="TestMode">
    /// The Test Mode for this test run.
    /// </param>
    /// <param name="TestPlatform">
    /// The Test Platform for this test run.
    /// </param>
    /// <param name="TestList">
    /// The list of tests to be executed.
    /// </param>
    public record TestData(TestMode TestMode, RuntimePlatform TestPlatform, IEnumerable<ITest> TestList)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            var testList = TestList ?? Enumerable.Empty<ITest>();
            var testNames = testList.Select(t => t?.FullName ?? "null").ToList();

            var testListString = testNames.Count == 0
                ? "(empty)"
                : string.Join("\n\t\t- ", testNames);

            return $"TestData:\n\tTestMode: {TestMode}\n\tTestPlatform: {TestPlatform}\n\tTestList ({testNames.Count}):{(testNames.Count == 0 ? " (empty)" : $"\n\t\t- {testListString}")}";
        }
    }
}
