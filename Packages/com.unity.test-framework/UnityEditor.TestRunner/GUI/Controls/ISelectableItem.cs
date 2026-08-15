using System;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// Defines a content element which can be used with the <see cref="GenericItemContentProvider{T}" /> content provider.
    /// </summary>
    internal interface ISelectableItem<out T>
    {
        /// <summary>
        /// The value represented by this item.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// The name to be used when displaying this item.
        /// </summary>
        string DisplayName { get; }
    }
}
