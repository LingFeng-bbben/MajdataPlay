using System;

namespace UnityEngine.TestRunner.TestProtocol
{
    internal class TestStartedMessage : MessageForRetryRepeat
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

        public TestStartedMessage(string testName, int iteration = 0) : this()
        {
            name = testName;
            spanId = DeterministicGuid.Create(testName, iteration);
        }
    }
}