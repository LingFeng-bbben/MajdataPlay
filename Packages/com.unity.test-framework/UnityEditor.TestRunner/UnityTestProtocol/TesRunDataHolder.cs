using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestRunner.UnityTestProtocol
{
    /// <summary>
    /// No longer in use.
    /// </summary>
    [Obsolete("No longer in use")]
    public interface ITestRunDataHolder
    {
        /// <summary>
        /// Gets the list of test run data.
        /// </summary>
        IList<TestRunData> TestRunDataList { get; }
    }

    /// <summary>
    /// No longer in use.
    /// </summary>
    [Obsolete("No longer in use")]
    public class TestRunDataHolder
        : ScriptableSingleton<TestRunDataHolder>,
            ISerializationCallbackReceiver,
            ITestRunDataHolder
    {
        [SerializeField]
        private TestRunData[] TestRunData;

        /// <inheritdoc/>
        public IList<TestRunData> TestRunDataList { get; private set; } = new List<TestRunData>();

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            TestRunData = TestRunDataList.ToArray();
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            TestRunDataList = TestRunData.ToList();
        }
    }
}
