using System;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools;
using UnityEngine.TestTools.NUnitExtensions;
using UnityEngine.TestTools.TestRunner;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    [Serializable]
    internal class TestJobData : ISerializationCallbackReceiver
    {
        [SerializeField]
        public string guid;

        [SerializeField]
        public string startTime;

        [NonSerialized]
        public Stack<TaskInfo> taskInfoStack = new Stack<TaskInfo>();

        [SerializeField]
        public int taskPC;

        [SerializeField]
        public bool isRunning;

        [SerializeField]
        public ExecutionSettings executionSettings;

        [SerializeField]
        public RunProgress runProgress = new RunProgress();

        [SerializeField]
        public string[] existingFiles;

        [SerializeField]
        public int undoGroup = -1;

        [SerializeField]
        public EditModeRunner editModeRunner;

        [SerializeField]
        public BeforeAfterTestCommandState setUpTearDownState;

        [SerializeField]
        public BeforeAfterTestCommandState oneTimeSetUpTearDownState;

        [SerializeField]
        public BeforeAfterTestCommandState outerUnityTestActionState;

        [SerializeField]
        public TestRunnerStateSerializer testRunnerStateSerializer;

        [SerializeField]
        public EnumerableTestState enumerableTestState;

        [SerializeField]
        private TaskInfo[] savedTaskInfoStack;

        [NonSerialized]
        public bool isHandledByRunner;

        [SerializeField]
        public SceneSetup[] SceneSetup;
        [NonSerialized]
        public TestTaskBase[] Tasks;

        [SerializeField]
        public TestProgress testProgress;

        public ITest testTree;

        [NonSerialized]
        public ITestFilter testFilter;

        [NonSerialized]
        public List<ITest> filteredTests = new();

        [NonSerialized]
        public TestStartedEvent TestStartedEvent;
        [NonSerialized]
        public TestFinishedEvent TestFinishedEvent;
        [NonSerialized]
        public RunStartedEvent RunStartedEvent;
        [NonSerialized]
        public RunFinishedEvent RunFinishedEvent;

        [NonSerialized]
        public UnityTestExecutionContext Context;

        [NonSerialized]
        public ConstructDelegator ConstructDelegator;

        [NonSerialized]
        public ITestResult TestResults;

        [SerializeField]
        public Scene InitTestScene;

        [SerializeField]
        public string InitTestScenePath;

        public BuildPlayerOptions PlayerBuildOptions { get; set; }

#if UNITY_6000_1_OR_NEWER
        public BuildPlayerWithProfileOptions PlayerBuildOptionsWithProfile { get; set; }
#endif

        [SerializeField]
        public PlaymodeTestsController PlaymodeTestsController;

        [SerializeField]
        public PlaymodeTestsControllerSettings PlayModeSettings;

        [SerializeField]
        public PlatformSpecificSetup PlatformSpecificSetup;

        [NonSerialized]
        public RuntimePlatform TargetRuntimePlatform = Application.platform;

        [SerializeField]
        public EnumerableTestState RetryRepeatState;

        [SerializeField]
        public SavedProjectSettings OriginalProjectSettings;

        [SerializeField]
        public int UserApplicationIdleTime = -1;

        [SerializeField]
        public int UserInteractionMode = -1;

        public TestJobData(ExecutionSettings settings)
        {
            guid = Guid.NewGuid().ToString();
            executionSettings = settings;
            isRunning = false;
            startTime = DateTime.Now.ToString("o");
        }

        public void OnBeforeSerialize()
        {
            savedTaskInfoStack = taskInfoStack.ToArray();
        }

        public void OnAfterDeserialize()
        {
            taskInfoStack = new Stack<TaskInfo>(savedTaskInfoStack);
        }

        /// <summary>
        /// Fetches configured build options, both PlayerBuildOptionsWithProfile and PlayerBuildOptions
        /// are initialized by  <see cref="LegacyPlayerRunTask"/>.
        /// </summary>
        public BuildOptions GetCurrentBuildOptions()
        {
#if UNITY_6000_1_OR_NEWER
            if (PlayerBuildOptionsWithProfile.buildProfile != null)
                return PlayerBuildOptionsWithProfile.options;
#endif

         return PlayerBuildOptions.options;
        }

        [Serializable]
        internal class SavedProjectSettings
        {
            public bool runInBackgroundValue;

            public bool consoleErrorPaused;
        }
    }
}
