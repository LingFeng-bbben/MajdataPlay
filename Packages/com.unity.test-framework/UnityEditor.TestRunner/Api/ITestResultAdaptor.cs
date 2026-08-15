using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;

namespace UnityEditor.TestTools.TestRunner.Api
{
    /// <summary>
    /// The `ITestResultAdaptor` is the representation of the test results for a node in the test tree implemented as a wrapper around the [NUnit](http://www.nunit.org/) [ITest](https://github.com/nunit/nunit/blob/master/src/NUnitFramework/framework/Interfaces/ITestResults.cs) interface.
    /// </summary>
    public interface ITestResultAdaptor
    {
        /// <summary>
        /// The test details of the test result tree node as a <see cref="TestAdaptor"/>
        /// </summary>
        ITestAdaptor Test { get; }
        ///<summary>
        ///The name of the test node.
        ///</summary>
        string Name { get; }
        /// <summary>
        /// Gets the full name of the test result
        /// </summary>
        ///<value>
        ///The name of the test result.
        ///</value>
        string FullName { get; }
        ///<summary>
        ///Gets the state of the result as a string.
        ///</summary>
        ///<value>
        ///It returns one of these values: `Inconclusive`, `Skipped`, `Skipped:Ignored`, `Skipped:Explicit`, `Passed`, `Failed`, `Failed:Error`, `Failed:Cancelled`, `Failed:Invalid`
        ///</value>
        string ResultState { get; }
        ///<summary>
        ///Gets the status of the test as an enum.
        ///</summary>
        ///<value>
        ///It returns one of these values:`Inconclusive`, `Skipped`, `Passed`, or `Failed`
        ///</value>
        TestStatus TestStatus { get; }
        /// <summary>
        /// Gets the elapsed time for running the test in seconds
        /// </summary>
        /// <value>
        /// Time in seconds.
        /// </value>
        double Duration { get; }
        /// <summary>
        /// Gets or sets the time the test started running.
        /// </summary>
        ///<value>
        ///A DataTime object.
        ///</value>
        DateTime StartTime { get; }
        ///<summary>
        ///Gets or sets the time the test finished running.
        ///</summary>
        ///<value>
        ///A DataTime object.
        ///</value>
        DateTime EndTime { get; }

        /// <summary>
        /// The message associated with a test failure or with not running the test
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Any stacktrace associated with an error or failure. Not available in the Compact Framework 1.0.
        /// </summary>
        string StackTrace { get; }

        /// <summary>
        /// The number of asserts executed when running the test and all its children.
        /// </summary>
        int AssertCount { get; }

        /// <summary>
        /// The number of test cases that failed when running the test and all its children.
        /// </summary>
        int FailCount { get; }

        /// <summary>
        /// The number of test cases that passed when running the test and all its children.
        /// </summary>
        int PassCount { get; }

        /// <summary>
        /// The number of test cases that were skipped when running the test and all its children.
        /// </summary>
        int SkipCount { get; }

        /// <summary>
        ///The number of test cases that were inconclusive when running the test and all its children.
        /// </summary>
        int InconclusiveCount { get; }

        /// <summary>
        /// Accessing HasChildren should not force creation of the Children collection in classes implementing this interface.
        /// </summary>
        /// <value>True if this result has any child results.</value>
        bool HasChildren { get; }

        /// <summary>
        /// Gets the the collection of child results.
        /// </summary>
        IEnumerable<ITestResultAdaptor> Children { get; }

        /// <summary>
        /// Gets any text output written to this result.
        /// </summary>
        string Output { get; }
        /// <summary>
        /// Use this to save the results to an XML file
        /// </summary>
        /// <returns>
        /// The test results as an `NUnit` XML node.
        /// </returns>
        TNode ToXml();
    }
}
