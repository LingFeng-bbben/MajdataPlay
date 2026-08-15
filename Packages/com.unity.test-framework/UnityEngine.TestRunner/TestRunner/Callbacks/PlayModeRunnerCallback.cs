using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    [AddComponentMenu("")]
    internal class PlayModeRunnerCallback : MonoBehaviour, ITestRunnerListener
    {
#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
        private TestResultRenderer m_ResultRenderer;
#endif

        public void RunFinished(ITestResult testResults)
        {
            Application.logMessageReceived -= LogRecieved;
#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
            if (Camera.main == null)
            {
                gameObject.AddComponent<Camera>();
            }
            m_ResultRenderer = new TestResultRenderer(testResults, gameObject.GetComponent<RemoteTestResultSender>());
            m_ResultRenderer.ShowResults();
#endif
        }

        public void TestFinished(ITestResult result)
        {
        }

#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
        public void OnGUI()
        {
            if (m_ResultRenderer != null)
                m_ResultRenderer.Draw();
        }
#endif

        public void RunStarted(ITest testsToRun)
        {
            Application.logMessageReceived += LogRecieved;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= LogRecieved;
        }

        public void TestStarted(ITest test)
        {
        }

        private void LogRecieved(string message, string stacktrace, LogType type)
        {
            if (TestContext.Out != null)
                TestContext.Out.WriteLine(message);
        }
    }
}
