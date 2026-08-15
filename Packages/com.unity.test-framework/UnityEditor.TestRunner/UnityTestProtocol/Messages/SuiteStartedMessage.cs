namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class SuiteStartedMessage : Message
    {
        public string name;
        public string minimalCommandLine;
        public string scope;
        public string platform;
        public string spanId;
        // Set by the producer to the open parent's spanId; Empty only for the root suite, which
        // the consumer treats as "unset" and attaches to its own run/session span.
        public string parentSpanId;

        public SuiteStartedMessage()
        {
            // Reported as a TestGroup, not a TestSuite: every node UTF emits (RunRoot, assembly,
            // namespace, fixture) is nested inside the single UTR-generated run suite, so it is a
            // grouping/fixture, not a run. Only the run suite stays a TestSuite; keeping these as
            // groups is what stops them flooding the consumer's suite list.
            type = "TestGroup";
            phase = "Begin";
        }
    }
}
