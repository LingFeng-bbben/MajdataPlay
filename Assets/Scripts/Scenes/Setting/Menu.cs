using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Numerics;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Scenes.Setting
{
    public class Menu : MonoBehaviour
    {
        public string Name { get; set; } = string.Empty;
        public int SelectedIndex => _selectedIndex;
        /// <summary>
        /// Option对象<para>e.g. GameSetting.Game</para>
        /// </summary>
        public object SubOptionObject { get; set; }
        public GameObject optionPrefab;

        float _lastWaitTime = 0;
        bool _isBound = false;
        Option[] _options = Array.Empty<Option>();
        SettingManager manager;
        void Start()
        {
            var type = SubOptionObject.GetType();
            var properties = type.GetProperties()
                                 .Where(x => x.GetCustomAttributes<SettingVisualizationIgnoreAttribute>().Count() == 0)
                                 .ToArray();
            _options = new Option[properties.Length];
            foreach(var (i,property) in properties.WithIndex())
            {
                var optionObj = Instantiate(optionPrefab, transform);
                var option = optionObj.GetComponent<Option>();
                _options[i] = option;
                option.PropertyInfo = property;
                option.OptionObject = SubOptionObject;
                option.Parent = this;
                option.Index = i;
            }

            var localizedText = $"MAJSETTING_CATEGORY_{Name}".i18n();
            titleText.text = localizedText;
            Localization.OnLanguageChanged += OnLangChanged;
            manager = FindObjectOfType<SettingManager>();
        }
        void OnDisable()
        {
            _selectedIndex = 0;
        }
        void OnDestroy()
        {
            Localization.OnLanguageChanged -= OnLangChanged;
        }
        void Update()
        {
            if(manager.IsPressed && manager.PressTime != 0)
            {
                if (manager.PressTime < 0.7f)
                {
                    return;
                }
                else if (_lastWaitTime < 0.2f)
                {
                    _lastWaitTime += Time.deltaTime;
                    return;
                }
                switch(manager.Direction)
                {
                    case 1:
                        NextOption();
                        _lastWaitTime = 0;
                        break;
                    case -1:
                        PreviousOption();
                        _lastWaitTime = 0;
                        break;
                }
            }
            else
            {
                _lastWaitTime = 0;
                if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A6))
                {
                    PreviousOption();
                }
                else if (InputManager.IsButtonClickedInThisFrame(ButtonZone.A3))
                {
                    NextOption();
                }
            }
        }
        void OnLangChanged(object? sender, Language newLanguage)
        {
            var localizedText = $"MAJSETTING_CATEGORY_{Name}".i18n();
            titleText.text = localizedText;
        }
        void PreviousOption()
        {
            _selectedIndex--;
            if (_selectedIndex < 0)
            {
                manager.PreviousMenu();
            }
            _selectedIndex = _selectedIndex.Clamp(0, _options.Length - 1);
        }
        void NextOption()
        {
            _selectedIndex++;
            if (_selectedIndex > _options.Length - 1)
            {
                manager.NextMenu();
            }
            _selectedIndex = _selectedIndex.Clamp(0, _options.Length - 1);
        }
        public void ToLast() => _selectedIndex = _options.Length - 1;
        public void ToFirst() => _selectedIndex = 0;
        [SerializeField]
        int _selectedIndex = 0;
        [SerializeField]
        TextMeshPro titleText;
    }
}
