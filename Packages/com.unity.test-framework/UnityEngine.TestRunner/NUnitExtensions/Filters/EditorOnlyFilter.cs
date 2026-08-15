using System;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestRunner.NUnitExtensions.Filters
{
    internal class EditorOnlyFilter : NonExplicitFilter
    {
        private const string k_Name = "EditorOnly";
        private bool m_EditorOnly;

        public EditorOnlyFilter(bool editorOnly)
        {
            m_EditorOnly = editorOnly;
        }

        public override bool Match(ITest test)
        {
            if (test.Properties.ContainsKey(k_Name))
            {
                var isEditorOnly = (bool)test.Properties.Get(k_Name);
                return isEditorOnly == m_EditorOnly;
            }

            if (test.Parent != null)
            {
                return Match(test.Parent);
            }

            return false;
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            return parentNode.AddElement(k_Name, m_EditorOnly.ToString());
        }

        public static void ApplyPropertyToTest(ITest test, bool isEditorOnly)
        {
            test.Properties.Set(k_Name, isEditorOnly);
        }
    }
}
