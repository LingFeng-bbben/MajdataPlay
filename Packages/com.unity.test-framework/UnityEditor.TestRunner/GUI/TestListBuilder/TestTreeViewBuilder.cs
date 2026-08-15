using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestRunner.NUnitExtensions.Filters;
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewController = UnityEditor.IMGUI.Controls.TreeViewController<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
using LazyTreeViewDataSource = UnityEditor.IMGUI.Controls.LazyTreeViewDataSource<int>;
using TreeViewUtility = UnityEditor.IMGUI.Controls.TreeViewUtility<int>;
using TreeViewSelectState = UnityEditor.IMGUI.Controls.TreeViewSelectState<int>;
using ITreeViewGUI = UnityEditor.IMGUI.Controls.ITreeViewGUI<int>;
using ITreeViewDragging = UnityEditor.IMGUI.Controls.ITreeViewDragging<int>;
using ITreeViewDataSource = UnityEditor.IMGUI.Controls.ITreeViewDataSource<int>;
using TreeViewGUI = UnityEditor.IMGUI.Controls.TreeViewGUI<int>;
using TreeViewDragging = UnityEditor.IMGUI.Controls.TreeViewDragging<int>;
using TreeViewDataSource = UnityEditor.IMGUI.Controls.TreeViewDataSource<int>;
using TreeViewItemAlphaNumericSort = UnityEditor.IMGUI.Controls.TreeViewItemAlphaNumericSort<int>;
using RenameOverlay = UnityEditor.RenameOverlay<int>;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal class TestTreeViewBuilder
    {
        internal struct TestCount
        {
            public int TotalTestCount;
            public int TotalFailedTestCount;
        }

        public List<TestRunnerResult> results = new List<TestRunnerResult>();
        public readonly Dictionary<string, TestTreeViewItem> m_treeFiltered = new Dictionary<string, TestTreeViewItem>();
        private readonly Dictionary<string, TestRunnerResult> m_OldTestResults;
        private readonly TestRunnerUIFilter m_UIFilter;
        private readonly ITestAdaptor[] m_TestListRoots;
        private readonly Dictionary<string, List<TestRunnerResult>> m_ChildrenResults;
        private readonly bool m_runningOnPlatform;

        private readonly List<string> m_AvailableCategories = new List<string>();

        public string[] AvailableCategories
        {
            get { return m_AvailableCategories.Distinct().OrderBy(a => a).ToArray(); }
        }

        public TestTreeViewBuilder(ITestAdaptor[] tests, Dictionary<string, TestRunnerResult> oldTestResultResults, TestRunnerUIFilter uiFilter, bool runningOnPlatform)
        {
            m_AvailableCategories.Add(CategoryFilterExtended.k_DefaultCategory);
            m_OldTestResults = oldTestResultResults;
            m_ChildrenResults = new Dictionary<string, List<TestRunnerResult>>();
            m_TestListRoots = tests;
            m_UIFilter = uiFilter;
            m_runningOnPlatform = runningOnPlatform;
        }

        public TreeViewItem BuildTreeView()
        {
            m_treeFiltered.Clear();
            var rootItem = new TreeViewItem(int.MaxValue, 0, null, "Invisible Root Item");
            foreach (var testRoot in m_TestListRoots)
            {
                ParseTestTree(0, rootItem, testRoot);
            }

            return rootItem;
        }

        private bool IsFilteredOutByUIFilter(ITestAdaptor test, TestRunnerResult result)
        {
            if (m_UIFilter.PassedHidden && result.resultStatus == TestRunnerResult.ResultStatus.Passed)
                return true;
            if (m_UIFilter.FailedHidden && (result.resultStatus == TestRunnerResult.ResultStatus.Failed || result.resultStatus == TestRunnerResult.ResultStatus.Inconclusive))
                return true;
            if (m_UIFilter.NotRunHidden && (result.resultStatus == TestRunnerResult.ResultStatus.NotRun || result.resultStatus == TestRunnerResult.ResultStatus.Skipped))
                return true;
            if (!string.IsNullOrEmpty(m_UIFilter.m_SearchString) && result.FullName.IndexOf(m_UIFilter.m_SearchString, StringComparison.InvariantCultureIgnoreCase) < 0)
                return true;
            if (m_UIFilter.CategoryFilter.Length > 0)
                return !test.Categories.Any(category => m_UIFilter.CategoryFilter.Contains(category));

            return false;
        }

        private TestCount ParseTestTree(int depth, TreeViewItem rootItem, ITestAdaptor testElement)
        {
            if (testElement == null)
            {
                return default;
            }

            var testCount = new TestCount();

            m_AvailableCategories.AddRange(testElement.Categories);

            var testElementId = testElement.UniqueName;
            if (!testElement.HasChildren)
            {
                m_OldTestResults.TryGetValue(testElementId, out var result);

                if (result != null && !m_runningOnPlatform &&
                    (result.ignoredOrSkipped
                     || result.notRunnable
                     || testElement.RunState == RunState.NotRunnable
                     || testElement.RunState == RunState.Ignored
                     || testElement.RunState == RunState.Skipped
                    )
                )
                {
                    // if the test was or becomes ignored or not runnable, we recreate the result in case it has changed
                    // It does not apply if we are running on a platform, as evaluation of runstate needs to be evaluated on the player.
                    result = null;
                }
                if (result == null)
                {
                    result = new TestRunnerResult(testElement);
                }
                results.Add(result);

                var test = new TestTreeViewItem(testElement, depth, rootItem);
                if (!IsFilteredOutByUIFilter(testElement, result))
                {
                    rootItem.AddChild(test);
                    if (!m_treeFiltered.ContainsKey(test.FullName))
                        m_treeFiltered.Add(test.FullName, test);
                }
                else
                {
                    return testCount;
                }
                test.SetResult(result);
                testCount.TotalTestCount = 1;
                testCount.TotalFailedTestCount = result.resultStatus == TestRunnerResult.ResultStatus.Failed ? 1 : 0;
                if (m_ChildrenResults != null && testElement.Parent != null)
                {
                    m_ChildrenResults.TryGetValue(testElement.ParentUniqueName, out var resultList);
                    if (resultList != null)
                    {
                        resultList.Add(result);
                    }
                    else
                    {
                        resultList = new List<TestRunnerResult> {result};
                        m_ChildrenResults.Add(testElement.ParentUniqueName, resultList);
                    }
                }

                return testCount;
            }

            var groupResult = new TestRunnerResult(testElement);

            m_OldTestResults.TryGetValue(testElementId, out var existingResults);

            if (existingResults != null)
            {
                groupResult.resultStatus = existingResults.resultStatus;
                groupResult.messages = existingResults.messages;
                groupResult.stacktrace =  existingResults.stacktrace;
                groupResult.output = existingResults.output;
                groupResult.duration = existingResults.duration;
            }

            results.Add(groupResult);
            var group = new TestTreeViewItem(testElement, depth, rootItem);

            depth++;

            foreach (var child in testElement.Children)
            {
                var childTestCount = ParseTestTree(depth, group, child);

                testCount.TotalTestCount += childTestCount.TotalTestCount;
                testCount.TotalFailedTestCount += childTestCount.TotalFailedTestCount;
            }


            if (testElement.IsTestAssembly && !testElement.HasChildren)
            {
                return testCount;
            }

            if (group.hasChildren)
                rootItem.AddChild(group);

            group.TotalChildrenCount = testCount.TotalTestCount;
            group.TotalSuccessChildrenCount = testCount.TotalFailedTestCount;
            groupResult.CalculateParentResult(testElementId, m_ChildrenResults);
            groupResult.CalculateAndSetParentDuration(testElementId, m_ChildrenResults);
            group.SetResult(groupResult);

            return testCount;
        }
    }
}
