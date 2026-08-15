using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;
using System;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// This attribute is an alternative to the standard `Ignore` attribute in [NUnit](http://www.nunit.org/). It allows for ignoring tests based on arguments which were passed to the test method.
    /// </summary>
    /// <example>
    /// <para>The following example shows a method to use the `ParametrizedIgnore` attribute to ignore only one test with specific combination of arguments, where someString is `b` and someInt is `10`.</para>
    /// <code>using NUnit.Framework;
    /// using System.Collections.Generic;
    /// using UnityEngine.TestTools;
    ///
    /// public class MyTestsClass
    /// {
    ///     public static IEnumerable&lt;TestCaseData&gt; MyTestCaseSource()
    ///     {
    ///         for (int i = 0; i &lt; 5; i++)
    ///         {
    ///             yield return new TestCaseData("a", i);
    ///             yield return new TestCaseData("b", i);
    ///         }
    ///     }
    ///
    ///     [Test, TestCaseSource("MyTestCaseSource")]
    ///     [ParametrizedIgnore("b", 3)]
    ///     public void Test(string someString, int someInt)
    ///     {
    ///         Assert.Pass();
    ///     }
    /// }</code>
    /// <para>It could also be used together with `Values` attribute in [NUnit](http://www.nunit.org/).</para>
    /// <code>using NUnit.Framework;
    /// using UnityEngine.TestTools;
    /// 
    /// public class MyTestsClass
    /// {
    ///     [Test]
    ///     [ParametrizedIgnore("b", 10)]
    ///     public void Test(
    ///         [Values("a", "b")] string someString,
    ///         [Values(5, 10)] int someInt)
    ///     {
    ///         Assert.Pass();
    ///     }
    /// }</code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ParametrizedIgnoreAttribute : NUnitAttribute, IWrapTestMethod
    {
        /// <summary>
        /// Argument combination for the test case to ignore.
        /// </summary>
        public object[] Arguments { get; }
        /// <summary>
        /// Reason for the ignore.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametrizedIgnoreAttribute"/> class with a argument combination that is ignored.
        /// </summary>
        /// <param name="arguments">The argument combination to ignore</param>
        public ParametrizedIgnoreAttribute(params object[] arguments)
        {
            this.Arguments = arguments;
        }

        /// <summary>
        /// Wraps a test command with the command to handled parametrized ignore.
        /// </summary>
        /// <param name="command">The command to wrap.</param>
        /// <returns>A command handling parametrized ignore, with respect to the inner command.</returns>
        public TestCommand Wrap(TestCommand command)
        {
            return new ParametrizedIgnoreCommand(command, Arguments, Reason);
        }
    }
}
