using System;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools.TestRunner.Callbacks
{
    internal class TestResultRendererCallback : MonoBehaviour, ITestRunnerListener
    {
#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
        private TestResultRenderer m_ResultRenderer;
#endif

        public void RunStarted(ITest testsToRun)
        {
        }

        public void RunFinished(ITestResult testResults)
        {
#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
            if (Camera.main == null)
            {
                gameObject.AddComponent<Camera>();
            }
            m_ResultRenderer = new TestResultRenderer(testResults, gameObject.GetComponent<RemoteTestResultSender>());
            m_ResultRenderer.ShowResults();
#endif
        }

#if UNITY_MODULE_IMGUI && !DISABLE_TESTFRAMEWORK_GUI
        public void OnGUI()
        {
            if (m_ResultRenderer != null)
                m_ResultRenderer.Draw();
        }
#endif

        public void TestStarted(ITest test)
        {
        }

        public void TestFinished(ITestResult result)
        {
        }
    }
}
