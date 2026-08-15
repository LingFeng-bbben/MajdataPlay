using System;

namespace UnityEditor.TestTools.TestRunner
{
    internal class SwitchPlatformSetup : IPlatformSetup
    {
        public void Setup()
        {
            EditorUserBuildSettings.switchCreateRomFile = true;
            EditorUserBuildSettings.switchNVNGraphicsDebugger = false;
            EditorUserBuildSettings.switchNVNDrawValidation_Heavy = true; // catches more graphics errors
            EditorUserBuildSettings.development = true;
#if UNITY_6000_0_OR_NEWER
            EditorUserBuildSettings.switchEnableHostIO = true;
#else
            EditorUserBuildSettings.switchRedirectWritesToHostMount = true;
#endif

            // We can use these when more debugging is required:
            //EditorUserBuildSettings.switchNVNDrawValidation = false; // cannot be used with shader debug
            //EditorUserBuildSettings.switchNVNGraphicsDebugger = true;
            //EditorUserBuildSettings.switchNVNShaderDebugging = true;
            //EditorUserBuildSettings.switchCreateSolutionFile = true; // for shorter iteration time
            //EditorUserBuildSettings.allowDebugging = true; // managed debugger can be attached
        }

        public void PostBuildAction()
        {
        }

        public void PostSuccessfulBuildAction()
        {
        }

        public void PostSuccessfulLaunchAction()
        {
        }

        public void CleanUp()
        {
        }
    }
}
