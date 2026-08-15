using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PostbuildCleanupWithTestDataAttributeFinder : AttributeFinderBase<IPostbuildCleanupWithTestData, PostBuildCleanupWithTestDataAttribute>
    {
        public PostbuildCleanupWithTestDataAttributeFinder() : base(attribute => attribute.TargetClass) { }
    }
}
