using System;
using System.Linq;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace UnityEngine.TestRunner.NUnitExtensions.Filters
{
    internal class AndFilterExtended : AndFilter
    {
        public AndFilterExtended(params ITestFilter[] filters) : base(filters) {}

        public override bool IsExplicitMatch(ITest test)
        {
            var explicitFilters = Filters.Where(filter => !(filter is NonExplicitFilter)).ToArray();
            if (explicitFilters.Length == 0)
            {
                return false;
            }

            foreach (TestFilter filter in explicitFilters)
            {
                if (!filter.IsExplicitMatch(test))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
