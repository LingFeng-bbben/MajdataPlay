using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using UnityEngine.TestRunner.NUnitExtensions.Filters;
using UnityEngine.TestTools;

namespace UnityEngine.TestRunner.NUnitExtensions
{
    internal static class TestExtensions
    {
        private static List<string> ExtractFixtureCategories(ITest test)
        {
            var fixtureCategories = test.Properties[PropertyNames.Category].Cast<string>().ToList();
            if (test.Parent != null)
            {
                fixtureCategories.AddRange(ExtractFixtureCategories(test.Parent));
            }

            return fixtureCategories;
        }

        public static List<string> GetAllCategoriesFromTest(this ITest test)
        {
            // Only mark tests as Uncategorized if the test fixture doesn't have a category,
            // otherwise the test inherits the Fixture category.
            // Recursively try checking until Parent is null - cause category can be set on higher level.
            var categories = ExtractFixtureCategories(test);
            if (categories.Count == 0 && test is TestMethod)
            {
                categories.Add(CategoryFilterExtended.k_DefaultCategory);
            }

            return categories;
        }

        public static void ParseForNameDuplicates(this ITest test)
        {
            var duplicates = new Dictionary<string, int>();
            for (var i = 0; i < test.Tests.Count; i++)
            {
                var child = test.Tests[i];
                int count;
                if (duplicates.TryGetValue(child.FullName, out count))
                {
                    count++;
                    child.Properties.Add("childIndex", count);
                    duplicates[child.FullName] = count;
                }
                else
                {
                    duplicates.Add(child.FullName, 1);
                }
                ParseForNameDuplicates(child);
            }
        }

        public static void ApplyPlatformToPropertyBag(this ITest test, TestPlatform testPlatform)
        {
            test.Properties.Set("platform", testPlatform);
            foreach (var child in test.Tests)
            {
                child.ApplyPlatformToPropertyBag(testPlatform);
            }
        }

        public static int GetChildIndex(this ITest test)
        {
            var index = test.Properties["childIndex"];
            return (int)index[0];
        }

        public static bool HasChildIndex(this ITest test)
        {
            var index = test.Properties["childIndex"];
            return index.Count > 0;
        }

        private static string GetAncestorPath(ITest test)
        {
            var path = "";
            var testParent = test.Parent;

            while (testParent != null && testParent.Parent != null && !string.IsNullOrEmpty(testParent.Name))
            {
                var segment = testParent.Name;
                if (testParent.HasChildIndex())
                {
                    var index = testParent.GetChildIndex();
                    if (index >= 0)
                        segment += index;
                }
                path = segment + "/" + path;
                testParent = testParent.Parent;
            }

            return path;
        }

        public static string GetUniqueName(this ITest test)
        {
            var id = GetAncestorPath(test) + GetFullName(test);
            if (test.HasChildIndex())
            {
                var index = test.GetChildIndex();
                if (index >= 0)
                    id += index;
            }
            if (test.IsSuite)
            {
                id += "[suite]";
            }
            return id;
        }

        public static int GetRetryIteration(this ITest test)
        {
            if (test.Properties.ContainsKey("retryIteration"))
            {
                return test.Properties["retryIteration"].OfType<int>().First();
            }

            return 0;
        }
        
        public static int GetRepeatIteration(this ITest test)
        {
            if (test.Properties.ContainsKey("repeatIteration"))
            {
                return test.Properties["repeatIteration"].OfType<int>().First();
            }

            return 0;
        }

        public static string GetFullName(this ITest test)
        {
            var typeInfo = test.TypeInfo ?? test.Parent?.TypeInfo ?? test.Tests.FirstOrDefault()?.TypeInfo;
            if (typeInfo == null)
            {
                return "[" + test.Name + "]";
            }

            var assemblyId = typeInfo.Assembly.GetName().Name;
            if (assemblyId == test.Name)
            {
                return $"[{test.Name}]";
            }

            return string.Format("[{0}][{1}]", assemblyId, test.FullName);
        }

        public static string GetFullNameWithoutDllPath(this ITest test)
        {
            if (test.Parent == null)
            {
                return string.Empty;
            }
            var typeInfo = test.TypeInfo ?? test.Parent?.TypeInfo;
            if (typeInfo == null && IsAssembly(test))
            {
                return test.Name;
            }

            return test.FullName;
        }

        private static bool IsAssembly(this ITest test)
        {
            return test.Parent.Parent == null;
        }

        public static string GetSkipReason(this ITest test)
        {
            if (test.Properties.ContainsKey(PropertyNames.SkipReason))
                return (string)test.Properties.Get(PropertyNames.SkipReason);

            return null;
        }

        public static string GetParentId(this ITest test)
        {
            if (test.Parent != null)
                return test.Parent.Id;

            return null;
        }

        public static string GetParentFullName(this ITest test)
        {
            if (test.Parent != null)
                return test.Parent.FullName;

            return null;
        }

        public static string GetParentUniqueName(this ITest test)
        {
            if (test.Parent != null)
                return GetUniqueName(test.Parent);

            return null;
        }

        internal static string GetFullName(string testFullName, int childIndex)
        {
            return childIndex != -1 ? GetIndexedTestCaseName(testFullName, childIndex) : testFullName;
        }
        private static string GetIndexedTestCaseName(string fullName, int index)
        {
            var generatedTestSuffix = " GeneratedTestCase" + index;
            if (fullName.EndsWith(")"))
            {
                // Test names from generated TestCaseSource look like Test(TestCaseSourceType)
                // This inserts a unique test case index in the name, so that it becomes Test(TestCaseSourceType GeneratedTestCase0)
                return fullName.Substring(0, fullName.Length - 1) + generatedTestSuffix + fullName[fullName.Length - 1];
            }

            // In some cases there can be tests with duplicate names generated in other ways and they won't have () in their name
            // We just append a suffix at the end of the name in that case
            return fullName + generatedTestSuffix;
        }
    }
}
