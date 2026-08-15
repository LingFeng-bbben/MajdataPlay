using System;
using Unity.Collections;
using UnityEngine;

namespace UnityEditor.U2D.PSD
{
    [Serializable]
    class PSDLayer : IPSDLayerMappingStrategyComparable
    {
        [SerializeField]
        string m_Name;
        [SerializeField]
        string m_SpriteName;
        [SerializeField]
        bool m_IsGroup;
        [SerializeField]
        int m_ParentIndex;
        [SerializeField]
        string m_SpriteID;
        [SerializeField]
        int m_LayerID;
        [SerializeField]
        Vector2Int m_MosaicPosition;
        [SerializeField]
        bool m_Flatten;
        [SerializeField]
        bool m_IsImported;
        [SerializeField]
        bool m_IsVisible;

        [NonSerialized]
        Vector2 m_LayerPosition;
        [NonSerialized]
        GameObject m_GameObject;

        public PSDLayer(NativeArray<Color32> tex, int parent, bool group, string layerName, int width, int height, int id, bool hidden)
        {
            isGroup = group;
            parentIndex = parent;
            texture = tex;
            name = layerName;
            this.width = width;
            this.height = height;
            layerID = id;
            m_Flatten = false;
            m_IsImported = false;
            m_IsVisible = hidden;
            m_SpriteID = new GUID().ToString();
        }

        public PSDLayer(PSDLayer layer)
        {
            m_Name = layer.m_Name;
            m_SpriteName = layer.m_SpriteName;
            m_IsGroup = layer.m_IsGroup;
            m_ParentIndex = layer.m_ParentIndex;
            m_SpriteID = layer.m_SpriteID;
            m_LayerID = layer.m_LayerID;
            m_MosaicPosition = layer.m_MosaicPosition;
            m_Flatten = layer.m_Flatten;
            m_IsImported = layer.m_IsImported;
            m_IsVisible = layer.m_IsVisible;
            m_LayerPosition = layer.m_LayerPosition;
            m_GameObject = layer.m_GameObject;
            width = layer.width;
            height = layer.height;
            texture = layer.texture;
        }

        public bool isVisible => m_IsVisible;
        public int layerID { get { return m_LayerID; } private set { m_LayerID = value; } }

        public string name { get { return m_Name; } private set { m_Name = value; } }
        public string spriteName { get { return m_SpriteName; } set { m_SpriteName = value; } }
        public bool isGroup { get { return m_IsGroup; } private set { m_IsGroup = value; } }
        public int parentIndex { get { return m_ParentIndex; } private set { m_ParentIndex = value; } }
        public Vector2Int mosaicPosition { get { return m_MosaicPosition; } set { m_MosaicPosition = value; } }
        public GUID spriteID { get { return new GUID(m_SpriteID); } set { m_SpriteID = value.ToString(); } }
        public Vector2 layerPosition { get => m_LayerPosition; set => m_LayerPosition = value; }
        public GameObject gameObject { get { return m_GameObject; } set { m_GameObject = value; } }

        public bool flatten
        {
            get => m_Flatten;
            set => m_Flatten = value;
        }

        public bool isImported
        {
            get => m_IsImported;
            set => m_IsImported = value;
        }

        public NativeArray<Color32> texture { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public void Dispose()
        {
            if (texture.IsCreated)
                texture.Dispose();
        }
    }
}
