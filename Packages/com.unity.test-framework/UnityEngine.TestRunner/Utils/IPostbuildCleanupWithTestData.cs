namespace UnityEngine.TestTools
{
    /// <summary>
    /// Implement this interface if you want to define a set of actions to execute as a post-build step. Cleanup runs right away for a standalone test run, but only after all the tests run within the Editor.
    /// </summary>
    public interface IPostbuildCleanupWithTestData
    {
        /// <summary>
        /// The code that will run as a post-build cleanup step.
        /// </summary>
        /// <param name="testData">
        /// The test data associated with the test run that just completed.
        /// </param>
        void Cleanup(TestData testData);
    }
}
