using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using UnityEngine.TestRunner.NUnitExtensions;
using UnityEngine.TestRunner.NUnitExtensions.Runner;
using UnityEngine.TestTools.Logging;
using UnityEngine.TestTools.TestRunner;

namespace UnityEngine.TestTools
{
    internal abstract class BeforeAfterTestCommandBase<T> : DelegatingTestCommand, IEnumerableTestMethodCommand where T : class
    {
        [Flags]
        public enum ExecutionFlags
        {
            None = 0,
            SkipAfterActions = 1 << 0,
            SkipStateReset = 1 << 1,
            SkipBeforeActions = 1 << 2
        }

        private string m_BeforeErrorPrefix;
        private string m_AfterErrorPrefix;
        protected BeforeAfterTestCommandBase(TestCommand innerCommand, string beforeErrorPrefix, string afterErrorPrefix)
            : base(innerCommand)
        {
            m_BeforeErrorPrefix = beforeErrorPrefix;
            m_AfterErrorPrefix = afterErrorPrefix;
        }

        protected T[] BeforeActions = new T[0];

        protected T[] AfterActions = new T[0];

        protected static MethodInfo[] GetActions(IDictionary<Type, List<MethodInfo>> cacheStorage, Type fixtureType, Type attributeType, Type[] returnTypes)
        {
            if (cacheStorage.TryGetValue(fixtureType, out var result))
            {
                return result.ToArray();
            }

            cacheStorage[fixtureType] = GetMethodsWithAttributeFromFixture(fixtureType,  attributeType, returnTypes);

            return cacheStorage[fixtureType].ToArray();
        }

        protected static T[] GetTestActions(IDictionary<MethodInfo,  List<T>> cacheStorage, ITest test)
        {
            var methodInfo = test.Method.MethodInfo;
            if (cacheStorage.TryGetValue(methodInfo, out var result))
            {
                return result.ToArray();
            }

            var attributesForMethodInfo = new List<T>();
            var parent = test.Parent;
            while (parent != null)
            {
                if (parent.TypeInfo != null)
                {
                    attributesForMethodInfo.AddRange(parent.TypeInfo.GetCustomAttributes<T>(true));
                }

                parent = parent.Parent;
            }

            var attributes = methodInfo.GetCustomAttributes(true);
            foreach (var attribute in attributes)
            {
                if (attribute is T attribute1)
                {
                    attributesForMethodInfo.Add(attribute1);
                }
            }

            cacheStorage[methodInfo] = attributesForMethodInfo;

            return cacheStorage[methodInfo].ToArray();
        }

        private static List<MethodInfo> GetMethodsWithAttributeFromFixture(Type fixtureType, Type setUpType, Type[] returnTypes)
        {
            MethodInfo[] methodsWithAttribute = Reflect.GetMethodsWithAttribute(fixtureType, setUpType, true);
            var methodsInfo = new List<MethodInfo>();
            methodsInfo.AddRange(methodsWithAttribute.Where(method => returnTypes.Any(type => type == method.ReturnType)));
            return methodsInfo;
        }

        protected abstract IEnumerator InvokeBefore(T action, Test test, UnityTestExecutionContext context);

        protected abstract IEnumerator InvokeAfter(T action, Test test, UnityTestExecutionContext context);

        protected virtual bool MoveBeforeEnumerator(IEnumerator enumerator, Test test)
        {
            return enumerator.MoveNext();
        }

        protected virtual bool MoveAfterEnumerator(IEnumerator enumerator, Test test)
        {
            return enumerator.MoveNext();
        }

        protected abstract BeforeAfterTestCommandState GetState(UnityTestExecutionContext context);

        protected virtual bool AllowFrameSkipAfterAction(T action)
        {
            return true;
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context)
        {
            return ExecuteEnumerable(context, ExecutionFlags.None);
        }

        public IEnumerable ExecuteEnumerable(ITestExecutionContext context, ExecutionFlags flags)
        {
            var unityContext = (UnityTestExecutionContext)context;
            var state = GetState(unityContext);
            if (state == null)
            {
                throw new Exception($"No state in context for {GetType().Name}.");
            }

            if (state.ShouldRestore)
            {
                state.ApplyContext(unityContext);
            }

            while ((flags & ExecutionFlags.SkipBeforeActions) == 0 && state.NextBeforeStepIndex < BeforeActions.Length)
            {
                var action = BeforeActions[state.NextBeforeStepIndex];
                IEnumerator enumerator;
                try
                {
                    enumerator = EnumeratorHelper.UnpackNestedEnumerators(InvokeBefore(action, Test, unityContext));
                    EnumeratorHelper.SetEnumeratorPC(state.NextBeforeStepPc);
                }
                catch (Exception ex)
                {
                    state.TestHasRun = true;
                    context.CurrentResult.RecordPrefixedException(m_BeforeErrorPrefix, ex);
                    break;
                }

                using (var logScope = new LogScope())
                {
                    while (true)
                    {
                        try
                        {
                            if (!enumerator.MoveNext())
                            {
                                logScope.EvaluateLogScope(true);
                                break;
                            }

                            if (!AllowFrameSkipAfterAction(action)) // Evaluate the log scope right away for the commands where we do not yield
                            {
                                logScope.EvaluateLogScope(true);
                            }
                            if (enumerator.Current is IEditModeTestYieldInstruction)
                            {
                                if (unityContext.TestMode == TestPlatform.PlayMode)
                                {
                                    throw new Exception($"PlayMode test are not allowed to yield {enumerator.Current.GetType().Name}");
                                }

                                if (EnumeratorHelper.IsRunningNestedEnumerator)
                                {
                                    throw new Exception($"Nested enumerators are not allowed to yield {enumerator.Current.GetType().Name}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            state.TestHasRun = true;
                            context.CurrentResult.RecordPrefixedException(m_BeforeErrorPrefix, ex);
                            state.StoreContext(unityContext);
                            break;
                        }

                        if (!EnumeratorHelper.IsRunningNestedEnumerator)
                        {
                            // Only store the state in the main enumerator. Domain reloads are not supported from nested enumerators.
                            state.NextBeforeStepPc = EnumeratorHelper.GetEnumeratorPC();
                            state.StoreContext(unityContext);
                        }

                        if (!AllowFrameSkipAfterAction(action))
                        {
                            break;
                        }

                        yield return enumerator.Current;
                    }
                }

                state.NextBeforeStepIndex++;
                state.NextBeforeStepPc = 0;
            }

            if (!state.TestHasRun)
            {
                state.ShouldRestore = false; // Any inner commands that can perform domain reloads are responsible for restoring the context
                if (innerCommand is IEnumerableTestMethodCommand)
                {
                    var executeEnumerable = ((IEnumerableTestMethodCommand)innerCommand).ExecuteEnumerable(context);
                    foreach (var iterator in executeEnumerable)
                    {
                        yield return iterator;
                    }
                }
                else
                {
                    context.CurrentResult = innerCommand.Execute(context);
                }

                state.TestHasRun = true;
            }

            while (!flags.HasFlag(ExecutionFlags.SkipAfterActions) && state.NextAfterStepIndex < AfterActions.Length)
            {
                state.TestAfterStarted = true;
                var action = AfterActions[state.NextAfterStepIndex];
                IEnumerator enumerator;
                try
                {
                    enumerator = EnumeratorHelper.UnpackNestedEnumerators(InvokeAfter(action, Test, unityContext));
                    EnumeratorHelper.SetEnumeratorPC(state.NextAfterStepPc);
                }
                catch (Exception ex)
                {
                    context.CurrentResult.RecordPrefixedException(m_AfterErrorPrefix, ex);
                    state.StoreContext(unityContext);
                    break;
                }

                using (var logScope = new LogScope())
                {
                    while (true)
                    {
                        try
                        {
                            if (!enumerator.MoveNext())
                            {
                                logScope.EvaluateLogScope(true);
                                break;
                            }

                            if (!AllowFrameSkipAfterAction(action)) // Evaluate the log scope right away for the commands where we do not yield
                            {
                                logScope.EvaluateLogScope(true);
                            }
                            if (enumerator.Current is IEditModeTestYieldInstruction)
                            {
                                if (unityContext.TestMode == TestPlatform.PlayMode)
                                {
                                    throw new Exception($"PlayMode test are not allowed to yield {enumerator.Current.GetType().Name}");
                                }

                                if (EnumeratorHelper.IsRunningNestedEnumerator)
                                {
                                    throw new Exception($"Nested enumerators are not allowed to yield {enumerator.Current.GetType().Name}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            context.CurrentResult.RecordPrefixedException(m_AfterErrorPrefix, ex);
                            state.StoreContext(unityContext);
                            break;
                        }

                        if (!EnumeratorHelper.IsRunningNestedEnumerator)
                        {
                            // Only store the state in the main enumerator. Domain reloads are not supported from nested enumerators.
                            state.NextAfterStepPc = EnumeratorHelper.GetEnumeratorPC();
                            state.StoreContext(unityContext);
                        }

                        if (!AllowFrameSkipAfterAction(action))
                        {
                            break;
                        }

                        yield return enumerator.Current;
                    }
                }

                state.NextAfterStepIndex++;
                state.NextAfterStepPc = 0;
            }

            if (!flags.HasFlag(ExecutionFlags.SkipStateReset))
                state.Reset();
        }

        public override TestResult Execute(ITestExecutionContext context)
        {
            throw new NotImplementedException("Use ExecuteEnumerable");
        }
    }
}
