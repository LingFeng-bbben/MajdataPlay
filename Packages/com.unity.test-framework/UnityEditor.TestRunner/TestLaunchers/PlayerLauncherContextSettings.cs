using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner
{
    internal class PlayerLauncherContextSettings : IDisposable
    {
        private ITestRunSettings m_OverloadSettings;
        private NamedBuildTarget m_TargetPlatform;

        private EditorBuildSettingsScene[] m_EditorBuildSettings;
#pragma warning disable 618
        private ResolutionDialogSetting m_DisplayResolutionDialog;
#pragma warning restore 618
        private bool m_RunInBackground;
        private FullScreenMode m_FullScreenMode;
        private bool m_ResizableWindow;
        private bool m_ShowUnitySplashScreen;
        private string m_OldproductName;
#if !UNITY_6000_0_OR_NEWER
        private string m_OldAotOptions;
#endif
#pragma warning disable 618
        private Lightmapping.GIWorkflowMode m_OldLightmapping;
#pragma warning restore 618
        private bool m_explicitNullChecks;

        private ManagedCodeVariant m_OldManagedCodeVariant;

        private bool m_Disposed;

        public PlayerLauncherContextSettings(ITestRunSettings overloadSettings, BuildTarget targetPlatform)
        {
            m_OverloadSettings = overloadSettings;
            m_TargetPlatform = NamedBuildTarget.FromActiveSettings(targetPlatform);
            SetupProjectParameters();

            if (overloadSettings != null)
            {
                overloadSettings.Apply();
            }
        }

        public void Dispose()
        {
            if (!m_Disposed)
            {
                CleanupProjectParameters();
                if (m_OverloadSettings != null)
                {
                    m_OverloadSettings.Dispose();
                }

                m_Disposed = true;
            }
        }

        private void SetupProjectParameters()
        {
            EditorApplication.LockReloadAssemblies();

            m_EditorBuildSettings = EditorBuildSettings.scenes;

#pragma warning disable 618
            m_DisplayResolutionDialog = PlayerSettings.displayResolutionDialog;
            PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
#pragma warning restore 618

            m_RunInBackground = PlayerSettings.runInBackground;
            PlayerSettings.runInBackground = true;

            m_FullScreenMode = PlayerSettings.fullScreenMode;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

#if !UNITY_6000_0_OR_NEWER
            m_OldAotOptions = PlayerSettings.aotOptions;
            PlayerSettings.aotOptions = "nimt-trampolines=1024";
#endif

            m_ResizableWindow = PlayerSettings.resizableWindow;
            PlayerSettings.resizableWindow = true;

            m_ShowUnitySplashScreen = PlayerSettings.SplashScreen.show;
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;

            m_OldproductName = PlayerSettings.productName;
            PlayerSettings.productName = string.Join("_", Application.productName.Split(Path.GetInvalidFileNameChars()));

#pragma warning disable 618
            m_OldLightmapping = Lightmapping.giWorkflowMode;
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
#pragma warning restore 618

            m_explicitNullChecks = EditorUserBuildSettings.explicitNullChecks;
            EditorUserBuildSettings.explicitNullChecks = true;

            // Test players should have checks enabled so assertions fire.
            m_OldManagedCodeVariant = PlayerSettings.GetManagedCodeVariant(m_TargetPlatform);
            PlayerSettings.SetManagedCodeVariant(m_TargetPlatform, ManagedCodeVariant.Checked);
        }

        private void CleanupProjectParameters()
        {
            EditorBuildSettings.scenes = m_EditorBuildSettings;

            PlayerSettings.fullScreenMode = m_FullScreenMode;
            PlayerSettings.runInBackground = m_RunInBackground;
#pragma warning disable 618
            PlayerSettings.displayResolutionDialog = m_DisplayResolutionDialog;
#pragma warning restore 618
            PlayerSettings.resizableWindow = m_ResizableWindow;
            PlayerSettings.SplashScreen.show = m_ShowUnitySplashScreen;
            PlayerSettings.SplashScreen.showUnityLogo = m_ShowUnitySplashScreen;
            PlayerSettings.productName = m_OldproductName;
#if !UNITY_6000_0_OR_NEWER
            PlayerSettings.aotOptions = m_OldAotOptions;
#endif
#pragma warning disable 618
            Lightmapping.giWorkflowMode = m_OldLightmapping;
#pragma warning restore 618
            EditorUserBuildSettings.explicitNullChecks = m_explicitNullChecks;
            PlayerSettings.SetManagedCodeVariant(m_TargetPlatform, m_OldManagedCodeVariant);

            EditorApplication.UnlockReloadAssemblies();
        }
    }
}
