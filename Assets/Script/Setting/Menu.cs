using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MajdataPlay.Scenes
{
    public class Menu : MonoBehaviour
    {
        public int SelectedIndex => _selectedIndex;
        /// <summary>
        /// Option对象<para>e.g. GameSetting.Game</para>
        /// </summary>
        public object SubOptionObject { get; set; }
        public GameObject optionPrefab;
        Option[] options = Array.Empty<Option>();
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
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A4);
            InputManager.Instance.BindArea(OnAreaDown, SensorType.A1);
        }
        void OnDisable()
        {
            _selectedIndex = 0;
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A4);
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A1);
        }
        void OnDestroy()
        {
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A4);
            InputManager.Instance.UnbindArea(OnAreaDown, SensorType.A1);
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (!e.IsClick)
                return;
            switch(e.Type)
            {
                case SensorType.A1:
                    _selectedIndex = (--_selectedIndex).Clamp(0, options.Length - 1);
                    break;
                case SensorType.A4:
                    _selectedIndex = (++_selectedIndex).Clamp(0, options.Length - 1);
                    break;
                default:
                    return;
            }
        }
        [SerializeField]
        int _selectedIndex = 0;
    }
}
