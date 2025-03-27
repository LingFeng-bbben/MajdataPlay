using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Types;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.SensorTest
{
    internal class IOListener: MajComponent
    {
        public static string NextScene { get; set; } = "List";

        readonly GameObject[] _sensorObjects = new GameObject[35];
        readonly Material[] _materials = new Material[35];
        readonly MeshRenderer[] _meshRenderers = new MeshRenderer[35];

        bool _exitFlag = false;
        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                _sensorObjects[i] = child.gameObject;
                _materials[i] = new Material(Shader.Find("Sprites/Default"));
                _meshRenderers[i] = child.GetComponent<MeshRenderer>();
                _meshRenderers[i].material = _materials[i];
                _materials[i].color = Color.blue;
            }
        }
        void Update()
        {
            if (_exitFlag)
                return;
            var rawData = InputManager.GetTouchPanelRawData();
            foreach(var (i,state) in rawData.Span.WithIndex())
            {
                if (i == 34)
                    continue;
                switch(state)
                {
                    case true:
                        _materials[i].color = Color.red;
                        break;
                    default:
                        _materials[i].color = Color.blue;
                        break;
                }
            }
            if (string.IsNullOrEmpty(NextScene))
                return;
            if(InputManager.CheckButtonStatus(SensorArea.A5, SensorStatus.On)) 
            {
                _exitFlag = true;
                MajInstances.SceneSwitcher.SwitchScene(NextScene);
                MajEnv.Mode = RunningMode.Play;
            }
        }
    }
}
