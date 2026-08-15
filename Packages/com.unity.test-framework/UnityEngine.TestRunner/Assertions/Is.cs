using System;

namespace UnityEngine.TestTools.Constraints
{
    /// <summary>
    /// NUnit allows you to write test assertions in a more descriptive and human readable way using the Assert.That mechanism, where the first parameter is an object under test and the second parameter describes conditions that the object has to meet.
    ///
    /// We have extended NUnit API with a custom constraint type and declared an overlay Is class. To resolve ambiguity between the original implementation and the custom one you must explicitly declare it with a using statement or via addressing through the full type name `UnityEngine.TestTools.Constraints.Is`.
    /// </summary>
    public class Is : NUnit.Framework.Is
    {
        /// <summary>
        /// A constraint type that invokes the delegate you provide as the parameter of Assert.That and checks whether it causes any GC memory allocations.
        /// </summary>
        /// <returns>An AllocationGCMemoryConstrain.</returns>
        /// <example>
        /// <code>
        /// using Is = UnityEngine.TestTools.Constraints.Is;
        ///
        /// class MyTestClass
        /// {
        ///     [Test]
        ///     public void MyTest()
        ///     {
        ///         Assert.That(() =&gt; {
        ///         var i = new int[500];
        ///         }, Is.AllocatingGCMemory());
        ///     }
        /// }
        /// </code>
        /// </example>
        public static AllocatingGCMemoryConstraint AllocatingGCMemory()
        {
            return new AllocatingGCMemoryConstraint();
        }
    }
}
