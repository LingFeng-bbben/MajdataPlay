using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using Object = UnityEngine.Object;
#nullable enable
namespace MajdataPlay.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RawSpriteRenderer : MonoBehaviour
    {
        [Header("Sprite Settings")]
        public Sprite sprite;
        public Color color = Color.white;

        [Header("Sorting Settings")]
        public string sortingLayerName = "Default";
        public int sortingOrder = 0;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private MaterialPropertyBlock _propBlock;

        private Sprite _lastSprite;
        private Color _lastColor;

        private string _lastSortingLayerName;
        private int _lastSortingLayerID;

        // 记录顶点数量，供单独更新颜色时使用
        private int _currentVertexCount = 0;

        void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "CustomSpriteMesh" };
                _mesh.MarkDynamic();
            }
            _meshFilter.sharedMesh = _mesh;

            if (_propBlock == null)
            {
                _propBlock = new MaterialPropertyBlock();
            }

            UpdateSpriteData();
            UpdateSorting();
        }

        void LateUpdate()
        {
            if (sprite != _lastSprite)
            {
                UpdateSpriteData();
            }
            else if (color != _lastColor)
            {
                UpdateColorsOnly();
            }

            UpdateSorting();
        }

        private void UpdateSpriteData()
        {
            _lastSprite = sprite;
            _lastColor = color;

            if (sprite == null)
            {
                _mesh.Clear();
                _currentVertexCount = 0;
                return;
            }

            // 1. 直接获取 Sprite 底层 C++ 内存的切片视图 (0分配, 0 GC)
            NativeSlice<Vector3> spritePositions = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position);
            NativeSlice<Vector2> spriteUVs = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0);

            // 返回的是 NativeArray，但它指向内部数据，不需要手动 Dispose
            NativeArray<ushort> spriteIndices = sprite.GetIndices();

            _currentVertexCount = spritePositions.Length;

            // 2. 使用 Allocator.Temp 在栈内存上极速分配临时数组 (0 GC, 极速)
            // UninitializedMemory 表示不花费 CPU 循环去清零数组，因为我们马上会覆盖它
            using var tempPositions = new NativeArray<Vector3>(_currentVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            using var tempUVs = new NativeArray<Vector2>(_currentVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var tempColors = new NativeArray<Color>(_currentVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            try
            {
                // 3. 内存块极速拷贝 (比 for 循环赋值快得多)
                spritePositions.CopyTo(tempPositions);
                spriteUVs.CopyTo(tempUVs);

                // 颜色只能手动循环填充
                for (int i = 0; i < _currentVertexCount; i++)
                {
                    tempColors[i] = color;
                }

                _mesh.Clear(false);

                // 4. 将 NativeArray 直接推送到 Mesh (Unity 底层直接 memcpy 进显存，0 GC)
                _mesh.SetVertices(tempPositions);
                _mesh.SetUVs(0, tempUVs);
                _mesh.SetColors(tempColors);

                // 索引数组可以直接传入，连 Temp 拷贝都省了！
                _mesh.SetIndices(spriteIndices, MeshTopology.Triangles, 0, false);

                _mesh.RecalculateBounds();

                // 5. 更新材质
                if (sprite.texture != null)
                {
                    _meshRenderer.GetPropertyBlock(_propBlock);
                    _propBlock.SetTexture("_MainTex", sprite.texture);
                    _meshRenderer.SetPropertyBlock(_propBlock);
                }
            }
            finally
            {
                tempColors.Dispose();
            }
        }

        private void UpdateColorsOnly()
        {
            _lastColor = color;

            if (sprite == null || _currentVertexCount == 0) return;

            // 当只有颜色改变时，利用栈内存分配临时颜色数组即可，瞬间完成
            var tempColors = new NativeArray<Color>(_currentVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            try
            {
                for (int i = 0; i < _currentVertexCount; i++)
                {
                    tempColors[i] = color;
                }

                _mesh.SetColors(tempColors);
            }
            finally
            {
                tempColors.Dispose();
            }
        }

        private void UpdateSorting()
        {
            if (_lastSortingLayerName != sortingLayerName)
            {
                _lastSortingLayerName = sortingLayerName;
                _lastSortingLayerID = SortingLayer.NameToID(sortingLayerName);
            }

            if (_meshRenderer.sortingLayerID != _lastSortingLayerID)
            {
                _meshRenderer.sortingLayerID = _lastSortingLayerID;
            }

            if (_meshRenderer.sortingOrder != sortingOrder)
            {
                _meshRenderer.sortingOrder = sortingOrder;
            }
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_mesh);
                else
                    DestroyImmediate(_mesh);
            }
        }
    }
}
