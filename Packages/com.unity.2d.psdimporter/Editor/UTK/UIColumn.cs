using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.PSD
{
    internal interface IColumnUpdate
    {
        void Update();
    }

    internal interface IUIModuleColumn
    {
        Column[] MakeColumn(PSDImporterLayerManagementMultiColumnTreeView treeView, SerializedProperty module);
    }

    internal class UICellElement : VisualElement
    {
        PSDImporterLayerManagementMultiColumnTreeView m_MultiColumnTreeView;
        int m_Index;

        public UICellElement()
        { }

        public virtual void BindPSDNode(int index, PSDImporterLayerManagementMultiColumnTreeView treeView)
        {
            UnbindPSDNode();
            this.index = index;
            this.treeView = treeView;
        }

        public virtual void UnbindPSDNode()
        {
            index = -1;
        }

        public PSDTreeViewNode psdTreeViewNode => index < 0 ? null : treeView.GetFromIndex(index);

        protected int index
        {
            get => m_Index;
            set => m_Index = value;
        }

        protected PSDImporterLayerManagementMultiColumnTreeView treeView
        {
            get => m_MultiColumnTreeView;
            set => m_MultiColumnTreeView = value;
        }
    }

    internal class UIColumn : Column
    {
        PSDImporterLayerManagementMultiColumnTreeView m_TreeView;

        public UIColumn(PSDImporterLayerManagementMultiColumnTreeView treeView)
        {
            m_TreeView = treeView;
        }
        private UIColumn() { }
        protected PSDImporterLayerManagementMultiColumnTreeView treeView => m_TreeView;
    }


}

