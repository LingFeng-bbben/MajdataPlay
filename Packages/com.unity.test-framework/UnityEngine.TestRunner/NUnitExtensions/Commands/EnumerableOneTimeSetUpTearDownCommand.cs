using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using Unity.Profiling;
using UnityEngine.TestRunner.NUnitExtensions.Runner;

namespace UnityEngine.TestTools
{
    internal class EnumerableOneTimeSetUpTearDownCommand : BeforeAfterTestCommandBase<MethodInfo>
    {
        private static readonly Dictionary<Type, List<MethodInfo>> m_BeforeActionsCache = new Dictionary<Type, List<MethodInfo>>();
        private static readonly Dictionary<Type, List<MethodInfo>> m_AfterActionsCache = new Dictionary<Type, List<MethodInfo>>();

        public class DoNothing : TestCommand
        {
            public DoNothing(Test test)
                : base(test)
            {
            }

            public override TestResult Execute(ITestExecutionContext context)
            {
                return context.CurrentResult;
            }
        }

        private readonly TestSuite _suite;

        private readonly ITypeInfo _typeInfo;
        private readonly object[] _arguments;

        public EnumerableOneTimeSetUpTearDownCommand(Test suite)
            : base(new DoNothing(suite), "OneTimeSetUp", "OneTimeTearDown")
        {
            _suite = (TestSuite)suite;
            _typeInfo = _suite.TypeInfo;
            _arguments = _suite.Arguments;
            using (new ProfilerMarker(nameof(EnumerableOneTimeSetUpTearDownCommand)).Auto())
            {
                if (Test.TypeInfo != null && Test.TypeInfo.Type != null)
                {
                    BeforeActions = GetActions(m_BeforeActionsCache, Test.TypeInfo.Type, typeof(UnityOneTimeSetUpAttribute), new[] { typeof(IEnumerator) });
                    AfterActions = Enumerable.Reverse(GetActions(m_AfterActionsCache, Test.TypeInfo.Type, typeof(UnityOneTimeTearDownAttribute), new[] { typeof(IEnumerator) })).ToArray();
                }
            }
        }

        protected override bool MoveAfterEnumerator(IEnumerator enumerator, Test test)
        {
            using (new ProfilerMarker(test.Name + ".OneTimeTearDown").Auto())
            {
                return base.MoveAfterEnumerator(enumerator, test);
            }
        }

        protected override bool MoveBeforeEnumerator(IEnumerator enumerator, Test test)
        {
            using (new ProfilerMarker(test.Name + ".OneTimeSetUp").Auto())
            {
                return base.MoveBeforeEnumerator(enumerator, test);
            }
        }
        private void SetTestObject(UnityTestExecutionContext context)
        {
            if (!_typeInfo.IsStaticClass)
            {
                context.TestObject = _suite.Fixture ?? _typeInfo.Construct(_arguments);
                if (_suite.Fixture == null)
                {
                    _suite.Fixture = context.TestObject;
                }

                base.Test.Fixture = _suite.Fixture;
            }
        }

        protected override IEnumerator InvokeBefore(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            SetTestObject(context);
            return (IEnumerator)Reflect.InvokeMethod(action, context.TestObject);
        }

        protected override IEnumerator InvokeAfter(MethodInfo action, Test test, UnityTestExecutionContext context)
        {
            SetTestObject(context);
            return (IEnumerator)Reflect.InvokeMethod(action, context.TestObject);
        }

        protected override BeforeAfterTestCommandState GetState(UnityTestExecutionContext context)
        {
            return context.OneTimeSetUpTearDownState;
        }

        public IEnumerable ExecuteOneTimeSetUpEnumerable(ITestExecutionContext context)
        {
            var state = GetState((UnityTestExecutionContext)context);
            if (state.NextBeforeStepIndex >= BeforeActions.Length && BeforeActions.Length > 0)
            {
                state.NextBeforeStepIndex = 0;
                state.NextBeforeStepPc = 0;
            }

            foreach (var child in ExecuteEnumerable(context, ExecutionFlags.SkipAfterActions | ExecutionFlags.SkipStateReset))
                yield return child;
        }

        public IEnumerable ExecuteOneTimeTeardownEnumerable(ITestExecutionContext context)
        {
            var state = GetState((UnityTestExecutionContext)context);
            if (state.NextAfterStepIndex >= AfterActions.Length && AfterActions.Length > 0)
            {
                state.NextAfterStepIndex = 0;
                state.NextAfterStepPc = 0;
            }

            foreach (var child in ExecuteEnumerable(context, ExecutionFlags.SkipBeforeActions | ExecutionFlags.SkipStateReset))
                yield return child;
        }
    }
}
