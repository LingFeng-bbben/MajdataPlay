using System.Collections.Generic;

namespace UnityEngine.TestRunner.NUnitExtensions.Runner
{
    /// <summary>
    /// A registry for <see cref="ITestCommandWrapper"/> implementations that wrap test commands.
    /// </summary>
    public static class TestCommandWrapperRegistry
    {
        static readonly List<ITestCommandWrapper> s_Wrappers = new List<ITestCommandWrapper>();
        static readonly object s_Lock = new object();

        /// <summary>
        /// Registers a test command wrapper to be applied during test execution.
        /// </summary>
        /// <param name="wrapper">The wrapper to register. Null values are ignored.</param>
        public static void Register(ITestCommandWrapper wrapper)
        {
            if (wrapper == null)
                return;

            lock (s_Lock)
            {
                if (!s_Wrappers.Contains(wrapper))
                    s_Wrappers.Add(wrapper);
            }
        }

        internal static void Unregister(ITestCommandWrapper wrapper)
        {
            if (wrapper == null)
                return;

            lock (s_Lock)
            {
                s_Wrappers.Remove(wrapper);
            }
        }

        internal static void Clear()
        {
            lock (s_Lock)
            {
                s_Wrappers.Clear();
            }
        }

        internal static IEnumerable<ITestCommandWrapper> GetWrappers()
        {
            lock (s_Lock)
            {
                return new List<ITestCommandWrapper>(s_Wrappers);
            }
        }
    }
}
