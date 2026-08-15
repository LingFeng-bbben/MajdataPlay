using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting
{
    internal class CoreSettings
    {
        private readonly PluginConfigurationItemMetadata _aotSafeMode;

        private const string Title = "Core Settings";

        readonly GUIContent _toggleAOTSafeModeLabel = new GUIContent("AOT Safe Mode");

        private bool _setting;

        public CoreSettings(BoltCoreConfiguration coreConfig)
        {
            _aotSafeMode = coreConfig.GetMetadata(nameof(BoltCoreConfiguration.aotSafeMode));

            _setting = (bool)_aotSafeMode.value;
        }

        private void SaveIfNeeded()
        {
            var settings = (bool)_aotSafeMode.value;

            if (_setting != settings)
            {
                _aotSafeMode.value = _setting;

                _aotSafeMode.SaveImmediately();
            }
        }

        public void OnGUI()
        {
            GUILayout.Space(5f);

            GUILayout.Label(Title, EditorStyles.boldLabel);

            GUILayout.Space(5f);

            _setting = GUILayout.Toggle(_setting, _toggleAOTSafeModeLabel);

            SaveIfNeeded();
        }
    }
}
