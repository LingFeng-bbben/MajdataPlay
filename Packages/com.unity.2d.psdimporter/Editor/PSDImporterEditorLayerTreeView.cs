using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.U2D.PSD
{
    internal class PSDLayerImportSettingSerializedPropertyWrapper : IPSDLayerMappingStrategyComparable
    {
        PSDLayerData m_Layer;
        SerializedProperty m_Array;
        SerializedProperty m_Element;
        SerializedProperty m_NameProperty;
        SerializedProperty m_LayerIdProperty;
        SerializedProperty m_FlattenProperty;
        SerializedProperty m_IsGroupProperty;
        SerializedProperty m_ImportLayerProperty;
        int m_ArrayIndex;
        bool m_WasLayerImported;

        public string name
        {
            get => m_NameProperty.stringValue;
            set
            {
                CheckAndAddElement();
                m_NameProperty.stringValue = value;
            }
        }

        public bool isGroup
        {
            get => m_IsGroupProperty.boolValue;
            set
            {
                CheckAndAddElement();
                m_IsGroupProperty.boolValue = value;
            }
        }

        public int layerID
        {
            get => m_LayerIdProperty.intValue;
            set
            {
                CheckAndAddElement();
                m_LayerIdProperty.intValue = value;
            }
        }

        public bool flatten
        {
            get
            {
                CheckIfIndexChanged();
                return m_FlattenProperty == null ? false : m_FlattenProperty.boolValue;
            }
            set
            {
                CheckAndAddElement();
                if (m_FlattenProperty.boolValue != value)
                {
                    m_FlattenProperty.boolValue = value;
                    m_FlattenProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public bool wasLayerImported
        {
            get => m_WasLayerImported;
            set => m_WasLayerImported = value;
        }

        public bool importLayer
        {
            get
            {
                CheckIfIndexChanged();
                return m_ImportLayerProperty == null ? wasLayerImported : m_ImportLayerProperty.boolValue;
            }
            set
            {
                CheckAndAddElement();
                if (m_ImportLayerProperty.boolValue != value)
                {
                    m_ImportLayerProperty.boolValue = value;
                    m_ImportLayerProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        void CheckIfIndexChanged()
        {
            if (m_ArrayIndex >= m_Array.arraySize)
            {
                m_ArrayIndex = m_Array.arraySize - 1;
                m_NameProperty = null;
                m_LayerIdProperty = null;
                m_FlattenProperty = null;
                m_IsGroupProperty = null;
                m_ImportLayerProperty = null;
                m_Element = null;
            }

        }
        void CheckAndAddElement()
        {
            if (m_Element == null)
            {
                int arraySize = m_Array.arraySize;
                m_ArrayIndex = arraySize;
                m_Array.arraySize = arraySize + 1;
                m_Element = m_Array.GetArrayElementAtIndex(arraySize);
                CacheProperty(m_Element);
                flatten = false;
                name = m_Layer.name;
                layerID = m_Layer.layerID;
                isGroup = m_Layer.isGroup;
                importLayer = wasLayerImported;
            }
        }

        void CacheProperty(SerializedProperty property)
        {
            m_NameProperty = property.FindPropertyRelative("name");
            m_LayerIdProperty = property.FindPropertyRelative("layerId");
            m_FlattenProperty = property.FindPropertyRelative("flatten");
            m_IsGroupProperty = property.FindPropertyRelative("isGroup");
            m_ImportLayerProperty = property.FindPropertyRelative("importLayer");
        }

        public PSDLayerImportSettingSerializedPropertyWrapper(SerializedProperty sp, SerializedProperty array, PSDLayerData layer, int index)
        {
            if (sp != null)
            {
                m_Element = sp;
                CacheProperty(sp);
            }

            m_Layer = layer;
            m_Array = array;
            m_ArrayIndex = index;
        }
    }

    class PSDTreeViewNode
    {
        public int id;
        public string displayName;
        public List<PSDTreeViewNode> children = new List<PSDTreeViewNode>();
        public PSDTreeViewNode parent;
        PSDLayerData m_Layer;
        bool m_Disable = false;
        public PSDLayerData layer => m_Layer;

        PSDLayerImportSettingSerializedPropertyWrapper m_Property;

        public bool disable
        {
            get => m_Disable;
            set => m_Disable = value;
        }

        public PSDTreeViewNode()
        {
            id = 1;
            displayName = "";
        }

        public PSDTreeViewNode(PSDLayerData layer, int id, PSDLayerImportSettingSerializedPropertyWrapper importSetting)
        {
            m_Layer = layer;
            displayName = layer.name;
            this.id = id;
            m_Property = importSetting;
        }

        protected PSDLayerImportSettingSerializedPropertyWrapper property => m_Property;

        public virtual bool importLayer
        {
            get => property.importLayer;
            set
            {
                if (property.importLayer != value)
                {
                    property.importLayer = value;
                }
            }
        }

        public void AddChild(PSDTreeViewNode child)
        {
            if (child == null) return;
            children.Add(child);
            child.parent = this;
        }

        public TreeViewItemData<int> BuildTreeViewItemData()
        {
            List<TreeViewItemData<int>> c = new List<TreeViewItemData<int>>();
            if (children != null)
            {
                c = children.Select(n => n.BuildTreeViewItemData()).ToList();
            }
            return new TreeViewItemData<int>(id, id, c);
        }
    }

    class PSDFoldoutTreeViewNode : PSDTreeViewNode
    {
        public virtual bool flatten
        {
            get => property.flatten;
            set
            {
                if (property.flatten != value)
                    property.flatten = value;
            }
        }

        public PSDFoldoutTreeViewNode()
            : base()
        { }

        public PSDFoldoutTreeViewNode(PSDLayerData layer, int id, PSDLayerImportSettingSerializedPropertyWrapper property)
            : base(layer, id, property)
        { }

    }

    class PSDFileTreeViewNode : PSDFoldoutTreeViewNode
    {
        LayerManagementTreeViewData m_PsdFileSerializedProperty;

        public PSDFileTreeViewNode(LayerManagementTreeViewData sp)
        {
            m_PsdFileSerializedProperty = sp;
        }
        public override bool flatten
        {
            get => !m_PsdFileSerializedProperty.mosaicLayers.boolValue;
            set
            {
                if (m_PsdFileSerializedProperty.mosaicLayers.boolValue == value)
                {
                    m_PsdFileSerializedProperty.mosaicLayers.boolValue = !value;
                    m_PsdFileSerializedProperty.mosaicLayers.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override bool importLayer
        {
            get => m_PsdFileSerializedProperty.importFileNodeState.boolValue;
            set
            {
                if (m_PsdFileSerializedProperty.importFileNodeState.boolValue != value)
                {
                    m_PsdFileSerializedProperty.importFileNodeState.boolValue = value;
                    m_PsdFileSerializedProperty.importFileNodeState.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }

    class PSDLayerTreeViewNode : PSDTreeViewNode
    {
        public PSDLayerTreeViewNode(PSDLayerData layer, int id, PSDLayerImportSettingSerializedPropertyWrapper property) : base(layer, id, property)
        { }
    }

    class PSDGroupTreeViewNode : PSDFoldoutTreeViewNode
    {
        public PSDGroupTreeViewNode(PSDLayerData layer, int id, PSDLayerImportSettingSerializedPropertyWrapper property)
            : base(layer, id, property)
        { }
    }
}

