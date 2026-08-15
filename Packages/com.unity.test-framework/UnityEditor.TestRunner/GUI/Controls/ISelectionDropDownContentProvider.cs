using System;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// Defines a content provider that can be used with the <see cref="SelectionDropDown" /> control.
    /// </summary>
    internal interface ISelectionDropDownContentProvider
    {
        /// <summary>
        /// The total number of items to display.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Multiple selection support.
        /// Multiple selection dropdowns don't get closed on selection change.
        /// </summary>
        bool IsMultiSelection { get; }

        /// <summary>
        /// The indices of items which should be followed by separator lines.
        /// </summary>
        int[] SeparatorIndices { get; }

        /// <summary>
        /// Returns the display name of the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item whose display name is to be returned.</param>
        /// <returns>The display name of the item at the specified index.</returns>
        string GetName(int index);

        /// <summary>
        /// Signals a request to select the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to be selected.</param>
        void SelectItem(int index);

        /// <summary>
        /// Returns the selection status of the item at the specified index.
        /// </summary>
        /// <param name="index">The index of the item whose selection status is to be returned.</param>
        /// <returns><c>true</c> if the item is currently selected; otherwise, <c>false</c>. </returns>
        bool IsSelected(int index);
    }
}
