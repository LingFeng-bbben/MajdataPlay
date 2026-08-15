using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// A DropDown editor control accepting <see cref="ISelectionDropDownContentProvider" />-based content providers.
    /// </summary>
    internal class SelectionDropDown : PopupWindowContent
    {
        private static readonly int k_ControlId = typeof(SelectionDropDown).GetHashCode();
        private readonly ISelectionDropDownContentProvider m_ContentProvider;
        private readonly Vector2 m_ContentSize;
        private Vector2 m_ScrollPosition = Vector2.zero;

        /// <summary>
        /// Creates a new instance of the <see cref="SelectionDropDown" /> editor control.
        /// </summary>
        /// <param name="contentProvider">The content provider to use.</param>
        public SelectionDropDown(ISelectionDropDownContentProvider contentProvider)
        {
            m_ContentProvider = contentProvider;
            var width = CalculateContentWidth();
            var height = CalculateContentHeight();
            m_ContentSize = new Vector2(width, height);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            editorWindow.wantsMouseMove = true;
            editorWindow.wantsMouseEnterLeaveWindow = true;
        }

        public override void OnClose()
        {
            GUIUtility.hotControl = 0;
            base.OnClose();
        }

        public override Vector2 GetWindowSize()
        {
            return m_ContentSize;
        }

        public override void OnGUI(Rect rect)
        {
            var evt = Event.current;
            var contentRect = new Rect(Styles.TopMargin, 0, 1, m_ContentSize.y);
            m_ScrollPosition = UnityEngine.GUI.BeginScrollView(rect, m_ScrollPosition, contentRect);
            {
                var yPos = Styles.TopMargin;
                for (var i = 0; i < m_ContentProvider.Count; ++i)
                {
                    var itemRect = new Rect(0, yPos, rect.width, Styles.LineHeight);
                    var separatorOffset = 0f;

                    switch (evt.type)
                    {
                        case EventType.Repaint:
                            var content = new GUIContent(m_ContentProvider.GetName(i));
                            var hover = itemRect.Contains(evt.mousePosition);
                            var on = m_ContentProvider.IsSelected(i);
                            Styles.MenuItem.Draw(itemRect, content, hover, false, on, false);
                            separatorOffset = DrawSeparator(i, itemRect);
                            break;

                        case EventType.MouseDown:
                            if (evt.button == 0 && itemRect.Contains(evt.mousePosition))
                            {
                                m_ContentProvider.SelectItem(i);
                                if (!m_ContentProvider.IsMultiSelection)
                                {
                                    editorWindow.Close();
                                }

                                evt.Use();
                            }

                            break;

                        case EventType.MouseEnterWindow:
                            GUIUtility.hotControl = k_ControlId;
                            evt.Use();
                            break;

                        case EventType.MouseLeaveWindow:
                            GUIUtility.hotControl = 0;
                            evt.Use();
                            break;

                        case EventType.MouseUp:
                        case EventType.MouseMove:
                            evt.Use();
                            break;
                    }

                    yPos += Styles.LineHeight + separatorOffset;
                }
            }
            UnityEngine.GUI.EndScrollView();
        }

        private float CalculateContentWidth()
        {
            var maxItemWidth = 0f;
            for (var i = 0; i < m_ContentProvider.Count; ++i)
            {
                var itemContent = new GUIContent(m_ContentProvider.GetName(i));
                var itemWidth = Styles.MenuItem.CalcSize(itemContent).x;
                maxItemWidth = Mathf.Max(itemWidth, maxItemWidth);
            }

            return maxItemWidth;
        }

        private float CalculateContentHeight()
        {
            return m_ContentProvider.Count * Styles.LineHeight
                + m_ContentProvider.SeparatorIndices.Length * Styles.SeparatorHeight
                + Styles.TopMargin + Styles.BottomMargin;
        }

        private float DrawSeparator(int i, Rect itemRect)
        {
            if (Array.IndexOf(m_ContentProvider.SeparatorIndices, i) < 0)
            {
                return 0f;
            }

            var separatorRect = GetSeparatorRect(itemRect);
            DrawRect(separatorRect, Styles.SeparatorColor);
            return Styles.SeparatorHeight;
        }

        private static Rect GetSeparatorRect(Rect itemRect)
        {
            var x = itemRect.x + Styles.SeparatorMargin;
            var y = itemRect.y + itemRect.height + Styles.SeparatorHeight * 0.15f;
            var width = itemRect.width - 2 * Styles.SeparatorMargin;
            const float height = 1f;

            return new Rect(x, y, width, height);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var originalColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color *= color;
            UnityEngine.GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            UnityEngine.GUI.color = originalColor;
        }

        private static class Styles
        {
            public const float LineHeight = EditorGUI.kSingleLineHeight;
            public const float TopMargin = 3f;
            public const float BottomMargin = 1f;
            public const float SeparatorHeight = 4f;
            public const float SeparatorMargin = 3f;
            public static readonly GUIStyle MenuItem = "MenuItem";
            public static readonly Color SeparatorColor = EditorGUIUtility.isProSkin
                ? new Color(0.32f, 0.32f, 0.32f, 1.333f)
                : new Color(0.6f, 0.6f, 0.6f, 1.333f);
        }
    }
}
