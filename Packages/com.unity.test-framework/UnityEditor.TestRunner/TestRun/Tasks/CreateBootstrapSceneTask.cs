using System;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.TestRunner;
using Object = UnityEngine.Object;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal class CreateBootstrapSceneTask : TestTaskBase
    {
        private bool m_includeTestController;
        private bool m_saveScene;
        private NewSceneSetup m_SceneSetup;

        public CreateBootstrapSceneTask(bool mIncludeTestController, bool saveScene, NewSceneSetup sceneSetup)
        {
            m_includeTestController = mIncludeTestController;
            m_saveScene = saveScene;
            m_SceneSetup = sceneSetup;
        }

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (m_saveScene)
            {
                testJobData.InitTestScenePath = "Assets/InitTestScene" + Guid.NewGuid() + ".unity";
            }

            testJobData.InitTestScene = EditorSceneManager.NewScene(m_SceneSetup, NewSceneMode.Single);

            /* This code from 2.0 is likely not needed and can be removed once backporting has finished.
            while (PlaymodeTestsController.IsControllerOnScene())
            {
                var gameObject = PlaymodeTestsController.GetController().gameObject;
                Object.DestroyImmediate(gameObject);
            }
            */

            var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(testJobData.executionSettings.filters
                .Select(filter => filter.ToRuntimeTestRunnerFilter(testJobData.executionSettings.runSynchronously)).ToArray(), testJobData.executionSettings.orderedTestNames,
                testJobData.executionSettings.randomOrderSeed, testJobData.executionSettings.featureFlags, testJobData.executionSettings.retryCount, testJobData.executionSettings.repeatCount, IsAutomated());

            if (m_includeTestController)
            {
                var go = new GameObject(PlaymodeTestsController.kPlaymodeTestControllerName);

                var editorLoadedTestAssemblyProvider =
                    new EditorLoadedTestAssemblyProvider(new EditorCompilationInterfaceProxy(),
                        new EditorAssembliesProxy());

                var runner = go.AddComponent<PlaymodeTestsController>();
                runner.AssembliesWithTests = editorLoadedTestAssemblyProvider
                    .GetAssembliesGroupedByType(TestPlatform.PlayMode).Select(x => x.Assembly.GetName().Name)
                    .ToList();
                runner.settings = settings;
                testJobData.PlaymodeTestsController = runner;
            }

            testJobData.PlayModeSettings = settings;

            if (m_saveScene)
            {
                EditorSceneManager.MarkSceneDirty(testJobData.InitTestScene);
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(testJobData.InitTestScene, testJobData.InitTestScenePath, false);
            }

            yield break;
        }
    }
}
