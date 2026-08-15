using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.GUI.TestAssets;
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
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using RenameOverlay = UnityEditor.RenameOverlay<int>;


namespace UnityEditor.TestTools.TestRunner.GUI
{
    [Serializable]
    internal class TestListGUI
    {
        private static readonly GUIContent s_GUIRunSelectedTests = EditorGUIUtility.TrTextContent("Run Selected", "Run selected test(s)");
        private static readonly GUIContent s_GUIRunAllTests = EditorGUIUtility.TrTextContent("Run All", "Run all tests");
        private static readonly GUIContent s_GUIRerunFailedTests = EditorGUIUtility.TrTextContent("Rerun Failed", "Rerun all failed tests");
        private static readonly GUIContent s_GUIRun = EditorGUIUtility.TrTextContent("Run");
        private static readonly GUIContent s_GUIRunUntilFailed = EditorGUIUtility.TrTextContent("Run Until Failed");
        private static readonly GUIContent s_GUIRun100Times = EditorGUIUtility.TrTextContent("Run 100 times");
        private static readonly GUIContent s_GUIOpenTest = EditorGUIUtility.TrTextContent("Open source code");
        private static readonly GUIContent s_GUIOpenErrorLine = EditorGUIUtility.TrTextContent("Open error line");
        private static readonly GUIContent s_GUIClearResults = EditorGUIUtility.TrTextContent("Clear Results", "Clear all test results");
        private static readonly GUIContent s_SaveResults = EditorGUIUtility.TrTextContent("Export Results", "Save the latest test results to a file");
        private static readonly GUIContent s_GUICancelRun = EditorGUIUtility.TrTextContent("Cancel Run");

        [SerializeField]
        private TestRunnerWindow m_Window;
        [NonSerialized]
        private TestRunnerApi m_TestRunnerApi;

        [NonSerialized]
        internal TestMode m_TestMode;

        [NonSerialized]
        internal bool m_RunOnPlatform;

        [NonSerialized]
        internal bool m_buildOnly;

        [SerializeField]
        private TestRunProgress runProgress;
        public Dictionary<string, TestTreeViewItem> filteredTree { get; set; }

        public List<TestRunnerResult> newResultList
        {
            get { return m_NewResultList; }
            set
            {
                m_NewResultList = value;
                m_ResultByKey = null;
            }
        }

        [SerializeField]
        private List<TestRunnerResult> m_NewResultList = new List<TestRunnerResult>();

        private Dictionary<string, TestRunnerResult> m_ResultByKey;
        internal Dictionary<string, TestRunnerResult> ResultsByKey
        {
            get
            {
                if (m_ResultByKey == null)
                {
                    m_ResultByKey = new Dictionary<string, TestRunnerResult>();
                    foreach (var result in newResultList)
                    {
                        if (m_ResultByKey.ContainsKey(result.uniqueId))
                        {
                            Debug.LogWarning($"Multiple tests has the same unique id '{result.uniqueId}', their results will be overwritten.");
                            continue;
                        }
                        m_ResultByKey.Add(result.uniqueId, result);
                    }
                }

                return m_ResultByKey;
            }
        }

        [SerializeField]
        private string m_ResultText;
        [SerializeField]
        private string m_ResultStacktrace;

        private TreeViewController m_TestListTree;
        [SerializeField]
        internal TreeViewState m_TestListState;
        [SerializeField]
        internal TestRunnerUIFilter m_TestRunnerUIFilter = new TestRunnerUIFilter();

        private Vector2 m_TestInfoScroll, m_TestListScroll;
        private List<TestRunnerResult> m_QueuedResults = new List<TestRunnerResult>();
        private ITestResultAdaptor m_LatestTestResults;

        private object delayedSearchHandle;

        public TestListGUI()
        {
            MonoCecilHelper = new MonoCecilHelper();
            AssetsDatabaseHelper = new AssetsDatabaseHelper();

            GuiHelper = new GuiHelper(MonoCecilHelper, AssetsDatabaseHelper);
            TestRunnerApi.runProgressChanged.AddListener(UpdateProgressStatus);
        }

        private void UpdateProgressStatus(TestRunProgress progress)
        {
            runProgress = progress;
            TestRunnerWindow.s_Instance.ScheduleRepaint();
        }

        private IMonoCecilHelper MonoCecilHelper { get; set; }
        private IAssetsDatabaseHelper AssetsDatabaseHelper { get; set; }
        private IGuiHelper GuiHelper { get; set; }

        private struct PlayerMenuItem
        {
            public GUIContent name;
            public bool filterSelectedTestsOnly;
        }

        public void PrintHeadPanel()
        {
            if (m_RunOnPlatform)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("Running on " + EditorUserBuildSettings.activeBuildTarget);
                EditorGUILayout.EndHorizontal();
            }
            else if (m_TestMode == TestMode.PlayMode)
            {
                // Note, the following empty vertical area is inserted to give a different imgui id to the search bar.
                // Otherwise imgui will threat the EditMode and PlayMode search bar as the same input.
                EditorGUILayout.BeginVertical();
                EditorGUILayout.EndVertical();
            }

            using (new EditorGUI.DisabledScope(m_TestListTree == null || IsBusy()))
            {
                m_TestRunnerUIFilter.OnModeGUI();
                DrawFilters();
            }
        }

        public void PrintProgressBar(Rect rect)
        {
            if (runProgress == null || runProgress.HasFinished || string.IsNullOrEmpty(runProgress.RunGuid) || !TestRunnerApi.IsRunning(runProgress.RunGuid))
            {
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), runProgress.Progress, runProgress.CurrentStepName);
            if (GUILayout.Button(s_GUICancelRun, GUILayout.Width(100)))
            {
                TestRunnerApi.CancelTestRun(runProgress.RunGuid);
            }

            EditorGUILayout.EndHorizontal();
        }

        public void PrintBottomPanel()
        {
            using (new EditorGUI.DisabledScope(m_TestListTree == null || IsBusy()))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
                    using (new EditorGUI.DisabledScope(m_LatestTestResults == null))
                    {
                        if (GUILayout.Button(s_SaveResults))
                        {
                            var filePath = EditorUtility.SaveFilePanel(s_SaveResults.text, "",
                                $"TestResults_{DateTime.Now:yyyyMMdd_HHmmss}.xml", "xml");
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                TestRunnerApi.SaveResultToFile(m_LatestTestResults, filePath);
                            }

                            GUIUtility.ExitGUI();
                        }
                    }

                    if (GUILayout.Button(s_GUIClearResults))
                    {
                        foreach (var result in newResultList)
                        {
                            result.Clear();
                        }

                        m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
                        Reload();
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(m_TestRunnerUIFilter.FailedCount == 0))
                    {
                        if (GUILayout.Button(s_GUIRerunFailedTests))
                        {
                            RunTests(RunFilterType.RunFailed);
                            GUIUtility.ExitGUI();
                        }
                    }

                    using (new EditorGUI.DisabledScope(m_TestListTree == null || !m_TestListTree.HasSelection()))
                    {
                        if (GUILayout.Button(s_GUIRunSelectedTests))
                        {
                            RunTests(RunFilterType.RunSelected);
                            GUIUtility.ExitGUI();
                        }
                    }

                    if (GUILayout.Button(s_GUIRunAllTests))
                    {
                        RunTests(RunFilterType.RunAll);
                        GUIUtility.ExitGUI();
                    }

                    if (m_TestMode == TestMode.PlayMode && m_RunOnPlatform)
                    {
                        PlayerMenuItem[] menuItems;

                        if (EditorUserBuildSettings.installInBuildFolder)
                        {
                            // Note: We select here m_buildOnly = false, so build location dialog won't show up
                            //       The player won't actually be ran when using together with EditorUserBuildSettings.installInBuildFolder
                            m_buildOnly = false;

                            menuItems = new []
                            {
                                new PlayerMenuItem()
                                {
                                    name = new GUIContent("Install All Tests In Build Folder"), filterSelectedTestsOnly = false
                                },
                                new PlayerMenuItem()
                                {
                                    name = new GUIContent("Install Selected Tests In Build Folder"), filterSelectedTestsOnly = true
                                }
                            };
                        }
                        else
                        {
                            m_buildOnly = true;

                            menuItems = new []
                            {
                                new PlayerMenuItem()
                                {
                                    name = new GUIContent($"{GetBuildText()} All Tests"), filterSelectedTestsOnly = false
                                },
                                new PlayerMenuItem()
                                {
                                    name = new GUIContent($"{GetBuildText()} Selected Tests"), filterSelectedTestsOnly = true
                                },
                            };
                        }

                        if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown))
                        {
                            Vector2 mousePos = Event.current.mousePosition;

                            EditorUtility.DisplayCustomMenu(new Rect(mousePos.x, mousePos.y, 0, 0),
                                menuItems.Select(m => m.name).ToArray(),
                                -1,
                                (object userData, string[] options, int selected) => RunTests(menuItems[selected].filterSelectedTestsOnly ? RunFilterType.BuildSelected : RunFilterType.BuildAll), menuItems);
                        }
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private string GetBuildText()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    if (EditorUserBuildSettings.exportAsGoogleAndroidProject)
                        return "Export";
                    break;
                case BuildTarget.iOS:
                    return "Export";
            }
            return "Build";
        }

        private string PickBuildLocation()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var lastLocation = EditorUserBuildSettings.GetBuildLocation(target);
            var extension = PostprocessBuildPlayer.GetExtensionForBuildTarget(targetGroup, target, BuildOptions.None);
            var defaultName = FileUtil.GetLastPathNameComponent(lastLocation);
            lastLocation = string.IsNullOrEmpty(lastLocation) ? string.Empty : Path.GetDirectoryName(lastLocation);
            bool updateExistingBuild;
            var location = EditorUtility.SaveBuildPanel(target, $"{GetBuildText()} {target}", lastLocation, defaultName, extension,
                out updateExistingBuild);
            if (!string.IsNullOrEmpty(location))
                EditorUserBuildSettings.SetBuildLocation(target, location);
            return location;
        }

        private void DrawFilters()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_TestRunnerUIFilter.Draw();
            EditorGUILayout.EndHorizontal();
        }

        public bool HasTreeData()
        {
            return m_TestListTree != null;
        }

        public void RenderTestList()
        {
            if (m_TestListTree == null)
            {
                GUILayout.Label("Loading...");
                return;
            }

            m_TestListScroll = EditorGUILayout.BeginScrollView(m_TestListScroll,
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(2000));

            if (m_TestListTree.data.root == null || m_TestListTree.data.rowCount == 0 || (!m_TestListTree.isSearching && !m_TestListTree.data.GetItem(0).hasChildren))
            {
                if (m_TestRunnerUIFilter.IsFiltering)
                {
                    var notMatchFoundStyle = new GUIStyle("label");
                    notMatchFoundStyle.normal.textColor = Color.red;
                    notMatchFoundStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("No match found", notMatchFoundStyle);
                    if (GUILayout.Button("Clear filters"))
                    {
                        m_TestRunnerUIFilter.Clear();
                        UpdateTestTree();
                        m_Window.Repaint();
                    }
                }
                RenderNoTestsInfo();
            }
            else
            {
                var treeRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                var treeViewKeyboardControlId = GUIUtility.GetControlID(FocusType.Keyboard);

                m_TestListTree.OnGUI(treeRect, treeViewKeyboardControlId);
            }

            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            EditorGUILayout.EndScrollView();
        }

        private void RenderNoTestsInfo()
        {
            var testScriptAssetsCreator = new TestScriptAssetsCreator();
            if (!testScriptAssetsCreator.ActiveFolderContainsTestAssemblyDefinition())
            {
                var noTestsText = "No tests to show.";

                if (!PlayerSettings.playModeTestRunnerEnabled)
                {
                    const string testsMustLiveInCustomTestAssemblies =
                        "Test scripts can be added to assemblies referencing the \"nunit.framework.dll\" library " +
                        "or folders with Assembly Definition References targeting \"UnityEngine.TestRunner\" or \"UnityEditor.TestRunner\".";

                    noTestsText += Environment.NewLine + testsMustLiveInCustomTestAssemblies;
                }

                EditorGUILayout.HelpBox(noTestsText, MessageType.Info);
                if (GUILayout.Button("Create a new Test Assembly Folder in the active path."))
                {
                    testScriptAssetsCreator.AddNewFolderWithTestAssemblyDefinition(m_TestMode == TestMode.EditMode);
                }
            }

            const string notTestAssembly = "Test Scripts can only be created inside test assemblies.";
            const string createTestScriptInCurrentFolder = "Create a new Test Script in the active path.";
            var canAddTestScriptAndItWillCompile = testScriptAssetsCreator.TestScriptWillCompileInActiveFolder();

            using (new EditorGUI.DisabledScope(!canAddTestScriptAndItWillCompile))
            {
                var createTestScriptInCurrentFolderGUI = !canAddTestScriptAndItWillCompile
                    ? new GUIContent(createTestScriptInCurrentFolder, notTestAssembly)
                    : new GUIContent(createTestScriptInCurrentFolder);

                if (GUILayout.Button(createTestScriptInCurrentFolderGUI))
                {
                    testScriptAssetsCreator.AddNewTestScript();
                }
            }
        }

        public void RenderDetails(float width)
        {
            m_TestInfoScroll = EditorGUILayout.BeginScrollView(m_TestInfoScroll, GUILayout.ExpandWidth(true));
            var resultTextHeight = TestRunnerWindow.Styles.info.CalcHeight(new GUIContent(m_ResultText), width);
            EditorGUILayout.SelectableLabel(m_ResultText, TestRunnerWindow.Styles.info,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true),
                GUILayout.MinHeight(resultTextHeight));
            EditorGUILayout.EndScrollView();
        }

        public void Reload()
        {
            if (m_TestListTree != null)
            {
                m_TestListTree.ReloadData();
                UpdateQueuedResults();
            }
        }

        public void Repaint()
        {
            if (m_TestListTree == null || m_TestListTree.data.root == null)
            {
                return;
            }

            m_TestListTree.Repaint();
            if (m_TestListTree.data.rowCount == 0)
                m_TestListTree.SetSelection(new int[0], false);
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
        }

        public void Init(TestRunnerWindow window, ITestAdaptor rootTest)
        {
            Init(window, new[] {rootTest});
        }

        private void Init(TestRunnerWindow window, ITestAdaptor[] rootTests)
        {
            if (m_Window == null)
            {
                m_Window = window;
            }

            if (m_TestListTree == null)
            {
                if (m_TestListState == null)
                {
                    m_TestListState = new TreeViewState();
                }
                if (m_TestListTree == null)
                    m_TestListTree = new TreeViewController(m_Window, m_TestListState);

                m_TestListTree.deselectOnUnhandledMouseDown = false;

                m_TestListTree.selectionChangedCallback += TestSelectionCallback;
                m_TestListTree.itemDoubleClickedCallback += TestDoubleClickCallback;
                m_TestListTree.contextClickItemCallback += TestContextClickCallback;

                var testListTreeViewDataSource = new TestListTreeViewDataSource(m_TestListTree, this, rootTests);

                m_TestListTree.Init(new Rect(),
                    testListTreeViewDataSource,
                    new TestListTreeViewGUI(m_TestListTree),
                    null);
            }

            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            m_TestRunnerUIFilter.RebuildTestList = () => m_TestListTree.ReloadData();
            m_TestRunnerUIFilter.UpdateTestTreeRoots = UpdateTestTree;
            m_TestRunnerUIFilter.SearchStringChanged = s =>
            {
                this.m_Window.CancelDelayedCall(this.delayedSearchHandle);
                this.delayedSearchHandle = this.m_Window.CallDelayed(m_TestListTree.ReloadData, 0.25);
            };
            m_TestRunnerUIFilter.SearchStringCleared = () =>
            {
                this.m_Window.CancelDelayedCall(this.delayedSearchHandle);
                this.delayedSearchHandle = null;
                m_TestListTree.ReloadData();
                FrameSelection();
            };
        }

        public void UpdateResult(TestRunnerResult result)
        {
            if (!HasTreeData())
            {
                m_QueuedResults.Add(result);
                return;
            }


            if (!ResultsByKey.TryGetValue(result.uniqueId, out var testRunnerResult))
            {
                // Add missing result due to e.g. changes in code for uniqueId due to change of package version.
                m_NewResultList.Add(result);
                ResultsByKey[result.uniqueId] = result;
                testRunnerResult = result;
            }

            testRunnerResult.Update(result);
            Repaint();
            m_Window.Repaint();
        }

        public void RunFinished(ITestResultAdaptor results)
        {
            m_LatestTestResults = results;
            UpdateTestTree();
        }

        private void UpdateTestTree()
        {
            var testList = this;
            if (m_TestRunnerApi == null)
            {
                m_TestRunnerApi= ScriptableObject.CreateInstance<TestRunnerApi>();
            }

            m_TestRunnerApi.RetrieveTestList(GetExecutionSettings(), rootTest =>
            {
                testList.UpdateTestTree(new[] { rootTest });
                testList.Reload();
            });
        }

        public void UpdateTestTree(ITestAdaptor[] tests)
        {
            if (!HasTreeData())
            {
                return;
            }

            (m_TestListTree.data as TestListTreeViewDataSource).UpdateRootTest(tests);

            m_TestListTree.ReloadData();
            Repaint();
            m_Window.Repaint();
        }

        private void UpdateQueuedResults()
        {
            foreach (var testRunnerResult in m_QueuedResults)
            {
                if (ResultsByKey.TryGetValue(testRunnerResult.uniqueId, out var existingResult))
                {
                    existingResult.Update(testRunnerResult);
                }
            }
            m_QueuedResults.Clear();
            TestSelectionCallback(m_TestListState.selectedIDs.ToArray());
            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            Repaint();
            m_Window.Repaint();
        }

        internal void TestSelectionCallback(int[] selected)
        {
            if (m_TestListTree != null && selected.Length > 0)
            {
                if (m_TestListTree != null)
                {
                    var node = m_TestListTree.FindItem(selected[0]);
                    if (node is TestTreeViewItem)
                    {
                        var test = node as TestTreeViewItem;
                        m_ResultText = test.GetResultText();
                        m_ResultStacktrace = test.result.stacktrace;
                    }
                }
            }
            else if (selected.Length == 0)
            {
                m_ResultText = "";
            }
        }

        private void TestDoubleClickCallback(int id)
        {
            if (IsBusy())
                return;

            RunTests(RunFilterType.RunSpecific, id);
            GUIUtility.ExitGUI();
        }

        private void RunTests(RunFilterType runFilter, params int[] specificTests)
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                Debug.LogError("Fix compilation issues before running tests");
                return;
            }

            var filters = ConstructFilter(runFilter, specificTests);
            if (filters == null)
            {
                return;
            }

            // The filter doesn't work for selections other than an individual test or assembly,
            // so when running with a selection, take another path.
            if (runFilter == RunFilterType.RunSelected)
            {
                foreach (var lineId in m_TestListState.selectedIDs)
                {
                    (m_TestListTree.FindItem(lineId) as TestTreeViewItem)?.ClearResultsRecursively();
                }
            }
            else
            {
                foreach (var filter in filters)
                {
                    filter.ClearResults(newResultList.OfType<UITestRunnerFilter.IClearableResult>().ToDictionary(result => result.Id));
                }
            }

            var testFilters = filters.Select(filter => new Filter
            {
                testMode = m_TestMode,
                assemblyNames = filter.assemblyNames,
                categoryNames = filter.categoryNames,
                groupNames = filter.groupNames,
                testNames = filter.testNames,
            }).ToArray();

#if UNITY_2022_2_OR_NEWER
            var executionSettings =
                CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : null,
                    testFilters);
#else
            var executionSettings =
                CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : (BuildTarget?)null,
                    testFilters);
#endif
            if (runFilter == RunFilterType.BuildAll || runFilter == RunFilterType.BuildSelected)
            {
                var runSettings = new PlayerLauncherTestRunSettings();
                runSettings.buildOnly = m_buildOnly;

                if (runSettings.buildOnly)
                {
                    runSettings.buildOnlyLocationPath = PickBuildLocation();
                    if (string.IsNullOrEmpty(runSettings.buildOnlyLocationPath))
                    {
                        Debug.LogWarning("Aborting, build selection was canceled.");
                        return;
                    }
                }

                executionSettings.overloadTestRunSettings = runSettings;

                Debug.Log(executionSettings.ToString());
            }

            if (m_TestRunnerApi == null)
            {
                m_TestRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            }

            m_TestRunnerApi.Execute(executionSettings);

            if (executionSettings.targetPlatform != null && !(runFilter == RunFilterType.BuildAll || runFilter == RunFilterType.BuildSelected))
            {
                GUIUtility.ExitGUI();
            }
        }

        private void TestContextClickCallback(int id)
        {
            if (id == 0)
                return;

            var m = new GenericMenu();
            var multilineSelection = m_TestListState.selectedIDs.Count > 1;

            if (!multilineSelection)
            {
                var testNode = GetSelectedTest();
                var isNotSuite = !testNode.IsGroupNode;
                if (isNotSuite)
                {
                    if (!string.IsNullOrEmpty(m_ResultStacktrace))
                    {
                        m.AddItem(s_GUIOpenErrorLine,
                            false,
                            data =>
                            {
                                if (!GuiHelper.OpenScriptInExternalEditor(m_ResultStacktrace))
                                {
                                    GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method);
                                }
                            },
                            "");
                    }

                    m.AddItem(s_GUIOpenTest,
                        false,
                        data => GuiHelper.OpenScriptInExternalEditor(testNode.type, testNode.method),
                        "");
                    m.AddSeparator("");
                }
            }

            if (!IsBusy())
            {
                m.AddItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun,
                    false,
                    data => RunTests(RunFilterType.RunSelected),
                    "");
            }
            else
                m.AddDisabledItem(multilineSelection ? s_GUIRunSelectedTests : s_GUIRun, false);

            m.ShowAsContext();
        }

        private enum RunFilterType
        {
            RunAll,
            RunSelected,
            RunFailed,
            RunSpecific,
            BuildAll,
            BuildSelected
        }

        private struct FilterConstructionStep
        {
            public int Id;
            public TreeViewItem Item;
        }

        private UITestRunnerFilter[] ConstructFilter(RunFilterType runFilter, int[] specificTests = null)
        {
            if ((runFilter == RunFilterType.RunAll || runFilter == RunFilterType.BuildAll) && !m_TestRunnerUIFilter.IsFiltering)
            {
                // Shortcut for RunAll, which will not trigger any explicit tests
                return new[] {new UITestRunnerFilter()};
            }

            if (runFilter == RunFilterType.RunFailed)
            {
                return new[]
                {
                    new UITestRunnerFilter()
                    {
                        testNames = ResultsByKey
                            .Where(resultPair => !resultPair.Value.isSuite && resultPair.Value.resultStatus == TestRunnerResult.ResultStatus.Failed)
                            .Select(resultPair => resultPair.Value.FullName)
                            .ToArray()
                    }
                };
            }

            var includedIds = GetIdsIncludedInRunFilter(runFilter, specificTests);
            var testsInFilter = includedIds.Select(id => m_TestListTree.FindItem(id)).OfType<TestTreeViewItem>()
                .SelectMany(item => item.GetMinimizedSelectedTree()).Distinct().ToArray();

            if (testsInFilter.Length == 0)
            {
                return null;
            }

            if (testsInFilter.Any(test => test.Parent == null))
            {
                // The root element is included in the minified list, which means we are running all tests
                // It should however trigger explicit tests, which is done by a groupNames filter matching all groups
                return new[]
                {
                    new UITestRunnerFilter()
                    {
                        groupNames = new[] {".*"}
                    }
                };
            }

            var assemblies = testsInFilter.Where(test => test.IsTestAssembly).ToArray();
            var tests = testsInFilter.Where(test => !test.IsTestAssembly).ToArray();

            var filters = new List<UITestRunnerFilter>();
            if (tests.Length > 0)
            {
                filters.Add(new UITestRunnerFilter
                {
                    testNames = tests.Select(test => test.FullName).ToArray()
                });
            }

            if (assemblies.Length > 0)
            {
                filters.Add(new UITestRunnerFilter
                {
                    assemblyNames = assemblies.Select(test =>
                    {
                        // remove .dll from the end of the name
                        var name = test.Name;
                        if (name.EndsWith(".dll"))
                            name = name.Substring(0, name.Length - 4);
                        return name;
                    }).ToArray()
                });
            }

            return filters.ToArray();
        }

        private IEnumerable<int> GetIdsIncludedInRunFilter(RunFilterType runFilter, int[] specificTests)
        {
            switch (runFilter)
            {
                case RunFilterType.RunSelected:
                case RunFilterType.BuildSelected:
                    return m_TestListState.selectedIDs;
                case RunFilterType.RunSpecific:
                    if (specificTests == null)
                    {
                        throw new ArgumentNullException(
                            $"For {nameof(RunFilterType.RunSpecific)}, the {nameof(specificTests)} argument must not be null.");
                    }

                    return specificTests;
                default:
                    return m_TestListTree.GetRowIDs();
            }
        }

        private TestTreeViewItem GetSelectedTest()
        {
            foreach (var lineId in m_TestListState.selectedIDs)
            {
                var line = m_TestListTree.FindItem(lineId);
                if (line is TestTreeViewItem)
                {
                    return line as TestTreeViewItem;
                }
            }
            return null;
        }

        private void FrameSelection()
        {
            if (m_TestListTree.HasSelection())
            {
                var firstClickedID = m_TestListState.selectedIDs.First() == m_TestListState.lastClickedID ? m_TestListState.selectedIDs.Last() : m_TestListState.selectedIDs.First();
                m_TestListTree.Frame(firstClickedID, true, false);
            }
        }

        public void RebuildUIFilter()
        {
            m_TestRunnerUIFilter.UpdateCounters(newResultList, filteredTree);
            if (m_TestRunnerUIFilter.IsFiltering)
            {
                m_TestListTree.ReloadData();
            }
        }

        private static bool IsBusy()
        {
            return TestRunnerApi.IsRunActive() || EditorApplication.isCompiling || EditorApplication.isPlaying;
        }

        public ExecutionSettings GetExecutionSettings()
        {
            var filter = new Filter
            {
                testMode = m_TestMode
            };

#if UNITY_2022_2_OR_NEWER
            return CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : null, filter);
#else
            return CreateExecutionSettings(m_RunOnPlatform ? EditorUserBuildSettings.activeBuildTarget : (BuildTarget?)null, filter);
#endif
        }

        private static ExecutionSettings CreateExecutionSettings(BuildTarget? buildTarget, params Filter[] filters)
        {
            return new ExecutionSettings(filters)
            {
                targetPlatform = buildTarget,
            };
        }
    }
}
