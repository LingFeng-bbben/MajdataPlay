namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class SuiteFinishedMessage : Message
    {
        public string name;
        public ulong duration; // milliseconds
        public string spanId;
        // Set by the producer to the open parent's spanId; Empty only for the root suite.
        public string parentSpanId;

        public SuiteFinishedMessage()
        {
            // Reported as a TestGroup, not a TestSuite — see SuiteStartedMessage: UTF's nodes are
            // groupings nested inside the run suite, not runs.
            type = "TestGroup";
            phase = "End";
        }
    }
}
