using System;
using System.Collections.Generic;
using UnityEditor.TestTools.TestRunner.Api;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal interface ITestRunnerApiMapper
    {
        string GetRunStateFromResultNunitXml(ITestResultAdaptor result);
        TestState GetTestStateFromResult(ITestResultAdaptor result);
        List<string> FlattenTestNames(ITestAdaptor testsToRun);
        TestPlanMessage MapTestToTestPlanMessage(ITestAdaptor testsToRun);
        TestStartedMessage MapTestToTestStartedMessage(ITestAdaptor test);
        SuiteStartedMessage MapSuiteToSuiteStartedMessage(ITestAdaptor test);
        SuiteStartedMessage MapToRunRootStartedMessage();
        SuiteFinishedMessage MapToRunRootFinishedMessage(ITestResultAdaptor result);
        TestFinishedMessage TestResultToTestFinishedMessage(ITestResultAdaptor result);
        SuiteFinishedMessage TestResultToSuiteFinishedMessage(ITestResultAdaptor result);
    }
}
