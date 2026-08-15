using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PrebuildSetupWithTestDataAttributeFinder : AttributeFinderBase<IPrebuildSetupWithTestData, PrebuildSetupWithTestDataAttribute>
    {
        public PrebuildSetupWithTestDataAttributeFinder() : base(attribute => attribute.TargetClass) { }
    }
}
