using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Collections;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Setting
{
    public class Option : MonoBehaviour
    {
        public int Index { get; set; } 
        public Menu Parent { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object OptionObject { get; set; }

        [SerializeField]
        TextMeshPro nameText;
        [SerializeField]
        TextMeshPro valueText;
        [SerializeField]
        TextMeshPro descriptionText;
        [ReadOnly]
        [SerializeField]
        int current = 0; // 当前选项的位置
        int maxOptionIndex = 0;
        [ReadOnly]
        [SerializeField]
        object[] options = Array.Empty<object>(); // 可用的选项

        float step = 1;
        bool isNum = false;
        bool isBound = false;
        bool isFloat = false;
        bool isReadOnly = false;
        bool isTriggering = false;
        float? maxValue = null;

        int lastIndex = 0;
        void Start()
        {
            nameText.text = Localization.GetLocalizedText(PropertyInfo.Name);
            //valueText.text = Localization.GetLocalizedText(PropertyInfo.GetValue(OptionObject).ToString());
            descriptionText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_DESC");
            InitOptions();
            UpdatePosition();
            UpdateOption();

            if (Parent.SelectedIndex == Index)
                BindArea();
            else
                UnbindArea();
        }
        void OnLangChanged(object? sender,Language newLanguage)
        {
            nameText.text = Localization.GetLocalizedText(PropertyInfo.Name);
            descriptionText.text = Localization.GetLocalizedText($"{PropertyInfo.Name}_MAJSETTING_DESC");
            UpdateOption();
        }
        void InitOptions()
        {
            var type = PropertyInfo.PropertyType;
            isFloat = type.IsFloatType();
            isNum = type.IsIntType() || isFloat;
            
            if (type.IsEnum)
            {
                var values = Enum.GetValues(type);
                maxOptionIndex = values.Length - 1;
                options = new object[values.Length];
                for (int i = 0; i < values.Length; i++)
                    options[i] = values.GetValue(i);
                var obj = PropertyInfo.GetValue(OptionObject);
                current = options.FindIndex(x => (int)x == (int)obj);
            }
            else if(type == typeof(bool))
            {
                options = new object[2] { true,false };
                var obj = PropertyInfo.GetValue(OptionObject);
                maxOptionIndex = 1;
                current = (bool)obj ? 0 : 1;
            }
            else if (isNum)
            {
                switch (PropertyInfo.Name)
                {
                    case "Answer":
                    case "BGM":
                    case "Tap":
                    case "Judge":
                    case "Slide":
                    case "Break":
                    case "Touch":
                    case "Voice":
                    case "OuterJudgeDistance":
                    case "InnerJudgeDistance":
                    case "BackgroundDim":
                        maxValue = 1;
                        step = 0.05f;
                        break;
                    case "TouchSpeed":
                    case "TapSpeed":
                        maxValue = null;
                        step = 0.25f;
                        break;
                    default:
                        maxValue = null;
                        step = 0.001f;
                        break;
                }
            }
            else // string
            {
                switch(PropertyInfo.Name)
                {
                    case "Resolution":
                        isReadOnly = true;
                        break;
                    case "Skin":
                        var skinManager = MajInstances.SkinManager;
                        var skinNames = skinManager.LoadedSkins.Select(x => x.Name)
                                                               .ToArray();
                        var currentSkin = skinManager.SelectedSkin;
                        options = skinNames;
                        maxOptionIndex = options.Length - 1;
                        current = skinNames.FindIndex(x => x == currentSkin.Name);
                        break;
                    case "Language":
                        var availableLangs = Localization.Available;
                        if(availableLangs.IsEmpty())
                        {
                            current = 0;
                            options = new object[] { "Unavailable" };
                            maxOptionIndex = 0;
                            isReadOnly = true;
                            PropertyInfo.SetValue(OptionObject, "Unavailable");
                            return;
                        }
                        var langNames = availableLangs.Select(x => x.ToString())
                                                      .ToArray();
                        var currentLang = Localization.Current;
                        options = langNames;
                        maxOptionIndex = options.Length - 1;
                        current = availableLangs.FindIndex(x => x == currentLang);
                        break;
                }
            }
        }
        void Update()
        {
            var currentIndex = Parent.SelectedIndex;
            if (lastIndex == currentIndex)
                return;

            if (currentIndex == Index)
                BindArea();
            else
                UnbindArea();
            lastIndex = currentIndex;
            UpdatePosition();
        }
        void UpdatePosition()
        {
            var diff = lastIndex - Index;
            var scale = GetScale(diff);
            var pos = GetPosition(diff);
            transform.localPosition = pos;
            transform.localScale = scale;
        }
        void UpdateOption()
        {
            var origin = PropertyInfo.GetValue(OptionObject).ToString();
            var localizedText = Localization.GetLocalizedText(origin);
            valueText.text = localizedText;
        }
        void Up()
        {
            if(isNum) // 数值类
            {
                var valueObj = PropertyInfo.GetValue(OptionObject);
                var value = (float)valueObj;
                value += step;
                value = MathF.Round(value, 3);
                if (maxValue is not null) //有上限
                    value = value.Clamp(0, (float)maxValue);
                PropertyInfo.SetValue(OptionObject, value);
            }
            else //非数值类
            {
                current++;
                current = current.Clamp(0, maxOptionIndex);
                PropertyInfo.SetValue(OptionObject, options[current]);
                switch(PropertyInfo.Name)
                {
                    case "Skin":
                        var skins = MajInstances.SkinManager.LoadedSkins;
                        var newSkin = skins.Find(x => x.Name == options[current].ToString());
                        MajInstances.SkinManager.SelectedSkin = newSkin;
                        break;
                }
            }
        }
        void Down()
        {
            if (isNum) // 数值类
            {
                var valueObj = PropertyInfo.GetValue(OptionObject);
                var value = (float)valueObj;
                value -= step;
                value = MathF.Round(value, 3);
                if (maxValue is not null) //有上限
                    value = value.Clamp(0, (float)maxValue);
                PropertyInfo.SetValue(OptionObject, value);
            }
            else //非数值类
            {
                current--;
                current = current.Clamp(0, maxOptionIndex);
                PropertyInfo.SetValue(OptionObject, options[current]);
                switch (PropertyInfo.Name)
                {
                    case "Skin":
                        var skins = MajInstances.SkinManager.LoadedSkins;
                        var newSkin = skins.Find(x => x.Name == options[current].ToString());
                        MajInstances.SkinManager.SelectedSkin = newSkin;
                        break;
                }
            }
        }
        void OnAreaDown(object sender, InputEventArgs e)
        {
            if (isReadOnly)
                return;
            else if(e.Status == SensorStatus.Off)
            {
                isTriggering = false;
                return;
            }
            else if (!e.IsClick)
                return;
            else if (e.IsButton)
                return;
            else if (isTriggering)
                return;
            isTriggering = true;
            switch (e.Type)
            {
                case SensorType.E4:
                case SensorType.B4:
                    Up();
                    break;
                case SensorType.E6:
                case SensorType.B5:
                    Down();
                    break;
            }
            UpdateOption();
        }
        void OnDestroy()
        {
            UnbindArea();
        }
        void OnDisable()
        {
            UnbindArea();
        }
        void OnEnable()
        {
            BindArea();
        }
        void BindArea()
        {
            if (isBound)
                return;
            else if(isReadOnly)
            {
                isBound = true;
                Localization.OnLanguageChanged += OnLangChanged;
                return;
            }
            else if (Parent == null)
                return;
            else if (Parent.SelectedIndex != Index)
                return;
            isBound = true;
            Localization.OnLanguageChanged += OnLangChanged;
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.E4);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.BindSensor(OnAreaDown, SensorType.E6);
        }
        void UnbindArea()
        {
            if (!isBound)
                return;
            else if (isReadOnly)
            {
                isBound = false; 
                Localization.OnLanguageChanged -= OnLangChanged;
                return;
            }
            isBound = false;
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E4);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.B5);
            MajInstances.InputManager.UnbindSensor(OnAreaDown, SensorType.E6);
        }
        Vector3 GetScale(int diff)
        {
            switch(diff)
            {
                case 1:
                case -1:
                    return new Vector3(0.7f, 0.7f, 0.7f);
                case 0:
                    return new Vector3(1, 1, 1);
                default:
                    return Vector3.zero;
            }
        }
        Vector3 GetPosition(int diff)
        {
            switch (diff)
            {
                case 1:
                    return new Vector3(-365, 0, 0);
                case -1:
                    return new Vector3(365, 0, 0);
                case 0:
                    return new Vector3(0, 0, 0);
                default:
                    return new Vector3(1000,0,0);
            }
        }
    }
}
