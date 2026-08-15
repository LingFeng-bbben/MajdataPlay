using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools.Logging;

namespace UnityEditor.TestTools.TestRunner.TestRun.Tasks
{
    internal abstract class BuildActionTaskBase<T> : TestTaskBase
    {
        private string typeName;
        internal IAttributeFinder attributeFinder;
        internal Action<string> logAction = Debug.Log;
        internal Func<ILogScope> logScopeProvider = () => new LogScope();
        internal Func<Type, object> createInstance = Activator.CreateInstance;

        protected BuildActionTaskBase(IAttributeFinder attributeFinder)
        {
            this.attributeFinder = attributeFinder;
            typeName = typeof(T).Name;
        }

        protected abstract void Action(T target, TestJobData testJobData);

        public override IEnumerator Execute(TestJobData testJobData)
        {
            if (testJobData.testTree == null)
            {
                throw new Exception($"Test tree is not available for {GetType().Name}.");
            }

            var enumerator = ExecuteMethods(testJobData);

            while (enumerator.MoveNext())
            {
                yield return null;
            }
        }

        private IEnumerator ExecuteMethods(TestJobData testJobData)
        {
            var exceptions = new List<Exception>();

            foreach (var targetClassType in attributeFinder.Search(testJobData.testTree, testJobData.testFilter, testJobData.TargetRuntimePlatform))
            {
                try
                {
                    logAction($"Executing {typeName} for: {targetClassType.FullName}.");
                    var targetClass = (T)createInstance(targetClassType);

                    using (var logScope = logScopeProvider())
                    {
                        Action(targetClass, testJobData);
                        logScope.EvaluateLogScope(true);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException($"One or more exceptions when executing {typeName}.", exceptions);
            }
            yield break;
        }
    }
}
