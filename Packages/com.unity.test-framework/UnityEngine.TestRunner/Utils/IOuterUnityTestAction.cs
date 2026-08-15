using System;
using System.Collections;
using NUnit.Framework.Interfaces;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// An attribute can implement this interface to provide actions to execute before setup and after teardown of tests.
    /// </summary>
    /// <example>
    /// <para>## IOuterUnityTestAction Example</para>
    /// <code>
    /// // 
    /// <![CDATA[
    /// using System.Collections;
    /// using NUnit.Framework;
    /// using NUnit.Framework.Interfaces;
    /// using UnityEngine;
    /// using UnityEngine.TestTools;
    ///
    /// public class MyTestClass
    /// {
    ///    [UnityTest, MyOuterActionAttribute]
    ///    public IEnumerator MyTestInsidePlaymode()
    ///    {
    ///        Assert.IsTrue(Application.isPlaying);
    ///        yield return null;
    ///    }
    /// }
    ///
    /// public class MyOuterActionAttribute : NUnitAttribute, IOuterUnityTestAction
    /// {
    ///    public IEnumerator BeforeTest(ITest test)
    ///    {
    ///        yield return new EnterPlayMode();
    ///    }
    ///
    ///    public IEnumerator AfterTest(ITest test)
    ///    {
    ///        yield return new ExitPlayMode();
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// <para>Test actions with domain reload example</para>
    /// <code>
    /// <![CDATA[
    /// using NUnit.Framework.Interfaces;
    ///
    ///
    /// public class TestActionOnSuiteAttribute : NUnitAttribute, ITestAction
    /// {
    ///    public void BeforeTest(ITest test)
    ///    {
    ///        Debug.Log("TestAction OnSuite BeforeTest");
    ///    }
    ///
    ///    public void AfterTest(ITest test)
    ///    {
    ///    }
    ///
    ///    public ActionTargets Targets { get { return ActionTargets.Suite; } }
    /// }
    ///
    /// public class TestActionOnTestAttribute : NUnitAttribute, ITestAction
    /// {
    ///    public void BeforeTest(ITest test)
    ///    {
    ///        Debug.Log("TestAction OnTest BeforeTest");
    ///    }
    ///
    ///    public void AfterTest(ITest test)
    ///    {
    ///        Debug.Log("TestAction OnTest AfterTest");
    ///    }
    ///
    ///    public ActionTargets Targets { get { return ActionTargets.Test; } }
    /// }
    ///
    /// public class OuterTestAttribute : NUnitAttribute, IOuterUnityTestAction
    /// {
    ///    public IEnumerator BeforeTest(ITest test)
    ///    {
    ///        Debug.Log("OuterTestAttribute BeforeTest");
    ///        yield return null;
    ///    }
    ///
    ///    public IEnumerator AfterTest(ITest test)
    ///    {
    ///        Debug.Log("OuterTestAttribute AfterTest");
    ///        yield return null;
    ///    }
    /// }
    ///
    /// [TestActionOnSuite]
    /// public class ActionOrderTestBase
    /// {
    ///    [Test, OuterTest, TestActionOnTest]
    ///    public void UnitTest()
    ///    {
    ///        Debug.Log("Test");
    ///    }
    ///
    ///    [UnityTest, OuterTest, TestActionOnTest]
    ///    public IEnumerator UnityTestWithDomainReload()
    ///    {
    ///        Log("Test part 1");
    ///        yield return new EnterPlayMode();
    ///        //Domain reload
    ///        yield return new ExitPlayMode();
    ///        Log("Test part 2");
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public interface IOuterUnityTestAction
    {
        /// <summary>Executed before each test is run</summary>
        /// <param name="test">The test that is going to be run.</param>
        /// <returns>Enumerable collection of actions to perform before test setup.</returns>
        IEnumerator BeforeTest(ITest test);

        /// <summary>Executed after each test is run</summary>
        /// <param name="test">The test that has just been run.</param>
        /// <returns>Enumerable collection of actions to perform after test teardown.</returns>
        IEnumerator AfterTest(ITest test);
    }
}
