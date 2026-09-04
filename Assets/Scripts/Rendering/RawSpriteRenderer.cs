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
#if UNITY_EDITOR 
using UnityEditor; 
#endif
#nullable enable
namespace MajdataPlay.Rendering
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class RawSpriteRenderer : MonoBehaviour
    {
        // ============================================================
        // Inspector
        // ============================================================

        [Header("Sprite")]
        [FormerlySerializedAs("sprite")]
        [SerializeField]
        private Sprite _sprite;

        [FormerlySerializedAs("color")]
        [SerializeField]
        private Color _color = Color.white;

        [SerializeField]
        private bool _flipX;

        [SerializeField]
        private bool _flipY;

        [Header("Sorting")]
        [SerializeField]
        private string _sortingLayerName = "Default";

        [SerializeField]
        private int _sortingOrder;

        [Header("Rendering")]
        [SerializeField]
        private Material _sharedMaterial;

        [SerializeField]
        private ShadowCastingMode _shadowCastingMode = ShadowCastingMode.Off;

        [SerializeField]
        private bool _receiveShadows;

        [SerializeField]
        private MotionVectorGenerationMode _motionVectorGenerationMode =
            MotionVectorGenerationMode.Object;

        // ============================================================
        // SpriteRenderer-like public API
        // ============================================================

        public Sprite sprite
        {
            get => _sprite;
            set
            {
                if (_sprite == value)
                    return;

                _sprite = value;
                MarkSpriteDirty();
            }
        }

        public Color color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;

                _color = value;
                MarkColorDirty();
            }
        }

        public bool flipX
        {
            get => _flipX;
            set
            {
                if (_flipX == value)
                    return;

                _flipX = value;
                MarkGeometryDirty();
            }
        }

        public bool flipY
        {
            get => _flipY;
            set
            {
                if (_flipY == value)
                    return;

                _flipY = value;
                MarkGeometryDirty();
            }
        }

        public string sortingLayerName
        {
            get => _sortingLayerName;
            set
            {
                value ??= "Default";

                if (_sortingLayerName == value)
                    return;

                _sortingLayerName = value;
                MarkSortingDirty();
            }
        }

        public int sortingLayerID
        {
            get
            {
                EnsureRenderer();
                return _meshRenderer != null
                    ? _meshRenderer.sortingLayerID
                    : SortingLayer.NameToID(_sortingLayerName);
            }

            set
            {
                EnsureRenderer();

                if (_meshRenderer == null)
                    return;

                if (_meshRenderer.sortingLayerID == value)
                    return;

                _meshRenderer.sortingLayerID = value;

                // 保证 Name 属性同步。
                _sortingLayerName = SortingLayer.IDToName(value);
            }
        }

        public int sortingOrder
        {
            get => _sortingOrder;
            set
            {
                if (_sortingOrder == value)
                    return;

                _sortingOrder = value;
                MarkSortingDirty();
            }
        }

        public Material sharedMaterial
        {
            get
            {
                EnsureRenderer();
                return _sharedMaterial != null
                    ? _sharedMaterial
                    : _meshRenderer != null
                        ? _meshRenderer.sharedMaterial
                        : null;
            }
            set
            {
                if (_sharedMaterial == value)
                    return;

                _sharedMaterial = value;
                ApplyMaterial();
            }
        }

        public ShadowCastingMode shadowCastingMode
        {
            get => _shadowCastingMode;
            set
            {
                if (_shadowCastingMode == value)
                    return;

                _shadowCastingMode = value;
                ApplyRendererSettings();
            }
        }

        public bool receiveShadows
        {
            get => _receiveShadows;
            set
            {
                if (_receiveShadows == value)
                    return;

                _receiveShadows = value;
                ApplyRendererSettings();
            }
        }

        public MotionVectorGenerationMode motionVectorGenerationMode
        {
            get => _motionVectorGenerationMode;
            set
            {
                if (_motionVectorGenerationMode == value)
                    return;

                _motionVectorGenerationMode = value;
                ApplyRendererSettings();
            }
        }

        /// <summary>
        /// 与 SpriteRenderer.enabled 对应。
        /// 实际上直接代理 MeshRenderer.enabled。
        /// </summary>
        public bool enabled
        {
            get
            {
                EnsureRenderer();
                return _meshRenderer != null && _meshRenderer.enabled;
            }
            set
            {
                EnsureRenderer();

                if (_meshRenderer != null)
                    _meshRenderer.enabled = value;
            }
        }

        public Bounds bounds
        {
            get
            {
                EnsureRenderer();
                return _meshRenderer != null
                    ? _meshRenderer.bounds
                    : default;
            }
        }

        public bool isVisible
        {
            get
            {
                EnsureRenderer();
                return _meshRenderer != null && _meshRenderer.isVisible;
            }
        }

        public Renderer rendererComponent
        {
            get
            {
                EnsureRenderer();
                return _meshRenderer;
            }
        }

        // ============================================================
        // Internal
        // ============================================================

        private static readonly int MainTexID =
            Shader.PropertyToID("_MainTex");

        private static readonly int RendererColorID =
            Shader.PropertyToID("_RendererColor");

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private Mesh _mesh;
        private MaterialPropertyBlock _propertyBlock;

        // ------------------------------------------------------------
        // Persistent Native buffers
        // ------------------------------------------------------------

        private NativeArray<Vector3> _positions;
        private NativeArray<Vector2> _uv0;
        private NativeArray<Vector3> _meshPositions;

        private NativeArray<ushort> _indices;

        private int _vertexCount;
        private int _indexCount;

        // ------------------------------------------------------------
        // Cached state
        // ------------------------------------------------------------

        private Sprite _appliedSprite;
        private Color _appliedColor;

        private bool _appliedFlipX;
        private bool _appliedFlipY;

        private int _appliedSortingLayerID;
        private int _appliedSortingOrder;

        private Material _appliedMaterial;

        private bool _spriteDirty = true;
        private bool _geometryDirty = true;
        private bool _colorDirty = true;
        private bool _sortingDirty = true;
        private bool _materialDirty = true;
        private bool _rendererSettingsDirty = true;

        // ============================================================
        // Unity lifecycle
        // ============================================================

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();

            _spriteDirty = true;
            _geometryDirty = true;
            _colorDirty = true;
            _sortingDirty = true;
            _materialDirty = true;
            _rendererSettingsDirty = true;

            ApplyAll();
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled)
                return;

            // ExecuteAlways 情况下，Inspector / Animation
            // 可能绕过 C# property setter，所以这里仍然做一次 cheap check。

            if (_sprite != _appliedSprite)
            {
                _spriteDirty = true;
                _geometryDirty = true;
            }

            if (_color != _appliedColor)
                _colorDirty = true;

            if (_flipX != _appliedFlipX ||
                _flipY != _appliedFlipY)
            {
                _geometryDirty = true;
            }

            if (_appliedSortingLayerID !=
                SortingLayer.NameToID(_sortingLayerName))
            {
                _sortingDirty = true;
            }

            if (_appliedSortingOrder != _sortingOrder)
                _sortingDirty = true;

            if (_sharedMaterial != _appliedMaterial)
                _materialDirty = true;

            ApplyDirty();
        }

#if UNITY_EDITOR

        private void OnValidate()
        {
            // OnValidate 可能发生在对象还没完成初始化的时候。
            if (!Application.isPlaying)
            {
                EditorApplication.delayCall += DelayedValidate;
            }
            else
            {
                MarkAllDirty();
            }
        }

        private void DelayedValidate()
        {
            if (this == null)
                return;

            if (gameObject == null)
                return;

            MarkAllDirty();

            if (isActiveAndEnabled)
                ApplyAll();
        }

#endif

        private void OnDisable()
        {
            // 不销毁 Mesh。
            // Disable/Enable 时继续复用，减少 native allocation。
        }

        private void OnDestroy()
        {
            DisposeNativeBuffers();

            if (_mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(_mesh);
                else
                    DestroyImmediate(_mesh);

                _mesh = null;
            }
        }

        // ============================================================
        // Initialization
        // ============================================================

        private void Initialize()
        {
            EnsureRenderer();
            EnsureMesh();
            EnsurePropertyBlock();
        }

        private void EnsureRenderer()
        {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();

            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void EnsureMesh()
        {
            if (_mesh != null)
                return;

            _mesh = new Mesh
            {
                name = "RawSpriteRenderer Mesh",
                hideFlags = HideFlags.HideAndDontSave
            };

            _mesh.MarkDynamic();

            _meshFilter.sharedMesh = _mesh;
        }

        private void EnsurePropertyBlock()
        {
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();
        }

        // ============================================================
        // Dirty
        // ============================================================

        private void MarkSpriteDirty()
        {
            _spriteDirty = true;
            _geometryDirty = true;
        }

        private void MarkGeometryDirty()
        {
            _geometryDirty = true;
        }

        private void MarkColorDirty()
        {
            _colorDirty = true;
        }

        private void MarkSortingDirty()
        {
            _sortingDirty = true;
        }

        private void MarkAllDirty()
        {
            _spriteDirty = true;
            _geometryDirty = true;
            _colorDirty = true;
            _sortingDirty = true;
            _materialDirty = true;
            _rendererSettingsDirty = true;
        }

        // ============================================================
        // Apply
        // ============================================================

        private void ApplyAll()
        {
            ApplyDirty();
        }

        private void ApplyDirty()
        {
            Initialize();

            if (_spriteDirty || _geometryDirty)
                ApplyGeometry();

            if (_colorDirty)
                ApplyColor();

            if (_sortingDirty)
                ApplySorting();

            if (_materialDirty)
                ApplyMaterial();

            if (_rendererSettingsDirty)
                ApplyRendererSettings();
        }

        // ============================================================
        // Sprite / Mesh
        // ============================================================

        private void ApplyGeometry()
        {
            _spriteDirty = false;
            _geometryDirty = false;

            Sprite currentSprite = _sprite;

            if (currentSprite == null)
            {
                ClearGeometry();

                _appliedSprite = null;
                _appliedFlipX = _flipX;
                _appliedFlipY = _flipY;

                return;
            }

            // Sprite 没变，只是 flip 改变。
            if (currentSprite == _appliedSprite)
            {
                ApplyFlipOnly(currentSprite);

                _appliedFlipX = _flipX;
                _appliedFlipY = _flipY;

                return;
            }

            _appliedSprite = currentSprite;

            NativeSlice<Vector3> sourcePositions =
                currentSprite.GetVertexAttribute<Vector3>(
                    VertexAttribute.Position);

            NativeSlice<Vector2> sourceUVs =
                currentSprite.GetVertexAttribute<Vector2>(
                    VertexAttribute.TexCoord0);

            NativeArray<ushort> sourceIndices =
                currentSprite.GetIndices();

            int vertexCount = sourcePositions.Length;
            int indexCount = sourceIndices.Length;

            if (vertexCount == 0 || indexCount == 0)
            {
                ClearGeometry();

                _appliedFlipX = _flipX;
                _appliedFlipY = _flipY;

                return;
            }

            EnsurePositionBuffers(vertexCount);
            EnsureUVBuffer(vertexCount);
            EnsureIndexBuffer(indexCount);

            // --------------------------------------------------------
            // Copy source Sprite data
            // --------------------------------------------------------

            sourcePositions.CopyTo(_positions);
            sourceUVs.CopyTo(_uv0);

            sourceIndices.CopyTo(_indices);

            _vertexCount = vertexCount;
            _indexCount = indexCount;

            // --------------------------------------------------------
            // Flip
            // --------------------------------------------------------

            BuildFlippedPositions(currentSprite);

            // --------------------------------------------------------
            // Upload
            // --------------------------------------------------------

            _mesh.Clear(false);

            _mesh.SetVertices(
                _meshPositions,
                0,
                _vertexCount,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);

            _mesh.SetUVs(
                0,
                _uv0,
                0,
                _vertexCount,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);

            _mesh.SetIndices(
                _indices,
                0,
                _indexCount,
                MeshTopology.Triangles,
                0,
                false,
                0);

            // Sprite.bounds 已经是 Sprite 几何对应的 local bounds。
            // 不需要 RecalculateBounds。
            _mesh.bounds = currentSprite.bounds;

            _meshFilter.sharedMesh = _mesh;

            _appliedFlipX = _flipX;
            _appliedFlipY = _flipY;

            // Texture 只在 Sprite 改变时更新。
            EnsurePropertyBlock();

            _meshRenderer.GetPropertyBlock(_propertyBlock);

            Texture texture = currentSprite.texture;
            _propertyBlock.SetTexture(MainTexID, texture);

            _meshRenderer.SetPropertyBlock(_propertyBlock);

            // sprite 已经处理完，不需要再次上传。
            _spriteDirty = false;
        }

        private void ApplyFlipOnly(Sprite currentSprite)
        {
            if (_mesh == null ||
                _vertexCount == 0)
            {
                return;
            }

            BuildFlippedPositions(currentSprite);

            _mesh.SetVertices(
                _meshPositions,
                0,
                _vertexCount,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);

            // Flip 后 bounds 不变。
            _mesh.bounds = currentSprite.bounds;
        }

        private void BuildFlippedPositions(Sprite spriteData)
        {
            NativeArray<Vector3> source = _positions;
            NativeArray<Vector3> destination = _meshPositions;

            // Sprite vertices 是以 Sprite pivot 为原点建立的。
            // 因此翻转需要围绕 pivot 原点做，而不是 Mesh center。

            for (int i = 0; i < _vertexCount; i++)
            {
                Vector3 p = source[i];

                if (_flipX)
                    p.x = -p.x;

                if (_flipY)
                    p.y = -p.y;

                destination[i] = p;
            }
        }

        private void ClearGeometry()
        {
            if (_mesh == null)
                return;

            _mesh.Clear(false);

            _vertexCount = 0;
            _indexCount = 0;

            // 清除纹理 MPB。
            EnsurePropertyBlock();

            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(MainTexID, null);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        // ============================================================
        // Color
        // ============================================================

        private void ApplyColor()
        {
            _colorDirty = false;

            _appliedColor = _color;

            EnsurePropertyBlock();

            _meshRenderer.GetPropertyBlock(_propertyBlock);

            // 与 SpriteRenderer / Sprites shader 使用方式一致。
            _propertyBlock.SetColor(
                RendererColorID,
                _color);

            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }

        // ============================================================
        // Sorting
        // ============================================================

        private void ApplySorting()
        {
            _sortingDirty = false;

            int sortingLayerID =
                SortingLayer.NameToID(_sortingLayerName);

            _meshRenderer.sortingLayerID = sortingLayerID;
            _meshRenderer.sortingOrder = _sortingOrder;

            _appliedSortingLayerID = sortingLayerID;
            _appliedSortingOrder = _sortingOrder;
        }

        // ============================================================
        // Material
        // ============================================================

        private void ApplyMaterial()
        {
            _materialDirty = false;

            Material material = _sharedMaterial;

            if (_meshRenderer.sharedMaterial != material)
                _meshRenderer.sharedMaterial = material;

            _appliedMaterial = material;
        }

        // ============================================================
        // Renderer settings
        // ============================================================

        private void ApplyRendererSettings()
        {
            _rendererSettingsDirty = false;

            _meshRenderer.shadowCastingMode =
                _shadowCastingMode;

            _meshRenderer.receiveShadows =
                _receiveShadows;

            _meshRenderer.motionVectorGenerationMode =
                _motionVectorGenerationMode;
        }

        // ============================================================
        // Native buffers
        // ============================================================

        private void EnsurePositionBuffers(int length)
        {
            if (_positions.IsCreated &&
                _positions.Length == length &&
                _meshPositions.IsCreated &&
                _meshPositions.Length == length)
            {
                return;
            }

            if (_positions.IsCreated)
                _positions.Dispose();

            if (_meshPositions.IsCreated)
                _meshPositions.Dispose();

            _positions = new NativeArray<Vector3>(
                length,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);

            _meshPositions = new NativeArray<Vector3>(
                length,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
        }

        private void EnsureUVBuffer(int length)
        {
            if (_uv0.IsCreated &&
                _uv0.Length == length)
            {
                return;
            }

            if (_uv0.IsCreated)
                _uv0.Dispose();

            _uv0 = new NativeArray<Vector2>(
                length,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
        }

        private void EnsureIndexBuffer(int length)
        {
            if (_indices.IsCreated &&
                _indices.Length == length)
            {
                return;
            }

            if (_indices.IsCreated)
                _indices.Dispose();

            _indices = new NativeArray<ushort>(
                length,
                Allocator.Persistent,
                NativeArrayOptions.UninitializedMemory);
        }

        private void DisposeNativeBuffers()
        {
            if (_positions.IsCreated)
                _positions.Dispose();

            if (_meshPositions.IsCreated)
                _meshPositions.Dispose();

            if (_uv0.IsCreated)
                _uv0.Dispose();

            if (_indices.IsCreated)
                _indices.Dispose();
        }
    }
}
