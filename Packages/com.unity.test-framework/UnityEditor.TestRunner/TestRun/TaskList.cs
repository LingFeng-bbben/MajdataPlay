using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Events;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Platform;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Player;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks.Scene;
using UnityEngine.TestTools;

namespace UnityEditor.TestTools.TestRunner.TestRun
{
    // Note: Indentation of the tasklist is purposefully incorrect, to ease comparison with the 2.0 tasklist.
    internal static class TaskList
    {
        public static IEnumerable<TestTaskBase> GetTaskList(ExecutionSettings settings)
        {
            if (settings == null)
            {
                yield break;
            }
                yield return new ClearDeveloperConsoleTask();
            if (settings.PlayerIncluded())
            {
                yield return new SaveModifiedSceneTask();
                yield return new StoreSceneSetupTask();
                yield return new CreateBootstrapSceneTask(true, true, NewSceneSetup.EmptyScene);
                yield return new DetermineRuntimePlatformTask();
                yield return new PlatformSpecificSetupTask();
                yield return new LegacyPlayerRunTask();
                yield return new PlatformSpecificPostBuildTask();
                yield return new PlatformSpecificSuccessfulBuildTask();
                yield return new PlatformSpecificSuccessfulLaunchTask();
                yield return new WaitForPlayerRunTask();
                yield return new PlatformSpecificCleanupTask();
                yield return new RestoreSceneSetupTask();
                yield return new DeleteBootstrapSceneTask();
                yield return new UnlockReloadAssembliesTask();
                yield break;
            }

            // ReSharper disable once BadControlBracesIndent
        var editMode = settings.EditModeIncluded() || (PlayerSettings.runPlayModeTestAsEditModeTest && settings.PlayModeInEditorIncluded());
        if (!editMode)
        {
            yield return new MarkRunAsPlayModeTask();
        }
            yield return new SaveModifiedSceneTask();
            yield return new RegisterFilesForCleanupVerificationTask();
            yield return new SaveUndoIndexTask();
            yield return new StoreSceneSetupTask();
            yield return new SetInteractionModeTask();
            yield return new RemoveAdditionalUntitledSceneTask();
            yield return new ReloadModifiedScenesTask();
            yield return new BuildNUnitFilterTask();
            yield return new BuildTestTreeTask(editMode ? TestPlatform.EditMode : TestPlatform.PlayMode);
            yield return new FilterTestTreeTask();
            yield return new CreateBootstrapSceneTask(!editMode, !editMode, editMode ? NewSceneSetup.DefaultGameObjects : NewSceneSetup.EmptyScene);
            yield return new CreateEventsTask();
            yield return new RegisterCallbackDelegatorEventsTask();
            yield return new RegisterTestRunCallbackEventsTask();
            yield return new PrebuildSetupWithTestDataTask(settings);
            yield return new PrebuildSetupTask();
            yield return new EnableTestOutLoggerTask();
            yield return new InitializeTestProgressTask();
            yield return new UpdateTestProgressTask();

        if (editMode)
        {
            yield return new GenerateContextTask();
            yield return new SetupConstructDelegatorTask();
            yield return new RunStartedInvocationEvent();
            yield return new EditModeRunTask();
            yield return new RunFinishedInvocationEvent();
            yield return new CleanupConstructDelegatorTask();
        }
        else
        {
            yield return new GenerateContextTask();
            yield return new PreparePlayModeRunTask();
            yield return new EnterPlayModeTask();
            yield return new PlayModeRunTask();
            yield return new ExitPlayModeTask();
            yield return new RestoreProjectSettingsTask();
            yield return new CleanupTestControllerTask();
        }
            yield return new PostbuildCleanupWithTestDataTask(settings);
            yield return new PostbuildCleanupTask();
            yield return new CleanUpContext();
            yield return new ResetInteractionModeTask();
            yield return new RestoreSceneSetupTask();
            yield return new DeleteBootstrapSceneTask();
            yield return new PerformUndoTask();
            yield return new CleanupVerificationTask();
            yield return new UnlockReloadAssembliesTask();
        }
    }
}
