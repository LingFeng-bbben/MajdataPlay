using System;

namespace UnityEditor.TestRunner.UnityTestProtocol
{
    /// <summary>
    /// Represents the data for a test run.
    /// </summary>
    [Serializable]
    public class TestRunData
    {
        /// <summary>
        /// The name of the test suite.
        /// </summary>
        public string SuiteName;

        /// <summary>
        /// The names of the tests in the fixture.
        /// </summary>
        public string[] TestsInFixture;

        /// <summary>
        /// The duration of the one-time setup.
        /// </summary>
        public long OneTimeSetUpDuration;

        /// <summary>
        /// The duration of the one-time teardown.
        /// </summary>
        public long OneTimeTearDownDuration;
    }
}
