using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.TestTools.TestRunner.GUI.Controls
{
    internal class MultiValueContentProvider<T> : ISelectionDropDownContentProvider where T : IEquatable<T>
    {
        private T[] m_Values;
        private bool[] m_Selected;
        private Action<T[]> m_SelectionChangeCallback;

        public MultiValueContentProvider(T[] values, T[] selectedValues, Action<T[]> selectionChangeCallback)
        {
            m_Values = values ?? throw new ArgumentNullException(nameof(values));
            if (selectedValues == null)
            {
                m_Selected = new bool[values.Length];
            }
            else
            {
                m_Selected = values.Select(value => selectedValues.Contains(value)).ToArray();
            }
            m_SelectionChangeCallback = selectionChangeCallback;
        }

        public int Count
        {
            get { return m_Values.Length; }
        }

        public bool IsMultiSelection
        {
            get { return true; }
        }
        public int[] SeparatorIndices
        {
            get { return new int[0]; }
        }
        public string GetName(int index)
        {
            if (!ValidateIndexBounds(index))
            {
                return string.Empty;
            }

            return m_Values[index].ToString();
        }

        public void SelectItem(int index)
        {
            if (!ValidateIndexBounds(index))
            {
                return;
            }

            m_Selected[index] = !m_Selected[index];
            m_SelectionChangeCallback.Invoke(m_Values.Where((v, i) => m_Selected[i]).ToArray());
        }

        public bool IsSelected(int index)
        {
            if (!ValidateIndexBounds(index))
            {
                return false;
            }

            return m_Selected[index];
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
