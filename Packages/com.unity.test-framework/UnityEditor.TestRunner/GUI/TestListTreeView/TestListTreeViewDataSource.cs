using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
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
    internal class TestListTreeViewDataSource : TreeViewDataSource
    {
        private bool m_ExpandTreeOnCreation;
        private readonly TestListGUI m_TestListGUI;
        private ITestAdaptor[] m_RootTests;

        public TestListTreeViewDataSource(TreeViewController testListTree, TestListGUI testListGUI, ITestAdaptor[] rootTests) : base(testListTree)
        {
            showRootItem = false;
            rootIsCollapsable = false;
            m_TestListGUI = testListGUI;
            m_RootTests = rootTests;
        }

        public void UpdateRootTest(ITestAdaptor[] rootTests)
        {
            m_RootTests = rootTests;
        }

        public override void FetchData()
        {
            var testListBuilder = new TestTreeViewBuilder(m_RootTests, m_TestListGUI.ResultsByKey, m_TestListGUI.m_TestRunnerUIFilter, m_TestListGUI.m_RunOnPlatform);

            m_RootItem = testListBuilder.BuildTreeView();
            SetExpanded(m_RootItem, true);
            if (m_RootItem.hasChildren && m_RootItem.children.Count == 1)
                SetExpanded(m_RootItem.children[0], true);

            if (m_ExpandTreeOnCreation)
                SetExpandedWithChildren(m_RootItem, true);

            m_TestListGUI.newResultList = new List<TestRunnerResult>(testListBuilder.results);
            m_TestListGUI.filteredTree = testListBuilder.m_treeFiltered;
            m_TestListGUI.m_TestRunnerUIFilter.availableCategories = testListBuilder.AvailableCategories;
            m_NeedRefreshRows = true;
        }

        public override bool IsRenamingItemAllowed(TreeViewItem item)
        {
            return false;
        }

        public void ExpandTreeOnCreation()
        {
            m_ExpandTreeOnCreation = true;
        }

        public override bool IsExpandable(TreeViewItem item)
        {
            if (item is TestTreeViewItem)
                return ((TestTreeViewItem)item).IsGroupNode;
            return base.IsExpandable(item);
        }
    }
}
