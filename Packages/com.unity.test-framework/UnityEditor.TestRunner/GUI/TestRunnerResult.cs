using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class TestRunnerResult : UITestRunnerFilter.IClearableResult
    {
        public string id;
        public string uniqueId;
        public string name;
        public string fullName;
        public ResultStatus resultStatus = ResultStatus.NotRun;
        public float duration;
        public string messages;
        public string output;
        public string stacktrace;
        public bool notRunnable;
        public bool ignoredOrSkipped;
        public string description;
        public bool isSuite;
        public List<string> categories;
        public string parentId;
        public string parentUniqueId;

        //This field is suppose to mark results from before domain reload
        //Such result is outdated because the code might haev changed
        //This field will get reset every time a domain reload happens
        [NonSerialized]
        public bool notOutdated;

        protected Action<TestRunnerResult> m_OnResultUpdate;

        internal TestRunnerResult(ITestAdaptor test)
        {
            id = test.Id;
            uniqueId = test.UniqueName;

            fullName = test.FullName;
            name = test.Name;
            description = test.Description;
            isSuite = test.IsSuite;

            ignoredOrSkipped = test.RunState == RunState.Ignored || test.RunState == RunState.Skipped;
            notRunnable = test.RunState == RunState.NotRunnable;

            if (ignoredOrSkipped)
            {
                resultStatus = ResultStatus.Skipped;
                messages = test.SkipReason;
            }
            if (notRunnable)
            {
                resultStatus = ResultStatus.Failed;
                messages = test.SkipReason;
            }
            categories = test.Categories.ToList();
            parentId = test.ParentId;
            parentUniqueId = test.ParentUniqueName;
        }

        internal TestRunnerResult(ITestResultAdaptor testResult) : this(testResult.Test)
        {
            notOutdated = true;

            messages = testResult.Message;
            output = testResult.Output;
            stacktrace = testResult.StackTrace;
            duration = (float)testResult.Duration;
            if (testResult.Test.IsSuite && testResult.ResultState == "Ignored")
            {
                resultStatus = ResultStatus.Passed;
            }
            else
            {
                resultStatus = ParseNUnitResultStatus(testResult.TestStatus);
            }
        }

        public void CalculateParentResult(string parentId, IDictionary<string, List<TestRunnerResult>> results)
        {
            if (results == null || resultStatus == ResultStatus.Failed) return;

            results.TryGetValue(parentId, out var childrenResult);
            if (childrenResult == null) return;

            var dominantStatus = ResultStatus.Skipped;
            float totalDuration = 0f;
            foreach (var child in childrenResult)
            {
                if (StatusPriority(child.resultStatus) > StatusPriority(dominantStatus))
                    dominantStatus = child.resultStatus;
                totalDuration += child.duration;
            }

            resultStatus = dominantStatus;
            UpdateParentResult(results);
            duration = totalDuration;
        }

        public void CalculateAndSetParentDuration(string parentId, IDictionary<string, List<TestRunnerResult>> results)
        {
            if (results == null) return;
            results.TryGetValue(parentId , out var childrenResult);
            if (childrenResult == null) return;
            var totalDuration = childrenResult.Sum(x => x.duration);
            duration = (float)totalDuration;
        }

        private void UpdateParentResult(IDictionary<string, List<TestRunnerResult>> results)
        {
            if (string.IsNullOrEmpty(parentUniqueId)) return;
            results.TryGetValue(parentUniqueId, out var parentResultList);
            if (parentResultList != null && parentResultList.Count > 0)
            {
                parentResultList.Add(this);
            }
            else
            {
                results.Add(parentUniqueId, new List<TestRunnerResult> {this});
            }
        }

        public void Update(TestRunnerResult result)
        {
            if (ReferenceEquals(result, null))
                return;
            resultStatus = result.resultStatus;
            duration = result.duration;
            messages = result.messages;
            output = result.output;
            stacktrace = result.stacktrace;
            ignoredOrSkipped = result.ignoredOrSkipped;
            notRunnable = result.notRunnable;
            description = result.description;
            notOutdated = result.notOutdated;
            if (m_OnResultUpdate != null)
            {
                m_OnResultUpdate(this);
            }
        }

        public void SetResultChangedCallback(Action<TestRunnerResult> resultUpdated)
        {
            m_OnResultUpdate = resultUpdated;
        }

        [Serializable]
        internal enum ResultStatus
        {
            NotRun,
            Passed,
            Failed,
            Inconclusive,
            Skipped
        }

        private static int StatusPriority(ResultStatus status) => status switch
        {
            ResultStatus.NotRun => 4,
            ResultStatus.Failed => 3,
            ResultStatus.Inconclusive => 2,
            ResultStatus.Passed => 1,
            _ => 0
        };

        private static ResultStatus ParseNUnitResultStatus(TestStatus status)
        {
            switch (status)
            {
                case TestStatus.Passed:
                    return ResultStatus.Passed;
                case TestStatus.Failed:
                    return ResultStatus.Failed;
                case TestStatus.Inconclusive:
                    return ResultStatus.Inconclusive;
                case TestStatus.Skipped:
                    return ResultStatus.Skipped;
                default:
                    return ResultStatus.NotRun;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", name, fullName);
        }

        public string Id { get { return uniqueId; } }
        public string FullName { get { return fullName; } }
        public string ParentId { get { return parentUniqueId; } }
        public bool IsSuite { get { return isSuite; } }
        public List<string> Categories { get { return categories; } }

        public void Clear()
        {
            resultStatus = ResultStatus.NotRun;
            stacktrace = string.Empty;
            duration = 0.0f;
            if (m_OnResultUpdate != null)
                m_OnResultUpdate(this);
        }
    }
}
