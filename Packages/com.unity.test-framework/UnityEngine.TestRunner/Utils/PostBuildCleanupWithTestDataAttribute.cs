using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// PostBuildCleanupWithTestData attributes run if the respective test or test class is in the current test run. The test is included either by running all tests or setting a filter that includes the test. If multiple tests reference the same pre-built setup or post-build cleanup, then it only runs once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public class PostBuildCleanupWithTestDataAttribute : Attribute
    {
        /// <summary>
        /// Initializes and returns an instance of PostBuildCleanupWithTestDataAttribute by type.
        /// </summary>
        /// <param name="targetClass">The type of the target class.</param>
        public PostBuildCleanupWithTestDataAttribute(Type targetClass)
        {
            TargetClass = targetClass;
        }

        /// <summary>
        /// Initializes and returns an instance of PostBuildCleanupWithTestDataAttribute by class name.
        /// </summary>
        /// <param name="targetClassName">The name of the target class.</param>
        public PostBuildCleanupWithTestDataAttribute(string targetClassName)
        {
            TargetClass = AttributeHelper.GetTargetClassFromName(targetClassName, typeof(IPostbuildCleanupWithTestData));
        }

        internal Type TargetClass { get; private set; }
    }
}
