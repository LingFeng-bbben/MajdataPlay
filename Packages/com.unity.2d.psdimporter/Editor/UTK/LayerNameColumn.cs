using UnityEditor.U2D.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.PSD
{
    internal class UICellLabelElement : UICellElement
    {
        static readonly Texture2D s_FolderIcon = EditorGUIUtility.FindTexture("Folder Icon") as Texture2D;

        Label m_Label;
        VisualElement m_FolderIcon;
        bool m_ShowFolderIcon;

        public UICellLabelElement()
        {
            m_FolderIcon = new VisualElement()
            {
                name = "UICellFolderElement"
            };
            m_Label = new Label()
            {
                name = "UICellLabelElement"
            };
            this.Add(m_FolderIcon);
            this.Add(m_Label);
        }

        public string text
        {
            set { m_Label.text = value; }
        }

        public bool showFolderIcon
        {
            get => m_ShowFolderIcon;
            set
            {
                m_ShowFolderIcon = value;
                if (m_ShowFolderIcon)
                {
                    m_FolderIcon.SetHiddenFromLayout(false);
                    m_FolderIcon.style.backgroundImage = new StyleBackground(s_FolderIcon);
                }
                else
                {
                    m_FolderIcon.SetHiddenFromLayout(true);
                }
            }
        }
    }

    internal class UILayerNameColumn : UIColumn
    {
        PSDImporterLayerManagementMultiColumnTreeView m_TreeView;
        public UILayerNameColumn(PSDImporterLayerManagementMultiColumnTreeView treeView) : base(treeView)
        {
            makeCell = () => new UICellLabelElement();
            bindCell = BindCell;
            sortable = false;
            stretchable = true;
            title = "Layers";
        }

        public virtual void BindCell(VisualElement e, int index)
        {
            PSDTreeViewNode item = treeView.GetFromIndex(index);
            UICellLabelElement label = (UICellLabelElement)e;
            label.text = item.displayName;
            label.showFolderIcon = item is PSDGroupTreeViewNode;
            label.SetEnabled(!item.disable);
            if (item.disable)
                label.tooltip = item.layer.IsEmpty ? Tooltips.layerEmptyToolTip : Tooltips.layerHiddenToolTip;
            else
                label.tooltip = "";
        }
    }

}

