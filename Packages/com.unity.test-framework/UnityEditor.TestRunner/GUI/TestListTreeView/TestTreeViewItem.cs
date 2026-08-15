using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
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
    internal sealed class TestTreeViewItem : TreeViewItem
    {
        public TestRunnerResult result;
        internal ITestAdaptor m_Test;

        public Type type;
        public MethodInfo method;

        private const int k_ResultTestMaxLength = 15000;

        public bool IsGroupNode { get { return m_Test.IsSuite; } }

        public string FullName { get { return m_Test.FullName; } }
        public string UniqueName { get { return m_Test.UniqueName; } }

        public override string displayName
        {
            get => $"{base.displayName}{(hasChildren ? $" ({TotalChildrenCount} tests) {(TotalSuccessChildrenCount > 0 ? $" {TotalSuccessChildrenCount} tests failed" : null)}" : null)}";
            set => base.displayName = value;
        }

        public string GetAssemblyName()
        {
            var test = m_Test;
            while (test != null)
            {
                if (test.IsTestAssembly)
                {
                    return test.FullName;
                }

                test = test.Parent;
            }

            return null;
        }


        public IEnumerable<ITestAdaptor> GetMinimizedSelectedTree()
        {
            if (!m_Test.HasChildren)
            {
                yield return m_Test;
                yield break;
            }

            var minimizedDescendants = children.OfType<TestTreeViewItem>().SelectMany(c => c.GetMinimizedSelectedTree()).ToArray();
            var includeChildren = minimizedDescendants.Count(c => c.Parent == m_Test);
            if (includeChildren == m_Test.Children.Count())
            {
                // All children are included in the filter, so we can just return the parent
                yield return m_Test;
                yield break;
            }
            
            foreach (var child in minimizedDescendants)
            {
                yield return child;
            }
        }

        public TestTreeViewItem(ITestAdaptor test, int depth, TreeViewItem parent)
            : base(GetId(test), depth, parent, test.Name)
        {
            m_Test = test;

            if (test.TypeInfo != null)
            {
                type = test.TypeInfo.Type;
            }
            if (test.Method != null)
            {
                method = test.Method.MethodInfo;
            }

            displayName = test.Name.Replace("\n", "");
            icon = Icons.s_UnknownImg;
        }

        public int TotalChildrenCount { get; set; }
        public int TotalSuccessChildrenCount { get; set; }

        private static int GetId(ITestAdaptor test)
        {
            return test.UniqueName.GetHashCode();
        }

        public void ClearResultsRecursively()
        {
            this.result?.Clear();
            if (this.children != null)
            {
                foreach (var child in this.children)
                {
                    (child as TestTreeViewItem)?.ClearResultsRecursively();
                }
            }
        }

        public void SetResult(TestRunnerResult testResult)
        {
            result = testResult;
            result.SetResultChangedCallback(ResultUpdated);
            ResultUpdated(result);
        }

        public string GetResultText()
        {
            if (result.resultStatus == TestRunnerResult.ResultStatus.NotRun)
            {
                if (result.ignoredOrSkipped)
                {
                    return result.messages;
                }
                return string.Empty;
            }
            var durationString = String.Format("{0:0.000}", result.duration);
            var sb = new StringBuilder(string.Format("{0} ({1}s)", displayName.Trim(), durationString));
            if (!string.IsNullOrEmpty(result.description))
            {
                sb.AppendFormat("\n{0}", result.description);
            }
            if (!string.IsNullOrEmpty(result.messages))
            {
                sb.Append("\n---\n");
                sb.Append(result.messages);
            }
            if (!string.IsNullOrEmpty(result.stacktrace))
            {
                sb.Append("\n---\n");
                sb.Append(StacktraceWithHyperlinks(result.stacktrace));
            }
            if (!string.IsNullOrEmpty(result.output))
            {
                sb.Append("\n---\n");
                sb.Append(result.output);
            }
            if (sb.Length > k_ResultTestMaxLength)
            {
                sb.Length = k_ResultTestMaxLength;
                sb.AppendFormat("...\n\n---MESSAGE TRUNCATED AT {0} CHARACTERS---", k_ResultTestMaxLength);
            }
            return sb.ToString().Trim();
        }

        private void ResultUpdated(TestRunnerResult testResult)
        {
            switch (testResult.resultStatus)
            {
                case TestRunnerResult.ResultStatus.Passed:
                    icon = Icons.s_SuccessImg;
                    break;
                case TestRunnerResult.ResultStatus.Failed:
                    icon = Icons.s_FailImg;
                    break;
                case TestRunnerResult.ResultStatus.Inconclusive:
                    icon = Icons.s_InconclusiveImg;
                    break;
                case TestRunnerResult.ResultStatus.Skipped:
                    icon = Icons.s_IgnoreImg;
                    break;
                default:
                    if (testResult.ignoredOrSkipped)
                    {
                        icon = Icons.s_IgnoreImg;
                    }
                    else if (testResult.notRunnable)
                    {
                        icon = Icons.s_FailImg;
                    }
                    else
                    {
                        icon = Icons.s_UnknownImg;
                    }
                    break;
            }
        }
        
        private static string StacktraceWithHyperlinks(string stacktraceText)
        {
            StringBuilder textWithHyperlinks = new StringBuilder();
            var lines = stacktraceText.Split(new string[] { "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; ++i)
            {
                string textBeforeFilePath = "] in ";
                int filePathIndex = lines[i].IndexOf(textBeforeFilePath, StringComparison.Ordinal);
                if (filePathIndex > 0)
                {
                    filePathIndex += textBeforeFilePath.Length;
                    if (lines[i][filePathIndex] != '<') // sometimes no url is given, just an id between <>, we can't do an hyperlink
                    {
                        string filePathPart = lines[i].Substring(filePathIndex);
                        int lineIndex = filePathPart.LastIndexOf(":", StringComparison.Ordinal); // LastIndex because the url can contain ':' ex:"C:"
                        if (lineIndex > 0)
                        {
                            string lineString = filePathPart.Substring(lineIndex + 1);
                            string filePath = filePathPart.Substring(0, lineIndex);
#if UNITY_2021_3_OR_NEWER
                            var displayedPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
#else
                            var displayedPath = filePath;
#endif
                            textWithHyperlinks.Append($"{lines[i].Substring(0, filePathIndex)}<color=#40a0ff><a href=\"{filePath}\" line=\"{lineString}\">{displayedPath}:{lineString}</a></color>\n");

                            continue; // continue to evade the default case
                        }
                    }
                }
                // default case if no hyperlink : we just write the line
                textWithHyperlinks.Append(lines[i] + "\n");
            }
            // Remove the last \n
            if (textWithHyperlinks.Length > 0) // textWithHyperlinks always ends with \n if it is not empty
                textWithHyperlinks.Remove(textWithHyperlinks.Length - 1, 1);
            
            return textWithHyperlinks.ToString();
        }
    }
}
