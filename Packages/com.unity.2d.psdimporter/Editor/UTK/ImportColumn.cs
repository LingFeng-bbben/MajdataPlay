using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Common;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.PSD
{
    /// <summary>
    /// Import Cell
    /// </summary>
    internal class UICellImportElement : UICellElement
    {
        VisualElement m_WarningIcon;
        Toggle m_ImportToggle;
        Toggle m_CollapseToggle;
        VisualElement m_CollapseToggleCheckMark;

        public UICellImportElement()
        {
            m_WarningIcon = new VisualElement()
            {
                name = "WarningIcon",
                tooltip = Tooltips.hiddenLayerNotImportWarning
            };
            m_ImportToggle = new Toggle()
            {
                name = "UICellImportToggle"
            };
            m_CollapseToggle = new Toggle()
            {
                name = "UICellCollapseToggle"
            };
            m_CollapseToggle.tooltip = Tooltips.groupSeparatedToolTip;
            m_CollapseToggleCheckMark = m_CollapseToggle.Q("unity-checkmark");
            m_ImportToggle.RegisterValueChangedCallback(ToggleImportChange);
            m_CollapseToggle.RegisterValueChangedCallback(ToggleCollapseChange);
            m_CollapseToggle.RegisterCallback<MouseEnterEvent>(CollapseToggleMouseEnter);
            m_CollapseToggle.RegisterCallback<MouseOutEvent>(CollapseToggleMouseOut);
            Add(m_ImportToggle);
            Add(m_WarningIcon);
            Add(m_CollapseToggle);
        }

        void CollapseToggleMouseOut(MouseOutEvent evt)
        {
            if (m_CollapseToggle.enabledSelf)
            {
                if (m_CollapseToggle.showMixedValue && m_CollapseToggle.value == false)
                {
                    m_CollapseToggleCheckMark.visible = true;
                }
            }
        }

        void CollapseToggleMouseEnter(MouseEnterEvent evt)
        {
            if (m_CollapseToggle.enabledSelf)
            {
                if (m_CollapseToggle.showMixedValue && m_CollapseToggle.value == false)
                {
                    m_CollapseToggleCheckMark.visible = false;
                }
            }
        }

        void ShowHideCollapseToggleCheckMark()
        {
            if (m_CollapseToggle.showMixedValue && m_CollapseToggle.value == false)
            {
                if (!m_CollapseToggle.hasHoverPseudoState)
                    m_CollapseToggleCheckMark.visible = true;
            }
            else
            {
                m_CollapseToggleCheckMark.visible = false;
            }

        }

        string TooltipForNode(PSDTreeViewNode node)
        {
            return node.layer?.IsEmpty == true ? Tooltips.emptyLayerNotImportWarning : Tooltips.hiddenLayerNotImportWarning;
        }

        public override void BindPSDNode(int index, PSDImporterLayerManagementMultiColumnTreeView treeView)
        {
            base.BindPSDNode(index, treeView);
            PSDTreeViewNode node = psdTreeViewNode;
            m_WarningIcon.visible = node.disable;
            if (m_WarningIcon.visible)
                m_WarningIcon.tooltip = TooltipForNode(node);
            if (node is PSDFoldoutTreeViewNode)
            {
                PSDFoldoutTreeViewNode collapsable = (PSDFoldoutTreeViewNode)node;
                SetToggleActive(m_CollapseToggle, true);
                m_CollapseToggle.tooltip = collapsable.flatten ? Tooltips.groupMergedToolTip : Tooltips.groupSeparatedToolTip;
            }
            else
            {
                SetToggleActive(m_CollapseToggle, false);
            }

            if (node is PSDFileTreeViewNode)
            {
                SetToggleActive(m_ImportToggle, false);
            }
            else
            {
                SetToggleActive(m_ImportToggle, true);
            }
            Update();
        }

        void SetToggleActive(Toggle t, bool b)
        {
            t.visible = b;
            t.SetEnabled(b);
        }

        public void Update()
        {
            PSDTreeViewNode node = psdTreeViewNode;
            if (node != null)
            {
                m_WarningIcon.visible = node.disable && (treeView.importHidden == false || node.layer?.IsEmpty == true);
                if (m_WarningIcon.visible)
                    m_WarningIcon.tooltip = TooltipForNode(node);
                if (m_ImportToggle.value != node.importLayer)
                    m_ImportToggle.SetValueWithoutNotify(node.importLayer);
                if (node is PSDFoldoutTreeViewNode)
                {
                    PSDFoldoutTreeViewNode group = (PSDFoldoutTreeViewNode)node;
                    if (m_CollapseToggle.value != group.flatten)
                    {
                        m_CollapseToggle.SetValueWithoutNotify(group.flatten);
                        m_CollapseToggle.showMixedValue = false;
                    }
                    // check if any child nodes are collapse
                    bool showMix = ChildrenIsCollapsed(node);
                    if (group.flatten == false && m_CollapseToggle.showMixedValue != showMix)
                    {
                        m_CollapseToggle.showMixedValue = showMix;
                        if (showMix)
                            m_CollapseToggle.tooltip = Tooltips.groupMixedToolTip;
                        ShowHideCollapseToggleCheckMark();
                    }
                }
            }
        }

        bool ChildrenIsCollapsed(PSDTreeViewNode treeViewNode)
        {
            for (int i = 0; i < treeViewNode?.children?.Count; ++i)
            {
                PSDTreeViewNode c = treeViewNode.children[i] as PSDTreeViewNode;
                if (c != null && c is PSDFoldoutTreeViewNode)
                {
                    PSDFoldoutTreeViewNode b = ((PSDFoldoutTreeViewNode)c);
                    if (b.flatten || ChildrenIsCollapsed(c))
                        return true;
                }
            }

            return false;
        }

        void ToggleImportChange(ChangeEvent<bool> e)
        {
            bool value = e.newValue;
            psdTreeViewNode.importLayer = value;
            SetChildrenNodeImport(psdTreeViewNode, value);
            PSDFoldoutTreeViewNode parent = psdTreeViewNode.parent as PSDFoldoutTreeViewNode;
            if (value)
            {
                while (parent != null)
                {
                    parent.importLayer = true;
                    parent = parent.parent as PSDFoldoutTreeViewNode;
                }
            }
            else
            {
                // if parent's children are all off, we turn off parent
                while (parent != null)
                {
                    bool import = false;
                    foreach (PSDTreeViewNode c in parent.children)
                    {
                        if (c.importLayer)
                        {
                            import = true;
                            break;
                        }
                    }
                    parent.importLayer = import;
                    parent = parent.parent as PSDFoldoutTreeViewNode;
                }
            }
        }

        internal static void SetChildrenNodeImport(PSDTreeViewNode treeViewNode, bool value)
        {
            treeViewNode.importLayer = value;
            if (treeViewNode.children != null)
            {
                foreach (PSDTreeViewNode p in treeViewNode.children)
                {
                    p.importLayer = value;
                    SetChildrenNodeImport(p, value);
                }
            }
        }

        void ToggleCollapseChange(ChangeEvent<bool> e)
        {
            if (psdTreeViewNode != null)
            {
                bool newValue = e.newValue;
                if (psdTreeViewNode is PSDFoldoutTreeViewNode)
                {
                    PSDFoldoutTreeViewNode group = (PSDFoldoutTreeViewNode)psdTreeViewNode;
                    group.flatten = newValue;
                    if (newValue)
                        m_CollapseToggle.showMixedValue = false;
                    m_CollapseToggle.tooltip = newValue ? Tooltips.groupMergedToolTip : Tooltips.groupSeparatedToolTip;
                }
            }
        }
    }

    internal interface ILayerImportColumnField
    {
        VisualElement MakeHeader();
        VisualElement MakeCell();
        void UnBindCell(VisualElement e, PSDTreeViewNode node);
        void BindCell(VisualElement e, PSDTreeViewNode node, SerializedProperty module);
        void UpdateCell(VisualElement e, PSDTreeViewNode node);
    }

    /// <summary>
    /// Import Column
    /// </summary>
    internal class UILayerImportColumn : UIColumn, IColumnUpdate
    {
        ImportColumnHeaderToggle m_ImportHeaderToggle;
        IList<SerializedProperty> m_ImportColumnFieldsSP;
        ILayerImportColumnField[] m_ImportColumnFields;
        Action ImportSelectionMenu = null;

        public UILayerImportColumn(PSDImporterLayerManagementMultiColumnTreeView treeView) : base(treeView)
        {
            makeCell = () =>
            {
                UICellImportElement cell = new UICellImportElement()
                {
                    name = "UICellImportElement"
                };
                if (m_ImportColumnFields != null)
                {
                    foreach (ILayerImportColumnField module in m_ImportColumnFields)
                    {
                        VisualElement v = module.MakeCell();
                        if (v != null)
                            cell.Add(v);
                    }
                }

                return cell;
            };
            bindCell = BindCell;
            destroyCell = DestroyCell;
            unbindCell = UnBindCell;
            makeHeader = MakeHeader;
            title = "Import Settings";
            optional = false;
            stretchable = false;
            sortable = false;
            resizable = false;
            width = 70;
            minWidth = 70;
            maxWidth = 70;
        }

        public virtual void DestroyCell(VisualElement e)
        { }

        public virtual void UnBindCell(VisualElement e, int index)
        {
            UICellImportElement toggle = (UICellImportElement)e;
            toggle.UnbindPSDNode();
            if (m_ImportColumnFields != null)
            {
                foreach (ILayerImportColumnField module in m_ImportColumnFields)
                {
                    module.UnBindCell(e, treeView.GetFromIndex(index));
                }
            }
        }

        public virtual void BindCell(VisualElement e, int index)
        {
            UICellImportElement toggle = (UICellImportElement)e;
            toggle.BindPSDNode(index, treeView);
            if (m_ImportColumnFields != null)
            {
                for (int i = 0; i < m_ImportColumnFields.Length; ++i)
                {
                    m_ImportColumnFields[i].BindCell(e, treeView.GetFromIndex(index), m_ImportColumnFieldsSP[i]);
                }
            }
        }

        void IColumnUpdate.Update()
        {
            if (ImportSelectionMenu != null)
            {
                ImportSelectionMenu.Invoke();
                ImportSelectionMenu = null;
            }

            m_ImportHeaderToggle.SetHeaderImportToggleValue(treeView.data[0].importLayer);
            for (int i = 0; i < treeView.itemsSource.Count; ++i)
            {
                UICellImportElement v = treeView.GetRootElementForIndex(i)?.Q<UICellImportElement>();
                v?.Update();
                if (m_ImportColumnFields != null)
                {
                    PSDTreeViewNode node = treeView.GetFromIndex(i);
                    foreach (ILayerImportColumnField module in m_ImportColumnFields)
                    {
                        module.UpdateCell(v, node);
                    }
                }
            }
        }

        VisualElement MakeHeader()
        {
            VisualElement ve = new VisualElement()
            {
                name = "ImportHeaderToggle"
            };
            m_ImportHeaderToggle = new ImportColumnHeaderToggle(ShowContextMenu, HeaderImportToggleChange);

            ve.Add(m_ImportHeaderToggle);

            VisualElement collapseHeader = new VisualElement()
            {
                name = "CollapseHeader"
            };
            VisualElement collapseHeaderIcon = new VisualElement()
            {
                name = "CollapseHeaderIcon",
                tooltip = Tooltips.collapseToggleTooltip
            };
            collapseHeader.Add(collapseHeaderIcon);

            ve.Add(collapseHeader);

            if (m_ImportColumnFields != null)
            {
                foreach (ILayerImportColumnField i in m_ImportColumnFields)
                {
                    VisualElement v = i.MakeHeader();
                    if (v != null)
                        ve.Add(v);
                }
            }

            return ve;
        }

        void ShowContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Select All Visible Layers"), false, () => ImportSelectionMenu = SelectAllVisibleLayers);
            menu.AddItem(new GUIContent("Deselect All Visible Layers"), false, () => ImportSelectionMenu = DeselectAllVisibleLayers);
            menu.AddItem(new GUIContent("Select All Hidden Layers"), false, () => ImportSelectionMenu = SelectAllHiddenLayers);
            menu.AddItem(new GUIContent("Deselect All Hidden Layers"), false, () => ImportSelectionMenu = DeselectAllHiddenLayers);
            menu.DropDown(m_ImportHeaderToggle.worldBound);
        }

        void HeaderImportToggleChange(bool b)
        {
            // Should always be the first one
            treeView.data[0].importLayer = b;
            UICellImportElement.SetChildrenNodeImport(treeView.data[0], b);
        }

        internal void SelectAllVisibleLayers()
        {
            foreach (PSDTreeViewNode d in treeView.data)
            {
                if (!d.disable)
                    d.importLayer = true;
            }
        }

        internal void DeselectAllVisibleLayers()
        {
            foreach (PSDTreeViewNode d in treeView.data)
            {
                if (!d.disable)
                    d.importLayer = false;
            }
        }

        internal void SelectAllHiddenLayers()
        {
            foreach (PSDTreeViewNode d in treeView.data)
            {
                if (d.disable)
                    d.importLayer = true;
            }
        }

        internal void DeselectAllHiddenLayers()
        {
            foreach (PSDTreeViewNode d in treeView.data)
            {
                if (d.disable)
                    d.importLayer = false;
            }
        }
    }
}
