using MajdataPlay.Collections;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay
{
    internal sealed class DummyTouchPanelRenderer : MajSingleton
    {
        public IReadOnlyDictionary<int, int> InstanceID2SensorIndexMappingTable
        {
            get
            {
                return _instanceID2SensorIndexMappingTable;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("sharedInstancedMaterial")]
        Material _sharedInstancedMaterial;

        readonly Dictionary<int, int> _instanceID2SensorIndexMappingTable = new();
        readonly Memory<SensorRenderer> _sensorRenderers = new SensorRenderer[34];        

        protected override void Awake()
        {
            base.Awake();

            var sensorRenderers = _sensorRenderers.Span;
            foreach (var (index, child) in Transform.ToEnumerable().WithIndex())
            {
                var collider = child.GetComponent<MeshCollider>();
                var renderer = child.GetComponent<MeshRenderer>();
                var filter = child.GetComponent<MeshFilter>();

                // 2. 将共享材质传入初始化
                sensorRenderers[index] = new SensorRenderer(index, filter, renderer, collider, child.gameObject, _sharedInstancedMaterial);
                _instanceID2SensorIndexMappingTable[collider.GetInstanceID()] = index;
            }
        }

        internal void OnPreUpdate()
        {
            if (IsSensorRendererEnabled())
            {
                var sensorRenderers = _sensorRenderers.Span;
                foreach (var (i, state) in InputManager.TouchPanelRawData.WithIndex())
                {
                    if (i == 34) continue;

#if UNITY_EDITOR
                    sensorRenderers[i].SetColor(state ? new Color(0, 0, 0, 0.4f) : new Color(0, 0, 0, 0.1f));
#else
                    sensorRenderers[i].SetColor(state ? new Color(0, 0, 0, 0.3f) : new Color(0, 0, 0, 0f));
#endif
                }
            }
            else
            {
                foreach (var renderer in _sensorRenderers.Span)
                {
                    renderer.SetColor(new Color(0, 0, 0, 0f));
                }
            }
        }

        bool IsSensorRendererEnabled() => MajEnv.Settings.Debug.DisplaySensor;

        class SensorRenderer
        {
            public int Index { get; init; }
            public MeshFilter MeshFilter { get; init; }
            public MeshRenderer MeshRenderer { get; init; }
            public MeshCollider MeshCollider { get; init; }
            public GameObject GameObject { get; init; }

            // URP 对应的是 _BaseColor，如果使用内置管线请改为 _Color
            static readonly int ColorPropId = Shader.PropertyToID("_BaseColor");
            MaterialPropertyBlock _propBlock;

            public SensorRenderer(int index, MeshFilter meshFilter, MeshRenderer meshRenderer, MeshCollider meshCollider, GameObject gameObject, Material sharedMaterial)
            {
                Index = index;
                MeshFilter = meshFilter;
                MeshRenderer = meshRenderer;
                MeshCollider = meshCollider;
                GameObject = gameObject;

                // 3. 关键：使用 sharedMaterial，绝不产生材质实例副本
                MeshRenderer.sharedMaterial = sharedMaterial;
                _propBlock = new MaterialPropertyBlock();

                SetColor(new Color(0, 0, 0, 0f));
            }

            public void SetColor(Color color)
            {
                // 4. 通过 PropertyBlock 修改颜色，保证 GPU 合批不被打破
                MeshRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(ColorPropId, color);
                MeshRenderer.SetPropertyBlock(_propBlock);
            }

            public void Destroy()
            {
                GameObject.Destroy(GameObject);
                // 共享材质由外部统一生命周期，此处无需 Destroy(_material)
            }
        }
    }
}
