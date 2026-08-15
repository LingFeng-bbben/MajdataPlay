using System;
using System.Collections.Generic;
using PDNWrapper;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// Custom hidden asset to store meta information of the last import state
    /// </summary>
    internal class PSDImportData : ScriptableObject
    {
        [SerializeField]
        int m_ImportedTextureWidth;
        public int importedTextureWidth
        {
            get => m_ImportedTextureWidth;
            set => m_ImportedTextureWidth = value;
        }

        [SerializeField]
        int m_ImportedTextureHeight;
        public int importedTextureHeight
        {
            get => m_ImportedTextureHeight;
            set => m_ImportedTextureHeight = value;
        }

        [SerializeField]
        Vector2Int m_DocumentSize;
        public Vector2Int documentSize
        {
            get => m_DocumentSize;
            set => m_DocumentSize = value;
        }

        [SerializeField]
        int m_TextureActualHeight;
        public int textureActualHeight
        {
            get => m_TextureActualHeight;
            set => m_TextureActualHeight = value;
        }

        [SerializeField]
        int m_TextureActualWidth;
        public int textureActualWidth
        {
            get => m_TextureActualWidth;
            set => m_TextureActualWidth = value;
        }

        [SerializeField]
        TextureImporterSettings m_SingleSpriteTextureImporterSettings;
        public TextureImporterSettings singleSpriteTextureImporterSettings
        {
            get => m_SingleSpriteTextureImporterSettings;
            set => m_SingleSpriteTextureImporterSettings = value;
        }

        [SerializeField]
        PSDLayerData[] m_PsdLayerData;
        public PSDLayerData[] psdLayerData => m_PsdLayerData;

        public void CreatePSDLayerData(List<BitmapLayer> bitmapLayer)
        {
            List<PSDLayerData> layerData = new List<PSDLayerData>();
            foreach (BitmapLayer fileLayer in bitmapLayer)
            {
                CreatePSDLayerData(fileLayer, layerData);
            }
            m_PsdLayerData = layerData.ToArray();
        }

        void CreatePSDLayerData(BitmapLayer layer, List<PSDLayerData> layerData, int parentIndex = -1)
        {
            layerData.Add(new PSDLayerData()
            {
                isGroup = layer.IsGroup,
                isVisible = layer.Visible,
                layerID = layer.LayerID,
                name = layer.Name,
                parentIndex = parentIndex,
                layerSizeOnFile = new Vector2Int(layer.documentRect.Width, layer.documentRect.Height)
            });
            parentIndex = layerData.Count - 1;
            foreach (BitmapLayer fileLayer in layer.ChildLayer)
            {
                CreatePSDLayerData(fileLayer, layerData, parentIndex);
            }
        }
    }

    // Struct to keep track of GOs and bone
    internal struct BoneGO
    {
        public GameObject go;
        public int index;
    }

    /// <summary>
    /// Capture per layer import settings
    /// </summary>
    [Serializable]
    class PSDLayerImportSetting : IPSDLayerMappingStrategyComparable
    {
        [SerializeField]
        string m_SpriteId;
        GUID m_SpriteIDGUID;

        public string name;
        public int layerId;
        public bool flatten;
        public bool isGroup;
        public bool importLayer;

        public int layerID => layerId;
        string IPSDLayerMappingStrategyComparable.name => name;
        bool IPSDLayerMappingStrategyComparable.isGroup => isGroup;

        public GUID spriteId
        {
            get
            {
                if (string.IsNullOrEmpty(m_SpriteId))
                {
                    m_SpriteIDGUID = GUID.Generate();
                    m_SpriteId = m_SpriteIDGUID.ToString();
                }

                return m_SpriteIDGUID;

            }
            set
            {
                m_SpriteIDGUID = value;
                m_SpriteId = m_SpriteIDGUID.ToString();
            }
        }
    }

    /// <summary>
    /// PSDLayer data for PSDImportData for last import state
    /// </summary>
    [Serializable]
    class PSDLayerData : IPSDLayerMappingStrategyComparable
    {
        [SerializeField]
        string m_Name;
        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField]
        int m_ParentIndex;
        public int parentIndex
        {
            get => m_ParentIndex;
            set => m_ParentIndex = value;
        }

        [SerializeField]
        int m_LayerID;
        public int layerID
        {
            get => m_LayerID;
            set => m_LayerID = value;
        }

        [SerializeField]
        bool m_IsVisible;
        public bool isVisible
        {
            get => m_IsVisible;
            set => m_IsVisible = value;
        }

        [SerializeField]
        bool m_IsGroup;
        public bool isGroup
        {
            get => m_IsGroup;
            set => m_IsGroup = value;
        }

        [SerializeField]
        bool m_IsImported;
        public bool isImported
        {
            get => m_IsImported;
            set => m_IsImported = value;
        }

        [SerializeField]
        Vector2Int m_LayerSizeOnFile;
        public Vector2Int layerSizeOnFile
        {
            get => m_LayerSizeOnFile;
            set => m_LayerSizeOnFile = value;
        }

        public bool IsEmpty => !(layerSizeOnFile.x > 0 && layerSizeOnFile.y > 0);
    }

    /// <summary>
    /// Data for extracting layers and colors from PSD
    /// </summary>
    class PSDExtractLayerData
    {
        public BitmapLayer bitmapLayer;
        public PSDLayerImportSetting importSetting;
        public PSDExtractLayerData[] children;
    }
}
