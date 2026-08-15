using MajdataPlay.Collections;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace MajdataPlay
{
    internal sealed class DummyTouchPanelRenderer : MajComponent
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
        readonly SensorRenderer[] _sensorRenderers = new SensorRenderer[34];        

        protected override void Awake()
        {
            base.Awake();
            Majdata<DummyTouchPanelRenderer>.SetAsSingleton(this);
            foreach (var (index, child) in Transform.ToEnumerable().WithIndex())
            {
                var collider = child.GetComponent<MeshCollider>();
                var renderer = child.GetComponent<MeshRenderer>();
                var filter = child.GetComponent<MeshFilter>();

                renderer.sortingLayerName = "Debug";
                renderer.sortingOrder = short.MaxValue;

                _sensorRenderers[index] = new SensorRenderer(index, filter, renderer, collider, child.gameObject, _sharedInstancedMaterial);
                _instanceID2SensorIndexMappingTable[collider.GetInstanceID()] = index;
            }
        }

        internal void OnPreUpdate()
        {
            if (IsSensorRendererEnabled())
            {
                var tpRawData = InputManager.TouchPanelRawData;

                for (var i = 0; i < tpRawData.Length; i++)
                {
                    if (i == 34)
                    {
                        continue;
                    }
                    var state = tpRawData[i];
#if UNITY_EDITOR
                    _sensorRenderers[i].SetColor(state ? new Color(0, 0, 0, 0.4f) : new Color(0, 0, 0, 0.1f));
#else
                    _sensorRenderers[i].SetColor(state ? new Color(0, 0, 0, 0.3f) : new Color(0, 0, 0, 0f));
#endif
                }
            }
            else
            {
                for (var i = 0; i < _sensorRenderers.Length; i++)
                {
                    _sensorRenderers[i].SetColor(new Color(0, 0, 0, 0f));
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

            MaterialPropertyBlock _propBlock;

            static readonly int ColorPropId = Shader.PropertyToID("_BaseColor");

            public SensorRenderer(int index, MeshFilter meshFilter, MeshRenderer meshRenderer, MeshCollider meshCollider, GameObject gameObject, Material sharedMaterial)
            {
                Index = index;
                MeshFilter = meshFilter;
                MeshRenderer = meshRenderer;
                MeshCollider = meshCollider;
                GameObject = gameObject;

                MeshRenderer.sharedMaterial = sharedMaterial;
                _propBlock = new MaterialPropertyBlock();

                SetColor(new Color(0, 0, 0, 0f));
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetColor(Color color)
            {
                MeshRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(ColorPropId, color);
                MeshRenderer.SetPropertyBlock(_propBlock);
            }

            public void Destroy()
            {
                GameObject.Destroy(GameObject);
            }
        }
    }
}
