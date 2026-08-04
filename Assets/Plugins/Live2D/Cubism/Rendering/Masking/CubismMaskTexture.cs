/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace Live2D.Cubism.Rendering.Masking
{
    /// <summary>
    /// Texture for rendering masks.
    /// </summary>
    [CreateAssetMenu(menuName = "Live2D Cubism/Mask Texture")]
    public sealed class CubismMaskTexture : ScriptableObject, ICubismMaskCommandSource
    {
        #region Conversion

        /// <summary>
        /// Converts a <see cref="CubismMaskTexture"/> to a <see cref="Texture"/>.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static implicit operator Texture(CubismMaskTexture value)
        {
            return value.RenderTexture;
        }

        #endregion

        /// <summary>
        /// The global mask texture.
        /// </summary>
        public static CubismMaskTexture GlobalMaskTexture
        {
            get { return Resources.Load<CubismMaskTexture>("Live2D/Cubism/GlobalMaskTexture"); }
        }


        /// <summary>
        /// <see cref="Size"/> backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private int _size = 1024;

        /// <summary>
        /// Texture size in pixels.
        /// </summary>
        public int Size
        {
            get { return _size; }
            set
            {
                // Return early if same value given.
                if (value == _size)
                {
                    return;
                }


                // Fail silently if not power-of-two.
                if (!value.IsPowerOfTwo())
                {
                    return;
                }


                // Apply changes.
                _size = value;

                if (_renderTextureCount < 1)
                {
                    RefreshRenderTexture();
                }
                else
                {
                    RefreshRenderTextures();
                }
            }
        }


        /// <summary>
        /// Channel count.
        /// </summary>
        public int Channels
        {
            get { return 4; }
        }


        /// <summary>
        /// <see cref="Subdivisions"/> backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private int _subdivisions = 3;

        /// <summary>
        /// Subdivision level.
        /// </summary>
        public int Subdivisions
        {
            get { return _subdivisions; }
            set
            {
                if (value == _subdivisions)
                {
                    return;
                }


                // Apply changes.
                _subdivisions = value;


                if (_renderTextureCount < 1)
                {
                    RefreshRenderTexture();
                }
                else
                {
                    RefreshRenderTextures();
                }
            }
        }

        /// <summary>
        /// <see cref="RenderTextureCount"/> backing field.
        /// </summary>
        [SerializeField, HideInInspector]
        private int _renderTextureCount = 0;

        /// <summary>
        /// Number of <see cref="RenderTextures"/>.
        /// </summary>
        public int RenderTextureCount
        {
            get
            {
                return _renderTextureCount;
            }
            set
            {
                if (value == _renderTextureCount)
                {
                    return;
                }

                _renderTextureCount = value < 1 ? 0 : value;

                if (_renderTextureCount < 1)
                {
                    RefreshRenderTexture();
                }
                else
                {
                    RefreshRenderTextures();
                }
            }
        }

        /// <summary>
        /// Tile pool 'allocator'.
        /// </summary>
        private CubismMaskTilePool TilePool { get; set; }


        /// <summary>
        /// <see cref="RenderTexture"/> backing field.
        /// </summary>
        private RenderTexture _renderTexture;

        /// <summary>
        /// <see cref="RenderTexture"/> to draw on.
        /// </summary>
        private RenderTexture RenderTexture
        {
            get
            {
                if (_renderTexture == null)
                {
                    RefreshRenderTexture();
                }


                return _renderTexture;
            }
            set { _renderTexture = value; }
        }

        /// <summary>
        /// <see cref="RenderTextures"/> backing field.
        /// </summary>
        private RenderTexture[] _renderTextures;

        /// <summary>
        /// <see cref="RenderTexture"/> to draw on.
        /// </summary>
        public RenderTexture[] RenderTextures
        {
            get
            {
                if (_renderTextures == null)
                {
                    RefreshRenderTextures();
                }

                return _renderTextures;
            }
            private set { _renderTextures = value; }
        }

        /// <summary>
        /// Sources.
        /// </summary>
        private List<SourcesItem> Sources { get; set; }


        /// <summary>
        /// True if instance is revived.
        /// </summary>
        private bool IsRevived
        {
            get { return TilePool != null; }
        }

        /// <summary>
        /// True if instance contains any sources.
        /// </summary>
        private bool ContainsSources
        {
            get { return Sources != null && Sources.Count > 0; }
        }

        #region Interface For ICubismMaskSources

        /// <summary>
        /// Number of command buffers required.
        /// </summary>
        public int CountOfCommandBuffers
        {
            get { return RenderTextureCount; }
        }

        /// <summary>
        /// Add source of masks for drawing.
        /// </summary>
        public void AddSource(ICubismMaskTextureCommandSource source)
        {
            // Make sure instance is valid.
            TryRevive();


            // Initialize container if necessary.
            if (Sources == null)
            {
                Sources = new List<SourcesItem>();
            }


            // Return early if source already exists.
            else if (Sources.FindIndex(i => i.Source == source) != -1)
            {
                return;
            }

            TilePool.UsedMaskCount += source.GetNecessaryTileCount();
            // Register source.
            var item = new SourcesItem
            {
                Source = source,
                Tiles = TilePool.AcquireTiles(source.GetNecessaryTileCount())
            };


            Sources.Add(item);


            // Apply tiles to source.
            source.SetTiles(item.Tiles);
        }

        /// <summary>
        /// Remove source of masks
        /// </summary>
        public void RemoveSource(ICubismMaskTextureCommandSource source)
        {
            // Return early if empty.
            if (!ContainsSources)
            {
                return;
            }


            var itemIndex = Sources.FindIndex(i => i.Source == source);


            // Return if source is invalid.
            if (itemIndex == -1)
            {
                return;
            }

            TilePool.UsedMaskCount -= source.GetNecessaryTileCount();

            // Return tiles and deregister source.
            TilePool.ReturnTiles(Sources[itemIndex].Tiles);
            Sources.RemoveAt(itemIndex);
        }

        #endregion

        private void TryRevive()
        {
            var isUseRenderTextures = _renderTextureCount > 0;
            if (isUseRenderTextures)
            {
                // Prevent the contents of RenderTextures from becoming empty.
                RefreshRenderTextures();
            }

            // Return early if already revived.
            if (IsRevived || isUseRenderTextures)
            {
                return;
            }


            RefreshRenderTexture();
        }

        private void ReinitializeSources()
        {
            // Reallocate tiles if sources exist.
            if (ContainsSources)
            {
                for (var sourceIndex = 0; sourceIndex < Sources.Count; sourceIndex++)
                {
                    var source = Sources[sourceIndex];
                    TilePool.UsedMaskCount += source.Source.GetNecessaryTileCount();
                }

                for (var i = 0; i < Sources.Count; ++i)
                {
                    var source = Sources[i];


                    source.Tiles = TilePool.AcquireTiles(source.Source.GetNecessaryTileCount());

                    source.Source.SetTiles(source.Tiles);


                    Sources[i] = source;
                }
            }
        }

        private void RefreshRenderTexture()
        {
            // Recreate render texture.
            RenderTexture = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGB32);

            // return early.
            if (RenderTextureCount > 0)
            {
                RefreshRenderTextures();
                return;
            }

            // Recreate render textures.
            if (_renderTextures != null)
            {
                for (int renderTextureIndex = 0; renderTextureIndex < RenderTextures.Length; renderTextureIndex++)
                {
                    DestroyImmediate(RenderTextures[renderTextureIndex]);
                }
            }

            RenderTextures = new RenderTexture[0];

            // Recreate allocator.
            TilePool = new CubismMaskTilePool(Subdivisions, Channels);


            // Reinitialize sources.
            ReinitializeSources();
        }

        private void RefreshRenderTextures()
        {
            // Recreate render textures.
            if (_renderTextures != null)
            {
                for (int renderTextureIndex = 0; renderTextureIndex < RenderTextures.Length; renderTextureIndex++)
                {
                    DestroyImmediate(RenderTextures[renderTextureIndex]);
                }
            }

            RenderTextures = new RenderTexture[RenderTextureCount];

            for (int renderTextureIndex = 0; renderTextureIndex < RenderTextureCount; renderTextureIndex++)
            {
                RenderTextures[renderTextureIndex] = new RenderTexture(Size, Size, 0, RenderTextureFormat.ARGB32);
            }

            // Recreate allocator.
            TilePool = new CubismMaskTilePool(-1, Channels, RenderTextureCount);

            // Reinitialize sources.
            ReinitializeSources();
        }

        #region Unity Event Handling

        /// <summary>
        /// Initializes instance.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void OnEnable()
        {
            CubismMaskCommandBuffer.AddSource(this);
        }

        /// <summary>
        /// Finalizes instance.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private void OnDestroy()
        {
            CubismMaskCommandBuffer.RemoveSource(this);
        }

        #endregion

        #region ICubismMaskCommandSource

        /// <summary>
        /// Called to enqueue source.
        /// </summary>
        /// <param name="buffer">Buffer to enqueue in.</param>
        void ICubismMaskCommandSource.AddToCommandBuffer(CommandBuffer buffer, bool isUsingMultipleBuffer, int renderTextureIndex)
        {
            // Return early if empty or failed.
            if (!ContainsSources
                || (isUsingMultipleBuffer && (renderTextureIndex >= RenderTextureCount))
                || (isUsingMultipleBuffer && (renderTextureIndex < 0))
                || (!isUsingMultipleBuffer && RenderTextureCount > 0))
            {
                return;
            }


            // Enqueue render target.
            if (isUsingMultipleBuffer)
            {
                buffer.SetRenderTarget(RenderTextures[renderTextureIndex]);
            }
            else
            {
                buffer.SetRenderTarget(RenderTexture);
            }

            buffer.ClearRenderTarget(false, true, Color.clear);


            // Enqueue sources.
            for (var i = 0; i < Sources.Count; ++i)
            {
                Sources[i].Source.AddToCommandBuffer(buffer, isUsingMultipleBuffer, renderTextureIndex);
            }
        }

        #endregion

        #region Source Item

        /// <summary>
        /// Source of masks and its tiles
        /// </summary>
        private struct SourcesItem
        {
            /// <summary>
            /// SourcesItem instance.
            /// </summary>
            public ICubismMaskTextureCommandSource Source;

            /// <summary>
            /// Tiles assigned to the instance.
            /// </summary>
            public CubismMaskTile[] Tiles;
        }

        #endregion
    }
}
