using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using UnityEngine;
#nullable enable
namespace MajdataPlay.SensorTest
{
    internal class IOListener: MajComponent
    {
        readonly GameObject[] _sensorObjects = new GameObject[35];
        readonly Material[] _materials = new Material[35];
        readonly MeshRenderer[] _meshRenderers = new MeshRenderer[35];

        InputManager _inputManager;
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
            _inputManager = MajInstances.InputManager;
        }
        void Update()
        {
            var rawData = _inputManager.GetTouchPanelRawData();
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
        }
    }
}
