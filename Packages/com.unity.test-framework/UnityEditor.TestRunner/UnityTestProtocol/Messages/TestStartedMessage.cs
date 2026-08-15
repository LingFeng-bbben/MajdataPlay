using System;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class TestStartedMessage : Message
    {
        public string name;
        public TestState state;
        public string spanId;
        public string parentSpanId;

        public TestStartedMessage()
        {
            type = "TestStatus";
            phase = "Begin";
            state = TestState.Inconclusive;
        }
    }
}
