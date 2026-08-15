using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.TestRunner.Callbacks;
using Object = UnityEngine.Object;
#if UNITY_6000_5_OR_NEWER
using UnityEngine.Assemblies;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal class TestLaunchFailedException : Exception
    {
        public TestLaunchFailedException() {}
        public TestLaunchFailedException(string message) : base(message) {}
    }

    [Serializable]
    internal class PlayerLauncher : RuntimeTestLauncherBase
    {
        private readonly BuildTarget m_TargetPlatform;
        private ITestRunSettings m_OverloadTestRunSettings;
        private string m_SceneName;
        private Scene m_Scene;
        private int m_HeartbeatTimeout;
        private string m_PlayerWithTestsPath;
        private PlaymodeTestsController m_Runner;

        internal PlayerLauncherBuildOptions playerBuildOptions { get; private set; }

        public PlayerLauncher(PlaymodeTestsControllerSettings settings, BuildTarget? targetPlatform, ITestRunSettings overloadTestRunSettings, int heartbeatTimeout, string playerWithTestsPath, string scenePath, Scene scene, PlaymodeTestsController runner) : base(settings)
        {
            m_TargetPlatform = targetPlatform ?? EditorUserBuildSettings.activeBuildTarget;
            m_OverloadTestRunSettings = overloadTestRunSettings;
            m_HeartbeatTimeout = heartbeatTimeout;
            m_PlayerWithTestsPath = playerWithTestsPath;
            m_SceneName = scenePath;
            m_Scene = scene;
            m_Runner = runner;
        }

        protected override RuntimePlatform TestTargetPlatform => BuildTargetConverter.TryConvertToRuntimePlatform(m_TargetPlatform);

        public override void Run()
        {
            var editorConnectionTestCollector = RemoteTestRunController.instance;
            editorConnectionTestCollector.hideFlags = HideFlags.HideAndDontSave;
            editorConnectionTestCollector.Init(m_TargetPlatform, m_HeartbeatTimeout);

            var remotePlayerLogController = RemotePlayerLogController.instance;
            remotePlayerLogController.hideFlags = HideFlags.HideAndDontSave;

            using (var settings = new PlayerLauncherContextSettings(m_OverloadTestRunSettings, m_TargetPlatform))
            {
                PrepareScene(m_SceneName, m_Scene, m_Runner);

                var filter = m_Settings.BuildNUnitFilter();
                var runner = LoadTests(filter);
                var exceptionThrown = ExecutePrebuildSetupWithTestDataMethods(runner.LoadedTest, filter) || ExecutePreBuildSetupMethods(runner.LoadedTest, filter);
                if (exceptionThrown)
                {
                    ReopenOriginalScene(m_Settings.originalScene);
                    CallbacksDelegator.instance.RunFailed("Run Failed: One or more errors in a prebuild setup. See the editor log for details.");
                    return;
                }

                EditorSceneManager.MarkSceneDirty(m_Scene);
                EditorSceneManager.SaveScene(m_Scene);

                playerBuildOptions = GetBuildOptions(m_SceneName);
                bool success = BuildAndRunFromLauncherOptions(playerBuildOptions);

                FilePathMetaInfo.TryCreateFile(runner.LoadedTest, playerBuildOptions);
                ExecutePostBuildCleanupMethods(runner.LoadedTest, filter);

                ReopenOriginalScene(m_Settings.originalScene);

                if (!success)
                {
                    Object.DestroyImmediate(editorConnectionTestCollector);
                    Debug.LogError("Player build failed");
                    throw new TestLaunchFailedException("Player build failed");
                }

                if ((playerBuildOptions.GetCurrentBuildOptions() & BuildOptions.AutoRunPlayer) != 0)
                {
                    editorConnectionTestCollector.PostSuccessfulBuildAction();
                }

                var runSettings = m_OverloadTestRunSettings as PlayerLauncherTestRunSettings;
                if (success && runSettings != null && runSettings.buildOnly)
                {
                    EditorUtility.RevealInFinder(playerBuildOptions.BuildPlayerOptions.locationPathName);
                }
            }
        }

        public void PrepareScene(string sceneName, Scene scene, PlaymodeTestsController runner)
        {
            runner.AddEventHandlerMonoBehaviour<PlayModeRunnerCallback>();
            var commandLineArgs = Environment.GetCommandLineArgs();
            if (!commandLineArgs.Contains("-doNotReportTestResultsBackToEditor"))
            {
                runner.AddEventHandlerMonoBehaviour<RemoteTestResultSender>();
            }
            runner.AddEventHandlerMonoBehaviour<PlayerQuitHandler>();
            runner.AddEventHandlerScriptableObject<TestRunCallbackListener>();

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, sceneName, false);
        }

        private bool BuildAndRunFromLauncherOptions(PlayerLauncherBuildOptions playerBuildOptions)
        {
            bool success = false;

#if UNITY_6000_1_OR_NEWER
            if (playerBuildOptions.ShouldBuildWithProfile())
            {
                playerBuildOptions.OnBeforeBuildProfileBuild(m_Scene.path);
                Debug.LogFormat(
                    LogType.Log, LogOption.NoStacktrace, null,
                    "Building player with build profile options:\n{0}", playerBuildOptions);
                success = BuildAndRunPlayer(playerBuildOptions.BuildPlayerWithProfileOptions);
                playerBuildOptions.OnAfterBuildProfileBuild();

            }
            else
#endif
            {
                Debug.LogFormat(
                    LogType.Log, LogOption.NoStacktrace, null,
                    "Building player with following options:\n{0}", playerBuildOptions);
                success = BuildAndRunPlayer(playerBuildOptions.BuildPlayerOptions);
            }

            return success;
        }

        private static bool BuildAndRunPlayer(BuildPlayerOptions buildOptions)
        {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Building player with following options:\n{0}", buildOptions);

#if !UNITY_2021_2_OR_NEWER
            // Android has to be in listen mode to establish player connection
		    // Only flip connect to host if we are older than Unity 2021.2
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.Android)
            {
                buildOptions.BuildPlayerOptions.options &= ~BuildOptions.ConnectToHost;
            }
#endif
            // For now, so does Lumin
#if !UNITY_2022_2_OR_NEWER
            if (buildOptions.BuildPlayerOptions.target == BuildTarget.Lumin)
            {
                buildOptions.BuildPlayerOptions.options &= ~BuildOptions.ConnectToHost;
            }
#endif

#if UNITY_2023_2_OR_NEWER
            // WebGL has to be in close on quit mode to ensure that the browser tab is closed when the player finishes running tests
            if (buildOptions.target == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = true;
            }
#endif

            var result = BuildPipeline.BuildPlayer(buildOptions);
            if (result.summary.result != BuildResult.Succeeded)
                Debug.LogError(result.SummarizeErrors());

#if UNITY_2023_2_OR_NEWER
            // Clean up WebGL close on quit mode
            if (buildOptions.target == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = false;
            }
#endif

            return result.summary.result == BuildResult.Succeeded;
        }

#if UNITY_6000_1_OR_NEWER
        private static bool BuildAndRunPlayer(BuildPlayerWithProfileOptions buildOptions)
        {
            if (buildOptions.buildProfile == null)
            {
                throw new TestLaunchFailedException("Player build failed, profile was null but BuildPlayerWithProfileOptions was used.");
            }

            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Building player with profile options:\n{0}", buildOptions);
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Building with profile at path:\n{0}",
                AssetDatabase.GetAssetPath(buildOptions.buildProfile));

            // WebGL has to be in close on quit mode to ensure that the browser tab is closed when the player finishes running tests
            if (buildOptions.buildProfile.buildTarget == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = true;
            }

            var result = BuildPipeline.BuildPlayer(buildOptions);
            if (result.summary.result != BuildResult.Succeeded)
                Debug.LogError(result.SummarizeErrors());

            // Clean up WebGL close on quit mode
            if (buildOptions.buildProfile.buildTarget == BuildTarget.WebGL)
            {
                PlayerSettings.WebGL.closeOnQuit = false;
            }

            return result.summary.result == BuildResult.Succeeded;
        }
#endif


        internal PlayerLauncherBuildOptions GetBuildOptions(string scenePath)
        {
            var buildOnly = false;
            var runSettings = m_OverloadTestRunSettings as PlayerLauncherTestRunSettings;
            if (runSettings != null)
            {
                buildOnly = runSettings.buildOnly;
            }

            var buildOptions = new BuildPlayerOptions();

            var scenes = new List<string> { scenePath };
            scenes.AddRange(EditorBuildSettings.scenes.Select(x => x.path));
            buildOptions.scenes = scenes.ToArray();

            buildOptions.options |= BuildOptions.Development | BuildOptions.ConnectToHost | BuildOptions.IncludeTestAssemblies | BuildOptions.StrictMode;
            buildOptions.target = m_TargetPlatform;

#if UNITY_2021_2_OR_NEWER
            buildOptions.subtarget = EditorUserBuildSettings.GetActiveSubtargetFor(m_TargetPlatform);
#endif

            if (EditorUserBuildSettings.buildWithCodeCoverage && BuildProfileModuleUtil.IsBuildTargetSupportedByCoverage(m_TargetPlatform))
                buildOptions.options |= BuildOptions.EnableCodeCoverage;

            if (EditorUserBuildSettings.waitForPlayerConnection)
                buildOptions.options |= BuildOptions.WaitForPlayerConnection;

            if (EditorUserBuildSettings.symlinkSources)
                buildOptions.options |= BuildOptions.SymlinkSources;

            if (EditorUserBuildSettings.allowDebugging)
                buildOptions.options |= BuildOptions.AllowDebugging;

            if (EditorUserBuildSettings.installInBuildFolder)
                buildOptions.options |= BuildOptions.InstallInBuildFolder;
            else if (!buildOnly)
                buildOptions.options |= BuildOptions.AutoRunPlayer;

            var buildTargetGroup = EditorUserBuildSettings.activeBuildTargetGroup;
            buildOptions.targetGroup = buildTargetGroup;

            //Check if Lz4 is supported for the current buildtargetgroup and enable it if need be
            if (PostprocessBuildPlayer.SupportsLz4Compression(buildTargetGroup, m_TargetPlatform))
            {
                if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4)
                    buildOptions.options |= BuildOptions.CompressWithLz4;
                else if (EditorUserBuildSettings.GetCompressionType(buildTargetGroup) == Compression.Lz4HC)
                    buildOptions.options |= BuildOptions.CompressWithLz4HC;
            }

            string buildLocation;
            if (buildOnly)
            {
                buildLocation = buildOptions.locationPathName = runSettings.buildOnlyLocationPath;
            }
            else
            {
                var reduceBuildLocationPathLength = false;

                //Some platforms hit MAX_PATH limits during the build process, in these cases minimize the path length
                if ((m_TargetPlatform == BuildTarget.WSAPlayer)
#if !UNITY_2021_1_OR_NEWER
                || (m_TargetPlatform == BuildTarget.XboxOne)
#endif
                || (m_TargetPlatform == BuildTarget.GameCoreXboxSeries) || (m_TargetPlatform == BuildTarget.GameCoreXboxOne)
                )
                {
                    reduceBuildLocationPathLength = true;
                }

                var uniqueTempPathInProject = FileUtil.GetUniqueTempPathInProject();
                var playerDirectoryName = "PlayerWithTests";

                //Some platforms hit MAX_PATH limits during the build process, in these cases minimize the path length
                if (reduceBuildLocationPathLength)
                {
                    playerDirectoryName = "PwT";
                    uniqueTempPathInProject = Path.GetTempFileName();
                    File.Delete(uniqueTempPathInProject);
                    Directory.CreateDirectory(uniqueTempPathInProject);
                }

                buildLocation = Path.Combine(string.IsNullOrEmpty(m_PlayerWithTestsPath) ? Path.GetFullPath(uniqueTempPathInProject) : m_PlayerWithTestsPath, playerDirectoryName);

                // iOS builds create a folder with Xcode project instead of an executable, therefore no executable name is added
                if (m_TargetPlatform == BuildTarget.iOS)
                {
                    buildOptions.locationPathName = buildLocation;
                }
                else
                {
                    string extensionForBuildTarget =
                        PostprocessBuildPlayer.GetExtensionForBuildTarget(buildTargetGroup, buildOptions.target,
                            buildOptions.options);
                    var playerExecutableName = "PlayerWithTests";

                    if (m_TargetPlatform == BuildTarget.GameCoreXboxSeries ||
                        m_TargetPlatform == BuildTarget.GameCoreXboxOne)
                        PlayerSettings.productName = playerExecutableName;

                    if (!string.IsNullOrEmpty(extensionForBuildTarget))
                        playerExecutableName += $".{extensionForBuildTarget}";

                    buildOptions.locationPathName = Path.Combine(buildLocation, playerExecutableName);
                }
            }

            var result = new PlayerLauncherBuildOptions
            {
                BuildPlayerOptions = buildOptions,
                PlayerDirectory = buildLocation,
            };

#if UNITY_6000_1_OR_NEWER
            BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions();
            options.locationPathName = buildOptions.locationPathName;
            options.assetBundleManifestPath = buildOptions.assetBundleManifestPath;
            options.options = buildOptions.options;
            options.buildProfile = null;
            result.BuildPlayerWithProfileOptions = options;
#endif

            return ModifyBuildOptions(result);
        }

        private PlayerLauncherBuildOptions ModifyBuildOptions(PlayerLauncherBuildOptions playerOptions)
        {
#if UNITY_6000_5_OR_NEWER
            var allAssemblies = CurrentAssemblies.GetLoadedAssemblies()
#else
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
#endif
                .Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEditor.TestRunner")).ToArray();
            var attributes = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(TestPlayerBuildModifierAttribute), true).OfType<TestPlayerBuildModifierAttribute>()).ToArray();
            var modifiers = attributes.Select(attribute => attribute.ConstructModifier()).ToArray();

            foreach (var modifier in modifiers)
            {
                playerOptions.BuildPlayerOptions = modifier.ModifyOptions(playerOptions.BuildPlayerOptions);
#if UNITY_6000_1_OR_NEWER
                playerOptions.BuildPlayerWithProfileOptions = modifier.ModifyOptions(playerOptions.BuildPlayerWithProfileOptions);
#endif
            }

            return playerOptions;
        }

        private static bool ShouldReduceBuildLocationPathLength(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.GameCoreXboxOne:
                case BuildTarget.GameCoreXboxSeries:
                case BuildTarget.WSAPlayer:
                    return true;
                default:
                    return false;
            }
        }
    }
}
