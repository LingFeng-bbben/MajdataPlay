using MajdataPlay.Collections;
using MajdataPlay.Extensions;
using MajdataPlay.IO;
using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                sensorRenderers[index] = new SensorRenderer(index, filter, renderer, collider, child.gameObject);
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
                    if (i == 34)
                        continue;
#if UNITY_EDITOR
                    sensorRenderers[i].Color = state ? new Color(0, 0, 0, 0.4f) : new Color(0, 0, 0, 0.1f);
#else
                    sensorRenderers[i].Color = state ? new Color(0, 0, 0, 0.3f) : new Color(0, 0, 0, 0f);
#endif
                }
            }
        }
        bool IsSensorRendererEnabled()
        {
            return MajInstances.Settings.Debug.DisplaySensor;
        }
        class SensorRenderer
        {
            public int Index { get; init; }
            public MeshFilter MeshFilter { get; init; }
            public MeshRenderer MeshRenderer { get; init; }
            public MeshCollider MeshCollider { get; init; }
            public GameObject GameObject { get; init; }
            public Color Color
            {
                get => _material.color;
                set => _material.color = value;
            }
            Material _material;
            public SensorRenderer(int index, MeshFilter meshFilter, MeshRenderer meshRenderer, MeshCollider meshCollider, GameObject gameObject)
            {
                Index = index;
                MeshFilter = meshFilter;
                MeshRenderer = meshRenderer;
                MeshCollider = meshCollider;
                _material = new Material(Shader.Find("Sprites/Default"));
                MeshRenderer.material = _material;
                GameObject = gameObject;
                Color = new Color(0, 0, 0, 0f);
            }
            public void Destroy()
            {
                GameObject.Destroy(GameObject);
                GameObject.Destroy(_material);
            }
        }
    }
}
