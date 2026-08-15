using System;
using NUnit.Framework;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// The `UnityOneTimeTearDown` and <see cref="UnityOneTimeSetUpAttribute"/> attributes are identical to the standard `OneTimeSetUp` and `OneTimeTearDown` attributes, with the exception that they allow for <see cref="IEditModeTestYieldInstruction"/>. The `UnityOneTimeSetUp` and `UnityOneTimeTearDown` attributes expect a return type of [IEnumerator](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerator?view=netframework-4.8).
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public class OneTimeSetUpTearDownExample
    /// {
    ///     [UnityOneTimeSetUp]
    ///     public IEnumerator SetUp()
    ///     {
    ///         yield return new EnterPlayMode();
    ///     }
    ///
    ///     [Test]
    ///     public void MyTest()
    ///     {
    ///         Debug.Log("This runs inside playmode");
    ///     }
    ///
    ///     [UnityOneTimeTearDown]
    ///     public IEnumerator TearDown()
    ///     {
    ///         yield return new ExitPlayMode();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <para>## Base and Derived class example</para>
    /// <code>
    /// <![CDATA[
    /// public class BaseClass
    /// {
    ///    [OneTimeSetUp]
    ///    public void OneTimeSetUp()
    ///    {
    ///       Debug.Log("OneTimeSetUp Base");
    ///    }
    ///
    ///    [SetUp]
    ///    public void SetUp()
    ///    {
    ///       Debug.Log("SetUp Base");
    ///    }
    ///
    ///    [UnityOneTimeSetUp]
    ///    public IEnumerator UnityOneTimeSetUp()
    ///    {
    ///       Debug.Log("UnityOneTimeSetUp Base");
    ///       yield return null;
    ///    }
    ///
    ///    [TearDown]
    ///    public void TearDown()
    ///    {
    ///       Debug.Log("TearDown Base");
    ///    }
    ///
    ///    [UnityOneTimeTearDown]
    ///    public IEnumerator UnityOneTimeTearDown()
    ///    {
    ///       Debug.Log("UnityOneTimeTearDown Base");
    ///       yield return null;
    ///    }
    /// }
    ///
    /// public class DerivedClass : BaseClass
    /// {
    ///    [OneTimeSetUp]
    ///    public new void OneTimeSetUp()
    ///    {
    ///       Debug.Log("OneTimeSetUp");
    ///    }
    ///
    ///    [SetUp]
    ///    public new void SetUp()
    ///    {
    ///       Debug.Log("SetUp");
    ///    }
    ///
    ///    [UnityOneTimeSetUp]
    ///    public new IEnumerator UnityOneTimeSetUp()
    ///    {
    ///       Debug.Log("UnityOneTimeSetUp");
    ///       yield return null;
    ///    }
    ///
    ///    [Test]
    ///    public void UnitTest()
    ///    {
    ///       Debug.Log("Test");
    ///    }
    ///
    ///    [UnityTest]
    ///    public IEnumerator UnityTest()
    ///    {
    ///       Debug.Log("UnityTest before yield");
    ///       yield return null;
    ///       Debug.Log("UnityTest after yield");
    ///    }
    ///
    ///    [TearDown]
    ///    public new void TearDown()
    ///    {
    ///       Debug.Log("TearDown");
    ///    }
    ///
    ///    [UnityOneTimeTearDown]
    ///    public new IEnumerator UnityOneTimeTearDown()
    ///    {
    ///       Debug.Log("UnityOneTimeTearDown");
    ///       yield return null;
    ///    }
    ///
    ///    [OneTimeTearDown]
    ///    public void OneTimeTearDown()
    ///    {
    ///       Debug.Log("OneTimeTearDown");
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// <para>## Domain reload example</para>
    /// <code>
    /// <![CDATA[
    /// public class BaseClass
    /// {
    ///    [OneTimeSetUp]
    ///    public void OneTimeSetUp()
    ///    {
    ///       Debug.Log("OneTimeSetUp Base");
    ///    }
    ///
    ///    [SetUp]
    ///    public void SetUp()
    ///    {
    ///       Debug.Log("SetUp Base");
    ///    }
    ///
    ///    [UnityOneTimeSetUp]
    ///    public IEnumerator UnityOneTimeSetUp()
    ///    {
    ///       Debug.Log("UnityOneTimeSetUp Base");
    ///       yield return null;
    ///    }
    ///
    ///    [TearDown]
    ///    public void TearDown()
    ///    {
    ///       Debug.Log("TearDown Base");
    ///    }
    ///
    ///    [UnityOneTimeTearDown]
    ///    public IEnumerator UnityOneTimeTearDown()
    ///    {
    ///       Debug.Log("UnityOneTimeTearDown Base");
    ///       yield return null;
    ///    }
    /// }
    ///
    /// public class DerivedClass : BaseClass
    /// {
    ///    [OneTimeSetUp]
    ///    public new void OneTimeSetUp()
    ///    {
    ///       Debug.Log("OneTimeSetUp");
    ///    }
    ///
    ///    [SetUp]
    ///    public new void SetUp()
    ///    {
    ///       Debug.Log("SetUp");
    ///    }
    ///
    ///    [UnityOneTimeSetUp]
    ///    public new IEnumerator UnityOneTimeSetUp()
    ///    {
    ///       Debug.Log("UnityOneTimeSetUp");
    ///       yield return null;
    ///    }
    ///
    ///    [Test]
    ///    public void UnitTest()
    ///    {
    ///       Debug.Log("Test");
    ///    }
    ///
    ///    [UnityTest]
    ///    public IEnumerator UnityTest()
    ///    {
    ///       Debug.Log("UnityTest before yield");
    ///       yield return new EnterPlayMode();
    ///       //Domain reload happening
    ///       yield return new ExitPlayMode();
    ///       Debug.Log("UnityTest after yield");
    ///    }
    ///
    ///    [TearDown]
    ///    public new void TearDown()
    ///    {
    ///       Debug.Log("TearDown");
    ///    }
    ///
    ///    [UnityOneTimeTearDown]
    ///    public new IEnumerator UnityOneTimeTearDown()
    ///    {
    ///       Debug.Log("UnityOneTimeTearDown");
    ///       yield return null;
    ///    }
    ///
    ///    [OneTimeTearDown]
    ///    public void OneTimeTearDown()
    ///    {
    ///       Debug.Log("OneTimeTearDown");
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public class UnityOneTimeTearDownAttribute : NUnitAttribute
    {
    }
}
