using System;
using UnityEditor.TestTools.TestRunner.GUI.Controls;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI
{
    internal static class TestRunnerGUI
    {
        private static Styles s_Styles;
        private static Styles Style => s_Styles ?? (s_Styles = new Styles());

        internal static void TestPlatformSelectionDropDown(ISelectionDropDownContentProvider contentProvider)
        {
            var text = Style.TestPlatformButtonString;
            for (int i = 0; i < contentProvider.Count; i++)
            {
                if (contentProvider.IsSelected(i))
                {
                    text += " " + contentProvider.GetName(i);
                    break;
                }
            }

            var content = new GUIContent(text);
            SelectionDropDown(contentProvider, content, GUILayout.Width(EditorStyles.toolbarDropDown.CalcSize(content).x));
        }

        internal static void CategorySelectionDropDown(ISelectionDropDownContentProvider contentProvider)
        {
            SelectionDropDown(contentProvider, Style.CategoryButtonContent, GUILayout.Width(Style.CategoryButtonWidth));
        }

        private static void SelectionDropDown(ISelectionDropDownContentProvider listContentProvider, GUIContent buttonContent,
            params GUILayoutOption[] options)
        {
            var rect = EditorGUILayout.GetControlRect(false, EditorGUI.kSingleLineHeight, Styles.DropdownButton, options);
            if (!EditorGUI.DropdownButton(rect, buttonContent, FocusType.Passive, Styles.DropdownButton))
            {
                return;
            }

            var selectionDropDown = new SelectionDropDown(listContentProvider);
            PopupWindow.Show(rect, selectionDropDown);
        }

        private class Styles
        {
            public static readonly GUIStyle DropdownButton = EditorStyles.toolbarDropDown;
            public readonly string TestPlatformButtonString = "Run Location:";
            public readonly GUIContent CategoryButtonContent = new GUIContent("Category");
            public readonly float CategoryButtonWidth;

            public Styles()
            {
                CategoryButtonWidth = DropdownButton.CalcSize(CategoryButtonContent).x;
            }
        }
    }
}
