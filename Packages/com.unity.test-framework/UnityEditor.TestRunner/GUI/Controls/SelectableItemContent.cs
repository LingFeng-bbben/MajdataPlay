using System;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// A default implementation of the <see cref="ISelectableItem{T}" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of the value represented by this content element.</typeparam>
    internal class SelectableItemContent<T> : ISelectableItem<T>
    {
        private readonly string m_DisplayName;

        /// <summary>
        /// Creates a new instance of the <see cref="SelectableItemContent{T}" /> class
        /// </summary>
        /// <param name="itemValue">The value represented by this item.</param>
        /// <param name="displayName">The display name of this item.</param>
        public SelectableItemContent(T itemValue, string displayName)
        {
            Value = itemValue;
            m_DisplayName = displayName;
        }

        public T Value { get; }

        public string DisplayName => m_DisplayName ?? string.Empty;
    }
}
