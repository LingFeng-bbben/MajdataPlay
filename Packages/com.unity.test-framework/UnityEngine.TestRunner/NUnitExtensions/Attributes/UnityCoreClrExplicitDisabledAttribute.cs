#pragma warning disable CS0809 // Obsolete member 'member' overrides non-obsolete member 'member'

using System;
using System.ComponentModel;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace UnityEngine.TestTools
{
    /// <summary>
    /// Use this attribute to disable tests on CoreCLR unless they are explicitly named to run.
    ///
    /// You can use this attribute on the test method, test class, or test assembly level.
    ///
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    /// using UnityEngine.TestTools;
    /// using NUnit.Framework;
    ///
    /// [TestFixture]
    /// public class TestClass
    /// {
    ///     [Test]
    ///     [UnityCoreClrExplicitDisabled]
    ///     public void TestMethod()
    ///     {
    ///         Assert.AreEqual(Application.platform, RuntimePlatform.WindowsPlayer);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>

    [Obsolete("Internal use only. This attribute is only intended for use during the implementation of CoreCLR support in Unity.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    public class UnityCoreClrExplicitDisabledAttribute : NUnitAttribute, IApplyToTest
    {
        private readonly string JiraTicket = "";
        private readonly string Reason = "Disabled for CoreCLR. Will run when explicitly named.";
        private readonly string Category = "DisabledCoreClrOnly";

        public UnityCoreClrExplicitDisabledAttribute(string jiraTicket)
        {
            if (string.IsNullOrEmpty(jiraTicket) || !jiraTicket.Contains("jira", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("No jira issue referenced. Must reference a jira issue.", nameof(jiraTicket));
            JiraTicket = jiraTicket;
            Reason = $"{Reason}\nSee {JiraTicket} for more details.";
        }

        public UnityCoreClrExplicitDisabledAttribute(string jiraTicket, string reason) : this(jiraTicket)
        {
            Reason = $"{reason}\nSee {JiraTicket} for more details.";
        }

        public UnityCoreClrExplicitDisabledAttribute(string jiraTicket, string reason, string category) : this(jiraTicket, reason)
        {
            Category = category;
        }

        public void ApplyToTest(Test test)
        {
            if (DisableTest())
            {
                test.RunState = RunState.Explicit;
                test.Properties.Set(PropertyNames.SkipReason, Reason);

                test.Properties.Add(PropertyNames.Category, Category);
            }
        }

        public override string ToString()
        {
            if (DisableTest())
                return $"CoreClrExplicitDisabled: \n{Reason}";
            else
                return "";
        }

        private bool DisableTest()
        {
#if ENABLE_CORECLR
            return true;
#else
            return false;
#endif
        }
    }
}
