using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI;

namespace UnityEditor.TestTools.TestRunner.UnityTestProtocol
{
    internal class TestRunnerApiMapper : ITestRunnerApiMapper
    {
        internal IGuiHelper guiHelper =  new GuiHelper(new MonoCecilHelper(), new AssetsDatabaseHelper());
        private readonly string _projectRepoPath;
        private readonly Dictionary<string, int> _iterationCounters = new Dictionary<string, int>();

        // Bridges the spanId from TestStarted to TestFinished. The Begin-side id uses _iterationCounters
        // (incremented per TestStarted), not RetryIteration, which stays 0 for skipped/inconclusive re-runs
        // and would diverge. Only leaf tests use this bridge — editor tests run serially, so a single leaf
        // is in flight at a time, which is why the bridge is a single in-flight span rather than a collection.
        // (Suites emitted by UtpMessageReporter get pure-deterministic ids — DeterministicGuid(suiteKey, 0,
        // salt) on both Begin and End, since a suite is entered exactly once per run — so Begin == End
        // reconciles without a bridge and suites never enter _inFlightSpan. The single-field invariant holds.)
        //
        // A domain reload (e.g. [InPlayMode] entering play mode) wipes in-memory state, so both are persisted
        // via _stateStore (a ScriptableSingleton, the test runner's standard reload-surviving mechanism): the
        // in-flight span so the post-reload End reuses its Begin's id, and _iterationCounters so each retry's
        // Begin keeps a distinct id instead of recomputing the same one (duplicate Begins render nested in the
        // consumer; see UTR-1268).
        private SpanIdEntry _inFlightSpan;

        // Per-process salt ensures spanIds are unique across editor runs within the same UTR session.
        // Without this, tests with identical FullNames in different packages (e.g. PackageIsolationTests)
        // produce colliding spanIds, corrupting the consumer's message tree.
        // Uses process ID because it survives domain reloads (same process) but differs across
        // editor launches (different processes).
        private readonly string _runSalt = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

        private readonly ISpanIdStateStore _stateStore;

        // Cached so the ~2N SaveState() calls in a run mutate one container in place instead of allocating
        // a fresh state object + list each call (which would be O(N^2) garbage in a large suite).
        private readonly SpanIdReloadState _reloadState;

        public TestRunnerApiMapper(string projectRepoPath, ISpanIdStateStore stateStore = null)
        {
            _projectRepoPath = projectRepoPath;
            _stateStore = stateStore ?? new ScriptableSingletonSpanIdStore();
            _reloadState = _stateStore.Load() ?? new SpanIdReloadState();
            LoadState();
        }

        public TestPlanMessage MapTestToTestPlanMessage(ITestAdaptor testsToRun)
        {
            _iterationCounters.Clear();
            _inFlightSpan = default;
            SaveState();
            var testsNames = testsToRun != null ? FlattenTestNames(testsToRun) : new List<string>();

            var msg = new TestPlanMessage
            {
                tests = testsNames
            };

            return msg;
        }

        public TestStartedMessage MapTestToTestStartedMessage(ITestAdaptor test)
        {
            var spanKey = test.UniqueName ?? test.FullName;
            _iterationCounters.TryGetValue(spanKey, out var iteration);
            _iterationCounters[spanKey] = iteration + 1;

            var spanId = DeterministicGuid.Create(spanKey, iteration, _runSalt);
            _inFlightSpan = new SpanIdEntry { spanKey = spanKey, spanId = spanId };
            SaveState();

            return new TestStartedMessage
            {
                name = test.FullName,
                spanId = spanId,
                parentSpanId = ResolveParentSpanId(test)
            };
        }

        // The single run-level root suite, emitted once from Run Started/Finished (see
        // UtpMessageReporter). It is keyed on the project repository path rather than the NUnit
        // synthetic root: that root's name is Application.productName, which is the project-folder
        // name on a cold launch but the ProjectSettings value after the first domain reload rebuilds
        // the test tree — so the tree-walked root would otherwise be emitted twice under two
        // different names and the consumer could not pair Begin with End. _projectRepoPath comes from
        // the -projectRepositoryPath command line and is identical across domain reloads, so this id
        // is stable; Run Started/Finished do not re-fire after a reload, so it is emitted exactly once.
        //
        // parentSpanId is intentionally omitted (left null/empty) so the consumer infers it from
        // the live span context (MessageContext.CurrentSpanId = ProcessInfo at the time RunStarted
        // fires). Setting it to Guid.Empty.ToString() would instead place RunRoot as a sibling of the
        // UTR-generated playmode TestSuite rather than nesting it inside the editor ProcessInfo span.
        string RunRootSpanId() => DeterministicGuid.Create(_projectRepoPath ?? string.Empty, 0, _runSalt);

        string RunRootName() =>
            string.IsNullOrEmpty(_projectRepoPath) ? "TestRun" : Path.GetFileName(_projectRepoPath.TrimEnd('/', '\\'));

        public SuiteStartedMessage MapToRunRootStartedMessage() =>
            new SuiteStartedMessage
            {
                name = RunRootName(),
                spanId = RunRootSpanId(),
            };

        public SuiteFinishedMessage MapToRunRootFinishedMessage(ITestResultAdaptor result) =>
            new SuiteFinishedMessage
            {
                name = RunRootName(),
                duration = result != null && result.Duration > 0 ? (ulong)(result.Duration * 1000) : 0,
                spanId = RunRootSpanId(),
            };

        // Link each node to its parent's spanId on the producer side rather than relying on the
        // consumer inferring the parent from its span-stack ordering. A parent is always a suite,
        // and a suite is entered exactly once per run (retries/repeats re-run only the leaf test,
        // never the enclosing suite; parameterized fixtures get distinct UniqueNames), so its
        // iteration is always 0 and its spanId is fully determined by its span key + run salt — the
        // very value it computed for itself at its Begin. So reconstruct it deterministically rather
        // than tracking live open-span state: the link is identical in the normal case but, crucially,
        // also survives a domain reload (which wipes in-memory state) without the producer fabricating
        // a dangling id or the consumer inferring anything.
        //
        // The key mirrors the own-id keying in the Map* methods (UniqueName, else FullName).
        string ResolveParentSpanId(ITestAdaptor test)
        {
            // Test assemblies are children of the suppressed run container; re-home them onto the
            // stable run-level root. IsTestAssembly is reload-reliable, unlike the Parent reference.
            if (test.IsTestAssembly)
                return RunRootSpanId();

            // A kept top-level suite (e.g. a scene group) also attaches to the run-level root, but only
            // when truly parentless. A post-reload adaptor can have a null Parent yet carry parent names
            // (not top-level) — so require the parent-name fields empty too, else reconstruct below.
            if (test.Parent == null
                && string.IsNullOrEmpty(test.ParentUniqueName)
                && string.IsNullOrEmpty(test.ParentFullName))
                return RunRootSpanId();

            var parentSpanKey = test.ParentUniqueName ?? test.ParentFullName;
            if (string.IsNullOrEmpty(parentSpanKey))
                return Guid.Empty.ToString();
            return DeterministicGuid.Create(parentSpanKey, 0, _runSalt);
        }

        public SuiteStartedMessage MapSuiteToSuiteStartedMessage(ITestAdaptor test)
        {
            return new SuiteStartedMessage
            {
                name = FormatTestName(test.FullName),
                spanId = SuiteSpanId(test),
                parentSpanId = ResolveParentSpanId(test)
            };
        }

        // A suite's spanId is pure-deterministic and the suite never enters _inFlightSpan: a suite is
        // entered exactly once per run, so its iteration is always 0 and DeterministicGuid(spanKey, 0,
        // salt) reconciles Begin == End on its own — including across a domain reload, since spanKey
        // (UniqueName), the literal 0, and the process salt are all reload-stable. Keeping suites off
        // the bridge (and out of _iterationCounters) preserves the single-leaf-in-flight invariant.
        //
        // Key on UniqueName, not FullName: suite FullNames are not unique (a namespace like "Tests"
        // recurs in every assembly), whereas UniqueName is assembly-prefixed (e.g. "Foo.dll/Tests/...").
        // The literal 0 also enforces "suites are iteration 0" rather than leaving it incidental.
        string SuiteSpanId(ITestAdaptor test) =>
            DeterministicGuid.Create(test.UniqueName ?? test.FullName, 0, _runSalt);

        // Strip the directory path when the suite name is a DLL path so the consumer shows the
        // assembly name rather than the full on-disk path.
        static string FormatTestName(string name) =>
            !string.IsNullOrEmpty(name) && name.EndsWith(".dll", System.StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(name) : name;

        public TestFinishedMessage TestResultToTestFinishedMessage(ITestResultAdaptor result)
        {
            string filePathString = default;
            int lineNumber = default;
            if (result.Test.Method != null && result.Test.TypeInfo != null)
            {
                var method = result.Test.Method.MethodInfo;
                var type = result.Test.TypeInfo.Type;
                var fileOpenInfo = guiHelper.GetFileOpenInfo(type, method);
                filePathString = !string.IsNullOrEmpty(_projectRepoPath) ? Path.Combine(_projectRepoPath, fileOpenInfo.FilePath) : fileOpenInfo.FilePath;
                lineNumber = fileOpenInfo.LineNumber;
            }

            var iteration = 0;
            if(result is TestResultAdaptor)
            {
                var adaptor = ((TestResultAdaptor)result);
                iteration = adaptor.RepeatIteration == 0 ? adaptor.RetryIteration : adaptor.RepeatIteration;
            }

            var spanKey = result.Test.UniqueName ?? result.Test.FullName;
            string spanId;
            if (_inFlightSpan.spanKey == spanKey)
            {
                spanId = _inFlightSpan.spanId;
                _inFlightSpan = default;
                SaveState();
            }
            else
                spanId = DeterministicGuid.Create(spanKey, iteration, _runSalt);

            return new TestFinishedMessage
            {
                name = result.Test.FullName,
                duration = Convert.ToUInt64(result.Duration * 1000),
                durationMicroseconds = Convert.ToUInt64(result.Duration * 1000000),
                message = result.Message,
                state = GetTestStateFromResult(result),
                stackTrace = result.StackTrace,
                fileName = filePathString,
                lineNumber = lineNumber,
                iteration = iteration,
                spanId = spanId,
                parentSpanId = ResolveParentSpanId(result.Test)
            };
        }

        public SuiteFinishedMessage TestResultToSuiteFinishedMessage(ITestResultAdaptor result)
        {
            return new SuiteFinishedMessage
            {
                name = FormatTestName(result.Test.FullName),
                // Guard against negative/NaN durations (precision, skipped suites): Convert.ToUInt64
                // throws OverflowException on those, whereas the guarded cast yields 0.
                duration = result.Duration > 0 ? (ulong)(result.Duration * 1000) : 0,
                // Recompute deterministically — matches the Begin without the bridge (see SuiteSpanId).
                spanId = SuiteSpanId(result.Test),
                parentSpanId = ResolveParentSpanId(result.Test)
            };
        }

        public string GetRunStateFromResultNunitXml(ITestResultAdaptor result)
        {
            var doc = new XmlDocument();
            doc.LoadXml(result.ToXml().OuterXml);
            return doc.FirstChild.Attributes["runstate"].Value;
        }

        public TestState GetTestStateFromResult(ITestResultAdaptor result)
        {
            var state = TestState.Failure;

            if (result.TestStatus == TestStatus.Passed)
            {
                state = TestState.Success;
            }
            else if (result.TestStatus == TestStatus.Skipped)
            {
                state = TestState.Skipped;

                if (result.ResultState.ToLowerInvariant().EndsWith("ignored"))
                {
                    state = TestState.Ignored;
                }
            }
            else
            {
                if (result.ResultState.ToLowerInvariant().Equals("inconclusive"))
                {
                    state = TestState.Inconclusive;
                }

                if (result.ResultState.ToLowerInvariant().EndsWith("cancelled") ||
                    result.ResultState.ToLowerInvariant().EndsWith("error"))
                {
                    state = TestState.Error;
                }
            }

            return state;
        }

        public List<string> FlattenTestNames(ITestAdaptor test)
        {
            var results = new List<string>();

            if (!test.IsSuite)
                results.Add(test.FullName);

            if (test.Children != null && test.Children.Any())
                foreach (var child in test.Children)
                    results.AddRange(FlattenTestNames(child));

            return results;
        }

        private void LoadState()
        {
            _inFlightSpan = _reloadState.inFlightSpan;
            _iterationCounters.Clear();
            if (_reloadState.iterationCounters != null)
                foreach (var entry in _reloadState.iterationCounters)
                    _iterationCounters[entry.spanKey] = entry.count;
        }

        // Mutates the cached _reloadState in place — Clear() keeps the list's capacity — so a run's ~2N
        // SaveState() calls allocate nothing after warm-up, instead of a new state object + list each time.
        private void SaveState()
        {
            _reloadState.inFlightSpan = _inFlightSpan;
            _reloadState.iterationCounters.Clear();
            foreach (var kvp in _iterationCounters)
                _reloadState.iterationCounters.Add(new IterationEntry { spanKey = kvp.Key, count = kvp.Value });

            _stateStore.Save(_reloadState);
        }
    }

    // Shared classification of nodes in the NUnit adaptor tree.
    internal static class TestTree
    {
        // The Unity-generated run container (productName-derived, reload-unstable) only holds the test
        // assemblies, so it is suppressed and replaced by the stable RunRoot. Detected as a top-level
        // suite whose children are all assemblies — not by empty parent name alone, which also matches a
        // meaningful parentless top (e.g. a scene group with a [SetUpFixture] child) that must stay emitted.
        public static bool IsDisposableRunContainer(ITestAdaptor test) =>
            IsDisposableRunContainer(test, test.Children);

        // Finish-path overload: remote finish builds result.Test without ApplyChildren(), so
        // result.Test.Children is null — take children from the result tree (populated on every path),
        // else the container's End escapes suppression as an orphan.
        public static bool IsDisposableRunContainer(ITestResultAdaptor result) =>
            IsDisposableRunContainer(result.Test, result.Children?.Select(child => child.Test));

        static bool IsDisposableRunContainer(ITestAdaptor test, IEnumerable<ITestAdaptor> children)
        {
            // A test assembly is never the container (its children are fixtures, not assemblies).
            if (test.IsTestAssembly)
                return false;

            // Key top-level on the parent-name fields, not Parent: remote paths never set the Parent
            // object, and a post-reload adaptor can have null Parent yet carry parent names.
            if (!string.IsNullOrEmpty(test.ParentUniqueName) || !string.IsNullOrEmpty(test.ParentFullName))
                return false;

            // Container = no non-assembly child (assembly-only OR empty). Empty counts because on a
            // filtered run the container's Begin sees the assemblies but its finish-side tree drops them;
            // keying on "no non-assembly child" keeps Begin and End on the same verdict, so the End can't
            // orphan. (A scene group, by contrast, carries its non-assembly child through both phases.)
            if (children != null)
                foreach (var child in children)
                    if (!child.IsTestAssembly)
                        return false;

            return true;
        }
    }

    // Persists the reload-surviving state. Abstracted so tests can supply an in-memory store.
    internal interface ISpanIdStateStore
    {
        SpanIdReloadState Load();
        void Save(SpanIdReloadState state);
    }

    // Unity can't serialize a Dictionary, so the counters are stored as an entry list.
    [Serializable]
    internal class SpanIdReloadState
    {
        public SpanIdEntry inFlightSpan;
        public List<IterationEntry> iterationCounters = new List<IterationEntry>();
    }

    [Serializable]
    internal struct SpanIdEntry
    {
        public string spanKey;
        public string spanId;
    }

    [Serializable]
    internal struct IterationEntry
    {
        public string spanKey;
        public int count;
    }

    // ScriptableSingleton survives domain reloads (cf. CallbacksHolder, RunData), rehydrating [SerializeField] state.
    internal class ScriptableSingletonSpanIdStore : ISpanIdStateStore
    {
        public SpanIdReloadState Load() => SpanIdStateHolder.instance.State;

        public void Save(SpanIdReloadState state) => SpanIdStateHolder.instance.State = state;
    }

    internal class SpanIdStateHolder : ScriptableSingleton<SpanIdStateHolder>
    {
        [UnityEngine.SerializeField]
        private SpanIdReloadState _state = new SpanIdReloadState();

        public SpanIdReloadState State
        {
            get => _state;
            set => _state = value;
        }
    }
}
