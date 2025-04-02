using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using System;
using TMPro;
using UnityEngine;
using KeyCode = MajdataPlay.IO.KeyCode;
#nullable enable
namespace MajdataPlay.Test
{
    internal class IOListener : MajComponent
    {
        public static string NextScene { get; set; } = "List";


        bool _exitFlag = false;
        bool _isWaitingForButtonPress = false;
        float _exitBtnPressTime = 0f;
        float _testBtnPressTime = 0f;
        int _selectedButton = -1;
        int _buttonMappingSubPage = 0;
        TestPages _currentPage = TestPages.Sensor;

        [SerializeField]
        GameObject _sensorParent;
        [SerializeField]
        GameObject _sensorTextParent;
        [SerializeField]
        GameObject _btnParent;
        [SerializeField]
        GameObject _btnStateTextParent;
        [SerializeField]
        GameObject _btnMappingParent;
        [SerializeField]
        GameObject _btnMappingTextParent;

        readonly int[] _buttonMappingIndexs = new int[]
        {
            0,4,8
        };

        readonly GameObject[] _btnObjects = new GameObject[12];
        readonly GameObject[] _btnStateObjects = new GameObject[12];
        readonly TextMeshPro[] _btnTexts = new TextMeshPro[12];
        readonly TextMeshPro[] _btnStateTexts = new TextMeshPro[12];

        readonly TextMeshPro[] _btnMappingNameTexts = new TextMeshPro[4];
        readonly TextMeshPro[] _btnMappingBindingKeyTexts = new TextMeshPro[4];

        readonly GameObject[] _sensorObjects = new GameObject[35];
        readonly Material[] _materials = new Material[35];
        readonly MeshRenderer[] _meshRenderers = new MeshRenderer[35];

        readonly ReadOnlyMemory<string> _buttonMappingNames = new string[12]
        {
            "Button 1",
            "Button 2",
            "Button 3",
            "Button 4",
            "Button 5",
            "Button 6",
            "Button 7",
            "Button 8",
            "TEST",
            "Select P1",
            "SERVICE",
            "Select P2",
        };
        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < 4; i++)
            {
                _btnMappingNameTexts[i] = _btnMappingParent.transform.GetChild(i).GetComponent<TextMeshPro>();
                _btnMappingBindingKeyTexts[i] = _btnMappingTextParent.transform.GetChild(i).GetComponent<TextMeshPro>();
            }
            for (var i = 0; i < _sensorParent.transform.childCount; i++)
            {
                var child = _sensorParent.transform.GetChild(i);
                _sensorObjects[i] = child.gameObject;
                _materials[i] = new Material(Shader.Find("Sprites/Default"));
                _meshRenderers[i] = child.GetComponent<MeshRenderer>();
                _meshRenderers[i].material = _materials[i];
                _materials[i].color = Color.blue;
            }
            for (var i = 0; i < _btnParent.transform.childCount; i++)
            {
                var child = _btnParent.transform.GetChild(i);
                var stateChild = _btnStateTextParent.transform.GetChild(i);
                _btnObjects[i] = child.gameObject;
                _btnStateObjects[i] = stateChild.gameObject;
                _btnTexts[i] = child.GetComponent<TextMeshPro>();
                _btnStateTexts[i] = stateChild.GetComponent<TextMeshPro>();
            }
        }
        void Update()
        {
            if (_exitFlag)
                return;
            switch (_currentPage)
            {
                case TestPages.Sensor:
                    SensorPageUpdate();
                    break;
                case TestPages.Button:
                    ButtonPageUpdate();
                    break;
                case TestPages.ButtonMapping:
                    ButtonMappingPageUpdate();
                    break; 
            }
            if (string.IsNullOrEmpty(NextScene))
                return;
            if (_exitBtnPressTime >= 5)
            {
                _exitFlag = true;
                MajInstances.SceneSwitcher.SwitchScene(NextScene);
                MajEnv.Mode = RunningMode.Play;
            }
        }
        void NextPage()
        {
            var currentPage = _currentPage;
            var nextPage = _currentPage + 1;
            if(nextPage > TestPages.ButtonMapping)
            {
                nextPage = (TestPages)0;
            }
            switch(currentPage)
            {
                case TestPages.Sensor:
                    _sensorParent.SetActive(false);
                    _sensorTextParent.SetActive(false);

                    _btnParent.SetActive(true);
                    _btnStateTextParent.SetActive(true);
                    break;
                case TestPages.Button:
                    _sensorParent.SetActive(true);
                    _sensorTextParent.SetActive(true);

                    _btnParent.SetActive(false);
                    _btnStateTextParent.SetActive(false);
                    break;
                case TestPages.ButtonMapping:
                    break;
            }
            _currentPage = nextPage;
        }
        void SensorPageUpdate()
        {
            var rawData = InputManager.TouchPanelRawData;
            foreach (var (i, state) in rawData.WithIndex())
            {
                if (i == 34)
                    continue;
                switch (state)
                {
                    case true:
                        _materials[i].color = Color.red;
                        break;
                    default:
                        _materials[i].color = Color.blue;
                        break;
                }
            }
            if(InputManager.IsButtonClickedInThisFrame(SensorArea.Test))
            {
                NextPage();
            }
            if(InputManager.IsButtonClickedInThisFrame(SensorArea.A5))
            {
                _exitBtnPressTime = 5;
            }
        }
        void ButtonPageUpdate()
        {
            const string ON_STRING = "ON";
            const string OFF_STRING = "OFF";
            Span<SensorArea> btns = stackalloc SensorArea[12]
            {
                SensorArea.A1,
                SensorArea.A2,
                SensorArea.A3,
                SensorArea.A4,
                SensorArea.A5,
                SensorArea.A6,
                SensorArea.A7,
                SensorArea.A8,
                SensorArea.Test,
                SensorArea.P1,
                SensorArea.Service,
                SensorArea.P2,
            };
            for (var i = 0; i < btns.Length; i++)
            {
                var state = InputManager.GetButtonStatusInThisFrame(btns[i]) is SensorStatus.On ? ON_STRING: OFF_STRING;
                _btnStateTexts[i].text = state;
            }
            if(_testBtnPressTime >= 5)
            {
                _testBtnPressTime = 0;
                NextPage();
            }
            else if (InputManager.CheckButtonStatusInThisFrame(SensorArea.Test, SensorStatus.On))
            {
                _testBtnPressTime += MajTimeline.DeltaTime;
            }
            else
            {
                _testBtnPressTime = 0;
            }
        }
        void ButtonMappingPageUpdate()
        {
            Span<KeyCode> availableKeys = stackalloc KeyCode[12]
            {
                KeyCode.B1,
                KeyCode.B2,
                KeyCode.B3,
                KeyCode.B4,
                KeyCode.B5,
                KeyCode.B6,
                KeyCode.B7,
                KeyCode.B8,
                KeyCode.Test,
                KeyCode.SelectP1,
                KeyCode.Service,
                KeyCode.SelectP2
            };
            Span<SensorArea> one = stackalloc SensorArea[2]
            {
                SensorArea.B1,
                SensorArea.E2
            };
            Span<SensorArea> two = stackalloc SensorArea[1]
            {
                SensorArea.B2
            };
            Span<SensorArea> three = stackalloc SensorArea[1]
            {
                SensorArea.B3
            };
            Span<SensorArea> four = stackalloc SensorArea[2]
            {
                SensorArea.B4,
                SensorArea.E4
            };
            ButtonMappingNextPage(0);

            if(_isWaitingForButtonPress)
            {
                Span<SensorArea> sensors = _selectedButton switch
                {
                    0 => one,
                    1 => two,
                    2 => three,
                    3 => four,
                    _ => throw new ArgumentOutOfRangeException()
                };
                bool isCancelRequested = false;
                foreach(var sensor in sensors)
                {
                    isCancelRequested |= InputManager.CheckSensorStatusInThisFrame(sensor, SensorStatus.On); 
                }
                if(isCancelRequested)
                {
                    _isWaitingForButtonPress = false;
                    _selectedButton = -1;
                    return;
                }
                var e = _buttonMappingIndexs[_buttonMappingSubPage];
                _btnMappingBindingKeyTexts[_selectedButton + e].text = "<Waiting4Press>";
                foreach(var key in availableKeys)
                {
                    if(Keyboard.IsKeyDown(key))
                    {
                        _isWaitingForButtonPress = false;
                        var area = GetButtonAreaFromIndex(_selectedButton + e);
                        InputManager.SetButtonNewBindingKey(area, key);
                        _selectedButton = -1;
                        break;
                    }
                }
            }
            else
            {

            }
        }
        void ButtonMappingNextPage(int diff)
        {
            var newPageIndex = (_buttonMappingSubPage + diff).Clamp(0, 2);
            var e = _buttonMappingIndexs[newPageIndex];
            for (var i = 0; i < 4; i++)
            {
                _btnMappingNameTexts[i].text = _buttonMappingNames.Span[i + e];
                var targetArea = GetButtonAreaFromIndex(i + e);
                var currentBindingKey = InputManager.GetButtonBindingKey(targetArea);
                if(currentBindingKey == KeyCode.Unset)
                {
                    _btnMappingBindingKeyTexts[i].text = "<Unset>";
                }
                else
                {
                    _btnMappingBindingKeyTexts[i].text = currentBindingKey.ToString();
                }
            }
        }
        SensorArea GetButtonAreaFromIndex(int index)
        {
            switch(index)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return (SensorArea)index;
                case 8:
                case 9:
                case 10:
                case 11:
                    return (SensorArea)(index + 25);
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        enum TestPages
        {
            Sensor,
            Button,
            ButtonMapping
        }
    }
}
