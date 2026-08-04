/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */


using System;
using UnityEngine;


namespace Live2D.Cubism.Rendering.Masking
{
    /// <summary>
    /// Virtual pool allocator for <see cref="CubismMaskTile"/>s.
    /// </summary>
    internal sealed class CubismMaskTilePool
    {
        private readonly int ClippingMaskMaxCountOnDefault = 36; // Maximum number of masks per normal frame buffer.
        private readonly int ClippingMaskMaxCountOnMultiRenderTexture = 32; // Maximum number of masks per frame buffer when there are two or more frame buffers.

        /// <summary>
        /// Level of subdivisions.
        /// </summary>
        private int Subdivisions { get; set; }

        /// <summary>
        /// Pool slots.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> slots are in use, <see langword="false"/> are available slots.
        /// </remarks>
        private bool[] Slots { get; set; }

        /// <summary>
        /// Number of <see cref="RenderTexture"/>.
        /// </summary>
        private int RenderTextureCount { get; set; }

        /// <summary>
        /// High precision mask flags.
        /// </summary>
        private bool IsUsingHighPrecisionMask { get; set; }

        /// <summary>
        /// Limit on number of clipping masks.
        /// </summary>
        private int UseClippingMaskMaxCount { get; set; }

        /// <summary>
        /// Channel count.
        /// </summary>
        private int Channels { get; set; }

        /// <summary>
        /// Number of masks used.
        /// </summary>
        public int UsedMaskCount { get; set; }

        /// <summary>
        /// Array of structures holding information for each mask tile.
        /// </summary>
        private LayoutContext[] LayoutContexts { get; set; }

        /// <summary>
        /// <see cref="HeadOfChannels"/> backing field.
        /// </summary>
        private LayoutContext[] _headOfChannels;

        /// <summary>
        /// Top of channel.
        /// </summary>
        private LayoutContext[] HeadOfChannels
        {
            get { return _headOfChannels; }
            set { _headOfChannels = value; }
        }

        #region Ctors

        /// <summary>
        /// Initializes instance.
        /// </summary>
        /// <param name="subdivisions">Number of <see cref="CubismMaskTexture"/> subdivisions.</param>
        /// <param name="channels">Number of <see cref="CubismMaskTexture"/> color channels.</param>
        public CubismMaskTilePool(int subdivisions, int channels, int renderTextureCount = -1)
        {
            RenderTextureCount = renderTextureCount;
            Channels = channels;

            if (RenderTextureCount < 1)
            {
#if UNITY_EDITOR
                Debug.Log("This MaskTexture use system: Subdivisions (Legacy)");
#endif

                Subdivisions = subdivisions;


                Slots = new bool[(int)Mathf.Pow(4, subdivisions) * Channels];
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("This MaskTexture use system: Multiple RenderTexture");
#endif
                UseClippingMaskMaxCount = renderTextureCount > 1
                    ? ClippingMaskMaxCountOnMultiRenderTexture * renderTextureCount
                    : ClippingMaskMaxCountOnDefault;

                Slots = new bool[UseClippingMaskMaxCount * Channels];
                UsedMaskCount = 0;

                LayoutContexts = new LayoutContext[UseClippingMaskMaxCount * Channels];
                for (int layoutContextIndex = 0; layoutContextIndex < LayoutContexts.Length; layoutContextIndex++)
                {
                    LayoutContexts[layoutContextIndex] = new LayoutContext
                    {
                        RenderTextureIndex = 0,
                        Channel = 0,
                        LayoutCount = 0,
                        LayoutCountContextIndex = 0,
                    };
                }

                HeadOfChannels = new LayoutContext[0];
            }
        }

        #endregion

        /// <summary>
        /// Acquires tiles.
        /// </summary>
        /// <param name="count">Number of tiles to acquire.</param>
        /// <returns>Acquired tiles on success; <see langword="null"/> otherwise.</returns>
        public CubismMaskTile[] AcquireTiles(int count)
        {
            var result = new CubismMaskTile[count];


            // Populate container.
            for (var i = 0; i < count; ++i)
            {
                var allocationSuccessful = false;


                for (var j = 0; j < Slots.Length; ++j)
                {
                    // Skip occupied slots.
                    if (Slots[j])
                    {
                        continue;
                    }


                    // Generate tile.
                    result[i] = ToTile(j);


                    // Flag slot as occupied.
                    Slots[j] = true;


                    // Flag allocation as successful.
                    allocationSuccessful = true;


                    break;
                }


                // Return as soon as one allocation fails.
                if (!allocationSuccessful)
                {
                    Debug.LogError("The currently specified mask texture exceeds the number of masks that can be drawn.");
                    return null;
                }
            }


            // Return on success.
            return result;
        }

        /// <summary>
        /// Releases tiles.
        /// </summary>
        /// <param name="tiles">Tiles to release.</param>
        public void ReturnTiles(CubismMaskTile[] tiles)
        {
            // Flag slots as available.
            for (var i = 0; i < tiles.Length; ++i)
            {
                Slots[ToIndex(tiles[i])] = false;
            }
        }


        /// <summary>
        /// Converts from index to <see cref="CubismMaskTile"/>.
        /// </summary>
        /// <param name="index">Index to convert.</param>
        /// <returns>Mask tile matching index.</returns>
        private CubismMaskTile ToTile(int index)
        {
            if (RenderTextureCount > 0)
            {
                if (UsedMaskCount > UseClippingMaskMaxCount)
                {
                    var overCount = UsedMaskCount - UseClippingMaskMaxCount;
                    Debug.LogError($"Not supported mask count : {overCount}\n[Details] render texture count : {RenderTextureCount}, mask count : {UsedMaskCount}");
                    return new CubismMaskTile
                    {
                        Channel = 0,
                        Column = 0,
                        Row = 0,
                        Size = 1,
                        RenderTextureIndex = 0
                    };
                }

                // If there is one render texture, divide it into 9 pieces (maximum 36 pieces).
                var layoutCountMaxValue = RenderTextureCount <= 1 ? 9 : 8;

                var countPerSheetDiv = UsedMaskCount / RenderTextureCount;
                var countPerSheetMod = UsedMaskCount % RenderTextureCount;

                // Use RGBA in sequence.
                var div = countPerSheetDiv / Channels; // Number of masks to be placed in one channel.
                var mod = countPerSheetDiv % Channels; // Excess. Allocate one by one to this numbered channel.

                // Start the calculation as the first index of that channel.
                if (LayoutContexts[index].LayoutCount < 1)
                {
                    if (HeadOfChannels.Length < 1)
                    {
                        LayoutContexts[index].Channel = 0;
                    }
                    else
                    {
                        LayoutContexts[index].Channel = HeadOfChannels[HeadOfChannels.Length - 1].Channel < (Channels - 1)
                            ? (HeadOfChannels[HeadOfChannels.Length - 1].Channel + 1) : 0;
                    }

                    var checkChannelNo = mod + 1 >= Channels ? 0 : mod;
                    var calcCountPerSheetDiv = countPerSheetDiv;

                    var addCountPerSheetDiv = 0;
                    if (countPerSheetDiv < 1)
                    {
                        addCountPerSheetDiv = countPerSheetMod > 0 ? 1 : 0;
                    }
                    else
                    {
                        addCountPerSheetDiv = (index % countPerSheetDiv) < countPerSheetMod ? 1 : 0;
                    }
                    calcCountPerSheetDiv += addCountPerSheetDiv;

                    var assignedRenderTextureIndex = (index / calcCountPerSheetDiv);

                    LayoutContexts[index].RenderTextureIndex = assignedRenderTextureIndex;

                    var addLayout = 0;
                    if ((LayoutContexts[index].Channel > mod && div < 1)
                        || (LayoutContexts[index].Channel < mod))
                    {
                        addLayout = 1;
                    }

                    LayoutContexts[index].LayoutCount = div + addLayout;
                    LayoutContexts[index].LayoutCountContextIndex = 0;

                    if (LayoutContexts[index].LayoutCount < layoutCountMaxValue
                        && LayoutContexts[index].Channel == checkChannelNo)
                    {
                        LayoutContexts[index].LayoutCount += (LayoutContexts[index].RenderTextureIndex < countPerSheetMod ? 1 : 0);
                    }

                    for (int count = 1; count < LayoutContexts[index].LayoutCount; count++)
                    {
                        LayoutContexts[index + count].RenderTextureIndex = LayoutContexts[index].RenderTextureIndex;
                        LayoutContexts[index + count].Channel = LayoutContexts[index].Channel;
                        LayoutContexts[index + count].LayoutCount = LayoutContexts[index].LayoutCount;
                        LayoutContexts[index + count].LayoutCountContextIndex = count;
                    }

                    Array.Resize(ref _headOfChannels, HeadOfChannels.Length + 1);
                    HeadOfChannels[HeadOfChannels.Length - 1] = LayoutContexts[index];
                }

                var assignedChannelIndex = div < 1 ? index : (index / div); // What number of Channel to assign.
                while (assignedChannelIndex > Channels - 1)
                {
                    assignedChannelIndex = (assignedChannelIndex % Channels);
                }

                var layoutCount = LayoutContexts[index].LayoutCount;

                if (layoutCount == 0)
                {
                    Debug.LogError("\"layerCount contains\" an unexpected value.");

                    // Tentative creation as nothing can be returned...
                    return new CubismMaskTile
                    {
                        Channel = 0,
                        Column = 0,
                        Row = 0,
                        Size = 1f,
                        RenderTextureIndex = 0
                    };
                }
                else if (layoutCount == 1)
                {
                    return new CubismMaskTile
                    {
                        Channel = LayoutContexts[index].Channel,
                        Column = 0,
                        Row = 0,
                        Size = 1,
                        RenderTextureIndex = LayoutContexts[index].RenderTextureIndex
                    };
                }
                else if (layoutCount <= 4)
                {
                    var tilesPerRow = 2; // Rows per tile
                    var currentTilePosition = LayoutContexts[index].LayoutCountContextIndex;
                    var tileSize = 1f / (float)tilesPerRow;
                    var column = currentTilePosition / tilesPerRow;
                    var rowId = currentTilePosition % tilesPerRow;

                    return new CubismMaskTile
                    {
                        Channel = LayoutContexts[index].Channel,
                        Column = column,
                        Row = rowId,
                        Size = tileSize,
                        RenderTextureIndex = LayoutContexts[index].RenderTextureIndex
                    };
                }
                else if (layoutCount <= layoutCountMaxValue)
                {
                    var tilesPerRow = 3; // Rows per tile

                    var currentTilePosition = LayoutContexts[index].LayoutCountContextIndex;
                    var tileSize = 1f / (float)tilesPerRow;
                    var column = currentTilePosition / tilesPerRow;
                    var rowId = currentTilePosition % tilesPerRow;

                    return new CubismMaskTile
                    {
                        Channel = LayoutContexts[index].Channel,
                        Column = column,
                        Row = rowId,
                        Size = tileSize,
                        RenderTextureIndex = LayoutContexts[index].RenderTextureIndex
                    };
                }
                else
                {
                    var overCount = UsedMaskCount - UseClippingMaskMaxCount;
                    Debug.LogError($"Not supported mask count : {overCount}\n[Details] render texture count : {RenderTextureCount}, mask count : {UsedMaskCount}");
                    return new CubismMaskTile
                    {
                        Channel = 0,
                        Column = 0,
                        Row = 0,
                        Size = 1f,
                        RenderTextureIndex = 0
                    };
                }
            }
            else
            {
                var tileCounts = (int)Mathf.Pow(4, Subdivisions - 1);
                var tilesPerRow = (int)Mathf.Pow(2, Subdivisions - 1);
                var tileSize = 1f / (float)tilesPerRow;


                var channel = index / tileCounts;
                var currentTilePosition = index - (channel * tileCounts);
                var column = currentTilePosition / tilesPerRow;
                var rowId = currentTilePosition % tilesPerRow;

                return new CubismMaskTile
                {
                    Channel = channel,
                    Column = column,
                    Row = rowId,
                    Size = tileSize
                };
            }
        }

        /// <summary>
        /// Converts from <see cref="CubismMaskTile"/> to index.
        /// </summary>
        /// <param name="tile">Tile to convert.</param>
        /// <returns>Tile index.</returns>
        private int ToIndex(CubismMaskTile tile)
        {
            var tilesPerRow = 0;
            var tileCounts = 0;

            if (RenderTextureCount > 0)
            {
                var countPerSheetDiv = UsedMaskCount / RenderTextureCount;
                var countPerSheetMod = UsedMaskCount % RenderTextureCount;

                // Use RGBA in sequence.
                var div = countPerSheetDiv / Channels; // Number of masks to be placed in one channel.
                var mod = countPerSheetDiv % Channels; // Excess. Allocate one by one to this numbered channel.

                tileCounts = div + (tile.Channel < mod ? 1 : 0);

                var checkChannelNo = mod + 1 >= Channels ? 0 : mod + 1;
                if (tile.Channel == checkChannelNo)
                {
                    tileCounts += tile.RenderTextureIndex < countPerSheetMod ? 1 : 0;
                }

                if (tileCounts <= 1)
                {
                    tilesPerRow = 0;
                }
                else if (tileCounts <= 4)
                {
                    tilesPerRow = 2;
                }
                else if (tileCounts <= 9)
                {
                    tilesPerRow = 3;
                }

                return (int)((tile.Channel * tileCounts) + (tile.Column * tilesPerRow) + tile.Channel * (UseClippingMaskMaxCount / Channels));
            }
            else
            {
                tileCounts = (int)Mathf.Pow(4, Subdivisions - 1);
                tilesPerRow = (int)Mathf.Pow(2, Subdivisions - 1);
                return (int)((tile.Channel * tileCounts) + (tile.Column * tilesPerRow) + tile.Row);
            }
        }

        /// <summary>
        /// The structure that holds the information for each mask tile.
        /// </summary>
        private struct LayoutContext
        {
            /// <summary>
            /// Index of the <see cref="RenderTexture"/> to which this mask is assigned.
            /// </summary>
            public int RenderTextureIndex;

            /// <summary>
            /// Index of the <see cref="Channels"/> to which this mask is assigned.
            /// </summary>
            public int Channel;

            /// <summary>
            /// Number of channel divisions to which this mask is assigned.
            /// </summary>
            public int LayoutCount;

            /// <summary>
            /// Index within a division.
            /// </summary>
            public int LayoutCountContextIndex;
        }
    }
}
