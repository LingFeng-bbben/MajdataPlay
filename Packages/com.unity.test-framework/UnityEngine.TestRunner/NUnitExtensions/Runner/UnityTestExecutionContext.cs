using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using UnityEngine.TestTools;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    internal class UnityTestExecutionContext : ITestExecutionContext
    {
        private readonly UnityTestExecutionContext _priorContext;
        private TestResult _currentResult;
        private int _assertCount;

        // CurrentContext is intentionally NOT reset in SubsystemRegistration for Fast Enter Play Mode (FEPM).
        // SubsystemRegistration fires on every play mode entry, including FEPM entries that skip the domain reload.
        // When edit mode tests enter play mode via EnterPlayMode(), EditModeRunner.TestConsumer continues running
        // and calls IsCancelled() which reads CurrentContext — wiping it here would cause a NullReferenceException.
        // CurrentContext does not need to be reset here because the play mode test runner always sets it fresh
        // via GenerateContextTask before any play mode tests execute.
#pragma warning disable UDR0001
        public static UnityTestExecutionContext CurrentContext { get; set; }
#pragma warning restore UDR0001

        public UnityTestExecutionContext Context { get; private set; }

        public bool Automated { get; set; }

        public Test CurrentTest { get; set; }
        public DateTime StartTime { get; set; }
        public long StartTicks { get; set; }
        public TestResult CurrentResult
        {
            get { return _currentResult; }
            set
            {
                _currentResult = value;
                if (value != null)
                    OutWriter = value.OutWriter;
            }
        }

        public object TestObject { get; set; }
        public string WorkDirectory { get; set; }


        private TestExecutionStatus _executionStatus;
        public TestExecutionStatus ExecutionStatus
        {
            get
            {
                // ExecutionStatus may have been set to StopRequested or AbortRequested
                // in a prior context. If so, reflect the same setting in this context.
                if (_executionStatus == TestExecutionStatus.Running && _priorContext != null)
                    _executionStatus = _priorContext.ExecutionStatus;

                return _executionStatus;
            }
            set
            {
                _executionStatus = value;

                // Push the same setting up to all prior contexts
                if (_priorContext != null)
                    _priorContext.ExecutionStatus = value;
            }
        }

        public List<ITestAction> UpstreamActions { get; private set; }
        public int TestCaseTimeout { get; set; }
        public CultureInfo CurrentCulture { get; set; }
        public CultureInfo CurrentUICulture { get; set; }
        public ITestListener Listener { get; set; }

        public UnityTestExecutionContext()
        {
            UpstreamActions = new List<ITestAction>();
            SetUpTearDownState = new BeforeAfterTestCommandState();
            OneTimeSetUpTearDownState = new BeforeAfterTestCommandState();
            OuterUnityTestActionState = new BeforeAfterTestCommandState();
            EnumerableTestState = new EnumerableTestState();
            ErrorCount = GetErrorCount();
        }

        public UnityTestExecutionContext(BeforeAfterTestCommandState setUpTearDownState, BeforeAfterTestCommandState oneTimeSetUpTearDownState,
            BeforeAfterTestCommandState outerUnityTestActionState, EnumerableTestState enumerableTestState) : this()
        {
            SetUpTearDownState = setUpTearDownState;
            OneTimeSetUpTearDownState = oneTimeSetUpTearDownState;
            OuterUnityTestActionState = outerUnityTestActionState;
            EnumerableTestState = enumerableTestState;
        }

        public UnityTestExecutionContext(UnityTestExecutionContext other)
        {
            _priorContext = other;

            CurrentTest = other.CurrentTest;
            CurrentResult = other.CurrentResult;
            TestObject = other.TestObject;
            WorkDirectory = other.WorkDirectory;
            Listener = other.Listener;
            TestCaseTimeout = other.TestCaseTimeout;
            UpstreamActions = new List<ITestAction>(other.UpstreamActions);
            SetUpTearDownState = other.SetUpTearDownState;
            OneTimeSetUpTearDownState = other.OneTimeSetUpTearDownState;
            OuterUnityTestActionState = other.OuterUnityTestActionState;
            EnumerableTestState = other.EnumerableTestState;
            ErrorCount = other.ErrorCount;

            TestContext.CurrentTestExecutionContext = this;

            CurrentCulture = other.CurrentCulture;
            CurrentUICulture = other.CurrentUICulture;
            TestMode = other.TestMode;
            IgnoreTests = other.IgnoreTests;
            FeatureFlags = other.FeatureFlags;
            CurrentContext = this;
            Automated = other.Automated;
            RepeatCount = other.RepeatCount;
            RetryCount = other.RetryCount;
        }

        public TextWriter OutWriter { get; private set; }
        public bool StopOnError { get; set; }

        public IWorkItemDispatcher Dispatcher { get; set; }

        public ParallelScope ParallelScope { get; set; }
        public string WorkerId { get; private set; }
        public Randomizer RandomGenerator { get; private set; }
        public ValueFormatter CurrentValueFormatter { get; private set; }
        public bool IsSingleThreaded { get; set; }
        public BeforeAfterTestCommandState SetUpTearDownState { get; set; }
        public BeforeAfterTestCommandState OneTimeSetUpTearDownState { get; set; }
        public BeforeAfterTestCommandState OuterUnityTestActionState { get; set; }
        public EnumerableTestState EnumerableTestState { get; set; }
        public IgnoreTest[] IgnoreTests { get; set; }
        public FeatureFlags FeatureFlags { get; set; }
        public int RetryCount { get; set; }
        public int RepeatCount { get; set; }
        public EnumerableTestState RetryRepeatState { get; set; }
        public int ErrorCount { get; set; }
        public bool HasErrorsInDomainReload { get; set; }

        internal int AssertCount
        {
            get
            {
                return _assertCount;
            }
        }

        public TestPlatform TestMode { get; set; }

        public void IncrementAssertCount()
        {
            _assertCount += 1;
        }

        public void AddFormatter(ValueFormatterFactory formatterFactory)
        {
            throw new NotImplementedException();
        }

        public bool HasTimedOut()
        {
            return Stopwatch.GetTimestamp() - StartTicks >
                   TestCaseTimeout * (Stopwatch.Frequency / 1000f);
        }

        public bool HasErrorsAfterDomainReload()
        {
            var errorCount = GetErrorCount();

            var exceedsPrevious = errorCount > ErrorCount;
            ErrorCount = errorCount;

            return exceedsPrevious;
        }

        public int GetErrorCount()
        {
            int errorCount = 0;
#if UNITY_EDITOR
            int warningCount = 0, logCount = 0;
            UnityEditor.LogEntries.GetCountsByType(ref errorCount, ref warningCount, ref logCount);
#endif
            return errorCount;
        }
    }
}
