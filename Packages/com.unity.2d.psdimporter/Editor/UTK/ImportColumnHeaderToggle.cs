using System;
using UnityEngine;
using UnityEngine.U2D.Common;
using UnityEngine.UIElements;

namespace UnityEditor.U2D.PSD
{
    internal class ImportColumnHeaderToggle : VisualElement
    {
        internal static readonly string ussClassName = "unity-dropdown-toggle";
        internal static readonly string dropdownClassName = ussClassName + "__dropdown";
        readonly Toggle m_Checkmark;
        readonly Button m_DropdownButton;
        Action<bool> m_ImportToggleChangeCallback;
        public ImportColumnHeaderToggle(Action dropdownClickEvent, Action<bool> importToggleChange)
        {
            AddToClassList(ussClassName);

            focusable = false;
            VisualElement checkbackBackground = new VisualElement()
            {
                name = "ImportColumnHeaderToggleCheckmarkBackground"
            };
            m_Checkmark = new Toggle()
            {
                name = "ImportColumnHeaderToggleCheckmark",
                tooltip = Tooltips.importToggleToolTip
            };
            m_ImportToggleChangeCallback = importToggleChange;
            m_Checkmark.RegisterValueChangedCallback(OnHeaderImportToggleChange);
            checkbackBackground.Add(m_Checkmark);
            m_DropdownButton = new Button(dropdownClickEvent)
            {
                name = "ImportColumnHeaderDropdown"
            };
            m_DropdownButton.AddToClassList(dropdownClassName);

            VisualElement arrow = new VisualElement();
            arrow.AddToClassList("unity-icon-arrow");
            arrow.pickingMode = PickingMode.Ignore;
            m_DropdownButton.Add(arrow);

            Add(checkbackBackground);
            Add(m_DropdownButton);
        }

        void OnHeaderImportToggleChange(ChangeEvent<bool> b)
        {
            m_ImportToggleChangeCallback?.Invoke(b.newValue);
        }

        public void SetHeaderImportToggleValue(bool b)
        {
            if (m_Checkmark.value != b)
                m_Checkmark.SetValueWithoutNotify(b);
        }
    }

}
