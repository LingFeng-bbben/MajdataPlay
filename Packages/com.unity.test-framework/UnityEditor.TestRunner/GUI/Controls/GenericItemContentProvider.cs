using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// A generic type content provider to be used with the <see cref="SelectionDropDown" /> control.
    /// </summary>
    /// <typeparam name="T">The type of values represented by content elements.</typeparam>
    internal class GenericItemContentProvider<T> : ISelectionDropDownContentProvider where T : IEquatable<T>
    {
        private readonly ISelectableItem<T>[] m_Items;
        private readonly Action<T> m_ValueChangedCallback;
        private T m_CurrentValue;

        /// <summary>
        /// Creates a new instance of the <see cref="GenericItemContentProvider{T}" /> class.
        /// </summary>
        /// <param name="initialValue">The initial selection value.</param>
        /// <param name="items">The set of selectable items.</param>
        /// <param name="separatorIndices">The indices of items which should be followed by separator lines.</param>
        /// <param name="valueChangedCallback"></param>
        /// <exception cref="ArgumentNullException">Thrown if any of the provided arguments is null, except for the separator indices.</exception>
        /// <exception cref="ArgumentException">Thrown if the set of items is empty or does not contain the initial selection value.</exception>
        public GenericItemContentProvider(T initialValue, ISelectableItem<T>[] items, int[] separatorIndices, Action<T> valueChangedCallback)
        {
            if (initialValue == null)
            {
                throw new ArgumentNullException(nameof(initialValue), "The initial selection value must not be null.");
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "The set of items must not be null.");
            }

            if (valueChangedCallback == null)
            {
                throw new ArgumentNullException(nameof(valueChangedCallback), "The value change callback must not be null.");
            }

            if (items.Length == 0)
            {
                throw new ArgumentException("The set of items must not be empty.", nameof(items));
            }

            if (!items.Any(i => i.Value.Equals(initialValue)))
            {
                throw new ArgumentException("The initial selection value must be in the items set.", nameof(items));
            }

            m_CurrentValue = initialValue;
            m_Items = items;
            SeparatorIndices = separatorIndices ?? new int[0];
            m_ValueChangedCallback = valueChangedCallback;
        }

        public int Count => m_Items.Length;
        public bool IsMultiSelection => false;

        public string GetName(int index)
        {
            return ValidateIndexBounds(index) ? m_Items[index].DisplayName : string.Empty;
        }

        public int[] SeparatorIndices { get; }

        public void SelectItem(int index)
        {
            if (!ValidateIndexBounds(index))
            {
                return;
            }

            if (IsSelected(index))
            {
                return;
            }

            m_CurrentValue = m_Items[index].Value;
            m_ValueChangedCallback(m_CurrentValue);
        }

        public bool IsSelected(int index)
        {
            return ValidateIndexBounds(index) && m_Items[index].Value.Equals(m_CurrentValue);
        }

        private bool ValidateIndexBounds(int index)
        {
            if (index < 0 || index >= Count)
            {
                Debug.LogError($"Requesting item index {index} from a collection of size {Count}");
                return false;
            }

            return true;
        }
    }
}
