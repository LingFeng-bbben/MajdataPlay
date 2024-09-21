using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Setting
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

        Option[] options = Array.Empty<Option>();
        bool isBound = false;
        SettingManager manager;
        void Start()
        {
            var type = SubOptionObject.GetType();
            var properties = type.GetProperties();
            options = new Option[properties.Length];
            foreach(var (i,property) in properties.WithIndex())
            {
                var optionObj = Instantiate(optionPrefab, transform);
                var option = optionObj.GetComponent<Option>();
                options[i] = option;
                option.PropertyInfo = property;
                option.OptionObject = SubOptionObject;
                option.Parent = this;
                option.Index = i;
            }
            titleText.text = Name;
            BindArea();
            manager = FindObjectOfType<SettingManager>();
        }
        void OnEnable()
        {
            BindArea();
        }
        void OnDisable()
        {
            _selectedIndex = 0;
            UnbindArea();
        }
        void OnDestroy()
        {
            UnbindArea();
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            switch(e.Type)
            {
                case SensorType.A6:
                    _selectedIndex--;
                    if (_selectedIndex < 0)
                        manager.PreviousMenu();
                    _selectedIndex = _selectedIndex.Clamp(0, options.Length - 1);
                    break;
                case SensorType.A3:
                    _selectedIndex++;
                    if (_selectedIndex > options.Length - 1)
                        manager.NextMenu();

                    _selectedIndex = _selectedIndex.Clamp(0, options.Length - 1);
                    break;
                default:
                    return;
            }
        }
        public void ToLast() => _selectedIndex = options.Length - 1;
        public void ToFirst() => _selectedIndex = 0;
        void BindArea()
        {
            if (isBound)
                return;
            isBound = true;
            InputManager.Instance.BindButton(OnAreaDown, SensorType.A3);
            InputManager.Instance.BindButton(OnAreaDown, SensorType.A6);
        }
        void UnbindArea()
        {
            if (!isBound)
                return;
            isBound = false;
            InputManager.Instance.UnbindButton(OnAreaDown, SensorType.A3);
            InputManager.Instance.UnbindButton(OnAreaDown, SensorType.A6);
        }
        [SerializeField]
        int _selectedIndex = 0;
        [SerializeField]
        TextMeshPro titleText;
    }
}
