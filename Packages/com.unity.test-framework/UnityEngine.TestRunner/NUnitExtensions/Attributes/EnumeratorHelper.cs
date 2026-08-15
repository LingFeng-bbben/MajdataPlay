using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.TestTools
{
    internal class EnumeratorHelper
    {
        public static bool IsRunningNestedEnumerator => enumeratorStack.Count > 0;

        // When an edit mode test yields across the play mode boundary (e.g., via EnterPlayMode()),
        // ProgressOnEnumerator is mid-flight and currentEnumerator/enumeratorStack must remain intact
        // so the coroutine can resume correctly. Clearing them here causes a NullReferenceException when
        // ProgressOnEnumerator resumes. These fields are always re-initialized fresh by
        // UnpackNestedEnumerators() at the start of each test, so no cleanup is needed here.
#pragma warning disable UDR0001
        private static IEnumerator currentEnumerator;
        private static Stack<IEnumerator> enumeratorStack = new Stack<IEnumerator>();
#pragma warning restore UDR0001

        /// <summary>
        /// This method executes a given enumerator and all nested enumerators.
        /// If any resuming (setting of pc) is needed, it needs to be done before being passed to this method.
        /// </summary>
        public static IEnumerator UnpackNestedEnumerators(IEnumerator testEnumerator)
        {
            if (testEnumerator == null)
            {
                throw new ArgumentNullException(nameof(testEnumerator));
            }

            currentEnumerator = testEnumerator;
            enumeratorStack.Clear();

            return ProgressOnEnumerator();
        }

        private static IEnumerator ProgressOnEnumerator()
        {
            while (true)
            {
                if (!currentEnumerator.MoveNext())
                {
                    if (enumeratorStack.Count == 0)
                    {
                        yield break;
                    }
                    currentEnumerator = enumeratorStack.Pop();
                    continue;
                }

                if (currentEnumerator.Current is IEnumerator nestedEnumerator)
                {
                    enumeratorStack.Push(currentEnumerator);
                    currentEnumerator = nestedEnumerator;
                }
                else
                {
                    yield return currentEnumerator.Current;
                }
            }
        }

        public static void SetEnumeratorPC(int pc)
        {
            if (currentEnumerator == null)
            {
                throw new Exception("No enumerator is currently running.");
            }

            if (IsRunningNestedEnumerator)
            {
                throw new Exception("Cannot set the enumerator PC while running nested enumerators.");
            }

            ActivePcHelper.SetEnumeratorPC(currentEnumerator, pc);
        }

        public static int GetEnumeratorPC()
        {
            if (currentEnumerator == null)
            {
                throw new Exception("No enumerator is currently running.");
            }

            if (IsRunningNestedEnumerator)
            {
                // Restrict the getting of PC, as it will not reflect what is currently running;
                throw new Exception("Cannot get the enumerator PC while running nested enumerators.");
            }

            return ActivePcHelper.GetEnumeratorPC(currentEnumerator);
        }
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticsOnLoad()
        {
            pcHelper = null;
        }
#endif

        private static TestCommandPcHelper pcHelper;
        internal static TestCommandPcHelper ActivePcHelper
        {
            get
            {
                if (pcHelper == null)
                {
                    pcHelper = new TestCommandPcHelper();
                }

                return pcHelper;
            }
            set
            {
                pcHelper = value;
            }
        }
    }
}
