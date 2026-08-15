using System;

namespace UnityEngine.TestRunner.TestProtocol
{
    internal class TestFinishedMessage : MessageForRetryRepeat
    {
        public string name;
        public TestState state;
        public string message;
        public ulong duration; // milliseconds
        public ulong durationMicroseconds;
        public string stackTrace;
        public string spanId;
        public string parentSpanId;

        public TestFinishedMessage()
        {
            type = "TestStatus";
            phase = "End";
        }

        public TestFinishedMessage(string testName, int iteration = 0) : this()
        {
            name = testName;
            spanId = DeterministicGuid.Create(testName, iteration);
        }
    }
}