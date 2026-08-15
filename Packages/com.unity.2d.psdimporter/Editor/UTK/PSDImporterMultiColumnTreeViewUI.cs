using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.PSD
{
    internal class LayerManagementTreeViewData
    {
        public string assetName;
        public PSDImportData importData;
        public SerializedProperty layerImportSettings;
        public SerializedProperty mosaicLayers;
        public SerializedProperty importHiddenLayers;
        public SerializedProperty importFileNodeState;
        public IPSDLayerMappingStrategy mappingStrategy;

        public LayerManagementTreeViewData(SerializedObject so)
        {
            Update(so);
        }

        public void Update(SerializedObject so)
        {
            mosaicLayers = so.FindProperty("m_MosaicLayers");
            importHiddenLayers = so.FindProperty("m_ImportHiddenLayers");
            importFileNodeState = so.FindProperty("m_ImportFileNodeState");
            assetName = Path.GetFileNameWithoutExtension(((ScriptedImporter)so.targetObject).assetPath);
            importData = ((PSDImporter)so.targetObject).importData;
            mappingStrategy = ((PSDImporter)so.targetObject).GetLayerMappingStrategy();
            layerImportSettings = so.FindProperty("m_PSDLayerImportSetting");
        }

    }

    internal class PSDImporterLayerManagementMultiColumnTreeView : MultiColumnTreeView
    {
        int m_LastArraySize;
        LayerManagementTreeViewData m_LayerManagementTreeViewData;
        PSDTreeViewNode[] m_Data;
        UILayerImportColumn m_LayerImportColumn;

        public void UpdateTreeView(SerializedObject so)
        {
            m_LayerManagementTreeViewData.Update(so);
            SetupColumns();
            RebuildTree();
        }

        void SetupColumns()
        {
            columns.Clear();
            m_LayerImportColumn = new UILayerImportColumn(this)
            {
                name = "UILayerImportColumn",
            };
            columns.Add(m_LayerImportColumn);

            Column col;
            col = new UILayerNameColumn(this)
            {
                name = "UILayerNameColumn",
            };
            columns.Add(col);

            columns.primaryColumnName = "UILayerNameColumn";
        }

        public PSDImporterLayerManagementMultiColumnTreeView(SerializedObject so)
        {
            viewDataKey = "PSDImporterLayerManagementMultiColumnTreeView-ViewDataKey";
            m_LayerManagementTreeViewData = new LayerManagementTreeViewData(so);
            showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            showBorder = true;
            UpdateTreeView(so);
        }

        public PSDTreeViewNode[] data => m_Data;

        public bool importHidden => m_LayerManagementTreeViewData.importHiddenLayers.boolValue;

        SerializedProperty layerImportSetting => m_LayerManagementTreeViewData.layerImportSettings;
        IList<PSDLayerData> importLayerData => m_LayerManagementTreeViewData.importData.psdLayerData;
        IPSDLayerMappingStrategy layerMappingStrategy => m_LayerManagementTreeViewData.mappingStrategy;

        void RebuildTree()
        {
            SetRootItems(BuildTree());
            Rebuild();
        }

        public void Update()
        {
            foreach (Column c in columns)
            {
                if (c is IColumnUpdate)
                {
                    ((IColumnUpdate)c).Update();
                }
            }
        }

        List<TreeViewItemData<int>> BuildTree()
        {
            List<TreeViewItemData<int>> treeViewData = new List<TreeViewItemData<int>>();
            layerImportSetting.serializedObject.Update();
            m_LastArraySize = layerImportSetting.arraySize;
            PSDFileTreeViewNode fileRoot = new PSDFileTreeViewNode(m_LayerManagementTreeViewData)
            {
                id = 0,
                displayName = m_LayerManagementTreeViewData.assetName
            };

            List<PSDLayerImportSettingSerializedPropertyWrapper> spWrapper = new List<PSDLayerImportSettingSerializedPropertyWrapper>();
            if (layerImportSetting.arraySize > 0)
            {
                SerializedProperty firstElement = layerImportSetting.GetArrayElementAtIndex(0);
                for (int i = 0; i < layerImportSetting.arraySize; ++i)
                {
                    spWrapper.Add(new PSDLayerImportSettingSerializedPropertyWrapper(firstElement, layerImportSetting, null, i));
                    firstElement.Next(false);
                }
            }
            if (importLayerData != null)
            {
                PSDTreeViewNode[] nodes = new PSDTreeViewNode[importLayerData.Count + 1];
                nodes[0] = fileRoot;
                for (int i = 1; i <= importLayerData.Count; ++i)
                {
                    PSDLayerData l = importLayerData[i - 1];
                    int importSettingIndex = spWrapper.FindIndex(x => layerMappingStrategy.Compare(x, l));
                    PSDLayerImportSettingSerializedPropertyWrapper importSetting = null;
                    if (importSettingIndex < 0)
                    {
                        importSetting = new PSDLayerImportSettingSerializedPropertyWrapper(null, layerImportSetting, l, layerImportSetting.arraySize)
                        {
                            wasLayerImported = (l.isVisible || m_LayerManagementTreeViewData.importHiddenLayers.boolValue) && !l.IsEmpty
                        };
                    }
                    else
                    {
                        importSetting = spWrapper[importSettingIndex];
                        spWrapper.RemoveAt(importSettingIndex);
                    }

                    if (l != null && l.isGroup)
                        nodes[i] = new PSDGroupTreeViewNode(l, i, importSetting);
                    else
                        nodes[i] = new PSDLayerTreeViewNode(l, i, importSetting);
                    PSDTreeViewNode node = nodes[i];

                    node.disable = !node.layer.isVisible || node.layer.IsEmpty;
                    while (node.layer.parentIndex != -1 && nodes[i].disable == false)
                    {
                        PSDTreeViewNode parentNode = nodes[node.layer.parentIndex + 1];
                        if (!node.layer.isVisible || node.layer.IsEmpty || !parentNode.layer.isVisible || parentNode.layer.IsEmpty)
                        {
                            nodes[i].disable = true;
                        }

                        node = nodes[node.layer.parentIndex + 1];
                    }
                }
                foreach (PSDTreeViewNode node in nodes)
                {
                    PSDTreeViewNode rootTreeViewNode = null;
                    if (node.layer == null)
                        continue;
                    if (node.layer.parentIndex == -1)
                    {
                        rootTreeViewNode = fileRoot;
                    }
                    else
                    {
                        rootTreeViewNode = nodes[node.layer.parentIndex + 1];
                    }
                    rootTreeViewNode.AddChild(node);
                }
                m_Data = nodes;
            }
            else
            {
                m_Data = new[] { fileRoot };
            }
            treeViewData.Add(fileRoot.BuildTreeViewItemData());
            return treeViewData;
        }

        public PSDTreeViewNode GetFromIndex(int i)
        {
            int e = GetItemDataForIndex<int>(i);
            return m_Data[e];
        }
    }
}
