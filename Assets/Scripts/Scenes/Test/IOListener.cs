using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using TMPro;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Test
{
    internal class IOListener : MajComponent
    {
        public static string NextScene { get; set; } = "List";


        bool _exitFlag = false;
        float _exitBtnPressTime = 0f;
        float _testBtnPressTime = 0f;
        TestPages _currentPage = TestPages.Sensor;

        GameObject _sensorParent;
        GameObject _sensorTextParent;
        GameObject _btnParent;
        GameObject _btnStateTextParent;

        readonly GameObject[] _btnObjects = new GameObject[12];
        readonly GameObject[] _btnStateObjects = new GameObject[12];
        readonly TextMeshPro[] _btnTexts = new TextMeshPro[12];
        readonly TextMeshPro[] _btnStateTexts = new TextMeshPro[12];

        readonly GameObject[] _sensorObjects = new GameObject[35];
        readonly Material[] _materials = new Material[35];
        readonly MeshRenderer[] _meshRenderers = new MeshRenderer[35];
        protected override void Awake()
        {
            base.Awake();
            _sensorParent = transform.GetChild(0).gameObject;
            _sensorTextParent = transform.GetChild(1).gameObject;
            _btnParent = transform.GetChild(2).gameObject;
            _btnStateTextParent = transform.GetChild(3).gameObject;
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
            if(nextPage > TestPages.Button)
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
            }
            _currentPage = nextPage;
        }
        void SensorPageUpdate()
        {
            var rawData = InputManager.GetTouchPanelRawData();
            foreach (var (i, state) in rawData.Span.WithIndex())
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
            if(InputManager.IsButtonClickedInThisFrame(ButtonZone.Test))
            {
                NextPage();
            }
            if(InputManager.IsButtonClickedInThisFrame(ButtonZone.A5))
            {
                _exitBtnPressTime = 5;
            }
        }
        void ButtonPageUpdate()
        {
            const string ON_STRING = "ON";
            const string OFF_STRING = "OFF";
            Span<ButtonZone> btns = stackalloc ButtonZone[12]
            {
                ButtonZone.A1,
                ButtonZone.A2,
                ButtonZone.A3,
                ButtonZone.A4,
                ButtonZone.A5,
                ButtonZone.A6,
                ButtonZone.A7,
                ButtonZone.A8,
                ButtonZone.Test,
                ButtonZone.P1,
                ButtonZone.Service,
                ButtonZone.P2,
            };
            for (var i = 0; i < btns.Length; i++)
            {
                var state = InputManager.GetButtonStatusInThisFrame(btns[i]) is SwitchStatus.On ? ON_STRING: OFF_STRING;
                _btnStateTexts[i].text = state;
            }
            if(_testBtnPressTime >= 5)
            {
                _testBtnPressTime = 0;
                NextPage();
            }
            else if (InputManager.CheckButtonStatusInThisFrame(ButtonZone.Test, SwitchStatus.On))
            {
                _testBtnPressTime += MajTimeline.DeltaTime;
            }
            else
            {
                _testBtnPressTime = 0;
            }
        }
        enum TestPages
        {
            Sensor,
            Button
        }
    }
}
