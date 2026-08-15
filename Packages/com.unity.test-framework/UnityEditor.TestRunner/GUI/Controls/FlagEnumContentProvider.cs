using System;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    /// <summary>
    /// A flag enum content provider to be used with the <see cref="SelectionDropDown" /> control.
    /// </summary>
    /// <typeparam name="T">The flag enum type.</typeparam>
    internal class FlagEnumContentProvider<T> : ISelectionDropDownContentProvider where T : Enum
    {
        private readonly Action<T> m_ValueChangedCallback;
        private readonly T[] m_Values;
        internal Func<string, string> DisplayNameGenerator = ObjectNames.NicifyVariableName;
        private T m_CurrentValue;

        /// <summary>
        /// Creates a new instance of the <see cref="FlagEnumContentProvider{T}" /> class.
        /// </summary>
        /// <param name="initialValue">The initial selection value.</param>
        /// <param name="valueChangedCallback">The callback to be invoked on selection change.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the generic enum parameter type is not integer based
        /// or if the initial selection value is empty.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if the provided change callback is null.</exception>
        public FlagEnumContentProvider(T initialValue, Action<T> valueChangedCallback)
        {
            if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            {
                throw new ArgumentException("Argument underlying type must be integer.");
            }

            if ((int)(object)initialValue == 0)
            {
                throw new ArgumentException("The initial value must not be an empty set.", nameof(initialValue));
            }

            if (valueChangedCallback == null)
            {
                throw new ArgumentNullException(nameof(valueChangedCallback), "The value change callback must not be null.");
            }

            m_CurrentValue = initialValue;
            m_Values = (T[])Enum.GetValues(typeof(T));
            m_ValueChangedCallback = valueChangedCallback;
        }

        public int Count => m_Values.Length;
        public bool IsMultiSelection => true;

        public string GetName(int index)
        {
            return ValidateIndexBounds(index) ? DisplayNameGenerator(m_Values[index].ToString()) : string.Empty;
        }

        public int[] SeparatorIndices => new int[0];

        public bool IsSelected(int index)
        {
            return ValidateIndexBounds(index) && IsSet(m_Values[index]);
        }

        public void SelectItem(int index)
        {
            if (!ValidateIndexBounds(index))
            {
                return;
            }

            if (ChangeValue(m_Values[index]))
            {
                m_ValueChangedCallback(m_CurrentValue);
            }
        }

        private bool ChangeValue(T flag)
        {
            var value = flag;
            var count = GetSetCount();

            if (IsSet(value))
            {
                if (count == 1)
                {
                    return false;
                }

                m_CurrentValue = FlagEnumUtility.RemoveFlag(m_CurrentValue, flag);
                return true;
            }

            m_CurrentValue = FlagEnumUtility.SetFlag(m_CurrentValue, flag);
            return true;
        }

        private bool IsSet(T flag)
        {
            return FlagEnumUtility.HasFlag(m_CurrentValue, flag);
        }

        private int GetSetCount()
        {
            return BitUtility.GetCardinality((int)(object)m_CurrentValue);
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
