using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting
{
    internal class LinkerPropertyProviderSettings
    {
        private readonly PluginConfigurationItemMetadata _linkerSettings;

        private const string Title = "Linker generation settings";

        private readonly GUIContent[] _toggleTargetsLabel =
        {
            new GUIContent("Scan graph assets"),
            new GUIContent("Scan scenes"),
            new GUIContent("Scan prefabs")
        };

        private Array _options = Enum.GetValues(typeof(BoltCoreConfiguration.LinkerScanTarget));
        private List<bool> _settings;

        public LinkerPropertyProviderSettings(BoltCoreConfiguration coreConfig)
        {
            _linkerSettings = coreConfig.GetMetadata(nameof(LinkerPropertyProviderSettings));

            _settings = new List<bool>((List<bool>)_linkerSettings.value);
        }

        private void SaveIfNeeded()
        {
            var settings = (List<bool>)_linkerSettings.value;

            if (!_settings.SequenceEqual(settings))
            {
                _linkerSettings.value = new List<bool>(_settings);

                _linkerSettings.SaveImmediately();
            }
        }

        public void OnGUI()
        {
            GUILayout.Space(5f);

            GUILayout.Label(Title, EditorStyles.boldLabel);

            GUILayout.Space(5f);

            var label = "Scan for types to be added to link.xml";

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(EditorGUIUtility.IconContent("console.infoicon"), GUILayout.ExpandWidth(true));
            GUILayout.Box(label, EditorStyles.wordWrappedLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            foreach (var option in _options)
            {
                _settings[(int)option] = GUILayout.Toggle(_settings[(int)option], _toggleTargetsLabel[(int)option]);
                GUILayout.Space(5f);
            }

            SaveIfNeeded();
        }
    }
}
