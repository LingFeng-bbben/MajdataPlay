using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;
#if UNITY_6000_5_OR_NEWER
using UnityEngine.Assemblies;
#endif

namespace UnityEditor.TestTools.TestRunner
{
    internal abstract class AttributeFinderBase : IAttributeFinder
    {
        public abstract IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform);
    }

    internal interface IAttributeFinder
    {
        IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform);
    }

    internal abstract class AttributeFinderBase<T1, T2> : AttributeFinderBase where T2 : Attribute
    {
        private readonly Func<T2, Type> m_TypeSelector;
        protected AttributeFinderBase(Func<T2, Type> typeSelector)
        {
            m_TypeSelector = typeSelector;
        }

        public override IEnumerable<Type> Search(ITest tests, ITestFilter filter, RuntimePlatform testTargetPlatform)
        {
            var selectedTests = new List<ITest>();
            TestFiltering.GetMatchingTests(tests, filter, ref selectedTests, testTargetPlatform);

            var result = new List<Type>();
            result.AddRange(GetTypesFromPrebuildAttributes(selectedTests));
            result.AddRange(GetTypesFromInterface(selectedTests, testTargetPlatform));

            return result.Distinct();
        }

        private IEnumerable<Type> GetTypesFromPrebuildAttributes(IEnumerable<ITest> tests)
        {
#if UNITY_6000_5_OR_NEWER
            var allAssemblies = CurrentAssemblies.GetLoadedAssemblies();
#else
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            allAssemblies = allAssemblies.Where(x => x.GetReferencedAssemblies().Any(z => z.Name == "UnityEditor.TestRunner")).ToArray();
            var attributesFromAssemblies = allAssemblies.SelectMany(assembly => assembly.GetCustomAttributes(typeof(T2), true).OfType<T2>());
            var attributesFromMethods = tests.SelectMany(t => t.Method.GetCustomAttributes<T2>(true).Select(attribute => attribute));
            var attributesFromTypes = tests.SelectMany(t => t.Method.TypeInfo.GetCustomAttributes<T2>(true).Select(attribute => attribute));

            var result = new List<T2>();
            result.AddRange(attributesFromAssemblies);
            result.AddRange(attributesFromMethods);
            result.AddRange(attributesFromTypes);

            return result.Select(m_TypeSelector).Where(type => type != null);
        }

        private static IEnumerable<Type> GetTypesFromInterface(IEnumerable<ITest> selectedTests, RuntimePlatform testTargetPlatform)
        {
            var typesWithInterfaces = selectedTests.Where(t => typeof(T1).IsAssignableFrom(t.Method.TypeInfo.Type) && TestFiltering.IsTestEnabledOnPlatform(t, testTargetPlatform));
            return typesWithInterfaces.Select(t => t.Method.TypeInfo.Type);
        }
    }
}
